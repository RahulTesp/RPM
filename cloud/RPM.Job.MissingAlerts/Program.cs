using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;

class Program
{
    static string CONN_STRING =string.Empty;
    private static Timer _timer = null;
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
                using (SqlConnection connection = new SqlConnection(CONN_STRING))
                {
                    SqlCommand command = new SqlCommand("usp_InsMissingAlerts", connection);
                    command.CommandType = CommandType.StoredProcedure;
                    SqlParameter returnParameter = command.Parameters.Add("RetVal", SqlDbType.Int);
                    returnParameter.Direction = ParameterDirection.ReturnValue;
                    connection.Open();
                    command.ExecuteNonQuery();
                    int id = (int)returnParameter.Value;
                    connection.Close();
                    string message = returnParameter.Value.ToString();
                    SqlCommand command1 = new SqlCommand("usp_DelPatientProgramPriority", connection);
                    command1.CommandType = CommandType.StoredProcedure;
                    SqlParameter returnParameter1 = command1.Parameters.Add("RetVal", SqlDbType.Int);
                    returnParameter1.Direction = ParameterDirection.ReturnValue;
                    connection.Open();
                    command1.ExecuteNonQuery();
                    int id1 = (int)returnParameter1.Value;
                    connection.Close();
                    string message1 = returnParameter1.Value.ToString();

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
