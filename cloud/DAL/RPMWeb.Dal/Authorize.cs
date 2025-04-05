using Microsoft.IdentityModel.Tokens;
using RPMWeb.Data.Common;
using System.Data;
using Microsoft.Data.SqlClient;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

using Roles = RPMWeb.Data.Common.Roles;

namespace RPMWeb.Dal
{
    public sealed class Authorize
    {
        public string IsSessionValid(string JwtToken, string ConnectionString)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    SqlCommand command = new SqlCommand("usp_UpdLoginSession", connection);
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@JwtToken", JwtToken);
                    SqlParameter returnParameter = new SqlParameter();
                    returnParameter.ParameterName = "@UserName";
                    returnParameter.SqlDbType = SqlDbType.NVarChar;
                    returnParameter.Direction = ParameterDirection.Output;
                    returnParameter.Size = 50;
                    command.Parameters.Add(returnParameter);
                    SqlParameter returnParameter1 = new SqlParameter();
                    returnParameter1.ParameterName = "@IsSucess";
                    returnParameter1.SqlDbType = SqlDbType.Bit;
                    returnParameter1.Direction = ParameterDirection.Output;
                    returnParameter1.Size = 10;
                    command.Parameters.Add(returnParameter1);
                    connection.Open();
                    command.ExecuteNonQuery();
                    var IsSucess = returnParameter1.Value;
                    string UserName = returnParameter.Value.ToString();
                    if (IsSucess.Equals(true))
                    {
                        return UserName;
                    }
                    connection.Close();

                    return string.Empty;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public bool IsNewSessionValid(string JwtToken, string userName, string ConnectionString)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    SqlCommand command = new SqlCommand("usp_getVerifySession", connection);
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@JwtToken", JwtToken);
                    command.Parameters.AddWithValue("@UserName", userName);
                    SqlParameter returnParameter = new SqlParameter();
                    SqlParameter returnParameter1 = new SqlParameter();
                    returnParameter1.ParameterName = "@IsSucess";
                    returnParameter1.SqlDbType = SqlDbType.Bit;
                    returnParameter1.Direction = ParameterDirection.Output;
                    returnParameter1.Size = 10;
                    command.Parameters.Add(returnParameter1);
                    connection.Open();
                    command.ExecuteNonQuery();
                    var IsSucess = returnParameter1.Value;
                    //string UserName = returnParameter.Value.ToString();
                    if (IsSucess.Equals(true))
                    {
                        connection.Close();
                        return true;

                    }
                    else
                    {
                        connection.Close();
                        return false;
                    }


                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public string getUserName(int id, string ConnectionString)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    SqlCommand command = new SqlCommand("GetUserName", connection);
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@userid", id);
                    SqlParameter returnParameter = new SqlParameter();
                    returnParameter.ParameterName = "@UserName";
                    returnParameter.SqlDbType = SqlDbType.NVarChar;
                    returnParameter.Direction = ParameterDirection.Output;
                    returnParameter.Size = 50;
                    command.Parameters.Add(returnParameter);
                    connection.Open();
                    command.ExecuteNonQuery();
                    string UserName = returnParameter.Value.ToString();
                    connection.Close();
                    return UserName;

                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public int getBearerId(string token, string ConnectionString)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    SqlCommand command = new SqlCommand("usp_getbearerid", connection);
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@token", token);
                    SqlParameter returnParameter = new SqlParameter();
                    returnParameter.ParameterName = "@BearerId";
                    returnParameter.SqlDbType = SqlDbType.NVarChar;
                    returnParameter.Direction = ParameterDirection.Output;
                    returnParameter.Size = 50;
                    command.Parameters.Add(returnParameter);
                    connection.Open();
                    command.ExecuteNonQuery();
                    int BearerId = Convert.ToInt32(returnParameter.Value);
                    connection.Close();
                    return BearerId;

                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public PatientData IsPatientSessionValid(string JwtToken, string ConnectionString)
        {
            try
            {
                PatientData data = new PatientData();
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    SqlCommand command = new SqlCommand("usp_UpdLoginSession", connection);
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@JwtToken", JwtToken);
                    SqlParameter returnParameter = new SqlParameter();
                    returnParameter.ParameterName = "@UserName";
                    returnParameter.SqlDbType = SqlDbType.NVarChar;
                    returnParameter.Direction = ParameterDirection.Output;
                    returnParameter.Size = 50;
                    command.Parameters.Add(returnParameter);
                    SqlParameter returnParameter1 = new SqlParameter();
                    returnParameter1.ParameterName = "@IsSucess";
                    returnParameter1.SqlDbType = SqlDbType.Bit;
                    returnParameter1.Direction = ParameterDirection.Output;
                    returnParameter1.Size = 10;
                    command.Parameters.Add(returnParameter1);
                    connection.Open();
                    command.ExecuteNonQuery();
                    var IsSucess = returnParameter1.Value;
                    string UserName = returnParameter.Value.ToString();
                    connection.Close();
                    if (IsSucess.Equals(true))
                    {
                        connection.Open();
                        using (SqlCommand command1 = new SqlCommand("usp_GetPatientProgramInfoBySession", connection))
                        {
                            command1.CommandType = CommandType.StoredProcedure;
                            command1.Parameters.AddWithValue("@Session", JwtToken);
                            command1.Parameters.AddWithValue("@CreatedBy", UserName);
                            SqlParameter returnParameter2 = command1.Parameters.Add("RetVal", SqlDbType.Int);
                            returnParameter2.Direction = ParameterDirection.ReturnValue;
                            SqlDataReader reader = command1.ExecuteReader();

                            while (reader.Read())
                            {
                                data.PatientId = Convert.ToInt32(reader["PatientId"]);
                                data.PatientProgramId = Convert.ToInt32(reader["PatientProgramId"]);
                                data.UserName = UserName;
                            }


                        }
                        connection.Close();
                    }
                }
                return data;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        //public bool ValidateTkn(string token)
        //{
        //    try
        //    {

        //        var tokenHandler = new JwtSecurityTokenHandler();
        //        string key = "remotepatientmonitoring"; //Secret key which will be used later during validation
        //        var issuer = "http://tesplabs.com";  //normally this will be your site URL
        //        var bykey = Encoding.ASCII.GetBytes(key);
        //        tokenHandler.ValidateToken(token, new TokenValidationParameters
        //        {
        //            ValidAudience = issuer,
        //            ValidateIssuerSigningKey = true,
        //            IssuerSigningKey = new SymmetricSecurityKey(bykey),
        //            ValidateIssuer = true,
        //            ValidIssuer = issuer,
        //            ValidateAudience = false,
        //            // set clockskew to zero so tokens expire exactly at token expiration time (instead of 5 minutes later)
        //            ClockSkew = TimeSpan.Zero
        //        }, out SecurityToken validatedToken);

        //        var jwtToken = (JwtSecurityToken)validatedToken;
        //        if (jwtToken != null)
        //        {
        //            return true;
        //        }
        //    }
        //    catch (SecurityTokenExpiredException ex)
        //    {
        //        return false;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //    return false;
        //}
        public bool ValidateTkn(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                string key = "remotepatientmonitoringtesplabspvtltd"; // Secret key (must be at least 32 characters for HS256)
                var issuer = "http://sample.com";  // Issuer and Audience
                var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
                var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidAudience = issuer,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = securityKey,
                    ValidateIssuer = true,
                    ValidIssuer = issuer,
                    ValidateAudience = true,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;
                if (jwtToken != null)
                {
                    return true;
                }
            }
            catch (SecurityTokenExpiredException)
            {
                return false;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return false;
        }
        public bool VerifyUserName(string UserName, string ConnectionString)
        {
            bool ret = true;
            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    SqlCommand command = new SqlCommand("usp_VerifyUserName", connection);
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@UserName", UserName);
                    SqlParameter returnParameter = command.Parameters.Add("RetVal", SqlDbType.Int);
                    returnParameter.Direction = ParameterDirection.ReturnValue;
                    connection.Open();
                    command.ExecuteNonQuery();
                    var id = returnParameter.Value;
                    if (id.Equals(0))
                    {
                        ret= false;
                    }
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return ret;
        }

        public bool CheckUserLockedStatus(string UserName, string ConnectionString)
        {
            bool ret = true;
            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    SqlCommand command = new SqlCommand("usp_CheckUserLocked", connection);
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@UserName", UserName);
                    SqlParameter returnParameter = command.Parameters.Add("RetVal", SqlDbType.Int);
                    returnParameter.Direction = ParameterDirection.ReturnValue;
                    connection.Open();
                    command.ExecuteNonQuery();
                    var id = returnParameter.Value;
                    if (id.Equals(0))
                    {
                        ret= false;
                    }
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return ret;
        }

        public UserRoleConfig GetUserRoleConfig(string ConnectionString, string UserName)
        {
            UserRoleConfig userRoleConfig = new UserRoleConfig();
            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    SqlCommand command = new SqlCommand("GetUserRoleConfig", connection);
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@UserName", UserName);
                    SqlParameter returnParameter = command.Parameters.Add("RetVal", SqlDbType.Int);
                    returnParameter.Direction = ParameterDirection.ReturnValue;
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();
                   
                    while (reader.Read())
                    {
                        userRoleConfig.RoleId = Convert.ToInt32(reader["RoleId"]);
                        userRoleConfig.IsMailSend = Convert.ToBoolean(reader["isMailsend"]);
                        userRoleConfig.IsSmsSend = Convert.ToBoolean(reader["isSmsSend"]);
                    }
                    if (reader.FieldCount == 0)
                    {
                        return null;
                    }
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return userRoleConfig;
        }
        public bool CheckUserActive(string ConnectionString, string UserName)
        {
            
            bool ret = true;
            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    SqlCommand command = new SqlCommand("usp_GetUserStatus", connection);
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@UserName", UserName);
                    SqlParameter returnParameter = command.Parameters.Add("RetVal", SqlDbType.Int);
                    returnParameter.Direction = ParameterDirection.ReturnValue;
                    connection.Open();
                    command.ExecuteNonQuery();
                    var id = returnParameter.Value;
                    if (id.Equals(0))
                    {
                        ret= false;
                    }
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return ret;
        }
        
        public bool LockUser(string UserName, string ConnectionString)
        {
            bool ret = true;
            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    SqlCommand command = new SqlCommand("usp_LockUser", connection);
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@UserName", UserName);
                    SqlParameter returnParameter = command.Parameters.Add("RetVal", SqlDbType.Int);
                    returnParameter.Direction = ParameterDirection.ReturnValue;
                    connection.Open();
                    command.ExecuteNonQuery();
                    var id = returnParameter.Value;
                    if (id.Equals(0))
                    {
                        ret= false;
                    }
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return ret;
        }
        public int UpdateRetryCount(string UserName, string ConnectionString)
        {
            int count = 0;
            bool ret = true;
            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    SqlCommand command = new SqlCommand("usp_UpdateRetryCount", connection);
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@UserName", UserName);
                    SqlParameter returnParameter = command.Parameters.Add("RetVal", SqlDbType.Int);
                    returnParameter.Direction = ParameterDirection.ReturnValue;
                    connection.Open();
                    command.ExecuteNonQuery();
                    var id = returnParameter.Value;
                    count=Convert.ToInt32(returnParameter.Value);
                    if (id.Equals(0))
                    {
                        ret= false;
                    }
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return count;
        }

        public int UpdateLoginDetails(string UserName, string ConnectionString)
        {
            int count = 0;
            bool ret = true;
            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    SqlCommand command = new SqlCommand("usp_UpdLogindetails", connection);
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@UserName", UserName);
                    SqlParameter returnParameter = command.Parameters.Add("RetVal", SqlDbType.Int);
                    returnParameter.Direction = ParameterDirection.ReturnValue;
                    connection.Open();
                    command.ExecuteNonQuery();
                    var id = returnParameter.Value;
                    count=Convert.ToInt32(returnParameter.Value);
                    if (id.Equals(0))
                    {
                        ret= false;
                    }
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return count;
        }
        public LoginDetails VerifyOtp(string Otp, string UserName, string ConnectionString)
        {
            LoginDetails loginDetails = new LoginDetails();
            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    SqlCommand command = new SqlCommand("usp_VerifyUserNameAndOtp", connection);
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@UserName", UserName);
                    command.Parameters.AddWithValue("@Otp", Otp);
                    SqlParameter returnParameter = command.Parameters.Add("RetVal", SqlDbType.Int);
                    returnParameter.Direction = ParameterDirection.ReturnValue;
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        loginDetails.Username = Convert.ToString(reader["UserName"]);
                        loginDetails.OTP = Convert.ToString(reader["OTP"]);
                        loginDetails.Token = Convert.ToString(reader["Token"]);
                        loginDetails.RetryCount = Convert.ToInt32(reader["RetryCount"]);
                        loginDetails.Match = Convert.ToString(reader["status"]);

                    }
                    if (reader.FieldCount == 0)
                    {
                        return null;
                    }

                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return loginDetails;
        }

        public LoginDetails InsertLoginDetails(string UserName, string otp, string tkn, string ConnectionString)
        {
            int result;
            LoginDetails loginDetails = new LoginDetails();
            try
            {
                using (SqlConnection con = new SqlConnection(ConnectionString))
                {
                    SqlCommand sqlCommand = new SqlCommand("usp_InsLoginDetails", con);
                    sqlCommand.CommandType = CommandType.StoredProcedure;
                    sqlCommand.Parameters.AddWithValue("@UserName", UserName);
                    sqlCommand.Parameters.AddWithValue("@Otp", otp);
                    sqlCommand.Parameters.AddWithValue("@Sessiontkn", tkn);
                    SqlParameter sqlParameter = sqlCommand.Parameters.Add("RetVal", SqlDbType.Int);
                    sqlParameter.Direction = ParameterDirection.ReturnValue;
                    con.Open();
                    //sqlCommand.ExecuteNonQuery();
                    SqlDataReader reader = sqlCommand.ExecuteReader();
                    while (reader.Read())
                    {

                        loginDetails.Username = Convert.ToString(reader["UserName"]);
                        loginDetails.OTP = Convert.ToString(reader["OTP"]);
                        loginDetails.Token = Convert.ToString(reader["Token"]);
                        loginDetails.RetryCount = Convert.ToInt32(reader["RetryCount"]);

                    }
                    if (reader.FieldCount == 0)
                    {
                        return null;
                    }
                    con.Close();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return loginDetails;
        }
        public LoginResponseToken CreateNewToken(int time, string connectionString)
        {
            LoginResponseToken resp = new LoginResponseToken();
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    string key = "remotepatientmonitoringtesplabspvtltd"; // Secret key (must be at least 32 characters for HS256)
                    var issuer = "http://sample.com";  // Issuer and Audience
                    var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
                    var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

                    // Define claims
                    var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("valid", "1"),
                new Claim("userid", "1"),
                new Claim("name", "tesp")
            };

                    // Create token
                    var token = new JwtSecurityToken(
                        issuer,
                        issuer,
                        claims,
                        expires: DateTime.UtcNow.AddHours(10),
                        signingCredentials: credentials
                    );

                    var jwttoken = new JwtSecurityTokenHandler().WriteToken(token);
                    resp.tkn = jwttoken;
                    resp.tkt = "Bearer";
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return resp;
        }

        //public LoginResponseToken CreateNewToken(int time, string connectionString)
        //{
        //    LoginResponseToken resp = new LoginResponseToken();
        //    try
        //    {
        //        using (SqlConnection connection = new SqlConnection(connectionString))
        //        {
        //            string key = "remotepatientmonitoring"; //Secret key which will be used later during validation    
        //            var issuer = "http://tesplabs.com";  //normally this will be your site URL    
        //            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        //            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
        //            //Create a List of Claims, Keep claims name short    
        //            var permClaims = new List<Claim>();
        //            permClaims.Add(new Claim(Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()));
        //            permClaims.Add(new Claim("valid", "1"));
        //            permClaims.Add(new Claim("userid", "1"));
        //            permClaims.Add(new Claim("name", "tesp"));
        //            //Create Security Token object by giving required parameters    
        //            var token = new JwtSecurityToken(issuer, //Issure    
        //                                issuer,  //Audience    
        //                                permClaims,
        //                                expires: DateTime.Now.AddMinutes(time),
        //                                signingCredentials: credentials);
        //            var jwttoken = new JwtSecurityTokenHandler().WriteToken(token);
        //            resp.tkn = jwttoken;
        //            resp.tkt = "Bearer";



        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //    return resp;
        //}
        public LoginResponse Login(RPMWeb.Data.Common.RPMLogin verPass, string ConnectionString)
        {
            LoginResponse resp = new LoginResponse();
            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    SqlCommand command = new SqlCommand("usp_VerifyPassword", connection);
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@UserName", verPass.UserName);
                    command.Parameters.AddWithValue("@Password", verPass.Password);
                    SqlParameter returnParameter = new SqlParameter();
                    returnParameter.ParameterName = "@LastPasswordChangedDate";
                    returnParameter.SqlDbType = SqlDbType.NVarChar;
                    returnParameter.Direction = ParameterDirection.Output;
                    returnParameter.Size = 50;
                    command.Parameters.Add(returnParameter);
                    SqlParameter returnParameter1 = new SqlParameter();
                    returnParameter1.ParameterName = "@IsSucess";
                    returnParameter1.SqlDbType = SqlDbType.Bit;
                    returnParameter1.Direction = ParameterDirection.Output;
                    returnParameter1.Size = 10;
                    command.Parameters.Add(returnParameter1);
                    connection.Open();
                    command.ExecuteNonQuery();

                    DateTime date;
                    if (returnParameter == null || returnParameter.Value==null)
                    {
                        throw new Exception("Something Went Wrong.");
                    }
                    if (returnParameter.Value != DBNull.Value)
                    {

                        // Parse and calculate new expiration date
                        date = DateTime.Parse(returnParameter.Value.ToString());
                        DateTime newEndDate = date.AddDays(30);

                        // Secret key (must be at least 32 characters for HS256)
                        string key = "remotepatientmonitoringtesplabspvtltd";
                        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
                        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

                        // Issuer and Audience
                        var issuer = "http://sample.com";

                        // Define claims
                        var claims = new List<Claim>
                        {
                            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                            new Claim("valid", "1"),
                            new Claim("userid", "1"),
                            new Claim("name", "tesp")
                        };


                        // Create token
                        var token = new JwtSecurityToken(
                            issuer,
                            issuer,
                            claims,
                            expires: DateTime.UtcNow.AddHours(10),
                            signingCredentials: credentials
                        );


                        //date = DateTime.Parse(returnParameter.Value.ToString());
                        //DateTime newenddate = date.AddDays(30);
                        //string key = "remotepatientmonitoring"; //Secret key which will be used later during validation    
                        //var issuer = "http://tesplabs.com";  //normally this will be your site URL    
                        //var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
                        //var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
                        ////Create a List of Claims, Keep claims name short    
                        //var permClaims = new List<Claim>();
                        //permClaims.Add(new Claim(Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()));
                        //permClaims.Add(new Claim("valid", "1"));
                        //permClaims.Add(new Claim("userid", "1"));
                        //permClaims.Add(new Claim("name", "tesp"));
                        ////Create Security Token object by giving required parameters    
                        //var token = new JwtSecurityToken(issuer, //Issure    
                        //                    issuer,  //Audience    
                        //                    permClaims,
                        //                    expires: DateTime.Now.AddHours(10),
                        //                    signingCredentials: credentials);
                        var jwttoken = new JwtSecurityTokenHandler().WriteToken(token);
                        resp.tkn = jwttoken;
                        resp.tkt = "Bearer";
                        Random rnd = new Random();
                        SqlCommand command1 = new SqlCommand("usp_InsLoginSession", connection);
                        command1.CommandType = CommandType.StoredProcedure;
                        command1.Parameters.AddWithValue("@UserName", verPass.UserName);
                        command1.Parameters.AddWithValue("@JwtToken", jwttoken);
                        command1.Parameters.AddWithValue("@LoginDateTime", DateTime.UtcNow);
                        command1.Parameters.AddWithValue("@LastActionDateTime", DateTime.UtcNow);
                        SqlParameter returnPar = command1.Parameters.Add("RetVal", SqlDbType.Bit);
                        returnPar.Direction = ParameterDirection.ReturnValue;

                        command1.ExecuteNonQuery();
                        // var id = returnPar.Value;

                        resp.reqPasswordchange = false;
                        SqlCommand command2 = new SqlCommand("usp_GetUserRolesPatient", connection);
                        command2.CommandType = CommandType.StoredProcedure;
                        command2.Parameters.AddWithValue("@UserName", verPass.UserName);
                        SqlDataReader reader = command2.ExecuteReader();
                        List<Roles> roles = new List<Roles>();
                        while (reader.Read())
                        {
                            Roles info = new Roles();
                            info.Id = (!DBNull.Value.Equals(reader["RoleId"])) ? Convert.ToInt32(reader["RoleId"]) : 0;
                            info.Role = reader["RoleName"].ToString();
                            info.ProgramName = (!DBNull.Value.Equals(reader["ProgramName"])) ? reader["ProgramName"].ToString() : null;
                            roles.Add(info);
                        }
                        resp.Roles = roles;

                        if (newEndDate >= DateTime.Now)
                        {
                            resp.reqPasswordchange = false;

                        }
                        else
                        {
                            resp.reqPasswordchange = false;
                            //resp.reqPasswordchange = true;
                        }
                        connection.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return resp;
        }

        public LoginResponse PatientLogin(RPMWeb.Data.Common.Login verPass, string ConnectionString)
        {
            LoginResponse resp = new LoginResponse();
            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    SqlCommand command = new SqlCommand("usp_VerifyPassword", connection);
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@UserName", verPass.UserName);
                    command.Parameters.AddWithValue("@Password", verPass.Password);
                    SqlParameter returnParameter = new SqlParameter();
                    returnParameter.ParameterName = "@LastPasswordChangedDate";
                    returnParameter.SqlDbType = SqlDbType.NVarChar;
                    returnParameter.Direction = ParameterDirection.Output;
                    returnParameter.Size = 50;
                    command.Parameters.Add(returnParameter);
                    SqlParameter returnParameter1 = new SqlParameter();
                    returnParameter1.ParameterName = "@IsSucess";
                    returnParameter1.SqlDbType = SqlDbType.Bit;
                    returnParameter1.Direction = ParameterDirection.Output;
                    returnParameter1.Size = 10;
                    command.Parameters.Add(returnParameter1);
                    connection.Open();
                    command.ExecuteNonQuery();

                    DateTime date;

                    if (returnParameter.Value != DBNull.Value)
                    {
                        //date = DateTime.Parse(returnParameter.Value.ToString());
                        //DateTime newenddate = date.AddDays(30);
                        //string key = "remotepatientmonitoring"; //Secret key which will be used later during validation    
                        //var issuer = "http://tesplabs.com";  //normally this will be your site URL    
                        //var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
                        //var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
                        ////Create a List of Claims, Keep claims name short    
                        //var permClaims = new List<Claim>();
                        //permClaims.Add(new Claim(Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()));
                        //permClaims.Add(new Claim("valid", "1"));
                        //permClaims.Add(new Claim("userid", "1"));
                        //permClaims.Add(new Claim("name", "tesp"));
                        ////Create Security Token object by giving required parameters    
                        //var token = new JwtSecurityToken(issuer, //Issure    
                        //                    issuer,  //Audience    
                        //                    permClaims,
                        //                    expires: DateTime.Now.AddHours(10),
                        //                    signingCredentials: credentials);
                        //var jwttoken = new JwtSecurityTokenHandler().WriteToken(token);
                        //resp.tkn = jwttoken;
                        //resp.tkt = "Bearer";
                        // Parse and calculate new expiration date
                        date = DateTime.Parse(returnParameter.Value.ToString());
                        DateTime newEndDate = date.AddDays(30);

                        // Secret key (must be at least 32 characters for HS256)
                        string key = "remotepatientmonitoringtesplabspvtltd";
                        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
                        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

                        // Issuer and Audience
                        var issuer = "http://sample.com";

                        // Define claims
                        var claims = new List<Claim>
                        {
                            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                            new Claim("valid", "1"),
                            new Claim("userid", "1"),
                            new Claim("name", "tesp")
                        };


                        // Create token
                        var token = new JwtSecurityToken(
                            issuer,
                            issuer,
                            claims,
                            expires: DateTime.UtcNow.AddHours(10),
                            signingCredentials: credentials
                        );
                        var jwttoken = new JwtSecurityTokenHandler().WriteToken(token);
                        Random rnd = new Random();
                        SqlCommand command1 = new SqlCommand("usp_InsLoginSession", connection);
                        command1.CommandType = CommandType.StoredProcedure;
                        command1.Parameters.AddWithValue("@UserName", verPass.UserName);
                        command1.Parameters.AddWithValue("@JwtToken", jwttoken);
                        command1.Parameters.AddWithValue("@LoginDateTime", DateTime.UtcNow);
                        command1.Parameters.AddWithValue("@LastActionDateTime", DateTime.UtcNow);
                        SqlParameter returnPar = command1.Parameters.Add("RetVal", SqlDbType.Bit);
                        returnPar.Direction = ParameterDirection.ReturnValue;

                        command1.ExecuteNonQuery();
                        // var id = returnPar.Value;

                        resp.reqPasswordchange = false;
                        SqlCommand command2 = new SqlCommand("usp_GetUserRolesPatient", connection);
                        command2.CommandType = CommandType.StoredProcedure;
                        command2.Parameters.AddWithValue("@UserName", verPass.UserName);
                        SqlDataReader reader = command2.ExecuteReader();
                        List<Roles> roles = new List<Roles>();
                        while (reader.Read())
                        {
                            Roles info = new Roles();
                            info.Id = (!DBNull.Value.Equals(reader["RoleId"])) ? Convert.ToInt32(reader["RoleId"]) : 0;
                            info.Role = reader["RoleName"].ToString();
                            info.ProgramName = (!DBNull.Value.Equals(reader["ProgramName"])) ? reader["ProgramName"].ToString() : null;
                            roles.Add(info);
                        }
                        resp.Roles = roles;

                        if (newEndDate >= DateTime.Now)
                        {
                            resp.reqPasswordchange = false;

                        }
                        else
                        {
                            resp.reqPasswordchange = false;
                            //resp.reqPasswordchange = true;
                        }
                        connection.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return resp;
        }
        public bool LogOut(Logout logout, string ConnectionString)
        {
            bool ret = true;
            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    SqlCommand command = new SqlCommand("usp_Logout", connection);
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@JwtToken", logout.JwtToken);
                    command.Parameters.AddWithValue("@CreatedBy", logout.createdBy);
                    SqlParameter returnParameter = command.Parameters.Add("RetVal", SqlDbType.Int);
                    returnParameter.Direction = ParameterDirection.ReturnValue;
                    connection.Open();
                    command.ExecuteNonQuery();
                    var id = returnParameter.Value;
                    if (id.Equals(0))
                    {
                        ret= false;
                    }
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return ret;
        }
        public bool UpdatePassword(Updatepassword updatepassword, string ConnectionString)
        {
            bool ret = true;
            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    SqlCommand command = new SqlCommand("usp_UpdPasword", connection);
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@UserName", updatepassword.UserName);
                    command.Parameters.AddWithValue("@NewPassword", updatepassword.NewPassword);
                    command.Parameters.AddWithValue("@OldPassword", updatepassword.OldPassword);
                    SqlParameter returnParameter = command.Parameters.Add("RetVal", SqlDbType.Int);
                    returnParameter.Direction = ParameterDirection.ReturnValue;
                    connection.Open();
                    command.ExecuteNonQuery();
                    var id = returnParameter.Value;
                    connection.Close();
                    if (id.Equals(0))
                    {
                        ret = false;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return ret;
        }

        public List<Roles> GetRolesMasterData(string CreatedBy, string ConnectionString)
        {
            List<Roles> Listinfo = new List<Roles>();
            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    SqlCommand command = new SqlCommand("usp_GetMasterDataRoles", connection);
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.Add("@CreatedBy", SqlDbType.NVarChar).Value = CreatedBy;
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        Roles info = new Roles();
                        info.Id = (!DBNull.Value.Equals(reader["Id"])) ? Convert.ToInt32(reader["Id"]) : 0;
                        info.Role = reader["Name"].ToString();
                        Listinfo.Add(info);
                    }
                }
            }
            catch
            {
                throw;
            }
            return Listinfo;

        }
        public ContactDetails GetPhoneNumberByUserName(string username, string ConnectionString)
        {
            ContactDetails contactDetails = new ContactDetails();
            List<Roles> Listinfo = new List<Roles>();
            try
            {

                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    SqlCommand command = new SqlCommand("usp_GetPhoneNumber", connection);
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.AddWithValue("@UserName", username);
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {

                        contactDetails.MobileNumber = Convert.ToString(reader["MobileNo"]);
                        contactDetails.MailId =  Convert.ToString(reader["MailId"]);
                        contactDetails.FromMail =  Convert.ToString(reader["FromEmail"]);
                        contactDetails.Password =  Convert.ToString(reader["Password"]);
                    }
                }
            }
            catch
            {
                throw;
            }
            return contactDetails;

        }
        public LoginResponseToken GetSessionByUserName(string username, string ConnectionString)
        {
            LoginResponseToken loginResponseToken = new LoginResponseToken();
            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    SqlCommand command = new SqlCommand("usp_getSessionbyUserName", connection);
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@UserName", username);
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {

                        loginResponseToken.tkn = Convert.ToString(reader["JwtToken"]);

                    }
                }
            }
            catch
            {
                throw;
            }
            return loginResponseToken;

        }
        public DateTime? GetLastPswdChange(string username, string ConnectionString)
        {
            DateTime? lastPswdChange = null;
            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    SqlCommand command = new SqlCommand("usp_GetlastpasswordChange", connection);
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@UserName", username);
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {

                        lastPswdChange = Convert.ToDateTime(reader["LastPasswordChangedDate"]);

                    }
                    connection.Close();
                }
            }
            catch
            {
                throw;
            }
            return lastPswdChange;

        }

        public List<Roles> GetUserRoles(string username, string ConnectionString)
        {
            List<Roles> roles = new List<Roles>();
            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();
                    SqlCommand command = new SqlCommand("usp_GetUserRolesPatient", connection);
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@UserName", username);
                    SqlDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        Roles info = new Roles();
                        info.Id = (!DBNull.Value.Equals(reader["RoleId"])) ? Convert.ToInt32(reader["RoleId"]) : 0;
                        info.Role = reader["RoleName"].ToString();
                        info.ProgramName = (!DBNull.Value.Equals(reader["ProgramName"])) ? reader["ProgramName"].ToString() : null;
                        roles.Add(info);
                    }
                    connection.Close();
                }
            }
            catch
            {
                throw;
            }
            return roles;

        }
 
        public List<OperationalMasterData> GetOperationalMasterData(string CreatedBy, string ConnectionString)
        {
            List<OperationalMasterData> Listinfo = new List<OperationalMasterData>();
            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    SqlCommand command = new SqlCommand("usp_GetOperationalMasterData", connection);
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.Add("@CreatedBy", SqlDbType.NVarChar).Value = CreatedBy;
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        OperationalMasterData info = new OperationalMasterData();
                        info.Id= (!DBNull.Value.Equals(reader["Id"])) ? Convert.ToInt32(reader["Id"]) : 0;
                        info.Name = reader["Name"].ToString();
                        info.Type = reader["Type"].ToString();
                        Listinfo.Add(info);
                    }
                }
            }
            catch
            {
                throw;
            }
            return Listinfo;

        }


        public bool UpdateUserPassword(string username, string  password, string ConnectionString)
        {
            bool ret = true;
            List<OperationalMasterData> Listinfo = new List<OperationalMasterData>();
            try
            {

                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    SqlCommand command = new SqlCommand("usp_ResetUserPassword", connection);
                    command.CommandType = CommandType.StoredProcedure;
                   
                   command.Parameters.Add("@username", SqlDbType.NVarChar).Value = username;
                   command.Parameters.Add("@password", SqlDbType.NVarChar).Value = password;

                    SqlParameter returnParameter = command.Parameters.Add("RetVal", SqlDbType.Int);
                    returnParameter.Direction = ParameterDirection.ReturnValue;
                    connection.Open();
                    command.ExecuteNonQuery();
                    var id = returnParameter.Value;
                    connection.Close();
                    if (id.Equals(0))
                    {
                        ret = false;
                    }
                }
            }
            catch
            {
                throw;
            }
            return ret;

        }
        public bool ClearOldSessions(string username, string ConnectionString)
        {
            bool ret = true;
            List<OperationalMasterData> Listinfo = new List<OperationalMasterData>();
            try
            {

                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    SqlCommand command = new SqlCommand("usp_ClearLoginSessions", connection);
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.Add("@username", SqlDbType.NVarChar).Value = username;
                    

                    SqlParameter returnParameter = command.Parameters.Add("RetVal", SqlDbType.Int);
                    returnParameter.Direction = ParameterDirection.ReturnValue;
                    connection.Open();
                    command.ExecuteNonQuery();
                    var id = returnParameter.Value;
                    connection.Close();
                    if (id.Equals(0))
                    {
                        ret = false;
                    }
                }
            }
            catch
            {
                throw;
            }
            return ret;

        }

        public bool UnlockUser(int? userId, int? patientId, string ConnectionString)
        {
            bool ret = true;
            List<OperationalMasterData> Listinfo = new List<OperationalMasterData>();
            try
            {

                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    SqlCommand command = new SqlCommand("usp_Unlockuser", connection);
                    command.CommandType = CommandType.StoredProcedure;
                    if (userId!=null)
                    {
                        command.Parameters.Add("@userid", SqlDbType.Int).Value = userId;
                    }
                    else if (patientId!=null)
                    {
                        command.Parameters.Add("@patientid", SqlDbType.Int).Value = patientId;
                    }
                    else
                    {
                        return false;
                    }
                    SqlParameter returnParameter = command.Parameters.Add("RetVal", SqlDbType.Int);
                    returnParameter.Direction = ParameterDirection.ReturnValue;
                    connection.Open();
                    command.ExecuteNonQuery();
                    var id = returnParameter.Value;
                    connection.Close();
                    if (id.Equals(0))
                    {
                        ret = false;
                    }
                }
            }
            catch
            {
                throw;
            }
            return ret;

        }
        public bool UnlockUserByUsername(string username, string ConnectionString)
        {
            bool ret = true;
            List<OperationalMasterData> Listinfo = new List<OperationalMasterData>();
            try
            {

                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    SqlCommand command = new SqlCommand("Usp_UnlockUserByUsername", connection);
                    command.CommandType = CommandType.StoredProcedure;
                    
                        command.Parameters.Add("@username", SqlDbType.NVarChar).Value = username;
                    
                    
                    SqlParameter returnParameter = command.Parameters.Add("RetVal", SqlDbType.Int);
                    returnParameter.Direction = ParameterDirection.ReturnValue;
                    connection.Open();
                    command.ExecuteNonQuery();
                    var id = returnParameter.Value;
                    connection.Close();
                    if (id.Equals(0))
                    {
                        ret = false;
                    }
                }
            }
            catch
            {
                throw;
            }
            return ret;

        }
        public DataSet GetMasterDataForStatesAndCities(string CreatedBy, string ConnectionString)
        {
            DataSet ds;
            try
            {
                using (SqlConnection con = new SqlConnection(ConnectionString))
                {
                    con.Open();

                    using (SqlCommand command = new SqlCommand("usp_GetMasterDataForStatesAndCities", con))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@CreatedBy", CreatedBy);
                        using (SqlDataAdapter sda = new SqlDataAdapter())
                        {
                            //command.Connection = con;
                            sda.SelectCommand = command;
                            using (ds = new DataSet())
                            {
                                sda.Fill(ds);
                                ds.Tables[0].TableName = "TimeZones";
                                ds.Tables[1].TableName = "States";
                                ds.Tables[2].TableName = "Cities";
                            }
                        }
                    }
                }
                return ds;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public UserPermission UserAccessRights(string username, int roleid, string ConnectionString)
        {
            UserPermission ret = new UserPermission();
            try
            {
                using (SqlConnection con = new SqlConnection(ConnectionString))
                {
                    con.Open();

                    using (SqlCommand command = new SqlCommand("usp_GetAccessRights", con))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@CreatedBy", username);
                        command.Parameters.AddWithValue("@RoleId", roleid);
                        SqlDataReader reader = command.ExecuteReader();
                        List<AccessRights> list = new List<AccessRights>();
                        while (reader.Read())
                        {
                            if (string.IsNullOrEmpty(ret.Username)) ret.Username = reader.IsDBNull(reader.GetOrdinal("UserName")) ? String.Empty : reader["UserName"].ToString();
                            if (string.IsNullOrEmpty(ret.UserRole)) ret.UserRole = reader.IsDBNull(reader.GetOrdinal("RoleName")) ? String.Empty : reader["RoleName"].ToString();
                            AccessRights ari = new AccessRights();
                            ari.Category = reader.IsDBNull(reader.GetOrdinal("Category")) ? String.Empty : reader["Category"].ToString();
                            ari.AccessId = (!DBNull.Value.Equals(reader["AccessId"]) ? (Convert.ToInt32(reader["AccessId"])) : 0);
                            ari.AccessName = reader.IsDBNull(reader.GetOrdinal("Access")) ? String.Empty : reader["Access"].ToString();
                            ari.AccessRight = reader.IsDBNull(reader.GetOrdinal("AccessRight")) ? String.Empty : reader["AccessRight"].ToString();
                            list.Add(ari);
                        }
                        if (list.Count > 0)
                        {
                            ret.UserAccessRights = list;
                        }
                        return ret;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public bool CheckPatientActive(string ConnectionString, string UserName)
        {

            bool ret = true;
            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    SqlCommand command = new SqlCommand("usp_GetCheckPatient", connection);
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@UserName", UserName);
                    SqlParameter returnParameter = command.Parameters.Add("RetVal", SqlDbType.Int);
                    returnParameter.Direction = ParameterDirection.ReturnValue;
                    connection.Open();
                    command.ExecuteNonQuery();
                    var id = returnParameter.Value;
                    if (id.Equals(0))
                    {
                        ret = false;
                    }
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return ret;
        }

    }
}
