using Microsoft.Extensions.Azure;
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
    public sealed class WebAppPatient
    {
        
        public List<PatientToDoListResponse> GetToDoList(string Username, DateTime StartDate, DateTime EndDate, string ConnectionString)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(ConnectionString))
                {
                    con.Open();

                    using (SqlCommand command = new SqlCommand("usp_GetPatientToDoList", con))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@CreatedBy", Username);
                        command.Parameters.AddWithValue("@StartDate", StartDate);
                        command.Parameters.AddWithValue("@EndDate", EndDate);

                        SqlParameter returnParameter = command.Parameters.Add("RetVal", SqlDbType.Int);
                        returnParameter.Direction = ParameterDirection.ReturnValue;
                        SqlDataReader reader = command.ExecuteReader();
                        Schedule ScheduleList = new Schedule();
                        //ScheduleTypes types = new ScheduleTypes();
                        List <PatientToDoListNew> PatientToDoListNew = new List<PatientToDoListNew>();
                        //List<int> roleids = new List<int>();
                        while (reader.Read())
                        {
                            PatientToDoListNew todo = new PatientToDoListNew();
                            //types.ScheduleId= (!DBNull.Value.Equals(reader["ScheduleId"])) ? Convert.ToInt32(reader["ScheduleId"]) : 0;
                            //types.CurrentScheduleId = (!DBNull.Value.Equals(reader["CurrentScheduleId"])) ? Convert.ToInt32(reader["CurrentScheduleId"]) : 0; 
                            //types.Interval = Convert.ToString(reader["Interval"]);
                            todo.StartDate = (!DBNull.Value.Equals(reader["StartDate"])) ? Convert.ToDateTime(reader["StartDate"]) : DateTime.MinValue;
                            todo.EndDate = (!DBNull.Value.Equals(reader["EndDate"])) ? Convert.ToDateTime(reader["EndDate"]) : DateTime.MinValue;
                            todo.ActivityName = Convert.ToString(reader["ToDoListType"]);                            
                            todo.Description = Convert.ToString(reader["Description"]);
                            todo.Morning = (!DBNull.Value.Equals(reader["Morning"])) ? Convert.ToBoolean(reader["Morning"]) : false;
                            todo.AfterNoon = (!DBNull.Value.Equals(reader["AfterNoon"])) ? Convert.ToBoolean(reader["AfterNoon"]) : false;
                           todo.Evening = (!DBNull.Value.Equals(reader["Evening"])) ? Convert.ToBoolean(reader["Evening"]) : false;
                            todo.Night = (!DBNull.Value.Equals(reader["Night"])) ? Convert.ToBoolean(reader["Night"]) : false;
                            todo.Schedule = Convert.ToString(reader["Schedule"]);
                            PatientToDoListNew.Add(todo);

                        }
                        List<PatientToDoListResponse> Response = new List<PatientToDoListResponse>();
                        foreach (PatientToDoListNew todo in PatientToDoListNew)
                        {
                            PatientToDoListResponse item = new PatientToDoListResponse();
                            if (todo.ActivityName == "Vital Reading")
                            {
                                item.Date = todo.StartDate;
                                item.Description = todo.Description;
                                item.Morning = todo.Morning;
                                item.AfterNoon = todo.AfterNoon;
                                item.Evening = todo.Evening;
                                item.Night = todo.Night;
                                item.ActivityName= todo.ActivityName;
                                Response.Add(item);
                            }
                            if(todo.ActivityName== "Medicine")
                            {
                                switch (todo.Schedule)
                                {
                                    case "Daily": 
                                        item.Date = StartDate;
                                        item.Description = todo.Description;
                                        item.Morning = todo.Morning;
                                        item.AfterNoon = todo.AfterNoon;
                                        item.Evening = todo.Evening;
                                        item.Night = todo.Night;
                                        item.ActivityName = todo.ActivityName;
                                        Response.Add(item);
                                        break;

                                    case "Weekly":
                                        if(todo.StartDate == todo.EndDate && todo.StartDate == StartDate)
                                        {  
                                            item.Date = StartDate;
                                            item.Description = todo.Description;
                                            item.Morning = todo.Morning;
                                            item.AfterNoon = todo.AfterNoon;
                                            item.Evening = todo.Evening;
                                            item.Night = todo.Night;
                                            item.ActivityName = todo.ActivityName;
                                            Response.Add(item);
                                            break;
                                        }
                                        else
                                        {
                                            while (todo.StartDate < todo.EndDate)
                                            {
                                                if (todo.StartDate != StartDate) { todo.StartDate = todo.StartDate.AddDays(7); }
                                                if (todo.StartDate == StartDate)
                                                {
                                                    item.Date =StartDate;
                                                    item.Description = todo.Description;
                                                    item.Morning = todo.Morning;
                                                    item.AfterNoon = todo.AfterNoon;
                                                    item.Evening = todo.Evening;
                                                    item.Night = todo.Night;
                                                    item.ActivityName = todo.ActivityName;
                                                    Response.Add(item);
                                                    break;
                                                }
                                            }
                                            break;

                                        }
                                    case "Monthly":
                                        if (todo.StartDate == todo.EndDate&& todo.StartDate==StartDate)
                                        { 
                                            item.Date = StartDate;
                                            item.Description = todo.Description;
                                            item.Morning = todo.Morning;
                                            item.AfterNoon = todo.AfterNoon;
                                            item.Evening = todo.Evening;
                                            item.Night = todo.Night;
                                            item.ActivityName = todo.ActivityName;
                                            Response.Add(item);
                                            break;
                                        }
                                        else
                                        {
                                            while (todo.StartDate < todo.EndDate)
                                            {
                                                if (todo.StartDate != StartDate) { todo.StartDate = todo.StartDate.AddMonths(1); }
                                                if (todo.StartDate == StartDate)
                                                {            
                                                    item.Date = StartDate;
                                                    item.Description = todo.Description;
                                                    item.Morning = todo.Morning;
                                                    item.AfterNoon = todo.AfterNoon;
                                                    item.Evening = todo.Evening;
                                                    item.Night = todo.Night;
                                                    item.ActivityName = todo.ActivityName;
                                                    Response.Add(item);
                                                    break;
                                                }
                                            }
                                            break;

                                        } 
                                    case "Alternative":
                                        if (todo.StartDate == todo.EndDate && todo.StartDate == StartDate)
                                        { 
                                            item.Date = StartDate;
                                            item.Description = todo.Description;
                                            item.Morning = todo.Morning;
                                            item.AfterNoon = todo.AfterNoon;
                                            item.Evening = todo.Evening;
                                            item.Night = todo.Night;
                                            item.ActivityName = todo.ActivityName;
                                            Response.Add(item);
                                            break;
                                        }
                                        else
                                        {
                                            while (todo.StartDate < todo.EndDate)
                                            {
                                                if (todo.StartDate != StartDate) { todo.StartDate = todo.StartDate.AddDays(2); } 
                                                if (todo.StartDate == StartDate)
                                                {
                                                    item.Date = StartDate;
                                                    item.Description = todo.Description;
                                                    item.Morning = todo.Morning;
                                                    item.AfterNoon = todo.AfterNoon;
                                                    item.Evening = todo.Evening;
                                                    item.Night = todo.Night;
                                                    item.ActivityName = todo.ActivityName;
                                                    Response.Add(item);
                                                    break;
                                                }
                                            }
                                            break;

                                        }
                                    default:     
                                        break;
                                }
                            }
                            
                        }


                        /*if (ScheduleList.ScheduleTypes.Count == 0)
                        {
                            return null;
                        }*/
                        return Response;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        
        
        }

        public PatientCurrentReading GetCurrentCycleReading(int PatientId,  string ConnectionString)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(ConnectionString))
                {
                    con.Open();

                    using (SqlCommand command = new SqlCommand("usp_GetCurrentCycleReading", con))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@PatientId", PatientId);
                  
                        //command.Parameters.AddWithValue("@EndDate", day);

                        SqlParameter returnParameter = command.Parameters.Add("RetVal", SqlDbType.Int);
                        returnParameter.Direction = ParameterDirection.ReturnValue;
                        SqlDataReader reader = command.ExecuteReader();
                        Schedule ScheduleList = new Schedule();
                        //ScheduleTypes types = new ScheduleTypes();
                        List<PatientToDoList> PatientToDoList = new List<PatientToDoList>();
                        //List<int> roleids = new List<int>();
                        PatientCurrentReading info = new PatientCurrentReading();
                        while (reader.Read())
                        {
                           
                            info.TotalReadings=Convert.ToInt32(reader["TotalReadings"]);
                            info.DaysCompleted=Convert.ToInt32(reader["DaysCompleted"]);
                            info.CPTCode=Convert.ToString(reader["CPTCode"]);
                            info.PatientVital= Convert.ToString(reader["PatientVital"]);

                        }

                        /*if (ScheduleList.ScheduleTypes.Count == 0)
                        {
                            return null;
                        }*/
                        return info;
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
