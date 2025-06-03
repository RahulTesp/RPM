//
//  RPMProgramInfoDataModel.swift
//  RPM
//
//  Created by Tesplabs on 09/08/1944 Saka.
//


// This file was generated from JSON Schema using quicktype, do not modify it directly.
// To parse the JSON, add this file to your project and do:
//
//   let programInfo = try? newJSONDecoder().decode(ProgramInfo.self, from: jsonData)

import Foundation

// MARK: - ProgramInfo
struct ProgramInfo: Codable {
    let patientDetails: PatientDetails
    let patientProgramdetails: PatientProgramdetails
    let patientProgramGoals: PatientProgramGoals
    let patientPrescribtionDetails: PatientPrescribtionDetails
    let patientEnrolledDetails: PatientEnrolledDetails
    let activePatientDetails, readyForDischargePatientDetails, onHoldPatientDetais, dischargedPatientDetails: ActivePatientDetails
    let patientDevicesDetails: PatientDevicesDetails
    let patientVitalDetails: PatientVitalDetails
    let patientInsurenceDetails: PatientInsurenceDetails
    let patientDocumentDetails: PatientDocumentDetails
    
    enum CodingKeys: String, CodingKey {
        case patientDetails = "PatientDetails"
        case patientProgramdetails = "PatientProgramdetails"
        case patientProgramGoals = "PatientProgramGoals"
        case patientPrescribtionDetails = "PatientPrescribtionDetails"
        case patientEnrolledDetails = "PatientEnrolledDetails"
        case activePatientDetails = "ActivePatientDetails"
        case readyForDischargePatientDetails = "ReadyForDischargePatientDetails"
        case onHoldPatientDetais = "OnHoldPatientDetais"
        case dischargedPatientDetails = "DischargedPatientDetails"
        case patientDevicesDetails = "PatientDevicesDetails"
        case patientVitalDetails = "PatientVitalDetails"
        case patientInsurenceDetails = "PatientInsurenceDetails"
        case patientDocumentDetails = "PatientDocumentDetails"
    }
}

// MARK: - ActivePatientDetails
struct ActivePatientDetails: Codable {
    let status: String?
    let assigneeID: Int
    let assigneeName: String?
    let managerID: Int
    let managerName: String?
    let assignedDate: String
    
    enum CodingKeys: String, CodingKey {
        case status = "Status"
        case assigneeID = "AssigneeId"
        case assigneeName = "AssigneeName"
        case managerID = "ManagerId"
        case managerName = "ManagerName"
        case assignedDate = "AssignedDate"
    }
}

// MARK: - PatientDetails
struct PatientDetails: Codable {
    let userName: String
    let userID, organizationID: Int
    let mobileNo, alternateMobNo, email, status: String
    let firstName, middleName, lastName, dob: String
    let gender: String
    let height: Double
    let weight: Int
    let address1, address2: String
    let cityID, stateID: Int
    let state: String
    let countryID: Int
    let picture: String
    let zipCode , cityName , timeZone : String
    let timeZoneID, utcDifference: Int
    let contact1Name, contact1RelationName, contact1Phone, contact2Name: String
    let contact2RelationName, contact2Phone, callTime, preference1: String
    let preference2, preference3, notes, language: String
    
    enum CodingKeys: String, CodingKey {
        case userName = "UserName"
        case userID = "UserId"
        case organizationID = "OrganizationID"
        case mobileNo = "MobileNo"
        case alternateMobNo = "AlternateMobNo"
        case email = "Email"
        case status = "Status"
        case firstName = "FirstName"
        case middleName = "MiddleName"
        case lastName = "LastName"
        case dob = "DOB"
        case gender = "Gender"
        case height = "Height"
        case weight = "Weight"
        case address1 = "Address1"
        case address2 = "Address2"
        case cityID = "CityId"
        case stateID = "StateId"
        case state = "State"
        case countryID = "CountryId"
        case picture = "Picture"
        case zipCode = "ZipCode"
        case cityName = "CityName"
        case timeZoneID = "TimeZoneID"
        case timeZone = "TimeZone"
        case utcDifference = "UTCDifference"
        case contact1Name = "Contact1Name"
        case contact1RelationName = "Contact1RelationName"
        case contact1Phone = "Contact1Phone"
        case contact2Name = "Contact2Name"
        case contact2RelationName = "Contact2RelationName"
        case contact2Phone = "Contact2Phone"
        case callTime = "CallTime"
        case preference1 = "Preference1"
        case preference2 = "Preference2"
        case preference3 = "Preference3"
        case notes = "Notes"
        case language = "Language"
    }
}

// MARK: - PatientDevicesDetails
struct PatientDevicesDetails: Codable {
    
    
    
    let patientDeviceInfos: [PatientDeviceInfo]
    
    enum CodingKeys: String, CodingKey {
        case patientDeviceInfos = "PatientDeviceInfos"
    }
}

// MARK: - PatientDeviceInfo
struct PatientDeviceInfo: Codable, Identifiable , Hashable{
    static func == (lhs: PatientDeviceInfo, rhs: PatientDeviceInfo) -> Bool {
        return true
    }
    func hash(into hasher: inout Hasher) {
        
    }
    let id = UUID()
    let vitalName: String
    let vitalID: Int
    let deviceName: String
    let deviceVendorID: Int
    let deviceNumber, deviceStatus, deviceCommunicationType, deviceModel: String
    let deviceVendorUserID, deviceVendorUserName: String
    
    enum CodingKeys: String, CodingKey {
        case vitalName = "VitalName"
        case vitalID = "VitalId"
        case deviceName = "DeviceName"
        case deviceVendorID = "DeviceVendorId"
        case deviceNumber = "DeviceNumber"
        case deviceStatus = "DeviceStatus"
        case deviceCommunicationType = "DeviceCommunicationType"
        case deviceModel = "DeviceModel"
        case deviceVendorUserID = "DeviceVendorUserId"
        case deviceVendorUserName = "DeviceVendorUserName"
    }
}

// MARK: - PatientDocumentDetails
struct PatientDocumentDetails: Codable {
    let patientDocumentinfos: [PatientDocumentinfo]
    
    enum CodingKeys: String, CodingKey {
        case patientDocumentinfos = "PatientDocumentinfos"
    }
}

// MARK: - PatientDocumentinfo
struct PatientDocumentinfo: Codable {
    let id: Int
    let documentType, documentName, createdOn: String
    let documentUNC: String
    
    enum CodingKeys: String, CodingKey {
        case id = "Id"
        case documentType = "DocumentType"
        case documentName = "DocumentName"
        case createdOn = "CreatedOn"
        case documentUNC = "DocumentUNC"
    }
}

// MARK: - PatientEnrolledDetails
struct PatientEnrolledDetails: Codable {
    let status, assignedDate: String?
    let patientEnrolledInfos: [PatientEnrolledInfo]
    
    enum CodingKeys: String, CodingKey {
        case status = "Status"
        case assignedDate = "AssignedDate"
        case patientEnrolledInfos
    }
}

// MARK: - PatientEnrolledInfo
struct PatientEnrolledInfo: Codable {
    let enrollmentPersonal: String
    
    enum CodingKeys: String, CodingKey {
        case enrollmentPersonal = "EnrollmentPersonal"
    }
}

// MARK: - PatientInsurenceDetails
struct PatientInsurenceDetails: Codable {
 
    let patientInsurenceInfos: [PatientInsurenceInfo]
    
    enum CodingKeys: String, CodingKey {
        case patientInsurenceInfos = "PatientInsurenceInfos"
    }
}

// MARK: - PatientInsurenceInfo
struct PatientInsurenceInfo: Codable , Identifiable, Equatable{
    
    static func == (lhs: PatientInsurenceInfo, rhs: PatientInsurenceInfo) -> Bool {
        return true
    }
    func hash(into hasher: inout Hasher) {
        
    }
    let id = UUID()
    
    
    
    let ids, insuranceVendorID: Int
    let insuranceVendorName: String
    let isPrimary: Bool
    
    enum CodingKeys: String, CodingKey {
        case ids = "Id"
        case insuranceVendorID = "InsuranceVendorId"
        case insuranceVendorName = "InsuranceVendorName"
        case isPrimary = "IsPrimary"
    }
}

// MARK: - PatientPrescribtionDetails
struct PatientPrescribtionDetails: Codable {
    let status, prescribedDate: String
    let physicianID: Int
    let physician, consultationDate, clinic, branch, clinicCode: String
    let patientDiagnosisInfos: [PatientDiagnosisInfo]
    
    enum CodingKeys: String, CodingKey {
        case status = "Status"
        case prescribedDate = "PrescribedDate"
        case physicianID = "PhysicianId"
        case physician = "Physician"
        case consultationDate = "ConsultationDate"
        case clinic = "Clinic"
        case branch = "Branch"
        case clinicCode = "ClinicCode"
    
        case patientDiagnosisInfos = "PatientDiagnosisInfos"
    }
}

// MARK: - PatientDiagnosisInfo
struct PatientDiagnosisInfo: Codable , Identifiable{
    
    static func == (lhs: PatientDiagnosisInfo, rhs: PatientDiagnosisInfo) -> Bool {
        return true
    }
    func hash(into hasher: inout Hasher) {
        
    }
    let id = UUID()
    
    let ids: Int
    let diagnosisName, diagnosisCode: String
    
    enum CodingKeys: String, CodingKey {
        case ids = "Id"
        case diagnosisName = "DiagnosisName"
        case diagnosisCode = "DiagnosisCode"
    }
}

// MARK: - PatientProgramGoals
struct PatientProgramGoals: Codable {
    let goalDetails: [GoalDetail]
}

// MARK: - GoalDetail
struct GoalDetail: Codable , Identifiable {
    
    static func == (lhs: GoalDetail, rhs: GoalDetail) -> Bool {
        return true
    }
    func hash(into hasher: inout Hasher) {
        
    }
    let id = UUID()
    
    let ids: Int
    let goal, goalDetailDescription: String
    
    enum CodingKeys: String, CodingKey {
        case ids = "Id"
        case goal = "Goal"
        case goalDetailDescription = "Description"
        
    }
}

// MARK: - PatientProgramdetails
struct PatientProgramdetails: Codable {
    let patientProgramID, programID: Int
    let programName: String
    let careTeamUserID: Int
    let assignedMember: String
    let managerID: Int
    let manager, startDate, endDate: String
    let duration: Int
    let programStatus, status: String
    let targetReadings: Int
    let patientVitalInfos: [PatientProgramdetailsPatientVitalInfo]
    
    enum CodingKeys: String, CodingKey {
        case patientProgramID = "PatientProgramId"
        case programID = "ProgramId"
        case programName = "ProgramName"
        case careTeamUserID = "CareTeamUserId"
        case assignedMember = "AssignedMember"
        case managerID = "ManagerId"
        case manager = "Manager"
        case startDate = "StartDate"
        case endDate = "EndDate"
        case duration = "Duration"
        case programStatus = "ProgramStatus"
        case status = "Status"
        case targetReadings = "TargetReadings"
        case patientVitalInfos = "PatientVitalInfos"
    }
}

// MARK: - PatientProgramdetailsPatientVitalInfo
struct PatientProgramdetailsPatientVitalInfo: Codable , Identifiable{
    
    static func == (lhs: PatientProgramdetailsPatientVitalInfo, rhs: PatientProgramdetailsPatientVitalInfo) -> Bool {
        return true
    }
    func hash(into hasher: inout Hasher) {
        
    }
    let id = UUID()
    
    let vital: String
    let vitalID: Int
    let selected: Bool
    
    enum CodingKeys: String, CodingKey {
        case vital = "Vital"
        case vitalID = "VitalId"
        case selected = "Selected"
    }
}

// MARK: - PatientVitalDetails
struct PatientVitalDetails: Codable {
    let patientVitalInfos: [PatientVitalDetailsPatientVitalInfo]
    
    enum CodingKeys: String, CodingKey {
        case patientVitalInfos = "PatientVitalInfos"
    }
}

// MARK: - PatientVitalDetailsPatientVitalInfo
struct PatientVitalDetailsPatientVitalInfo: Codable , Identifiable {
    
    
    static func == (lhs: PatientVitalDetailsPatientVitalInfo, rhs: PatientVitalDetailsPatientVitalInfo) -> Bool {
        return true
    }
    func hash(into hasher: inout Hasher) {
        
    }
    let id = UUID()
    
    let vitalName: String
    let vitalID, scheduleID: Int
    let scheduleName, schedule: String
    let vitalScheduleID: Int
    let vitalScheduleName: String
    let morning, afternoon, evening, night: Bool
    let vitalMeasureInfos: [VitalMeasureInfo]
    
    enum CodingKeys: String, CodingKey {
        case vitalName = "VitalName"
        case vitalID = "VitalId"
        case scheduleID = "ScheduleId"
        case scheduleName = "ScheduleName"
        case schedule = "Schedule"
        case vitalScheduleID = "VitalScheduleId"
        case vitalScheduleName = "VitalScheduleName"
        case morning = "Morning"
        case afternoon = "Afternoon"
        case evening = "Evening"
        case night = "Night"
        case vitalMeasureInfos = "VitalMeasureInfos"
    }
}

// MARK: - VitalMeasureInfo
struct VitalMeasureInfo: Codable , Identifiable{
    
    static func == (lhs: VitalMeasureInfo, rhs: VitalMeasureInfo) -> Bool {
        return true
    }
    func hash(into hasher: inout Hasher) {
        
    }
    let id = UUID()
    
    
    let ids, deviceVitalMeasuresID: Int
    let measureName, unitName: String
    let measureOrder, criticallMinimum, cautiousMinimum, normalMinimum: Int
    let normalMaximum, cautiousMaximum, criticalMaximum: Int
    
    enum CodingKeys: String, CodingKey {
        case ids = "Id"
        case deviceVitalMeasuresID = "DeviceVitalMeasuresId"
        case measureName = "MeasureName"
        case unitName = "UnitName"
        case measureOrder = "MeasureOrder"
        case criticallMinimum = "CriticallMinimum"
        case cautiousMinimum = "CautiousMinimum"
        case normalMinimum = "NormalMinimum"
        case normalMaximum = "NormalMaximum"
        case cautiousMaximum = "CautiousMaximum"
        case criticalMaximum = "CriticalMaximum"
    }
}
