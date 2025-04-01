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
    public sealed class PgmNGoals
    {
        public bool AddPgmandGoals(PgmGoals data, string ConnectionString)
        {
            bool ret = true;
            try
            {
                string ProgramGoals = "INSERT INTO ProgramGoals([ProgramId],[Goal],[Description],[CreatedBy])VALUES";
                string inserts = string.Empty;
                List<GoalDetails> details1 = data.goalDetails;
                foreach (GoalDetails details in details1)
                {
                    string insertvalues = "(" + data.ProgramId + ",'" + details.Goal + "','" + details.Description + "','"+ data .CreatedBy+ "'),";
                    inserts = inserts + insertvalues;
                }
                string script = ProgramGoals + inserts;
                string ProgramGoalsInput = script.Substring(0, script.Length - 1);
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    SqlCommand command = new SqlCommand("usp_InsProgramGoals", connection);
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@ProgramGoals", ProgramGoalsInput);                   
                    command.Parameters.AddWithValue("@CreatedBy", data.CreatedBy);
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
        public bool UpdatePgmandGoals(PgmGoals data, string ConnectionString)
        {
            bool ret = true;
            try
            {
                string ProgramGoals = "INSERT INTO @@#programGoals([Id],[ProgramId],[Goal],[Description],[CreatedBy])VALUES";
                string inserts = string.Empty;
                List<GoalDetails> details1 = data.goalDetails;
                foreach (GoalDetails details in details1)
                {
                    string insertvalues = "(" + details.Id + "," + data.ProgramId + ",'" + details.Goal + "','" + details.Description + "','" + data.CreatedBy + "'),";
                    inserts = inserts + insertvalues;
                }
                string script = ProgramGoals + inserts;
                string ProgramGoalsInput = script.Substring(0, script.Length - 1) ;
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();
                    SqlCommand command = new SqlCommand("usp_UpdProgramGoals", connection);
                    command.CommandTimeout = 0;
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@ProgramGoals", ProgramGoalsInput);
                    command.Parameters.AddWithValue("@ModifiedBy", data.CreatedBy);
                    SqlParameter returnParameter = command.Parameters.Add("RetVal", SqlDbType.Int);
                    returnParameter.Direction = ParameterDirection.ReturnValue;                   
                    command.ExecuteNonQuery();
                    int id = (int)returnParameter.Value;                   
                    if (id.Equals(0))
                    {
                        ret = false;
                    }
                    connection.Close();
                    return ret;
                }
            }
            catch
            {
                throw;
            }
            
        }
       public List<GetPgmGoals> GetPgmandGoals(int ProgramId, string CreatedBy, string ConnectionString)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(ConnectionString))
                {
                    con.Open();

                    using (SqlCommand command = new SqlCommand("usp_GetProgramGoals", con))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@CreatedBy", CreatedBy);
                        command.Parameters.AddWithValue("@ProgramId", ProgramId);
                        SqlParameter returnParameter = command.Parameters.Add("RetVal", SqlDbType.Int);
                        returnParameter.Direction = ParameterDirection.ReturnValue;
                        SqlDataReader reader = command.ExecuteReader();
                        List<GetPgmGoals> list = new List<GetPgmGoals>();
                        while (reader.Read())
                        {
                            GetPgmGoals ret = new GetPgmGoals();
                            //ret.Id = (int)reader["Id"];                           
                            //ret.ProgramName = reader["ProgramName"].ToString();                         
                            //ret.Goal = reader["Goal"].ToString();
                            //ret.Description = reader["Description"].ToString();
                            list.Add(ret);
                        }
                        if (reader.FieldCount == 0)
                        {
                            return null;
                        }
                        return list;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public List<GetPgmGoals> GetAllPgmandGoals( string CreatedBy, string ConnectionString)
        {
            List<GetPgmGoals> ListGoals = new List<GetPgmGoals>();
            try
            {
                using (SqlConnection con = new SqlConnection(ConnectionString))
                {
                    con.Open();
                   
                    using (SqlCommand command = new SqlCommand("usp_GetProgramGoals", con))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@CreatedBy", CreatedBy);
                        using (SqlDataAdapter sda = new SqlDataAdapter())
                        {
                            DataSet ds;
                            //command.Connection = con;
                            sda.SelectCommand = command;
                            using (ds = new DataSet())
                            {
                                sda.Fill(ds);
                                int pgmid = 0;                                
                                foreach (DataRow dr in ds.Tables[0].Rows)
                                {
                                    if(pgmid!= Convert.ToInt32(dr["ProgramId"]))
                                    {
                                        pgmid = Convert.ToInt32(dr["ProgramId"]);
                                        GetPgmGoals goal = new GetPgmGoals();
                                        goal.ProgramId = Convert.ToInt32(dr["ProgramId"]);
                                        goal.ProgramName = dr["ProgramName"].ToString();
                                        goal.Duration = Convert.ToInt32(dr["Duration"]);
                                        List<Vitals> listv = new List<Vitals>();
                                        foreach (DataRow dr1 in ds.Tables[0].Rows)
                                        {
                                            if (Convert.ToInt32(dr["ProgramId"]) == Convert.ToInt32(dr1["ProgramId"]))
                                            {
                                                Vitals vitals = new Vitals();
                                                vitals.VitalId = Convert.ToInt32(dr1["VitalId"]);
                                                vitals.VitalName = dr1["VitalName"].ToString();
                                                listv.Add(vitals);
                                            }
                                        }
                                        goal.Vitals = listv;
                                        List<GoalDetails> goalLists = new List<GoalDetails>();
                                        foreach (DataRow dr1 in ds.Tables[1].Rows)
                                        {
                                            if (Convert.ToInt32(dr["ProgramId"]) == Convert.ToInt32(dr1["ProgramId"]))
                                            {
                                                GoalDetails d = new GoalDetails(Convert.ToInt32(dr1["Id"]),
                                                   dr1["Goal"].ToString(),
                                                   dr1["Description"].ToString());
                                                goalLists.Add(d);
                                            }
                                        }
                                        goal.goalDetails = goalLists;
                                        ListGoals.Add(goal);
                                    }                                   
                                }                                                                                                                                   
                            }
                        }
                    }
                }
                return ListGoals;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
