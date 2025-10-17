//
//  TimeAgoUtils.swift
//  RPM
//
//  Created by Tesplabs on 08/11/23.
//

import SwiftUI
   

struct TimeAgoUtils {
    static func calculateTimeAgo(localDateString: String) -> String {
        print("localDateString: \(localDateString)")
        //  Parse the input UTC string
        let inputDateFormatter = DateFormatter()
        inputDateFormatter.dateFormat = "yyyy-MM-dd'T'HH:mm:ss"
        inputDateFormatter.timeZone = TimeZone(identifier: "UTC")
        guard let utcDate = inputDateFormatter.date(from: localDateString) else {
            print(" Failed to parse the input date string.")
            return "Invalid Date"
        }
        //  No manual offset addition — utcDate already represents the exact moment in time
        let localDate = utcDate
        print(" localDate (auto-converted in print): \(localDate)")
        //  Current date — as is
        let currentDate = Date()
        print(" currentDate: \(currentDate)")
        //  Optional debug print (formatted local time)
        let debugFormatter = DateFormatter()
        debugFormatter.dateFormat = "yyyy-MM-dd HH:mm:ss"
        debugFormatter.timeZone = TimeZone.current
        print(" formatted localDate: \(debugFormatter.string(from: localDate))")
        print(" formatted currentDate: \(debugFormatter.string(from: currentDate))")
        //  Calculate difference
        let calendar = Calendar.current
        let timeDifference = calendar.dateComponents(
            [.second, .minute, .hour, .day, .weekOfYear, .month],
            from: localDate,
            to: currentDate
        )
        let secondsDifference = timeDifference.second ?? 0
        let minutesDifference = timeDifference.minute ?? 0
        let hoursDifference = timeDifference.hour ?? 0
        let daysDifference = timeDifference.day ?? 0
        let weeksDifference = timeDifference.weekOfYear ?? 0
        let monthsDifference = timeDifference.month ?? 0
        print("""
        secondsDifference: \(secondsDifference)
        minutesDifference: \(minutesDifference)
        hoursDifference: \(hoursDifference)
        daysDifference: \(daysDifference)
        """)
        //  Return human-readable string
        if monthsDifference > 0 {
            return "\(monthsDifference) \(monthsDifference == 1 ? "month" : "months") ago"
        } else if weeksDifference > 0 {
            return "\(weeksDifference) \(weeksDifference == 1 ? "week" : "weeks") ago"
        } else if daysDifference == 0 && hoursDifference > 0 {
            let roundedHours = minutesDifference > 30 ? hoursDifference + 1 : hoursDifference
            return "\(roundedHours) \(roundedHours == 1 ? "hour" : "hours") ago"
        } else if daysDifference == 0 && hoursDifference == 0 && minutesDifference > 0 {
            return "\(minutesDifference) \(minutesDifference == 1 ? "minute" : "minutes") ago"
        } else if daysDifference == 0 && hoursDifference == 0 && minutesDifference == 0 && secondsDifference > 0 {
            return "\(secondsDifference) \(secondsDifference == 1 ? "second" : "seconds") ago"
        } else if daysDifference < 7 && daysDifference >= 1 {
            let dayOfWeekSymbol = calendar.weekdaySymbols[calendar.component(.weekday, from: localDate) - 1]
            print(" local day: \(dayOfWeekSymbol)")
            return dayOfWeekSymbol
        } else if daysDifference >= 7 && daysDifference <= 14 {
            return "1 week ago"
        } else if daysDifference >= 14 && daysDifference <= 21 {
            return "2 weeks ago"
        } else if daysDifference >= 21 && daysDifference <= 28 {
            return "3 weeks ago"
        } else {
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
