//
//  RPMGeneratePasswordDataModel.swift
//  RPM
//
//  Created by Tesplabs on 18/12/1944 Saka.
//

import Foundation
// This file was generated from JSON Schema using quicktype, do not modify it directly.
// To parse the JSON, add this file to your project and do:
//
//   let rPMGeneratePasswordDataModel = try? JSONDecoder().decode(RPMGeneratePasswordDataModel.self, from: jsonData)



// MARK: - RPMGeneratePasswordDataModel
struct RPMGeneratePasswordDataModel: Codable {
    let tkn, tkt: String
    let reqPasswordchange: Bool

    let mfa: Bool
    let mobilenumber: String

    let timeLimit: Int
    let validMailID, validMobile: Bool

    enum CodingKeys: String, CodingKey {
        case tkn, tkt, reqPasswordchange
    
        case mfa = "MFA"
        case mobilenumber = "Mobilenumber"
       
        case timeLimit = "TimeLimit"
        case validMailID = "ValidMailId"
        case validMobile = "ValidMobile"
    }
}


