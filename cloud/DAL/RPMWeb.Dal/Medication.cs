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
    public sealed class Medication
    {
        public int AddPatientProgramMedication(PatientMedication data, string ConnectionString)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    SqlCommand command = new SqlCommand("usp_InsPatientProgramMedication", connection);
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@PatientId", data.PatientId);
                    command.Parameters.AddWithValue("@PatientProgramId", data.PatientProgramId);
                    command.Parameters.AddWithValue("@Medicinename", data.Medicinename);
                    command.Parameters.AddWithValue("@MedicineSchedule", data.MedicineSchedule);
                    command.Parameters.AddWithValue("@BeforeOrAfterMeal", data.BeforeOrAfterMeal);
                    command.Parameters.AddWithValue("@Morning", data.Morning);
                    command.Parameters.AddWithValue("@AfterNoon", data.AfterNoon);
                    command.Parameters.AddWithValue("@Evening", data.Evening);
                    command.Parameters.AddWithValue("@Night", data.Night);
                    command.Parameters.AddWithValue("@StartDate", data.StartDate);
                    command.Parameters.AddWithValue("@EndDate", data.EndDate);
                    command.Parameters.AddWithValue("@Description", data.Description);
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
        public bool UpdatePatientProgramSymptoms(PatientMedication data, string ConnectionString)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    SqlCommand command = new SqlCommand("usp_UpdPatientProgramMedication", connection);
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@Id", data.Id);                  
                    command.Parameters.AddWithValue("@Medicinename", data.Medicinename);
                    command.Parameters.AddWithValue("@MedicineSchedule", data.MedicineSchedule);
                    command.Parameters.AddWithValue("@BeforeOrAfterMeal", data.BeforeOrAfterMeal);
                    command.Parameters.AddWithValue("@Morning", data.Morning);
                    command.Parameters.AddWithValue("@AfterNoon", data.AfterNoon);
                    command.Parameters.AddWithValue("@Evening", data.Evening);
                    command.Parameters.AddWithValue("@Night", data.Night);
                    command.Parameters.AddWithValue("@StartDate", data.StartDate);
                    command.Parameters.AddWithValue("@EndDate", data.EndDate);
                    command.Parameters.AddWithValue("@Description", data.Description);
                    command.Parameters.AddWithValue("@ModifiedBy", data.CreatedBy);
                    SqlParameter returnParameter = command.Parameters.Add("RetVal", SqlDbType.Int);
                    returnParameter.Direction = ParameterDirection.ReturnValue;
                    connection.Open();
                    command.ExecuteNonQuery();
                    int id = (int)returnParameter.Value;
                    connection.Close();
                    if (id.Equals(0))
                    {
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }

        }
        public List<GetPatientMedication> GetPatientMedication(int PatientId, int PatientProgramId, string CreatedBy, string ConnectionString)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(ConnectionString))
                {
                    con.Open();

                    using (SqlCommand command = new SqlCommand("usp_GetPatientProgramMedication", con))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@CreatedBy", CreatedBy);
                        command.Parameters.AddWithValue("@PatientId", PatientId);
                        command.Parameters.AddWithValue("@PatientProgramId", PatientProgramId);
                        SqlParameter returnParameter = command.Parameters.Add("RetVal", SqlDbType.Int);
                        returnParameter.Direction = ParameterDirection.ReturnValue;
                        SqlDataReader reader = command.ExecuteReader();
                        List<GetPatientMedication> ListSymptoms = new List<GetPatientMedication>();
                        while (reader.Read())
                        {
                            GetPatientMedication ret = new GetPatientMedication();
                            ret.Id = (int)reader["Id"];
                            ret.Medicinename = reader["Medicinename"].ToString();
                            ret.MedicineSchedule = reader["MedicineSchedule"].ToString();
                            ret.BeforeOrAfterMeal = reader["BeforeOrAfterMeal"].ToString();
                            ret.Morning = (!DBNull.Value.Equals(reader["Morning"])) ? Convert.ToBoolean(reader["Morning"]) : false;
                            ret.AfterNoon = (!DBNull.Value.Equals(reader["AfterNoon"])) ? Convert.ToBoolean(reader["AfterNoon"]) : false;
                            ret.Evening = (!DBNull.Value.Equals(reader["Evening"])) ? Convert.ToBoolean(reader["Evening"]) : false;
                            ret.Night = (!DBNull.Value.Equals(reader["Night"])) ? Convert.ToBoolean(reader["Night"]) : false;
                            ret.StartDate = (!DBNull.Value.Equals(reader["StartDate"])) ? Convert.ToDateTime(reader["StartDate"]) : DateTime.MinValue;
                            ret.EndDate = (!DBNull.Value.Equals(reader["EndDate"])) ? Convert.ToDateTime(reader["EndDate"]) : DateTime.MinValue;
                            ret.Description = reader["Description"].ToString();
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
