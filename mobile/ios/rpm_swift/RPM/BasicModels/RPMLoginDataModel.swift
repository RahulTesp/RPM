// This file was generated from JSON Schema using quicktype, do not modify it directly.
// To parse the JSON, add this file to your project and do:
//
//   let rPMLoginDataModel = try? JSONDecoder().decode(RPMLoginDataModel.self, from: jsonData)

import Foundation

// MARK: - RPMLoginDataModel
struct RPMLoginDataModel: Codable {
    let tkn, tkt: String
    let reqPasswordchange: Bool
    let roles: [Role]?
    let mfa: Bool?
    let mobilenumber: String?
    let timeLimit: Int?

    enum CodingKeys: String, CodingKey {
        case tkn = "tkn"
        case tkt = "tkt"
        
         case reqPasswordchange = "reqPasswordchange"
        case roles = "Roles"
        case mfa = "MFA"
        case mobilenumber = "Mobilenumber"
        case timeLimit = "TimeLimit"
    }
}

// MARK: - Role
struct Role: Codable {
    let id: Int
    let role,programName: String

    enum CodingKeys: String, CodingKey {
        case id = "Id"
        case role = "Role"
        case programName = "ProgramName"
    }
}
