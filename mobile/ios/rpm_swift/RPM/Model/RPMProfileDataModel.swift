//
//  RPMProfileDataModel.swift
//  RPM
//
//  Created by Tesplabs on 15/04/1944 Saka.
//


import Foundation

// MARK: - Profile
struct Profile: Codable {
    let firstName, middleName, lastName, dob: String
    let gender: String
    let height, weight: Int
    let email, phoneNo, alternateMobNo, address1: String
    let address2, city, state: String
    let timeZone: String
    let zipCode, emergencyContact1, emergencyContactNumber1, emergencyContact1Relation: String
    let emergencyContact2, emergencyContactNumber2, emergencyContact2Relation, callTime: String
    let language, preference1, preference2, preference3: String
    let notes: String
    
    enum CodingKeys: String, CodingKey {
        case firstName = "FirstName"
        case middleName = "MiddleName"
        case lastName = "LastName"
        case dob = "DOB"
        case gender = "Gender"
        case height = "Height"
        case weight = "Weight"
        case email = "Email"
        case phoneNo = "PhoneNo"
        case alternateMobNo = "AlternateMobNo"
        case address1 = "Address1"
        case address2 = "Address2"
        case city = "City"
        case state = "State"
        case timeZone = "TimeZone"
        case zipCode = "ZipCode"
        case emergencyContact1 = "EmergencyContact1"
        case emergencyContactNumber1 = "EmergencyContactNumber1"
        case emergencyContact1Relation = "EmergencyContact1Relation"
        case emergencyContact2 = "EmergencyContact2"
        case emergencyContactNumber2 = "EmergencyContactNumber2"
        case emergencyContact2Relation = "EmergencyContact2Relation"
        case callTime = "CallTime"
        case language = "Language"
        case preference1 = "Preference1"
        case preference2 = "Preference2"
        case preference3 = "Preference3"
        case notes = "Notes"
    }
}

typealias Profiles = [Profile]
