using System.Data.SqlClient;
using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.Configuration.AdvanceExtensibility;

public static class SqlHelper
{
    public static async Task EnsureDatabaseExists(this EndpointConfiguration endpointConfiguration)
    {
        var catalog = endpointConfiguration.GetSettings().EndpointName();

        await EnsureDatabaseExists(catalog).ConfigureAwait(false);
    }

    public static async Task EnsureDatabaseExists(string catalog)
    {
        using (var connection = new SqlConnection(DefaultConnectionString))
        {
            await connection.OpenAsync().ConfigureAwait(false);

            using (var command = connection.CreateCommand())
            {
                command.CommandText = $@"
if(db_id('{BuildCatalogName(catalog, false)}') is null)
    create database {BuildCatalogName(catalog)};
";
                await command.ExecuteNonQueryAsync().ConfigureAwait(false);
            }
        }
    }

    public static string BuildAddressWithCatalog(string address, string catalog)
    {
        return $"{address}@dbo@{BuildCatalogName(catalog)}";
    }

    public static string BuildCatalogName(string catalog, bool escapeName = true)
    {
        string BuildEscape(string s) => escapeName ? s : string.Empty;

        return $"{BuildEscape("[")}{DatabasePrefix}{catalog}{BuildEscape("]")}";
    }

    public static string BuildCatalogConnectionString(string catalog)
    {
        var builder = new SqlConnectionStringBuilder(DefaultConnectionString)
        {
            InitialCatalog = BuildCatalogName(catalog, false)
        };

        return builder.ConnectionString;
    }

    private const string DatabasePrefix = "Case00031061_";
    const string DefaultConnectionString = @"Data Source=.\SQLEXPRESS;Initial Catalog=master;Integrated Security=True;";
}