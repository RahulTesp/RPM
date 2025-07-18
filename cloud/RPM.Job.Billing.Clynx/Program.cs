using Microsoft.Extensions.Configuration;
using RPMPatientBillingJob;


class Program
{
    static string CONN_STRING =string.Empty;
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
                BillingProcessMgr inst = new BillingProcessMgr();
                inst.BillingProcess(CONN_STRING);
            }
            catch (Exception ex)
            {
                Console.WriteLine("exception:" + ex);
            }
            
        }
    }
}
public class RpmSettings
{
    public string? ConnectionString { get; set; }
}
public class AppSettings
{
    public string? Hour { get; set; }
    public string? Minutes { get; set; }
    public string? TimerDelay { get; set; }
}
