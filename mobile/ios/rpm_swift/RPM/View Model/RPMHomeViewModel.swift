//
//  RMPHomeViewModel.swift
//  RPM
//
//  Created by Tesplabs on 18/03/1944 Saka.
//


import Foundation
import SwiftUI

@MainActor
final class RPMHomeViewModel: ObservableObject {
    @Published var accounts : RPMProfileAndProgramDataModel?
    @Published var loading = true
    @Published var notificationCount : NotificationCount?
    
    init() {
  
    }
    
    func reset() {
          accounts = nil
          // Reset other values as needed
      }
    
    func dashboard()
    {
   
        guard let tkn = jwtToken else { return }

        DashboardManager.shared.dashboard(tkn: tkn){ (result ) in
            DispatchQueue.main.async {
            switch result {
            case .success(let account):
        
                self.accounts = account
                self.loading = false
                print("dashbosrdloading")
                print(self.loading)
          
            case .failure(let error):
          
                self.loading = false
                print("dashboard failed: \(error)")
                if error == .unauthorized {
                    SessionManager.shared.handleUnauthorizedResponse()
                }
             
            }
        }
        }
    }
    
    func getnotifyCount()
    
    {

        guard let tkn = jwtToken else { return }

        DashboardManager.shared.getnotifyCount(tkn: tkn){ (result ) in
            DispatchQueue.main.async {
            switch result {
            case .success(let account):
                print("success")
              
                self.notificationCount = account
                self.loading = false
            
            case .failure(let error):
           
                self.loading = false
                print("notif failed: \(error)")
             
            }
        }
        }
    }
    
    func rejectCall(roomName: String, completion: @escaping (Bool) -> Void){
        NetworkManager.shared.rejectCall { result in
            switch result {
            case .success(let success):
                if success {
                    print(" Call rejected successfully for room: \(roomName)")
                    completion(true)
                } else {
                    print(" Call rejection failed (but no error) for room: \(roomName)")
                    completion(false)
                }
            case .failure(let error):
                print(" API Error while rejecting call for room \(roomName): \(error.localizedDescription)")
                completion(false)
            }
        }
    }
}

private var jwtToken: String? {
    UserDefaults.standard.string(forKey: "jsonwebtoken")
}
