using Microsoft.Extensions.Configuration;
using RPMPatientBillingJob;
class Program
{
    static string CONN_STRING =string.Empty;
    static async Task Main(string[] args)
    {
        try
        {
            // Set up configuration
            var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddEnvironmentVariables() // Allows overriding via Azure App Settings
            .Build();
            if(config == null)
            {
                Console.WriteLine("Configuration is null.");
                return;
            }
            // Access a specific config value
            string? connStr = config["RPM:ConnectionString"];
            Console.WriteLine($"RPM Connection String: {connStr}");
            if(connStr == null) {
                Console.WriteLine("Connection string is null in appsettings.json.");
                return;
            }
            CONN_STRING = connStr;
            Console.WriteLine("WebJob started...");
            if(CONN_STRING == null)
            {
                Console.WriteLine("Connection string is null.");
                return;
            }
       
            BillingProcessMgr inst = new BillingProcessMgr();
            inst.BillingProcess(CONN_STRING);
        }
        catch (Exception ex)
        {
            Console.WriteLine("exception:" + ex);
        }

    }
}
