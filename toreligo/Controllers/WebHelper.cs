using System.Security.Claims;

namespace toreligo.Controllers;

public static class WebHelper
{
    public static long GetUserId(ClaimsPrincipal user) => Convert.ToInt32(user.Claims.First(i => i.Type == "Id").Value);
}