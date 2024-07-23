using Npgsql;
#pragma warning disable CS8618

namespace toreligo.Domain.Database;

public class PostgreConnectionParameters
{
    public string Host { get; set; }
    public string Database { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
    public int? Port { get; set; }
    public bool UseSsl { get; set; }

    public string Schema { get; set; }

    public bool? Pooling { get; set; }
    public int? MinPoolSize { get; set; }
    public int? MaxPoolSize { get; set; }

    public string FormatConnectionString()
    {
        var connectionStringBuilder = new NpgsqlConnectionStringBuilder
        {
            Host = Host,
            Database = Database,
            Username = Username,
            Password = Password,
            ApplicationName = "toreligo"
        };
        if (Port.HasValue)
            connectionStringBuilder.Port = Port.Value;
        if (UseSsl)
        {
            connectionStringBuilder.SslMode = SslMode.Prefer;
            connectionStringBuilder.TrustServerCertificate = true;
        }

        if (!string.IsNullOrWhiteSpace(Schema))
            connectionStringBuilder.SearchPath = Schema;

        if (Pooling.HasValue)
            connectionStringBuilder.Pooling = Pooling.Value;
        if (MinPoolSize.HasValue)
            connectionStringBuilder.MinPoolSize = MinPoolSize.Value;
        if (MaxPoolSize.HasValue)
            connectionStringBuilder.MaxPoolSize = MaxPoolSize.Value;

        connectionStringBuilder.CommandTimeout = 60;

        return connectionStringBuilder.ConnectionString;
    }
}
