using LinqToDB.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using toreligo.Domain.Authentication;
using toreligo.Domain.Authentication.Entities;
using toreligo.Domain.Authentication.Models;
using toreligo.Domain.Database;

namespace toreligo.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly EntityStorage entityStorage;
    private readonly AuthService authService;

    public AuthController(EntityStorage entityStorage, AuthService authService)
    {
        this.entityStorage = entityStorage;
        this.authService = authService;
    }

    [HttpPost("")]
    public IActionResult Auth(AuthRequest request)
    {
        var login = request.Login;
        var pass = request.Pass;
        if (login.Length > UserData.NameLength || pass.Length > UserData.NameLength)
            return BadRequest("Неверный логин или пароль");

        var userData = entityStorage.Select<UserData>()
            .FirstOrDefault(x => x.Login == request.Login);

        if (userData == default)
            return BadRequest("Неверный логин или пароль");

        if (!authService.VerifyPasswordHash(request.Pass, userData.PasswordHash, userData.PasswordSalt))
            return BadRequest("Неверный логин или пароль");

        var session = authService.CreateSession(userData);
        SetRefreshToken(session.RefreshToken, session.ExpiredAt);
        return Ok(session.JwtToken);
    }

    [HttpPost("reg")]
    public IActionResult Reg(RegRequest request)
    {
        var login = request.Login;
        var pass = request.Pass;

        if (login.IsNullOrEmpty() || pass.IsNullOrEmpty())
            return BadRequest("Логин или пароль пустые");

        if (login.Length > UserData.NameLength || pass.Length > UserData.NameLength)
            return BadRequest("Слишком длинный логин или пароль. Максимальная длина - 32 символа");

        if (entityStorage.Select<UserData>().Any(x => x.Login == login))
            return BadRequest("Логин недоступен");

        if (pass.Length < 6)
            return BadRequest("Пароль меньше 6 символов");

        authService.CreatePasswordHash(request.Pass, out var passwordHash, out var passwordSalt);
        var userData = entityStorage.CreateEntity(new UserData
        {
            Login = login,
            Name = login,
            PasswordHash = passwordHash,
            PasswordSalt = passwordSalt,
            BirthdayDate = request.Birthday.Date
        });

        var session = authService.CreateSession(userData);
        SetRefreshToken(session.RefreshToken, session.ExpiredAt);
        return Ok(session.JwtToken);
    }

    [HttpPost("getUser"), Authorize]
    public UserModel GetUser()
    {
        var userId = WebHelper.GetUserId(User);
        var user = entityStorage.GetById<UserData>(userId);
        return new UserModel
        {
            Name = user.Name,
            BirthdayYear = user.BirthdayDate.Date.Year
        };
    }

    [HttpPost("logout"), Authorize]
    public IActionResult Logout()
    {
        var refreshToken = Request.Cookies["refreshToken"];
        if (refreshToken.IsNullOrEmpty())
            return BadRequest();

        entityStorage.Update<AuthSession>(x => x.RefreshToken == refreshToken, _ => new AuthSession
        {
            IsRevoked = true
        });
        SetRefreshToken("", DateTime.UtcNow.AddDays(-1));
        return Ok();
    }

    [HttpGet("refresh")]
    public IActionResult RefreshToken(string accessToken)
    {
        var refreshToken = Request.Cookies["refreshToken"];
        if (refreshToken.IsNullOrEmpty())
            return BadRequest("Invalid Refresh Token.");

        if (!authService.TryValidateTokens(accessToken, refreshToken, out var currentSession))
            return BadRequest("Invalid token");

        currentSession!.IsUsed = true;
        entityStorage.Save(currentSession);
        var userData = entityStorage.GetById<UserData>(currentSession.UserDataId);
        var newSession = authService.CreateSession(userData);
        SetRefreshToken(newSession.RefreshToken, newSession.ExpiredAt);
        return Ok(newSession.JwtToken);
    }

    private void SetRefreshToken(string token, DateTime expiredAt)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Expires = expiredAt,
            Path = "/api/auth"
        };
        Response.Cookies.Append("refreshToken", token, cookieOptions);
    }
}