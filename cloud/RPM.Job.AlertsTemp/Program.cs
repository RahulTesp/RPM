using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;

class Program
{
    static string CONN_STRING =string.Empty;
    private static Timer _timer = null;
    static async Task Main(string[] args)
    {
        // Set up configuration
        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddEnvironmentVariables() // Allows overriding via Azure App Settings
            .Build();

        // Access a specific config value
        string connStr = config["RPM:ConnectionString"];
        Console.WriteLine($"RPM Connection String: {connStr}");

        // Optional: bind strongly-typed object
        var rpmSettings = config.GetSection("RPM").Get<RpmSettings>();
        Console.WriteLine($"RPM.ConnectionString (typed): {rpmSettings?.ConnectionString}");
        CONN_STRING = rpmSettings?.ConnectionString;
        Console.WriteLine("WebJob started...");
        if(CONN_STRING == null)
        {
            Console.WriteLine("Connection string is null.");
            return;
        }
        while (true)
        {
            try
            {
                _timer = new Timer(TimerCallback, null, 0, 30000);
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine("exception:" + ex);
            }
            
        }
    }
    private static void TimerCallback(Object o)
    {
        try
        {
            Console.WriteLine("Timer Api call back");
            using (SqlConnection connection = new SqlConnection(CONN_STRING))
            {
                connection.Open();
                SqlCommand command1 = new SqlCommand("usp_InsAlertsTemp", connection);
                command1.CommandType = CommandType.StoredProcedure;
                command1.ExecuteNonQuery();
                connection.Close();
            }
        }
        catch (Exception Ex)
        {
            Console.WriteLine(Ex);
        }

    }
}
public class RpmSettings
{
    public string? ConnectionString { get; set; }
}
