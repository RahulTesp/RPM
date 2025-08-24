//
//  MembersListViewModel.swift
//  RPM
//
//  Created by Tesplabs on 17/01/24.
//


import Foundation
import SwiftUI

@MainActor
final class MembersListViewModel: ObservableObject {
    @Published var memberDetails : [MembersListDataModel]?
    @Published var loading = true
    
    init() {
        print("initmemeberapicalled")
    }
    
    func memDetails()
    {
        
        
        let defaults = UserDefaults.standard
        guard let tkn = defaults.string(forKey: "jsonwebtoken")
                
                
        else {
            
            return
        }
        
        
        DashboardManager.shared.membersLists(tkn: tkn){ (result ) in
            DispatchQueue.main.async {
                switch result {
                case .success(let account):
                    
                    self.memberDetails = account
                    self.loading = false
                    print("membersListsloading")
                    print(self.loading)
                    
                    
                case .failure(let accounttt):
                    print("membersListsaccounttt")
                    print(accounttt)
                    self.loading = false
                    print("membersListsloadingF")
                    print(self.loading)
                    print("memberDetails faileddd")
                    
                }
            }
        }
        
    }
    
    
    func getChatSidForMember(memberUsername: String,  completed: @escaping (String,Bool, AlertItem?) -> Void) {
        
        print("getChatSidCALL")
        let defaults = UserDefaults.standard
        
        guard let tkn = defaults.string(forKey: "jsonwebtoken")
                
                
        else {
            
            return
        }
        
        
        DashboardManager.shared.getChatSID(
            tkn: tkn,
            memberUserName: memberUsername,
            
            completed: { result in
                DispatchQueue.main.async {
                    switch result {
                    case .success(let account):
                        
                        print("getChatSIDSUCCESS",account)
                        
                        completed(account,true, nil)
                        
                    case .failure(let error):
                        print("getChatSIDerror",error)
                        switch error {
                        case .invalidData:
                            completed("",false, AlertContext.invalidData)
                        case .invalidURL:
                            completed("",false, AlertContext.invalidURL)
                        case .invalidResponse:
                            completed("",false, AlertContext.invalidResponse)
                        case .unableToComplete:
                            completed("",false, AlertContext.unableToComplete)
                        case .decodingError:
                            completed("",false, AlertContext.decodingError)
                        case .invalidPassword:
                            completed("",false, AlertContext.invalidPassword)
                        case .lockedError:
                            completed("",false, AlertContext.lockedError)
                        case .numberInvalidError:
                            completed("",false, AlertContext.numberInvalidError)
                        case .otpWrongError:
                            completed("",false, AlertContext.otpWrongError)
                        case .invalidUser:
                            completed("",false, AlertContext.invalidUser)
                        case .unauthorized:
                            completed("",false, AlertContext.unauthorized)
                        }
                        
                    }
                }
            })
    }
    
    
    func chatUpdate(toUser: String, convSid : String, completed: @escaping (Int,Bool, AlertItem?) -> Void) {
        
        print("chatUpdateCALL")
        
        DashboardManager.shared.chatUpdate(
            toUser: toUser,
            convSID: convSid,
            
            completed: { result in
                DispatchQueue.main.async {
                    switch result {
                    case .success(let statusCode):
                        
                        print("chatUpdateSUCCESS",statusCode)
                        
                        completed(statusCode,true, nil)
                        
                    case .failure(let error):
                        print("chatUpdateerror",error)
                        switch error {
                        case .invalidData:
                            completed(0,false, AlertContext.invalidData)
                        case .invalidURL:
                            completed(0,false, AlertContext.invalidURL)
                        case .invalidResponse:
                            completed(0,false, AlertContext.invalidResponse)
                        case .unableToComplete:
                            completed(0,false, AlertContext.unableToComplete)
                        case .decodingError:
                            completed(0,false, AlertContext.decodingError)
                        case .invalidPassword:
                            completed(0,false, AlertContext.invalidPassword)
                        case .lockedError:
                            completed(0,false, AlertContext.lockedError)
                        case .numberInvalidError:
                            completed(0,false, AlertContext.numberInvalidError)
                        case .otpWrongError:
                            completed(0,false, AlertContext.otpWrongError)
                        case .invalidUser:
                            completed(0,false, AlertContext.invalidUser)
                        case .unauthorized:
                            completed(0,false, AlertContext.unauthorized)
                        }
                        
                    }
                }
            })
    }
    
    
}



