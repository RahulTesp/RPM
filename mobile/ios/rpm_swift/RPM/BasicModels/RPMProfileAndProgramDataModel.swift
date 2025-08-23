//
//  RPMHomePageDataModel.swift
//  RPM
//
//  Created by Tesplabs on 18/03/1944 Saka.
//


import Foundation


struct RPMProfileAndProgramDataModel: Decodable {
    
    let name: String?
    let userName: String?
    let programName: String?
    let programType: String?
    let completedDuration: String?
    let currentDuration, totalDuration: Int?
    let status: String?
    
    enum CodingKeys: String , CodingKey{
        case name = "Name"
        case userName = "UserName"
        case programName = "ProgramName"
        case programType = "ProgramType"
        case completedDuration = "CompletedDuration"
        case currentDuration = "CurrentDuration"
        case totalDuration = "TotalDuration"
        case status = "Status"
        
    }
    
    
    init(name: String , userName : String , programName : String, programType : String, completedDuration : String,currentDuration : Int,totalDuration : Int, status : String)
    {
        self.name = name
        self.userName = userName
        self.programName = programName
        self.programType = programType
        self.completedDuration = completedDuration
        self.currentDuration = currentDuration
        self.totalDuration = totalDuration
        self.status = status
    }
}
