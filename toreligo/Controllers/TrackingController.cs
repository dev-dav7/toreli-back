using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using toreligo.Domain.Database;
using toreligo.Domain.Group;
using toreligo.Domain.Tracking;
using toreligo.Domain.Tracking.Entity;
using toreligo.Domain.Tracking.Models;
using toreligo.Domain.Tracking.Services;

namespace toreligo.Controllers;

[ApiController]
[Authorize]
[Route("api/track")]
public class TrackController : ControllerBase
{
    private readonly EntityStorage entityStorage;
    private readonly FillTrackingService fillTrackingService;
    private readonly TrackRepository trackRepository;
    private readonly GroupRepository groupRepository;

    public TrackController(EntityStorage entityStorage,
        FillTrackingService fillTrackingService,
        TrackRepository trackRepository,
        GroupRepository groupRepository)
    {
        this.entityStorage = entityStorage;
        this.fillTrackingService = fillTrackingService;
        this.trackRepository = trackRepository;
        this.groupRepository = groupRepository;
    }

    [HttpPost("getAll")]
    public IActionResult GetTracks()
    {
        var userId = WebHelper.GetUserId(User);
        var tracks = trackRepository
            .UserTracks(userId)
            .ToArray()
            .Select(x=> x.ToTrackSerialisedModel());

        return Ok(tracks);
    }

    [HttpPost("create")]
    public IActionResult CreateTrack(TrackSerializedModel request)
    {
        var userId = WebHelper.GetUserId(User);
        if (request.GroupId.HasValue)
        {
            if (!groupRepository.FindUserForGroup(userId, request.GroupId.Value).Any())
                return BadRequest("Указана некорректная группа");
        }

        var createdTrack = entityStorage.CreateEntity(new Track
        {
            OwnerId = userId,
            GroupId = request.GroupId,
            Params = request.Params,
            Options = request.Options
        });

        return Ok(createdTrack.ToTrackSerialisedModel());
    }

    [HttpPost("getValues")]
    public IActionResult GetValues()
    {
        var userId = WebHelper.GetUserId(User);
        var actualValues = (
                from t in trackRepository.UserTracks(userId)
                join f in entityStorage.Select<TrackFill>()
                    on t.Id equals f.TrackId
                select f)
            .ToArray();

        var values = actualValues.Select(x => new ValuesModel
        {
            Day = x.Day,
            DayE = x.DayE,
            Values = JsonSerializer.Deserialize<List<TrackValue>>(x.Values),
            TrackId = x.TrackId
        }).ToArray();
        return Ok(values);
    }

    [HttpPost("values/{trackId}")]
    public IActionResult GetValuesById(long trackId)
    {
        var userId = WebHelper.GetUserId(User);
        var values = trackRepository.TrackFillQuery(userId, trackId)
            .ToArray()
            .Select(x => new TrackFillModel
            {
                Day = x.Day,
                DayE = x.DayE,
                Values = JsonSerializer.Deserialize<List<TrackValue>>(x.Values)
            })
            .AsEnumerable();

        return Ok(new TrackFillsModel
        {
            TrackId = trackId,
            Values = values
        });
    }

    [HttpPost("values/{trackId}/{day}")]
    public IActionResult FindValues(long trackId, int day)
    {
        var userId = WebHelper.GetUserId(User);
        var values = trackRepository.TrackFillQuery(userId, trackId)
            .Where(x => x.Day <= day && day <= x.DayE)
            .ToArray();

        if (values.Length == 1)
            return Ok(new TrackFillModel
            {
                Day = values[0].Day,
                DayE = values[0].DayE,
                Values = JsonSerializer.Deserialize<List<TrackValue>>(values[0].Values)
            });

        return Ok(new TrackFillModel());
    }

    [HttpPost("setValues")]
    public IActionResult SetValues(FillTrackRequest fillTrackRequest) => FillValues(fillTrackRequest, true);

    [HttpPost("fillValues")]
    public IActionResult FillValues(FillTrackRequest fillTrackRequest) => FillValues(fillTrackRequest, false);

    private IActionResult FillValues(FillTrackRequest fillTrackRequest, bool withoutResponse)
    {
        var userId = WebHelper.GetUserId(User);
        if (!trackRepository.FindTrackForUser(userId, fillTrackRequest.TrackId).Any())
            return BadRequest("Трек не найден");

        var fromForSearch = fillTrackRequest.Day - 1;
        var toForSearch = fillTrackRequest.DayE + 1;
        var perspectiveIntersectedFills = entityStorage.Select<TrackFill>()
            .Where(x => x.TrackId == fillTrackRequest.TrackId)
            .Where(x => x.Day <= toForSearch && x.DayE >= toForSearch || //конец отрезка А внутри отрезка Б 
                        x.Day <= fromForSearch && x.DayE >= fromForSearch || //начало отрезка А внутри отрезка Б
                        x.Day >= fromForSearch && x.DayE <= toForSearch) // отрезок А покрыват отрезок Б
            .ToList();

        var valueForInsert = "";
        var filtredValue = fillTrackRequest.Values.Where(x => x.Values.Count() != 0).ToArray();
        if (filtredValue.Any())
            valueForInsert = JsonSerializer.Serialize(fillTrackRequest.Values);

        var newFill = fillTrackingService.InsertValues(fillTrackRequest.TrackId,
            fillTrackRequest.Day, fillTrackRequest.DayE, valueForInsert,
            perspectiveIntersectedFills);

        foreach (var value in newFill)
            entityStorage.Create(value);

        var idsToRemove = perspectiveIntersectedFills.Select(f => f.Id).ToArray();
        entityStorage.Remove<TrackFill>(x => idsToRemove.Contains(x.Id));

        if (withoutResponse)
            return Ok();

        var fillFrom = -1;
        var fillTo = -1;
        if (perspectiveIntersectedFills.Any())
        {
            fillFrom = perspectiveIntersectedFills.Min(x => x.Day);
            fillTo = perspectiveIntersectedFills.Max(x => x.DayE);
        }
        else if (newFill.Any())
        {
            fillFrom = newFill.Min(x => x.Day);
            fillTo = newFill.Max(x => x.DayE);
        }

        return Ok(new SetValueResponse
        {
            RemoveFrom = fillFrom,
            RemoveTo = fillTo,
            Add = newFill.Select(x => new TrackFillModel
            {
                Day = x.Day,
                DayE = x.DayE,
                Values = JsonSerializer.Deserialize<List<TrackValue>>(x.Values)
            })
        });
    }
}