//
//  RPMChangePwdViewModel.swift
//  RPM
//
//  Created by Tesplabs on 03/05/1944 Saka.
//

import Foundation

import SwiftUI

@MainActor
final class RPMChangePwdViewModel: ObservableObject {
    
    @Published var isPwdChanged : Bool = false
    
    
    func changePwd(userName: String, oldPassword: String, newPassword: String, completed: @escaping (Bool, AlertItem?) -> Void) {
        
        let defaults = UserDefaults.standard
        
        guard let tkn = defaults.string(forKey: "jsonwebtoken")
            
        else {
            
            return
        }
        
        
        NetworkManager.shared.changePwd(
            tkn: tkn,
            userName: userName,
            oldPassword: oldPassword,
            newPassword: newPassword,
            completed: { result in
           
                    switch result {
                    case .success(let account):
                        
                        print("changePwdSUCCESS",account)
                     
                            self.isPwdChanged = true
                            completed(true, nil)
                     
                    case .failure(let error):
                        print("changePwderror",error)
                    
                        switch error {
                        case .invalidData:
                            completed(false, AlertContext.invalidData)
                        case .invalidURL:
                            completed(false, AlertContext.invalidURL)
                        case .invalidResponse:
                            completed(false, AlertContext.invalidResponse)
                        case .unableToComplete:
                            completed(false, AlertContext.unableToComplete)
                        case .decodingError:
                            completed(false, AlertContext.decodingError)
                        case .invalidPassword:
                            completed(false, AlertContext.invalidPassword)
                        case .lockedError:
                            completed(false, AlertContext.lockedError)
                        case .numberInvalidError:
                            completed(false, AlertContext.numberInvalidError)
                        case .otpWrongError:
                            completed(false, AlertContext.otpWrongError)
                        case .invalidUser:
                            completed(false, AlertContext.invalidUser)
                        case .noInternet:
                            completed(false, AlertContext.noInternet)
                     
                        case .unauthorized:
                            completed(false, AlertContext.unauthorized)
                        }
                }
            })
    }
}




