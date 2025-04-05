using RPMWeb.Data.Common;
using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPMWeb.Dal
{
    public class Device
    {
        public ReturnMsg AddDeviceProc(AddDevicePro info, string ConnectionString)
        {
            ReturnMsg ret = new ReturnMsg();
            ret.Msg = "";
            ret.Val = 0;
            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    SqlCommand command = new SqlCommand("usp_InsDeviceProc", connection);
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@Name", info.Device);
                    command.Parameters.AddWithValue("@DeviceType", info.Device);
                    command.Parameters.AddWithValue("@DeviceModel", info.DeviceModel);
                    command.Parameters.AddWithValue("@DeviceVendor", info.DeviceVendor);
                    command.Parameters.AddWithValue("@DeviceManufacturer", info.DeviceManufacturer);
                    command.Parameters.AddWithValue("@DeviceSerialNo", info.DeviceSerialNo);
                    command.Parameters.AddWithValue("@DeviceIMEINo", info.DeviceIMEINo);
                    command.Parameters.AddWithValue("@DeviceStatus", "Available");
                    command.Parameters.AddWithValue("@DeviceCommunicationType", info.DeviceType);
                    command.Parameters.AddWithValue("@PurchaseDate", info.PurchaseDate);
                    command.Parameters.AddWithValue("@CreatedBy", info.CreatedBy);
                    SqlParameter returnParameter = new SqlParameter();
                    returnParameter.ParameterName = "@Return";
                    returnParameter.SqlDbType = SqlDbType.NVarChar;
                    returnParameter.Direction = ParameterDirection.Output;
                    returnParameter.Size = 50;
                    command.Parameters.Add(returnParameter);
                    SqlParameter returnParameter1 = command.Parameters.Add("RetVal", SqlDbType.Int);
                    returnParameter1.Direction = ParameterDirection.ReturnValue;
                    connection.Open();
                    command.ExecuteNonQuery();
                    string id = returnParameter.Value.ToString();
                    int val = (int)returnParameter1.Value;
                    connection.Close();

                    ret.Msg = id;
                    ret.Val = val;

                    return ret;

                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public int UpdateDeviceProc(UpdateDevice info, string ConnectionString)
        {
            int ret = 0;
            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    SqlCommand command = new SqlCommand("usp_UpdDeviceProc", connection);
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@Id", info.Id);
                    command.Parameters.AddWithValue("@Name", info.Device);                   
                    command.Parameters.AddWithValue("@DeviceType", info.Device);
                    command.Parameters.AddWithValue("@DeviceModel", info.DeviceModel);
                    command.Parameters.AddWithValue("@DeviceManufacturer", info.DeviceManufacturer);
                    command.Parameters.AddWithValue("@DeviceIMEINo", info.DeviceIMEINo);
                    command.Parameters.AddWithValue("@DeviceCommunicationType", info.DeviceType);
                    command.Parameters.AddWithValue("@PurchaseDate", info.PurchaseDate);
                    command.Parameters.AddWithValue("@ModifiedBy", info.CreatedBy);
                    SqlParameter returnParameter = command.Parameters.Add("RetVal", SqlDbType.Int);
                    returnParameter.Direction = ParameterDirection.ReturnValue;
                    connection.Open();
                    command.ExecuteNonQuery();
                    ret = (int)returnParameter.Value;
                    connection.Close();

                }
                return ret;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public DataSet GetDeviceMasterData(string CreatedBy, string ConnectionString)
        {
            DataSet ds;
            try
            {
                using (SqlConnection con = new SqlConnection(ConnectionString))
                {
                    con.Open();

                    using (SqlCommand command = new SqlCommand("usp_GetMasterDataForDevice", con))
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
                                ds.Tables[0].TableName = "DeviceModel";
                                ds.Tables[1].TableName = "DeviceVendor";
                                ds.Tables[2].TableName = "DeviceType";
                                ds.Tables[3].TableName = "DeviceManufacturer";
                                ds.Tables[4].TableName = "Device";
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
        public List<DeviceInfo> GetDeviceInfo(string CreatedBy, string ConnectionString)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(ConnectionString))
                {
                    con.Open();

                    using (SqlCommand command = new SqlCommand("usp_GetAllDevices", con))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@CreatedBy", CreatedBy);
                        SqlParameter returnParameter = command.Parameters.Add("RetVal", SqlDbType.Int);
                        returnParameter.Direction = ParameterDirection.ReturnValue;
                        SqlDataReader reader = command.ExecuteReader();
                        List<DeviceInfo> devicelist = new List<DeviceInfo>();
                        while (reader.Read())
                        {
                            DeviceInfo info = new DeviceInfo();
                            info.Id = Convert.ToInt32(reader["Id"]);
                            info.DeviceNumber = reader["DeviceNumber"].ToString();
                            info.Device = reader["Device"].ToString();
                            info.DeviceType = reader["DeviceType"].ToString();
                            info.PatientName = reader["PatientName"].ToString();
                            info.DeviceActivatedDateTime = reader["DeviceActivatedDateTime"].ToString();
                            info.DeviceStatus = reader["DeviceStatus"].ToString();
                            devicelist.Add(info);
                        }
                        if (reader.FieldCount == 0)
                        {
                            return null;
                        }
                        return devicelist;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public ReturnMsg AddDeviceVendor(AddDeviceVendor info, string ConnectionString)
        {
            ReturnMsg ret = new ReturnMsg();
            ret.Msg = "";
            ret.Val = 0;
            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    SqlCommand command = new SqlCommand("usp_InsDeviceVendor", connection);
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@Name", info.Name);
                    command.Parameters.AddWithValue("@Code", info.Code);
                    command.Parameters.AddWithValue("@Address1", info.Address1);
                    command.Parameters.AddWithValue("@Address2", info.Address2);
                    command.Parameters.AddWithValue("@CityId", info.CityId);
                    command.Parameters.AddWithValue("@StateId", info.StateId);
                    command.Parameters.AddWithValue("@CountryId", info.CountryId);
                    command.Parameters.AddWithValue("@ZipCode", info.ZipCode);
                    command.Parameters.AddWithValue("@PrimaryPhoneNumber", info.PrimaryPhoneNumber);
                    command.Parameters.AddWithValue("@AlternatePhoneNumber", info.AlternatePhoneNumber);
                    command.Parameters.AddWithValue("@CreatedBy", info.CreatedBy);
                    SqlParameter returnParameter = new SqlParameter();
                    returnParameter.ParameterName = "@Return";
                    returnParameter.SqlDbType = SqlDbType.NVarChar;
                    returnParameter.Direction = ParameterDirection.Output;
                    returnParameter.Size = 50;
                    command.Parameters.Add(returnParameter);
                    SqlParameter returnParameter1 = command.Parameters.Add("RetVal", SqlDbType.Int);
                    returnParameter1.Direction = ParameterDirection.ReturnValue;
                    connection.Open();
                    command.ExecuteNonQuery();
                    var id = returnParameter.Value.ToString();
                    int val = (int)returnParameter1.Value;
                    connection.Close();
                    ret.Msg = id;
                    ret.Val = val;
                    return ret;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public int UpdateDeviceVendor(UpdateDeviceVendor info, string ConnectionString)
        {
            int ret = 0;
            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    SqlCommand command = new SqlCommand("usp_UpdDeviceVendor", connection);
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@Id", info.Id);
                    command.Parameters.AddWithValue("@Name", info.Name);
                    command.Parameters.AddWithValue("@Code", info.Code);
                    command.Parameters.AddWithValue("@Address1", info.Address1);
                    command.Parameters.AddWithValue("@Address2", info.Address2);
                    command.Parameters.AddWithValue("@CityId", info.CityId);
                    command.Parameters.AddWithValue("@StateId", info.StateId);
                    command.Parameters.AddWithValue("@CountryId", info.CountryId);
                    command.Parameters.AddWithValue("@ZipCode", info.ZipCode);
                    command.Parameters.AddWithValue("@PrimaryPhoneNumber", info.PrimaryPhoneNumber);
                    command.Parameters.AddWithValue("@AlternatePhoneNumber", info.AlternatePhoneNumber);
                    command.Parameters.AddWithValue("@ModifiedBy", info.CreatedBy);
                    SqlParameter returnParameter = command.Parameters.Add("RetVal", SqlDbType.Int);
                    returnParameter.Direction = ParameterDirection.ReturnValue;
                    connection.Open();
                    command.ExecuteNonQuery();
                    ret = (int)returnParameter.Value;
                    connection.Close();

                }
                return ret;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public bool IsValidVendorCode(string Code,string CreatedBy, string ConnectionString)
        {
            bool ret = false;
            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    SqlCommand command = new SqlCommand("usp_IsValidVendorCode", connection);
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@Code", Code);
                    command.Parameters.AddWithValue("@CreatedBy", CreatedBy);
                    SqlParameter returnParameter = command.Parameters.Add("RetVal", SqlDbType.Int);
                    returnParameter.Direction = ParameterDirection.ReturnValue;
                    connection.Open();
                    command.ExecuteNonQuery();
                    int output = (int)returnParameter.Value;
                    if (output == 1)
                    {
                        ret = true;
                    }
                    connection.Close();

                }
                return ret;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
