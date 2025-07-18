using Microsoft.Extensions.Configuration;
using RPMCriticalAlertJob;
using RPMWeb.Data.Common;
using RPMWeb.Dal;

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
                List<SystemConfigInfo> lstConfig = DalCommon.GetSystemConfig(CONN_STRING, "Storage", "User");
                if (lstConfig.Count > 0)
                {
                    SystemConfigInfo? sci = lstConfig.Find(x => x.Name == "ConnString");
                    if (sci != null)
                    {
                        AlertProcessMgr instAlert = new AlertProcessMgr();
                        instAlert.CheckandNotifyAlerts(CONN_STRING);
                        UserNotificationMgr instNotify = new UserNotificationMgr();
                        instNotify.VerifyAndSendUserNotifications(CONN_STRING);

                    }
                    else
                    {
                        Console.WriteLine("System Confiugration not found..");
                    }
                }
                else
                {
                    Console.WriteLine("System Confiugration not found..");
                }
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
