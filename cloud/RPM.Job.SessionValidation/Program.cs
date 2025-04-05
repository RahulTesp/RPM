using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

class Program
{
    static string CONN_STRING =string.Empty;
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
                var tokenHandler = new JwtSecurityTokenHandler();
                var tokenValidationParameters = new TokenValidationParameters
                {
                    RequireSignedTokens = false,
                    ValidateIssuerSigningKey = false,
                    //IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("remotepatientmonitoring")),
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
                        //UpdInvalidSessionZero(jwtToken, CONN_STRING);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("exception:" + ex);
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
                    string jwtToken = Convert.ToString(reader["JwtToken"]);
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
public class RpmSettings
{
    public string? ConnectionString { get; set; }
}
