	//
//  RPMAlert.swift
//  RPM
//
//  Created by Prajeesh Prabhakar on 30/05/22.
//

import Foundation
import SwiftUI

struct AlertItem: Identifiable {
    var id = UUID()
    var title: String
    var message: String?
    var dismissButton: Alert.Button?
}


enum AlertContext {
    
    //MARK: - Network Errors
    static let invalidURL       = AlertItem(title: "Server Error",
                                            message: "There is an error trying to reach the server. If this persists, please contact support.",
                                            dismissButton: .default(Text("Ok")))
    
    static let unableToComplete = AlertItem(title: "Server Error",
                                            message: "Unable to complete your request at this time. Please check your internet connection.",
                                            dismissButton: .default(Text("Ok")))
    
    static let invalidResponse  = AlertItem(title: "Server Error",
                                            message: "Invalid response from the server. Please try again or contact support.",
                                            dismissButton: .default(Text("Ok")))
    static let invalidUser  = AlertItem(title: "Server Error",
                                            message: "Invalid User.",
                                            dismissButton: .default(Text("Ok")))
    
    static let invalidPassword  = AlertItem(title: "Login Failed, Please Check Your Credentials..!",
                                            message: "Your Account will be locked after 5 invalid attempts.",
                                            dismissButton: .default(Text("Ok")))
    
    static let invalidData      = AlertItem(title: "Server Error",
                                            message: "The data received from the server was invalid. Please try again or contact support.",
                                            dismissButton: .default(Text("Ok")))
    
    static let decodingError      = AlertItem(title: "Decoding Error",
                                            message: "Please try again or contact support.",
                                            dismissButton: .default(Text("Ok")))
    static let lockedError      = AlertItem(title: "User Locked",
                                            message: "Please contact your careteam.",
                                            dismissButton: .default(Text("Ok")))
    static let numberInvalidError      = AlertItem(title: "Invalid Number",
                                            message: "Failed to send otp to Mobile,Please contact Admin.",
                                            dismissButton: .default(Text("Ok")))
    static let otpWrongError      = AlertItem(title: "Error",
                                            message: "OTP may be Wrong.",
                                            dismissButton: .default(Text("Ok")))
    static let unauthorized      = AlertItem(title: "Error",
                                            message: "Session Expired.",
                                            dismissButton: .default(Text("Ok")))
  
}
