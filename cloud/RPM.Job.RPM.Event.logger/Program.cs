using System;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace RPM.Job.RPM.Event.logger
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("RPM.Job.RPM.Event.logger WebJob started.");

            var builder = new Microsoft.Extensions.Configuration.ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            var configuration = builder.Build();

            LoggerJob job = new LoggerJob(configuration);
            var logs = job.GetTodayLogs();
            if (logs.Rows.Count > 0)
            {
                string htmlTable = job.BuildHtmlTable(logs);
                string toEmail = "rpmsupport@tesplabs.com";
                string subject = "Today's Event Logs";
                string result = job.SendEmail(htmlTable, toEmail, subject);
                Console.WriteLine(result);
            }
            else
            {
                Console.WriteLine("No logs found for today.");
            }
        }
    }
}
