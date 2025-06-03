//
//  MembersListDataModel.swift
//  RPM
//
//  Created by Tesplabs on 17/01/24.
//


// This file was generated from JSON Schema using quicktype, do not modify it directly.
// To parse the JSON, add this file to your project and do:
//
//   let medications = try? newJSONDecoder().decode(Medications.self, from: jsonData)

import Foundation

// MARK: - Medication
struct MembersListDataModel: Codable , Identifiable , Equatable{
    
    static func == (lhs: MembersListDataModel, rhs: MembersListDataModel) -> Bool {
        return true
    }
    func hash(into hasher: inout Hasher) {
        
    }
    let id = UUID()
    
  
    let memberUserName, role ,memberName : String
   
    
    enum CodingKeys: String, CodingKey {
        case memberUserName = "MemberUserName"
        case role = "Role"
        case memberName = "MemberName"
      
    }
}

typealias MembersListModels = [MembersListDataModel]
