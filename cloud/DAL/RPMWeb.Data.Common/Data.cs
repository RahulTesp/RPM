using System.ComponentModel.DataAnnotations;

namespace RPMWeb.Data.Common
{
    public class CareTeamBaseInfo
    {
        public int TeamId { get; set; }
        public string TeamName { get; set; }
        public int ManagerUserId { get; set; }
    }
    public class CareTeamMember
    {
        public int UserId { get; set; }
        public string Role { get; set; }
        public string MemberFirstName { get; set; }
        public string MemberLastName { get; set; }
        public int MemberPatientCount { get; set; }
        public int MemberDischargePatientCount { get; set; }
    }
    public class CareTeamInfo : CareTeamBaseInfo
    {
        public CareTeamInfo() { TeamMembers = new List<CareTeamMember>(); }
        public string ManagerFirstName { get; set; }
        public string ManagerLastName { get; set; }
        public int ManagerPatientCount { get; set; }
        public List<CareTeamMember> TeamMembers { get; set; }
    }
    public class CareTeams
    {
        public int careTeamId { get; set; }
        public string Name { get; set; }
        public int ManagerId { get; set; }
        public List<int> MemberUserId { get; set; }
        public string CreatedBy { get; set; }

    }
    public class NonAssignedCM
    {
        public int CareTeamMemberUserId { get; set; }
        public string UserName { get; set; }
        public string Name { get; set; }

    }
    public class PgmNGoalsData
    {
        public string PgmName { get; set; }
    }
    public class PgmGoals
    {

        public int ProgramId { get; set; }
        public List<GoalDetails> goalDetails { get; set; }
        public string CreatedBy { get; set; }

    }
    public class GetPgmGoals
    {
        public int ProgramId { get; set; }
        public string ProgramName { get; set; }
        public int Duration { get; set; }
        public List<Vitals> Vitals { get; set; }
        public List<GoalDetails> goalDetails { get; set; }

    }
    public class Vitals
    {
        public int VitalId { get; set; }
        public string VitalName { get; set; }
    }
    public class GoalDetails
    {
        public GoalDetails(int id, string goal, string description)
        {
            Id = id;
            Goal = goal;
            Description = description;
        }

        public int Id { get; set; }
        public string Goal { get; set; }
        public string Description { get; set; }
    }

    public class ProgramDiagnostics
    {
        public int Id { get; set; }
        public string DiagnosisName { get; set; }
        public string DiagnosisCode { get; set; }
    }
    public class Data
    {
    }
    public class VerifyUserName
    {
        public string UserName { get; set; }
        public string? CreatedBy { get; set; }
    }
    public class Login
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public string CreatedBy { get; set; }
        public string OTP { get; set; }
        public string token { get; set; }
    }
    public class RPMLogin
    {
        [Required]
        public string? UserName { get; set; }

        [Required]
        public string? Password { get; set; }

        public string? CreatedBy { get; set; }
        public string? OTP { get; set; }
        public string? token { get; set; }
    }
    public class Logout
    {
        public string JwtToken { get; set; }
        public string createdBy { get; set; }
    }
    public class UnlockUser
    {
        public int? Patientid { get; set; }
        public int? UserId { get; set; }

        public bool isLocked { get; set; }
    }

    public class ResetPassword
    {
        public string Username { get; set; }
        public string password { get; set; }

        public string Otp { get; set; }


    }
    public class ContactDetails
    {
        public string MailId { get; set; }
        public string MobileNumber { get; set; }

        public string FromMail { get; set; }

        public string Password { get; set; }
    }
    public class LoginResponseToken
    {
        public string tkn { get; set; }
        public string tkt { get; set; }

        public bool reqPasswordchange { get; set; }

        public List<Roles> Roles { get; set; }

        public bool MFA { get; set; }

        public string Mobilenumber { get; set; }

        public string MailId { get; set; }

        public int TimeLimit { get; set; }
        public bool ValidMailId { get; set; }

        public bool ValidMobile { get; set; }

    }

    
    public class LoginDetails
    {
        public string Username { get; set; }
        public string OTP { get; set; }
        public string Token { get; set; }
        public int RetryCount { get; set; }

        public string Match { get; set; }

    }
    public class UserRoleConfig
    {
        public int RoleId { get; set; }
        public bool IsMailSend { get; set; }
        public bool IsSmsSend { get; set; }


    }
    public class LoginResponse
    {
        public string tkn { get; set; }
        public string tkt { get; set; }
        public bool reqPasswordchange { get; set; }
        public List<Roles> Roles { get; set; }
        public bool MFA { get; set; }

        


    }
    public class Updatepassword
    {
        public string UserName { get; set; }
        public string NewPassword { get; set; }
        public string OldPassword { get; set; }
    }
    public class OperationalMasterData
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
    }
    public class Roles
    {
        public int Id { get; set; }
        public string Role { get; set; }

        public string ProgramName { get; set; }

    }

    public class AccessRights
    {
        public string Category { get; set; }
        public int AccessId { get; set; }
        public string AccessName { get; set; }
        public string AccessRight { get; set; }
    }
    public class UserPermission
    {
        public string Username { get; set; }
        public string UserRole { get; set; }
        public List<AccessRights> UserAccessRights { get; set; }

    }
    public class PatientNProgramDetails
    {
        public GetPatientDetails PatientDetails { get; set; }
        public GetPatientProgramdetails PatientProgramdetails { get; set; }
        public GetPatientProgramGoals PatientProgramGoals { get; set; }
        public GetPatientPrescribtionDetails PatientPrescribtionDetails { get; set; }
        public GetPatientEnrolledDetails PatientEnrolledDetails { get; set; }
        public PatientStatusInfo ActivePatientDetails { get; set; }
        public PatientStatusInfo ReadyForDischargePatientDetails { get; set; }
        public PatientStatusInfo OnHoldPatientDetais { get; set; }
        public PatientStatusInfo InActivePatientDetais { get; set; }
        public PatientStatusInfo DischargedPatientDetails { get; set; }
        public GetPatientDevicesDetails PatientDevicesDetails { get; set; }
        public GetPatientVitalDetails PatientVitalDetails { get; set; }
        public GetPatientInsurenceDetails PatientInsurenceDetails { get; set; }
        public GetPatientDocumentDetails PatientDocumentDetails { get; set; }
        public ProileSummary ProfileSummary { get; set; }

    }
    public class GetPatientDetails
    {
        public string UserName { get; set; }
        public int UserId { get; set; }
        public int OrganizationID { get; set; }
        public string MobileNo { get; set; }
        public string @AlternateMobNo { get; set; }
        public string Email { get; set; }
        public string Status { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public DateTime DOB { get; set; }
        public string Gender { get; set; }
        public float Height { get; set; }
        public float Weight { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string CityName { get; set; }
        public int CityId { get; set; }
        public int StateId { get; set; }
        public string State { get; set; }
        public int CountryId { get; set; }
        public string Picture { get; set; }
        public string ZipCode { get; set; }

        public string TimeZone { get; set; }
        public int TimeZoneID { get; set; }
        public int UTCDifference { get; set; }
        public string Contact1Name { get; set; }
        public string Contact1RelationName { get; set; }
        public string Contact1Phone { get; set; }
        public string Contact2Name { get; set; }
        public string Contact2RelationName { get; set; }
        public string Contact2Phone { get; set; }
        public string CallTime { get; set; }
        public string Preference1 { get; set; }
        public string Preference2 { get; set; }
        public string Preference3 { get; set; }
        public string Notes { get; set; }
        public string Language { get; set; }
    }
    public class GetPatientProgramdetails
    {
        public int PatientProgramId { get; set; }
        public int ProgramId { get; set; }
        public string ProgramName { get; set; }
        public int CareTeamUserId { get; set; }
        public string AssignedMember { get; set; }
        public int ManagerId { get; set; }
        public string Manager { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int Duration { get; set; }
        public string ProgramStatus { get; set; }
        public string Status { get; set; }
        public int TargetReadings { get; set; }
        public List<GetPatientVitalInfo> PatientVitalInfos { get; set; }
    }

    public class GetPatientVitalInfo
    {
        //ccm change
        public GetPatientVitalInfo(string _vital, int _vitalId, bool _sel)
        {
            Vital = _vital;
            VitalId = _vitalId;
            Selected = _sel;
        }
        public string Vital { get; set; }
        public int VitalId { get; set; }
        public bool Selected { get; set; }
    }

    public class GetPatientProgramGoals
    {
        public List<GoalDetails> goalDetails { get; set; }
    }
    public class GetPatientPrescribtionDetails
    {
        public string Status { get; set; }
        public DateTime PrescribedDate { get; set; }
        public int PhysicianId { get; set; }
        public string Physician { get; set; }
        public string ConsultationDate { get; set; }//fix

       // public DateTime ConsultationDate { get; set; }//fix
        public string Clinic { get; set; }
        public string ClinicCode { get; set; }
        public string Branch { get; set; }
        public List<PatientDiagnosisInfo> PatientDiagnosisInfos { get; set; }

    }
    public class PatientDiagnosisInfo
    {
        public PatientDiagnosisInfo(int _id, string _name, string _code)
        {
            Id = _id;
            DiagnosisName = _name;
            DiagnosisCode = _code;
        }
        public int Id { get; set; }
        public string DiagnosisName { get; set; }
        public string DiagnosisCode { get; set; }
    }
    public class GetPatientEnrolledDetails
    {
        public string Status { get; set; }
        public DateTime AssignedDate { get; set; }
        public List<PatientEnrolledInfo> patientEnrolledInfos { get; set; }
    }
    public class PatientEnrolledInfo
    {
        public PatientEnrolledInfo(string _ep)
        {
            EnrollmentPersonal = _ep;

        }
        public string EnrollmentPersonal { get; set; }

    }
    public class PatientStatusInfo
    {
        public string Status { get; set; }
        public int AssigneeId { get; set; }
        public string AssigneeName { get; set; }
        public int ManagerId { get; set; }
        public string ManagerName { get; set; }
        public DateTime AssignedDate { get; set; }
    }
    public class GetPatientDevicesDetails
    {
        public List<PatientDeviceInfo> PatientDeviceInfos { get; set; }
    }

    public class PatientDeviceInfo
    {
        public PatientDeviceInfo(string _vitalName, int _vId, string _deviceName, int _deviceVid, string _deviceNo, string _deviceStatus, string _deviceCT, string _deviceModel, string _deviceVUID, string _deviceVUname)
        {
            VitalName = _vitalName;
            VitalId = _vId;
            DeviceName = _deviceName;
            DeviceVendorId = _deviceVid;
            DeviceNumber = _deviceNo;
            DeviceStatus = _deviceStatus;
            DeviceCommunicationType = _deviceCT;
            DeviceModel = _deviceModel;
            DeviceVendorUserId = _deviceVUID;
            DeviceVendorUserName = _deviceVUname;

        }
        public string VitalName { get; set; }
        public int VitalId { get; set; }
        public string DeviceName { get; set; }
        public int DeviceVendorId { get; set; }
        public string DeviceNumber { get; set; }
        public string DeviceStatus { get; set; }
        public string DeviceCommunicationType { get; set; }
        public string DeviceModel { get; set; }
        public string DeviceVendorUserId { get; set; }
        public string DeviceVendorUserName { get; set; }
    }
    public class GetPatientVitalDetails
    {
        public List<PatientVitalInfo> PatientVitalInfos { get; set; }

    }
    public class PatientVitalInfo
    {
        public string VitalName { get; set; }
        public int VitalId { get; set; }
        public int ScheduleId { get; set; }
        public string ScheduleName { get; set; }
        public string Schedule { get; set; }
        public int VitalScheduleId { get; set; }
        public string VitalScheduleName { get; set; }
        public bool Morning { get; set; }
        public bool Afternoon { get; set; }
        public bool Evening { get; set; }
        public bool Night { get; set; }
        public List<VitalMeasureInfo> VitalMeasureInfos { get; set; }
    }
    public class VitalMeasureInfo
    {
        public int Id { get; set; }
        public int DeviceVitalMeasuresId { get; set; }
        public string MeasureName { get; set; }
        public string UnitName { get; set; }
        public int MeasureOrder { get; set; }
        public float CriticallMinimum { get; set; }
        public float CautiousMinimum { get; set; }
        public float NormalMinimum { get; set; }
        public float NormalMaximum { get; set; }
        public float CautiousMaximum { get; set; }
        public float CriticalMaximum { get; set; }
    }

    public class GetPatientInsurenceDetails
    {
        public List<PatientInsurenceInfo> PatientInsurenceInfos { get; set; }
    }
    public class PatientInsurenceInfo
    {
        public PatientInsurenceInfo(int _id, int _insVId, string _insVName, bool _isPrimary)
        {
            Id = _id;
            InsuranceVendorId = _insVId;
            InsuranceVendorName = _insVName;
            IsPrimary = _isPrimary;
        }
        public int Id { get; set; }
        public int InsuranceVendorId { get; set; }
        public string InsuranceVendorName { get; set; }
        public bool IsPrimary { get; set; }

    }
    public class GetPatientDocumentDetails
    {
        public List<PatientDocumentinfo> PatientDocumentinfos { get; set; }
    }
    public class PatientDocumentinfo
    {
        public PatientDocumentinfo(int _id, string _dType, string _dName, DateTime _createdOn, string _dUNC)
        {
            Id = _id;
            DocumentType = _dType;
            DocumentName = _dName;
            CreatedOn = _createdOn;
            DocumentUNC = _dUNC;
        }
        public int Id { get; set; }
        public string DocumentType { get; set; }
        public string DocumentName { get; set; }
        public DateTime CreatedOn { get; set; }
        public string DocumentUNC { get; set; }

    }

    public class DraftPatient
    {
        public int PatientId { get; set; }
        public string PatientName { get; set; }
        public DateTime CreatedOn { get; set; }
    }
    public class PatientDetails
    {
        
        public int PatientId { get; set; }
        public string Status { get; set; }
        public string UserName { get; set; }
        public int OrganizationID { get; set; }
        public string ClinicName { get; set; }
        public string ClinicCode { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public DateTime DOB { get; set; }
        public string Gender { get; set; }
        public float Height { get; set; }
        public float Weight { get; set; }
        public string MobileNo { get; set; }
        public string AlternateMobNo { get; set; }
        public string Email { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public int CityID { get; set; }
        public int StateId { get; set; }
        public int CountryId { get; set; }
        public string ZipCode { get; set; }
        public string Language { get; set; }
        public string Contact1Name { get; set; }
        public string Contact1RelationName { get; set; }
        public string Contact1Phone { get; set; }
        public string Contact2Name { get; set; }
        public string Contact2RelationName { get; set; }
        public string Contact2Phone { get; set; }
        public string Notes { get; set; }
        public int TimeZoneID { get; set; }
        public string Picture { get; set; }
        public string CallTime { get; set; }
        public string Preference1 { get; set; }
        public string Preference2 { get; set; }
        public string Preference3 { get; set; }

    }
    public class ResetPatientPW
    {
        public int PatientId { get; set; }
        public string? CreatedBy { get; set; }
    }
    public class SearchPatient
    {
        public int PatientId { get; set; }
        public int PatientProgramId { get; set; }
    }
    public class GetPatientInfo
    {

        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string DOB { get; set; }

        public string Gender { get; set; }
        public int Height { get; set; }
        public int Weight { get; set; }
        public string Email { get; set; }
        public string PhoneNo { get; set; }
        public string AlternateMobNo { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public int TimeZoneId { get; set; }
        public string TimeZone { get; set; }
        public string ZipCode { get; set; }
        public string EmergencyContact1 { get; set; }
        public string EmergencyContactNumber1 { get; set; }
        public string EmergencyContact1Relation { get; set; }
        public string EmergencyContact2 { get; set; }
        public string EmergencyContactNumber2 { get; set; }
        public string EmergencyContact2Relation { get; set; }
        public string CallTime { get; set; }
        public string Language { get; set; }
        public string Preference1 { get; set; }
        public string Preference2 { get; set; }
        public string Preference3 { get; set; }
        public string Notes { get; set; }
    }

    public class PatientProgramDetailsInsert
    {
        public int PatientId { get; set; }
        public int PatientProgramId { get; set; }
        public int ProgramId { get; set; }
        public List<int> VitalIds { get; set; }//ccm change
        public int PhysicianId { get; set; }
        public string? ConsultationDate { get; set; }
        public int CareTeamUserId { get; set; }
        public string PatientStatus { get; set; }
        public DateTime PrescribedDate { get; set; }
        public int TargetReadings { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        //public string CreatedBy { get; set; }
        public GoalDetails[] GoalDetails { get; set; }
        public ProgramDiagnostics[] ProgramDiagnosis { get; set; }
    }
    public class UpdateProgramDetails
    {
        public int PatientId { get; set; }
        public int PatientProgramId { get; set; }
        public int ProgramId { get; set; }
        public List<int> VitalIds { get; set; }//ccm change
        public string CreatedBy { get; set; }
        public GoalDetails[] GoalDetails { get; set; }
        public ProgramDiagnostics[] ProgramDiagnosis { get; set; }
    }
    public class PatientProgramRenew
    {
        public int PatientId { get; set; }
        public int PatientProgramId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string CreatedBy { get; set; }

    }
    public class PatientProgramDetailsUpdate
    {
        public int PatientId { get; set; }
        public int PatientProgramId { get; set; }
        public List<int> VitalIds { get; set; }//ccm change
        public int PhysicianId { get; set; }
        public string ConsultationDate { get; set; }
        public string PrescribedDate { get; set; }
        public string EnrolledDate { get; set; }
        public int CareTeamUserId { get; set; }
        public string PatientStatus { get; set; }
        public int TargetReadings { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string CreatedBy { get; set; }
        public GoalDetails[] patientProgramGoals { get; set; }
        public ProgramDiagnostics[] PatientProgramDiagnosis { get; set; }
        //public PatientDeviceDetails[] PatientDeviceDetails { get; set; }
        public PatientVitalDetails[] PatientVitalDetails { get; set; }
        public PatientInsurenceDetails[] PatientInsurenceDetails { get; set; }

    }
    public class PatientDeviceDetails
    {
        public int Id { get; set; }

    }
    public class PatientVitalDetails
    {
        public string VitalName { get; set; }
        public int VitalId { get; set; }
        public string ScheduleName { get; set; }
        public string Schedule { get; set; }
        public int ScheduleId { get; set; }
        public int VitalScheduleId { get; set; }
        public int NoOfTimes { get; set; }
        public bool Morning { get; set; }
        public bool Afternoon { get; set; }
        public bool Evening { get; set; }
        public bool Night { get; set; }
        public List<VitalMeasureInfos> VitalMeasureInfos { get; set; }

    }
    public class VitalMeasureInfos
    {
        public int Id { get; set; }
        public int DeviceVitalMeasuresId { get; set; }
        public string MeasureName { get; set; }
        public int MeasureOrder { get; set; }
        public float NormalMinimum { get; set; }
        public float NormalMaximum { get; set; }
        public float CautiousMinimum { get; set; }
        public float CautiousMaximum { get; set; }
        public float criticallMinimum { get; set; }
        public float criticalMaximum { get; set; }
    }
    public class ProgramDetailsMasterDataAddPatient
    {
        public List<ProgramDetailsMasterDataProgram> ProgramDetailsMasterData { get; set; }
        public List<ClinicDetails> ClinicDetails { get; set; }
        public List<PhysicianDetails> PhysicianDetails { get; set; }
        public List<CareTeamMembers> CareTeamMembers { get; set; }
        public List<PatientsStatus> PatientStatuses { get; set; }
        public List<ScheduleList> ScheduleLists { get; set; }
        public List<VitalScheduleList> VitalScheduleLists { get; set; }
        public List<BillingCodes> BillingCodes { get; set; }
        public List<DeviceCommunicationType> DeviceCommunicationTypes { get; set; }
        public List<DeviceDetails> DeviceDetails { get; set; }
        public List<InsurenceDetails> InsurenceDetails { get; set; }
    }
    public class ClinicDetails
    {
        public int Id { get; set; }
        public string ClinicName { get; set; }
        public string ClinicCode { get; set; }
    }
    public class PhysicianDetails
    {
        public int UserId { get; set; }
        public int ClinicID { get; set; }
        public string PhysicianName { get; set; }
    }
    public class Cities
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int StateId { get; set; }
        public string State { get; set; }
        public string StateCode { get; set; }
        public string Zipcode { get; set; }
        public int TimeZoneId { get; set; }
    }
    public class CareTeamMembers
    {
        public int UserId { get; set; }
        public int CareTeamId { get; set; }
        public string CareTeamMemberName { get; set; }
    }
    public class PatientsStatus
    {
        public int Id { get; set; }
        public string PatientStatus { get; set; }
    }
    public class ProgramDetailsMasterDataProgram
    {
        public int ProgramId { get; set; }
        public string ProgramName { get; set; }
        public List<ProgramVitals> Vitals { get; set; }
        public List<GoalDetails> goalDetails { get; set; }
        public int Duration { get; set; }
        public string Name { get; set; }

    }
    public class PatientProgramDetails
    {
        public int Id { get; set; }
        public string PatientUserName { get; set; }
        public string PatientFirstName { get; set; }
        public string PatientLastName { get; set; }
        public int CareTeamMemberId { get; set; }
        public string CareTeamMemberUserName { get; set; }
        public DateTime ProgramStartDate { get; set; }
        public DateTime ProgramEndDate { get; set; }
        public string ProgramStatus { get; set; }

    }
    public class PatientAllPrograms
    {
        public int PatientId { get; set; }
        public int PatientProgramId { get; set; }
        public int ProgramId { get; set; }
        public string ProgramName { get; set; }
        public bool IsActive { get; set; }
        public DateTime StartDate { get; set; }
    }
    public class ScheduleList
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
    public class VitalScheduleList
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int VitalId { get; set; }
    }
    public class BillingCodes
    {
        public int Id { get; set; }
        public string BillingCode { get; set; }
        public string FrequencyPeriod { get; set; }
        public int TargetReadings { get; set; }
        public string TargetDuration { get; set; }
        public string Description { get; set; }
    }
    public class DeviceCommunicationType
    {
        public int Id { get; set; }
        public string DeviceCommunicationTypeName { get; set; }
    }
    public class DeviceDetails
    {
        public int VitalId { get; set; }
        public string DeviceName { get; set; }
        public int DeviceVendorId { get; set; }
        public string DeviceNumber { get; set; }
        public string DeviceModel { get; set; }
        public string DeviceStatus { get; set; }
        public int DeviceCommunicationTypeId { get; set; }
    }
    public class InsurenceDetails
    {
        public int VendorId { get; set; }
        public string VendorName { get; set; }
    }
    public class ProgramVitals
    {
        public int VitalId { get; set; }
        public string VitalName { get; set; }
        public List<ProgramVitalDignostics> Dignostics { get; set; }
    }
    public class ProgramVitalDignostics
    {
        public string DiagnosisCode { get; set; }
        public string DiagnosisName { get; set; }
    }
    public class PatientInsurenceDetails
    {
        public int Id { get; set; }
        public string InsuranceVendorName { get; set; }
        public bool IsPrimary { get; set; }

    }
    public class PatientSymptom
    {
        public int Id { get; set; }
        public int PatientId { get; set; }
        public int PatientProgramId { get; set; }
        public string? Symptom { get; set; }
        public string? Description { get; set; }
        public DateTime SymptomStartDateTime { get; set; }
        public string? CreatedBy { get; set; }

    }
    public class GetPatientSymptom
    {
        public int Id { get; set; }
        public string Symptom { get; set; }
        public string Description { get; set; }
        public DateTime SymptomStartDateTime { get; set; }

    }
    public class Symptom
    {
        public int Id { get; set; }
        public string Symptoms { get; set; }
    }
    public class PatientMedication
    {
        public int Id { get; set; }
        public int PatientId { get; set; }
        public int PatientProgramId { get; set; }
        public string Medicinename { get; set; }
        public string MedicineSchedule { get; set; }
        public string BeforeOrAfterMeal { get; set; }
        public bool Morning { get; set; }
        public bool AfterNoon { get; set; }
        public bool Evening { get; set; }
        public bool Night { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Description { get; set; }
        public string CreatedBy { get; set; }

    }
    public class GetPatientMedication
    {
        public int Id { get; set; }
        public string Medicinename { get; set; }
        public string MedicineSchedule { get; set; }
        public string BeforeOrAfterMeal { get; set; }
        public bool Morning { get; set; }
        public bool AfterNoon { get; set; }
        public bool Evening { get; set; }
        public bool Night { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Description { get; set; }

    }
    public class UserProfiles
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public int[] RoleId { get; set; }
        public int OrganizationID { get; set; }
        public string MobileNo { get; set; }
        public string Email { get; set; }
        public int SupervisorId { get; set; }
        public string Status { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public int CityId { get; set; }
        public int StateId { get; set; }
        public int CountryId { get; set; }
        public string ZipCode { get; set; }
        public int TimeZoneID { get; set; }
        public string Picture { get; set; }
        public string CreatedBy { get; set; }
    }
    public class GetUserProfiles
    {

        public int UserId { get; set; }
        public string UserName { get; set; }
        public int OrganizationID { get; set; }
        public List<int> RoleIds { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string MobileNo { get; set; }
        public string Email { get; set; }
        //public DateTime DOB { get; set; }
        //public string Gender { get; set; }
        public string Status { get; set; }

        public int CityId { get; set; }
        public int StateId { get; set; }
        public int CountryId { get; set; }
        public string ZipCode { get; set; }
        public int TimeZoneID { get; set; }
        public string Picture { get; set; }
        public bool HasPatients { get; set; }

    }
    public class PatientInfoForProgramInsert
    {
        public int OrganizationId { get; set; }
        public DateTime ConsultationDate { get; set; }
    }
    public class GetAllPatientInfo
    {
        public int PatientId { get; set; }
        public int PatientProgramId { get; set; }
        public int AssignedMemberId { get; set; }
        public int ClinicId { get; set; }
        public string ClinicName { get; set; }
        public string PatientNumber { get; set; }
        public string PatientName { get; set; }
        public string ProgramName { get; set; }
        public string Program { get; set; }
        public string EnrolledDate { get; set; }
        public string PhysicianName { get; set; }
        public string AssignedMember { get; set; }
        public string PatientType { get; set; }
        public List<VitalInfo> VitalInfo { get; set; }
        public string Priority { get; set; }
        public GetAllPatientInfo()
        {
            VitalInfo = new List<VitalInfo>();
        }
    }
    public class VitalInfo
    {
        public string Vital { get; set; }
        public string VitalPriority { get; set; }
        public int AlertTypeId { get; set; }
    }
    public class GetAllPatientSmsInfo
    {
        public int Id { get; set; }
        public int PatientId { get; set; }
        public int PatientProgramId { get; set; }
        public string PatientName { get; set; }
        public string ProgramName { get; set; }

        public string FromNumber { get; set; }
        public string Message { get; set; }

        public string Senddate { get; set; }  
       
    }
    public class ClinicInfo
    {
        public string Name { get; set; }
        public string Code { get; set; }
        public int ParentOrganizationID { get; set; }
        public string PhoneNumber { get; set; }
        public string AlternateNumber { get; set; }
        public string AddrLine1 { get; set; }
        public string AddrLine2 { get; set; }
        public string ZipCode { get; set; }
        public int CityId { get; set; }
        public int StateId { get; set; }
        public int CountryId { get; set; }
        public int TimeZoneId { get; set; }


    }
    public class UpdClinicInfo : ClinicInfo
    {
        public int Id { get; set; }
    }
    public class ClinicAllInfo
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool Active { get; set; }
        public int CityId { get; set; }
        public int StateId { get; set; }
        public string CityName { get; set; }
        public int CountryId { get; set; }
        public int PhysicianCount { get; set; }
        public int PatientCount { get; set; }
    }
    public class TaskInfo
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public int TaskTypeId { get; set; }
        public int PatientId { get; set; }
        public int CareteamMemberUserId { get; set; }
        public DateTime DueDate { get; set; }
        public int PriorityId { get; set; }
        public string? Status { get; set; }
        public int? WatcherUserId { get; set; }
        public string? Comments { get; set; }
        public string? CreatedBy { get; set; }
    }
    public class TaskResponse
    {
        public int AlertId { get; set; }
        public int RoleId { get; set; }
        public string AlertStatus { get; set; }
        public int UserId { get; set; }
        public string Comments { get; set; }
        public string? CreatedBy { get; set; }

    }
        public class ScheduleInfo
        {
            public int Id { get; set; }
            public int CurrentScheduleId { get; set; }
            public bool IsPatient { get; set; }
            public string? Schedule { get; set; }
            public int ScheduleTypeId { get; set; }
            public bool Mon { get; set; }
            public bool Tue { get; set; }
            public bool Wed { get; set; }
            public bool Thu { get; set; }
            public bool Fri { get; set; }
            public bool Sat { get; set; }
            public bool Sun { get; set; }
            public int WeekSelection { get; set; }
            public DateTime CurrentScheduleDate { get; set; }
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
            public string? StartTime { get; set; }
            public string? CurrentScheduleStartTime { get; set; }
            public int Duration { get; set; }
            public int CurrentScheduleDuration { get; set; }
            public string? Comments { get; set; }
            public string? CurrentScheduleComments { get; set; }
            public int AssignedTo { get; set; }
            public string? AssigneeName { get; set; }
            public int AssignedBy { get; set; }
            public string? CreatedBy { get; set; }
            public bool IsCompleted { get; set; }

        }
    public class CurrentScheduleInfo
    {
        public int CurrentScheduleId { get; set; }
        public DateTime ScheduleDate { get; set; }
        public string StartTime { get; set; }
        public int Duration { get; set; }
        public string Comments { get; set; }
        public bool IsCompleted { get; set; }
        public string CreatedBy { get; set; }
    }
    public class CompletedSchedules
    {
        public List<int> IDs { get; set; }
        public string ModifiedBy { get; set; }
    }
    public class NoteInfo
    {
        public int Id { get; set; }
        public int NoteTypeId { get; set; }
        public int PatientId { get; set; }
        public int PatientProgramId { get; set; }
        public int CareteamMemberUserId { get; set; }
        public bool IsCallNote { get; set; }
        public bool IsEstablishedCall { get; set; }
        public int VitalId { get; set; }
        public List<VitalNotes> Notes { get; set; }
        public string Duration { get; set; }
        public string CreatedBy { get; set; }

    }
    public class GetNotes
    {
        public int Id { get; set; }
        public int NoteTypeId { get; set; }
        public string NoteType { get; set; }
        public int PatientId { get; set; }
        public int CareteamMemberUserId { get; set; }
        public bool IsEstablishedCall { get; set; }
        public string CompletedBy { get; set; }
        public bool IsCallNote { get; set; }
        public int Duration { get; set; }
        public DateTime CreatedOn { get; set; }
    }
    public class GetAllNotes
    {
        public int Id { get; set; }
        public int NoteTypeId { get; set; }
        public string NoteType { get; set; }
        public int PatientId { get; set; }
        public int PatientProgramId { get; set; }
        public int CareteamMemberUserId { get; set; }
        public bool IsPhoneCall { get; set; }
        public bool IsCareGiver { get; set; }
        public bool IsEstablished { get; set; }
        public string CompletedBy { get; set; }
        public int Duration { get; set; }
        public DateTime CreatedOn { get; set; }
        public string CallType { get; set; }
    }
    public class NoteAnswers
    {
        public int AnsId { get; set; }
        public string AnsDescription { get; set; }
        public bool Check { get; set; }

    }
    public class NoteQuestion
    {
        public int QuestionId { get; set; }
        public string questiondescription { get; set; }
        public List<NoteAnswers> NoteAnswers { get; set; }
    }
    public class DashboardPatientStatusList
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public DashboardPatientStatus status { get; set; }
    }

    public class DashboardPatientStatusList1
    {
        public string Name { get; set; }
        public int Id { get; set; }
    }
    public class DashboardPatientStatus
    {
        public int Prescribed { get; set; }
        public int Enrolled { get; set; }
        public int Active { get; set; }
        public int InActive { get; set; }
        public int OnHold { get; set; }
        public int ReadyForDischarge { get; set; }
        public int Discharged { get; set; }
        public int Total { get; set; }
    }
    public class DashboardVitalsList
    {
        public string VitalName { get; set; }
        public Priority Priorities { get; set; }
    }
    public class DashboardAlerts
    {
        public int Id { get; set; }

        public int Index { get; set; }
        public int PatientId { get; set; }
        public string PatientName { get; set; }
        public int PatientProgramId { get; set; }
        public string VitalAlert { get; set; }
        public string Priority { get; set; }
        public int AssignedToCareTeamUserId { get; set; }
        public DateTime Time { get; set; }
    }
    public class Priority
    {
        public int Critical { get; set; }
        public int Cautious { get; set; }
        public int Normal { get; set; }

    }
    public class WorldTimeZone
    {
        public string Name { get; set; }
        public string CurrentUtcOffset { get; set; }
        public bool IsCurrentlyDst { get; set; }
    }
    public class iGlucoseConfig
    {
        public string CreateAccUrl { get; set; }
        public string AddDeviceUrl { get; set; }
        public string ValidateDeviceUrl { get; set; }
        public string RemoveDeviceUrl { get; set; }
        public string ApiKey { get; set; }

    }
    public class DeviceRegister
    {
        public int PatientId { get; set; }
        public int VendorId { get; set; }
        public string DeviceId { get; set; }
        public string DeviceModel { get; set; }
        public string PatientNumber { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int TimeZoneOffset { get; set; }
    }
    public class ReRegisterDevice
    {
        public int PatientId { get; set; }
        public string DeviceSerialNo { get; set; }
        public string PatientNumber { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int TimeZoneOffset { get; set; }
        public int VendorId { get; set; }
        public string DeviceModel { get; set; }

    }
    public class UpdateDeviceStatus
    {
        public string DeviceStatus { get; set; }
        public string DeviceSerialNo { get; set; }
        public string CreatedBy { get; set; }
    }
    public class PatientDeviceConnectivityInfo
    {
        public string DeviceVendorUserid { get; set; }
        public string Password { get; set; }
        public string PatientNumber { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string VendorName { get; set; }

    }
    public class HttpResponse
    {
        public int HttpRetCode { get; set; }
        public string Message { get; set; }
    }
    public class RegisterDeviceResponse : HttpResponse
    {
        public string DevicUserId { get; set; }
    }
    public class GetAlert
    {
        public int Id { get; set; }
        public string PatientName { get; set; }
        public int PatientId { get; set; }
        public string AlertType { get; set; }
        public string Description { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime CreatedOn { get; set; }
        public string Priority { get; set; }
        public int PriorityId { get; set; }
        public string Status { get; set; }
        public string AssignedMember { get; set; }
        public int AssignedMemberId { get; set; }
        public int CareTeamId { get; set; }
        public string Comments { get; set; }
        public List<TeamMember> Members { get; set; }
    }
    public class TeamMember
    {
        public int Userid { get; set; }
        public string Member { get; set; }
    }
    public class GetTask
    {
        public int Id { get; set; }
        public string PatientName { get; set; }
        public int PatientId { get; set; }
        public string TaskType { get; set; }
        public int TaskTypeId { get; set; }
        public string Description { get; set; }
        public DateTime DueDate { get; set; }
        public string Priority { get; set; }
        public int PriorityId { get; set; }
        public string Status { get; set; }
        public string AssignedMember { get; set; }
        public int AssignedMemberId { get; set; }
        public int CareTeamId { get; set; }
        public string Comments { get; set; }
        public List<TeamMember> Members { get; set; }
    }
    public class SystemConfigInfo
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public string Descripiton { get; set; }

    }
    public class VitalReadings
    {
        public List<BloodPressure> BloodPressure { get; set; }
        public List<BloodGlucose> BloodGlucose { get; set; }
        public List<Weight> Weight { get; set; }
        public List<BloodOxygen> BloodOxygen { get; set; }
    }

    public class PatientBillReportDetailsCCM
    {
        public string DOS { get; set; }
        public string PatientName { get; set; }

        public string PatientNumber { get; set; }
        public string clinic { get; set; }
        public string clinicname { get; set; }

        public string DOB { get; set; }
        public string CPT99490 { get; set; }
        public string CPT99439 { get; set; }
        public string CPT99491 { get; set; }
        public string CPT99437 { get; set; }
        public string CPT99487 { get; set; }
        public string CPT99489 { get; set; }

        public DateTime EnrolledDate { get; set; }

    }
    public class PatientBillReportDetailsPCM
    {
        public string DOS { get; set; }
        public string PatientName { get; set; }

        public string PatientNumber { get; set; }
        public string clinic { get; set; }
        public string clinicname { get; set; }

        public string DOB { get; set; }
        public string CPT99424 { get; set; }
        public string CPT99425 { get; set; }
        public string CPT99426 { get; set; }
        public string CPT99427 { get; set; }


        public DateTime EnrolledDate { get; set; }

    }
    public class PatientVitalReadings
    {
        public List<BloodPressureReading> BloodPressure { get; set; }
        public List<BloodGlucoseReading> BloodGlucose { get; set; }
        public List<WeightReading> Weight { get; set; }
        public List<BloodOxygenReading> BloodOxygen { get; set; }
    }
    public class BloodPressure
    {
        public DateTime ReadingDate { get; set; }
        public List<BloodPressureReading> BloodPressureReadings { get; set; }
    }
    public class BloodPressureReading
    {
        public DateTime ReadingTime { get; set; }
        public int Systolic { get; set; }
        public int Diastolic { get; set; }
        public int pulse { get; set; }
        public string SystolicStatus { get; set; }
        public string DiastolicStatus { get; set; }
        public string pulseStatus { get; set; }
        public string Status { get; set; }
        public string Remarks { get; set; }
    }
    public class BloodGlucose
    {
        public DateTime ReadingDate { get; set; }
        public List<BloodGlucoseReading> BloodGlucoseReadings { get; set; }
    }
    public class BloodGlucoseReading
    {
        public DateTime ReadingTime { get; set; }
        public string Schedule { get; set; }
        public int BGmgdl { get; set; }
        public string Status { get; set; }
        public string Remarks { get; set; }
    }
    public class Weight
    {
        public DateTime ReadingDate { get; set; }
        public List<WeightReading> WeightReadings { get; set; }
    }
    public class WeightReading
    {
        public DateTime ReadingTime { get; set; }
        public string Schedule { get; set; }
        public float BWlbs { get; set; }
        public string Status { get; set; }
        public string Remarks { get; set; }
    }
    public class BloodOxygen
    {
        public DateTime ReadingDate { get; set; }
        public List<BloodOxygenReading> BloodOxygenReadings { get; set; }
    }
    public class BloodOxygenReading
    {
        public DateTime ReadingTime { get; set; }
        public int Pulse { get; set; }
        public int Oxygen { get; set; }
        public string PulseStatus { get; set; }
        public string OxygenStatus { get; set; }
        public string Status { get; set; }
        public string Remarks { get; set; }
    }
    public class HealthTrends
    {
        public string VitalName { get; set; }
        public int VitalId { get; set; }
        public List<DateTime> Time { get; set; }

        public List<Values> Values { get; set; }
        public VitalMeasure LatestVitalMeasure { get; set; }


    }
    public class PatientHealthTrends
    {
        public List<HealthTrends> Values { get; set; }
    }
    public class Values
    {
        public List<string> data { get; set; }
        public string label { get; set; }
    }
    public class GetSchedules
    {
        public DateTime ScheduleDate { get; set; }
        public List<SchedueInfo> SchedueInfos { get; set; }
    }
    public class Programs
    {
        public string Name { get; set; }
        public string ProgramType { get; set; }
    }
    public class vitalIdsList
    {
        public List<int> VitalIds { get; set; }
    }
    public class SchedueInfo
    {
        public int Id { get; set; }
        public int CurrentScheduleId { get; set; }
        public int PatientId { get; set; }
        public int PatientProgramId { get; set; }

        public string ProgramName { get; set; }
        public string ScheduleTime { get; set; }
        public string ScheduleType { get; set; }
        public string Description { get; set; }
        public string ContactName { get; set; }
        public int AssignedBy { get; set; }
        public string AssignedByName { get; set; }
        public bool IsCompleted { get; set; }
    }
    public class GetAlertsAndTasks
    {
        public int Id { get; set; }
        public int PatientId { get; set; }
        public string PatientName { get; set; }
        public int PatientProgramId { get; set; }
        public string Type { get; set; }
        public string Priority { get; set; }
        public string CreatedBy { get; set; }
        public int AssignedToId { get; set; }
        public string AssignedMember { get; set; }
        public DateTime DueDate { get; set; }
        public string Status { get; set; }
        public string TaskOrAlert { get; set; }
    }
    public class NoteDetails
    {

        public int NoteTypeId { get; set; }
        public int IsCallNote { get; set; }
        public int IsEstablishedCall { get; set; }
        public List<VitalNotes> Notes { get; set; }
        public string Duration { get; set; }
    }
    public class VitalNotes
    {
        public int VitalId { get; set; }
        public string Notes { get; set; }
    }
    public class Schedule
    {
        public List<ScheduleTypes> ScheduleTypes { get; set; }
    }
    public class ScheduleTypes
    {
        public string ScheduleType { get; set; }
        public string Time { get; set; }

        public string Decription { get; set; }
    }
    public class ProileSummary
    {
        public string Name { get; set; }

        public string UserName { get; set; }

        public string ProgramName { get; set; }
        public string ProgramType { get; set; }
        public string CompletedDuration { get; set; }
        public int CurrentDuration { get; set; }
        public int TotalDuration { get; set; }

        public string Status { get; set; }
    }
    public class PatientSummary
    {
        public PatientSummary()
        {
            Measurements = new List<PatientMeasureCollection>();
        }
        public string VitalName { get; set; }
        public List<PatientMeasureCollection> Measurements { get; set; }

    }
    public class PatientMeasureCollection
    {
        public PatientMeasureCollection()
        {
            Time = new List<DateTime>();
            Value = new List<string>();
        }
        public string Measurement { get; set; }
        public List<DateTime> Time { get; set; }
        public List<string> Value { get; set; }
    }

    
    public class VitalSummary
    {
        public List<Vitalslist> vitals { get; set; }
    }
    public class Vitalslist
    {
        public string VitalName { get; set; }
        public List<VitalDetails> VitalDetails { get; set; }
    }
    public class VitalDetails
    {

        // public DateTime Date { get; set; }

        public List<vitalData> Vitaldata { get; set; }
    }
    public class vitalData
    {
        public string Value { get; set; }
        public string unit { get; set; }
        public string time { get; set; }

        public string MeasureName { get; set; }
    }
    public class DeviceTelemetry
    {
        public string deviceId { get; set; }
        public int createdAt { get; set; }
        public object data { get; set; }
        public bool isTest { get; set; }
    }

    public class TranstekDeviceTelemetry
    {
        public string deviceId { get; set; }
        public long createdAt { get; set; }
        public object data { get; set; }
        public object deviceData { get; set; }
        public bool isTest { get; set; }
        public string modelNumber { get; set; }
    }

    public class DeviceTelemetryStatus
    {
        public string deviceId { get; set; }
        public int createdAt { get; set; }
        public object status { get; set; }
        public bool isTest { get; set; }
    }
    public class StagingInput
    {

        public string reading_id { get; set; }
        public string device_id { get; set; }
        public string device_model { get; set; }
        public string date_recorded { get; set; }
        public string date_received { get; set; }
        public string reading_type { get; set; }
        public int battery { get; set; }
        public string time_zone_offset { get; set; }
        public string data_type { get; set; }
        public string data_unit { get; set; }
        public double data_value { get; set; }
        public bool before_meal { get; set; }
        public string event_flag { get; set; }
        public bool irregular { get; set; }
    }
    public class UploadPatientDocument
    {
        public int PatientId { get; set; }
        public int PatientProgramId { get; set; }
        public string DocumentType { get; set; }
        public string DocumentName { get; set; }
        public string Blob_Conn_String { get; set; }
        public string ContainerName { get; set; }
        public string CreatedBy { get; set; }
    }
    public class PatientBillReport
    {
        public string PatientId { get; set; }

        public string clinic { get; set; }
        public string ClinicName { get; set; }
        public string BillingCode { get; set; }
        public string PatientNumber { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Dob { get; set; }
        public string DiagnosisCode { get; set; }
        public string ProgramName { get; set; }
        public string ProgramEnrolledDate { get; set; }
        public string DeviceName { get; set; }
        public string DeviceActivateDate { get; set; }
        public string LastReadyToBillDate { get; set; }
        public string ReadyToBillDate { get; set; }

        public string Physician { get; set; }


    }
    public class BillingInfo
    {
        public string BillingCode { get; set; }
        public int Total { get; set; }
        public int TargetMet { get; set; }
        public int ReadyToBill { get; set; }
        public int MissingInfo { get; set; }
        public int OnHold { get; set; }
    }

    public class PatientBilldata
    {
        public string CPTCode { get; set; }
        public int? Total { get; set; }
        public int Completed { get; set; }

        public int ReadyTobill { get; set; }

        public bool IsTargetMet { get; set; }

        public DateTime BillingStartDate { get; set; }
        public string ProgramName { get; set; }

    }
    public class PatientDocuments
    {
        public int Id { get; set; }
        public string DocumentType { get; set; }
        public string DocumentName { get; set; }
        public string DocumentUNC { get; set; }
        public DateTime CreatedOn { get; set; }
    }
    public class PatientNotification_ins
    {
        public int PatientId { get; set; }
        public string Description { get; set; }
        public string CreatedBy { get; set; }
    }
    public class PatientNotificationReadStatus
    {
        public int Id { get; set; }
        public int PatientId { get; set; }
        public bool ReadStatus { get; set; }
        public string ModifiedBy { get; set; }
    }
    //public class SystemNotificationByUser
    //{
    //    public SystemNotificationByUser()
    //    {
    //        Data = new List<NotificationDataByDate>();
    //    }
    //    public int TotalNotifications { get; set; }
    //    public int TotalUnRead { get; set; }
    //    //public List<NotificationData> Data { get; set; }
    //    public List<NotificationDataByDate> Data { get; set; }
    //}
    public class SystemNotificationByUser
    {
        public int TotalNotifications { get; set; }
        public int TotalUnRead { get; set; }
        public List<NotificationDataByDate> Data { get; set; } = new(); // Ensure it's never null
    }
    public class SystemNotificationCount
    {
        public int TotalNotifications { get; set; }
        public int TotalUnRead { get; set; }
    }
    public class PatientNotificationData
    {
        public int Id { get; set; }
        public int Counter { get; set; }
        public string Description { get; set; }
        public DateTime CreatedOn { get; set; }
        public bool IsRead { get; set; }
    }
    public class NotificationAuditData
    {
        public int NotificationAuditId { get; set; }
        public int NotificationId { get; set; }
        public string AuditCreatedBy { get; set; }
        public bool IsRead { get; set; }
        public bool IsNotify { get; set; }
        public int PatientId { get; set; }
        public int ProgramId { get; set; }
    }
    public class NotificationData : NotificationAuditData
    {
        public string Description { get; set; }
        public DateTime CreatedOn { get; set; }
        public string Type { get; set; }
        public string SubType { get; set; }
    }
    public class NotificationDataByDate
    {
        public NotificationDataByDate()
        {
            NotificationList = new List<NotificationData>();
        }
        public DateTime NotificationDate { get; set; }
        public List<NotificationData> NotificationList { get; set; }

    }
    public class NotificationStatusUpdate
    {
        public int NotificationId { get; set; }
        public int NotificationAuditId { get; set; }
        public string ModifiedBy { get; set; }
    }
    public class SystemNotification
    {
        public SystemNotification()
        {
            NotificationData = new List<NotificationData>();
        }
        public string UserName { get; set; }
        public string UserRole { get; set; }
        public List<NotificationData> NotificationData { get; set; }

    }
    public class VitalMeasure
    {
        public string VitalName { get; set; }
        public DateTime Date { get; set; }
        public string Value { get; set; }
        public string unit { get; set; }
        public string PulseValue { get; set; }
        public string PulseUnit { get; set; }
        public string time { get; set; }

    }
    public class VitalSummaryMeasures
    {
        public List<PatientLatestSummary> PatientSummary { get; set; }
    }
    public class PatientLatestSummary
    {
        public VitalMeasure LatestVitalMeasure { get; set; }
        public List<DateTime> Time { get; set; }
        public List<Values> Values { get; set; }


    }
    public class SystemNotification_ins
    {
        public int RecId { get; set; }
        public int UserId { get; set; }
        public int NotificationTypeId { get; set; }
        public string Desc { get; set; }
        public string CreatedBy { get; set; }

    }
    public class PatientProgramData
    {
        public int PatienttId { get; set; }
        public int PatientProgramid { get; set; }
        public DateTime? ActiveDate { get; set; }

        public int TargetReading { get; set; }
        public string status { get; set; }

    }
    public class BillingCodesDetails
    {
        public int BillingCodeID { get; set; }
        public string BillingCode { get; set; }

        public int Frequency { get; set; }

        public string FrequencyPeriod { get; set; }

        public int BillingThreshold { get; set; }

        public string BillingPeriod { get; set; }

        public int TargetReadings { get; set; }
    }
    public class PatientInteraction
    {
        public int PatientProgramId { get; set; }
        public int Duration { get; set; }

        public DateTime Date { get; set; }

        public int IsCallNote { get; set; }

        public int IsEstablishedCall { get; set; }
    }
    public class VitalReading
    {
        public int programId { get; set; }
        public DateTime ReadingDate { get; set; }
        public int Totalreadings { get; set; }
    }

    public class searchPatient
    {
        public int PatientId { get; set; }
        public int PatientProgramId { get; set; }
        public string PatientNumber { get; set; }
        public string PatientName { get; set; }
        public string ProgramName { get; set; }
    }

    public class DeviceValidate
    {
        public string DeviceId { get; set; }
        public string DeviceModel { get; set; }
        public string DeviceVendor { get; set; }
        public string CreatedBy { get; set; }
    }
    public class AddDevice
    {
        public string Name { get; set; }
        public int PatientId { get; set; }
        public string DeviceType { get; set; }
        public string DeviceModel { get; set; }
        public string DeviceVendor { get; set; }
        public string DeviceManufacturer { get; set; }
        public string DeviceNumber { get; set; }
        public string DeviceSerialNo { get; set; }
        public string DeviceIMEINo { get; set; }
        public string DeviceStatus { get; set; }
        public string DeviceCommunicationType { get; set; }
        public string DeviceActivatedDateTime { get; set; }
        public string City { get; set; }
        public string PurchaseDate { get; set; }
        public string ManufactureDate { get; set; }
        public int WarrantyPeriod { get; set; }
        public int Lifetime { get; set; }
        public string CreatedBy { get; set; }
    }
    public class AddDevicePro
    {
        public string Device { get; set; }
        public string DeviceType { get; set; }
        public string DeviceModel { get; set; }
        public string DeviceVendor { get; set; }
        public string DeviceManufacturer { get; set; }
        public string DeviceSerialNo { get; set; }
        public string DeviceIMEINo { get; set; }
        public string PurchaseDate { get; set; }
        public string CreatedBy { get; set; }
    }
    public class MobileHomeData
    {
        public ProileSummary MobProfileSummary { get; set; }
        public Schedule MobSchedule { get; set; }
        public VitalSummaryMeasures MobVitalSummaryMeasures { get; set; }

    }
    public class ClientConfig
    {
        public string CountryCode { get; set; }
    }


    public class PatientBilldataList
    {
        public TotalCounts TotalCounts { get; set; }
        public List<PatientBilldataRecords> PatientBilldataRecords { get; set; }



    }
    public class PatientBilldataRecords
    {
        public string PatientNumber { get; set; }

        public int patientId { get; set; }

        public int PatientProgramId { get; set; }
        public string PatientName { get; set; }
        public string Program { get; set; }
        public string AssignedMemeber { get; set; }

        public int DaysCompleted { get; set; }

        public string ClinicName { get; set; }

        public string Code { get; set; }


        public int TortalVitalCount { get; set; }

        public int interaction { get; set; }

        public string NextBillingDate { get; set; }




    }

    public class TotalCounts
    {
        public int Total { get; set; }

        public int TotalCount { get; set; }

        public int? C1Total { get; set; }

        public int? C2Total { get; set; }

        public int? TodayCount { get; set; }

        public int? Next7DaysCount { get; set; }
    }
    public class bodytracedata
    {
        public string imei { get; set; }
        public long ts { get; set; }
        public int batteryVoltage { get; set; }
        public int signalStrength { get; set; }
        public int rssi { get; set; }
        public long deviceId { get; set; }
        public object values { get; set; }
    }

    public class bodytraceweight
    {
        public bool unit { get; set; }
        public int tare { get; set; }
        public int weight { get; set; }
    }
    public class bodytracepressure
    {
        public int systolic { get; set; }
        public int diastolic { get; set; }
        public int pulse { get; set; }
        public bool unit { get; set; }
        public bool irregular { get; set; }
    }

    public class StatusMessage
    {
        public string Status { get; set; }


    }
    public class AddPatientVendorConn
    {
        public string DeviceId { get; set; }
        public string PatientVendorUserId { get; set; }
        public string VendorName { get; set; }
        public string CreatedBy { get; set; }

    }

    public class NoteInfo_V1
    {
        public int Id { get; set; }
        public int PatientId { get; set; }
        public int PatientProgramId { get; set; }
        public int NoteTypeId { get; set; }
        public string NoteType { get; set; }
        public bool IsEstablishedCall { get; set; }
        public bool IsCareGiver { get; set; }
        public bool IsCallNote { get; set; }
        public int Duration { get; set; }
        public string Notes { get; set; }
        public int CompletedByUserId { get; set; }
        public string CreatedBy { get; set; }
        public string calltype { get; set; }
        public List<MainQuestion> MainQuestions { get; set; }

    }
    //public class NotesProgramMaster
    //{
    //    public string ProgrameName { get; set; }
    //    public List<NotesTypeMasterData> NotesTypeMasterDatas { get; set; }
    //}

    public class NotesTypeMasterData
    {

        public string Type { get; set; }

        public List<GetMainQuestion> MainQuestions { get; set; }
    }
    public class GetPatientNotesQA
    {
        public int PatientId { get; set; }
        public int PatientProgramId { get; set; }
        public int NoteTypeId { get; set; }
        public bool IsEstablishedCall { get; set; }
        public bool IsCareGiver { get; set; }
        public bool IsCallNote { get; set; }
        public int Duration { get; set; }
        public string Notes { get; set; }
        public int CompletedByUserId { get; set; }

        //public string CallType { get; set; }
        public List<GetMainQuestion> MainQuestions { get; set; }


    }
    public class GetMainQuestion
    {
        public int QuestionId { get; set; }
        public string Question { get; set; }
        public bool IsMandatory { get; set; }
        public bool IsMultipleChoice { get; set; }
        public List<AnswerType> AnswerTypes { get; set; }
        public List<GetSubQuestion> SubQuestions { get; set; }
        public string Notes { get; set; }
    }

    public class MainQuestion
    {
        public int QuestionId { get; set; }
        public string Question { get; set; }
        public string Notes { get; set; }
        public List<int> AnswersIds { get; set; }
        public List<AnswerType> AnswerTypes { get; set; }

    }
    public class AnswerType
    {
        public AnswerType(int _id, string _ans, bool _checked)
        {
            AnswerId = _id;
            Answer = _ans;
            Checked = _checked;
        }
        public int AnswerId { get; set; }
        public string Answer { get; set; }
        public bool Checked { get; set; }
    }
    public class SubQuestion
    {
        public int QuestionId { get; set; }
        public string Question { get; set; }
        public List<AnswerType> AnswerTypes { get; set; }
        public List<int> AnswersIds { get; set; }
    }
    public class GetSubQuestion
    {
        public int QuestionId { get; set; }
        public string Question { get; set; }
        public bool IsMandatory { get; set; }
        public bool IsMultipleChoice { get; set; }
        public List<AnswerType> AnswerTypes { get; set; }
        public string Notes { get; set; }

    }

    public class LastBilleddata
    {
        public string CPTCode { get; set; }
        public string Last_Billing_Cycle { get; set; }
        public string reading { get; set; }
        public string status { get; set; }
    }
    public class BillingDatesUpdates
    {
        public int PatientId { get; set; }
        public int PatientProgramId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime BilledDate { get; set; }
    }

    public class PatientToDoList
    {
        public int ScheduleId { get; set; }
        public int CurrentScheduleId { get; set; }
        public DateTime Date { get; set; }
        public string Interval { get; set; }
        public string ScheduleType { get; set; }
        public string Decription { get; set; }
    }
    public class PatientToDoListNew
    {
        //public int ScheduleId { get; set; }
        //public int CurrentScheduleId { get; set; }
        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }
        public string ActivityName { get; set; }
        public string Description { get; set; }
        public bool Morning { get; set; }
        public bool AfterNoon { get; set; }
        public bool Evening { get; set; }
        public bool Night { get; set; }
        public string Schedule { get; set; }

    }
    public class PatientToDoListResponse
    {
        public DateTime Date { get; set; }
        public string ActivityName { get; set; }
        public string Description { get; set; }
        public bool Morning { get; set; }
        public bool AfterNoon { get; set; }
        public bool Evening { get; set; }
        public bool Night { get; set; }
    }
    public class PatientCurrentReading
    {
        public int TotalReadings { get; set; }
        public int DaysCompleted { get; set; }

        public string CPTCode { get; set; }

        public string PatientVital { get; set; }
    }
    public class PatientData
    {
        public int PatientId { get; set; }
        public int PatientProgramId { get; set; }
        public string UserName { get; set; }
    }
    public class BillingInfoCounts
    {

        public int Total { get; set; }
        public int TargetMet { get; set; }

        public int TargetNotMet { get; set; }
        public int ReadyToBill { get; set; }

        public string CycleDate { get; set; }


    }
    public class PatientDailyBillingData
    {
        public int PatientId { get; set; }
        public int PatientProgramId { get; set; }
        public int BillingCodeId { get; set; }
        public string Status { get; set; }
        public int TotalVitalCount { get; set; }
        public int TotalDuration { get; set; }
        public int DaysCompleted { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? LastBilledDate { get; set; }
        public DateTime CreatedOn { get; set; }
    }
    public class Dates
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public int Totalreading { get; set; }
    }
    public class ReturnMsg
    {
        public string Msg { get; set; }
        public int Val { get; set; }
    }
    public class UpdateDeviceVendor
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public int CityId { get; set; }
        public int StateId { get; set; }
        public int CountryId { get; set; }
        public string ZipCode { get; set; }
        public string PrimaryPhoneNumber { get; set; }
        public string AlternatePhoneNumber { get; set; }
        public string CreatedBy { get; set; }
    }
    public class AddDeviceVendor
    {
        public string Name { get; set; }
        public string Code { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public int CityId { get; set; }
        public int StateId { get; set; }
        public int CountryId { get; set; }
        public string ZipCode { get; set; }
        public string PrimaryPhoneNumber { get; set; }
        public string AlternatePhoneNumber { get; set; }
        public string CreatedBy { get; set; }
    }
    public class DeviceInfo
    {
        public int Id { get; set; }
        public string DeviceNumber { get; set; }
        public string Device { get; set; }
        public string DeviceType { get; set; }
        public string PatientName { get; set; }
        public string DeviceActivatedDateTime { get; set; }
        public string DeviceStatus { get; set; }
    }
    public class UpdateDevice
    {
        public int Id { get; set; }
        public string Device { get; set; }
        public string DeviceType { get; set; }
        public string DeviceModel { get; set; }
        public string DeviceVendor { get; set; }
        public string DeviceManufacturer { get; set; }
        public string DeviceSerialNo { get; set; }
        public string DeviceIMEINo { get; set; }
        public string PurchaseDate { get; set; }
        public string CreatedBy { get; set; }
    }
    public class BillingType
    {
        public string Provider { get; set; }

    }
    public class DashboardAlertAndTask
    {
        public int Id { get; set; }
        public int PatientId { get; set; }
        public int PatientProgramId { get; set; }
        public string PatientName { get; set; }
        public string Priority { get; set; }
        public string VitalAlert { get; set; }
        public string Status { get; set; }
        public DateTime DueDate { get; set; }

    }
    public class DashboardTeamOverView
    {
        public string TeamName { get; set; }
        public int CareTeamId { get; set; }
        public int Alerts { get; set; }
        public int SLABreached { get; set; }
        public int DueToday { get; set; }
    }
    public class NewPatientCredential
    {
        public int PatientId { get; set; }
        public string password { get; set; }
    }
    public class NewUserCredential
    {
        public int UserId { get; set; }
        public string password { get; set; }
    }
    public class ResetUserPW
    {
        public int UserId { get; set; }
        public string CreatedBy { get; set; }
    }
    public class CallLogReport
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int? Userid { get; set; }

    }
    public class DeactivateUser
    {
        public int flag { get; set; }
        public string status { get; set; }

        public string username { get; set; }
    }

    public class nonEstablishedCallReport
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public int RoleId { get; set; }


    }
    public class PatientBillReportDetails
    {
        public string DOS { get; set; }
        public string PatientName { get; set; }

        public string PatientNumber { get; set; }
        public string clinic { get; set; }

        public string clinicname { get; set; }

        public string DOB { get; set; }
        public string CPT99454 { get; set; }
        public string CPT99457 { get; set; }
        public string CPT99458_First { get; set; }

        public string CPT99458_Second { get; set; }

        public DateTime EnrolledDate { get; set; }

    }
    public class FbToken
    {
        public string fbtkn { get; set; }
    }
    public class firebaseAdmin
    {
        public string type { get; set; }
        public string project_id { get; set; }
        public string private_key_id { get; set; }
        public string private_key { get; set; }
        public string client_email { get; set; }
        public string client_id { get; set; }
        public string auth_uri { get; set; }
        public string token_uri { get; set; }
        public string auth_provider_x509_cert_url { get; set; }
        public string client_x509_cert_url { get; set; }

    }

    public class firebasenotificationmessage
    {
        public string title { get; set; }
        public string body { get; set; }
    }

    public class VideoCallDetails
    {
        public string room { get; set; }
        public string token { get; set; }


    }
    public class ChatDetails
    {
        public string token { get; set; }
        public bool istoken { get; set; }

        public bool makeisactivezero { get; set; }
        public string Application { get; set; }
    }
    public class ChatResourceDetails
    {
        public string CreatedBy { get; set; }
        public string ToUser { get; set; }
        public string ConversationSid { get; set; }

        public string ChatToken { get; set; }

    }
    public class SaveSmsInfo
    {

        public string PatientUserName { get; set; }
        public string fromNo { get; set; }
        public string toNo { get; set; }
        public string Body { get; set; }
        public DateTime SentDate { get; set; }
        public string Direction { get; set; }
        public string Status { get; set; }
    }
    public class GetSmsInfo
    {
        public string Body { get; set; }
        public DateTime SentDate { get; set; }
        public string Direction { get; set; }
        public string Status { get; set; }
        public string Sender { get; set; }
    }
    public class PatientCareTeamMembers
    {
        public string MemberUserName { get; set; }
        public string Role { get; set; }
        public string MemberName { get; set; }
    }


    public class chathook {
        public string author { get; set; }
        public string Body { get; set; }
        public string Attributes { get; set; }
        public string AccountSid { get; set; }
        public string ClientIdentity { get; set; }
        public string EventType { get; set; }
        public string Source { get; set; }
        public string ConversationSid { get; set; }
        public string ParticipantSid { get; set; }
        //public string DateCreated { get; set; }


    }

    public class smshook
    {
        public string MessageSid { get; set; }
        public string SmsSid { get; set; }
        public string AccountSid { get; set; }
        public string MessagingServiceSid { get; set; }

        public string From { get; set; }
        public string To { get; set; }

        public string Body { get; set; }

        public string SmsStatus { get; set; }




    }
    public class commUserNamesforVideoCall
    {
        public string CommUserNameCareTeam { get; set; }
        public string CommUserNamePatient { get; set; }
    }
    public class ConverationHistory
    {
        public string ContactName { get; set; }
        public string LastMessage { get; set; }
        public string DateTime { get; set; }
        public string ConversationSid { get; set; }
        public List<MessageHistoryItem> Messages { get; set; }
    }
    public class ConversationHeartBeat
    {
        public string ConversationSid { get; set; }
        public string UserName { get; set; }
        public string LastActiveAt { get; set; }
    }

    public class ConversationsNotification
    {
        public string ConversationSid { get; set; }
        public string ToUser { get; set; }
        public string FromUser { get; set; }
        public string Message { get; set; }
    }
    public class PatientDetials
    {
        public int PatientId { get; set; }
        public int ProgramId { get; set; }
    }
    public class MessageHistoryItem
    {
        public string Message { get; set; }
        public string DateTime { get; set; }
        public string Author { get; set; }
    }
	public class deviceAvailable
    {
        public string DeviceNumber { get; set; }
    }
	 public class PatientProgramDetailsInsertActivePatients
    {
        public int PatientId { get; set; }
        public int PatientProgramId { get; set; }
        public int ProgramId { get; set; }
        public List<int> VitalIds { get; set; }
        public string CreatedBy { get; set; }
        public GoalDetails[] GoalDetails { get; set; }
        public ProgramDiagnostics[] ProgramDiagnosis { get; set; }
    }
	
}
