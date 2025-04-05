using Microsoft.Extensions.Configuration;
using RPMWeb.Dal;
using RPMWeb.Data.Common;
using System.Collections.Concurrent;

namespace RPMCriticalAlertJob
{
    public class AlertProcessMgr
    {
        static string CONN_STRING = string.Empty;
        static int MessageSendingIntervel = 5;
        public void CheckandNotifyAlerts()
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
            Task task = new Task(() =>
            {
                ConcurrentDictionary<string, DateTime> LastUpdatedUser = new ConcurrentDictionary<string, DateTime>();
                try
                {
                    while (true)
                    {
                        List<SystemConfigInfo> lstConfig = DalCommon.GetSystemConfig(CONN_STRING, "Notify", "User");
                        if (lstConfig != null && lstConfig.Count > 0)
                        {
                            SystemConfigInfo sci = lstConfig.Find(x => x.Name == "PubSubKey");
                            SystemConfigInfo hn = lstConfig.Find(x => x.Name == "HubName");
                            User inst = new User();
                            List<string> users = inst.GetUserHasCriticalAlers(CONN_STRING);
                            foreach (string user in users)
                            {
                                DateTime lastSentTime = DateTime.MinValue;
                                if (LastUpdatedUser.TryGetValue(user, out lastSentTime))
                                {
                                    TimeSpan ts = DateTime.Now - lastSentTime;
                                    if(ts.TotalMinutes < MessageSendingIntervel)
                                    {
                                        continue;
                                    }
                                }
                                AlertNotification alertNotification = new AlertNotification();
                                if (sci != null && hn != null)
                                {
                                    alertNotification.EventType = NotificationType.PriorityAlert.ToString();
                                    alertNotification.User = user;

                                    var connString = sci.Value;
                                    var hubname = hn.Value;
                                    //var serviceClient = new WebPubSubServiceClient(connString, hubname);
                                    //Azure.Response resp = serviceClient.SendToUser(user, RequestContent.Create(alertNotification), ContentType.ApplicationJson);
                                    //if(resp != null && (HttpStatusCode)resp.Status  == HttpStatusCode.Accepted)
                                    //{
                                    //    if (LastUpdatedUser.ContainsKey(user))
                                    //        LastUpdatedUser[user] = DateTime.Now;
                                    //    else
                                    //        LastUpdatedUser.TryAdd(user, DateTime.Now);
                                    //    Console.WriteLine("Send message to user " + user +" " + DateTime.Now.ToString());
                                    //}
                                    //else
                                    //{
                                    //    Console.WriteLine("Message Send failed " + user +" "+ resp.Status.ToString());
                                    //}
                                   
                                }
                            }
                        }
                        Thread.Sleep(10 * 1000);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }


            });
            task.Start();
        }
    }
}
