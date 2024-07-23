using LinqToDB.Configuration;
using LinqToDB.Data;


namespace toreligo.Domain.Database;

public class AppDataConnection : DataConnection
{
    public AppDataConnection(LinqToDbConnectionOptions<AppDataConnection> options) : base(options)
    {

    }
}