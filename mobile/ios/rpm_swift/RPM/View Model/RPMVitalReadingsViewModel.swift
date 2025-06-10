//
//  RPMVitalReadingsViewModel.swift
//  RPM
//
//  Created by Tesplabs on 24/09/1944 Saka.
//


import Foundation
import SwiftUI

func strtodtt(timeval: String) -> String
{
    print("timeval")
    print(timeval)
   
    let dateFormatterr = DateFormatter()
    dateFormatterr.locale = Locale.init(identifier: "en_GB")
    dateFormatterr.dateFormat = "MMM d, yyyy"
    print("cnvrtd")
    print(dateFormatterr.date(from: timeval))
    
    let dateObj = dateFormatterr.date(from: timeval)
    
    dateFormatterr.dateFormat = "yyyy-MM-dd"
   
    let dateObjs = dateFormatterr.string(from: dateObj ?? Date())
    
    print("final")
    print(dateFormatterr.string(from: dateObj ?? Date()))
   
    return
    dateFormatterr.string(from: dateObj ?? Date())
   
}



private func format(date: Date) -> String {
    let dateFormatter = DateFormatter()
    dateFormatter.dateStyle = .medium
    
   print("coming date")
    print(date)

    date.formatted(.dateTime.day().month().year())
    print("coming date formtd")
    print( date.formatted(.dateTime.day().month().year()))
    return date.formatted(.dateTime.day().month().year())
}

func convertVitalMonthRdngsDate(inputDate: String) -> String {
print("inputDatemonth",inputDate)
   
     let olDateFormatter = DateFormatter()
    
#if targetEnvironment(simulator)
    print("simulator")
    


let currentTimeZone = TimeZone.current.identifier
print("Current Time Zone: \(currentTimeZone)")

// Set correct input date format
olDateFormatter.dateFormat = "d MMM yyyy"  // Matches input like "1 May 2025"
olDateFormatter.locale = Locale(identifier: "en_US_POSIX") // Ensure consistent parsing across devices

if let oldDate = olDateFormatter.date(from: inputDate) {
    let convertDateFormatter = DateFormatter()
    convertDateFormatter.dateFormat = "yyyy-MM-dd"
    let monthdt1 = convertDateFormatter.string(from: oldDate)
    print("simumonthdt1", monthdt1)
    return monthdt1
} else {
    print("Failed to parse date: \(inputDate)")
    return "Invalid Date"
}

 
#else
    // Device
    print("Device")
    
    // Determine the current time zone
    let currentTimeZone = TimeZone.current.identifier
    print("Current Time Zone: \(currentTimeZone)")

    if currentTimeZone == "America/Phoenix" {
        // If the current time zone is Arizona, use the "MMM d, yyyy" format
        print("Arizona Time Zone")
        olDateFormatter.dateFormat = "MMM d, yyyy"
    } else {
        // For other time zones, use the "d MMM yyyy" format
        print("Other Time Zone")
        olDateFormatter.dateFormat = "d MMM yyyy"
    }

    
   // olDateFormatter.dateFormat = "d MMM yyyy"

    let oldDate = olDateFormatter.date(from: inputDate)

    let convertDateFormatter = DateFormatter()
    convertDateFormatter.dateFormat = "yyyy-MM-dd"
   var monthdt1 = convertDateFormatter.string(from: oldDate!)
    print("monthdt1",monthdt1)
    return monthdt1
    
#endif
    
}


@MainActor

final class RPMVitalReadingsViewModel: ObservableObject {
   
    @Published var vitalReadings : VitalReadingsDataModel?
    @Published var loading = true
    @State private var currentMonthDate = Date()
    @Published var showNoInternetAlert = false
    
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
        print("startDateOfMonth",startDateOfMonth)
        print("endDateOfMonth",endDateOfMonth)
        DashboardManager.shared.getVitalReadingsList(tkn: tkn, startDate:
                                                        convertVitalMonthRdngsDate(inputDate: startDateOfMonth)
                                                     
                                                     , endDate :
                                                        convertVitalMonthRdngsDate(inputDate: endDateOfMonth)
                                                      
        ){  (result ) in
            DispatchQueue.main.async {
            switch result {
            case .success(let account):
                
                self.vitalReadings = account
                self.loading = false
                
                print("monthrdngsloading",account)
                print(self.loading)
              
            case .failure(let error):
          
                self.loading = false
                switch error {
                case .noInternet:
                    self.showNoInternetAlert = true
                default:
                
                    print("Unhandled error: \(error)")
                }
                print("monthrdngs init failed")
                
            }
        }
        }
        
    }
    

    func getVitalReadings(startDate: String, endDate : String , completed: @escaping (String?, AlertItem?) -> Void)
    {
        print("getVitalReadings startDate",startDate)
        print("getVitalReadings endDate",endDate)
  
        let defaults = UserDefaults.standard
        guard let tkn = defaults.string(forKey: "jsonwebtoken")
            
        else {
            
            return
        }
        
        DashboardManager.shared.getVitalReadingsList(tkn: tkn,
                                                     startDate:  startDate,
                                                     endDate :  endDate)
        {  (result ) in
            DispatchQueue.main.async {
            switch result {
            case .success(let account):
                print("getVitalReadingsList",account)
       
                self.vitalReadings = account
                self.loading = false
                print("loading")
                print(self.loading)
              
            case .failure:
           
                print("getVitalSummary faileddd")
                
            }
        }
        }
    }
    
}



func localToUTCt(dateStr: String) -> String? {
    let dateFormatter = DateFormatter()
    dateFormatter.dateFormat = "yyyy-MM-dd"
    dateFormatter.calendar = Calendar.current
    dateFormatter.timeZone = TimeZone.current
    
    if let date = dateFormatter.date(from: dateStr) {
        dateFormatter.timeZone = TimeZone(abbreviation: "UTC")
        dateFormatter.dateFormat = "yyyy-MM-dd'T'HH:mm:ss"
        
        return dateFormatter.string(from: date)
    }
    return nil
}
