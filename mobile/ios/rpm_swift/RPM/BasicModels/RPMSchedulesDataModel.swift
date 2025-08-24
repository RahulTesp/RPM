//
//  RPMSchedulesDataModel.swift
//  RPM
//
//  Created by Tesplabs on 27/07/1944 Saka.
//



// This file was generated from JSON Schema using quicktype, do not modify it directly.
// To parse the JSON, add this file to your project and do:
//
//   let rPMSchedules = try? newJSONDecoder().decode(RPMSchedules.self, from: jsonData)

import Foundation

// MARK: - RPMSchedule
struct RPMSchedule: Codable , Identifiable , Equatable{
    
    static func == (lhs: RPMSchedule, rhs: RPMSchedule) -> Bool {
        return true
    }
    func hash(into hasher: inout Hasher) {
        
    }
    let id = UUID()
    
    
    let scheduleDate: String
    let schedueInfos: [SchedueInfo]
    
    enum CodingKeys: String, CodingKey {
        case scheduleDate = "ScheduleDate"
        case schedueInfos = "SchedueInfos"
    }
}

// MARK: - SchedueInfo
struct SchedueInfo: Codable , Identifiable {
    
    static func == (lhs: SchedueInfo, rhs: SchedueInfo) -> Bool {
        return true
    }
    func hash(into hasher: inout Hasher) {
        
    }
    let id = UUID()
    let ids, currentScheduleID, patientID, patientProgramID: Int
    let scheduleTime, scheduleType, schedueInfoDescription: String
    let contactName: String
    let assignedBy: Int
    let assignedByName: String
    let isCompleted: Bool
    
    enum CodingKeys: String, CodingKey {
        case ids = "Id"
        case currentScheduleID = "CurrentScheduleId"
        case patientID = "PatientId"
        case patientProgramID = "PatientProgramId"
        case scheduleTime = "ScheduleTime"
        case scheduleType = "ScheduleType"
        case schedueInfoDescription = "Description"
        case contactName = "ContactName"
        case assignedBy = "AssignedBy"
        case assignedByName = "AssignedByName"
        case isCompleted = "IsCompleted"
    }
}

enum AssignedByName: String, Codable {
    case azizMalli = "AzizMalli"
    case shijinKT = "ShijinKT"
}

enum ContactName: String, Codable {
    case shakeelA = "ShakeelA"
}

typealias RPMSchedules = [RPMSchedule]
