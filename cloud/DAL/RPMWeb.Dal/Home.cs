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
    class Home
    {

        public List<DashboardPatientStatusList> GetDashboardPatientStatus(int RoleId, string CreatedBy, string ConnectionString)
        {

            List<DashboardPatientStatusList> list = new List<DashboardPatientStatusList>();
            List<DashboardPatientStatusList1> list1 = new List<DashboardPatientStatusList1>();
            try
            {
                using (SqlConnection con = new SqlConnection(ConnectionString))
                {

                    using (SqlCommand command = new SqlCommand("usp_DashboardGetAllPatientsStatus", con))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@RoleId", RoleId);
                        command.Parameters.AddWithValue("@CreatedBy", CreatedBy);
                        con.Open();
                        using (SqlDataAdapter sda = new SqlDataAdapter())
                        {
                            DataSet ds;
                            sda.SelectCommand = command;
                            using (ds = new DataSet())
                            {
                                sda.Fill(ds);

                                List<string> listNames = new List<string>();
                                List<int> ListIds = new List<int>();

                                /*foreach (DataRow dr in ds.Tables[0].Rows)
                                {
                                    DashboardPatientStatusList1 l = new DashboardPatientStatusList1();
                                    if (!ListIds.Contains(Convert.ToInt32(dr["Id"])))
                                    {
                                        ListIds.Add(Convert.ToInt32(dr["Id"]));
                                        //listNames.Add(dr["Name"].ToString());
                                        l.Name = dr["Name"].ToString();
                                        l.Id= Convert.ToInt32(dr["Id"]);
                                        list1.Add(l);
                                    }
                                    
                                }*/

                                foreach (DataRow dr in ds.Tables[0].Rows)
                                {
                                    DashboardPatientStatusList1 l = new DashboardPatientStatusList1();
                                    if (!ListIds.Contains((!DBNull.Value.Equals(dr["Id"])) ? Convert.ToInt32(dr["Id"]) : 0) || !listNames.Contains((dr["Name"].ToString())))
                                    {
                                        ListIds.Add((!DBNull.Value.Equals(dr["Id"])) ? Convert.ToInt32(dr["Id"]) : 0);
                                        listNames.Add(dr["Name"].ToString());
                                        l.Name = dr["Name"].ToString();
                                        l.Id = (!DBNull.Value.Equals(dr["Id"])) ? Convert.ToInt32(dr["Id"]) : 0;
                                        list1.Add(l);
                                    }

                                }







                                /*foreach (DataRow dr in ds.Tables[0].Rows)
                                 {
                                     if (!listNames.Contains(dr["Name"].ToString()))
                                     {
                                         listNames.Add(dr["Name"].ToString());
                                     }

                                 }*/
                                foreach (DashboardPatientStatusList1 na in list1)
                                {

                                    DashboardPatientStatusList statusList = new DashboardPatientStatusList();
                                    DashboardPatientStatus dashboardPatient = new DashboardPatientStatus();
                                    foreach (DataRow dr in ds.Tables[0].Rows)
                                    {

                                        if (na.Name == dr["Name"].ToString() && na.Id == ((!DBNull.Value.Equals(dr["Id"])) ? Convert.ToInt32(dr["Id"]) : 0))
                                        {
                                            statusList.Id = dr["Id"].ToString();
                                            statusList.Name = dr["Name"].ToString();
                                            if (dr["Status"].ToString() == "NoPatients")
                                            {
                                                dashboardPatient.Prescribed = 0;
                                                dashboardPatient.Enrolled = 0;
                                                dashboardPatient.Active = 0;
                                                dashboardPatient.InActive = 0;
                                                dashboardPatient.OnHold = 0;
                                                dashboardPatient.ReadyForDischarge = 0;
                                                dashboardPatient.Discharged = 0;
                                            }

                                            if (dr["Status"].ToString() == "Prescribed")
                                                dashboardPatient.Prescribed = (!DBNull.Value.Equals(dr["PatientCount"])) ? Convert.ToInt32(dr["PatientCount"]) : 0;
                                            if (dr["Status"].ToString() == "Enrolled")
                                                dashboardPatient.Enrolled = (!DBNull.Value.Equals(dr["PatientCount"])) ? Convert.ToInt32(dr["PatientCount"]) : 0;
                                            if (dr["Status"].ToString() == "Active")
                                                dashboardPatient.Active = (!DBNull.Value.Equals(dr["PatientCount"])) ? Convert.ToInt32(dr["PatientCount"]) : 0;
                                            if (dr["Status"].ToString() == "InActive")
                                                dashboardPatient.InActive = (!DBNull.Value.Equals(dr["PatientCount"])) ? Convert.ToInt32(dr["PatientCount"]) : 0;
                                            if (dr["Status"].ToString() == "OnHold")
                                                dashboardPatient.OnHold = (!DBNull.Value.Equals(dr["PatientCount"])) ? Convert.ToInt32(dr["PatientCount"]) : 0;
                                            if (dr["Status"].ToString() == "ReadyToDischarge")
                                                dashboardPatient.ReadyForDischarge = (!DBNull.Value.Equals(dr["PatientCount"])) ? Convert.ToInt32(dr["PatientCount"]) : 0;
                                            if (dr["Status"].ToString() == "Discharged")
                                                dashboardPatient.Discharged = (!DBNull.Value.Equals(dr["PatientCount"])) ? Convert.ToInt32(dr["PatientCount"]) : 0;
                                            dashboardPatient.Total = (dashboardPatient.Prescribed + dashboardPatient.Enrolled + dashboardPatient.Active + dashboardPatient.InActive +
                                                                      dashboardPatient.OnHold + dashboardPatient.ReadyForDischarge + dashboardPatient.Discharged);

                                        }
                                        statusList.status = dashboardPatient;
                                    }
                                    list.Add(statusList);
                                }


                            }
                        }
                    }
                }

                return list;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public List<DashboardVitalsList> GetDashboardVitalCount(int Days,DateTime ToDate,int UtcOffset, int RoleId, string CreatedBy, string ConnectionString)
        {
            List<DashboardVitalsList> listAll = new List<DashboardVitalsList>();
            List<DashboardVitalsList> list = new List<DashboardVitalsList>();
            try
            {
                using (SqlConnection con = new SqlConnection(ConnectionString))
                {

                    using (SqlCommand command = new SqlCommand("usp_DashboardgetVitalSummary", con))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@Days", Days);
                        command.Parameters.AddWithValue("@ToDate", ToDate);
                        command.Parameters.AddWithValue("@UtcOffset", UtcOffset);
                        command.Parameters.AddWithValue("@RoleId", RoleId);
                        command.Parameters.AddWithValue("@CreatedBy", CreatedBy);
                        con.Open();
                        using (SqlDataAdapter sda = new SqlDataAdapter())
                        {
                            DataSet ds;
                            sda.SelectCommand = command;
                            using (ds = new DataSet())
                            {
                                sda.Fill(ds);

                                List<string> listNames = new List<string>();
                                foreach (DataRow dr in ds.Tables[0].Rows)
                                {
                                    if (!listNames.Contains(dr["VitalName"].ToString()))
                                    {
                                        listNames.Add(dr["VitalName"].ToString());
                                    }

                                }
                               
                                int TotalCritical = 0;
                                int TotalCautious = 0;
                                int TotalNormal = 0;
                                foreach (string na in listNames)
                                {
                                    DashboardVitalsList vitalList = new DashboardVitalsList();
                                    Priority priority = new Priority();

                                    foreach (DataRow dr in ds.Tables[0].Rows)
                                    {
                                        if (na == dr["VitalName"].ToString())
                                        {

                                            vitalList.VitalName = dr["VitalName"].ToString();
                                            if (dr["Priority"].ToString() == "Critical")
                                            {
                                                priority.Critical = (!DBNull.Value.Equals(dr["Total"])) ? Convert.ToInt32(dr["Total"]) : 0;
                                                TotalCritical = TotalCritical + priority.Critical;
                                            }
                                            if (dr["Priority"].ToString() == "Cautious")
                                            {
                                                priority.Cautious = (!DBNull.Value.Equals(dr["Total"])) ? Convert.ToInt32(dr["Total"]) : 0;
                                                TotalCautious = TotalCautious + priority.Cautious;
                                            }

                                            if (dr["Priority"].ToString() == "Normal")
                                            {
                                                priority.Normal = (!DBNull.Value.Equals(dr["Total"])) ? Convert.ToInt32(dr["Total"]) : 0;
                                                TotalNormal = TotalNormal + priority.Normal;
                                            }                                        
                                        }
                                        else
                                        {

                                        }
                                        vitalList.Priorities = priority;
                                    }
                                    list.Add(vitalList);
                                    
                                }                               
                                foreach(DashboardVitalsList listv in list)
                                {
                                    listAll.Add(listv);
                                }
                            }
                        }
                    }
                }

                return listAll;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public List<DashboardAlerts> GetDashboardAlerts(int RoleId,string CreatedBy, string ConnectionString)
        {
            List<DashboardAlerts> dashboardAlerts = new List<DashboardAlerts>();
            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    SqlCommand command = new SqlCommand("usp_DashboardGetCriticalAlerts", connection);
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add("@RoleId", SqlDbType.Int).Value = RoleId;
                    command.Parameters.Add("@CreatedBy", SqlDbType.NVarChar).Value = CreatedBy;
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    var i = 0;
                    while (reader.Read())
                    {

                        DashboardAlerts info = new DashboardAlerts();
                        info.Index = i;
                        info.Id = (!DBNull.Value.Equals(reader["Id"])) ? Convert.ToInt32(reader["Id"]) : 0;
                        info.PatientId = (!DBNull.Value.Equals(reader["PatientId"])) ? Convert.ToInt32(reader["PatientId"]) : 0;
                        info.PatientName = reader["PatientName"].ToString();
                        info.PatientProgramId = (!DBNull.Value.Equals(reader["PatientProgramId"])) ? Convert.ToInt32(reader["PatientProgramId"]) : 0;
                        info.VitalAlert = reader["VitalAlert"].ToString();
                        info.Priority = reader["Priority"].ToString();
                        info.AssignedToCareTeamUserId = (!DBNull.Value.Equals(reader["AssignedToCareTeamUserId"])) ? Convert.ToInt32(reader["AssignedToCareTeamUserId"]) : 0;
                        info.Time = (!DBNull.Value.Equals(reader["Time"])) ? Convert.ToDateTime(reader["Time"]) : DateTime.MinValue;
                        dashboardAlerts.Add(info);
                        i++;
                    }
                }
            }
            catch
            {
                throw;
            }
            return dashboardAlerts;

        }
        public List<DashboardAlertAndTask> GetDashboardTodaysAlertsandTasks(int RoleId, DateTime StartDate, DateTime EndDate, string CreatedBy, string ConnectionString)
        {
            List<DashboardAlertAndTask> dashboardAlerts = new List<DashboardAlertAndTask>();
            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    SqlCommand command = new SqlCommand("usp_DashboardGetTodaysAlertsAndTasks", connection);
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add("@RoleId", SqlDbType.Int).Value = RoleId;
                    command.Parameters.Add("@StartDate", SqlDbType.DateTime).Value = StartDate;
                    command.Parameters.Add("@EndDate", SqlDbType.DateTime).Value = EndDate;
                    command.Parameters.Add("@CreatedBy", SqlDbType.NVarChar).Value = CreatedBy;
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {

                        DashboardAlertAndTask info = new DashboardAlertAndTask();
                        info.Id = (!DBNull.Value.Equals(reader["Id"])) ? Convert.ToInt32(reader["Id"]) : 0;
                        info.PatientId = (!DBNull.Value.Equals(reader["PatientId"])) ? Convert.ToInt32(reader["PatientId"]) : 0;
                        info.PatientName = reader["PatientName"].ToString();
                        info.PatientProgramId = (!DBNull.Value.Equals(reader["PatientProgramId"])) ? Convert.ToInt32(reader["PatientProgramId"]) : 0;
                        info.VitalAlert = reader["VitalAlert"].ToString();
                        info.Priority = reader["Priority"].ToString();
                        info.Status = reader["Status"].ToString();
                        info.DueDate = (!DBNull.Value.Equals(reader["DueDate"])) ? Convert.ToDateTime(reader["DueDate"]) : DateTime.MinValue;
                        dashboardAlerts.Add(info);
                    }
                }
            }
            catch
            {
                throw;
            }
            return dashboardAlerts;

        }
        public List<DashboardTeamOverView> GetDashboardTeamOverview(int RoleId, DateTime StartDate, DateTime EndDate, string CreatedBy, string ConnectionString)
        {
            List<DashboardTeamOverView> teamoverview = new List<DashboardTeamOverView>();
            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    SqlCommand command = new SqlCommand("usp_DashboardGetTeamOverview", connection);
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add("@RoleId", SqlDbType.Int).Value = RoleId;
                    command.Parameters.Add("@StartDate", SqlDbType.DateTime).Value = StartDate;
                    command.Parameters.Add("@EndDate", SqlDbType.DateTime).Value = EndDate;
                    command.Parameters.Add("@CreatedBy", SqlDbType.NVarChar).Value = CreatedBy;
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {

                        DashboardTeamOverView info = new DashboardTeamOverView();
                        info.TeamName = reader["TeamName"].ToString();
                        info.CareTeamId = (!DBNull.Value.Equals(reader["CareTeamId"])) ? Convert.ToInt32(reader["CareTeamId"]) : 0;
                        info.Alerts = (!DBNull.Value.Equals(reader["Alerts"])) ? Convert.ToInt32(reader["Alerts"]) : 0;
                        info.DueToday = (!DBNull.Value.Equals(reader["DueToday"])) ? Convert.ToInt32(reader["DueToday"]) : 0;
                        info.SLABreached = (!DBNull.Value.Equals(reader["SLABreached"])) ? Convert.ToInt32(reader["SLABreached"]) : 0;
                        teamoverview.Add(info);
                    }
                }
            }
            catch
            {
                throw;
            }
            return teamoverview;

        }
    }
}

