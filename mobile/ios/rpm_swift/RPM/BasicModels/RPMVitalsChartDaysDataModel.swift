//
//  RPMVitalsChartDaysDataModel.swift
//  RPM
//
//  Created by Tesplabs on 11/07/1944 Saka.

import Foundation

struct RPMVitalsChartDaysDataModel: Codable, Identifiable {
    let id = UUID()
    let vitalName: String?
    let time: [String]?
    let values: [Valuev]?
    let latestvm: LatestVitalMeasure?
    
    enum CodingKeys: String, CodingKey {
        case vitalName = "VitalName"
        case time = "Time"
        case values = "Values"
        case latestvm = "LatestVitalMeasure"
    }
}

// A wrapper to handle both single-object and array responses
struct RPMVitalsChartDaysDataWrapper: Decodable {
    let items: [RPMVitalsChartDaysDataModel]

    init(from decoder: Decoder) throws {
        let container = try decoder.singleValueContainer()
        if let singleItem = try? container.decode(RPMVitalsChartDaysDataModel.self) {
            // Response is a single object, wrap it in an array
            self.items = [singleItem]
        } else if let multipleItems = try? container.decode([RPMVitalsChartDaysDataModel].self) {
            // Response is already an array
            self.items = multipleItems
        } else {
            throw DecodingError.dataCorruptedError(in: container, debugDescription: "Invalid JSON structure")
        }
    }
}



// MARK: - Value
struct Valuev: Codable ,Equatable , Identifiable {
    
    static func == (lhs: Valuev, rhs: Valuev) -> Bool {
        return lhs.label == rhs.label && lhs.data == rhs.data
    }


    func hash(into hasher: inout Hasher) {
        hasher.combine(id)
    }

        let id = UUID()
    
    let data: [String?]
    let label: String
}

// MARK: - LatestVitalMeasure
struct LatestVitalMeasure: Codable {
    let vitalName, date, value, unit: String?
    let pulseValue, pulseUnit, time: String?
    
    
    enum CodingKeys: String, CodingKey {
        case vitalName = "VitalName"
        case date = "Date"
        case value = "Value"
        case unit = "unit"
        case pulseValue = "PulseValue"
        case pulseUnit = "PulseUnit"
        case time = "time"
    }
}

