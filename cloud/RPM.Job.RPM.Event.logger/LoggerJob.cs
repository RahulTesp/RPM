using System;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;

namespace RPM.Job.RPM.Event.logger
{
    public class LoggerJob
    {
        private readonly string _connectionString;
        private readonly IConfiguration _configuration;

        public LoggerJob(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public ContactDetails GetEmailDetails()
        {
            ContactDetails contactDetails = new ContactDetails();
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    SqlCommand command = new SqlCommand("usp_GetEmailDetails", connection);
                    command.CommandType = CommandType.StoredProcedure;
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        contactDetails.FromMail = Convert.ToString(reader["FromEmail"]);
                        contactDetails.Password = Convert.ToString(reader["Password"]);
                    }
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
            return contactDetails;
        }

        public string SendEmail(string body, string toEmail, string subject)
        {
            try
            {
                ContactDetails contactDetails = GetEmailDetails();
                string smtp = _configuration["SMTP"] ?? "smtp.office365.com";
                SmtpClient smtpClient = new SmtpClient(smtp, 587);
                smtpClient.EnableSsl = true;
                smtpClient.Credentials = new NetworkCredential(contactDetails.FromMail, contactDetails.Password);
                MailMessage mailMessage = new MailMessage(contactDetails.FromMail, toEmail, subject, body);
                smtpClient.Send(mailMessage);
                return "Email Sent";
            }
            catch (Exception ex)
            {
                return "Failed to Send Email - " + ex;
            }
        }

        public DataTable GetTodayLogs()
        {
            DataTable dt = new DataTable();
            try
            {
                using (var connection = new Microsoft.Data.SqlClient.SqlConnection(_connectionString))
                {
                    string query = @"SELECT LogDescription, ObjectName, LogUser, LogCreatedOn FROM dbo.EventLog WHERE LogCreatedOn >= DATEADD(day, -3, GETDATE()) ORDER BY LogCreatedOn DESC";
                    using (var adapter = new Microsoft.Data.SqlClient.SqlDataAdapter(query, connection))
                    {
                        adapter.Fill(dt);
                    }
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
            return dt;
        }

        public string BuildHtmlTable(DataTable dt)
        {
            StringBuilder sb = new StringBuilder();
            // Define custom column widths
            foreach (DataRow row in dt.Rows)
            {
                sb.AppendLine($"[LogUser] : {row["LogUser"]}");
                // Indent LogDescription for wrapped lines
                string logDesc = Convert.ToString(row["LogDescription"]);
                string indent = new string(' ', 18);
                var logDescLines = logDesc.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                if (logDesc.Length > 80)
                {
                    for (int i = 0; i < logDesc.Length; i += 80)
                    {
                        string part = logDesc.Substring(i, Math.Min(80, logDesc.Length - i));
                        if (i == 0)
                            sb.AppendLine($"[LogDescription] : {part}");
                        else
                            sb.AppendLine($"{indent}{part}");
                    }
                }
                else
                {
                    sb.AppendLine($"[LogDescription] : {logDesc}");
                }
                sb.AppendLine();
                sb.AppendLine($"[ObjectName] : {row["ObjectName"]}");
                sb.AppendLine();
                sb.AppendLine($"[LogCreatedOn] : {row["LogCreatedOn"]}");
                sb.AppendLine();
                sb.AppendLine(new string('#', 100));
                sb.AppendLine();
            }
            return sb.ToString();
        }
    }
}
