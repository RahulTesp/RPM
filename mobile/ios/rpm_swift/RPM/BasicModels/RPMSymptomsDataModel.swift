//
//  RPMSymptomsDataModel.swift
//  RPM
//
//  Created by Tesplabs on 26/07/1944 Saka.
//
//
// This file was generated from JSON Schema using quicktype, do not modify it directly.
// To parse the JSON, add this file to your project and do:
//
//   let rPMSymptoms = try? newJSONDecoder().decode(RPMSymptoms.self, from: jsonData)

import Foundation

// MARK: - RPMSymptom
struct RPMSymptom: Codable , Identifiable , Equatable{
    
    static func == (lhs: RPMSymptom, rhs: RPMSymptom) -> Bool {
        return true
    }
    func hash(into hasher: inout Hasher) {
        
    }
    let id = UUID()
    let ids: Int
    let symptom, rpmSymptomDescription, symptomStartDateTime: String
    
    enum CodingKeys: String, CodingKey {
        case ids = "Id"
        case symptom = "Symptom"
        case rpmSymptomDescription = "Description"
        case symptomStartDateTime = "SymptomStartDateTime"
    }
}

typealias RPMSymptoms = [RPMSymptom]
