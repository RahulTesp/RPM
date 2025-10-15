using Microsoft.Extensions.Configuration;
using RPM.Job.Simulator.iGlucose;
using System.Data.Common;

class Program
{
    static string CONN_STRING = string.Empty;
    static int noofreadings = 0;
    static void Main(string[] args)
    {
        try
        {
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
 
            int.TryParse(config["RPM:ReadingsPerDay"],out noofreadings);
            if (connStr == null)
            {
                Console.WriteLine("Connection string is null in appsettings.json.");
                return;
            }
            if (string.IsNullOrWhiteSpace(CONN_STRING))
            {
                Console.WriteLine("Connection string not found in environment variables.");
                return;
            }
            CONN_STRING = connStr;
            // Parse connection string for server and database info
            var builder = new DbConnectionStringBuilder { ConnectionString = connStr };
            string server = builder.ContainsKey("Server") ? builder["Server"].ToString() : "";
            string database = builder.ContainsKey("Initial Catalog") ? builder["Initial Catalog"].ToString() : "";
            Console.WriteLine($"Server: {server}");
            Console.WriteLine($"Database: {database}");
            Console.WriteLine("Started Device Simulator WebJob");
            var readingService = new ReadingService(CONN_STRING);
            for (int i = 0; i < noofreadings; i++)
            {
                readingService.getandProcessActiveDevices();
            }
               
            Console.WriteLine("job Completed successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Job failed: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
        }
        
    }
}
