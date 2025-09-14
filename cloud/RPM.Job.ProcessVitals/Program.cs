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
        await TimerCallback();
        Thread.Sleep(20000);

    }

    private static async Task TimerCallback()
    {
        try
        {
            Console.WriteLine("Timer Api call back");
            using (SqlConnection connection = new SqlConnection(CONN_STRING))
            {
                connection.Open();
                /*SqlCommand command = new SqlCommand("usp_InsPatientVitalMeasures", connection);
                command.CommandType = CommandType.StoredProcedure;

                command.ExecuteNonQuery();*/
                /*SqlCommand command4 = new SqlCommand("usp_InsAlerts", connection);
                command4.CommandType = CommandType.StoredProcedure;
                command4.ExecuteNonQuery();
                SqlCommand command1 = new SqlCommand("usp_InsAlertsTemp", connection);
                command1.CommandType = CommandType.StoredProcedure;
                command1.ExecuteNonQuery();*/
                SqlCommand command2 = new SqlCommand("usp_InsPatientProgramPriority", connection);
                command2.CommandTimeout = 900000;
                command2.CommandType = CommandType.StoredProcedure;
                command2.ExecuteNonQuery();
                SqlCommand command3 = new SqlCommand("usp_InsAlertSummary", connection);
                command3.CommandTimeout = 900000;
                command3.CommandType = CommandType.StoredProcedure;
                command3.ExecuteNonQuery();
                connection.Close();
            }
        }
        catch (Exception Ex)
        {
            CommandType = CommandType.StoredProcedure,
            CommandTimeout = timeoutSeconds
        };

    }
}
public class RpmSettings
{
    public string? ConnectionString { get; set; }
}
