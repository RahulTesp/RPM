//
//  RMPTodoListViewModel.swift
//  RPM
//
//  Created by Tesplabs on 10/04/1944 Saka.
//


import Foundation
import SwiftUI

@MainActor
final class RPMTodoListViewModel: ObservableObject {
    @Published var todoAct : [TodoList]?
    @Published var loadingtodoAct = true

    init() {

    }
    func refresh() {
        let todaysDate = NSDate()
        let dateFormatter = DateFormatter()
        dateFormatter.dateFormat = "yyyy-MM-dd"
        let DateInFormat = dateFormatter.string(from: todaysDate as Date)
        
        loadingtodoAct = true
        todoActivities(dt: DateInFormat, completed: { aa, vv in
            print("aa")
       
        })
       }
    
    
   
    func todoActivities(dt: String, completed: @escaping (String?, AlertItem?) -> Void) {
        let defaults = UserDefaults.standard
        guard let tkn = defaults.string(forKey: "jsonwebtoken") else {
          //  self.showNoInternetAlert = true
            completed(nil, nil)
            return
        }

        DashboardManager.shared.todolistsActivities(tkn: tkn, dt: dt) { result in
            DispatchQueue.main.async {

            switch result {
            case .success(let activities):
                self.todoAct = activities
                self.loadingtodoAct = false
                completed("Success", nil)
                
            case .failure(let error):
                self.todoAct = []
                self.loadingtodoAct = false
        
                if error == .unauthorized {
                    SessionManager.shared.handleUnauthorizedResponse()
                }
                
                let alert = AlertItem(
                    title: "Error",
                    message: error.localizedDescription,
                    dismissButton: .default(Text("OK"))
                )
                completed(nil, alert)
            }
        }
        }
    }
}


