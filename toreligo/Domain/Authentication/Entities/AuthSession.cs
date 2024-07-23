using LinqToDB;
using LinqToDB.Mapping;
using toreligo.Domain.Database;

#pragma warning disable CS8618

namespace toreligo.Domain.Authentication.Entities;

[Table("auth_session")]
public class AuthSession : EntityWithId
{
    [Column("id_user_data")]
    public long UserDataId { get; set; }
    
    [Column("refresh_token", Length = 64)]
    public  string RefreshToken { get; set; }
    
    [Column("jwt_token", Length = 512)]
    public  string JwtToken { get; set; }
    
    [Column("id_jwt_token", Length = 512)]
    public  string JwtTokenId { get; set; }
    
    [Column("added_at", DataType = DataType.Timestamp)]
    public DateTime AddedAt { get; set; }

    [Column("expired_at", DataType = DataType.Timestamp)]
    public DateTime ExpiredAt { get; set; }

    [Column("is_used")]
    public bool IsUsed { get; set; }

    [Column("is_revoked")]
    public bool IsRevoked { get; set; }
}