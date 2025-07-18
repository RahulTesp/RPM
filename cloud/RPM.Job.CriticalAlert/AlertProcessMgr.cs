using RPMWeb.Dal;
using RPMWeb.Data.Common;
using System.Collections.Concurrent;

namespace RPMCriticalAlertJob
{
    public class AlertProcessMgr
    {
        static string CONN_STRING = string.Empty;
        static int MessageSendingIntervel = 5;
        public void CheckandNotifyAlerts(string Connection)
        {
            CONN_STRING = Connection;
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
                            SystemConfigInfo? sci = lstConfig.Find(x => x.Name == "PubSubKey");
                            SystemConfigInfo? hn = lstConfig.Find(x => x.Name == "HubName");
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
