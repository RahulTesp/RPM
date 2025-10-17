using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.Data;
using System.Data.Common;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
// cron 0 0 7 * * *
namespace SessionValidationJob
{
    // To learn more about Microsoft Azure WebJobs SDK, please see https://go.microsoft.com/fwlink/?LinkID=320976
    internal class Program
    {
        static string CONN_STRING = string.Empty;
        static void Main(string[] args)
        {
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
            if (connStr == null)
            {
                Console.WriteLine("Connection string is null in appsettings.json.");
                return;
            }
            if (string.IsNullOrWhiteSpace(CONN_STRING))
            {
                Console.WriteLine("Connection string not found in environment variables.");
                return;
            }
            CONN_STRING = connStr;
            // Parse connection string for server and database info
            var builder = new DbConnectionStringBuilder { ConnectionString = connStr };
            string server = builder.ContainsKey("Server") ? builder["Server"].ToString() : "";
            string database = builder.ContainsKey("Initial Catalog") ? builder["Initial Catalog"].ToString() : "";

            Console.WriteLine($"Server: {server}");
            Console.WriteLine($"Database: {database}");

            Console.WriteLine("Starting Session validation WebJob...");
            try
            {
                IsTokenExpired();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }

            Console.WriteLine("WebJob finished.");
        }
        public static void IsTokenExpired()
        {

            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenValidationParameters = new TokenValidationParameters
            {
                RequireSignedTokens = false,
                ValidateIssuerSigningKey = false,
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = true,
                SignatureValidator = (token, parameters) => { return new JwtSecurityToken(token); }

            };
            List<string> Tokens = GetAllLoginSessions(CONN_STRING);
            foreach (string jwtToken in Tokens)
            {

                try
                {
                    ClaimsPrincipal claimsPrincipal = tokenHandler.ValidateToken(jwtToken, tokenValidationParameters, out SecurityToken validatedToken);
                }
                catch (SecurityTokenExpiredException)
                {
                    UpdInvalidSessionZero(jwtToken, CONN_STRING);
                }
                catch (Exception)
                {
                    Console.WriteLine("Not able to validate token");
                }
            }

        }

        public static List<string> GetAllLoginSessions(string ConnectionString)
        {
            try
            {
                List<string> Tokens = new List<string>();

                using (SqlConnection con = new SqlConnection(ConnectionString))
                {
                    SqlCommand command = new SqlCommand("usp_GetAllLoginSessions", con);
                    command.CommandType = CommandType.StoredProcedure;
                    SqlParameter returnParameter = command.Parameters.Add("RetVal", SqlDbType.Int);
                    returnParameter.Direction = ParameterDirection.ReturnValue;
                    con.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        string? jwtToken = Convert.ToString(reader["JwtToken"]);
                        Tokens.Add(jwtToken);
                    }

                    con.Close();
                }
                return Tokens;
            }
            catch (Exception ex) { throw ex; }
        }

        public static void UpdInvalidSessionZero(string jwtToken, string ConnectionString)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(ConnectionString))
                {
                    SqlCommand command = new SqlCommand("usp_UpdInvalidSessionzero", con);
                    command.Parameters.AddWithValue("@JwtToken", jwtToken);
                    command.CommandType = CommandType.StoredProcedure;
                    SqlParameter returnParameter = command.Parameters.Add("RetVal", SqlDbType.Int);
                    returnParameter.Direction = ParameterDirection.ReturnValue;
                    con.Open();
                    command.ExecuteReader();
                    con.Close();
                }

            }
            catch (Exception ex) { throw ex; }

        }

    }
}
