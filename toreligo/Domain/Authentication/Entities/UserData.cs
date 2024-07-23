using LinqToDB;
using LinqToDB.Mapping;
using toreligo.Domain.Database;

#pragma warning disable CS8618

namespace toreligo.Domain.Authentication.Entities;

[Table("user_data")]
public class UserData : EntityWithId
{
    [Column("login", Length = LoginLength)]
    public string Login { get; set; }

    [Column("name", Length = NameLength)] public string Name { get; set; }

    [Column("hash", Length = PassDataLength)]
    public byte[] PasswordHash { get; set; }

    [Column("salt", Length = PassDataLength)]
    public byte[] PasswordSalt { get; set; }

    [Column("birthday_date", DataType = DataType.DateTime)]
    public DateTime BirthdayDate { get; set; }

    public const int LoginLength = 32;
    public const int NameLength = 64;
    public const int PassDataLength = 128;
}