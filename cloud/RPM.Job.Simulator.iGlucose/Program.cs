using Microsoft.Extensions.Configuration;
using RPM.Job.Simulator.iGlucose;

class Program
    {
    static string CONN_STRING = string.Empty;
    static void Main(string[] args)
        {
            try
            {
                var config = new ConfigurationBuilder()
                    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                    .AddEnvironmentVariables()
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
            Console.WriteLine("Started Device Readings WebJob");

                var readingService = new ReadingService(CONN_STRING);
                readingService.getandProcessActiveDevices();

                Console.WriteLine("job Completed successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Job failed: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
            }
        }
    }
