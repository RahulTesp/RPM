//
//  TimeAgoUtils.swift
//  RPM
//
//  Created by Tesplabs on 08/11/23.
//

import SwiftUI

struct TimeAgoUtils {
    static func calculateTimeAgo(localDateString: String) -> String {
        
        // First, create a DateFormatter for parsing the input string
        let inputDateFormatter = DateFormatter()
        inputDateFormatter.dateFormat = "yyyy-MM-dd'T'HH:mm:ss"
        inputDateFormatter.timeZone = TimeZone(identifier: "UTC")
        
        guard let utcDate = inputDateFormatter.date(from: localDateString) else {
            print("Failed to parse the input date string.")
            return "Invalid Date"
        }
        
        // Convert UTC date to local time zone
        let localDate = utcDate.addingTimeInterval(TimeInterval(TimeZone.current.secondsFromGMT(for: utcDate)))
        
        print("localDate: \(localDate)")
      
        
        // Set the calendar and time zone to the system's local time
        let calendar = Calendar.current
        
        // Ensure the current date is obtained in the local time zone
        let currentDate = Date().addingTimeInterval(TimeInterval(TimeZone.current.secondsFromGMT()))
        
        let dateFormatterNW = DateFormatter()
        dateFormatterNW.dateFormat = "yyyy-MM-dd HH:mm:ss"
        dateFormatterNW.timeZone = TimeZone.current  // Set the desired time zone
        let formattedDate = dateFormatterNW.string(from: currentDate)
        
        print("Current Date: \(currentDate)")
        print("formattedCDate: \(formattedDate)")
        
        // Calculate the time difference in seconds
     
        let timeDifference = calendar.dateComponents([.second, .minute, .hour, .day], from: localDate, to: currentDate)
        print("Time Difference: \(timeDifference)")
        
        
        let secondsDifference = timeDifference.second ?? 0
        let minutesDifference = timeDifference.minute ?? 0
        let hoursDifference = timeDifference.hour ?? 0
        let daysDifference = timeDifference.day ?? 0
        
        let weeksDifference = timeDifference.weekOfYear ?? 0
              let monthsDifference = timeDifference.month ?? 0
        
        print("secondsDifference: \(secondsDifference)")
        print("minutesDifference: \(minutesDifference)")
        print("hoursDifference: \(hoursDifference)")
        print("daysDifference: \(daysDifference)")
        
        // Prioritize larger time units
        
        if monthsDifference > 0 {
                  return "\(monthsDifference) \(monthsDifference == 1 ? "month" : "months") ago"
              } else if weeksDifference > 0 {
                  return "\(weeksDifference) \(weeksDifference == 1 ? "week" : "weeks") ago"
              }
        
        
        
        else if daysDifference == 0 && hoursDifference > 0 && hoursDifference < 24{
            
            let roundedHours: Int
               if minutesDifference > 30 {
                   // If the minute difference is greater than 30, round up the hours
                   roundedHours = hoursDifference + 1
               } else {
                   roundedHours = hoursDifference
               }
               
               return "\(roundedHours) \(roundedHours == 1 ? "hour" : "hours") ago"
            
            
            
             
           } else if daysDifference == 0 && hoursDifference == 0 && minutesDifference > 0 && minutesDifference < 60{
               return "\(minutesDifference) \(minutesDifference == 1 ? "minute" : "minutes") ago"
           }
           
           else if daysDifference == 0 && hoursDifference == 0 && minutesDifference == 0 && secondsDifference > 0 && secondsDifference < 60 {
               return "\(secondsDifference) \(secondsDifference == 1 ? "second" : "seconds") ago"
           }
           
        
        else if daysDifference < 7 && daysDifference >= 1 {
            let localDayOfWeek = calendar.component(.weekday, from: localDate)
            let correctedLocalDayOfWeek = (localDayOfWeek + 5) % 7 // Adjust to start from Monday
            let dayOfWeekSymbol = calendar.weekdaySymbols[correctedLocalDayOfWeek]
            print("localDayOfWeek: \(localDayOfWeek), correctedLocalDayOfWeek: \(correctedLocalDayOfWeek), dayOfWeekSymbol: \(dayOfWeekSymbol)")
            return dayOfWeekSymbol
        }


        else if daysDifference  >= 7 && daysDifference <= 14 {
            return "1 week ago"
        } else if daysDifference  >= 14 && daysDifference  <= 21 {
            return "2 weeks ago"
        } else if daysDifference  >= 21 && daysDifference <= 28 {
            return "3 weeks ago"
        }
  
              else {
                    let yearDiff = calendar.component(.year, from: currentDate) - calendar.component(.year, from: localDate)
                    let monthDiff = calendar.component(.month, from: currentDate) - calendar.component(.month, from: localDate)

                    if yearDiff > 0 || monthDiff > 1 {
                        return "\(monthDiff) months ago"
                    } else if yearDiff == 0 && monthDiff == 1 {
                        return "1 month ago"
                    }
                }
            
     
        return "Some default text"
    }
}

