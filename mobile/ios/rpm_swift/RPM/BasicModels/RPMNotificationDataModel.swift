//
//  RPMNotificationDataModel.swift
//  RPM
//
//  Created by Tesplabs on 01/04/1944 Saka.
//

import Foundation

// MARK: - Notifications
struct Notifications: Hashable, Codable ,Identifiable {

    let id = UUID()
    let totalNotifications, totalUnRead: Int?
    let data: [Datum]
    
    enum CodingKeys: String, CodingKey {
        case totalNotifications = "TotalNotifications"
        case totalUnRead = "TotalUnRead"
        case data = "Data"
    }
}

// MARK: - Datum
struct Datum: Hashable, Codable ,Identifiable{

    var id: String { notificationDate ?? UUID().uuidString } // <- stable ID
    let notificationDate: String?
    let notificationList: [NotificationList]
    
    enum CodingKeys: String, CodingKey {
        case notificationDate = "NotificationDate"
        case notificationList = "NotificationList"
    }
}

// MARK: - NotificationList
struct NotificationList: Hashable, Codable ,Identifiable {

    var id: Int { notificationID ?? -1 }

    let notificationListDescription, createdOn, type, subType: String?
    let notificationAuditID, notificationID: Int?
    let isRead, isNotify: Bool?
    
    enum CodingKeys: String, CodingKey {
        case notificationListDescription = "Description"
        case createdOn = "CreatedOn"
        case type = "Type"
        case subType = "SubType"
        case notificationAuditID = "NotificationAuditId"
        case notificationID = "NotificationId"
        case isRead = "IsRead"
        case isNotify = "IsNotify"
    }
}

// MARK: - Encode/decode helpers

class JSONNullN: Codable, Hashable {
    
    public static func == (lhs: JSONNullN, rhs: JSONNullN) -> Bool {
        return true
    }
    
    public var hashValue: Int {
        return 0
    }
    
    public init() {}
    
    public func encode(to encoder: Encoder) throws {
        var container = encoder.singleValueContainer()
        try container.encodeNil()
    }
}
