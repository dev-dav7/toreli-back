using System.ComponentModel.DataAnnotations;
#pragma warning disable CS8618

namespace toreligo.Domain.Authentication.Models;

public class RegRequest
{
    [Required] public string Login { get; set; }

    [Required] public string Pass { get; set; }

    [Required] public DateTime Birthday { get; set; }
}

public class AuthRequest
{
    [Required] public string Login { get; set; }

    [Required] public string Pass { get; set; }
}

public class UserModel
{
    public string Name { get; set; }
    public int BirthdayYear { get; set; }
}
