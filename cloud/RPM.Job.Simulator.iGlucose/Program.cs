using Microsoft.Extensions.Configuration;
using RPM.Job.Simulator.iGlucose;

class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var config = new ConfigurationBuilder()
                    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                    .AddEnvironmentVariables()
                    .Build();

                // connection string
                string connectionString = config.GetConnectionString("DefaultConnection")
                                          ?? Environment.GetEnvironmentVariable("SqlConnectionString");

                if (string.IsNullOrEmpty(connectionString))
                {
                    Console.WriteLine("Connection string not found. Configure it in appsettings.json file or Azure App Settings.");
                    return;
                }

                Console.WriteLine("Started Device Readings WebJob");

                var readingService = new ReadingService(connectionString);
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
