using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.Data;
using System.IdentityModel.Tokens.Jwt;


class Program
{
    static string CONN_STRING = string.Empty;

    static async Task Main(string[] args)
    {
        // Set up configuration
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
        Console.WriteLine($"RPM Connection String: {connStr}");
        if (connStr == null)
        {
            Console.WriteLine("Connection string is null in appsettings.json.");
            return;
        }
        CONN_STRING = connStr;

        if (string.IsNullOrWhiteSpace(CONN_STRING))
        {
            Console.WriteLine("Connection string is null or empty.");
            return;
        }

        Console.WriteLine("WebJob started...");

        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(1)); // Run every 1 minute
        while (await timer.WaitForNextTickAsync())
        {
            await CheckAndInvalidateExpiredTokens();
        }
    }

    private static async Task CheckAndInvalidateExpiredTokens()
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var tokenValidationParameters = new TokenValidationParameters
        {
            RequireSignedTokens = false,
            ValidateIssuerSigningKey = false,
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            SignatureValidator = (token, parameters) => new JwtSecurityToken(token)
        };

        try
        {
            var tokens = await GetAllLoginSessionsAsync(CONN_STRING);

            foreach (var jwtToken in tokens)
            {
                try
                {
                    tokenHandler.ValidateToken(jwtToken, tokenValidationParameters, out _);
                }
                catch (SecurityTokenExpiredException)
                {
                    await UpdInvalidSessionZeroAsync(jwtToken, CONN_STRING);
                }
                catch (Exception)
                {
                    Console.WriteLine("Invalid token or unable to validate.");
                    // Optional: You can decide to update invalid session here too
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error during token check: " + ex.Message);
        }
    }

    private static async Task<List<string>> GetAllLoginSessionsAsync(string connectionString)
    {
        var tokens = new List<string>();

        try
        {
            using var con = new SqlConnection(connectionString);
            using var command = new SqlCommand("usp_GetAllLoginSessions", con)
            {
                CommandType = CommandType.StoredProcedure
            };

            await con.OpenAsync();

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                string? jwtToken = reader["JwtToken"]?.ToString();
                if (!string.IsNullOrWhiteSpace(jwtToken))
                {
                    tokens.Add(jwtToken);
                }
            }

            return tokens;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error fetching sessions: " + ex.Message);
            return tokens;
        }
    }

    private static async Task UpdInvalidSessionZeroAsync(string jwtToken, string connectionString)
    {
        try
        {
            using var con = new SqlConnection(connectionString);
            using var command = new SqlCommand("usp_UpdInvalidSessionzero", con)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.AddWithValue("@JwtToken", jwtToken);
            command.Parameters.Add("RetVal", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;

            await con.OpenAsync();
            await command.ExecuteNonQueryAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error updating invalid session: " + ex.Message);
        }
    }
}