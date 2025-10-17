//
//  AppModel.swift
//  ConversationsApp
//
//  Created by Berkus Karchebnyy on 17.11.2021.
//  Copyright © 2021 Twilio, Inc. All rights reserved.
//

import Combine
import CoreData
import Network
import SwiftUI
import TwilioConversationsClient

///
/// AppModel is a repository of application's globally available data and operations.
/// See https://github.com/shufflingB/swiftui-core-data-with-model/ for details about AppModel.
///
///

class AppModel: NSObject, ObservableObject {
    static let shared = AppModel() // used only for AppDelegate to notify the client of Push Notification device token changes
    
    // CoreData objects are directly exposed and are then mapped into ViewModels for particular view as needed.
    @Published var selectedConversation: PersistentConversationDataItem? = nil
    @Published var myIdentity = ""
    @Published var myUser: TCHUser?
    @Published var globalStatus: GlobalStatus = .none
    @Published var conversationsError: TCHError? = nil
    
    private var clientState: TCHClientConnectionState = .unknown
    private(set) var client: ConversationsClientWrapper = ConversationsClientWrapper()
    private let networkMonitor = NWPathMonitor()
    var deviceToken: Data?
    
    @Published var isClientReady = false

    
    var coreDataManager: CoreDataManager!
    var conversationManager: ConversationManager!
    var messagesManager: MessagesManager!
    var participantsManager: ParticipantsManager!
    // MARK: Global Status
    
    enum GlobalStatus {
        case none
        case noConnectivity
        case signedOutSuccessfully
    }
    
    // MARK: Typing

    enum TypingActivity {
        case startedTyping(Conversation, Participant)
        case stoppedTyping(Conversation, Participant)
    }

    public var typingPublisher = PassthroughSubject<TypingActivity, Never>()

    
    private var cancellableSet: Set<AnyCancellable> = []
    let id = UUID()
    
    init(inMemory: Bool = false) {
        coreDataManager = CoreDataManager(inMemory: inMemory)
        
        super.init()
        print("AppModel.shared init called with id: \(id)")
        print(" AppModel.shared created")
        
        conversationManager = ConversationManager(client, coreDataDelegate: coreDataManager)
        conversationManager.subscribeConversations(onRefresh: false)
        messagesManager = MessagesManager(coreDataDelegate: coreDataManager, conversationManager: conversationManager)
        participantsManager = ParticipantsManager(coreDataDelegate: coreDataManager, conversationManager: conversationManager)
        
        // subscribe to changes regarding the user's network connectivity
        
        networkMonitor.pathUpdateHandler = { path in
            if path.status == .satisfied {
                self.globalStatus = .none
            } else {
                self.globalStatus = .noConnectivity
            }
        }
        
        networkMonitor.start(queue: .main)
    }
    
    deinit {
        print(" AppModel.shared deinitialized!")
    }

    func getManagedContext() -> NSManagedObjectContext {
        return coreDataManager.managedObjectContext
    }
    // MARK: Client
   
    func saveUser(_ user: TCHUser?) {
        DispatchQueue.main.async {
            self.myUser = user
            if let identity = user?.identity {
                self.myIdentity = identity
            }
            print("myuseridentity", self.myIdentity)
        }
    }

  
    func signOut() {

         DispatchQueue.main.async {
             self.globalStatus = .signedOutSuccessfully
             DispatchQueue.main.asyncAfter(deadline: .now() + GlobalStatusView.ttl) {
                 self.clientState = .unknown
                 self.globalStatus = .none
             }
             self.client.shutdown()
         }
         
         self.wipeAllCache()
     }
     
    func refreshUnreadIfReady() {
        print("Refreshing unread count")
          if isClientReady {
              print("Refreshing isClientReady")
              conversationManager.refreshUnreadCount()
          } else {
              print("Client not ready yet, will refresh when ready")
              $isClientReady
                  .filter { $0 }
                  .first()
                  .sink { [weak self] _ in
                      print("Client became ready, refreshing unread count")
                      self?.conversationManager.refreshUnreadCount()
                  }
                  .store(in: &cancellableSet)
          }
      }
    
    func signOutChat() {
        DispatchQueue.global(qos: .userInitiated).async {
            // Step 1: Shutdown safely off the main thread
            print("[ChatSignOut] Shutting down client…")
            self.client.shutdown()
            print("[ChatSignOut] Client shutdown complete")

            // Step 2: Wipe local cache
            self.wipeAllCache()
            print("[ChatSignOut] Local cache wiped")

            //  Step 3: UI updates on the main thread
            DispatchQueue.main.async {
                self.conversationManager.reset()
                print("[ChatSignOut] Conversation manager reset")

                self.selectedConversation = nil
                self.myIdentity = ""
                self.myUser = nil
                print("[ChatSignOut] User identity & state cleared")

                self.globalStatus = .signedOutSuccessfully
                print("[ChatSignOut] Global status set to signedOutSuccessfully")

                DispatchQueue.main.asyncAfter(deadline: .now() + GlobalStatusView.ttl) {
                    self.clientState = .unknown
                    self.globalStatus = .none
                    print("[ChatSignOut] Client state reset to unknown, global status cleared")
                }
            }
        }
    }


    func assignDelegateIfNeeded(for conversation: TCHConversation) {
        print("Delegate set for SID: \(conversation.sid ?? "nil")")

        if conversation.delegate !== AppModel.shared {
            conversation.delegate = AppModel.shared
            print(" Delegate set for conversation SID: \(conversation.sid ?? "nil")")
        } else {
            print(" Delegate already set for conversation SID: \(conversation.sid ?? "nil")")
        }
    }

    
    func wipeAllCache() {
        print("wipeAllCache")
        let managedObjectContext = getManagedContext()
        PersistentConversationDataItem.deleteAllUnchecked(inContext: managedObjectContext)
        PersistentMessageDataItem.deleteAllUnchecked(inContext: managedObjectContext)
        PersistentParticipantDataItem.deleteAllUnchecked(inContext: managedObjectContext)
    }
    
    
    // MARK: Local Typing
    
    func typing(in item: PersistentConversationDataItem?) {
        guard let convo = item, let conversationSid = convo.sid else {
            print("No valid conversation SID found.")
            return
        }
        print("Sending typing indicator for SID: \(conversationSid)")
        conversationManager.retrieveConversation(conversationSid) { (conversation, error) in
            if let conversation = conversation {
                print("Got conversation, sending typing...")
                conversation.typing()
            } else {
                print("Failed to retrieve conversation: \(String(describing: error))")
            }
        }
    }
}


// MARK: Client delegate methods
extension AppModel: TwilioConversationsClientDelegate {

    // MARK: Client changes
    
    func conversationsClient(_ client: TwilioConversationsClient, connectionStateUpdated state: TCHClientConnectionState) {
        print("conversationsClientstate",state)
        self.clientState = state
        
    }
    func conversationsClientTokenWillExpire(_ client: TwilioConversationsClient) {
        print("conversationsClientTokenWillExpire",client)
        print("conversationsClientTokenWillExpire1",self.client)
        self.client.conversationsClientTokenWillExpire(client)
    }
    
    func conversationsClientTokenExpired(_ client: TwilioConversationsClient) {
        print("conversationsClientTokenExpired",client)
        print("conversationsClientTokenExpired1",self.client)
        self.client.conversationsClientTokenExpired(client)
    }
 
    
    func conversationsClient(_ client: TwilioConversationsClient, conversationsError errorReceived: TCHError) {
        DispatchQueue.main.async {
            print("errorReceived",errorReceived)
            self.conversationsError = errorReceived
        }
    }
    
  
    func conversationsClient(_ client: TwilioConversationsClient, synchronizationStatusUpdated status: TCHClientSynchronizationStatus) {
        print("syncstatus",status)
        print("synclient1\n",client)
        print("synclient2\n",client.self)
        print("synclient3\n",client.description)
        print("synclient4\n",client.user)
        print("self.client",self.client)
        print("self DELEGATE VAL",self)
        if status == .failed {
            print("syncstatusfailed",status)
            DispatchQueue.main.async {
                       self.isClientReady = false
                   }
           self.client.updateToken(shouldLogout: false)
        }
        if status == .completed {
            print("statuscompleted",status)
            print("STATUSclient",client)
            client.delegate = self
            DispatchQueue.main.async {
                       self.isClientReady = true
                   }
            TwilioConversationsClient.setLogLevel(.debug)

            conversationManager.loadAllConversations()
          
        }
        
        if let client = AppModel.shared.client.conversationsClient {
            print(" Twilio connection state: \(client.connectionState.rawValue)")
        } else {
            print(" Twilio client not initialized")
        }
    }
    
    
    // MARK: Conversation changes

    func conversationsClient(_ client: TwilioConversationsClient, conversationAdded conversation: TCHConversation) {
  
        assignDelegateIfNeeded(for: conversation)
        
        print("conversation.delegate === AppModel.shared?1", conversation.delegate === AppModel.shared)

        if let _ = PersistentConversationDataItem.from(conversation: conversation, inContext: getManagedContext()) {
            coreDataManager.saveContext()
        }
    }
    

    // MARK: User changes

    func conversationsClient(_ client: TwilioConversationsClient, user: TCHUser, updated update: TCHUserUpdate) {
        if user.identity == myIdentity {
            myUser = user
        }
    }
}

// MARK: TCHConversationDelegate methods

extension AppModel: TCHConversationDelegate {

    
    // MARK: Conversation changes

    func conversationsClient(_ client: TwilioConversationsClient, conversation: TCHConversation, updated update: TCHConversationUpdate) {
        if let _ = PersistentConversationDataItem.from(conversation: conversation, inContext: getManagedContext()) {
            coreDataManager.saveContext()
        }
    }
    
    func conversation(_ conversation: TCHConversation, participantStartedTyping participant: TCHParticipant) {
         print(" participantStartedTyping: \(participant.identity ?? "unknown")")
  
     }

     func conversation(_ conversation: TCHConversation, participantStoppedTyping participant: TCHParticipant) {
         print(" participantStoppedTyping: \(participant.identity ?? "unknown")")
         // Optionally handle
     }
    

    // MARK: Message changes

    func conversationsClient(_ client: TwilioConversationsClient, conversation: TCHConversation, message: TCHMessage, updated: TCHMessageUpdate) {
        let managedObjectContext = getManagedContext()
        if let _ = PersistentMessageDataItem.from(message: message, inConversation: conversation, withDirection: message.author == self.myIdentity ? .outgoing : .incoming, inContext: managedObjectContext) {
            coreDataManager.saveContext()
        }
    }

    func conversationsClient(_ client: TwilioConversationsClient, conversation: TCHConversation, messageAdded message: TCHMessage) {
        print("messageadded")
        guard conversation.sid != nil else {
            return
        }
                        
        let managedObjectContext = getManagedContext()

        if let _ = PersistentMessageDataItem.from(message: message, inConversation: conversation, withDirection: message.author == myIdentity ? .outgoing : .incoming, inContext: managedObjectContext) {
            coreDataManager.saveContext()
        }

        // Update conversation last message stats
        if let _ = PersistentConversationDataItem.from(conversation: conversation, inContext: managedObjectContext) {
            coreDataManager.saveContext()
        }
    }

    func conversationsClient(_ client: TwilioConversationsClient, conversation: TCHConversation, messageDeleted message: TCHMessage) {
        print("message deleted callback")
        guard let messageSid = message.sid else {
            return
        }
        
        let managedObjectContext = getManagedContext()
        PersistentMessageDataItem.deleteMessagesUnchecked([messageSid], inContext: managedObjectContext)

        // Update conversation last message stats
        if let _ = PersistentConversationDataItem.from(conversation: conversation, inContext: managedObjectContext) {
            coreDataManager.saveContext()
        }
    }

// MARK: Client changes
    
    func conversationsClient(_ client: TwilioConversationsClient,
                             conversation: TCHConversation,
                             synchronizationStatusUpdated status: TCHConversationSynchronizationStatus) {
        guard conversation.synchronizationStatus.rawValue >= TCHConversationSynchronizationStatus.all.rawValue else {
            return
        }
        
        
        print(" Already joinedsynchronizationStatus: ",conversation.synchronizationStatus.rawValue)
        
        if status == .all {
            self.assignDelegateIfNeeded(for: conversation)

            if conversation.status == .notParticipating {
                print(" Not participating, attempting to join: \(conversation.sid ?? "nil")")
                conversation.join { result in
                    if result.isSuccessful {
                        print(" Joined conversation: \(conversation.sid ?? "nil")")
                    } else {
                        print(" Failed to join conversation: \(result.error?.localizedDescription ?? "unknown")")
                    }
                }
            } else {
                print(" Already joined: \(conversation.sid ?? "nil")")
            }
        }


        print("conversation.delegate === AppModel.shared?2", conversation.delegate === AppModel.shared)

        print(" Conversation join statusAPP:", conversation.status.rawValue)
        conversation.participants().forEach {
            print("Participants - \($0.identity ?? "unknown")")
        }

        if let _ = PersistentConversationDataItem.from(conversation: conversation, inContext: getManagedContext()) {
            coreDataManager.saveContext()
        }
    }

    
    
    // MARK: Participant changes

    func conversationsClient(_ client: TwilioConversationsClient, conversation: TCHConversation, participantJoined participant: TCHParticipant) {
        let managedObjectContext = getManagedContext()
        
        if let _ = PersistentParticipantDataItem.from(participant: participant, inConversation: conversation, inContext: managedObjectContext) {
       
            coreDataManager.saveContext()
        }

      //   Update conversation participant stats
        if let _ = PersistentConversationDataItem.from(conversation: conversation, inContext: managedObjectContext) {
     
            coreDataManager.saveContext()
        }
    }
    func conversationsClient(_ client: TwilioConversationsClient, conversation: TCHConversation, participant: TCHParticipant, updated: TCHParticipantUpdate) {
        let managedObjectContext = getManagedContext()
        
        if let _ = PersistentParticipantDataItem.from(participant: participant, inConversation: conversation, inContext: managedObjectContext) {
            coreDataManager.saveContext()
        }
        
        // Update conversation participant stats
        if let _ = PersistentConversationDataItem.from(conversation: conversation, inContext: managedObjectContext) {
            coreDataManager.saveContext()
        }
    }
    
    func conversationsClient(_ client: TwilioConversationsClient, conversation: TCHConversation, participantLeft participant: TCHParticipant) {
        guard let conversationSid = conversation.sid,
              let participantSid = participant.sid else {
                  return
              }
        
        let managedObjectContext = getManagedContext()
        PersistentParticipantDataItem.deleteParticipants([participantSid], inContext: managedObjectContext)
        
        if participant.identity == myIdentity {
            PersistentConversationDataItem.deleteConversationsUnchecked([conversationSid], inContext: managedObjectContext)
        } else {
            // Update conversation participant stats
            if let _ = PersistentConversationDataItem.from(conversation: conversation, inContext: managedObjectContext) {
                coreDataManager.saveContext()
            }
        }
    }
    
}
