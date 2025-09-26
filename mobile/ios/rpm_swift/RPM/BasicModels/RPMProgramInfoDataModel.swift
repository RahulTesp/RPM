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


// MARK: - Root
struct ProgramInfo: Codable {
    let patientDetails: PatientDetails
    let patientProgramdetails: PatientProgramdetails
    let patientProgramGoals: PatientProgramGoals
    let patientPrescribtionDetails: PatientPrescribtionDetails
    let patientEnrolledDetails: PatientEnrolledDetails
    let activePatientDetails: ActivePatientDetails
    let readyForDischargePatientDetails: ActivePatientDetails
    let onHoldPatientDetais: ActivePatientDetails
    let inActivePatientDetais: ActivePatientDetails
    let dischargedPatientDetails: ActivePatientDetails
    let patientDevicesDetails: PatientDevicesDetails
    let patientVitalDetails: PatientVitalDetails
    let patientInsurenceDetails: PatientInsurenceDetails
    let patientDocumentDetails: PatientDocumentDetails
    let profileSummary: ProfileSummary
    
    enum CodingKeys: String, CodingKey {
        case patientDetails = "PatientDetails"
        case patientProgramdetails = "PatientProgramdetails"
        case patientProgramGoals = "PatientProgramGoals"
        case patientPrescribtionDetails = "PatientPrescribtionDetails"
        case patientEnrolledDetails = "PatientEnrolledDetails"
        case activePatientDetails = "ActivePatientDetails"
        case readyForDischargePatientDetails = "ReadyForDischargePatientDetails"
        case onHoldPatientDetais = "OnHoldPatientDetais"
        case inActivePatientDetais = "InActivePatientDetais"
        case dischargedPatientDetails = "DischargedPatientDetails"
        case patientDevicesDetails = "PatientDevicesDetails"
        case patientVitalDetails = "PatientVitalDetails"
        case patientInsurenceDetails = "PatientInsurenceDetails"
        case patientDocumentDetails = "PatientDocumentDetails"
        case profileSummary = "ProfileSummary"
    }
        
}
 
// MARK: - PatientDetails
struct PatientDetails: Codable {
    let userName: String?
    let userId: Int?
    let organizationID: Int?
    let mobileNo: String?
    let alternateMobNo: String?
    let email: String?
    let status: String?
    let firstName: String?
    let middleName: String?
    let lastName: String?
    let dob: String?
    let gender: String?
    let height: Double?
    let weight: Double?
    let address1: String?
    let address2: String?
    let cityName: String?
    let cityId: Int?
    let stateId: Int?
    let state: String?
    let countryId: Int?
    let picture: String?
    let zipCode: String?
    let timeZone: String?
    let timeZoneID: Int?
    let utcDifference: Int?
    let contact1Name: String?
    let contact1RelationName: String?
    let contact1Phone: String?
    let contact2Name: String?
    let contact2RelationName: String?
    let contact2Phone: String?
    let callTime: String?
    let preference1: String?
    let preference2: String?
    let preference3: String?
    let notes: String?
    let language: String?
 
    enum CodingKeys: String, CodingKey {
        case userName = "UserName"
        case userId = "UserId"
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
        case cityName = "CityName"
        case cityId = "CityId"
        case stateId = "StateId"
        case state = "State"
        case countryId = "CountryId"
        case picture = "Picture"
        case zipCode = "ZipCode"
        case timeZone = "TimeZone"
        case timeZoneID = "TimeZoneID"
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
 
// MARK: - PatientProgramdetails
struct PatientProgramdetails: Codable {
    let patientProgramId: Int?
    let programId: Int?
    let programName: String?
    let careTeamUserId: Int?
    let assignedMember: String?
    let managerId: Int?
    let manager: String?
    let startDate: String?
    let endDate: String?
    let duration: Int?
    let programStatus: String?
    let status: String?
    let targetReadings: Int?
    let patientVitalInfos: [PatientProgramVitalInfo]
 
    enum CodingKeys: String, CodingKey {
        case patientProgramId = "PatientProgramId"
        case programId = "ProgramId"
        case programName = "ProgramName"
        case careTeamUserId = "CareTeamUserId"
        case assignedMember = "AssignedMember"
        case managerId = "ManagerId"
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
 
struct PatientProgramVitalInfo: Codable, Identifiable {
    let vitalId: Int
    let vital: String
    let selected: Bool
 
    // Use vitalId as the unique identifier for SwiftUI
    var id: Int { vitalId }
 
    enum CodingKeys: String, CodingKey {
        case vitalId = "VitalId"
        case vital = "Vital"
        case selected = "Selected"
    }
}


// MARK: - PatientProgramGoals

struct PatientProgramGoals: Codable {

    let goalDetails: [GoalDetail]

}
 
struct GoalDetail: Codable, Identifiable {

    let id: Int              // API's Id

    let goal: String

    let description: String
 
    enum CodingKeys: String, CodingKey {

        case id = "Id"

        case goal = "Goal"

        case description = "Description"

    }

}

 
 
// MARK: - PatientPrescribtionDetails

struct PatientPrescribtionDetails: Codable {

    let status: String?

    let prescribedDate: String?

    let physicianId: Int?

    let physician: String?

    let consultationDate: String?

    let clinic: String?

    let clinicCode: String?

    let branch: String?

    let patientDiagnosisInfos: [PatientDiagnosisInfo]
 
    enum CodingKeys: String, CodingKey {

        case status = "Status"

        case prescribedDate = "PrescribedDate"

        case physicianId = "PhysicianId"

        case physician = "Physician"

        case consultationDate = "ConsultationDate"

        case clinic = "Clinic"

        case clinicCode = "ClinicCode"

        case branch = "Branch"

        case patientDiagnosisInfos = "PatientDiagnosisInfos"

    }

}
 
struct PatientDiagnosisInfo: Codable,Identifiable {

    let id: Int

    let diagnosisName: String

    let diagnosisCode: String
 
    enum CodingKeys: String, CodingKey {

        case id = "Id"

        case diagnosisName = "DiagnosisName"

        case diagnosisCode = "DiagnosisCode"

    }

}
 


// MARK: - PatientEnrolledDetails

struct PatientEnrolledDetails: Codable {

    let status: String?

    let assignedDate: String

    let patientEnrolledInfos: [PatientEnrolledInfo]
 
    enum CodingKeys: String, CodingKey {

        case status = "Status"

        case assignedDate = "AssignedDate"

        case patientEnrolledInfos = "patientEnrolledInfos"

    }

}
 
struct PatientEnrolledInfo: Codable ,Identifiable{
    let id = UUID()   // Auto-generate unique ID
  

    let enrollmentPersonal: String
 
    enum CodingKeys: String, CodingKey {

        case enrollmentPersonal = "EnrollmentPersonal"

    }

}
 
// MARK: - ActivePatientDetails (used for multiple states)

struct ActivePatientDetails: Codable {

    let status: String?

    let assigneeId: Int

    let assigneeName: String?

    let managerId: Int

    let managerName: String?

    let assignedDate: String
 
    enum CodingKeys: String, CodingKey {

        case status = "Status"

        case assigneeId = "AssigneeId"

        case assigneeName = "AssigneeName"

        case managerId = "ManagerId"

        case managerName = "ManagerName"

        case assignedDate = "AssignedDate"

    }

}
 

// MARK: - PatientDevicesDetails
struct PatientDevicesDetails: Codable {
    let patientDeviceInfos: [PatientDeviceInfo]
 
    enum CodingKeys: String, CodingKey {
        case patientDeviceInfos = "PatientDeviceInfos"
    }
}
 
struct PatientDeviceInfo: Codable {
    let vitalName: String
    let vitalId: Int
    let deviceName: String
    let deviceVendorId: Int
    let deviceNumber: String
    let deviceStatus: String
    let deviceCommunicationType: String
    let deviceModel: String
    let deviceVendorUserId: String
    let deviceVendorUserName: String
 
    enum CodingKeys: String, CodingKey {
        case vitalName = "VitalName"
        case vitalId = "VitalId"
        case deviceName = "DeviceName"
        case deviceVendorId = "DeviceVendorId"
        case deviceNumber = "DeviceNumber"
        case deviceStatus = "DeviceStatus"
        case deviceCommunicationType = "DeviceCommunicationType"
        case deviceModel = "DeviceModel"
        case deviceVendorUserId = "DeviceVendorUserId"
        case deviceVendorUserName = "DeviceVendorUserName"
    }
}
 
// MARK: - PatientVitalDetails
struct PatientVitalDetails: Codable {
    let patientVitalInfos: [PatientVitalInfo]
 
    enum CodingKeys: String, CodingKey {
        case patientVitalInfos = "PatientVitalInfos"
    }
}


struct PatientVitalInfo: Codable, Identifiable {
    let vitalName: String
    let vitalId: Int
    let scheduleId: Int
    let scheduleName: String
    let schedule: String
    let vitalScheduleId: Int
    let vitalScheduleName: String
    let morning: Bool
    let afternoon: Bool
    let evening: Bool
    let night: Bool
    let vitalMeasureInfos: [VitalMeasureInfo]
 
    // Use vitalId for unique ID
    var id: Int { vitalId }
 
    enum CodingKeys: String, CodingKey {
        case vitalName = "VitalName"
        case vitalId = "VitalId"
        case scheduleId = "ScheduleId"
        case scheduleName = "ScheduleName"
        case schedule = "Schedule"
        case vitalScheduleId = "VitalScheduleId"
        case vitalScheduleName = "VitalScheduleName"
        case morning = "Morning"
        case afternoon = "Afternoon"
        case evening = "Evening"
        case night = "Night"
        case vitalMeasureInfos = "VitalMeasureInfos"
    }
}

struct VitalMeasureInfo: Codable,Identifiable {
    let id: Int
    let deviceVitalMeasuresId: Int
    let measureName: String
    let unitName: String
    let measureOrder: Int
    let criticallMinimum: Double
    let cautiousMinimum: Double
    let normalMinimum: Double
    let normalMaximum: Double
    let cautiousMaximum: Double
    let criticalMaximum: Double
 
    enum CodingKeys: String, CodingKey {
        case id = "Id"
        case deviceVitalMeasuresId = "DeviceVitalMeasuresId"
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


struct PatientInsurenceDetails: Codable {
    let patientInsurenceInfos: [PatientInsurenceInfo]
 
    enum CodingKeys: String, CodingKey {
        case patientInsurenceInfos = "PatientInsurenceInfos"
    }
}
 
struct PatientInsurenceInfo: Codable, Identifiable {
    let id = UUID() // SwiftUI unique ID
 
    let insuranceVendorId: Int?
    let insuranceVendorName: String?
    let isPrimary: Bool?
 
    enum CodingKeys: String, CodingKey {
        case insuranceVendorId = "InsuranceVendorId"
        case insuranceVendorName = "InsuranceVendorName"
        case isPrimary = "IsPrimary"
    }
}

 
 
// MARK: - PatientDocumentDetails
struct PatientDocumentDetails: Codable {
//    let patientDocumentinfos: [String] // empty array in JSON, adjust if later has objects
      let patientDocumentinfos: [PatientDocumentInfo]
 
    enum CodingKeys: String, CodingKey {
        case patientDocumentinfos = "PatientDocumentinfos"
    }
}
struct PatientDocumentInfo: Codable, Identifiable {

    let id = UUID()

    // add fields based on your real JSON structure for document info

}
 


// MARK: - ProfileSummary
struct ProfileSummary: Codable {
    let name: String
    let userName: String
    let programName: String
    let programType: String
    let completedDuration: String
    let currentDuration: Int
    let totalDuration: Int
    let status: String
 
    enum CodingKeys: String, CodingKey {
        case name = "Name"
        case userName = "UserName"
        case programName = "ProgramName"
        case programType = "ProgramType"
        case completedDuration = "CompletedDuration"
        case currentDuration = "CurrentDuration"
        case totalDuration = "TotalDuration"
        case status = "Status"
    }
}
