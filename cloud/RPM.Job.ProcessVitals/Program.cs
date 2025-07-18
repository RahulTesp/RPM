using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;
//cron contiuous
class Program
{
    static string CONN_STRING = string.Empty;

    static async Task Main(string[] args)
    {
        // Set up configuration
        var config = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json", optional: true)
        .AddEnvironmentVariables() // Allows overriding via Azure App Settings
        .Build();
        if (config == null)
        {
            Console.WriteLine("Configuration is null.");
            return;
        }
        // Access a specific config value
        string? connStr = config["RPM:ConnectionString"];
        Console.WriteLine($"RPM Connection String: {connStr}");
        if (connStr == null)
        {
            Console.WriteLine("Connection string is null in appsettings.json.");
            return;
        }
        CONN_STRING = connStr;
        Console.WriteLine(CONN_STRING);
        if (string.IsNullOrEmpty(CONN_STRING))
        {
            Console.WriteLine("Connection string is null or empty.");
            return;
        }

        Console.WriteLine("WebJob started...");

        using var periodicTimer = new PeriodicTimer(TimeSpan.FromSeconds(30));
        while (await periodicTimer.WaitForNextTickAsync())
        {
            await TimerCallback();
        }
    }

    private static async Task TimerCallback()
    {
        try
        {
            Console.WriteLine($"[{DateTime.Now}] Timer triggered.");

            using SqlConnection connection = new SqlConnection(CONN_STRING);
            await connection.OpenAsync();

            // Execute stored procedures
            await ExecuteStoredProcedure(connection, "usp_InsPatientVitalMeasures");
            //await ExecuteStoredProcedure(connection, "usp_InsAlertsTemp");
            //await ExecuteStoredProcedure(connection, "usp_InsPatientProgramPriority", 900);
            //await ExecuteStoredProcedure(connection, "usp_InsAlertSummary", 900);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[{DateTime.Now}] Exception: {ex.Message}");
        }
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