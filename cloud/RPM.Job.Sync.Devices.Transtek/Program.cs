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
            // Use the static class name to call the static method  
            Functions.SyncDevices();
 
        }
        catch (Exception ex)
        {
            Console.WriteLine("exception:" + ex);
        }

    }
}
