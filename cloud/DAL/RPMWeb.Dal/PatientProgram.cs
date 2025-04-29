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
    public sealed class PatientProgram
    {
        public int SavePatientProgram(PatientProgramDetailsInsert data, string ConnectionString)
        {

            try
            {
			//ccm change
                string ProgramVitals = "INSERT INTO  PatientProgramVitals([PatientProgramId],[ProgramId],[VitalId],[CreatedBy])values";
                string ProgramVitalsinserts = string.Empty;
                List<int> vitalids = data.VitalIds;
                string ProgramVitalsInput = string.Empty;
                if(vitalids!=null)
                if (vitalids.Count > 0)
                {
                    foreach (int vitalId in vitalids)
                    {
                        string insertvalues = "('PATIENTPROGRAMIDXXX'," + data.ProgramId + "," + vitalId + ",'" + data.CreatedBy + "'),";
                        ProgramVitalsinserts = ProgramVitalsinserts + insertvalues;
                    }
                    string scriptV = ProgramVitals + ProgramVitalsinserts;
                    ProgramVitalsInput = scriptV.Substring(0, scriptV.Length - 1);
                }
                string ProgramDiagnostics = "INSERT INTO  PatientProgramDiagnostics([PatientProgramId],[DiagnosticsName],[DiagnosticsCode],[CreatedBy])values";
                string ProgramDiagnosticsinserts = string.Empty;
                ProgramDiagnostics[] details2 = data.ProgramDiagnosis;
                foreach (ProgramDiagnostics details in details2)
                {
                    string insertvalues = "('PATIENTPROGRAMIDXXX','" + details.DiagnosisName + "','" + details.DiagnosisCode + "','" + data.CreatedBy + "'),";
                    ProgramDiagnosticsinserts = ProgramDiagnosticsinserts + insertvalues;
                }
                string script2 = ProgramDiagnostics + ProgramDiagnosticsinserts;
                string ProgramDiagnosticsInput = script2.Substring(0, script2.Length - 1);
                string ProgramGoals = "INSERT INTO PatientProgramGoals([PatientProgramId],[Goal],[Description],[CreatedBy])VALUES";
                string ProgramGoalsinserts = string.Empty;
                GoalDetails[] details1 = data.GoalDetails;
                foreach (GoalDetails details in details1)
                {
                    string insertvalues = "('PATIENTPROGRAMIDXXX','" + details.Goal + "','" + details.Description + "','" + data.CreatedBy + "'),";
                    ProgramGoalsinserts = ProgramGoalsinserts + insertvalues;
                }
                string script = ProgramGoals + ProgramGoalsinserts;
                string ProgramGoalsInput = script.Substring(0, script.Length - 1);
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    SqlCommand command = new SqlCommand("usp_InsPatientProgram", connection);
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@PatientId", data.PatientId);
                    command.Parameters.AddWithValue("@ProgramId", data.ProgramId);
                    command.Parameters.AddWithValue("@PhysicianId", data.PhysicianId);
                    command.Parameters.AddWithValue("@CareTeamUserId", data.CareTeamUserId);
                    command.Parameters.AddWithValue("@PatientStatus", data.PatientStatus);
                    command.Parameters.AddWithValue("@PrescribedDate", data.PrescribedDate);
                    command.Parameters.AddWithValue("@TargetReadings", data.TargetReadings);
                    command.Parameters.AddWithValue("@ConsultationDate", data.ConsultationDate);
                    command.Parameters.AddWithValue("@StartDate", data.StartDate);
                    command.Parameters.AddWithValue("@EndDate", data.EndDate);
                    command.Parameters.AddWithValue("@VitalIds", ProgramVitalsInput);//ccm change
                    command.Parameters.AddWithValue("@ProgramDiagnostics", ProgramDiagnosticsInput);
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
                        return id;
                    }
                    return id;
                }
            }
            catch
            {
                throw;
            }
        }

        public int AddNewPatientProgram(PatientProgramDetailsInsert data, string ConnectionString)
        {

            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    SqlCommand command = new SqlCommand("usp_UdpPatientProgramStatus", connection);
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@PatientId", data.PatientId);
                    command.Parameters.AddWithValue("@PatientProgramId", data.PatientProgramId);
                    command.Parameters.AddWithValue("@ModifiedBy", data.CreatedBy);
                    SqlParameter returnParameter = command.Parameters.Add("RetVal", SqlDbType.Int);
                    returnParameter.Direction = ParameterDirection.ReturnValue;
                    connection.Open();
                    command.ExecuteNonQuery();
                    int sucess = (int)returnParameter.Value;
                    connection.Close();
                    if (sucess == 1)
                    {
					//ccm change
                        string ProgramVitals = "INSERT INTO  PatientProgramVitals([PatientProgramId],[ProgramId],[VitalId],[CreatedBy])values";
                        string ProgramVitalsinserts = string.Empty;
                        List<int> vitalids = data.VitalIds;
                        string ProgramVitalsInput = string.Empty;
                        if (vitalids != null)
                            if (vitalids.Count > 0)
                            {
                                foreach (int vitalId in vitalids)
                                {
                                    string insertvalues = "('PATIENTPROGRAMIDXXX'," + data.ProgramId + "," + vitalId + ",'" + data.CreatedBy + "'),";
                                    ProgramVitalsinserts = ProgramVitalsinserts + insertvalues;
                                }
                                string scriptV = ProgramVitals + ProgramVitalsinserts;
                                ProgramVitalsInput = scriptV.Substring(0, scriptV.Length - 1);
                            }
                        string ProgramDiagnostics = "INSERT INTO  PatientProgramDiagnostics([PatientProgramId],[DiagnosticsName],[DiagnosticsCode],[CreatedBy])values";
                        string ProgramDiagnosticsinserts = string.Empty;
                        ProgramDiagnostics[] details2 = data.ProgramDiagnosis;
                        foreach (ProgramDiagnostics details in details2)
                        {
                            string insertvalues = "('PATIENTPROGRAMIDXXX','" + details.DiagnosisName + "','" + details.DiagnosisCode + "','" + data.CreatedBy + "'),";
                            ProgramDiagnosticsinserts = ProgramDiagnosticsinserts + insertvalues;
                        }
                        string script2 = ProgramDiagnostics + ProgramDiagnosticsinserts;
                        string ProgramDiagnosticsInput = script2.Substring(0, script2.Length - 1);
                        string ProgramGoals = "INSERT INTO PatientProgramGoals([PatientProgramId],[Goal],[Description],[CreatedBy])VALUES";
                        string ProgramGoalsinserts = string.Empty;
                        GoalDetails[] details1 = data.GoalDetails;
                        foreach (GoalDetails details in details1)
                        {
                            string insertvalues = "('PATIENTPROGRAMIDXXX','" + details.Goal + "','" + details.Description + "','" + data.CreatedBy + "'),";
                            ProgramGoalsinserts = ProgramGoalsinserts + insertvalues;
                        }
                        string script = ProgramGoals + ProgramGoalsinserts;
                        string ProgramGoalsInput = script.Substring(0, script.Length - 1);
                        using (SqlConnection connection1 = new SqlConnection(ConnectionString))
                        {
                            SqlCommand command1 = new SqlCommand("usp_InsPatientProgram", connection1);
                            command1.CommandType = CommandType.StoredProcedure;
                            command1.Parameters.AddWithValue("@PatientId", data.PatientId);
                            command1.Parameters.AddWithValue("@ProgramId", data.ProgramId);
                            command1.Parameters.AddWithValue("@VitalIds", ProgramVitalsInput);//ccm change
                            command1.Parameters.AddWithValue("@PhysicianId", data.PhysicianId);
                            command1.Parameters.AddWithValue("@CareTeamUserId", data.CareTeamUserId);
                            command1.Parameters.AddWithValue("@PatientStatus", data.PatientStatus);
                            command1.Parameters.AddWithValue("@PrescribedDate", data.PrescribedDate);
                            command1.Parameters.AddWithValue("@TargetReadings", data.TargetReadings);
                            command1.Parameters.AddWithValue("@ConsultationDate", data.ConsultationDate);
                            command1.Parameters.AddWithValue("@StartDate", data.StartDate);
                            command1.Parameters.AddWithValue("@EndDate", data.EndDate);
                            command1.Parameters.AddWithValue("@ProgramDiagnostics", ProgramDiagnosticsInput);
                            command1.Parameters.AddWithValue("@ProgramGoals", ProgramGoalsInput);
                            command1.Parameters.AddWithValue("@CreatedBy", data.CreatedBy);
                            SqlParameter returnParameter1 = command1.Parameters.Add("RetVal", SqlDbType.Int);
                            returnParameter1.Direction = ParameterDirection.ReturnValue;
                            connection1.Open();
                            command1.ExecuteNonQuery();
                            int id = (int)returnParameter1.Value;
                            connection1.Close();
                            if (id.Equals(0))
                            {
                                return id;
                            }
                            return id;
                        }
                    }
                    else
                        return 0;
                }
            }
            catch
            {
                throw;
            }
        }
        
        public int UpdateProgram(UpdateProgramDetails data, string ConnectionString)
        {

            try
            {
                DataSet ds;
                List<int> newVitals;
                List<int> deletedVitals;
                string ProgramVitalsNew = "INSERT INTO  #tempNewVitals([PatientProgramId],[VitalId])values";
                string ProgramVitalsDel = "INSERT INTO  #tempDeletedVitals ([PatientProgramId],[VitalId])values";
                string ProgramVitalsNewinserts = string.Empty;
                string ProgramVitalsDeletedinserts = string.Empty;
                List<int> vitalids = data.VitalIds;
                string ProgramVitalsNewInput = string.Empty;
                string ProgramVitalsDeletedInput = string.Empty;
                if (vitalids != null)
                    if (vitalids.Count > 0)
                    {
                        using (SqlConnection con = new SqlConnection(ConnectionString))
                        {
                            con.Open();
                            using (SqlCommand command = new SqlCommand("usp_GetPatientProgramVitals", con))
                            {
                                command.CommandType = CommandType.StoredProcedure;
                                command.Parameters.AddWithValue("@PatientProgramId", data.PatientProgramId);
                                using (SqlDataAdapter sda = new SqlDataAdapter())
                                {
                                    sda.SelectCommand = command;
                                    using (ds = new DataSet())
                                    {
                                        sda.Fill(ds);
                                        List<int> oldVitals = ds.Tables[0].AsEnumerable().Select(s => s.Field<int>("VitalId")).Distinct().ToList();
                                        newVitals = vitalids.Except(oldVitals).ToList();
                                        deletedVitals = oldVitals.Except(vitalids).ToList();
                                    }
                                }
                            }
                        }
                        foreach (int vitalId in newVitals)
                        {
                            string insertvalues = "(" + data.PatientProgramId  + "," + vitalId + "),";
                            ProgramVitalsNewinserts += insertvalues;
                        }
                        foreach (int vitalId in deletedVitals)
                        {
                            string insertvalues = "(" + data.PatientProgramId +  "," + vitalId + "),";
                            ProgramVitalsDeletedinserts  += insertvalues;
                        }
                        string scriptV = ProgramVitalsNew + ProgramVitalsNewinserts;
                        ProgramVitalsNewInput = scriptV.Substring(0, scriptV.Length - 1);
                        scriptV = ProgramVitalsDel + ProgramVitalsDeletedinserts;
                        ProgramVitalsDeletedInput = scriptV.Substring(0, scriptV.Length - 1);
                        if (newVitals.Count == 0)
                        {
                            ProgramVitalsNewInput = string.Empty;
                        }
                        if (deletedVitals.Count == 0)
                        {
                            ProgramVitalsDeletedInput = string.Empty;
                        }
                    }
                string ProgramDiagnostics = "INSERT INTO  PatientProgramDiagnostics([PatientProgramId],[DiagnosticsName],[DiagnosticsCode],[CreatedBy])values";
                string ProgramDiagnosticsinserts = string.Empty;
                string ProgramDiagnosticsInput = string.Empty;
                ProgramDiagnostics[] details2 = data.ProgramDiagnosis;
                if (details2.Length > 0)
                {
                    foreach (ProgramDiagnostics details in details2)
                    {
                        string insertvalues = "(" + data.PatientProgramId + ",'" + details.DiagnosisName + "','" + details.DiagnosisCode + "','" + data.CreatedBy + "'),";
                        ProgramDiagnosticsinserts = ProgramDiagnosticsinserts + insertvalues;
                    }
                    string script2 = ProgramDiagnostics + ProgramDiagnosticsinserts;
                    ProgramDiagnosticsInput = script2.Substring(0, script2.Length - 1);
                }

                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    SqlCommand command = new SqlCommand("usp_UpdPatientProgramDetailsRenew", connection);
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@PatientId", data.PatientId);
                    command.Parameters.AddWithValue("@ProgramId", data.ProgramId);
                    command.Parameters.AddWithValue("@VitalIdsNew", ProgramVitalsNewInput);
                    command.Parameters.AddWithValue("@VitalIdsDeleted", ProgramVitalsDeletedInput);
                    command.Parameters.AddWithValue("@ProgramDiagnostics", ProgramDiagnosticsInput);
                    command.Parameters.AddWithValue("@PatientProgramId", data.PatientProgramId);
                    command.Parameters.AddWithValue("@CreatedBy", data.CreatedBy);
                    SqlParameter returnParameter = command.Parameters.Add("RetVal", SqlDbType.Int);
                    returnParameter.Direction = ParameterDirection.ReturnValue;
                    connection.Open();
                    command.ExecuteNonQuery();
                    int id = (int)returnParameter.Value;
                    connection.Close();
                    if (id.Equals(0))
                    {
                        return id;
                    }
                    return id;
                }
            }
            catch
            {
                throw;
            }
        }
        
        public int RenewPatientProgram(PatientProgramRenew Info, string ConnectionString)
        {
            int ret = 0;
            try
            {
                using (SqlConnection connection1 = new SqlConnection(ConnectionString))
                {
                    SqlCommand comm = new SqlCommand("usp_GetPatientlastestUsedDeviceAndDetails", connection1);
                    comm.CommandType = CommandType.StoredProcedure;
                    comm.Parameters.Add("@PatientProgramId", SqlDbType.Int).Value = Info.PatientProgramId;
                    comm.Parameters.Add("@CreatedBy", SqlDbType.NVarChar).Value = "User";
                    connection1.Open();
                    SqlDataReader reader = comm.ExecuteReader();
                    ReRegisterDevice reg = new ReRegisterDevice();
                    while (reader.Read())
                    {
                        reg.PatientId = (!DBNull.Value.Equals(reader["PatientId"])) ? Convert.ToInt32(reader["PatientId"]) : 0;
                        reg.PatientNumber = reader.IsDBNull(reader.GetOrdinal("PatientNumber")) ? string.Empty : reader["PatientNumber"].ToString();
                        reg.FirstName = reader.IsDBNull(reader.GetOrdinal("FirstName")) ? string.Empty : reader["FirstName"].ToString();
                        reg.LastName = reader.IsDBNull(reader.GetOrdinal("LastName")) ? string.Empty : reader["LastName"].ToString();
                        reg.DeviceSerialNo = reader.IsDBNull(reader.GetOrdinal("DeviceSerialNo")) ? string.Empty : reader["DeviceSerialNo"].ToString();
                        reg.TimeZoneOffset = (!DBNull.Value.Equals(reader["TimeZone"])) ? Convert.ToInt32(reader["TimeZone"]) : 0;
                        reg.VendorId = (!DBNull.Value.Equals(reader["VendorId"])) ? Convert.ToInt32(reader["VendorId"]) : 0;
                        reg.DeviceModel = reader.IsDBNull(reader.GetOrdinal("DeviceModel")) ? string.Empty : reader["DeviceModel"].ToString();
                    }
                    connection1.Close();
                    if (reg != null)
                    {
                        if (reg.DeviceSerialNo == "")
                        {
                            int Id = RenewPatientProgramAfterDevActivation(Info, ConnectionString);
                            if (Id != 0)
                            {
                                ret = Id;
                            }
                        }
                        else
                        {
                            RegisterDeviceResponse resp = ReactivateDevice(reg, ConnectionString);
                            if (resp.HttpRetCode == 200)
                            {
                                int Id = RenewPatientProgramAfterDevActivation(Info, ConnectionString);
                                if (Id != 0)
                                {
                                    ret = Id;
                                }
                            }
                        }
                        
                    }
                }
                return ret;
            }
            catch (Exception ex)
            {
                throw(ex);
            }
        }
        public RegisterDeviceResponse ReactivateDevice( ReRegisterDevice reg, string ConnectionString)
        {
            RegisterDeviceResponse response = new RegisterDeviceResponse();
            response.HttpRetCode = 400;
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
                    cmd.Parameters.Add("@PatientId", SqlDbType.Int).Value = reg.PatientId;
                    cmd.Parameters.Add("@DeviceId", SqlDbType.NVarChar).Value = reg.DeviceSerialNo;
                    cmd.Parameters.Add("@ConnectionStatus", SqlDbType.NVarChar).Value = "Active";
                    cmd.Parameters.Add("@ModifiedBy", SqlDbType.NVarChar).Value = "sa";
                    con.Open();
                    cmd.ExecuteNonQuery();
                    con.Close();
                }
                response.HttpRetCode = 200;
                response.DevicUserId = reg.PatientNumber;
                response.Message = "Device set to active without vendor activation.";
                return response;
            }
           
            PatientDeviceConnectivityInfo pdci = null;
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                SqlCommand comm = new SqlCommand("usp_GetPatientVendorConnectivity", connection);
                comm.CommandType = CommandType.StoredProcedure;
                comm.Parameters.Add("@PatientId", SqlDbType.Int).Value = reg.PatientId;
                comm.Parameters.Add("@VendorId", SqlDbType.Int).Value = reg.VendorId;
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
                  
            RpmWebHandler webhandler = new RpmWebHandler();
            if (pdci == null)
            {
                DeviceRegisterRequest req = new DeviceRegisterRequest();
                req.device_id = reg.DeviceSerialNo;
                req.username = reg.PatientNumber;
                req.first_name = reg.FirstName;
                req.last_name = reg.LastName;
                req.time_zone = reg.TimeZoneOffset;
                int nDevUserId = 0;
                HttpResponse resp = webhandler.SendRegisterRequest(req, igc, ref nDevUserId);
                if (resp != null && resp.HttpRetCode == 201)
                {
                    using (SqlConnection connection = new SqlConnection(ConnectionString))
                    {
                        SqlCommand comm = new SqlCommand("[usp_InsPatientVendorConnectivity]", connection);
                        comm.CommandType = CommandType.StoredProcedure;
                        comm.Parameters.Add("@PatientId", SqlDbType.Int).Value = reg.PatientId;
                        comm.Parameters.Add("@VendorId", SqlDbType.Int).Value = reg.VendorId;
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
                            response.DevicUserId = nDevUserId.ToString();
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
                response = webhandler.AddDeviceRequest(reg.DeviceSerialNo, pdci.DeviceVendorUserid, reg.DeviceModel, igc);
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
                    cmd.Parameters.Add("@PatientId", SqlDbType.Int).Value = reg.PatientId;
                    cmd.Parameters.Add("@DeviceId", SqlDbType.NVarChar).Value = reg.DeviceSerialNo;
                    cmd.Parameters.Add("@ConnectionStatus", SqlDbType.NVarChar).Value = devStatus;
                    cmd.Parameters.Add("@ModifiedBy", SqlDbType.NVarChar).Value = "sa";
                    con.Open();
                    cmd.ExecuteNonQuery();
                    con.Close();
                           
                }
            }
            return response;
        }
        public int RenewPatientProgramAfterDevActivation(PatientProgramRenew Info, string ConnectionString)
        {
            int ret = 0;
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                SqlCommand command = new SqlCommand("usp_InsRenewPatientProgram", connection);
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@PatientId", Info.PatientId);
                command.Parameters.AddWithValue("@PatientProgramId", Info.PatientProgramId);
                command.Parameters.Add("@StartDate", SqlDbType.SmallDateTime).Value = Info.StartDate;
                command.Parameters.Add("@EndDate", SqlDbType.SmallDateTime).Value = Info.EndDate;
                command.Parameters.AddWithValue("@CreatedBy", Info.CreatedBy);
                SqlParameter returnParameter = command.Parameters.Add("RetVal", SqlDbType.Int);
                returnParameter.Direction = ParameterDirection.ReturnValue;
                connection.Open();
                command.ExecuteNonQuery();
                int id = (int)returnParameter.Value;
                connection.Close();
                if (id != 0)
                {
                    ret = id;
                }
                return ret;
            }
        }
        public bool UpdatePatientProgram(PatientProgramDetailsUpdate data, string ConnectionString)
        {
            bool ret = true;
            try
            {
			//ccm change
                string ProgramVitals = "INSERT INTO  PatientProgramVitals([PatientProgramId],[ProgramId],[VitalId],[CreatedBy])values";
                string ProgramVitalsinserts = string.Empty;
                List<int> vitalids = data.VitalIds;
                string ProgramVitalsInput = string.Empty;
                if (vitalids != null)
                    if (vitalids.Count > 0)
                    {
                        foreach (int vitalId in vitalids)
                        {
                            string insertvalues = "('PATIENTPROGRAMIDXXX','PROGRAMIDXXX',"  + vitalId + ",'" + data.CreatedBy + "'),";
                            ProgramVitalsinserts = ProgramVitalsinserts + insertvalues;
                        }
                        string scriptV = ProgramVitals + ProgramVitalsinserts;
                        ProgramVitalsInput = scriptV.Substring(0, scriptV.Length - 1);
                    }
                //ProgramGoalupdate
                string ProgramGoals = "insert into @@#PatientProgramGoals([Id],[PatientProgramId],[Goal],[Description],[CreatedBy])values";
                string ProgramGoalsinserts = string.Empty;
                string ProgramDiagnosticsInput = string.Empty;
                GoalDetails[] goalDetails = data.patientProgramGoals;
                string ProgramGoalsInput = string.Empty;
                if (goalDetails.Length > 0)
                {
                    foreach (GoalDetails details in goalDetails)
                    {
                        string insertvalues = "('" + details.Id + "','" + data.PatientProgramId + "','"
                            + details.Goal + "','" + details.Description + "','" + data.CreatedBy + "'),";
                        ProgramGoalsinserts = ProgramGoalsinserts + insertvalues;
                    }
                    string script = ProgramGoals + ProgramGoalsinserts;
                    ProgramGoalsInput = script.Substring(0, script.Length - 1);
                }
                

                //ProgramDiagnosticsupdate
                string ProgramDiagnostics = "insert into @@#PatientProgramDiagnostics(Id,PatientProgramId,DiagnosticsName,DiagnosticsCode,CreatedBy) values";
                string ProgramDiagnosticsinserts = string.Empty;
                ProgramDiagnostics[] programDiagnostics = data.PatientProgramDiagnosis;
                if (programDiagnostics.Length > 0)
                {
                    foreach (ProgramDiagnostics details in programDiagnostics)
                    {
                        string insertvalues = "('" + details.Id + "','" + data.PatientProgramId +
                            "','" + details.DiagnosisName + "','" + details.DiagnosisCode +
                            "','" + data.CreatedBy + "'),";
                        ProgramDiagnosticsinserts = ProgramDiagnosticsinserts + insertvalues;
                    }
                    string script2 = ProgramDiagnostics + ProgramDiagnosticsinserts;
                     ProgramDiagnosticsInput = script2.Substring(0, script2.Length - 1);
                }
                   

                //ProgramDevicesupdate
                //string ProgramDevices = "insert into @@#Device(Id,PatientId)values";
                //string ProgramDeviesinserts = string.Empty;
                //PatientDeviceDetails[] patientDeviceDetails = data.PatientDeviceDetails;
                //foreach (PatientDeviceDetails details in patientDeviceDetails)
                //{
                //    string insertvalues = "('" + details.Id + "','" + data.PatientId + "'),";
                //    ProgramDeviesinserts = ProgramDeviesinserts + insertvalues;
                //}
                //string script3 = ProgramDevices + ProgramDeviesinserts;
                //string ProgramDevicesInput = script3.Substring(0, script3.Length - 1);

                //PatientVitalDetailsUpdate
                string PatientVitalDetails = "insert into @@#PatientProgramDetails (ID,PatientProgramId,DeviceVitalMeasuresId," +
                    "ScheduleId,VitalScheduleId,NoOfTimes, Morning, Afternoon, Evening, Night, MeasureName, MeasureOrder, NormalMinimum," +
                    " NormalMaximum, CautiousMinimum, CautiousMaximum, criticallMinimum, criticalMaximum, CreatedBy)values";
                string PatientVitalDetailsinserts = string.Empty;
                string ProgramVitalDetailsInput = string.Empty;
                PatientVitalDetails[] patientVitalDetails = data.PatientVitalDetails;
                if (patientVitalDetails.Length > 0)
                {
                    foreach (PatientVitalDetails Details in patientVitalDetails)
                    {
                        foreach (VitalMeasureInfos details in Details.VitalMeasureInfos)
                        {
                            string insertvalues =
                                "('" + details.Id + "','"
                                + data.PatientProgramId + "','"
                                + details.DeviceVitalMeasuresId + "','"
                                + Details.ScheduleId + "','"
                                + Details.VitalScheduleId + "','"
                                + Details.NoOfTimes + "','"
                                + Details.Morning + "','"
                                + Details.Afternoon + "','"
                                + Details.Evening + "','"
                                + Details.Night + "','"
                                + details.MeasureName + "','"
                                + details.MeasureOrder + "','"
                                + details.NormalMinimum + "','"
                                + details.NormalMaximum + "','"
                                + details.CautiousMinimum + "','"
                                + details.CautiousMaximum + "','"
                                + details.criticallMinimum + "','"
                                + details.criticalMaximum + "','"
                                + data.CreatedBy + "'),";
                            PatientVitalDetailsinserts = PatientVitalDetailsinserts + insertvalues;
                        }
                    }
                    string script4 = PatientVitalDetails + PatientVitalDetailsinserts;
                    ProgramVitalDetailsInput = script4.Substring(0, script4.Length - 1);
                }

               

                //ProgramInsurenceupdate
                string ProgramInsurence = string.Empty;
                string ProgramInsurenceinserts = string.Empty;
                string ProgramInsurenceInput = string.Empty;
                PatientInsurenceDetails[] patientInsurenceDetails = data.PatientInsurenceDetails;
                if (patientInsurenceDetails.Length > 0)
                {
                    ProgramInsurence = "insert into @@#PatientProgramInsurance([Id],[PatientProgramId]," +
                   "[InsuranceVendorName],[IsPrimary],[CreatedBy])values";

                    foreach (PatientInsurenceDetails details in patientInsurenceDetails)
                    {
                        string insertvalues = "('" + details.Id + "','" + data.PatientProgramId + "','"
                            + details.InsuranceVendorName + "','" + details.IsPrimary + "','" + data.CreatedBy + "'),";
                        ProgramInsurenceinserts = ProgramInsurenceinserts + insertvalues;
                    }
                    string script5 = ProgramInsurence + ProgramInsurenceinserts;
                    ProgramInsurenceInput = script5.Substring(0, script5.Length - 1);
                }
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    SqlCommand command = new SqlCommand("usp_updPatientProgram", connection);
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@PatientId", data.PatientId);
                    command.Parameters.AddWithValue("@PatientProgramId", data.PatientProgramId);
                    command.Parameters.AddWithValue("@PhysicianId", data.PhysicianId);
                    command.Parameters.AddWithValue("@ConsultationDate", data.ConsultationDate);
                    command.Parameters.AddWithValue("@CareTeamUserId", data.CareTeamUserId);
                    command.Parameters.AddWithValue("@StartDate", data.StartDate);
                    command.Parameters.AddWithValue("@EndDate", data.EndDate);
                    command.Parameters.AddWithValue("@PatientStatus", data.PatientStatus);
                    command.Parameters.AddWithValue("@TargetReadings", data.TargetReadings);
                    command.Parameters.AddWithValue("@VitalIds", ProgramVitalsInput);//ccm change
                    command.Parameters.AddWithValue("@CreatedBy", data.CreatedBy);
                    command.Parameters.AddWithValue("@PatientProgramGoals", ProgramGoalsInput);
                    command.Parameters.AddWithValue("@PatientProgramDiagnostics", ProgramDiagnosticsInput);
                    command.Parameters.AddWithValue("@PatientProgramDetails", ProgramVitalDetailsInput);
                    command.Parameters.AddWithValue("@PrescribedDate", data.PrescribedDate);
                    command.Parameters.AddWithValue("@EnrolledDate", data.EnrolledDate);
                    //command.Parameters.AddWithValue("@Deviceupdate", ProgramDevicesInput);
                    command.Parameters.AddWithValue("@PatientProgramInsurance", ProgramInsurenceInput);

                    SqlParameter returnParameter = command.Parameters.Add("RetVal", SqlDbType.Int);
                    returnParameter.Direction = ParameterDirection.ReturnValue;
                    connection.Open();
                    command.ExecuteNonQuery();
                    int id = (int)returnParameter.Value;
                    connection.Close();

                    if(data.PatientStatus== "Discharged")
                    {
                        
                        firebasenotificationmessage notify = new firebasenotificationmessage();
                        notify.title = "Discharge Patient";
                        notify.body = "Discharge Successful";
                        string category = "system";
                       // new Notification().GetFirebaseNotificationByUser(UserName, notify, category);
                    }


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
        public List<PatientProgramDetails> GetAllPatientProgramDetails(string ConnectionString)
        {
            List<PatientProgramDetails> ret = new List<PatientProgramDetails>();
            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    SqlCommand command = new SqlCommand("usp_GetActivePatientProgramDuration", connection);
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add("@CreatedBy", SqlDbType.NVarChar).Value = "System";
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        PatientProgramDetails info = new PatientProgramDetails();
                        info.Id = (!DBNull.Value.Equals(reader["Id"])) ? Convert.ToInt32(reader["Id"]) : 0;
                        info.PatientUserName = (!DBNull.Value.Equals(reader["PatientUserName"])) ? reader["PatientUserName"].ToString() : string.Empty;
                        info.PatientFirstName = (!DBNull.Value.Equals(reader["FirstName"])) ? reader["FirstName"].ToString() : string.Empty;
                        info.PatientLastName = (!DBNull.Value.Equals(reader["LastName"])) ? reader["LastName"].ToString() : string.Empty;
                        info.CareTeamMemberId = (!DBNull.Value.Equals(reader["CareTeamUserId"])) ? Convert.ToInt32(reader["CareTeamUserId"]) : 0;
                        info.CareTeamMemberUserName = (!DBNull.Value.Equals(reader["CareTeamUserName"])) ? reader["CareTeamUserName"].ToString() : string.Empty;
                        info.ProgramStartDate = (!DBNull.Value.Equals(reader["StartDate"])) ? Convert.ToDateTime(reader["StartDate"]) : DateTime.MinValue;
                        info.ProgramEndDate = (!DBNull.Value.Equals(reader["EndDate"])) ? Convert.ToDateTime(reader["EndDate"]) : DateTime.MinValue;
                        info.ProgramStatus = (!DBNull.Value.Equals(reader["Status"])) ? reader["Status"].ToString() : string.Empty;
                        ret.Add(info);
                    }
                }
            }
            catch
            {
                throw;
            }
            return ret;

        }

        public List<PatientAllPrograms> GetAllPatientPrograms(int PatientId, string UserName, string ConnectionString)
        {
            List<PatientAllPrograms> ret = new List<PatientAllPrograms>();
            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    SqlCommand command = new SqlCommand("usp_GetAllPatientPrograms", connection);
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add("@PatientId", SqlDbType.NVarChar).Value = PatientId;
                    command.Parameters.Add("@CreatedBy", SqlDbType.NVarChar).Value = UserName;
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        PatientAllPrograms info = new PatientAllPrograms();
                        info.PatientId = (!DBNull.Value.Equals(reader["PatientId"])) ? Convert.ToInt32(reader["PatientId"]) : 0;
                        info.PatientProgramId = (!DBNull.Value.Equals(reader["PatientProgramId"])) ? Convert.ToInt32(reader["PatientProgramId"]) : 0;
                        info.ProgramId = (!DBNull.Value.Equals(reader["ProgramId"])) ? Convert.ToInt32(reader["ProgramId"]) : 0;
                        info.ProgramName = (!DBNull.Value.Equals(reader["ProgramName"])) ? reader["ProgramName"].ToString() : string.Empty;
                        info.IsActive = (!DBNull.Value.Equals(reader["IsActive"])) ? Convert.ToBoolean(reader["IsActive"]) : false;
                        info.StartDate = (!DBNull.Value.Equals(reader["StartDate"])) ? Convert.ToDateTime(reader["StartDate"]) : DateTime.MinValue;
                        ret.Add(info);
                    }
                }
            }
            catch
            {
                throw;
            }
            return ret;

        }
        public int ActivePatientsProgramVitalAddition(PatientProgramDetailsInsertActivePatients data, string ConnectionString)
        {

            try
            {
                string ProgramVitals = "INSERT INTO  PatientProgramVitals([PatientProgramId],[ProgramId],[VitalId],[CreatedBy])values";
                string ProgramVitalsinserts = string.Empty;
                List<int> vitalids = data.VitalIds;
                string ProgramVitalsInput = string.Empty;
                if (vitalids != null)
                    if (vitalids.Count > 0)
                    {
                        foreach (int vitalId in vitalids)
                        {
                            string insertvalues = "('"+ data .PatientProgramId+ "'," + data.ProgramId + "," + vitalId + ",'" + data.CreatedBy + "'),";
                            ProgramVitalsinserts += insertvalues;
                        }
                        string scriptV = ProgramVitals + ProgramVitalsinserts;
                        ProgramVitalsInput = scriptV.Substring(0, scriptV.Length - 1);
                    }
                string ProgramDiagnostics = "INSERT INTO  PatientProgramDiagnostics([PatientProgramId],[DiagnosticsName],[DiagnosticsCode],[CreatedBy])values";
                string ProgramDiagnosticsinserts = string.Empty;
                ProgramDiagnostics[] details2 = data.ProgramDiagnosis;
                foreach (ProgramDiagnostics details in details2)
                {
                    string insertvalues = "('" + data.PatientProgramId + "','" + details.DiagnosisName + "','" + details.DiagnosisCode + "','" + data.CreatedBy + "'),";
                    ProgramDiagnosticsinserts += insertvalues;
                }
                string script2 = ProgramDiagnostics + ProgramDiagnosticsinserts;
                string ProgramDiagnosticsInput = script2.Substring(0, script2.Length - 1);
                string ProgramGoals = "INSERT INTO PatientProgramGoals([PatientProgramId],[Goal],[Description],[CreatedBy])VALUES";
                string ProgramGoalsinserts = string.Empty;
                GoalDetails[] details1 = data.GoalDetails;
                foreach (GoalDetails details in details1)
                {
                    string insertvalues = "('" + data.PatientProgramId + "','" + details.Goal + "','" + details.Description + "','" + data.CreatedBy + "'),";
                    ProgramGoalsinserts += insertvalues;
                }
                string script = ProgramGoals + ProgramGoalsinserts;
                string ProgramGoalsInput = script.Substring(0, script.Length - 1);
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    SqlCommand command = new SqlCommand("usp_InsPatientProgramVitalAddActivePatient", connection);
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@PatientId", data.PatientId);
                    command.Parameters.AddWithValue("@PatientProgramId", data.PatientProgramId);
                    command.Parameters.AddWithValue("@VitalIds", ProgramVitalsInput);
                    command.Parameters.AddWithValue("@ProgramDiagnostics", ProgramDiagnosticsInput);
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
                        return id;
                    }
                    return id;
                }
            }
            catch
            {
                throw;
            }
        }
    }
}

