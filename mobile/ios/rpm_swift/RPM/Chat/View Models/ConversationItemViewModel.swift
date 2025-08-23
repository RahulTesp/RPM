//
//  ConversationItemViewModel.swift
//  RPM
//
//  Created by Tesplabs on 30/06/25.
//

import Combine
import CoreData


class ConversationItemViewModel: ObservableObject {
    @Published var unreadCount: Int64

    private var model: PersistentConversationDataItem
    private var context: NSManagedObjectContext

    init(model: PersistentConversationDataItem, context: NSManagedObjectContext) {
        self.model = model
        self.context = context
        self.unreadCount = model.unreadMessagesCount
        observeCoreDataUpdates(context: context)
    }

    func markAllMessagesRead() {
        // Optimistically update UI first
        self.unreadCount = 0

        context.perform {
            self.model.unreadMessagesCount = 0
            do {
                try self.context.save()
            } catch {
                print("Failed to save unread count: \(error)")
            }
        }
    }

    func syncUnreadCountFromCoreData() {
        context.perform {
            let actual = self.model.unreadMessagesCount
            DispatchQueue.main.async {
                self.unreadCount = actual
            }
        }
    }
    
    private func observeCoreDataUpdates(context: NSManagedObjectContext) {
         NotificationCenter.default.addObserver(
             forName: Notification.Name("TotalUnreadMessageCountUpdated"),
             object: nil,
             queue: .main
         ) { [weak self] _ in
             self?.unreadCount = Int64(Int(self?.model.unreadMessagesCount ?? 0))
         }
     }
    deinit {
        NotificationCenter.default.removeObserver(self)
    }
}
