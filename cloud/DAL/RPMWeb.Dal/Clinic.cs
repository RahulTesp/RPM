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
    public class Clinic
    {
        public int AddClinic(ClinicInfo info,string CreatedBy, string ConnectionString)
        {
          
            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    SqlCommand command = new SqlCommand("usp_InsOrganization", connection);
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add("@Name", SqlDbType.NVarChar).Value = info.Name;
                    command.Parameters.Add("@Code", SqlDbType.NVarChar).Value = info.Code;
                    command.Parameters.Add("@ParentOrganizationID", SqlDbType.NVarChar).Value = info.ParentOrganizationID;
                    command.Parameters.Add("@PointOfContact", SqlDbType.NVarChar).Value = info.Name;
                    command.Parameters.Add("@OrganizationTypeName", SqlDbType.NVarChar).Value = "Clinic";
                    command.Parameters.Add("@PhoneNumber", SqlDbType.NVarChar).Value = info.PhoneNumber;
                    command.Parameters.Add("@AlternateNumber", SqlDbType.NVarChar).Value = info.AlternateNumber;
                    command.Parameters.Add("@Address1", SqlDbType.NVarChar).Value = info.AddrLine1;
                    command.Parameters.Add("@Address2", SqlDbType.NVarChar).Value = info.AddrLine2;
                    command.Parameters.Add("@ZipCode", SqlDbType.NVarChar).Value = info.ZipCode;
                    command.Parameters.Add("@CityId", SqlDbType.Int).Value = info.CityId;
                    command.Parameters.Add("@StateId", SqlDbType.Int).Value = info.StateId;
                    command.Parameters.Add("@CountryId", SqlDbType.Int).Value = info.CountryId;
                    command.Parameters.Add("@TimeZoneID", SqlDbType.Int).Value = info.TimeZoneId;
                    command.Parameters.AddWithValue("@CreatedBy", CreatedBy);
                    SqlParameter returnParameter = command.Parameters.Add("RetVal", SqlDbType.Int);
                    returnParameter.Direction = ParameterDirection.ReturnValue;
                    connection.Open();
                    command.ExecuteNonQuery();
                    int id = (int)returnParameter.Value;
                    connection.Close();
                    if (id.Equals(0))
                    {
                        return 0;
                    }
                    else
                    {
                        return id;
                    }

                }
            }
            catch
            {
                throw;
            }
            
        }
        public bool UpdateClinic(UpdClinicInfo info, string CreatedBy,string ConnectionString)
        {
            bool ret = true;
            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    SqlCommand command = new SqlCommand("usp_UpdOrganization", connection);
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add("@Id", SqlDbType.Int).Value = info.Id;
                    command.Parameters.Add("@Name", SqlDbType.NVarChar).Value = info.Name;
                    command.Parameters.Add("@Code", SqlDbType.NVarChar).Value = info.Code;
                    command.Parameters.Add("@ParentOrganizationID", SqlDbType.NVarChar).Value = info.ParentOrganizationID;
                    command.Parameters.Add("@PointOfContact", SqlDbType.NVarChar).Value = info.Name;
                    command.Parameters.Add("@OrganizationTypeName", SqlDbType.NVarChar).Value = "Clinic";
                    command.Parameters.Add("@PhoneNumber", SqlDbType.NVarChar).Value = info.PhoneNumber;
                    command.Parameters.Add("@AlternateNumber", SqlDbType.NVarChar).Value = info.AlternateNumber;
                    command.Parameters.Add("@Address1", SqlDbType.NVarChar).Value = info.AddrLine1;
                    command.Parameters.Add("@Address2", SqlDbType.NVarChar).Value = info.AddrLine2;
                    command.Parameters.Add("@ZipCode", SqlDbType.NVarChar).Value = info.ZipCode;
                    command.Parameters.Add("@CityId", SqlDbType.Int).Value = info.CityId;
                    command.Parameters.Add("@StateId", SqlDbType.Int).Value = info.StateId;
                    command.Parameters.Add("@CountryId", SqlDbType.Int).Value = info.CountryId;
                    command.Parameters.Add("@TimeZoneID", SqlDbType.Int).Value = info.TimeZoneId;
                    command.Parameters.AddWithValue("@ModifiedBy", CreatedBy);
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
                }
            }
            catch
            {
                throw;
            }
            return ret;
        }
        public ClinicInfo GetClinic(int clinicid, string ConnectionString)
        {
            ClinicInfo info = null;
            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    SqlCommand command = new SqlCommand("usp_GetOrganization", connection);
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add("@Id", SqlDbType.Int).Value = clinicid;
                    command.Parameters.Add("@CreatedBy", SqlDbType.NVarChar).Value = "";
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        info = new ClinicInfo();
                        info.Name = reader["Name"].ToString();
                        info.Code = reader["Code"].ToString();
                        info.ParentOrganizationID = (!DBNull.Value.Equals(reader["ParentOrganizationID"])) ? Convert.ToInt32(reader["ParentOrganizationID"]) : 0;
                        info.AddrLine1 = reader["Address1"].ToString();
                        info.AddrLine2 = reader["Address2"].ToString();
                        info.PhoneNumber = reader["PhoneNumber"].ToString();
                        info.AlternateNumber = reader["AlternateNumber"].ToString();
                        info.CityId = (!DBNull.Value.Equals(reader["CityID"])) ? Convert.ToInt32(reader["CityID"]) : 0;
                        info.StateId = (!DBNull.Value.Equals(reader["StateId"])) ? Convert.ToInt32(reader["StateId"]) : 0;
                        info.CountryId = (!DBNull.Value.Equals(reader["CountryId"])) ? Convert.ToInt32(reader["CountryId"]) : 0;
                        info.ZipCode = reader["ZipCode"].ToString();
                        info.TimeZoneId = (!DBNull.Value.Equals(reader["TimeZoneID"])) ? Convert.ToInt32(reader["TimeZoneID"]) : 0;
                    }
                }
            }
            catch
            {
                throw;
            }
            return info;

        }
        public bool DeleteClinic(int clinicid, string ConnectionString)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    SqlCommand command = new SqlCommand("usp_DelOrganization", connection);
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add("@Id", SqlDbType.Int).Value = clinicid;
                    command.Parameters.Add("@ModifiedBy", SqlDbType.NVarChar).Value = "";
                    connection.Open();
                    int retval = command.ExecuteNonQuery();
                }
            }
            catch
            {
                throw;
            }
            return true;

        }
        public List<ClinicAllInfo> GetAllClinic(string ConnectionString)
        {
            //DataSet ds;
            List<ClinicAllInfo> lstRet = new List<ClinicAllInfo>();
            try
            {
               
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    SqlCommand command = new SqlCommand("usp_GetAllOrganization", connection);
                    command.CommandType = CommandType.StoredProcedure;
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        ClinicAllInfo info = new ClinicAllInfo();
                        info.Id = (int)reader["OrgId"];
                        info.Name = reader["OrgName"].ToString();
                        info.Code = reader["OrgCode"].ToString();
                        info.Active = (!DBNull.Value.Equals(reader["OrgIsactive"])) ? Convert.ToBoolean(reader["OrgIsactive"]) : false;
                        info.CityId = (!DBNull.Value.Equals(reader["OrgCityId"])) ? Convert.ToInt32(reader["OrgCityId"]) : 0;
                        info.StateId = (!DBNull.Value.Equals(reader["OrgStateId"])) ? Convert.ToInt32(reader["OrgStateId"]) : 0;
                        info.CityName= reader["OrgCityName"].ToString();
                        info.CountryId = (!DBNull.Value.Equals(reader["OrgCountryId"])) ? Convert.ToInt32(reader["OrgCountryId"]) : 0;
                        info.CreatedDate = (DateTime)reader["OrgCreatedOn"];
                        info.PatientCount = (!DBNull.Value.Equals(reader["OrgPatientCount"])) ? Convert.ToInt32(reader["OrgPatientCount"]) : 0;
                        info.PhysicianCount = (!DBNull.Value.Equals(reader["OrgPhysicianCount"])) ? Convert.ToInt32(reader["OrgPhysicianCount"]) : 0;
                        lstRet.Add(info);
                    }                    
                }
            }
            catch
            {
                throw;
            }
            return lstRet;

        }
    }
}

