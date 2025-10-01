using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data.Common;
// cron 0 0 7 * * *
namespace TranstekDataCleanupJob
{
    internal class Program
    {
        static string CONN_STRING = string.Empty;
        static void Main(string[] args)
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
            if (connStr == null)
            {
                Console.WriteLine("Connection string is null in appsettings.json.");
                return;
            }
            CONN_STRING = connStr;
            // Parse connection string for server and database info
            var builder = new DbConnectionStringBuilder { ConnectionString = connStr };
            string server = builder.ContainsKey("Server") ? builder["Server"].ToString() : "";
            string database = builder.ContainsKey("Initial Catalog") ? builder["Initial Catalog"].ToString() : "";

            Console.WriteLine($"Server: {server}");
            Console.WriteLine($"Database: {database}");
            Console.WriteLine("Starting house keeping WebJob for Transtek...");

            if (string.IsNullOrWhiteSpace(CONN_STRING))
            {
                Console.WriteLine("Connection string not found in environment variables.");
                return;
            }

            try
            {
                using (var conn = new SqlConnection(CONN_STRING))
                {
                    conn.Open();
                    using (var cmd = new SqlCommand("usp_DelTranstekDataStore", conn))
                    {
                        int rowsAffected = cmd.ExecuteNonQuery();
                        Console.WriteLine($"Deleted {rowsAffected} records older than 2 months.");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }

            Console.WriteLine("WebJob finished.");
        }
    }
}
