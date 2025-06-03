//
//  ConversationsManager.swift
//  ConversationsApp
//
//  Created by Cece Laitano on 4/4/22.
//  Copyright © 2022 Twilio, Inc. All rights reserved.
//

import Foundation
import TwilioConversationsClient
import CoreData
import Combine

enum ConversationEvent {
    case leftConversation
    case messageCopied
    case messageDeleted
    case notificationsTurnedOn
    case notificationsTurnedOff
    case participantAdded
    case participantRemoved
}

class ConversationManager: ObservableObject {
    
    @Published var conversations = [PersistentConversationDataItem]()
    @Published var isConversationsLoading = false
    @Published var isConversationsRefreshing = false
    
    private var client: ConversationsClientWrapper = ConversationsClientWrapper()
    private var cancellableSet: Set<AnyCancellable> = []
    private var coreDataDelegate: CoreDataDelegate
    
    @Published var totalUnreadCount: Int = 0
    @Published var unreadCount: Int64 = 0
    
    // MARK: Events
    
    var conversationEventPublisher = PassthroughSubject<ConversationEvent, Never>()
    
    init(_ client: ConversationsClientWrapper, coreDataDelegate: CoreDataDelegate ) {
        self.client = client
        self.coreDataDelegate = coreDataDelegate
        
        NotificationCenter.default.publisher(for: Notification.Name("TotalUnreadMessageCountUpdated"))
            .sink { [weak self] notification in
                if let userInfo = notification.userInfo {
                    if let count = userInfo["totalUnreadCount"] as? Int64 {
                        // print(" ConversationManager received Int64 count: \(count)")
                        self?.unreadCount = count
                    } else if let number = userInfo["totalUnreadCount"] as? NSNumber {
                        let count = number.int64Value
                        // print(" ConversationManager received NSNumber count: \(count)")
                        self?.unreadCount = count
                    } else {
                        // print(" ConversationManager received notification but count not found or wrong type")
                    }
                }
            }
            .store(in: &cancellableSet)
        
    }
    
    
    
    deinit {
        NotificationCenter.default.removeObserver(self)
    }
    
    func subscribeConversations(onRefresh: Bool) {
        
        if (onRefresh) {
            isConversationsRefreshing = true
        } else {
            isConversationsLoading = true
        }
        NSLog("Setting up Core Data update subscription for Conversations")
        
        let request = PersistentConversationDataItem.fetchRequest()
        
        request.sortDescriptors = [
            NSSortDescriptor(key: "lastMessageDate", ascending: false),
            NSSortDescriptor(key: "friendlyName", ascending: true)]
        
        ObservableResultPublisher(with: request, context: coreDataDelegate.managedObjectContext)
            .sink(
                receiveCompletion: {
                    NSLog("Completion from fetch conversations - \($0)")
                    print("Completion","Completion")
                },
                receiveValue: { [weak self] items in
                    let sortedItems = items.sorted(by: self!.sorterForConversations)
                    //   print("sortedItems",sortedItems)
                    self?.conversations = sortedItems
                    //  print("self?.conversations",self?.conversations)
                    if (onRefresh) {
                        self?.isConversationsRefreshing = false
                    } else {
                        self?.isConversationsLoading = false
                    }
                })
            .store(in: &cancellableSet)
    }
    
    func fetchConversationDataItem(by chatSid: String) -> PersistentConversationDataItem? {
        let fetchRequest: NSFetchRequest<PersistentConversationDataItem> = PersistentConversationDataItem.fetchRequest()
        fetchRequest.predicate = NSPredicate(format: "sid == %@", chatSid)
        fetchRequest.fetchLimit = 1
        
        do {
            let results = try coreDataDelegate.managedObjectContext.fetch(fetchRequest)
            return results.first
        } catch {
            //  print("Error fetching conversation: \(error.localizedDescription)")
            return nil
        }
    }
    
    
    func sorterForConversations(this:PersistentConversationDataItem, that:PersistentConversationDataItem) -> Bool {
        // Some conversations have null values so excluding from sorting
        if (this.dateCreated == nil){
            return false
        }
        if (that.dateCreated == nil){
            return true
        }
        
        let thisDate = this.lastMessageDate == nil ? this.dateCreated : this.lastMessageDate
        let thatDate = that.lastMessageDate == nil ? that.dateCreated : that.lastMessageDate
        
        return thisDate! > thatDate!
    }
    
    
    func loadAllConversations() {
        //  print("conversationsClient", client.conversationsClient)
        guard let client = client.conversationsClient else {
            //  print("loadAllclient", client)
            return
        }
        
        
        // Assign delegate to each conversation here
        client.myConversations()?.forEach { conversation in
            //    conversation.delegate = self
            
            DispatchQueue.main.async {
                self.isConversationsLoading = false
            }
            
            //  self.isConversationsLoading = false
            
            conversation.delegate = AppModel.shared
            //  print("conversation.delegate", conversation.delegate)
            // Save or update your persistent data as before
            _ = PersistentConversationDataItem.from(conversation: conversation, inContext: coreDataDelegate.managedObjectContext)
        }
        
        coreDataDelegate.saveContext()
    }
    
    
    func retrieveConversation(_ conversationSid: String, completion: @escaping (TCHConversation?, Error?) -> Void) {
        print("retrieveConversation")
        
        guard let client = client.conversationsClient else {
            completion(nil, DataFetchError.conversationsClientIsNotAvailable)
            return
        }
        
        
        client.conversation(withSidOrUniqueName: conversationSid) { (result, conversation) in
            guard result.isSuccessful, let conversation = conversation else {
                completion(nil, DataFetchError.requiredDataCallsFailed)
                return
            }
            
            // Check if current user is participant
            let currentUserIdentity = client.user?.identity
            
            let isParticipant = conversation.participants().contains(where: { participant in
                return participant.identity == currentUserIdentity
            })
            
            //  print("Participants count:", conversation.participants().count)
            for participant in conversation.participants() {
                //     print("Participant identity: \(participant.identity ?? "unknown")")
            }
            
            
            
            if isParticipant {
                // Already joined
                conversation.delegate = AppModel.shared
                //  print(" Already joined. Delegate set.")
                completion(conversation, nil)
            } else {
                // Not participant - join now
                conversation.join { result in
                    if let error = result.error {
                        //   print("error join.", error)
                        completion(nil, error)
                    } else {
                        conversation.delegate = AppModel.shared
                        // print(" Successfully joined. Delegate set.")
                        
                        //  Add remote participant if not already added
                        //    self.addMissingParticipantIfNeeded(conversation: conversation, identity: "johnwick")
                        
                        completion(conversation, nil)
                    }
                }
                
            }
            
            
            
        }
        
    }
    
    
    func addMissingParticipantIfNeeded(conversation: TCHConversation, identity: String) {
        let alreadyPresent = conversation.participants().contains { $0.identity == identity }
        
        if !alreadyPresent {
            let attributes = TCHJsonAttributes(dictionary: [:]) // Empty attributes
            
            conversation.addParticipant(byIdentity: identity, attributes: attributes, completion: { result in
                if result.isSuccessful {
                    //   print(" \(identity) added.")
                } else {
                    // print(" Failed to add \(identity): \(result.error?.localizedDescription ?? "unknown error")")
                }
            })
        } else {
            // print("\(identity) is already a participant.")
        }
    }
    
    
    func createAndJoinConversation(friendlyName: String?, completion: @escaping (Result<String,Error>) -> Void) {
        //  print("createAndJoinConversation",friendlyName)
        let creationOptions: [String: Any] = [
            TCHConversationOptionFriendlyName: friendlyName ?? "",
        ]
        
        guard let client = client.conversationsClient else {
            completion(.failure(DataFetchError.conversationsClientIsNotAvailable))
            return
        }
        
        client.createConversation(options: creationOptions) { result, conversation in
            guard let conversation = conversation else {
                completion(.failure(result.error ?? DataFetchError.requiredDataCallsFailed))
                return
            }
            
            conversation.join { result in
                if let error = result.error {
                    completion(.failure(error))
                } else {
                    // Set delegate AFTER successful join
                    conversation.delegate = AppModel.shared
                    //   print(" Delegate set after join:", conversation.delegate ?? "nil")
                    
                    completion(.success(conversation.sid ?? ""))
                }
            }
        }
    }
}
