using LinqToDB.Common;
using toreligo.Domain.Tracking.Entity;

namespace toreligo.Domain.Tracking.Services;

public class FillTrackingService
{
    public List<TrackFill> InsertValues(long trackId, int day, int dayE, string newValue, List<TrackFill> currentFill)
        => CleanFills(BuildDirtyFills(trackId, day, dayE, newValue, currentFill));

    private List<TrackFill> CleanFills(List<TrackFill> dirtyFill)
    {
        //считается, что гарантировано что при сортировке по day, каждый следующий элемент имеет day < dayE предыдущего 
        var cleanedFill = new List<TrackFill>();
        foreach (var fill in dirtyFill.OrderBy(x => x.Day))
        {
            if (cleanedFill.Count == 0)
            {
                cleanedFill.Add(fill);
                continue;
            }

            var previous = cleanedFill.Last();
            if (previous.DayE + 1 == fill.Day && previous.Values == fill.Values)
                previous.DayE = fill.DayE;
            else
                cleanedFill.Add(fill);
        }

        return cleanedFill.ToList();
    }

    private List<TrackFill> BuildDirtyFills(long trackId, int day, int dayE, string newValue,
        List<TrackFill> currentFill)
    {
        //считается, что гарантированно day >= dayE
        var values = new List<TrackFill>();
        if (!newValue.IsNullOrEmpty())
            PushValue(day, dayE, newValue);

        foreach (var fill in currentFill)
        {
            //отрезки не пересекаются
            if (dayE < fill.Day || fill.DayE < day)
            {
                PushValue(fill.Day, fill.DayE, fill.Values);
                continue;
            }

            //старый отрезок внутри нового
            if (day <= fill.Day && fill.DayE <= dayE)
            {
                continue;
            }

            //новый отрезок внутри старого
            if (fill.Day < day && dayE < fill.DayE)
            {
                PushValue(fill.Day, day - 1, fill.Values);
                PushValue(dayE + 1, fill.DayE, fill.Values);
                continue;
            }

            //новый отрезок начинается до или в начале старого и заканчивается внутри старого
            if (day <= fill.Day && fill.Day <= dayE && dayE < fill.DayE)
            {
                PushValue(dayE + 1, fill.DayE, fill.Values);
                continue;
            }

            //новый отрезок начинается внутри старого отрезка и заканчивается после или в конце старого
            if (fill.Day < day && day <= fill.DayE && fill.DayE <= dayE)
            {
                PushValue(fill.Day, day - 1, fill.Values);
                continue;
            }
        }

        return values;

        void PushValue(int dayFrom, int dayTo, string value) =>
            values.Add(new TrackFill
            {
                Day = dayFrom,
                DayE = dayTo,
                Values = value,
                TrackId = trackId
            });
    }
}