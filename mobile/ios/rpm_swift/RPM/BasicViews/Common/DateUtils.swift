//
//  DateUtils.swift
//  RPM
//
//  Created by Tesplabs on 08/11/23.
//

import Foundation

struct DateUtils {
 
    static func convertToUTC(localDateStr: String, inputFormatStr: String, outputFormatStr: String) -> String? {
        let inputFormatter = DateFormatter()
        inputFormatter.dateFormat = inputFormatStr
        inputFormatter.timeZone = TimeZone.current

        guard let localDate = inputFormatter.date(from: localDateStr) else {
            return nil
        }

        let outputFormatter = DateFormatter()
        outputFormatter.dateFormat = outputFormatStr
        outputFormatter.timeZone = TimeZone(identifier: "UTC")

        let utcDate = outputFormatter.string(from: localDate)
        print("convutcdate", utcDate)
        return utcDate
    }


    static func convertUtcToLocalFormatted(utcDateStr: String, outputFormatStr: String) -> String? {
        let inputFormatter = DateFormatter()
        inputFormatter.dateFormat = "yyyy-MM-dd'T'HH:mm:ss"
        inputFormatter.timeZone = TimeZone(abbreviation: "UTC")
        
        if let utcDate = inputFormatter.date(from: utcDateStr) {
            let outputFormatter = DateFormatter()
            outputFormatter.dateFormat = outputFormatStr
            outputFormatter.locale = Locale.current
            outputFormatter.timeZone = TimeZone.current
            return outputFormatter.string(from: utcDate)
        }
        return nil
    }

    static func convertUtcToLocalTimeDiff(utcDateStr: String, outputFormatStr: String) -> String? {
        print("utcDateStr:\(utcDateStr)")
        print("outputFormatStr:\(outputFormatStr)")
        let inputFormatter = DateFormatter()
        inputFormatter.dateFormat = "yyyy-MM-dd'T'HH:mm:ss"
        inputFormatter.timeZone = TimeZone(abbreviation: "UTC")
        
        if let utcDate = inputFormatter.date(from: utcDateStr) {
            let outputFormatter = DateFormatter()
            outputFormatter.dateFormat = outputFormatStr
            outputFormatter.locale = Locale.current
            outputFormatter.timeZone = TimeZone.current
            print("utcDate:\(utcDate)")
            // Format the date and exclude the time zone offset
            var formattedDate = outputFormatter.string(from: utcDate)
            if let indexOfPlus = formattedDate.firstIndex(of: "+") {
                formattedDate.removeSubrange(indexOfPlus..<formattedDate.endIndex)
            }
            print("formattedDate:\(formattedDate)")
            return formattedDate
        }
        
        return nil
    }

    static func formatDate(inputDateStr: String, outputFormat: String) -> String? {
        let inputDateFormat = DateFormatter()
        inputDateFormat.dateFormat = "yyyy-MM-dd'T'HH:mm:ss"
        
        let outputDateFormat = DateFormatter()
        outputDateFormat.dateFormat = outputFormat
        
        if let date = inputDateFormat.date(from: inputDateStr) {
            return outputDateFormat.string(from: date)
        }
        return nil
    }
}
