//
//  NotificationViewModel.swift
//  RPM
//
//  Created by Tesplabs on 01/04/1944 Saka.
//

import Foundation


@MainActor
final class NotificationViewModel: ObservableObject {
    
    @Published var notifications : String?
    @Published var notif : Notifications?
 
    init() {
  
        getnotify()
        
    }
 
    func getnotify()
    
    {
        print("getnotifyCALLED")
        
        let defaults = UserDefaults.standard
        guard let tkn = defaults.string(forKey: "jsonwebtoken")
                
                
        else {
            
            return
        }
        
        DashboardManager.shared.getnotify(tkn: tkn){ (result ) in
            DispatchQueue.main.async {
            switch result {
            case .success(let account):
                print("getnotifysuccess",account)
          
                self.notif = account
            
            case .failure:
                print("getnotify faileddd")
                
                
            }
        }
        }
    }
  
    func deleteAllNotifications() {
        let defaults = UserDefaults.standard
        guard let token = defaults.string(forKey: "jsonwebtoken") else {
            print("Token not found")
            return
        }
        
        DashboardManager.shared.deleteAllNotifications(tkn: token) { result in
            DispatchQueue.main.async {
            switch result {
            case .success:
                print("All notifications deleted successfully")
           
                self.getnotify() // Fetch fresh notifications after deleting
    
            case .failure(let error):
                print("Failed to delete notifications: \(error)")
            }
        }
        }
    }

}
