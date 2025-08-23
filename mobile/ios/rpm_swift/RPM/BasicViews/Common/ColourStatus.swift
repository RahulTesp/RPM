//
//  ColourStatus.swift
//  RPM
//
//  Created by Tesplabs on 16/11/23.
//

import Foundation
import SwiftUI

struct ColourStatus {

    // Define a function to determine the color based on systolicstatus
    static func getColorForStatus(_ status: String) -> Color {
        switch status {
        case "Critical":
            return Color.red
        case "Cautious":
            return Color.orange
        case "Normal":
            return Color.black
        default:
            return Color.black // Provide a default color for other cases, if needed
        }
    }
    static func getLineColorForStatus(_ status: String) -> Color {
        switch status {
        case "Critical":
            return Color.red
        case "Cautious":
            return Color.orange
        case "Normal":
            return Color.white
        default:
            return Color.white // Provide a default color for other cases, if needed
        }
    }
    // Define helper functions to determine colors based on the status
    static func getForegroundColor(for status: String?) -> Color {
        switch status {
        case "Active":
            return .white
        case "Enrolled":
            return .white
        case "Prescribed":
            return .white
        case "InActive":
            return .gray
        case "OnHold":
            return .black
        case "Discharged":
            return .white
        case "ReadyToDischarge":
            return .white
        default:
            return .gray
        }
    }

    static func getBackgroundColor(for status: String?) -> Color {
        switch status {
        case "Active":
            return Color("darkGreen")
        case "Enrolled":
            return Color("enrollStatus")
        case "Prescribed":
            return Color("enrollStatus")
        case "InActive":
            return Color("White")
        case "OnHold":
            return Color("White")
        case "Discharged":
            return Color("dischargeStatus")
        case "ReadyToDischarge":
            return Color("dischargeStatus")
        default:
            return Color("White")
        }
    }
}
