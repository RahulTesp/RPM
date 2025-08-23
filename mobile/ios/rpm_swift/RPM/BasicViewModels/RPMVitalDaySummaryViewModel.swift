
//  RPMVitalSummaryViewModel.swift
//  RPM
//
//  Created by Tesplabs on 15/07/1944 Saka.
//


import Foundation
import SwiftUI


private func format(date: Date) -> String {
    let dateFormatter = DateFormatter()
    dateFormatter.dateStyle = .medium
    
    print("coming date model")
     print(date)

    return dateFormatter.string(from: date)

}


@MainActor
final class RPMVitalDaySummaryViewModel: ObservableObject {
    
    @Published var vitalSummary : VitalReadingsDataModel?
    @Published var loading = true
    @State private var currentMonthDate = Date()
    @State var currentDateValue : String = Date.getCurrentDate()
    
    init() {
    
    }
    
    
    func defaultVitalSumm()
    {
        
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
        
        DashboardManager.shared.getVitalSummaryDefault(tkn: tkn
                                                 
        ){  (result ) in
            DispatchQueue.main.async {
            switch result {
            case .success(let account):
                
                self.vitalSummary = account
                self.loading = false
                print("loading")
                print("getVitalSummaryDefault",account)
                print(self.loading)
            
            case .failure(let error):
           
                self.loading = false
       
                    print("Unhandled error: \(error)")
             
                if error == .unauthorized {
                    SessionManager.shared.handleUnauthorizedResponse()
                }
                
                print("getVitalSummary init failed")
                
            }
        }
        }
   
    }
    

    
    func getVitalSummary(startDate: String, endDate : String , completed: @escaping (String?, AlertItem?) -> Void)
    {
        print("getvitalstartDate", startDate)
     
        print("getvitalendDate",endDate)
     
        let defaults = UserDefaults.standard
        guard let tkn = defaults.string(forKey: "jsonwebtoken")
             
        else {
            
            return
        }
        
        DashboardManager.shared.getVitalSummaryList(tkn: tkn,
                                                    startDate:  startDate,
                                                    endDate :  endDate)
        {  (result ) in
            DispatchQueue.main.async {
            switch result {
            case .success(let account):
         
                self.vitalSummary = account
                self.loading = false
                print("loading")
                print("getVitalSummaryListdata",account)
                print(self.loading)
            
            case .failure(let error):
             
                self.loading = false
          
                    print("Unhandled error: \(error)")
              
                print("getVitalSummary faileddd")
                
            }
        }
        }
    }
}
