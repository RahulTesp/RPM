//
//  RPMPatientVitalSummary.swift
//  RPM
//
//  Created by Tesplabs on 28/06/1944 Saka.
//


// This file was generated from JSON Schema using quicktype, do not modify it directly.
// To parse the JSON, add this file to your project and do:
//
//   let patientVitalSummary = try? newJSONDecoder().decode(PatientVitalSummary.self, from: jsonData)

import Foundation

// MARK: - PatientVitalSummaryElement
struct PatientVitalSummaryElement: Codable {
    let vitalName: String
    let measurements: [Measurement]
    
    enum CodingKeys: String, CodingKey {
        case vitalName = "VitalName"
        case measurements = "Measurements"
    }
}

// MARK: - Measurement
struct Measurement: Codable {
    let measurement: String
    let time: [String]
    let value: [Int]
    
    enum CodingKeys: String, CodingKey {
        case measurement = "Measurement"
        case time = "Time"
        case value = "Value"
    }
}

typealias PatientVitalSummary = [PatientVitalSummaryElement]

