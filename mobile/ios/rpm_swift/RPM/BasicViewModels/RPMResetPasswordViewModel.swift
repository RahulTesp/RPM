//
//  RPMResetPasswordViewModel.swift
//  RPM
//
//  Created by Tesplabs on 17/12/1944 Saka.
//

import Foundation
import UIKit
import SwiftUI


@MainActor

final class RPMResetPasswordViewModel: ObservableObject {
    
    @Published var isAuthenticated : Bool = false
    @Published var isLoggedOut : Bool = false
    @Published var loginData : RPMGeneratePasswordDataModel?
    var usrnm : String?
    var usrpwd : String?
    @Published  var loggedIn: Bool = false
    @Published var isOn: Bool = false
    
    func generateOTP(userName: String, completed: @escaping (String?, AlertItem?) -> Void) {
        let defaults = UserDefaults.standard
        
        NetworkManager.shared.generateOTP(userName: userName) { result in
            switch result {
            case .success(let generateOTPDataModel):
                DispatchQueue.main.async {   // ensure on main
                    completed(generateOTPDataModel.tkn, nil)
                    
                    self.loginData = generateOTPDataModel
                    self.isAuthenticated = true
                    
                    defaults.set(generateOTPDataModel.mobilenumber, forKey: "MobileNumberRP")
                    defaults.set(generateOTPDataModel.tkn, forKey: "jsonwebtokenRP")
                    defaults.set(generateOTPDataModel.timeLimit, forKey: "TimeLimitRP")
                }
                
            case .failure(let error):
                DispatchQueue.main.async {   //  ensure on main
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

 
    
    func resetPassword(
        userName: String,
        otp: String,
        password: String,
        isResetpwd: Binding<Bool>,
        completed: @escaping (String?, AlertItem?) -> Void
    ) {
        print("userName")
        print("otp")
        print(userName)
        print(otp)
        
        UserDefaults.standard.setValue(false, forKey: "resetSuccess")
        
        NetworkManager.shared.resetPassword(userName: userName, otp: otp, password: password) { result in
            
            switch result {
            case .success(let account):
                print("account")
                print(account)
                
                //  Update all UI-bound state on main thread
                DispatchQueue.main.async {
                    isResetpwd.wrappedValue = true  // triggers alert
                    self.isOn = true
                    UserDefaults.standard.set(true, forKey: "resetSuccess")
                    completed(account, nil)
                }

                
            case .failure(let error):
                print("error")
                print(error)
                
                DispatchQueue.main.async {
                    UserDefaults.standard.setValue(false, forKey: "userLocked")
                    if error == .lockedError {
                        UserDefaults.standard.setValue(true, forKey: "userLocked")
                    }
                    
                    print("userLocked: \(UserDefaults.standard.bool(forKey: "userLocked"))")
                    
                    // Send the appropriate alert
                    switch error {
                    case .invalidData: completed(nil, AlertContext.invalidData)
                    case .invalidURL: completed(nil, AlertContext.invalidURL)
                    case .invalidResponse: completed(nil, AlertContext.invalidResponse)
                    case .invalidPassword: completed(nil, AlertContext.invalidPassword)
                    case .unableToComplete: completed(nil, AlertContext.unableToComplete)
                    case .decodingError: completed(nil, AlertContext.decodingError)
                    case .lockedError: completed(nil, AlertContext.lockedError)
                    case .numberInvalidError: completed(nil, AlertContext.numberInvalidError)
                    case .otpWrongError: completed(nil, AlertContext.otpWrongError)
                    case .invalidUser: completed(nil, AlertContext.invalidUser)
                    case .unauthorized: completed(nil, AlertContext.unauthorized)
                    }
                }
            }
        }
    }

 
    
    func logout( completed:@escaping (String?, AlertItem?) -> Void) {
        
        print("self.ggttttt")
        print(self.isAuthenticated)
        let defaults = UserDefaults.standard
        
        guard let tkn = defaults.string(forKey: "jsonwebtoken")
                
        else {
            
            return
        }
        
        NetworkManager.shared.logOut(
            tkn: tkn,
            
            completed: { result in
                
                switch result {
                case .success(let result):
                    print("self.eeeeisLogout")
                    print(self.isAuthenticated)
                    print("\nSUCCESS\n")
                    print(result)
                    
                    print("self.isAuthenticated logout")
                    print(self.isAuthenticated)
                    
                    self.isAuthenticated = false
                    print(self.isAuthenticated)
                    
                    self.isLoggedOut = true
                    print(self.isLoggedOut)
                    
                case .failure(let error):
                    switch error {
                    case .invalidData:
                        completed(nil, AlertContext.invalidData)
                    case .invalidURL:
                        completed(nil, AlertContext.invalidURL)
                    case .invalidResponse:
                        completed(nil, AlertContext.invalidResponse)
                    case .unableToComplete:
                        completed(nil, AlertContext.unableToComplete)
                    case .decodingError:
                        completed(nil, AlertContext.decodingError)
                    case .invalidPassword:
                        completed(nil, AlertContext.invalidPassword)
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
            })
    }
    
}
