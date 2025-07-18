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
    // private var imageCache = DefaultImageCache.shared
    private(set) var client: ConversationsClientWrapper = ConversationsClientWrapper()
    private let networkMonitor = NWPathMonitor()
    var deviceToken: Data?
    
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
    
    private var cancellableSet: Set<AnyCancellable> = []
    
    init(inMemory: Bool = false) {
        coreDataManager = CoreDataManager(inMemory: inMemory)
        
        super.init()
        
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
//         deregisterFromPushNotifications()
//         
//         try? ConversationsCredentialStorage.shared.deleteCredentials()

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
     
    
    func signOutChat() {
        DispatchQueue.global(qos: .userInitiated).async {
            // Step 1: Shutdown safely off the main thread
            self.client.shutdown()

            // Step 2: Wipe local cache
            self.wipeAllCache()

            //  Step 3: UI updates on the main thread
            DispatchQueue.main.async {
                self.selectedConversation = nil
                self.myIdentity = ""
                self.myUser = nil

                self.globalStatus = .signedOutSuccessfully

                DispatchQueue.main.asyncAfter(deadline: .now() + GlobalStatusView.ttl) {
                    self.clientState = .unknown
                    self.globalStatus = .none
                }
            }
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
         
           self.client.updateToken(shouldLogout: false)
        }
        if status == .completed {
            print("statuscompleted",status)
            print("STATUSclient",client)
            client.delegate = self

            conversationManager.loadAllConversations()
        }
    }
    
    
    // MARK: Conversation changes

    func conversationsClient(_ client: TwilioConversationsClient, conversationAdded conversation: TCHConversation) {
      //  NSLog("Conversation added: \(String(describing: conversation.sid)) w/ name \(String(describing: conversation.friendlyName))")
        conversation.delegate = self
      //  print("conversation.delegateApp", self)
        if let _ = PersistentConversationDataItem.from(conversation: conversation, inContext: getManagedContext()) {
            coreDataManager.saveContext()
         //   NSLog("Conversation upserted")
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
    
    func conversation(_ conversation: TCHConversation, typingStartedBy participant: TCHParticipant) {
        print("Typing started by \(participant.identity ?? "unknown") in \(conversation.sid ?? "unknown")")
        // Optional: publish to observers/UI here
        // typingPublisher.send(.startedTyping(conversation, participant))
    }

    func conversation(_ conversation: TCHConversation, typingEndedBy participant: TCHParticipant) {
        print("Typing ended by \(participant.identity ?? "unknown") in \(conversation.sid ?? "unknown")")
        // Optional: publish to observers/UI here
        // typingPublisher.send(.stoppedTyping(conversation, participant))
    }

 
    // MARK: Message changes

    func conversationsClient(_ client: TwilioConversationsClient, conversation: TCHConversation, message: TCHMessage, updated: TCHMessageUpdate) {
        let managedObjectContext = getManagedContext()
        if let _ = PersistentMessageDataItem.from(message: message, inConversation: conversation, withDirection: message.author == self.myIdentity ? .outgoing : .incoming, inContext: managedObjectContext) {
            coreDataManager.saveContext()
        }
    }

    func conversationsClient(_ client: TwilioConversationsClient, conversation: TCHConversation, messageAdded message: TCHMessage) {
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

    //  SET THE DELEGATE HERE!
    conversation.delegate = AppModel.shared
     // print(" Conversation delegate set after sync for: \(conversation.sid ?? "nil")")

      //  Log join status
      print(" Conversation join statusAPP:", conversation.status.rawValue)
    
     //Update conversation last message stats
    if let _ = PersistentConversationDataItem.from(conversation: conversation, inContext: getManagedContext()) {
        coreDataManager.saveContext()
    }
}
    
    
    // MARK: Participant changes

    func conversationsClient(_ client: TwilioConversationsClient, conversation: TCHConversation, participantJoined participant: TCHParticipant) {
        let managedObjectContext = getManagedContext()
        
        if let _ = PersistentParticipantDataItem.from(participant: participant, inConversation: conversation, inContext: managedObjectContext) {
         //   print("managedObjectContext",managedObjectContext)
            coreDataManager.saveContext()
        }

      //   Update conversation participant stats
        if let _ = PersistentConversationDataItem.from(conversation: conversation, inContext: managedObjectContext) {
          //  print("upmanagedObjectContext",managedObjectContext)
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
