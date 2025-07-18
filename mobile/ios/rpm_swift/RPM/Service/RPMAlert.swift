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
    var title: Text
    var message: Text
    var dismissButton: Alert.Button?
}

enum AlertContext {
    
    //MARK: - Network Errors
    static let invalidURL       = AlertItem(title: Text("Server Error"),
                                            message: Text("There is an error trying to reach the server. If this persists, please contact support."),
                                            dismissButton: .default(Text("Ok")))
    
    static let unableToComplete = AlertItem(title: Text("Server Error"),
                                            message: Text("Unable to complete your request at this time. Please check your internet connection."),
                                            dismissButton: .default(Text("Ok")))
    
    static let invalidResponse  = AlertItem(title: Text("Server Error"),
                                            message: Text("Invalid response from the server. Please try again or contact support."),
                                            dismissButton: .default(Text("Ok")))
    static let invalidUser  = AlertItem(title: Text("Server Error"),
                                            message: Text("Invalid User."),
                                            dismissButton: .default(Text("Ok")))
    
    static let invalidPassword  = AlertItem(title: Text("Server Error"),
                                            message: Text("You have entered an invalid username or password."),
                                            dismissButton: .default(Text("Ok")))
    
    static let invalidData      = AlertItem(title: Text("Server Error"),
                                            message: Text("The data received from the server was invalid. Please try again or contact support."),
                                            dismissButton: .default(Text("Ok")))
    
    static let decodingError      = AlertItem(title: Text("Decoding Error"),
                                            message: Text("Please try again or contact support."),
                                            dismissButton: .default(Text("Ok")))
    static let lockedError      = AlertItem(title: Text("User Locked"),
                                            message: Text("Please contact your careteam."),
                                            dismissButton: .default(Text("Ok")))
    static let numberInvalidError      = AlertItem(title: Text("Invalid Number"),
                                            message: Text("Failed to send otp to Mobile,Please contact Admin."),
                                            dismissButton: .default(Text("Ok")))
    static let otpWrongError      = AlertItem(title: Text("Error"),
                                            message: Text("OTP may be Wrong."),
                                            dismissButton: .default(Text("Ok")))
    static let noInternet      = AlertItem(title: Text("Error"),
                                            message: Text("No Internet Connection."),
                                            dismissButton: .default(Text("Ok")))
    static let unauthorized      = AlertItem(title: Text("Error"),
                                            message: Text("Session Expired."),
                                            dismissButton: .default(Text("Ok")))
  
}
