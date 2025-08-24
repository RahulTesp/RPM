//
//  PersistentConversationDataItem+CoreDataExt.swift
//  ConversationsApp
//
//  Copyright © Twilio, Inc. All rights reserved.
//

import CoreData
import TwilioConversationsClient

extension PersistentConversationDataItem {

    var title: String {
        get {
            self.friendlyName ?? self.uniqueName ?? self.sid ?? "<unknown>"
        }
    }

    var displayName: String {
            get {
                // Extract title username from the title (or any other logic you want)
                let titleUsername = self.title.components(separatedBy: "-").first ?? ""
                
                // Return the displayName, either from a method or directly
                return getMemberNameFromUsername(titleUsername) ?? titleUsername
            }
        }
    
    var unreadCount: Int64 {
          return self.unreadMessagesCount
      }
    
    func getMemberNameFromUsername(_ username: String) -> String? {
        guard let data = UserDefaults.standard.data(forKey: "savedMemberList"),
              let savedMembers = try? JSONDecoder().decode([MembersListDataModel].self, from: data) else {
            return nil
        }

        // Trim and compare for safety
        return savedMembers.first(where: {
            $0.memberUserName.trimmingCharacters(in: .whitespacesAndNewlines) ==
            username.trimmingCharacters(in: .whitespacesAndNewlines)
        })?.memberName
    }

    
    public var deliveryStatus: String {
        get {
            "checkmark"
        }
    }
    
    public var lastMessageContentIcon: String {
        get {
            if (lastMessageContentType == .image){
                return "photo.fill"
            } else if (lastMessageContentType == .file){
                return "paperclip"
            } else {
                return ""
            }
        }
    }
    
    var lastMessageContentType: MessageType {
        get {
            if (self.lastMessageType == MessageType.image.rawValue) {
                return MessageType.image
            } else if (self.lastMessageType == MessageType.file.rawValue) {
                return MessageType.file
            } else {
                return MessageType.text
            }
        }
    }
    
    var lastMessageContentAuthor: String {
        get {
            return self.lastMessageAuthor ?? ""
        }
    }
    
    enum MessageType : String {
        case text = "text"
        case image = "image"
        case file = "file"
    }
    
    static let formatter = RelativeDateTimeFormatter()

    public var lastMessageDateFormatted: String {
        get {
            guard let date = self.lastMessageDate else {
                return ""
            }

            if Calendar.current.isDateInToday(date) {
                let dateFormatter = DateFormatter()
                dateFormatter.dateStyle = .none
                dateFormatter.timeStyle = .short
                return dateFormatter.string(from: date)
            }

            return PersistentConversationDataItem.formatter.localizedString(for: date, relativeTo: Date.now) // MARK: ios 15+
        }
    }
    
    public var lastMessageTextTrimmed: String {
           return (lastMessagePreview ?? "").trimmingCharacters(in: .whitespacesAndNewlines)
       }
    
    static func from(conversation: TCHConversation, inContext context: NSManagedObjectContext) -> PersistentConversationDataItem? {
        guard let conversationSid = conversation.sid else {
            return nil
        }

        // update/import participants associated with this conversation

        conversation.participants().forEach { participant in
            if let item = PersistentParticipantDataItem.from(participant: participant, inConversation: conversation, inContext: context) {
                item.update(with: participant, withConversationSid: conversationSid, inContext: context)
            } else {
                context.performAndWait {
                    let item = PersistentParticipantDataItem.from(participant: participant, inConversation: conversation, inContext: context)
                    item?.update(with: participant, withConversationSid: conversationSid, inContext: context)
                }
            }
        }

        if let item = PersistentConversationDataItem.from(sid: conversationSid, inContext: context) {
            item.update(with: conversation, inContext: context)
            return item
        } else {
            return context.performAndWait {
                let item = PersistentConversationDataItem(context: context)
                item.update(with: conversation, inContext: context)
                return item
            }
        }
    }

    func update(with conversation: TCHConversation, inContext context: NSManagedObjectContext) {
        print(" update(with:inContext:) called for conversation.sid: \(conversation.sid ?? "nil")")
        
        context.perform {
            print(" Performing context updates for conversation \(self.sid ?? "nil")")
            self.sid = conversation.sid
            self.attributes = conversation.attributes()?.string
            self.muted = conversation.notificationLevel == .muted
            self.dateCreated = conversation.dateCreatedAsDate
            
            if let friendlyName = conversation.friendlyName {
                print(" Updating friendlyName: \(friendlyName)")
                self.friendlyName = friendlyName
            }
            if let uniqueName = conversation.uniqueName {
                print(" Updating uniqueName: \(uniqueName)")
                self.uniqueName = uniqueName
            }
            if let dateUpdated = conversation.dateUpdatedAsDate {
                print(" Updating dateUpdated: \(dateUpdated)")
                self.dateUpdated = dateUpdated
            }
            if let createdBy = conversation.createdBy {
                print(" Updating createdBy: \(createdBy)")
                self.createdBy = createdBy
            }
            if let lastMessageDate = conversation.lastMessageDate {
                print(" Updating lastMessageDate: \(lastMessageDate)")
                self.lastMessageDate = lastMessageDate
            }
            if let lastReadIndex = conversation.lastReadMessageIndex?.int64Value {
                print(" Updating lastReadMessageIndex: \(lastReadIndex)")
                self.lastReadMessageIndex = lastReadIndex
            }
        }

        DispatchQueue.global(qos: .background).async {
            print(" Fetching participants count for conversation \(self.sid ?? "nil")")
            conversation.getParticipantsCount { result, count in
                if result.isSuccessful {
                    print(" Participants count fetched: \(count)")
                    context.perform {
                        self.participantsCount = Int64(count)
                    }
                } else {
                    print(" Failed to fetch participants count")
                }
            }
            
        //new code
            print("Fetching unread messages count for conversation \(self.sid ?? "nil")")

            conversation.getLastMessages(withCount: 1) { result, messages in
                guard result.isSuccessful, let lastMessage = messages?.first else {
                    print(" Failed to fetch last message")
                    return
                }

                // Only update unread count if last message is NOT from current user
                if lastMessage.author != AppModel.shared.myIdentity {
                    conversation.getUnreadMessagesCount { result, count in
                        if result.isSuccessful, let count = count {
                            print(" Unread messages count fetched: \(count)")
                            context.perform {
                                self.unreadMessagesCount = Int64(truncating: count)
                                let totalUnread = PersistentConversationDataItem.totalUnreadCount(inContext: context)
                                print(" Posting TotalUnreadMessageCountUpdated with totalUnreadCount: \(totalUnread)")
                                NotificationCenter.default.post(
                                    name: Notification.Name("TotalUnreadMessageCountUpdated"),
                                    object: nil,
                                    userInfo: ["totalUnreadCount": totalUnread]
                                )
                            }
                        } else {
                            print(" Failed to fetch unread messages count")
                            print(" getUnreadMessagesCount result: \(result.isSuccessful), count: \(String(describing: count)), error: \(String(describing: result.error?.localizedDescription))")
                            
                            // Fallback: use total messages and lastRead index
                            conversation.getMessagesCount { result, count in
                                if result.isSuccessful {
                                    print(" (Fallback) Messages count fetched: \(count)")
                                    context.perform {
                                        self.messagesCount = Int64(count)
                                        let lastRead = conversation.lastReadMessageIndex?.intValue ?? -1
                                        let unreadCount = Int64(Int(count) - lastRead - 1)
                                        print(" (Fallback) Messages lastRead: \(lastRead), unreadCount: \(unreadCount)")
                                        
                                        if unreadCount > 0 {
                                            self.unreadMessagesCount = unreadCount
                                            let totalUnread = PersistentConversationDataItem.totalUnreadCount(inContext: context)
                                            print(" (Fallback) Posting TotalUnreadMessageCountUpdated with totalUnreadCount: \(totalUnread)")
                                            NotificationCenter.default.post(
                                                name: Notification.Name("TotalUnreadMessageCountUpdated"),
                                                object: nil,
                                                userInfo: ["totalUnreadCount": totalUnread]
                                            )
                                        }
                                    }
                                } else {
                                    print(" (Fallback) Failed to fetch messages count")
                                }
                            }
                        }
                    }
                } else {
                    print(" Last message was sent by current user, no unread count update needed")
                }
            }
        

            print(" Fetching total messages count for conversation \(self.sid ?? "nil")")
            conversation.getMessagesCount { result, count in
                if result.isSuccessful {
                    print(" Messages count fetched: \(count)")
                    context.perform {
                        self.messagesCount = Int64(count)
                    }
                } else {
                    print(" Failed to fetch messages count")
                }
            }

            print(" Fetching last message preview for conversation \(self.sid ?? "nil")")
            conversation.getLastMessages(withCount: 1) { result, messages in
                if result.isSuccessful, let messages = messages, !messages.isEmpty {
                    print("Last message fetched for preview")
                    context.perform {
                        if let lastMessage = messages.first {
                            if lastMessage.attachedMedia.count == 0 {
                                let newPreview = lastMessage.body
                                print(" Last message is text: \(newPreview)")
                                if self.lastMessagePreview != newPreview {
                                    print(" Updating lastMessagePreview")
                                    self.lastMessagePreview = newPreview
                                }
                                self.lastMessageType = MessageType.text.rawValue
                            } else if (["image/jpeg", "image/png"].contains(lastMessage.attachedMedia.first?.contentType)) {
                                print(" Last message is an image")
                                self.lastMessageType = MessageType.image.rawValue
                            } else {
                                print(" Last message is a file")
                                self.lastMessageType = MessageType.file.rawValue
                            }
                            self.lastMessageSid = lastMessage.sid
                            self.lastMessageAuthor = lastMessage.author
                            print(" Last message author: \(self.lastMessageAuthor ?? "")")
                            let _: PersistentMessageDataItem? = PersistentMessageDataItem.from(sid: lastMessage.sid!, inContext: context)
                        }
                    }
                } else {
                    print(" Failed to fetch last message preview")
                }
            }
        }
    }

    static func totalUnreadCount(inContext context: NSManagedObjectContext) -> Int64 {
        let fetchRequest: NSFetchRequest<NSFetchRequestResult> = NSFetchRequest(entityName: "PersistentConversationDataItem")
        fetchRequest.resultType = .dictionaryResultType

        let sumExpression = NSExpression(forFunction: "sum:", arguments: [NSExpression(forKeyPath: "unreadMessagesCount")])
        let expressionDescription = NSExpressionDescription()
        expressionDescription.name = "totalUnread"
        expressionDescription.expression = sumExpression
        expressionDescription.expressionResultType = .integer64AttributeType
        fetchRequest.propertiesToFetch = [expressionDescription]

        do {
            if let results = try context.fetch(fetchRequest) as? [[String: Int64]],
               let total = results.first?["totalUnread"] {
             
                         return total
            }
        } catch {
         
        }
        return 0
    }

    

    static func from(sid: String, inContext context: NSManagedObjectContext) -> PersistentConversationDataItem? {
        objc_sync_enter(self)
        defer { objc_sync_exit(self) }
       // print("PersistentConversationDataItem",PersistentConversationDataItem.self)
        let fetchRequest = PersistentConversationDataItem.fetchRequest()
        fetchRequest.predicate = NSPredicate(format: "sid = %@", sid)

        do {
         
            let result = try context.performAndWait {
                try context.fetch(fetchRequest)
            }
            return result.first
        } catch {
            return nil
        }
    }

    static func deleteConversationsUnchecked(_ conversationSids: [String], inContext context: NSManagedObjectContext) {
        if conversationSids.isEmpty {
            return
        }

        objc_sync_enter(self)
        defer { objc_sync_exit(self) }

        context.perform {
            let fetchRequest = NSFetchRequest<NSFetchRequestResult>(entityName: "PersistentConversationDataItem")
            let predicates: [NSPredicate] = conversationSids.compactMap { NSPredicate(format: "sid = %@", $0) }
            fetchRequest.predicate = NSCompoundPredicate(type: .or, subpredicates: predicates)

            let deleteRequest = NSBatchDeleteRequest(fetchRequest: fetchRequest)
            try! context.executeAndMergeChanges(using: deleteRequest)
        }
    }

    static func deleteAllUnchecked(inContext context: NSManagedObjectContext) {
        objc_sync_enter(self)
        defer { objc_sync_exit(self) }

        let fetchRequest = NSFetchRequest<NSFetchRequestResult>(entityName: "PersistentConversationDataItem")

        let deleteRequest = NSBatchDeleteRequest(fetchRequest: fetchRequest)
        try! context.executeAndMergeChanges(using: deleteRequest)
    }
}

/// For view testing
extension PersistentConversationDataItem {
    struct Decode: Decodable, Hashable {
        var sid: String
        var attributes: String?
        var muted: Bool
        var dateCreated: TimeInterval
        var dateUpdated: TimeInterval
        var friendlyName: String
        var uniqueName: String?
        var createdBy: String
        var lastMessageDate: TimeInterval
        var participantsCount: Int64
        var unreadMessagesCount: Int64
        var messagesCount: Int64
        var lastMessagePreview: String?
        
        func conversation(inContext context: NSManagedObjectContext) -> PersistentConversationDataItem {
            let item = PersistentConversationDataItem(context: context)
            item.sid = self.sid
            item.attributes = self.attributes
            item.muted = self.muted
            item.dateCreated = Date(timeIntervalSince1970: self.dateCreated)
            item.friendlyName = self.friendlyName
            item.uniqueName = self.uniqueName
            item.dateUpdated = Date(timeIntervalSince1970: self.dateUpdated)
            item.createdBy = self.createdBy
            item.lastMessageDate = Date(timeIntervalSince1970: self.lastMessageDate)
            item.participantsCount = self.participantsCount
            item.unreadMessagesCount = self.unreadMessagesCount
            item.messagesCount = self.messagesCount
            item.lastMessagePreview = self.lastMessagePreview
            context.performAndWait {
                try! context.save()
            }
            return item
        }
    }
}
