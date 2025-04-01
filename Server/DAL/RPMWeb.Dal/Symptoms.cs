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
    public sealed class Symptoms
    {
        public int AddPatientProgramSymptoms(PatientSymptom data, string ConnectionString)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    SqlCommand command = new SqlCommand("usp_InsPatientProgramSymptoms", connection);
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@PatientId", data.PatientId);
                    command.Parameters.AddWithValue("@Symptom", data.Symptom);
                    command.Parameters.AddWithValue("@Description", data.Description);
                    command.Parameters.AddWithValue("@SymptomStartDateTime", data.SymptomStartDateTime);
                    command.Parameters.AddWithValue("@CreatedBy", data.CreatedBy);
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
            catch (Exception)
            {
                throw;
            }

        }
        public bool UpdatePatientProgramSymptoms(PatientSymptom data, string ConnectionString)
        {
            bool ret = true;
            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    SqlCommand command = new SqlCommand("usp_UpdPatientProgramSymptoms", connection);
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@Id", data.Id);
                    command.Parameters.AddWithValue("@Symptom", data.Symptom);
                    command.Parameters.AddWithValue("@Description", data.Description);
                    command.Parameters.AddWithValue("@SymptomStartDateTime", data.SymptomStartDateTime);
                    command.Parameters.AddWithValue("@ModifiedBy", data.CreatedBy);
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
            catch (Exception)
            {
                throw;
            }

        }
        public List<GetPatientSymptom> GetPatientSymptoms(int PatientId, int PatientProgramId, string CreatedBy, string ConnectionString)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(ConnectionString))
                {
                    con.Open();

                    using (SqlCommand command = new SqlCommand("usp_GetPatientSymptoms", con))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@CreatedBy", CreatedBy);
                        command.Parameters.AddWithValue("@PatientId", PatientId);
                        command.Parameters.AddWithValue("@PatientProgramId", PatientProgramId);
                        SqlParameter returnParameter = command.Parameters.Add("RetVal", SqlDbType.Int);
                        returnParameter.Direction = ParameterDirection.ReturnValue;
                        SqlDataReader reader = command.ExecuteReader();
                        List<GetPatientSymptom> ListSymptoms = new List<GetPatientSymptom>();
                        while (reader.Read())
                        {
                            GetPatientSymptom ret = new GetPatientSymptom();
                            ret.Id = (int)reader["Id"];
                            ret.Symptom = reader["Symptom"].ToString();
                            ret.Description = reader["Description"].ToString();
                            ret.SymptomStartDateTime = (DateTime)reader["SymptomStartDateTime"];
                            ListSymptoms.Add(ret);
                        }
                        if (reader.FieldCount == 0)
                        {
                            return null;
                        }
                        return ListSymptoms;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public List<Symptom> GetSymptomsMasterData(string CreatedBy, string ConnectionString)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(ConnectionString))
                {
                    con.Open();

                    using (SqlCommand command = new SqlCommand("usp_GetMasterDataForSymptoms", con))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@CreatedBy", CreatedBy);                      
                        SqlParameter returnParameter = command.Parameters.Add("RetVal", SqlDbType.Int);
                        returnParameter.Direction = ParameterDirection.ReturnValue;
                        SqlDataReader reader = command.ExecuteReader();
                        List<Symptom> ListSymptoms = new List<Symptom>();
                        while (reader.Read())
                        {
                            Symptom ret = new Symptom();
                            ret.Id = (int)reader["Id"];
                            ret.Symptoms = reader["SymptomName"].ToString();                           
                            ListSymptoms.Add(ret);
                        }
                        if (reader.FieldCount == 0)
                        {
                            return null;
                        }
                        return ListSymptoms;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
