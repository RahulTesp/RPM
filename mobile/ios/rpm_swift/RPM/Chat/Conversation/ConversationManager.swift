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
    @Published var hasLoadedConversationsOnce = false

    var viewModelCache: [String: ConversationItemViewModel] = [:]

    // MARK: Events
    
    var conversationEventPublisher = PassthroughSubject<ConversationEvent, Never>()
    
    init(_ client: ConversationsClientWrapper, coreDataDelegate: CoreDataDelegate ) {
        self.client = client
        self.coreDataDelegate = coreDataDelegate
        
        NotificationCenter.default.publisher(for: Notification.Name("TotalUnreadMessageCountUpdated"))
            .sink { [weak self] notification in
                if let userInfo = notification.userInfo {
                    if let count = userInfo["totalUnreadCount"] as? Int64 {
                        self?.unreadCount = count
                    } else if let number = userInfo["totalUnreadCount"] as? NSNumber {
                        let count = number.int64Value
                        self?.unreadCount = count
                    } else {
                      
                    }
                }
            }
            .store(in: &cancellableSet)
        
    }

    deinit {
        NotificationCenter.default.removeObserver(self)
    }
    
    func reset() {
        // Clear any in-memory arrays or caches
        self.conversations = []
        unreadCount = 0
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
               
                    self?.conversations = sortedItems
                    self?.hasLoadedConversationsOnce = true
                 
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
        guard let client = client.conversationsClient else { return }

        client.myConversations()?.forEach { conversation in
            AppModel.shared.assignDelegateIfNeeded(for: conversation)

            if conversation.synchronizationStatus == .all {
                if conversation.status == .notParticipating {
                    print(" Not participating, attempting to join: \(conversation.sid ?? "nil")")
                    conversation.join { result in
                        if result.isSuccessful {
                            print(" Successfully joined: \(conversation.sid ?? "nil")")
                            self.ensureAllParticipantsExplicit(in: conversation)
                            
                            // Delay update to Core Data slightly to wait for participants to be added explicitly
                            DispatchQueue.main.asyncAfter(deadline: .now() + 1.0) {
                                DispatchQueue.main.asyncAfter(deadline: .now() + 1.0) {
                                    if let _ = PersistentConversationDataItem.from(conversation: conversation, inContext: self.coreDataDelegate.managedObjectContext) {
                                        self.coreDataDelegate.saveContext()
                                    }
                                }

                            }
                        } else {
                            print(" Failed to join conversation \(conversation.sid ?? "nil"): \(result.error?.localizedDescription ?? "unknown")")
                        }
                    }
                } else {
                    print(" Already joined: \(conversation.sid ?? "nil")")
                    self.ensureAllParticipantsExplicit(in: conversation)

                    // Similarly delay Core Data update slightly
                    DispatchQueue.main.asyncAfter(deadline: .now() + 1.0) {
                        DispatchQueue.main.asyncAfter(deadline: .now() + 1.0) {
                            if let _ = PersistentConversationDataItem.from(conversation: conversation, inContext: self.coreDataDelegate.managedObjectContext) {
                                self.coreDataDelegate.saveContext()
                            }
                        }

                    }
                }
            }
        }

        coreDataDelegate.saveContext()

        DispatchQueue.main.async {
            self.isConversationsLoading = false
        }
    }

    func ensureAllParticipantsExplicit(in conversation: TCHConversation) {
        let existingParticipants = conversation.participants()
        let currentIdentities = existingParticipants.compactMap { $0.identity }

        guard !currentIdentities.isEmpty else {
            print(" No participants found in conversation SID: \(conversation.sid ?? "unknown")")
            return
        }

        let attributes = TCHJsonAttributes(dictionary: [:])

        for identity in currentIdentities {
            conversation.addParticipant(byIdentity: identity, attributes: attributes) { result in
                if result.isSuccessful {
                    print(" Re-added participant explicitly: \(identity)")
                } else {
                    print(" Could not add \(identity): \(result.error?.localizedDescription ?? "possibly already added")")
                }
            }
        }
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
            
    
            for participant in conversation.participants() {
             
            }
        
            if isParticipant {
                // Already joined
             
                AppModel.shared.assignDelegateIfNeeded(for: conversation)

                print("conversation.delegate === AppModel.shared?4", conversation.delegate === AppModel.shared)

                print(" 2retrieveConversationDelegate set for conversation SID: \(conversation.sid ?? "nil")")
         
                completion(conversation, nil)
            } else {
                // Not participant - join now
                conversation.join { result in
                    if let error = result.error {
                     
                        completion(nil, error)
                    } else {
                 
                        AppModel.shared.assignDelegateIfNeeded(for: conversation)

                        print("conversation.delegate === AppModel.shared?5", conversation.delegate === AppModel.shared)

                        print(" 3retrieveConversationDelegate set for conversation SID: \(conversation.sid ?? "nil")")
                        
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
                  
                } else {
                   
                }
            })
        } else {
            
        }
    }
    
    
    func createAndJoinConversation(friendlyName: String?, completion: @escaping (Result<String,Error>) -> Void) {
       
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
              
                    AppModel.shared.assignDelegateIfNeeded(for: conversation)

                    print("conversation.delegate === AppModel.shared?6", conversation.delegate === AppModel.shared)

                    print(" 4retrieveConversationDelegate set for conversation SID: \(conversation.sid ?? "nil")")
                    
                    completion(.success(conversation.sid ?? ""))
                }
            }
        }
    }
    
    func viewModel(for conversation: PersistentConversationDataItem, context: NSManagedObjectContext) -> ConversationItemViewModel {
        if let sid = conversation.sid {
            if let existing = viewModelCache[sid] {
                return existing
            } else {
                let vm = ConversationItemViewModel(model: conversation, context: context)
                viewModelCache[sid] = vm
                return vm
            }
        } else {
            return ConversationItemViewModel(model: conversation, context: context)
        }
    }
}
