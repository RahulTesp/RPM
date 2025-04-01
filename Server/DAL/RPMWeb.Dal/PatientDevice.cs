using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RPMWeb.Data.Common;
using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;

namespace RPMWeb.Dal
{
    public class PatientDevice
    {
        public List<WorldTimeZone> GetWorldTimeZone(string ConnectionString)
        {
            List<WorldTimeZone> info = new List<WorldTimeZone>();
            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    string query = "select * from sys.time_zone_info";
                    SqlCommand command = new SqlCommand(query, connection);
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        WorldTimeZone wtz = new WorldTimeZone();
                        //info.Id = !reader.IsDBNull(reader.GetOrdinal("")) ? (int)reader[""] : 0;
                        wtz.Name = !reader.IsDBNull(reader.GetOrdinal("name")) ? reader["name"].ToString() : string.Empty;
                        wtz.CurrentUtcOffset = !reader.IsDBNull(reader.GetOrdinal("current_utc_offset")) ? reader["current_utc_offset"].ToString() : string.Empty;
                        wtz.IsCurrentlyDst = !reader.IsDBNull(reader.GetOrdinal("is_currently_dst")) ? (bool)reader["is_currently_dst"] : false;
                        info.Add(wtz);
                    }
                }
            }
            catch
            {
                throw;
            }
            return info;

        }
        public RegisterDeviceResponse RegisterPatientDevice(DeviceRegister dr, string ConnectionString)
        {
            RegisterDeviceResponse response = new RegisterDeviceResponse();
            response.HttpRetCode = 400;
            try
            {
                using (SqlConnection connm = new SqlConnection(ConnectionString))
                {
                    SqlCommand commandm = new SqlCommand("[usp_VerifyDeviceStatus]", connm);
                    commandm.CommandType = CommandType.StoredProcedure;
                    commandm.Parameters.Add("@DeviceId", SqlDbType.NVarChar).Value = dr.DeviceId;
                    commandm.Parameters.Add("@Createdby", SqlDbType.NVarChar).Value = "sa";
                    connm.Open();
                    SqlParameter returnParameterm = commandm.Parameters.Add("RetVal", SqlDbType.Bit);
                    returnParameterm.Direction = ParameterDirection.ReturnValue;
                    commandm.ExecuteNonQuery();
                    int idm = (int)returnParameterm.Value;
                    connm.Close();
                    if (idm == 1)
                    {
                        List<SystemConfigInfo> igc = DalCommon.GetSystemConfig(ConnectionString, "iGlucose", "User");
                        if (igc == null)
                        {
                            response.Message = "Invalid Configuration.";
                            return response;
                        }
                        SystemConfigInfo igcenable = igc.Find(x => x.Name.Equals("Enable"));
                        if (igcenable == null || string.IsNullOrEmpty(igcenable.Value) || igcenable.Value.Equals("0"))
                        {
                            using (SqlConnection con = new SqlConnection(ConnectionString))
                            {
                                SqlCommand cmd = new SqlCommand("[usp_UpdDeviceConnectionStatus]", con);
                                cmd.CommandType = CommandType.StoredProcedure;
                                cmd.Parameters.Add("@PatientId", SqlDbType.Int).Value = dr.PatientId;
                                cmd.Parameters.Add("@DeviceId", SqlDbType.NVarChar).Value = dr.DeviceId;
                                cmd.Parameters.Add("@ConnectionStatus", SqlDbType.NVarChar).Value = "Active";
                                cmd.Parameters.Add("@ModifiedBy", SqlDbType.NVarChar).Value = "sa";
                                con.Open();
                                cmd.ExecuteNonQuery();
                                con.Close();
                            }
                            response.HttpRetCode = 200;//added
                            response.DevicUserId = dr.PatientNumber;//added
                            response.Message = "Device set to active without vendor activation.";//added
                            return response;
                        }

                        using (SqlConnection conn = new SqlConnection(ConnectionString))
                        {
                            SqlCommand command = new SqlCommand("[usp_VerifyPatientActiveDevice]", conn);
                            command.CommandType = CommandType.StoredProcedure;
                            command.Parameters.Add("@PatientId", SqlDbType.Int).Value = dr.PatientId;
                            command.Parameters.Add("@DeviceId", SqlDbType.NVarChar).Value = dr.DeviceId;
                            command.Parameters.Add("@Createdby", SqlDbType.NVarChar).Value = "sa";
                            conn.Open();
                            SqlParameter returnParameter = command.Parameters.Add("RetVal", SqlDbType.Bit);
                            returnParameter.Direction = ParameterDirection.ReturnValue;
                            command.ExecuteNonQuery();
                            int id = (int)returnParameter.Value;
                            conn.Close();
                            if (id == 1)
                            {

                                PatientDeviceConnectivityInfo pdci = null;
                                using (SqlConnection connection = new SqlConnection(ConnectionString))
                                {
                                    SqlCommand comm = new SqlCommand("usp_GetPatientVendorConnectivity", connection);
                                    comm.CommandType = CommandType.StoredProcedure;
                                    comm.Parameters.Add("@PatientId", SqlDbType.Int).Value = dr.PatientId;
                                    comm.Parameters.Add("@VendorId", SqlDbType.Int).Value = dr.VendorId;
                                    comm.Parameters.Add("@CreatedBy", SqlDbType.NVarChar).Value = "User";
                                    connection.Open();
                                    SqlDataReader reader = comm.ExecuteReader();
                                    while (reader.Read())
                                    {
                                        if (pdci == null) pdci = new PatientDeviceConnectivityInfo();
                                        pdci.DeviceVendorUserid = reader.IsDBNull(reader.GetOrdinal("DeviceVendorUserId")) ? string.Empty : reader["DeviceVendorUserId"].ToString();
                                        pdci.Password = reader.IsDBNull(reader.GetOrdinal("Password")) ? string.Empty : reader["Password"].ToString();
                                        pdci.PatientNumber = reader.IsDBNull(reader.GetOrdinal("PatientNumber")) ? string.Empty : reader["PatientNumber"].ToString();
                                        pdci.FirstName = reader.IsDBNull(reader.GetOrdinal("FirstName")) ? string.Empty : reader["FirstName"].ToString();
                                        pdci.LastName = reader.IsDBNull(reader.GetOrdinal("LastName")) ? string.Empty : reader["LastName"].ToString();
                                        pdci.VendorName = reader.IsDBNull(reader.GetOrdinal("Name")) ? string.Empty : reader["Name"].ToString();
                                    }
                                    connection.Close();
                                }

                                //Get Device Serial number
                                string DeviceSlNo = string.Empty;
                                using (SqlConnection connection = new SqlConnection(ConnectionString))
                                {
                                    SqlCommand comm = new SqlCommand("usp_GetDeviceSerialNumber", connection);
                                    comm.CommandType = CommandType.StoredProcedure;
                                    comm.Parameters.Add("@DeviceNo", SqlDbType.NVarChar).Value = dr.DeviceId;
                                    comm.Parameters.Add("@CreatedBy", SqlDbType.NVarChar).Value = "User";
                                    connection.Open();
                                    SqlDataReader reader = comm.ExecuteReader();
                                    while (reader.Read())
                                    {
                                        DeviceSlNo = reader.IsDBNull(reader.GetOrdinal("DeviceSerialNo")) ? string.Empty : reader["DeviceSerialNo"].ToString();
                                    }
                                    connection.Close();
                                }
                                if (string.IsNullOrEmpty(DeviceSlNo))
                                {
                                    response.Message = "Invalid device id.";
                                    return response;
                                }

                                RpmWebHandler webhandler = new RpmWebHandler();
                                if (pdci == null)
                                {
                                    DeviceRegisterRequest req = new DeviceRegisterRequest();
                                    req.device_id = DeviceSlNo;
                                    req.username = dr.PatientNumber;
                                    req.first_name = dr.FirstName;
                                    req.last_name = dr.LastName;
                                    req.time_zone = dr.TimeZoneOffset;
                                    int nDevUserId = 0;
                                    HttpResponse resp = webhandler.SendRegisterRequest(req, igc, ref nDevUserId);
                                    if (resp != null && resp.HttpRetCode == 201)
                                    {
                                        using (SqlConnection connection = new SqlConnection(ConnectionString))
                                        {
                                            SqlCommand comm = new SqlCommand("[usp_InsPatientVendorConnectivity]", connection);
                                            comm.CommandType = CommandType.StoredProcedure;
                                            comm.Parameters.Add("@PatientId", SqlDbType.Int).Value = dr.PatientId;
                                            comm.Parameters.Add("@VendorId", SqlDbType.Int).Value = dr.VendorId;
                                            comm.Parameters.Add("@DeviceUserId", SqlDbType.NVarChar).Value = nDevUserId.ToString();
                                            //comm.Parameters.Add("@DeviceUserId", SqlDbType.NVarChar).Value = dr.PatientNumber;//ADDED
                                            comm.Parameters.Add("@Createdby", SqlDbType.NVarChar).Value = "sa";
                                            connection.Open();
                                            SqlParameter returnParam = comm.Parameters.Add("RetVal", SqlDbType.Int);
                                            returnParam.Direction = ParameterDirection.ReturnValue;
                                            comm.ExecuteNonQuery();
                                            int id1 = (int)returnParam.Value;
                                            if (id1 > 0)
                                            {
                                                response.HttpRetCode = resp.HttpRetCode;
                                                //response.HttpRetCode = 200;
                                                response.DevicUserId = nDevUserId.ToString();
                                                //response.DevicUserId = dr.PatientNumber;//ADDED
                                                response.Message = "Patient device successfully registered in vendor and RPM";
                                            }
                                            else
                                            {
                                                response.HttpRetCode = 500;
                                                response.Message = "Patient device successfully registered to vendor, not in RPM";
                                            }
                                            connection.Close();
                                        }
                                    }
                                    else
                                    {
                                        response.HttpRetCode = resp.HttpRetCode;
                                        response.Message = resp.Message;
                                    }
                                }
                                else
                                {
                                    response = webhandler.AddDeviceRequest(DeviceSlNo, pdci.DeviceVendorUserid, dr.DeviceModel, igc);
                                }
                                // Set patient Device status
                                string devStatus = ((response.HttpRetCode == 200) || (response.HttpRetCode == 201)) ? "Active" : "Error";
                                //string devStatus = "Active";
                                if (devStatus.Equals("Active"))
                                {
                                    using (SqlConnection con = new SqlConnection(ConnectionString))
                                    {
                                        SqlCommand cmd = new SqlCommand("[usp_UpdDeviceConnectionStatus]", con);
                                        cmd.CommandType = CommandType.StoredProcedure;
                                        cmd.Parameters.Add("@PatientId", SqlDbType.Int).Value = dr.PatientId;
                                        cmd.Parameters.Add("@DeviceId", SqlDbType.NVarChar).Value = dr.DeviceId;
                                        cmd.Parameters.Add("@ConnectionStatus", SqlDbType.NVarChar).Value = devStatus;
                                        cmd.Parameters.Add("@ModifiedBy", SqlDbType.NVarChar).Value = "sa";
                                        con.Open();
                                        cmd.ExecuteNonQuery();
                                        con.Close();
                                        response.HttpRetCode = response.HttpRetCode;//added
                                        response.DevicUserId = dr.PatientNumber;//added
                                        response.Message = response.Message;//added
                                    }
                                }
                                else
                                {
                                    response.HttpRetCode = response.HttpRetCode;//added
                                    response.DevicUserId = dr.PatientNumber;//added
                                    response.Message = response.Message;//added
                                }

                            }
                            else
                            {
                                response.Message = "Patient already have an active device for the vital";
                            }
                        }
                    }
                    else
                    {
                        response.Message = "Device is not Available";
                    }
                }
                return response;
            }
            catch
            {
                throw;
            }
        }
        public bool ValidateDevice(string deviceid, string ConnectionString)
        {
            try
            {
                List<SystemConfigInfo> igc = DalCommon.GetSystemConfig(ConnectionString, "iGlucose", "User");
                if (igc == null) return false;

                SystemConfigInfo igcenable = igc.Find(x => x.Name.Equals("Enable"));
                if (igcenable == null || string.IsNullOrEmpty(igcenable.Value) || igcenable.Value.Equals("0"))
                {
                    return false;
                }

                RpmWebHandler webhandler = new RpmWebHandler();
                return webhandler.ValidateDeviceRequest(deviceid, igc);
            }
            catch
            {
                throw;
            }
        }
        public RegisterDeviceResponse RemovePatientDevice(DeviceRegister dr, string ConnectionString)
        {
            RegisterDeviceResponse response = new RegisterDeviceResponse();
            response.HttpRetCode = 400;
            try
            {
                List<SystemConfigInfo> igc = DalCommon.GetSystemConfig(ConnectionString, "iGlucose", "User");
                if (igc == null)
                {
                    response.Message = "Invalid Configuration.";
                    return response;
                }
                //Get Device Serial number
                string DeviceSlNo = string.Empty;
                using (SqlConnection connection = new SqlConnection(ConnectionString))

                {
                    SqlCommand comm = new SqlCommand("usp_GetDeviceSerialNumber", connection);
                    comm.CommandType = CommandType.StoredProcedure;
                    comm.Parameters.Add("@DeviceNo", SqlDbType.NVarChar).Value = dr.DeviceId;
                    comm.Parameters.Add("@CreatedBy", SqlDbType.NVarChar).Value = "User";
                    connection.Open();
                    SqlDataReader reader = comm.ExecuteReader();
                    while (reader.Read())
                    {
                        DeviceSlNo = reader.IsDBNull(reader.GetOrdinal("DeviceSerialNo")) ? string.Empty : reader["DeviceSerialNo"].ToString();
                    }
                    connection.Close();
                }
                if (string.IsNullOrEmpty(DeviceSlNo))
                {
                    response.Message = "Invalid device id.";
                    return response;
                }

                SystemConfigInfo igcenable = igc.Find(x => x.Name.Equals("Enable"));
                if (igcenable == null || string.IsNullOrEmpty(igcenable.Value) || igcenable.Value.Equals("0"))
                {
                    //commented below code to ignore device removal from RPM database, device will be removde from Patient only from vendor portal
                    /*using (SqlConnection con = new SqlConnection(ConnectionString))
                    {
                        SqlCommand cmd = new SqlCommand("[usp_UdpDeviceRemoval]", con);
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@DeviceSerialNo", SqlDbType.NVarChar).Value = dr.DeviceId;
                        cmd.Parameters.Add("@ModifiedBy", SqlDbType.NVarChar).Value = "sa";
                        con.Open();
                        cmd.ExecuteNonQuery();
                        con.Close();
                    }*/
                    response.HttpRetCode = 200;
                    response.Message = "Sucess";
                    return response;
                }

                PatientDeviceConnectivityInfo pdci = null;
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    SqlCommand command = new SqlCommand("usp_GetPatientVendorConnectivity", connection);
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add("@PatientId", SqlDbType.Int).Value = dr.PatientId;
                    command.Parameters.Add("@VendorId", SqlDbType.Int).Value = dr.VendorId;
                    command.Parameters.Add("@CreatedBy", SqlDbType.NVarChar).Value = "User";
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        if (pdci == null) pdci = new PatientDeviceConnectivityInfo();
                        pdci.DeviceVendorUserid = reader.IsDBNull(reader.GetOrdinal("DeviceVendorUserId")) ? string.Empty : reader["DeviceVendorUserId"].ToString();
                        pdci.Password = reader.IsDBNull(reader.GetOrdinal("Password")) ? string.Empty : reader["Password"].ToString();
                        pdci.PatientNumber = reader.IsDBNull(reader.GetOrdinal("PatientNumber")) ? string.Empty : reader["PatientNumber"].ToString();
                        pdci.FirstName = reader.IsDBNull(reader.GetOrdinal("FirstName")) ? string.Empty : reader["FirstName"].ToString();
                        pdci.LastName = reader.IsDBNull(reader.GetOrdinal("LastName")) ? string.Empty : reader["LastName"].ToString();
                        pdci.VendorName = reader.IsDBNull(reader.GetOrdinal("Name")) ? string.Empty : reader["Name"].ToString();
                    }
                    connection.Close();
                }
                if (pdci == null)
                {
                    response.Message = "Device not configured for this user.";
                    return response;
                }
               
                RpmWebHandler webhandler = new RpmWebHandler();
                response = webhandler.RemoveDeviceRequest(DeviceSlNo, pdci.DeviceVendorUserid, dr.DeviceModel, igc);
                //commented below code to ignore device removal from RPM database, device will be removde from Patient only from vendor portal
                /*if (response.HttpRetCode == 200)
                {
                    using (SqlConnection con = new SqlConnection(ConnectionString))
                    {
                        SqlCommand cmd = new SqlCommand("[usp_UdpDeviceRemoval]", con);
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@DeviceSerialNo", SqlDbType.NVarChar).Value = dr.DeviceId;
                        cmd.Parameters.Add("@ModifiedBy", SqlDbType.NVarChar).Value = "sa";
                        con.Open();
                        cmd.ExecuteNonQuery();
                        con.Close();
                    }
                }*/ 
                return response;

            }
            catch
            {
                throw;
            }

        }
        public RegisterDeviceResponse ResetPatientDevice(DeviceRegister dr, string ConnectionString)
        {
            RegisterDeviceResponse response = new RegisterDeviceResponse();
            response.HttpRetCode = 400;
            try
            {
                List<SystemConfigInfo> igc = DalCommon.GetSystemConfig(ConnectionString, "iGlucose", "User");
                if (igc == null)
                {
                    response.Message = "Invalid Configuration.";
                    return response;
                }
                //Get Device Serial number
                string DeviceSlNo = string.Empty;
                using (SqlConnection connection = new SqlConnection(ConnectionString))

                {
                    SqlCommand comm = new SqlCommand("usp_GetDeviceSerialNumber", connection);
                    comm.CommandType = CommandType.StoredProcedure;
                    comm.Parameters.Add("@DeviceNo", SqlDbType.NVarChar).Value = dr.DeviceId;
                    comm.Parameters.Add("@CreatedBy", SqlDbType.NVarChar).Value = "User";
                    connection.Open();
                    SqlDataReader reader = comm.ExecuteReader();
                    while (reader.Read())
                    {
                        DeviceSlNo = reader.IsDBNull(reader.GetOrdinal("DeviceSerialNo")) ? string.Empty : reader["DeviceSerialNo"].ToString();
                    }
                    connection.Close();
                }
                if (string.IsNullOrEmpty(DeviceSlNo))
                {
                    response.Message = "Invalid device id.";
                    return response;
                }
                SystemConfigInfo igcenable = igc.Find(x => x.Name.Equals("Enable"));
                if (igcenable == null || string.IsNullOrEmpty(igcenable.Value) || igcenable.Value.Equals("0"))
                {
                    //commented below code to ignore device removal from RPM database, device will be removde from Patient only from vendor portal
                    using (SqlConnection con = new SqlConnection(ConnectionString))
                    {
                        SqlCommand cmd = new SqlCommand("[usp_UdpDeviceRemoval]", con);
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@DeviceSerialNo", SqlDbType.NVarChar).Value = dr.DeviceId;
                        cmd.Parameters.Add("@ModifiedBy", SqlDbType.NVarChar).Value = "sa";
                        con.Open();
                        cmd.ExecuteNonQuery();
                        con.Close();
                    }
                    response.HttpRetCode = 200;
                    response.Message = "Sucess";
                    return response;
                }

                PatientDeviceConnectivityInfo pdci = null;
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    SqlCommand command = new SqlCommand("usp_GetPatientVendorConnectivity", connection);
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add("@PatientId", SqlDbType.Int).Value = dr.PatientId;
                    command.Parameters.Add("@VendorId", SqlDbType.Int).Value = dr.VendorId;
                    command.Parameters.Add("@CreatedBy", SqlDbType.NVarChar).Value = "User";
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        if (pdci == null) pdci = new PatientDeviceConnectivityInfo();
                        pdci.DeviceVendorUserid = reader.IsDBNull(reader.GetOrdinal("DeviceVendorUserId")) ? string.Empty : reader["DeviceVendorUserId"].ToString();
                        pdci.Password = reader.IsDBNull(reader.GetOrdinal("Password")) ? string.Empty : reader["Password"].ToString();
                        pdci.PatientNumber = reader.IsDBNull(reader.GetOrdinal("PatientNumber")) ? string.Empty : reader["PatientNumber"].ToString();
                        pdci.FirstName = reader.IsDBNull(reader.GetOrdinal("FirstName")) ? string.Empty : reader["FirstName"].ToString();
                        pdci.LastName = reader.IsDBNull(reader.GetOrdinal("LastName")) ? string.Empty : reader["LastName"].ToString();
                        pdci.VendorName = reader.IsDBNull(reader.GetOrdinal("Name")) ? string.Empty : reader["Name"].ToString();
                    }
                    connection.Close();
                }
                if (pdci == null)
                {
                    response.Message = "Device not configured for this user.";
                    return response;
                }
                RpmWebHandler webhandler = new RpmWebHandler();
                response = webhandler.RemoveDeviceRequest(DeviceSlNo, pdci.DeviceVendorUserid, dr.DeviceModel, igc);
                //commented below code to ignore device removal from RPM database, device will be removde from Patient only from vendor portal
                if (response.HttpRetCode == 200)
                {
                    using (SqlConnection con = new SqlConnection(ConnectionString))
                    {
                        SqlCommand cmd = new SqlCommand("[usp_UdpDeviceRemoval]", con);
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@DeviceSerialNo", SqlDbType.NVarChar).Value = dr.DeviceId;
                        cmd.Parameters.Add("@ModifiedBy", SqlDbType.NVarChar).Value = "sa";
                        con.Open();
                        cmd.ExecuteNonQuery();
                        con.Close();
                    }
                }
                return response;

            }
            catch
            {
                throw;
            }

        }
        public string IsValidlifesenseDevice(string DeviceId,string ConnectionString)
        {
            
            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    SqlCommand command = new SqlCommand("usp_GetDeviceManufacturer", connection);
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@DeviceId", DeviceId);
                    SqlParameter returnParameter = command.Parameters.Add("RetVal", SqlDbType.Int);
                    returnParameter.Direction = ParameterDirection.ReturnValue;
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    string Manufacturer="";
                    string DeviceType = "";
                    while (reader.Read())
                    {
                        Manufacturer = reader["Manufacturer"].ToString();
                        DeviceType = reader["DeviceType"].ToString();
                    }
                    connection.Close();
                    if (Manufacturer== "lifesense")
                    {
                        return DeviceType;
                    }
                    return null;
                    
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }            
        }
        public bool UpdateDeviceStatus(UpdateDeviceStatus status, string ConnectionString)
        {
            bool ret = true;
            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    SqlCommand command = new SqlCommand("usp_UdpDeviceStatus", connection);
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@DeviceStatus", status.DeviceStatus);
                    command.Parameters.AddWithValue("@DeviceSerialNo", status.DeviceSerialNo);
                    command.Parameters.AddWithValue("@ModifiedBy", status.CreatedBy);                   
                    connection.Open();
                    SqlParameter returnParameter = command.Parameters.Add("RetVal", SqlDbType.Int);
                    returnParameter.Direction = ParameterDirection.ReturnValue;
                    command.ExecuteNonQuery();
                    connection.Close();
                    int id = (int)returnParameter.Value;
                    if (id.Equals(0))
                    {
                        ret = false;
                    }

                }
                return ret;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public bool IsDeviceAvailable(DeviceValidate info, string ConnectionString)
        {
            bool ret = true;
            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    SqlCommand command = new SqlCommand("usp_ValidateDevice", connection);
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@DeviceId", info.DeviceId);
                    command.Parameters.AddWithValue("@DeviceModel", info.DeviceModel);
                    command.Parameters.AddWithValue("@DeviceVendor", info.DeviceVendor);
                    command.Parameters.AddWithValue("@CreatedBy", info.CreatedBy);
                    SqlParameter returnParameter = command.Parameters.Add("RetVal", SqlDbType.Int);
                    returnParameter.Direction = ParameterDirection.ReturnValue;                 
                    connection.Open();
                    command.ExecuteNonQuery();
                    int id = (int)returnParameter.Value;
                    connection.Close();
                    if (id.Equals(0))
                    {
                        ret = false;
                    }
                    return ret;

                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public bool IsDeviceModelAvailable(DeviceValidate info, string ConnectionString)
        {
            bool ret = true;
            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    SqlCommand command = new SqlCommand("usp_InsDeviceModelIfNotExist", connection);
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@DeviceModel", info.DeviceModel);
                    command.Parameters.AddWithValue("@CreatedBy", info.CreatedBy);
                    SqlParameter returnParameter = command.Parameters.Add("RetVal", SqlDbType.Int);
                    returnParameter.Direction = ParameterDirection.ReturnValue;
                    connection.Open();
                    command.ExecuteNonQuery();
                    int id = (int)returnParameter.Value;
                    connection.Close();
                    if (id.Equals(0))
                    {
                        ret = false;
                    }
                    return ret;

                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public int AddDevice(AddDevice info, string ConnectionString)
        {
            int ret = 0;
            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    SqlCommand command = new SqlCommand("usp_InsDevice", connection);
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@Name", info.Name);
                    command.Parameters.AddWithValue("@PatientId", info.PatientId);
                    command.Parameters.AddWithValue("@DeviceType", info.DeviceType);
                    command.Parameters.AddWithValue("@DeviceModel", info.DeviceModel);
                    command.Parameters.AddWithValue("@DeviceVendor", info.DeviceVendor);
                    command.Parameters.AddWithValue("@DeviceManufacturer", info.DeviceManufacturer);
                    command.Parameters.AddWithValue("@DeviceNumber", info.DeviceNumber);
                    command.Parameters.AddWithValue("@DeviceSerialNo", info.DeviceSerialNo);
                    command.Parameters.AddWithValue("@DeviceIMEINo", info.DeviceIMEINo);
                    command.Parameters.AddWithValue("@DeviceStatus", info.DeviceStatus);
                    command.Parameters.AddWithValue("@DeviceCommunicationType", info.DeviceCommunicationType);
                    command.Parameters.AddWithValue("@DeviceActivatedDateTime", info.DeviceActivatedDateTime);
                    command.Parameters.AddWithValue("@City", info.City);
                    command.Parameters.AddWithValue("@PurchaseDate", info.PurchaseDate);
                    command.Parameters.AddWithValue("@ManufactureDate", info.ManufactureDate);
                    command.Parameters.AddWithValue("@WarrantyPeriod", info.WarrantyPeriod);
                    command.Parameters.AddWithValue("@Lifetime", info.Lifetime);
                    command.Parameters.AddWithValue("@CreatedBy", info.CreatedBy);
                    SqlParameter returnParameter = command.Parameters.Add("RetVal", SqlDbType.Int);
                    returnParameter.Direction = ParameterDirection.ReturnValue;
                    connection.Open();
                    command.ExecuteNonQuery();
                    int id = (int)returnParameter.Value;
                    connection.Close();
                    if (id>0)
                    {
                        ret = id;
                    }
                    return ret;

                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public bool bodytracedataInsert(bodytracedata data, string ConnectionString)
        {
            bool ret = true;
            using (SqlConnection con = new SqlConnection(ConnectionString))
            {
                SqlCommand cmd = new SqlCommand("[usp_InsBodyTraceTable]", con);
                cmd.CommandType = CommandType.StoredProcedure;
                string output = JsonConvert.SerializeObject(data);              
                cmd.Parameters.Add("@Data", SqlDbType.NVarChar).Value = output;                
                SqlParameter returnParameter = cmd.Parameters.Add("RetVal", SqlDbType.Int);
                returnParameter.Direction = ParameterDirection.ReturnValue;
                con.Open();
                cmd.ExecuteNonQuery();
                con.Close();
                int id = (int)returnParameter.Value;
                if (id.Equals(0))
                {
                    ret = false;
                }
            }
            
            return ret;
        }
        public bool StagingTableInsert(DeviceTelemetry dev, string DeviceType,string ConnectionString)
        {
            bool ret = true;
            JObject obj = JObject.Parse(dev.data.ToString());
            if (DeviceType == "Blood Pressure Monitor")
            {
                StagingInput blood_pressuresystolic = new StagingInput();
                StagingInput blood_pressurediastolic = new StagingInput();
                StagingInput blood_pressurepulse = new StagingInput();
                blood_pressuresystolic.reading_id = obj["ts"].ToString();
                blood_pressurediastolic.reading_id = obj["ts"].ToString();
                blood_pressurepulse.reading_id = obj["ts"].ToString();
                blood_pressuresystolic.device_id = dev.deviceId;
                blood_pressurediastolic.device_id = dev.deviceId;
                blood_pressurepulse.device_id = dev.deviceId;
                blood_pressuresystolic.device_model = "";
                blood_pressurediastolic.device_model = "";
                blood_pressurepulse.device_model = "";
                var dateTime = DateTimeOffset.FromUnixTimeSeconds(Convert.ToInt64(obj["ts"])).DateTime;
                blood_pressuresystolic.date_recorded = dateTime.ToString("yyyy-MM-dd HH:mm:ss");
                blood_pressurediastolic.date_recorded = dateTime.ToString("yyyy-MM-dd HH:mm:ss");
                blood_pressurepulse.date_recorded = dateTime.ToString("yyyy-MM-dd HH:mm:ss");
                var dateTimeRx = DateTimeOffset.FromUnixTimeSeconds(dev.createdAt).DateTime;
                blood_pressuresystolic.date_received = dateTimeRx.ToString("yyyy-MM-dd HH:mm:ss");
                blood_pressurediastolic.date_received = dateTimeRx.ToString("yyyy-MM-dd HH:mm:ss");
                blood_pressurepulse.date_received = dateTimeRx.ToString("yyyy-MM-dd HH:mm:ss");
                blood_pressuresystolic.reading_type = "blood_pressure";
                blood_pressurediastolic.reading_type = "blood_pressure";
                blood_pressurepulse.reading_type = "blood_pressure";
                blood_pressuresystolic.battery = 0;
                blood_pressurediastolic.battery = 0;
                blood_pressurepulse.battery = 0;
                blood_pressuresystolic.time_zone_offset = "";
                blood_pressurediastolic.time_zone_offset = "";
                blood_pressurepulse.time_zone_offset = "";
                blood_pressuresystolic.before_meal = false;
                blood_pressurediastolic.before_meal = false;
                blood_pressurepulse.before_meal = false;
                blood_pressuresystolic.event_flag  = "";
                blood_pressurediastolic.event_flag = "";
                blood_pressurepulse.event_flag = "";
                blood_pressuresystolic.irregular = false;
                blood_pressurediastolic.irregular = false;
                blood_pressurepulse.irregular = false;
                blood_pressuresystolic.data_type = "systolic";
                blood_pressuresystolic.data_unit = "mmHg";
                blood_pressuresystolic.data_value = Convert.ToDouble(obj["sys"]);
                blood_pressurediastolic.data_type = "diastolic";
                blood_pressurediastolic.data_unit = "mmHg";
                blood_pressurediastolic.data_value = Convert.ToDouble(obj["dia"]);
                Console.WriteLine("Added to queue:" + blood_pressurediastolic.device_id);
                blood_pressurepulse.data_type = "pulse";
                blood_pressurepulse.data_unit = "bps";
                blood_pressurepulse.data_value = Convert.ToDouble(obj["pul"]);
                string Json = "Insert into JsonStg1(Json)values(" + blood_pressuresystolic + "," + blood_pressurediastolic + "," + blood_pressurepulse + ")";
                using (SqlConnection con = new SqlConnection(ConnectionString))
                {
                    SqlCommand cmd = new SqlCommand("[usp_InsJsonStg1]", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@Json", SqlDbType.NVarChar).Value = Json;
                    cmd.Parameters.Add("@CreatedBy", SqlDbType.NVarChar).Value = "sa";
                    SqlParameter returnParameter = cmd.Parameters.Add("RetVal", SqlDbType.Int);
                    returnParameter.Direction = ParameterDirection.ReturnValue;
                    con.Open();
                    cmd.ExecuteNonQuery();
                    con.Close();
                    int id = (int)returnParameter.Value;
                    if (id.Equals(0))
                    {
                        ret = false;
                    }
                }
            }
            else if (DeviceType == "Body Weight Monitor")
            {
                StagingInput weight = new StagingInput();
                weight.reading_id = obj["ts"].ToString();
                weight.device_id = dev.deviceId;
                weight.device_model = "";
                var dateTime = DateTimeOffset.FromUnixTimeSeconds(Convert.ToInt64(obj["ts"])).DateTime;
                weight.date_recorded = dateTime.ToString("yyyy-MM-dd HH:mm:ss");
                var dateTimeRx = DateTimeOffset.FromUnixTimeSeconds(dev.createdAt).DateTime;
                weight.date_received = dateTimeRx.ToString("yyyy-MM-dd HH:mm:ss");
                weight.reading_type = "Weight";
                weight.battery = 0;
                weight.time_zone_offset = "";
                weight.before_meal = false;
                weight.event_flag = "";
                weight.irregular = false;
                weight.data_type = "Weight";
                weight.data_unit = "grams";
                weight.data_value = Convert.ToDouble(obj["wt"]);
                //JavaScriptSerializer js = new JavaScriptSerializer();
                //string jsonData = js.Serialize(weight);
                string jsonData = JsonConvert.SerializeObject(weight);
                string Json = "Insert into JsonStg1(Json)values('" + jsonData + "')";
                using (SqlConnection con = new SqlConnection(ConnectionString))
                {
                    SqlCommand cmd = new SqlCommand("[usp_InsJsonStg1]", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@Json", SqlDbType.NVarChar).Value = Json;
                    cmd.Parameters.Add("@CreatedBy", SqlDbType.NVarChar).Value = "sa";
                    SqlParameter returnParameter = cmd.Parameters.Add("RetVal", SqlDbType.Int);
                    returnParameter.Direction = ParameterDirection.ReturnValue;
                    con.Open();
                    cmd.ExecuteNonQuery();
                    con.Close();
                    int id = (int)returnParameter.Value;
                    if (id.Equals(0))
                    {
                        ret = false;
                    }
                }
            }
            return ret;
        }
        public bool InsertPatienVendorConnectivity(AddPatientVendorConn data, string ConnectionString)
        {
            bool ret = true;
            using (SqlConnection con = new SqlConnection(ConnectionString))
            {
                SqlCommand cmd = new SqlCommand("[usp_InsPatientVendorConnectivity_dev]", con);
                cmd.CommandType = CommandType.StoredProcedure;                
                cmd.Parameters.Add("@DeviceId", SqlDbType.NVarChar).Value = data.DeviceId;
                cmd.Parameters.Add("@PatientVendorUserId", SqlDbType.NVarChar).Value = data.PatientVendorUserId;
                cmd.Parameters.Add("@VendorName", SqlDbType.NVarChar).Value = data.VendorName;
                cmd.Parameters.Add("@CreatedBy", SqlDbType.NVarChar).Value = data.CreatedBy;
                SqlParameter returnParameter = cmd.Parameters.Add("RetVal", SqlDbType.Int);
                returnParameter.Direction = ParameterDirection.ReturnValue;
                con.Open();
                cmd.ExecuteNonQuery();
                con.Close();
                int id = (int)returnParameter.Value;
                if (id.Equals(0))
                {
                    ret = false;
                }
            }

            return ret;
        }
    }
}
