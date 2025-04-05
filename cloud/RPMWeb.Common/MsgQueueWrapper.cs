using Azure.Storage.Queues;
using RPMWeb.Dal;
using RPMWeb.Data.Common;
using System.Text;
using System.Text.Json;

namespace RPMWeb.Common
{
    public static class MsgQueueWrapper
    {
        public static bool PushToQueue(AlertNotification notification, string dbconnectionstring)
        {
            try
            {
                List<SystemConfigInfo> lstConfig = DalCommon.GetSystemConfig(dbconnectionstring, "Storage", "User");
                if (lstConfig.Count > 0)
                {
                    SystemConfigInfo sci = lstConfig.Find(x => x.Name == "ConnString");
                    if (sci != null)
                    {
                        string connectionString = sci.Value;
                        // Instantiate a QueueClient which will be used to create and manipulate the queue
                        QueueClient queueClient = new QueueClient(connectionString, "rpmnotifications");

                        // Create the queue if it doesn't already exist
                        queueClient.CreateIfNotExists();

                        if (queueClient.Exists())
                        {
                            string jsonString = JsonSerializer.Serialize(notification);
                            byte[] byData = ASCIIEncoding.ASCII.GetBytes(jsonString);
                            string base64String = Convert.ToBase64String(byData);
                            // Send a message to the queue
                            queueClient.SendMessage(base64String);
                        }
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
            catch //(Exception ex)
            {
                return false;
            }
            return true;
        }
        public static bool SendMessage(SendSms1 smsdata, string dbconnectionstring)
        {
            try
            {
                List<SystemConfigInfo> lstConfig = DalCommon.GetSystemConfig(dbconnectionstring, "Storage", "User");
                if (lstConfig.Count > 0)
                {
                    SystemConfigInfo sci = lstConfig.Find(x => x.Name == "ConnString");
                    if (sci != null)
                    {
                        string connectionString = sci.Value;
                        // Instantiate a QueueClient which will be used to create and manipulate the queue
                        QueueClient queueClient = new QueueClient(connectionString, "smsnotifications");

                        // Create the queue if it doesn't already exist
                        queueClient.CreateIfNotExists();

                        if (queueClient.Exists())
                        {
                            string jsonString = JsonSerializer.Serialize(smsdata);
                            byte[] byData = ASCIIEncoding.ASCII.GetBytes(jsonString);
                            string base64String = Convert.ToBase64String(byData);
                            // Send a message to the queue
                            queueClient.SendMessage(base64String);
                        }
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
            catch //(Exception ex)
            {
                return false;
            }
            return true;
        }

    }
}
