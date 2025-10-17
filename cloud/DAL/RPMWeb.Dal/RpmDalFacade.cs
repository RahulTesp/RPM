using DocumentFormat.OpenXml.Drawing.Diagrams;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;
using RPMWeb.Data.Common;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO.Packaging;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using static RPMWeb.Dal.Notes;



namespace RPMWeb.Dal
{
    public static class RpmDalFacade
    {
        public static string ConnectionString { get; set; }
        public static string IsSessionValid(string token)
        {            
            return new Authorize().IsSessionValid(token, ConnectionString);            
        }
        public static int GetBearerId(string token)
        {
            return new Authorize().getBearerId(token, ConnectionString);
        }
        public static string getUserName(int id)
        {
            return new Authorize().getUserName(id, ConnectionString);
        }
        public static bool IsNewSessionValid(string token,string userName)
        {
            return new Authorize().IsNewSessionValid(token,userName, ConnectionString);
        }
        public static PatientData IsPatientSessionValid(string JwtToken)
        {
            return new Authorize().IsPatientSessionValid(JwtToken, ConnectionString);
        }
        public static bool LockUser(string Username)
        {
            return new Authorize().LockUser(Username, ConnectionString);
        }
        public static int UpdateRetryCount(string Username)
        {
            return new Authorize().UpdateRetryCount(Username, ConnectionString);
        }
        public static int UpdateLoginDetails(string Username)
        {
            return new Authorize().UpdateLoginDetails(Username, ConnectionString);
        }
        public static bool ValidateTkn(string token)
        {
            Authorize auth = new Authorize();
            return auth.ValidateTkn(token);
        }
        public static LoginDetails VerifyOtp(string otp,string username)
        {
            Authorize auth = new Authorize();
            return auth.VerifyOtp(otp,username, ConnectionString);
        }
        public static LoginResponseToken GetSessionByUserName(string username)
        {
            Authorize auth = new Authorize();
            return auth.GetSessionByUserName(username, ConnectionString);
        }
        public static DateTime? GetLastPswdChange(string username)
        {
            Authorize auth = new Authorize();
            return auth.GetLastPswdChange(username, ConnectionString);
        }
        public static List<Roles> GetUserRoles(string username)
        {
            Authorize auth = new Authorize();
            return auth.GetUserRoles(username, ConnectionString);
        }

        public static string IsValidlifesenseDevice(string DeviceId)
        {
            return new PatientDevice().IsValidlifesenseDevice(DeviceId, ConnectionString);
        }
        public static bool UpdateDeviceStatus(UpdateDeviceStatus status)
        {
            return new PatientDevice().UpdateDeviceStatus(status,ConnectionString);
        }
        public static bool IsDeviceAvailable(DeviceValidate info)
        {
            return new PatientDevice().IsDeviceAvailable(info, ConnectionString);
        }
        public static bool IsDeviceModelAvailable(DeviceValidate info)
        {
            return new PatientDevice().IsDeviceModelAvailable(info, ConnectionString);
        }
        public static int AddDevice(AddDevice info)
        {
            return new PatientDevice().AddDevice(info, ConnectionString);
        }
        public static bool StagingTableInsert(TranstekDeviceTelemetry dev, string DeviceType)
        {
            return new PatientDevice().StagingTableInsert(dev, DeviceType, ConnectionString);
        }
        public static  bool InsertPatienVendorConnectivity(AddPatientVendorConn data)
        {
            return new PatientDevice().InsertPatienVendorConnectivity(data, ConnectionString);
        }
        public static LoginDetails InsertLoginDetails(string username, string otp, string tkn)
        {
            return new Authorize().InsertLoginDetails(username, otp, tkn, ConnectionString);
        }
        public static bool bodytracedataInsert(bodytracedata data)
        {
            return new PatientDevice().bodytracedataInsert(data, ConnectionString);
        }
        public static bool VerifyUserName(VerifyUserName user)
        {
            return new Authorize().VerifyUserName(user.UserName, ConnectionString);
        }
        public static LoginResponse Login(RPMWeb.Data.Common.RPMLogin verPass)
        {
            return new Authorize().Login(verPass, ConnectionString);
        }
 
        public static ContactDetails GetPhoneNumberByUserName(string username)
        {
            return new Authorize().GetPhoneNumberByUserName(username, ConnectionString);
        }
        public static LoginResponseToken CreateNewToken(int time)
        {
            return new Authorize().CreateNewToken(time,ConnectionString);
        }
        public static bool CheckUserLockedStatus(string username)
        {
            return new Authorize().CheckUserLockedStatus(username,ConnectionString);
        }
        public static UserRoleConfig GetUserRoleConfig(string username)
        {
            return new Authorize().GetUserRoleConfig( ConnectionString, username);
        }
        public static bool CheckUserActive(string username)
        {
            return new Authorize().CheckUserActive(ConnectionString, username);
        }
        
        public static bool LogOut(Logout logout)
        {
            return new Authorize().LogOut(logout, ConnectionString);
        }
        public static bool UpdatePassword(Updatepassword updatepassword)
        {
            return new Authorize().UpdatePassword(updatepassword, ConnectionString);
        }
        public static List<DashboardVitalsList> GetDashboardVitalCount(int Days, DateTime ToDate, int UtcOffset, int RoleId, string CreatedBy)
        {
            return new Home().GetDashboardVitalCount(Days, ToDate, UtcOffset, RoleId, CreatedBy, ConnectionString);
        }
        public static List<DashboardAlerts> GetDashboardAlerts(int RoleId, string CreatedBy)
        {
            return new Home().GetDashboardAlerts(RoleId, CreatedBy, ConnectionString);
        }
        public static  List<DashboardPatientStatusList> GetDashboardPatientStatus(int RoleId, string CreatedBy)
        {
            return new Home().GetDashboardPatientStatus(RoleId, CreatedBy, ConnectionString);
        }
        public static List<OperationalMasterData> GetOperationalMasterData(string CreatedBy)
        {
            return new Authorize().GetOperationalMasterData(CreatedBy, ConnectionString);
        }
        public static bool UnlockUser(int? userId,int? patientid)
        {
            return new Authorize().UnlockUser(userId,patientid, ConnectionString);
        }
        public static bool UnlockUserByUsername(string username)
        {
            return new Authorize().UnlockUserByUsername(username, ConnectionString);
        }
        public static bool UpdateUserPassword(string username , string password)
        {
            return new Authorize().UpdateUserPassword(username, password, ConnectionString);
        }
        public static bool ClearOldSessions(string username)
        {
            return new Authorize().ClearOldSessions(username, ConnectionString);
        }
        public static bool UserLockStatusCheck(int? userId, int? patientid)
        {
            return new User().UserLockStatusCheck(userId, patientid, ConnectionString);
        }
        public static List<Roles> GetRolesMasterData(string CreatedBy)
        {
            return new Authorize().GetRolesMasterData(CreatedBy, ConnectionString);
        }
        public static DataSet GetMasterDataForStatesAndCities(string CreatedBy)
        {
            return new Authorize().GetMasterDataForStatesAndCities(CreatedBy, ConnectionString);
        }
        public static UserPermission GetUserAccessRights(string username, int roleid)
        {
            return new Authorize().UserAccessRights(username, roleid, ConnectionString);
        }
        public static List<DraftPatient> GetDraftPatients(string CreatedBy)
        {
            return new Patient().GetDraftPatients(CreatedBy, ConnectionString);
        }
        public static bool UploadProfilePicture(int PatientId, IFormFile httpPostedFile, string filename, string Blob_Conn_String, string ContainerName, string UserName)
        {
            return new Patient().UploadProfilePicture(PatientId, httpPostedFile, filename, Blob_Conn_String, ContainerName, UserName, ConnectionString);
        }
        public static bool UploadUserProfilePicture(int UserId, IFormFile httpPostedFile, string filename, string Blob_Conn_String, string ContainerName, string UserName)
        {
            return new User().UploadUserProfilePicture(UserId, httpPostedFile, filename, Blob_Conn_String, ContainerName, UserName, ConnectionString);
        }
        public static bool UploadPatientDocument(UploadPatientDocument info, IFormFile httpPostedFile)
        {
            return new Patient().UploadPatientDocument(info, httpPostedFile, ConnectionString);
        }
        public static List<PatientDocuments> PatientDocuments(int PatientId, string CreatedBy)
        {
            return new Patient().PatientDocuments(PatientId, CreatedBy, ConnectionString);
        }
        public static PatientDetails GetDraftPatientDetails(int PatientId, string CreatedBy)
        {
            return new Patient().GetDraftPatientDetails(PatientId,CreatedBy, ConnectionString);
        }
        public static NewPatientCredential SavePatient(PatientDetails Info)
        {
            return new Patient().SavePatient(Info, ConnectionString);
        }
        public static bool UpdatePatient(PatientDetails Info)
        {
            return new Patient().UpdatePatient(Info, ConnectionString);
        }
        public static  ProgramDetailsMasterDataAddPatient GetProgramDetailsMasterData(int RoleId,string UserName)
        {
            return new Patient().GetProgramDetailsMasterData(RoleId,UserName, ConnectionString);
        }
        public static List<DeviceDetails> GetDeviceDetails(int VitalId, string CreatedBy)
        {
            return new Patient().GetDeviceDetails(VitalId, CreatedBy, ConnectionString);
        }
        public static PatientNProgramDetails GetPatient(int PatientId,int PatientProgramId,string CreatedBy)
        {
            return new Patient().GetPatient(PatientId,PatientProgramId,CreatedBy,ConnectionString);
        }
        public static DataSet GetAllPatient(int Days, string Vitals, string PatientType,int RoleId, string CreatedBy)
        {
            return new Patient().GetAllPatient(Days,Vitals,PatientType,RoleId,CreatedBy, ConnectionString);
        }
        public static List<GetAllPatientInfo> GetAllPatients(string PatientType, string Vitals, int RoleId, string CreatedBy)
        {
            return new Patient().GetAllPatients( PatientType,  Vitals,  RoleId, CreatedBy, ConnectionString);
        }
        public static List<GetAllPatientInfo> GetAllPatientsList(DateTime ToDate,int UtcOffset, int Days, int RoleId, string CreatedBy)
        {
            return new Patient().GetAllPatientsList(ToDate, UtcOffset, Days, RoleId, CreatedBy, ConnectionString);
        }
        public static List<GetAllPatientSmsInfo> GetAllPatientsSmsList( int RoleId, string CreatedBy)
        {
            return new Patient().GetAllPatientsSmsList( RoleId, CreatedBy, ConnectionString);
        }

        public static string GetPatientLastPgmStatus(int PatientId, int PatientProgramId, string Createdby)
        {
            return new Patient().GetPatientLastPgmStatus(PatientId, PatientProgramId, Createdby, ConnectionString);
        }
        public static VitalReadings GetPatientVitalReadings(int PatientId,int PatientProgramId, DateTime StartDate, DateTime EndDate, string CreatedBy)
        {
            return new Patient().GetPatientVitalReadings(PatientId, PatientProgramId, StartDate, EndDate, CreatedBy,ConnectionString);
        }
        public static PatientVitalReadings GetPatientVitalReadingswithDateTime(int PatientId,int PatientProgramId, DateTime StartDate, DateTime EndDate, string CreatedBy)
        {
            return new Patient().GetPatientVitalReadingswithDateTime(PatientId, PatientProgramId, StartDate, EndDate, CreatedBy, ConnectionString);
        }
        public static List<HealthTrends> GetPatientHealthTrends(string username, int PatientId,int PatientProgramId, DateTime StartDate, DateTime EndDate, string CreatedBy)
        {
            return new Patient().GetPatientHealthTrends(username,PatientId, PatientProgramId, StartDate, EndDate, CreatedBy, ConnectionString);
        }
        public static PatientDocuments GetMyPatientDocuments(string username, int PatientId, int PatientProgramId, int DocId)
        {
            return new Patient().GetMyPatientDocuments(username, PatientId, PatientProgramId, DocId, ConnectionString);
        }
        public static List<GetSchedules> GetPatientSchedule(int PatientId, DateTime StartDate, DateTime EndDate, string CreatedBy)
        {
            return new Patient().GetPatientSchedule(PatientId, StartDate, EndDate, CreatedBy, ConnectionString);
        }
        public static List<GetAlertsAndTasks> GetPatientAlertAndTask(int PatientId, int PatientProgramId, DateTime StartDate, DateTime EndDate, string CreatedBy)
        {
            return new Patient().GetPatientAlertAndTask(PatientId, PatientProgramId, StartDate, EndDate, CreatedBy, ConnectionString);
        }
        public static List<DashboardAlerts> GetPatientCriticalAlerts(int PatientId, string CreatedBy)
        {
            return new Patient().GetPatientCriticalAlerts(PatientId, CreatedBy, ConnectionString);
        }
        public static int GetPatientInteractionTime(int PatientId, string CreatedBy)
        {
            return new Patient().GetPatientInteractionTime(PatientId, CreatedBy, ConnectionString);
        }
        public static NewPatientCredential UpdatePatientPassword(ResetPatientPW Info)
        {
            return new Patient().UpdatePatientPassword(Info, ConnectionString);
        }

        public static NewPatientCredential UpdateUserPassword(ResetUserPW Info)
        {
            return new User().UpdateUserPassword(Info, ConnectionString);
        }
        public static SearchPatient SearchPatient(string PatientNumber, string CreatedBy)
        {
            return new Patient().SearchPatient(PatientNumber, CreatedBy, ConnectionString);
        }
        public static int SavePatientProgram(PatientProgramDetailsInsert Info, string CreatedBy)
        {
            return new PatientProgram().SavePatientProgram(Info, CreatedBy,ConnectionString);
        }
        
        public static bool UpdatePatientProgram(PatientProgramDetailsUpdate Info)
        {
            return new PatientProgram().UpdatePatientProgram(Info, ConnectionString);
        }

        public static List<PatientAllPrograms> GetAllPatientPrograms(int PatientId, string UserName)
        {
            return new PatientProgram().GetAllPatientPrograms(PatientId, UserName,ConnectionString);
        }
        public static int RenewPatientProgram(PatientProgramRenew Info)
        {
            return new PatientProgram().RenewPatientProgram(Info, ConnectionString);
        }

        public static List<GetPgmGoals> GetAllPgmandGoals(string CreatedBy)
        {
            return new PgmNGoals().GetAllPgmandGoals( CreatedBy, ConnectionString);
        }
        public static List<GetPgmGoals> GetPgmandGoals(int ProgramId, string CreatedBy)
        {
            return new PgmNGoals().GetPgmandGoals(ProgramId, CreatedBy, ConnectionString);
        }
        public static bool AddPgmandGoals(PgmGoals data)
        {
            return new PgmNGoals().AddPgmandGoals(data, ConnectionString);
        }
        public static bool UpdatePgmandGoals(PgmGoals data)
        {
            return new PgmNGoals().UpdatePgmandGoals(data, ConnectionString);
        }
        public static int AddCareTeam(CareTeams data) 
        {
            return new CareTeam().AddCareTeam(data, ConnectionString);
        }
        
        public static int UpdateCareTeam(CareTeams data)
        {
            return new CareTeam().UpdateCareTeam(data, ConnectionString);
        }
        public static CareTeamInfo GetTeamDetails(int CareteamId, string CreatedBy)
        {
            return new CareTeam().GetTeamMembersByTeam(CareteamId, CreatedBy, ConnectionString);
        }
        public static List<CareTeamBaseInfo> GetTeamDetails(string CreatedBy)
        {
            return new CareTeam().GetTeam(CreatedBy, ConnectionString);
        }
        public static List<NonAssignedCM> GetNonAssignedCaerTeamMembers(string CreatedBy)
        {
            return new CareTeam().GetNonAssignedCaerTeamMembers(CreatedBy, ConnectionString);
        }
        public static DataSet GetTeamTasks( DateTime TodayDate, DateTime StartDate, DateTime EndDate, int RoleId ,string CreatedBy)
        {
            return new CareTeam().GetTeamTasks(TodayDate, StartDate, EndDate, RoleId,CreatedBy, ConnectionString);
        }
        public static DataSet GetTeamAlerts(int RoleId, string CreatedBy)
        {
            return new CareTeam().GetTeamAlerts(RoleId, CreatedBy, ConnectionString);
        }
        public static DataSet GetDetailedTeamAlerts(int CareTeamId, string CreatedBy)
        {
            return new CareTeam().GetDetailedTeamAlerts(CareTeamId, CreatedBy, ConnectionString);
        }
        public static GetUserProfiles GetUserProfiles(int UserId, string CreatedBy)
        {
            return new User().GetUserProfiles(UserId, CreatedBy, ConnectionString);
        
        }
        public static GetUserProfiles GetMyProfiles(string CreatedBy)
        {
            return new User().GetMyProfiles(CreatedBy, ConnectionString);
        }
        public static NewUserCredential SaveUsers(UserProfiles Info)
        {
            return new User().SaveUsers(Info, ConnectionString);
        }
        public static List<LastBilleddata> GetPatientLastBilledDetailsBydate(int patientId, int patientProgramId, DateTime billeddate)
        {
            return new Patient().GetPatientlastBilledCycleByDate(patientId, patientProgramId, billeddate, ConnectionString);
        }
        public static bool UpdateUser(UserProfiles ui)
        {
            return new User().UpdateUser(ui, ConnectionString);
        }
        public static DataSet GetAllUsers(int RoleId ,string CreatedBy)
        {
            return new User().GetAllUsers(RoleId,CreatedBy, ConnectionString);
        }
        public static  PatientInfoForProgramInsert GetPatientInfoForProgramInsert(int PatientId, string CreatedBy)
        {
            return new Patient().GetPatientInfoForProgramInsert(PatientId, CreatedBy, ConnectionString);
        }
        public static int AddClinic(ClinicInfo info,string CreatedBy)
        {
            return new Clinic().AddClinic(info, CreatedBy, ConnectionString);
        }
        public static bool UpdateClinic(UpdClinicInfo info,string CreatedBy)
        {
            return new Clinic().UpdateClinic(info, CreatedBy, ConnectionString);
        }
        public static ClinicInfo GetClinic(int clinicid)
        {
            return new Clinic().GetClinic(clinicid, ConnectionString);
        }
        public static List<ClinicAllInfo> GetAllClinic()
        {
            return new Clinic().GetAllClinic(ConnectionString);
        }
        public static int AddTask(TaskInfo info)
        {
            return new Tasks().AddTask(info,ConnectionString);
        }
        public static bool UpdateTask(TaskInfo info)
        {
            return new Tasks().UpdateTask(info, ConnectionString);
        }
        public static DataSet GetTasks(DateTime StartDate, DateTime EndDate ,string CreatedBy)
        {
            return new Tasks().GetTasks(StartDate, EndDate,CreatedBy, ConnectionString);
        }
        public static DataSet GetTasksByTypeAndId(string TaskType,int CareTeamId, DateTime TodayDate, DateTime StartDate, DateTime EndDate, int RoleId,string CreatedBy)
        {
            return new Tasks().GetTasksByTypeAndId(TaskType, CareTeamId, TodayDate, StartDate, EndDate,RoleId, CreatedBy, ConnectionString);
        }
        public static GetTask GetTaskById(int Id)
        {
            return new Tasks().GetTaskById(Id, ConnectionString);
        }
        public static DataSet GetMasterDataForTask(int RoleId, string CreatedBy)
        {
            return new Tasks().GetMasterDataForTask(RoleId, CreatedBy, ConnectionString);
        }
        public static DataSet GetAlerts(int RoleId,string CreatedBy)
        {
            return new Alerts().GetAlerts(RoleId,CreatedBy,ConnectionString);
        }
        public static DataSet GetTeamAlertsById(string AlertType,int CareTeamId,int RoleId, string CreatedBy)
        {
            return new Alerts().GetTeamAlertsById(AlertType, CareTeamId,RoleId, CreatedBy, ConnectionString);
        }
        public static GetAlert GetAlertById(int Id)
        {
            return new Alerts().GetAlertById(Id,ConnectionString);
        }
        public static GetAlert GetAlertByIdPatient(int Id)
        {
            return new Alerts().GetAlertByIdPatient(Id, ConnectionString);
        }
        public static int AlertResponse(TaskResponse info)
        {
            return new Alerts().AlertResponse(info, ConnectionString);
        }
        public static int AlertResponseFromPatient(TaskResponse info)
        {
            return new Alerts().AlertResponseFromPatient(info, ConnectionString);
        }
        
        public static int AddSchedule(ScheduleInfo info)
        {
            return new Schedules().AddSchedule(info, ConnectionString);
        }
        public static bool UpdateSchedule(ScheduleInfo info)
        {
            return new Schedules().UpdateSchedule(info, ConnectionString);
        }
        public static bool UpdateCurrentSchedule(CurrentScheduleInfo info)
        {
            return new Schedules().UpdateCurrentSchedule(info, ConnectionString);
        }
        public static bool UpdateScheduleCompletion(CompletedSchedules data)
        {
            return new Schedules().UpdateScheduleCompletion(data, ConnectionString);
        }
        public static DataSet GetMasterDataForSchedules(int RoleId, string CreatedBy)
        {
            return new Schedules().GetMasterDataForSchedules(RoleId, CreatedBy, ConnectionString);
        }
        public static List<GetSchedules> GetCareTeamSchedule(DateTime StartDate, DateTime EndDate, string CreatedBy)
        {
            return new Schedules().GetCareTeamSchedule(StartDate,EndDate,CreatedBy,ConnectionString);
        }
        public static ScheduleInfo GetWorklistScheduleById(int CurrentScheduleId, string CreatedBy)
        {
            return new Schedules().GetWorklistScheduleById( CurrentScheduleId, CreatedBy, ConnectionString);
        }
        public static int AddNotes(NoteInfo info)
        {
            return new Notes().AddNotes(info, ConnectionString);
        }
        public static bool UpdateNotes(NoteInfo info)
        {
            return new Notes().UpdateNotes(info, ConnectionString);
        }
        public static NoteDetails GetPatientReviewNotesDetails(int PatientNoteId, string CreatedBy)
        {
            return new Notes().GetPatientReviewNotesDetails(PatientNoteId, CreatedBy, ConnectionString);
        }
        public static NoteDetails GetPatientCallNotesDetails(int PatientNoteId, string CreatedBy)
        {
            return new Notes().GetPatientCallNotesDetails(PatientNoteId, CreatedBy, ConnectionString);
        }
        public static int AddNotesDetails_V1(NoteInfo_V1 info)
        {
            return new Notes().AddNotesDetails_V1(info, ConnectionString);
        }
        public static int UpdateNotesDetails_V1(NoteInfo_V1 info)
        {
            return new Notes().UpdateNotesDetails_V1(info, ConnectionString);
        }
        public static List<GetNotes> GetPatientCallNotes(int PatientId, DateTime StartDate, DateTime EndDate, string CreatedBy)
        {
            return new Notes().GetPatientCallNotes(PatientId, StartDate, EndDate, CreatedBy, ConnectionString);
        }
        public static GetPatientNotesQA GetPatientNotes(string ProgrameName, string Type, int PatientNoteId, string CreatedBy)
        {
            return new Notes().GetPatientNotes(ProgrameName, Type, PatientNoteId, CreatedBy, ConnectionString);
        }
        public static GetPatientNotesQA GetPatientNotes(int ProgramId, string Type, int PatientNoteId, string CreatedBy)
        {
            return new Notes().GetPatientNotes(ProgramId, Type, PatientNoteId, CreatedBy, ConnectionString);
        }
		public static NotesTypeMasterData GetMasterDataNotes(int ProgramId, string Type, string CreatedBy)
        {
            return new Notes().GetMasterDataNotes(ProgramId, Type,CreatedBy, ConnectionString);
        }
        public static List<GetNotes> GetPatientReviewNotes(int PatientId, DateTime StartDate, DateTime EndDate, string CreatedBy)
        {
            return new Notes().GetPatientReviewNotes(PatientId, StartDate, EndDate, CreatedBy, ConnectionString);
        }
        public static List<GetAllNotes> GetPatientNotes(int PatientId, int PatientProgramId, string NoteType, DateTime StartDate, DateTime EndDate, string CreatedBy)
        {
            return new Notes().GetPatientNotes(PatientId, PatientProgramId, NoteType, StartDate, EndDate, CreatedBy,ConnectionString);
        }
        public static RegisterDeviceResponse RegisterPatientDevice(DeviceRegister dr)
        {
            return new PatientDevice().RegisterPatientDevice(dr, ConnectionString);
        }
        public static bool ValidateDevice(string deviceid)
        {
            return new PatientDevice().ValidateDevice(deviceid, ConnectionString);
        }
        public static RegisterDeviceResponse RemoveDevice(DeviceRegister dr)
        {
            return new PatientDevice().RemovePatientDevice(dr, ConnectionString);
        }
        public static RegisterDeviceResponse ResetDevice(DeviceRegister dr)
        {
            return new PatientDevice().ResetPatientDevice(dr, ConnectionString);
        }
        public static List<WorldTimeZone> GetWorldTimeZone()
        {
            return new PatientDevice().GetWorldTimeZone(ConnectionString);
        }
        public static int AddPatientProgramSymptoms(PatientSymptom data)
        {
            return new Symptoms().AddPatientProgramSymptoms(data,ConnectionString);
        }
        public static  bool UpdatePatientProgramSymptoms(PatientSymptom data)
        {
            return new Symptoms().UpdatePatientProgramSymptoms(data, ConnectionString);
        }
        public static List<GetPatientSymptom> GetPatientSymptoms(int PatientId,int PatientProgramId, string CreatedBy)
        {
            return new Symptoms().GetPatientSymptoms(PatientId, PatientProgramId, CreatedBy, ConnectionString);
        }
        public static List<Symptom> GetSymptomsMasterData(string CreatedBy)
        {
            return new Symptoms().GetSymptomsMasterData(CreatedBy, ConnectionString);
        }
        public static int AddPatientProgramMedication(PatientMedication data)
        {
            return new Medication().AddPatientProgramMedication(data, ConnectionString);
        }
        public static bool UpdatePatientProgramMedication(PatientMedication data)
        {
            return new Medication().UpdatePatientProgramSymptoms(data, ConnectionString);
        }
        public static List<GetPatientMedication> GetPatientMedication(int PatientId, int PatientProgramId, string CreatedBy)
        {
            return new Medication().GetPatientMedication(PatientId, PatientProgramId, CreatedBy, ConnectionString);
        }
        public static ProileSummary GetMyProfileAndProgram(string Username)
        {
            return new User().GetMyProfileAndProgram(Username, ConnectionString);
        }
        public static Schedule GetToDoList(string Username, DateTime day)
        {
            return new Schedules().GetToDoList(Username, day, ConnectionString);
        }
        public static List<PatientSummary> GetPatientVitalSummary(string Username, DateTime StartDate, DateTime EndDate)
        {
            return new Patient().GetPatientVitalSummary(Username,StartDate,EndDate, ConnectionString);
        }
        public static VitalSummary GetVitalSummaryDetails(string Username, DateTime startDate, DateTime endDate)
        {
            return new Patient().GetVitalSummaryDetails(Username, startDate, endDate, ConnectionString);
        }
        public static List<GetPatientInfo> GetPatientInfo(string UserName)
        {
            return new Patient().GetPatientInfo(UserName, ConnectionString);
        }
        public static string GetPatientBillingReport(DateTime startDate, DateTime endDate, int? patientId, string clinic, string cptCode, int isMonth, string Username,string Format, string Blob_Conn_String, string ContainerName)
        {
            string report = new Patient().GetPatientBillingReport(startDate, endDate, patientId, clinic, cptCode, isMonth, Username, Format, ConnectionString, Blob_Conn_String,  ContainerName);
            return report;
            
        }
        
        public static string DownloadInvoice(string blob_Conn_String, string containerName)
        {
            string report = new Patient().DownloadInvoice(blob_Conn_String, containerName, ConnectionString);
            return report;
        }
        public static List<BillingInfo> GetPatientBillingInfo(DateTime startDate, DateTime endDate, string Username)
        {
            List<BillingInfo> report = new Patient().GetPatientBillingInfos(startDate, endDate, Username, ConnectionString);
            return report;
        }
        public static List<BillingInfoCounts> GetPatientBillingInfoCounts(string billingCode,string cycle,int RoleId, string Username)
        {
            List<BillingInfoCounts> report = new Patient().GetPatientBillingInfoCounts(billingCode, cycle, RoleId, Username, ConnectionString);
            return report;
        }

        public static BillingType GetBillingType()
        {
            BillingType type = new Patient().GetBillingType(ConnectionString);
            return type;
        }
        public static List<PatientBilldata> GetPatientBillingData(int patientId, string Username)
        {
            List<PatientBilldata> data = new Patient().GetPatientBillingData(patientId, Username, ConnectionString);
            return data;
        }
        public static int AddPatientNotification(PatientNotification_ins info)
        {
            int rowid = new Notification().AddPatientProgramMedication(info, ConnectionString);
            return rowid;
        }
        public static int UpdateNotificationReadStatus(NotificationStatusUpdate info)
        {
            int rowid = 0;
            //if (info.NotificationAuditId==0)
            //{
            //    NotificationAuditData nda =  new NotificationAuditData();
            //    nda.NotificationAuditId = info.NotificationAuditId;
            //    nda.NotificationId = info.NotificationId;
            //    nda.IsRead = true;
            //    nda.IsNotify = true;
            //    nda.AuditCreatedBy = info.ModifiedBy;

            //    rowid = new Notification().AddSystemNotificationAudit(nda, ConnectionString);
            //}
            //else
            //{
                rowid = new Notification().UpdateSystemNotificationAuditReadStatus(info.NotificationId, true, ConnectionString);
            //}
            return rowid;
        }
        public static SystemNotificationByUser GetSystemNotificationsByUser(string UserName, DateTime? StartDate,
                                                                int Count, string user)
        {
            return new Notification().GetSystemNotificationsByUser(UserName, StartDate, Count, user, ConnectionString);
        }
        public static SystemNotificationCount GetSystemNotificationCount(string UserName)
        {
            return new Notification().GetSystemNotificationCount(UserName, ConnectionString);
        }
        public static VitalSummaryMeasures GetRecentPatientVitalSummary(string Username, int dayCount)
        {
            return new Patient().GetRecentPatientVitalSummary(Username, dayCount, ConnectionString);
        }
        public static List<BillingInfo> GetPatientBillingInfo(int isPast, int isFuture, int isToday, int isCurrentMonth, int isLastMonth, string Username)
        {
            List<BillingInfo> report = new Patient().GetPatientBillingInfos(isPast, isFuture, isToday, isCurrentMonth, isLastMonth, Username, ConnectionString);
            return report;
        }
        public static List<PatientBilldata> GetBillingDataByPatientId(int patientId, int patientprogramId, string Username)
        {
            List<PatientBilldata> data = new Patient().GetBillingDataByPatientId(patientId, patientprogramId, Username, ConnectionString);
            return data;
        }

        public static List<searchPatient> SearchPatient(int RoleId, string UserName)
        {
            return new Patient().SearchPatient(RoleId, UserName, ConnectionString);
        }

        public static PatientBilldataList GetPatientBillingDataList(string patientType, string patientFilter, string patientId,  string patientName,  string program,  string assignedmember,  int Index, string readingFilter, string interactionFilter,int RoleId,string CreatedBy,string ProgramType)
        {
            PatientBilldataList data = new Patient().GetPatientBillingDataList(patientType,  patientFilter,  patientId,  patientName,  program,  assignedmember,  Index,readingFilter,interactionFilter,RoleId,CreatedBy ,ConnectionString, ProgramType);
            return data;
        }

        public static List<ProgramVitalDignostics> GetDiagnosisCodeByVitalId(vitalIdsList vitalId)
        {
            List<ProgramVitalDignostics> codes = new Patient().GetDiagnosisCodeByVitalId(ConnectionString, vitalId);
            return codes;
        }
        public static List<string> GetBillingCodes()
        {
            List<string> codes = new Patient().GetBillingCodes(ConnectionString);
            return codes;
        }
        public static List<Programs> GetAllPrograms()
        {
            List<Programs> programs = new Patient().GetAllPrograms(ConnectionString);
            return programs;
        }
        public static MobileHomeData GetMobHomeData(string UserName, DateTime day, int count)
        {
            MobileHomeData ret = new MobileHomeData();
            try
            {
                ret.MobProfileSummary = GetMyProfileAndProgram(UserName);
                ret.MobSchedule = GetToDoList(UserName, day);
                ret.MobVitalSummaryMeasures = GetRecentPatientVitalSummary(UserName, count);
            }
            catch
            {
                throw;
            }
            return ret;
        }
        public static ClientConfig GetClientConfig(string user)
        {
            ClientConfig ret = new ClientConfig();
            try
            {
                List<SystemConfigInfo> igc = DalCommon.GetSystemConfig(ConnectionString, "Client", user);
                foreach(SystemConfigInfo sc in igc)
                {
                    switch(sc.Name)
                    {
                        case "CountryCode":
                            ret.CountryCode = sc.Value;
                            break;
                    }
                }
            }
            catch
            {
                throw;
            }
            return ret;
        }
        public static int AddNewPatientProgram(PatientProgramDetailsInsert data, string Createdby)
        {
            return new PatientProgram().AddNewPatientProgram(data, Createdby, ConnectionString);
        }
        public static int UpdateProgram(UpdateProgramDetails data)
        {
            return new PatientProgram().UpdateProgram(data, ConnectionString);
        }
        public static List<LastBilleddata> GetPatientLastBilledDetails(int patientId, int patientProgramId,string status)
        {
            return new Patient().GetPatientlastBilledCycle(patientId, patientProgramId,status,ConnectionString);
        }
        public static bool UpdateBillDates(BillingDatesUpdates Info)
        {
            return new Patient().UpdateBillDatesMedIT(Info, ConnectionString);
        }
        public static ReturnMsg AddDeviceProc(AddDevicePro info)
        {
            return new Device().AddDeviceProc(info, ConnectionString);
        }
        public static int UpdateDeviceProc(UpdateDevice info)
        {
            return new Device().UpdateDeviceProc(info, ConnectionString);
        }
        public static DataSet GetDeviceMasterData(string CreatedBy)
        {
            return new Device().GetDeviceMasterData(CreatedBy, ConnectionString);
        }
        public static List<DeviceInfo> GetDeviceInfo(string CreatedBy)
        {
            return new Device().GetDeviceInfo(CreatedBy, ConnectionString);
        }
        public static ReturnMsg AddDeviceVendor(AddDeviceVendor info)
        {
            return new Device().AddDeviceVendor(info, ConnectionString);
        }
        public static int UpdateDeviceVendor(UpdateDeviceVendor info)
        {
            return new Device().UpdateDeviceVendor(info, ConnectionString);
        }
        public static bool IsValidVendorCode(string Code, string CreatedBy)
        {
            return new Device().IsValidVendorCode(Code, CreatedBy, ConnectionString);
        }
        public static List<DashboardAlertAndTask> GetDashboardTodaysAlertsandTasks(int RoleId, DateTime StartDate, DateTime EndDate, string CreatedBy)
        {
            return new Home().GetDashboardTodaysAlertsandTasks(RoleId, StartDate, EndDate, CreatedBy, ConnectionString);
        }
        public static List<DashboardTeamOverView> GetDashboardTeamOverview(int RoleId, DateTime StartDate, DateTime EndDate, string CreatedBy)
        {
            return new Home().GetDashboardTeamOverview(RoleId, StartDate, EndDate, CreatedBy, ConnectionString);
        }
         public static List<PatientToDoListResponse> GetPatientToDoList(string Username, DateTime StartDate, DateTime EndDate)
        {
            return new WebAppPatient().GetToDoList(Username, StartDate, EndDate, ConnectionString);
        }
        public static PatientCurrentReading GetCurrentCycleReading(int Patientid)
        {
            return new WebAppPatient().GetCurrentCycleReading(Patientid, ConnectionString);
        }
        public static bool DeleteDraft(int PatientId)
        {
            return new Patient().DeleteDraft(PatientId, ConnectionString);

        }
        public static DeactivateUser DeactivateUser(int UserId)
        {
            return new User().DeactivateUser(UserId, ConnectionString);
        }
        public static bool DeletePatientNotes(int noteid)
        {
            return new Notes().DeletePatientNotes(noteid, ConnectionString);
        }
        public static bool DeletePatientDocuments(int documentid)
        {
            return new Patient().DeletePatientDocuments(documentid, ConnectionString);
        }
        public static string CallLogByCareTeam(string blob_Conn_String, string containerName, DateTime startDate, DateTime endDate, int? userId,string createdBy)
        {
            string report = new Notes().CallLogByCareTeam(blob_Conn_String, containerName, ConnectionString,startDate,endDate,userId,createdBy);
            return report;
        }

        public static string NonEstablishedCallReport(string blob_Conn_String, string containerName,DateTime startDate, DateTime endDate,int RoleId,string Createdby)
        {
            string report = new Notes().NonEstablishedCallReport(blob_Conn_String, containerName, ConnectionString, startDate, endDate,RoleId,Createdby);
            return report;
        }
        public static string GetPatientBillingReportDetails(DateTime startDate, DateTime endDate, int? patientId, string clinic, int isMonth, string Username, string Format, string Blob_Conn_String, string ContainerName, string ProgramType)
        {
            string report = string.Empty;
            if (ProgramType.ToUpper()=="RPM")
            {
                report = new Patient().GetPatientBillingReportDetails(startDate, endDate, patientId, clinic, isMonth, Username, Format, ConnectionString, Blob_Conn_String, ContainerName);
            }
            else if (ProgramType.ToUpper()=="CCM")
            {
                report = new Patient().GetPatientBillingReportDetailsCCM(startDate, endDate, patientId, clinic, isMonth, Username, Format, ConnectionString, Blob_Conn_String, ContainerName);
            }
            else if (ProgramType.ToUpper() == "PCM")
            {
                report = new Patient().GetPatientBillingReportDetailsPCM(startDate, endDate, patientId, clinic, isMonth, Username, Format, ConnectionString, Blob_Conn_String, ContainerName);
            }
            return report;
        }
        public static string GetPatientMissingBillingReportDetails(DateTime startDate, DateTime endDate, int? patientId, string clinic, int isMonth, string Username, string Format, string Blob_Conn_String, string ContainerName, string ProgramType)
        {
            string report = string.Empty;
            if (ProgramType.ToUpper()=="RPM")
            {
                report = new Patient().GetPatientMissingBillingReportDetails(startDate, endDate, patientId, clinic, isMonth, Username, Format, ConnectionString, Blob_Conn_String, ContainerName);
            }
            else if (ProgramType.ToUpper()=="CCM")
            {
                report = new Patient().GetPatientMissingBillingReportDetailsCCM(startDate, endDate, patientId, clinic, isMonth, Username, Format, ConnectionString, Blob_Conn_String, ContainerName);
            }
            else if (ProgramType.ToUpper() == "PCM")
            {
                report = new Patient().GetPatientMissingBillingReportDetailsPCM(startDate, endDate, patientId, clinic, isMonth, Username, Format, ConnectionString, Blob_Conn_String, ContainerName);
            }
            return report;
        }

        public static string[] GetLanguages()
        {
            string[] Languages=new User().GetLanguages(ConnectionString);
            return Languages;
        }

        public static bool CheckPatientActive(string username)
        {
            return new Authorize().CheckPatientActive(ConnectionString, username);
        }

        public static bool DeleteSystemNotificationsByUser(int notificationId, string UserName)

        {
            return new Notification().DeleteSystemNotificationsByUser(notificationId, UserName, ConnectionString);
        }
        public static bool DeleteSystemNotificationsReadUnRead(int notificationId, string UserName)

        {
            return new Notification().DeleteSystemNotificationsReadUnRead(notificationId, UserName, ConnectionString);
        }
        public static bool InsertFirebaseToken(string UserName, string Bearer, string Token)

        {
            return new Notification().InsertFirebaseToken(UserName, Bearer,Token, ConnectionString);
        }
        public static bool DeleteFirebaseToken(string UserName, string Bearer, string Token)

        {
            return new Notification().DeleteFirebaseToken(UserName, Bearer, Token, ConnectionString);
        }

        public static bool IsPatientOnline(string PatientId, string UserName)
        {
            return new Notification().IsPatientOnline(PatientId, UserName,ConnectionString);
        }
        public static string IsRoomExists(string UserName, string PatientId)
        {
            return new User().IsRoomExists(UserName,PatientId,ConnectionString);
        }
        public static string GetCommUserName(string UserName)
        {
            return new User().GetCommUserName(UserName, ConnectionString);
        }
        public static commUserNamesforVideoCall GetCommUserNamesforVideo(string CareTeam, string Patient)
        {
            return new User().GetCommUserNamesforVideo(CareTeam,Patient, ConnectionString);
        }
        public static void UpdateVideoRoom(string UserName, string PatientId, string RoomName)
        {
            new User().UpdateVideoRoom(UserName, PatientId, RoomName, ConnectionString);
        }
        public static void UpdateVideoRoomToken(string UserName, string Token, string roomname)
        {
            new User().UpdateVideoRoomToken(UserName, Token, roomname, ConnectionString);
        }

        public static void GetFirebaseNotificationByUser(string UserName,string ReceiverId, firebasenotificationmessage notify, string category)
        {
            new Notification().GetFirebaseNotificationByUser(UserName,ReceiverId, notify, category, ConnectionString);
        }

       public static void GetFirebaseNotificationCallRejection(string UserName, string ReceiverId, firebasenotificationmessage notify, string category, int tokenid)
        {
            new Notification().GetFirebaseNotificationCallRejection(UserName, ReceiverId, notify, category, ConnectionString, tokenid);
        }
        public static ChatDetails GetChatDetails(string UserName, string App)
        {
            return new User().GetChatDetails(UserName, App, ConnectionString);
        }
        public static void UpdateChatDetails(string UserName, ChatDetails chatdetails)
        {
            new User().UpdateChatDetails(UserName, chatdetails, ConnectionString);
        }
        public static bool UpdateChatResource(ChatResourceDetails chatresource)
        {
            return new User().UpdateChatResource(chatresource, ConnectionString);
        }
        public static string GetChatResource(string UserName, string ToUser)
        {
            return new User().GetChatResource(UserName, ToUser, ConnectionString);
        }
        public async static Task<List<ConverationHistory>> GetAllConversations(string UserName, string ToUser, string AccountSIDValue, string AuthTokenValue,string ChatServiceSid)
        {
            return await  new User().GetAllConversationsAsync(UserName, ToUser, AccountSIDValue, AuthTokenValue, ChatServiceSid, ConnectionString);
        }
        public static void UpdInvalidSessionZero(string jwtToken)
        {
            new User().UpdInvalidSessionZero(jwtToken, ConnectionString);
        }
        public static List<string> GetAllLoginSessions()
        {
            return new User().GetAllLoginSessions(ConnectionString);
        }
        public static  void UpdatePatientSmsDetails(string UserName,SaveSmsInfo Info)
        {
            new Patient().UpdatePatientSmsDetails(UserName,Info, ConnectionString);
        }
        

        public static List<PatientCareTeamMembers> GetPatientCareteamMembers(string CreatedBy)
        {
            return new CareTeam().GetPatientCareteamMembers(CreatedBy, ConnectionString);
        }

        public static bool UpdateChatWebhook(chathook hook)
        {
           return new Patient().UpdateChatWebhook(hook, ConnectionString);
        }

        public static void UpdateIncomingSmsDetails(smshook hook)
        {
             new Patient().UpdateIncomingSmsDetails(hook, ConnectionString);
        }

        public static List<GetSmsInfo> GetPatientsSmsDetails(int PatientId, int PatientProgramId, DateTime StartDate, DateTime EndDate, string CreatedBy)
        {
            return new Patient().GetPatientSmsDetails(PatientId, PatientProgramId, StartDate, EndDate, CreatedBy, ConnectionString);
        }
        public static void NotifyPatientStatusChange(string status, PatientDetails patient, string UserName)
        {
            new Notification().NotifyPatientStatusChange(status, patient, UserName, ConnectionString);
        }
        public static ChatDetails GenerateChatToken(ChatDetails chatdetails, string UserName, string Application)
        {
            return new CommServices().GenerateChatToken(chatdetails, UserName, Application, ConnectionString);
        }
        public static DateTime? GetExpiryFromJwt(string token)
        {
            return new CommServices().GetExpiryFromJwt(token);
        }
        public static bool UpdateUserConversationActivity(string username, string activeConversationSid, DateTimeOffset lastActiveAt, string actor)
        {
            return new User().UpdateUserConversationActivity(username, activeConversationSid, lastActiveAt, actor, ConnectionString);
        }
        public static void NotifyConversation(string activeConversationSid, string FromUser, string ToUser, string Message)
        {
            new User().NotifyConversation(activeConversationSid, FromUser, ToUser, Message, ConnectionString);
        }
		 public static string GetDeviceType(string DeviceModel)
        {
            return new PatientDevice().GetDeviceType(DeviceModel, ConnectionString);
        }
        public static bool MakeDeviceAvailable(string deviceNumber)
        {
            return new PatientDevice().MakeDeviceAvailable(deviceNumber, ConnectionString);
        }

    }

}
