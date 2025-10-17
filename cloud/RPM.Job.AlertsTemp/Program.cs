using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;
using System.Data.Common;

class Program
{
    static string CONN_STRING = string.Empty;
    static async Task Main(string[] args)
    {
        // Load configuration from appsettings.json and environment variables
        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        string? connStr = config["RPM:ConnectionString"];
        if (string.IsNullOrEmpty(connStr))
        {
            Console.WriteLine("Connection string is missing in appsettings.json.");
            return;
        }

        // Parse connection string for server and database info
        var builder = new DbConnectionStringBuilder { ConnectionString = connStr };
        string server = builder.ContainsKey("Server") ? builder["Server"].ToString() : "";
        string database = builder.ContainsKey("Initial Catalog") ? builder["Initial Catalog"].ToString() : "";

        Console.WriteLine($"Server: {server}");
        Console.WriteLine($"Database: {database}");

        CONN_STRING = connStr;
        Console.WriteLine("WebJob started...");

        // Continuous loop: execute job every 5 seconds
        while (true)
        {
            try
            {
                await TimerCallback();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{DateTime.Now}] Exception: {ex}");
            }
            await Task.Delay(TimeSpan.FromSeconds(20)); // Non-blocking delay
        }
    }

    private static async Task TimerCallback()
    {
        Console.WriteLine($"[{DateTime.Now}] Timer triggered.");

        using SqlConnection connection = new SqlConnection(CONN_STRING);
        await connection.OpenAsync();
        await ExecuteStoredProcedure(connection, "usp_InsAlertsTemp");
    }

    private static async Task ExecuteStoredProcedure(SqlConnection connection, string procedureName, int timeoutSeconds = 300)
    {
        using SqlCommand command = new SqlCommand(procedureName, connection)
        {
            CommandType = CommandType.StoredProcedure,
            CommandTimeout = timeoutSeconds
        };

        await command.ExecuteNonQueryAsync();
        Console.WriteLine($"Executed {procedureName} successfully.");
    }
}