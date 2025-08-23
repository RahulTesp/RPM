//
//  RPMMedicationsDataModel.swift
//  RPM
//
//  Created by Tesplabs on 26/07/1944 Saka.
//


// This file was generated from JSON Schema using quicktype, do not modify it directly.
// To parse the JSON, add this file to your project and do:
//
//   let medications = try? newJSONDecoder().decode(Medications.self, from: jsonData)

import Foundation

// MARK: - Medication
struct Medication: Codable , Identifiable , Equatable{
    
    static func == (lhs: Medication, rhs: Medication) -> Bool {
        return true
    }
    func hash(into hasher: inout Hasher) {
        
    }
    let id = UUID()
    
    let ids: Int
    let medicinename, medicineSchedule ,beforeoraftermeal : String
    let morning, afterNoon, evening, night: Bool
    let startDate, endDate, medicationDescription: String
    
    enum CodingKeys: String, CodingKey {
        case ids = "Id"
        case medicinename = "Medicinename"
        case medicineSchedule = "MedicineSchedule"
        case beforeoraftermeal = "BeforeOrAfterMeal"
        case morning = "Morning"
        case afterNoon = "AfterNoon"
        case evening = "Evening"
        case night = "Night"
        case startDate = "StartDate"
        case endDate = "EndDate"
        case medicationDescription = "Description"
    }
}

typealias Medications = [Medication]
