using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.InkML;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RPMWeb.Data.Common;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Globalization;

namespace RPMWeb.Dal
{
    public class PatientDevice
    {
        static ConcurrentDictionary<string, string> vitalunits_dictionary = new ConcurrentDictionary<string, string>();
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
        public void GetVitalUnits(string ConnectionString)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(ConnectionString))
                {
                    con.Open();
                    SqlCommand command1 = new SqlCommand("usp_GetVitalUnits", con);
                    command1.CommandType = System.Data.CommandType.StoredProcedure;
                    using (SqlDataReader reader = command1.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var MeasureName = reader["MeasureName"].ToString();
                            var Unit = reader["UnitName"].ToString();
                            vitalunits_dictionary.TryAdd(MeasureName, Unit);
                        }
                    }
                    con.Close();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
        public int GetTimeZoneOffset(string tz)
        {
            string tzString = tz.Trim().ToLower();
            int timeZoneOffset = 0;

            if (tzString.StartsWith("utc"))
            {
                tzString = tzString.Substring(3);
            }

            if (int.TryParse(tzString, out timeZoneOffset))
            {
                return timeZoneOffset;
            }
            return 0;
        }
        public DateTime ConvertUnixTimestampToDateTime(long timestamp)
        {
            DateTime dateTimeRx;
            if (timestamp.ToString().Length == 10)
            {
                dateTimeRx = DateTimeOffset.FromUnixTimeSeconds(timestamp).DateTime;
            }
            else if (timestamp.ToString().Length == 13)
            {
                dateTimeRx = DateTimeOffset.FromUnixTimeSeconds(timestamp / 1000).DateTime;
            }
            else
            {
                throw new ArgumentException("Invalid timestamp length.");
            }

            return dateTimeRx;
        }
        public string GetCurrentTranstekReadingId(string connectionString)
        {
            string newReadingId = string.Empty;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("usp_GetTranstekReadingId", connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    connection.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            newReadingId = reader.GetString(0);  // Assuming the result is in the first column
                        }
                    }
                }
            }
            return newReadingId;
        }
        public void UpdateTranstekReadingId(string connectionString, long newReadingId)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("usp_UpdTranstekReadingId", connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@NewReadingId", newReadingId);
                    connection.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }
        public bool StagingTableInsert(TranstekDeviceTelemetry dev, string DeviceType, string ConnectionString)
        {
            GetVitalUnits(ConnectionString);
            bool ret = false;
            string stagingInsert = "INSERT INTO JsonStg([Json])VALUES";

            if (DeviceType == "Blood Pressure Monitor ")
            {
                ret=ProcessBloobPressureData(dev, ConnectionString, stagingInsert);
            }
            else if (DeviceType == "Blood Glucose Monitor")
            {
                ret=ProcessBloodGlucoseData(dev, ConnectionString, stagingInsert);
            }
            else if (DeviceType == "Body Weight Monitor")
            {
                ret=ProcessWeightData(dev, ConnectionString, stagingInsert);
            }
            else if (DeviceType == "Pulse Oximeter")
            {
                ret=ProcessOxygenData(dev, ConnectionString, stagingInsert);
            }
            return ret;
        }
        private bool ProcessBloobPressureData(TranstekDeviceTelemetry dev, string ConnectionString, string stagingInsert)
        {
            bool ret;
            try
            {
                vitalunits_dictionary.TryGetValue("Systolic", out string Unitsystolic);
                vitalunits_dictionary.TryGetValue("Diastolic", out string Unitdiastolic);
                vitalunits_dictionary.TryGetValue("Pulse", out string Unitpulse);
                JObject obj = JObject.Parse(dev.data.ToString());
                StagingInput blood_pressuresystolic = new StagingInput();
                StagingInput blood_pressurediastolic = new StagingInput();
                StagingInput blood_pressurepulse = new StagingInput();
                string readingIdWithPrefix = "TK" + obj["imei"] + obj["ts"];
                blood_pressuresystolic.reading_id = readingIdWithPrefix;
                blood_pressurediastolic.reading_id = readingIdWithPrefix;
                blood_pressurepulse.reading_id = readingIdWithPrefix;
                blood_pressuresystolic.device_id = dev.deviceId;
                blood_pressurediastolic.device_id = dev.deviceId;
                blood_pressurepulse.device_id = dev.deviceId;
                blood_pressuresystolic.device_model = dev.modelNumber;
                blood_pressurediastolic.device_model = dev.modelNumber;
                blood_pressurepulse.device_model = dev.modelNumber;
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
                blood_pressuresystolic.battery = (int)obj["bat"];
                blood_pressurediastolic.battery = (int)obj["bat"];
                blood_pressurepulse.battery = (int)obj["bat"];
                var time = obj["tz"];
                if (time != null)
                {
                    int timeZone = GetTimeZoneOffset(time.ToString());
                    blood_pressuresystolic.time_zone_offset = timeZone.ToString();
                    blood_pressurediastolic.time_zone_offset = timeZone.ToString();
                    blood_pressurepulse.time_zone_offset = timeZone.ToString();
                }
                else
                {
                    blood_pressuresystolic.time_zone_offset = null;
                    blood_pressurediastolic.time_zone_offset = null;
                    blood_pressurepulse.time_zone_offset = null;
                }
                blood_pressuresystolic.before_meal = false;
                blood_pressurediastolic.before_meal = false;
                blood_pressurepulse.before_meal = false;
                blood_pressuresystolic.event_flag = null;
                blood_pressurediastolic.event_flag = null;
                blood_pressurepulse.event_flag = null;
                blood_pressuresystolic.irregular = false;
                blood_pressurediastolic.irregular = false;
                blood_pressurepulse.irregular = false;
                blood_pressuresystolic.data_type = "systolic";
                blood_pressuresystolic.data_unit = Unitsystolic;
                blood_pressuresystolic.data_value = Convert.ToDouble(obj["sys"]);
                blood_pressurediastolic.data_type = "diastolic";
                blood_pressurediastolic.data_unit = Unitdiastolic;
                blood_pressurediastolic.data_value = Convert.ToDouble(obj["dia"]);
                blood_pressurepulse.data_type = "pulse";
                blood_pressurepulse.data_unit = Unitpulse;
                blood_pressurepulse.data_value = Convert.ToDouble(obj["pul"]);
                string jsonDataSys = JsonConvert.SerializeObject(blood_pressuresystolic);
                string jsonDataDia = JsonConvert.SerializeObject(blood_pressurediastolic);
                string jsonDataPul = JsonConvert.SerializeObject(blood_pressurepulse);
                string insertvalues = "('" + jsonDataSys + "'),('" + jsonDataDia + "'),('" + jsonDataPul + "')";
                StagingTableInsertJson(stagingInsert + insertvalues, ConnectionString);
                ret = true;
            }
            catch (Exception)
            {
                ret = false;
            }
            return ret;
        }
        private bool ProcessBloodGlucoseData(TranstekDeviceTelemetry dev, string ConnectionString, string stagingInsert)
        {
            bool ret;
            try
            {
                vitalunits_dictionary.TryGetValue("Fasting", out string Unitglucose);
                JObject obj = JObject.Parse(dev.data.ToString());
                StagingInput blood_glucose = new StagingInput();
                string readingIdWithPrefix = "TK" + obj["imei"] + obj["ts"];
                blood_glucose.reading_id = readingIdWithPrefix;
                blood_glucose.device_id = dev.deviceId;
                blood_glucose.device_model = dev.modelNumber;
                var dateTime = DateTimeOffset.FromUnixTimeSeconds(Convert.ToInt64(obj["ts"])).DateTime;
                blood_glucose.date_recorded = dateTime.ToString("yyyy-MM-dd HH:mm:ss");
                var dateTimeRx = DateTimeOffset.FromUnixTimeSeconds(dev.createdAt).DateTime;
                blood_glucose.date_received = dateTimeRx.ToString("yyyy-MM-dd HH:mm:ss");
                blood_glucose.reading_type = "blood_glucose";
                blood_glucose.battery = 0;
                var time = obj["ts_tz"];
                if (time != null)
                {
                    int timeZone = GetTimeZoneOffset(time.ToString());
                    blood_glucose.time_zone_offset = timeZone.ToString();
                }
                else
                {
                    blood_glucose.time_zone_offset = null;
                }
                blood_glucose.irregular = false;
                bool isUnitmmol = obj["unit"].ToString() == "1";
                bool isUnitmgdl = obj["unit"].ToString() == "2";
                blood_glucose.data_unit = Unitglucose;
                if (isUnitmmol)
                {
                    blood_glucose.data_value = Convert.ToDouble(obj["data"]) * 18;
                }
                else
                {
                    blood_glucose.data_value = Convert.ToDouble(obj["data"]);
                }
                bool isFasting = obj["meal"].ToString() == "1";
                bool isNonFasting = obj["meal"].ToString() == "2";
                if (isFasting)
                {
                    blood_glucose.data_type = "Fasting";
                    blood_glucose.before_meal = true;
                    blood_glucose.event_flag = "0";
                }

                if (isNonFasting)
                {
                    blood_glucose.data_type = "Non-Fasting";
                    blood_glucose.before_meal = false;
                    blood_glucose.event_flag = "1";
                }
                string jsonData = JsonConvert.SerializeObject(blood_glucose);
                StagingTableInsertJson(stagingInsert + "('" + jsonData + "')", ConnectionString);
                ret = true;
            }
            catch (Exception)
            {
                ret = false;
            }
            return ret;
        }
        private bool ProcessWeightData(TranstekDeviceTelemetry dev, string ConnectionString, string stagingInsert)
        {
            bool ret;
            try
            {
                vitalunits_dictionary.TryGetValue("Weight", out string Unitweight);
                JObject obj = JObject.Parse(dev.data.ToString());
                string readingIdWithPrefix = "TK" + obj["imei"] + obj["ts"];
                StagingInput weight = new StagingInput();
                weight.reading_id = readingIdWithPrefix;
                weight.device_id = dev.deviceId;
                weight.device_model = dev.modelNumber;
                var dateTime = DateTimeOffset.FromUnixTimeSeconds(Convert.ToInt64(obj["ts"])).DateTime;
                weight.date_recorded = dateTime.ToString("yyyy-MM-dd HH:mm:ss");
                var dateTimeRx = DateTimeOffset.FromUnixTimeSeconds(dev.createdAt).DateTime;
                weight.date_received = dateTimeRx.ToString("yyyy-MM-dd HH:mm:ss");
                weight.reading_type = "weight";
                weight.battery = (int)obj["bat"];
                var time = obj["tz"];
                if (time != null)
                {
                    int timeZone = GetTimeZoneOffset(time.ToString());
                    weight.time_zone_offset = timeZone.ToString();
                }
                else
                {
                    weight.time_zone_offset = null;
                }
                weight.before_meal = false;
                weight.event_flag = null;
                weight.irregular = false;
                weight.data_type = "weight";
                weight.data_unit = Unitweight;
                if (Unitweight == "kg")
                {
                    weight.data_value = Convert.ToDouble(obj["wt"]) / 1000;
                }
                else if (Unitweight == "lbs")
                {
                    weight.data_value = Convert.ToDouble(obj["wt"]) * 0.00220462;
                }
                string jsonData = JsonConvert.SerializeObject(weight);
                StagingTableInsertJson(stagingInsert + "('" + jsonData + "')", ConnectionString);
                ret = true;
            }
            catch (Exception)
            {
                ret = false;
            }
            return ret;
        }
        private bool ProcessOxygenData(TranstekDeviceTelemetry dev, string ConnectionString,string stagingInsert)
        {
            bool ret;
            try
            {
                vitalunits_dictionary.TryGetValue("Pulse", out string Unitpulse);
                vitalunits_dictionary.TryGetValue("Oxygen", out string Unitspo2);
                JObject objOxygen = JObject.Parse(dev.deviceData.ToString());
                string readingIdWithPrefix = "TK" + objOxygen["imei"] + objOxygen["ts"];
                StagingInput pulseoximeter_oxygen = new StagingInput();
                StagingInput pulseoximeter_pulse = new StagingInput();
                pulseoximeter_oxygen.reading_id = readingIdWithPrefix;
                pulseoximeter_pulse.reading_id = readingIdWithPrefix;
                pulseoximeter_oxygen.device_id = dev.deviceId;
                pulseoximeter_pulse.device_id = dev.deviceId;
                pulseoximeter_oxygen.device_model = "BM1000";
                pulseoximeter_pulse.device_model = "BM1000";
                string timeString = objOxygen["time"].ToString();
                string dateTimePart = timeString.Substring(0, timeString.Length - 3);
                DateTime time = DateTime.ParseExact(dateTimePart, "yy/MM/dd,HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
                string formattedTime = time.ToString("yyyy-MM-dd HH:mm:ss");
                pulseoximeter_oxygen.date_recorded = formattedTime;
                pulseoximeter_pulse.date_recorded = formattedTime;
                var dateTimeRx = ConvertUnixTimestampToDateTime(dev.createdAt);
                pulseoximeter_oxygen.date_received = dateTimeRx.ToString("yyyy-MM-dd HH:mm:ss");
                pulseoximeter_pulse.date_received = dateTimeRx.ToString("yyyy-MM-dd HH:mm:ss");
                pulseoximeter_oxygen.reading_type = "pulse_ox";
                pulseoximeter_pulse.reading_type = "pulse_ox";
                pulseoximeter_oxygen.battery = (int)objOxygen["battery"];
                pulseoximeter_pulse.battery = (int)objOxygen["battery"];
                string timeZone = objOxygen["time"].ToString();
                string timeZoneOffsetStr = timeZone.Substring(timeZone.Length - 3);
                if (int.TryParse(timeZoneOffsetStr, out int timeZoneOffsetInMinutes))
                {
                    int totalMinutes = timeZoneOffsetInMinutes * 15;
                    int timeZoneOffsetInHours = totalMinutes / 60;
                    int remainingMinutes = totalMinutes % 60;
                    pulseoximeter_oxygen.time_zone_offset = timeZoneOffsetInHours.ToString();
                    pulseoximeter_pulse.time_zone_offset = timeZoneOffsetInHours.ToString();
                }
                else
                {
                    pulseoximeter_oxygen.time_zone_offset = null;
                    pulseoximeter_pulse.time_zone_offset = null;
                }
                pulseoximeter_oxygen.before_meal = false;
                pulseoximeter_pulse.before_meal = false;
                pulseoximeter_oxygen.event_flag = null;
                pulseoximeter_pulse.event_flag = null;
                pulseoximeter_oxygen.irregular = false;
                pulseoximeter_pulse.irregular = false;
                pulseoximeter_oxygen.data_type = "Oxygen";
                pulseoximeter_oxygen.data_unit = Unitspo2;
                pulseoximeter_oxygen.data_value = Convert.ToDouble(objOxygen["spo2"]);
                pulseoximeter_pulse.data_type = "Pulse";
                pulseoximeter_pulse.data_unit = Unitpulse;
                pulseoximeter_pulse.data_value = Convert.ToDouble(objOxygen["pr"]);
                string jsonDataOx = JsonConvert.SerializeObject(pulseoximeter_oxygen);
                string jsonDataPulse = JsonConvert.SerializeObject(pulseoximeter_pulse);
                string insertvalues = "('" + jsonDataOx + "'),('" + jsonDataPulse + "')";
                StagingTableInsertJson(stagingInsert + insertvalues, ConnectionString);
                ret = true;
            }
            catch (Exception)
            {
                ret = false;
            }
            return ret;
        }
        private static void StagingTableInsertJson(string jsonData, string ConnectionString)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(ConnectionString))
                {
                    SqlCommand command = new SqlCommand("usp_InsJsonStg_Transtek", con);
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@Json", jsonData);
                    command.Parameters.AddWithValue("@CreatedBy", DateTime.Now.ToUniversalTime());
                    con.Open();
                    command.ExecuteScalar();
                    con.Close();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
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
