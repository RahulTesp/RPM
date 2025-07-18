//
//  RPMNotificationCountDataModel.swift
//  RPM
//
//  Created by Tesplabs on 10/05/1944 Saka.
//


import Foundation

// MARK: - Welcome
struct NotificationCount: Codable {
    let totalNotifications, totalUnRead: Int?
    
    enum CodingKeys: String, CodingKey {
        case totalNotifications = "TotalNotifications"
        case totalUnRead = "TotalUnRead"
    }
}
