using toreligo.Domain.Database;

namespace toreligo.Configurations;

public static class PostgressConfig
{
    public const string productionSchema = "prod";

    public static PostgreConnectionParameters Local =>
        new()
        {
            Host = "localhost",
            Port = 5433,
            Database = "tr_prod",
            Username = "prod",  
            Password = "<>",
            Schema = productionSchema
        };
}