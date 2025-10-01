using Microsoft.Extensions.Configuration;
using SyncTranstekDevices;
using System.Data.Common;
//cron 0 0 7 * * *
class Program
{
    static string CONN_STRING = string.Empty;
    static async Task Main(string[] args)
    {
        try
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

            Console.WriteLine("WebJob started...");
            if (CONN_STRING == null)
            {
                Console.WriteLine("Connection string is null.");
                return;
            }

            // Use the static class name to call the static method  
            Functions.SyncDevices();
 
        }
        catch (Exception ex)
        {
            Console.WriteLine("exception:" + ex);
        }

    }
}
