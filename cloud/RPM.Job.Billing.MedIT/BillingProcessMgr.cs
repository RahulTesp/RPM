using Microsoft.Extensions.Configuration;
using RPMPatientBilling.PatientBilling;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace RPMPatientBillingJob
{
    public class BillingProcessMgr
    {
        static System.Timers.Timer timer;
        static System.Timers.Timer aTimer;
        ManualResetEvent evBillingResult = new ManualResetEvent(false);
        ManualResetEvent evBillingCount = new ManualResetEvent(false);
        static string CONN_STRING = string.Empty;
        static string Time_Delay = string.Empty;
        public void BillingProcessResults()
        {
            Task task = new Task(() =>
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
                var appSettings = config.GetSection("RPM").Get<AppSettings>();
                Console.WriteLine($"RPM.ConnectionString (typed): {rpmSettings?.ConnectionString}");
                CONN_STRING = rpmSettings?.ConnectionString;
                int Hour = Convert.ToInt32(appSettings.Hour);
                int Minutes = Convert.ToInt32(appSettings.Minutes);
                Time_Delay = appSettings.TimerDelay;
                try
                {
                    DateTime scheduleTime = new DateTime(DateTime.UtcNow.Year,
                                                         DateTime.UtcNow.Month,
                                                         DateTime.UtcNow.Day,
                                                         Hour,
                                                         Minutes,
                                                         0);
                    if(DateTime.UtcNow>scheduleTime)
                    {
                        scheduleTime = scheduleTime.AddDays(1);
                    }
                    TimeSpan currentTimeDiff = scheduleTime - DateTime.UtcNow;
                    int timeToWait = (int)Math.Abs(currentTimeDiff.TotalMilliseconds);
                    while (true)
                    {                        
                        Console.WriteLine("Timeout in " + timeToWait.ToString());
                        evBillingResult.WaitOne(timeToWait);
                        {
                            string cs = CONN_STRING;
                            RPMBilling rpm = new RPMBilling();
                            rpm.UpdatePatientBilling(cs);
                        }
                        scheduleTime = scheduleTime.AddDays(1);
                        currentTimeDiff = scheduleTime - DateTime.UtcNow;
                        timeToWait = (int)Math.Abs(currentTimeDiff.TotalMilliseconds);

                    }

                    /*int Hour = Convert.ToInt32(ConfigurationManager.AppSettings["Hour"]);
                    int minutes = Convert.ToInt32(ConfigurationManager.AppSettings["Minutes"]);
                    DateTime nowTime = DateTime.Now;
                    DateTime scheduledTime = new DateTime(nowTime.Year, nowTime.Month, nowTime.Day, Hour, minutes, 0, 0); //Specify your scheduled time HH,MM,SS [8am and 42 minutes]
                    if (nowTime > scheduledTime)
                    {
                        scheduledTime = scheduledTime.AddDays(1);
                    }
                    double tickTime = (double)(scheduledTime - DateTime.Now).TotalMilliseconds;
                    timer = new System.Timers.Timer(tickTime);
                    timer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
                    timer.Start();*/
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            });
            task.Start();
            
        }

        public void BillingProcess(string cs)
        {
            try
            {
                /*string cs = ConfigurationManager.ConnectionStrings["RPM"].ConnectionString;
                RPMPatientBilling.PatientBilling.RPMBilling rpm = new RPMPatientBilling.PatientBilling.RPMBilling();
                rpm.GenaratePateintBillingCount(cs);
                return;*/
                double timerDelay = 1;
                string configValue = Time_Delay;
                if(!string .IsNullOrEmpty(configValue))
                {
                    timerDelay = Convert.ToSingle(configValue);
                }
                double nDelay = timerDelay * 60 * 60*1000;
                /*Task task = new Task(() =>
                {
                    while (true)
                    {
                        Console.WriteLine("BillingProcess Job started! - " + DateTime.UtcNow + "");
                        string cs = ConfigurationManager.ConnectionStrings["RPM"].ConnectionString;
                        RPMPatientBilling.PatientBilling.RPMBilling rpm = new RPMPatientBilling.PatientBilling.RPMBilling();
                        rpm.GenaratePateintBillingCount(cs);
                        rpm.UpdatePatientBilling(cs);
                        Console.WriteLine(@"BillingProcess Job Completed! - " + DateTime.UtcNow +
                                           @"Waiting period "+ (int)nDelay);
                        evBillingCount.WaitOne((int)nDelay);
                    }

                });
                task.Start();*/
                {
                    Console.WriteLine(@"BillingProcess Initializing  - Running first time" + DateTime.UtcNow +
                                       @"Waiting period " + (int)nDelay);
                    RPMPatientBilling.PatientBilling.RPMBilling rpm = new RPMPatientBilling.PatientBilling.RPMBilling();
                    rpm.GenaratePateintBillingCount(cs);
                    rpm.UpdatePatientBilling(cs);
                }
                Console.WriteLine(@"Starting timer.");
                aTimer = new System.Timers.Timer(nDelay); //one hour in milliseconds
                aTimer.Elapsed += new ElapsedEventHandler(OnTimedJobEvent);
                aTimer.Start();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
           
        }
        private static void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            Console.WriteLine("BillingProcessResults Job started! - " + DateTime.UtcNow + "");
            timer.Stop();
            string cs = CONN_STRING;
            //RPMPatientBilling.PatientBilling.RPMBilling rpm = new RPMPatientBilling.PatientBilling.RPMBilling();
           // rpm.GetPatientBillingReport(cs);
            BillingProcessMgr bpm = new BillingProcessMgr();
            bpm.BillingProcessResults();
            Console.WriteLine("BillingProcessResults Job Completed! - " + DateTime.UtcNow + "");
        }
        private static void OnTimedJobEvent(object source, ElapsedEventArgs e)
        {
            Console.WriteLine("BillingProcess Job started! - " + DateTime.UtcNow + "");
            string cs = CONN_STRING;
            RPMPatientBilling.PatientBilling.RPMBilling rpm = new RPMPatientBilling.PatientBilling.RPMBilling();
            rpm.GenaratePateintBillingCount(cs);
            rpm.UpdatePatientBilling(cs);
            Console.WriteLine("BillingProcess Job Completed! - " + DateTime.UtcNow + "");
        }
    }
}
