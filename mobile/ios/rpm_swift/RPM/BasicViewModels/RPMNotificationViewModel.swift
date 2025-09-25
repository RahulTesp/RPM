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
  
       // getnotify()
        
    }
 
    func getnotify(completion: (() -> Void)? = nil) {
        print("getnotifyCALLED")
        
        let defaults = UserDefaults.standard
        guard let tkn = defaults.string(forKey: "jsonwebtoken") else {
            completion?() // Still call it, even if token is missing
            return
        }

        DashboardManager.shared.getnotify(tkn: tkn) { result in
            DispatchQueue.main.async {
                switch result {
                case .success(let account):
                    print("getnotifysuccess", account)
                    self.notif = account

                case .failure:
                    print("getnotify failed")
                }
                completion?() //  Notify that fetch is done in both success/failure
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
