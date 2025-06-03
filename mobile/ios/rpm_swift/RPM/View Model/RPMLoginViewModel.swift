//
//  RPMLoginViewModel.swift
//  RPM
//
//  Created by Prajeesh Prabhakar on 30/05/22.
//

import Foundation
import SwiftUI
import FirebaseMessaging

@MainActor
final class RPMLoginViewModel: ObservableObject {
    
    @Published var isAuthenticated : Bool = false
    @Published var accessToken : String = ""
    @Published var isLoggedOut : Bool = false
    @Published var loginData : RPMLoginDataModel?
    var usrnm : String?
    var usrpwd : String?
    @Published  var loggedIn: Bool = false
    @AppStorage("isLoggedIn") var isLoggedIn: Bool = false

    
    func login(userName: String, password: String, completed: @escaping (String?, AlertItem?) -> Void) {
        
        let defaults = UserDefaults.standard
        
        NetworkManager.shared.login(userName: userName, password: password) { result in
            DispatchQueue.main.async {
                switch result {
                case .success(let loginDataModel):
                    // Pass token immediately
               
                    
                    print("loginDataModel:", loginDataModel)
                    print("Token:", loginDataModel.tkn)
                    print("MFA:", loginDataModel.mfa)
                    
                    // Set MFA flags

                    defaults.setValue(loginDataModel.mfa, forKey: "MFAvalue")
                    
                    // Update view model state
                    self.loginData = loginDataModel
                    defaults.setValue(loginDataModel.mobilenumber, forKey: "MobileNumber")
                    self.isAuthenticated = true
                    self.isLoggedIn = true
                    
                    print("LOGINVWMODLisLoggedIn",self.isLoggedIn)
                    print("User authenticated:", self.isAuthenticated)
                    
                    if loginDataModel.mfa == true {
                        // Save token temporarily for MFA flow
                        defaults.setValue(loginDataModel.tkn, forKey: "jsonwebtokenold")
                        print("Saved MFA token (old):", defaults.string(forKey: "jsonwebtokenold") ?? "nil")
                    } else {
                        // Save token for normal flow
                        defaults.setValue(loginDataModel.tkn, forKey: "jsonwebtoken")
                        print("Saved login token:", defaults.string(forKey: "jsonwebtoken") ?? "nil")
                        
                        defaults.setValue("MFAENABLEDFALSE", forKey: "MFAENABLEDFALSE")
                        
                        // Fetch Firebase token
                        Messaging.messaging().token { token, error in
                            if let error = error {
                                print("Error fetching FCM token:", error)
                                return
                            }
                            
                            guard let fcmToken = token else {
                                print("FCM token is nil.")
                                return
                            }
                            
                            print("FCM Token:", fcmToken)
                            
                            let accessToken = defaults.string(forKey: "jsonwebtoken") ?? ""
                            
                            // Save FCM token to server
                            NetworkManager.shared.saveFirebaseToken(fbToken: fcmToken, accessToken: accessToken) { result in
                                switch result {
                                case .success:
                                    print("Firebase token saved successfully.")
                                    
                                case .failure(let error):
                                    print("Failed to save Firebase token:", error)
                                    
                                    switch error {
                                    case .invalidData:
                                        completed("", AlertContext.invalidData)
                                    case .invalidURL:
                                        completed("", AlertContext.invalidURL)
                                    case .invalidResponse:
                                        completed("", AlertContext.invalidResponse)
                                    case .unableToComplete:
                                        completed("", AlertContext.unableToComplete)
                                    case .decodingError:
                                        completed("", AlertContext.decodingError)
                                    case .invalidPassword:
                                        completed("", AlertContext.invalidPassword)
                                    case .lockedError:
                                        completed("", AlertContext.lockedError)
                                    case .numberInvalidError:
                                        completed("", AlertContext.numberInvalidError)
                                    case .otpWrongError:
                                        completed("", AlertContext.otpWrongError)
                                    case .invalidUser:
                                        completed("", AlertContext.invalidUser)
                                    case .noInternet:
                                        completed("", AlertContext.noInternet)
                                    case .unauthorized:
                                        completed("", AlertContext.unauthorized)
                                    }
                                }
                            }
                        }
                    }
                    
                    completed(loginDataModel.tkn, nil)

                case .failure(let error):
                    switch error {
                    case .invalidData:
                        completed(nil, AlertContext.invalidData)
                    case .invalidURL:
                        completed(nil, AlertContext.invalidURL)
                    case .invalidResponse:
                        completed(nil, AlertContext.invalidResponse)
                    case .invalidPassword:
                        completed(nil, AlertContext.invalidPassword)
                    case .unableToComplete:
                        completed(nil, AlertContext.unableToComplete)
                    case .decodingError:
                        completed(nil, AlertContext.decodingError)
                    case .lockedError:
                        completed(nil, AlertContext.lockedError)
                    case .numberInvalidError:
                        completed(nil, AlertContext.numberInvalidError)
                    case .otpWrongError:
                        completed(nil, AlertContext.otpWrongError)
                    case .invalidUser:
                        completed(nil, AlertContext.invalidUser)
                    case .noInternet:
                        completed(nil, AlertContext.noInternet)
                    case .unauthorized:
                        completed(nil, AlertContext.unauthorized)
                    }
                }
            }
        }
    }

 
    func verifyOtp(userName: String, otp: String, completed: @escaping (String?, AlertItem?) -> Void) {
        print("userName")
        print("otp")
        print(userName)
        print(otp)
       
        let defaults = UserDefaults.standard
        NetworkManager.shared.otpVerification(userName: userName,
                                    otp: otp,
                                    completed: { result in
            DispatchQueue.main.async {
                switch result {
                case .success(let otpDataModel):
                    completed(otpDataModel.tkn, nil)
                    print("otpDataModel",otpDataModel)
                  
                    //permanent token
                    defaults.setValue(otpDataModel.tkn, forKey: "jsonwebtoken")
                    
                   
                    print("tokenvalverif", UserDefaults.standard.string(forKey: "jsonwebtoken"))
                    
                    self.accessToken = otpDataModel.tkn
                    
                    
                    defaults.setValue(otpDataModel.mfa, forKey: "MFAvalue")
                    print("otpDataModelmfa", UserDefaults.standard.bool(forKey: "MFAvalue"))
                    
                    defaults.setValue("MFAENABLEDTRUE", forKey: "MFAENABLEDTRUE")
                  
                        self.isAuthenticated = true
                        
                        print("self.isAuthenticatedlogins")
                        print(self.isAuthenticated)
                 
                case .failure(let error):
                    print("otperror")
                        print(error)
                    UserDefaults.standard.setValue(false, forKey: "userLocked")
                    print("userLocked val1111")
                    print( (UserDefaults.standard.bool(forKey: "userLocked") ))
                    if(error == .lockedError)
                                        {
                                            UserDefaults.standard.setValue(true, forKey: "userLocked")
                                            print("userLocked val222")
                                            print( (UserDefaults.standard.bool(forKey: "userLocked") ))
                    
                                        }
                    
                    if(error == .otpWrongError)
                                        {
                                            UserDefaults.standard.setValue(true, forKey: "otpWrong")
                                            print("userLocked val222")
                                            print( (UserDefaults.standard.bool(forKey: "otpWrong") ))
                    
                                        }
                    switch error {
                    case .invalidData:
                        completed(nil, AlertContext.invalidData)
                    case .invalidURL:
                        completed(nil, AlertContext.invalidURL)
                    case .invalidResponse:
                        completed(nil, AlertContext.invalidResponse)
                    case .invalidPassword:
                        completed(nil, AlertContext.invalidPassword)
                    case .unableToComplete:
                        completed(nil, AlertContext.unableToComplete)
                    case .decodingError:
                        completed(nil, AlertContext.decodingError)
                    case .lockedError:
                        completed(nil, AlertContext.lockedError)
                    case .numberInvalidError:
                        completed(nil, AlertContext.numberInvalidError)
                    case .invalidUser:
                        completed(nil, AlertContext.invalidUser)
                    case .otpWrongError:
                        completed(nil, AlertContext.otpWrongError)
                    case .noInternet:
                        completed(nil, AlertContext.noInternet)
                    case .unauthorized:
                        completed(nil, AlertContext.unauthorized)
                    }
                }
            }
        })
    }
    
    
    func logout(completion: @escaping (String?, AlertItem?) -> Void) {
        print("self.ggttttt")
        print(self.isAuthenticated)
        let defaults = UserDefaults.standard
        
        guard let tkn = defaults.string(forKey: "jsonwebtoken") else {
            completion(nil, AlertContext.invalidUser) // or other alert
            return
        }
        
        NetworkManager.shared.logOut(tkn: tkn) { result in
            DispatchQueue.main.async {
                switch result {
                case .success(let responseString):
                    print("LOGOUTresponseString",responseString)
                    print("self.isAuthenticated",self.isAuthenticated)
                 
                    self.isAuthenticated = false
                    print("self.isAuthenticated",self.isAuthenticated)
                
                    self.isLoggedOut = true
                    print("self.isLoggedOut",self.isLoggedOut)
                
                    completion(responseString, nil)
                    
                case .failure(let error):
                    print("logOutrror", error)
                    let alert: AlertItem
                    switch error {
                    case .invalidData: alert = AlertContext.invalidData
                    case .invalidURL: alert = AlertContext.invalidURL
                    case .invalidResponse: alert = AlertContext.invalidResponse
                    case .unableToComplete: alert = AlertContext.unableToComplete
                    case .decodingError: alert = AlertContext.decodingError
                    case .invalidPassword: alert = AlertContext.invalidPassword
                    case .lockedError: alert = AlertContext.lockedError
                    case .numberInvalidError: alert = AlertContext.numberInvalidError
                    case .otpWrongError: alert = AlertContext.otpWrongError
                    case .invalidUser: alert = AlertContext.invalidUser
                    case .noInternet: alert = AlertContext.noInternet
                    case .unauthorized:
                        alert = AlertContext.unauthorized
                    }
                    completion(nil, alert)
                }
            }
        }
    }
}
