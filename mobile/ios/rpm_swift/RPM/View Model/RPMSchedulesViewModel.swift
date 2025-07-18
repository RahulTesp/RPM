
//  RPMSchedulesViewModel.swift
//  RPM
//
//  Created by Tesplabs on 27/07/1944 Saka.
//


import Foundation
import SwiftUI

private func format(date: Date) -> String {
    let dateFormatter = DateFormatter()
    dateFormatter.dateStyle = .medium
 
    print("coming date view")
     print(date)
   
     date.formatted(.dateTime.day().month().year())
     print("coming date formtd view")
     print( date.formatted(.dateTime.day().month().year()))
     return date.formatted(.dateTime.day().month().year())
  
}

@MainActor

final class RPMSchedulesViewModel: ObservableObject {
    @Published var schedDetls : [RPMSchedule]?
    @Published var loading = true
    @State private var currentMonthDate = Date()
    init() {
        var startDateOfMonth: String {
            let components = Calendar.current.dateComponents([.year, .month], from: currentMonthDate)
            
            let startOfMonth = Calendar.current.date(from: components)!
            return format(date: startOfMonth)
        }
        
        
        var endDateOfMonth: String {
            var components = Calendar.current.dateComponents([.year, .month], from: currentMonthDate)
            components.month = (components.month ?? 0) + 1
            components.hour = (components.hour ?? 0) - 1
            let endOfMonth = Calendar.current.date(from: components)!
            return format(date: endOfMonth)
        }
   
        let defaults = UserDefaults.standard
        guard let tkn = defaults.string(forKey: "jsonwebtoken")
              
        else {
            
            return
        }
        
        MoreManager.shared.activitySchedules(tkn: tkn, startDate:
                                                convertVitalMonthRdngsDate(inputDate: startDateOfMonth)
                              
                                             , endDate :
                                                
                                                convertVitalMonthRdngsDate(inputDate: endDateOfMonth)
                                        
        ){  (result ) in
            DispatchQueue.main.async {
            switch result {
            case .success(let account):
              
                self.schedDetls = account
                self.loading = false
                print("loading")
                print(self.loading)
             
            case .failure:
                print("getVitalSummary init failed")
            }
        }
        }
        
    }
    
 
    func scheduleDetails(startDate: String, endDate : String , completed: @escaping (String?, AlertItem?) -> Void)
    {
        
        let defaults = UserDefaults.standard
        guard let tkn = defaults.string(forKey: "jsonwebtoken")
              
        else {
            
            return
        }
     
        MoreManager.shared.activitySchedules(tkn: tkn , startDate: startDate, endDate : endDate){ [self] (result ) in
            DispatchQueue.main.async {
            switch result {
            case .success(let account):
           
                self.schedDetls = account
                self.loading = false
                print("loading")
                print(self.loading)
             
            case .failure:
                print("faileddd")
            
            }
        }
        }
        
    }
}



