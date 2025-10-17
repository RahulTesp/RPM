


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

    init() {
       }
    
    
    func login(userName: String, password: String, completed: @escaping (String?, AlertItem?) -> Void) {
        let defaults = UserDefaults.standard
        NetworkManager.shared.login(userName: userName, password: password) { [weak self] result in
            guard let self = self else { return }
            DispatchQueue.main.async {
                switch result {
                case .success(let loginDataModel):
                    print("loginDataModel:", loginDataModel)
                    print("Token:", loginDataModel.tkn)
                    print("MFA:", loginDataModel.mfa)
                    // Set MFA flags
                    defaults.setValue(loginDataModel.mfa, forKey: "MFAvalue")
                    // Update view model state
                    self.loginData = loginDataModel
                    defaults.setValue(loginDataModel.mobilenumber, forKey: "MobileNumber")

                    if loginDataModel.mfa == true {
                        // MFA true → temporary token
                        defaults.setValue(loginDataModel.tkn, forKey: "jsonwebtokenold")
                        print("Saved MFA token (old):", defaults.string(forKey: "jsonwebtokenold") ?? "nil")
                        // Trigger the completion to SwiftUI
                                                completed(loginDataModel.tkn, nil)
                     
                    } else {
                        SessionManager.shared.reset()
                        self.isAuthenticated = true
                        print("User authenticated:", self.isAuthenticated)
                        // MFA false → final token
                        defaults.setValue(loginDataModel.tkn, forKey: "jsonwebtoken")
                        print("Saved login token:", defaults.string(forKey: "jsonwebtoken") ?? "nil")
                        defaults.setValue("MFAENABLEDFALSE", forKey: "MFAENABLEDFALSE")
                        //  Save Firebase token to server
                        self.saveFirebaseTokenIfAvailable()
                    }
     
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
                    case .unauthorized:
                        completed(nil, AlertContext.unauthorized)
                    }
                }
            }
        }
    }
    
    

    func verifyOtp(userName: String, otp: String, completed: @escaping (String?, AlertItem?) -> Void) {
        let defaults = UserDefaults.standard
        NetworkManager.shared.otpVerification(userName: userName, otp: otp) { [weak self] result in
            guard let self = self else { return }
            DispatchQueue.main.async {
                switch result {
                case .success(let otpDataModel):
                    completed(otpDataModel.tkn, nil)
                    print("otpDataModel", otpDataModel)
                    // Permanent token
                    defaults.setValue(otpDataModel.tkn, forKey: "jsonwebtoken")
                    print("tokenvalverif", UserDefaults.standard.string(forKey: "jsonwebtoken") ?? "")
                    self.accessToken = otpDataModel.tkn
                    defaults.setValue(otpDataModel.mfa, forKey: "MFAvalue")
                    print("otpDataModelmfa", UserDefaults.standard.bool(forKey: "MFAvalue"))
                    defaults.setValue("MFAENABLEDTRUE", forKey: "MFAENABLEDTRUE")
                    SessionManager.shared.reset()
                    self.isAuthenticated = true
                    print("self.isAuthenticatedlogins")
                    print(self.isAuthenticated)
                    //  When MFA is true → final token is ready, now save Firebase token

                        self.saveFirebaseTokenIfAvailable()
     
                case .failure(let error):
                    print("otperror: \(error)")
                    UserDefaults.standard.setValue(false, forKey: "userLocked")
     
                    if error == .lockedError {
                        UserDefaults.standard.setValue(true, forKey: "userLocked")
                    }
                    if error == .otpWrongError {
                        UserDefaults.standard.setValue(true, forKey: "otpWrong")
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
                    case .unauthorized:
                        completed(nil, AlertContext.unauthorized)
                    }
                }
            }
        }
    }
    
    
      
    func saveFirebaseTokenIfAvailable() {
        print("saveFirebaseTokenIfAvailable")
        let defaults = UserDefaults.standard
        guard let fcmToken = defaults.string(forKey: "FCMToken"),
              let accessToken = defaults.string(forKey: "jsonwebtoken") else {
            print("FCM token or access token not available yet")
            return
        }
        NetworkManager.shared.saveFirebaseToken(fbToken: fcmToken, accessToken: accessToken) { result in
            switch result {
            case .success:
                print("Firebase token saved successfully.",fcmToken)
            case .failure(let error):
                print("Failed to save Firebase token: \(error)")
            }
        }
    }
    
    func logout(completion: @escaping (String?, AlertItem?) -> Void) {
        print("self.ggttttt")
        print(self.isAuthenticated)
        let defaults = UserDefaults.standard
        
        guard let tkn = defaults.string(forKey: "jsonwebtoken") else {
            completion(nil, AlertContext.invalidUser)
            return
        }
        
        NetworkManager.shared.logOut(tkn: tkn) { result in
            DispatchQueue.main.async {
                switch result {
                case .success(let responseString):
                    print("LOGOUTresponseString", responseString)
                    
                    // Clear token
                                   defaults.removeObject(forKey: "jsonwebtoken")
                                   
                    
                    // Reset session flags
                    SessionManager.shared.reset()
                    
                    // Make sure UI goes back to login
                    self.isAuthenticated = false
                    self.isLoggedOut = true
                    
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
                    case .unauthorized: alert = AlertContext.unauthorized
                    }
                    completion(nil, alert)
                }
            }
        }
    }
}

