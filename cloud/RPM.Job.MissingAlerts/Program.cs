using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;

class Program
{
    static string CONN_STRING = string.Empty;

    static async Task Main(string[] args)
    {
        // Set up configuration
        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var rpmSettings = config.GetSection("RPM").Get<RpmSettings>();
        CONN_STRING = rpmSettings?.ConnectionString;

        if (string.IsNullOrEmpty(CONN_STRING))
        {
            Console.WriteLine("Connection string is null or empty.");
            return;
        }

        Console.WriteLine("WebJob started...");

        while (true)
        {
            try
            {
                using SqlConnection connection = new SqlConnection(CONN_STRING);
                await connection.OpenAsync();

                // First stored procedure
                using (SqlCommand command = new SqlCommand("usp_InsMissingAlerts", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.CommandTimeout = 300;

                    SqlParameter returnParameter = command.Parameters.Add("RetVal", SqlDbType.Int);
                    returnParameter.Direction = ParameterDirection.ReturnValue;

                    await command.ExecuteNonQueryAsync();
                    int result1 = (int)returnParameter.Value;
                    Console.WriteLine($"usp_InsMissingAlerts executed with return value: {result1}");
                }

                // Second stored procedure
                using (SqlCommand command1 = new SqlCommand("usp_DelPatientProgramPriority", connection))
                {
                    command1.CommandType = CommandType.StoredProcedure;
                    command1.CommandTimeout = 300;

                    SqlParameter returnParameter1 = command1.Parameters.Add("RetVal", SqlDbType.Int);
                    returnParameter1.Direction = ParameterDirection.ReturnValue;

                    await command1.ExecuteNonQueryAsync();
                    int result2 = (int)returnParameter1.Value;
                    Console.WriteLine($"usp_DelPatientProgramPriority executed with return value: {result2}");
                }

                await connection.CloseAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception: " + ex.ToString());
            }

            // Optional: Delay between executions
            await Task.Delay(TimeSpan.FromMinutes(1)); // Adjust as needed
        }
    }
}

public class RpmSettings
{
    public string? ConnectionString { get; set; }
}
