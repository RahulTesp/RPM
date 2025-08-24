//
//  RPMVitalReadingsDataModel.swift
//  RPM
//
//  Created by Tesplabs on 24/09/1944 Saka.
//


// This file was generated from JSON Schema using quicktype, do not modify it directly.
// To parse the JSON, add this file to your project and do:
//
//   let vitalReadingsDataModel = try? newJSONDecoder().decode(VitalReadingsDataModel.self, from: jsonData)

import Foundation

// MARK: - VitalReadingsDataModel
struct VitalReadingsDataModel: Codable {

    let bloodPressure: [BloodPressure]?
     let bloodGlucose: [BloodGlucose]?
    let weight: [Weight]?
    let bloodOxygen: [BloodOxygen]?
    
    
    enum CodingKeys: String, CodingKey {
        case bloodPressure = "BloodPressure"
          case bloodGlucose = "BloodGlucose"
        case weight = "Weight"
        case bloodOxygen = "BloodOxygen"
    }
    
}
// MARK: - BloodPressure
struct BloodPressure: Codable , Hashable ,Identifiable , Equatable{
    
    static func == (lhs: BloodPressure, rhs: BloodPressure) -> Bool {
        return true
    }
    func hash(into hasher: inout Hasher) {
        
    }
    
    let id = UUID()
    
    let readingTime: String
    let systolic, diastolic, pulse: Int
    let systolicStatus, diastolicStatus: String
    let pulseStatus: String
    let status, remarks: String
    
    enum CodingKeys: String, CodingKey {
        case readingTime = "ReadingTime"
        case systolic = "Systolic"
        case diastolic = "Diastolic"
        case pulse
        case systolicStatus = "SystolicStatus"
        case diastolicStatus = "DiastolicStatus"
        case pulseStatus
        case status = "Status"
        case remarks = "Remarks"
    }
}
struct BloodGlucose: Codable ,  Hashable , Identifiable  ,Equatable{
    
    static func == (lhs: BloodGlucose, rhs: BloodGlucose) -> Bool {
        return true
    }
    func hash(into hasher: inout Hasher) {
        
    }
    
    let id = UUID()
    let readingTimeG , schedule , gluStatus : String
    let bgmdl : Int
    
    enum CodingKeys: String, CodingKey {
        case readingTimeG = "ReadingTime"
        case schedule = "Schedule"
        case bgmdl = "BGmgdl"
        case gluStatus = "Status"
    
        
    }
}
struct Weight: Codable , Hashable , Equatable, Identifiable {
    
    static func == (lhs: Weight, rhs: Weight) -> Bool {
        return true
    }
    func hash(into hasher: inout Hasher) {
        
    }
    
    let id = UUID()
    
    let readingTimes: String
    
    
    let bWlbs : Double
    
    let weightstatus: String
    enum CodingKeys: String, CodingKey {
        case readingTimes = "ReadingTime"
        
        case bWlbs = "BWlbs"
        case weightstatus = "Status"
    }
}

struct BloodOxygen: Codable ,  Hashable , Identifiable  ,Equatable{
    
    static func == (lhs: BloodOxygen, rhs: BloodOxygen) -> Bool {
        return true
    }
    func hash(into hasher: inout Hasher) {
        
    }
    
    let id = UUID()
    let readingTimeO , pulseStatus, oxygenStatus , status: String
    let pulseO , oxygen : Int
    
    enum CodingKeys: String, CodingKey {
        case readingTimeO = "ReadingTime"
        case pulseO = "Pulse"
        case oxygen = "Oxygen"
        case pulseStatus = "PulseStatus"
        case oxygenStatus = "OxygenStatus"
        case status = "Status"
        
    }
}

typealias vitalReadingsDataModel = VitalReadingsDataModel


