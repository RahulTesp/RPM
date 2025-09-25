//
//  RPMMedicationsAddViewModel.swift
//  RPM
//
//  Created by Tesplabs on 13/08/1944 Saka.
//



import Foundation
import SwiftUI

@MainActor

final class RPMMedicationsAddViewModel: ObservableObject {
    
    @Published var isPwdChanged : Bool = false
    @Published var showingAlert: Bool? = false
    
    func addMed(medName: String, schedule1: String,schedule2: String, morning : Int , afternoon : Int, evening : Int , night : Int, startDate: String, endDate : String , comments : String , completed: @escaping (Bool, AlertItem?) -> Void) {
       
        let defaults = UserDefaults.standard
        
        guard let tkn = defaults.string(forKey: "jsonwebtoken")
              
        else {
            
            return
        }
        
        
        MoreManager.shared.addMedications(
            tkn: tkn,
            medName: medName,
            schedule1: schedule1,
            schedule2: schedule2,
            morning : morning,
            afternoon : afternoon,
            evening : evening,
            night : night,
            startDate: startDate,
            endDate : endDate ,
            comments : comments,
            completed: { result in
       
                    switch result {
                    case .success(let account):
                        
                        print("\nMEDSUCCESS\n",account)
                   
                            self.isPwdChanged = true
                            self.showingAlert = true
                            completed(true, nil)
                    
                    case .failure(let error):
                        
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
                        case .unauthorized:
                            completed(false, AlertContext.unauthorized)
                        }
                        completed(false, nil)
                 
                }
            })
    }
    
}




