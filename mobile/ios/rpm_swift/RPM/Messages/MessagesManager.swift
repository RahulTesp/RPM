//
//  MessagesManager.swift
//  ConversationsApp
//
//  Created by Cece Laitano on 4/6/22.
//  Copyright © 2022 Twilio, Inc. All rights reserved.
//

import Foundation
import TwilioConversationsClient
import Combine

class MessagesManager: ObservableObject {
    
    @Published var messages = [PersistentMessageDataItem]()
    
    private var coreDataDelegate: CoreDataDelegate
    private var conversationManager: ConversationManager
    private var cancellableSet: Set<AnyCancellable> = []
    
    // Track messages that are being deleted
    @Published var deletingMessages: Set<String> = [] // or use message SID instead of messageIndex

    
    init(coreDataDelegate: CoreDataDelegate, conversationManager: ConversationManager ) {
        self.coreDataDelegate = coreDataDelegate
        self.conversationManager = conversationManager
    }
    
    
    func subscribeMessages(inConversation conversation: PersistentConversationDataItem) {
        AppModel.shared.selectedConversation = conversation
        NSLog("Setting up Core Data update subscription for Messages in conversation \(conversation.sid ?? "<unknown>")")
      
        if let conversationSid = conversation.sid {
            let request = PersistentMessageDataItem.fetchRequest()
            request.predicate = NSPredicate(format: "conversationSid = %@", conversationSid)
            request.sortDescriptors = [NSSortDescriptor(key: "messageIndex", ascending: true)]
            
            ObservableResultPublisher(with: request, context: coreDataDelegate.managedObjectContext)
                .sink(
                    receiveCompletion: {
                        NSLog("Completion from fetch messages - \($0)")
                    },
                    receiveValue: { [weak self] items in
                        self?.messages = items
                        print("messagesloaded",items)
                    })
                .store(in: &cancellableSet)
        }
    }
    
    func preloadMessagesIfNeeded(for conversation: PersistentConversationDataItem) {
        guard let conversationSid = conversation.sid else { return }

        let request = PersistentMessageDataItem.fetchRequest()
        request.predicate = NSPredicate(format: "conversationSid = %@", conversationSid)
        request.sortDescriptors = [NSSortDescriptor(key: "messageIndex", ascending: true)]

        do {
            let results = try coreDataDelegate.managedObjectContext.fetch(request)
            print("Preloaded messages count: \(results.count)")
        } catch {
            print("Error preloading messages: \(error)")
        }
    }

 
    func reset() {
        // Clear any in-memory arrays or caches
     
        self.messages = []
    }
    
    func loadLastMessagePageIn(_ conversationItem: PersistentConversationDataItem, max: UInt) {
        
        print(" loadLastMessagePageIn called: ONAPPEAR= , max=\(max)")
        guard let sid = conversationItem.sid else {
            return
        }
        
        conversationManager.retrieveConversation(sid) { (conversation, error) in
            conversation?.getLastMessages(withCount: max) { (result, messages) in
                if let _ = messages?.map({ PersistentMessageDataItem.from(message: $0, inConversation: conversation!, withDirection: $0.author == AppModel.shared.myIdentity ? .outgoing : .incoming, inContext: self.coreDataDelegate.managedObjectContext)
                
                    print(" loadLastMessagePageInMessage author: \($0.author), myIdentity: \(AppModel.shared.myIdentity)")
                    
                }) {
                    self.coreDataDelegate.saveContext()
                }
            }
        }
    }
    
    func loadMessages(for conversationItem: PersistentConversationDataItem, before messageIndex: Int64, max: UInt) {
        print("loadMessages called: before=\(messageIndex), max=\(max)")

        guard let sid = conversationItem.sid else {
            print(" No SID in conversationItem")
            return
        }

        conversationManager.retrieveConversation(sid) { (conversation, error) in
            guard let conversation = conversation else {
                print(" retrieveConversation failed with error: \(String(describing: error))")
                return
            }

            let adjustedIndex = messageIndex > 0 ? messageIndex - 1 : 0
            
            print(" adjustedIndex = \(adjustedIndex)")

            
            print("Successfully retrieved conversation, fetching messages before index \(adjustedIndex)...")

            conversation.getMessagesBefore(UInt(adjustedIndex), withCount: max + 1) { (result, messages) in
                print(" getMessagesBefore callback started")

                if let error = result.error {
                    print(" getMessagesBefore failed with error: \(error.localizedDescription)")
                } else {
                    print(" getMessagesBefore success")
                }

                guard let messages = messages else {
                    print(" messages is nil in getMessagesBefore")
                    return
                }

                print(" getMessagesBefore returned \(messages.count) messages")

                let persistedMessages = messages.map {
                    PersistentMessageDataItem.from(
                        message: $0,
                        inConversation: conversation,
                        withDirection: $0.author == AppModel.shared.myIdentity ? .outgoing : .incoming,
                        inContext: self.coreDataDelegate.managedObjectContext
                    )
                }

                print(" Persisted \(persistedMessages.count) messages")

                if !persistedMessages.isEmpty {
                    self.coreDataDelegate.saveContext()
                }
            }
        }
    }

    func updateMessage(attributes: [String: Any]?, for messageIndex: Int64?, conversationSid: String?) {
        guard let conversationSid = conversationSid,
              let attributes = attributes,
              let messageIndex = messageIndex else {
            return
        }
        conversationManager.retrieveConversation(conversationSid) { tchConversation, error in
            tchConversation?.message(withIndex:  NSNumber(integerLiteral: Int(messageIndex)), completion: { result, tchMessage in
                guard let messageToUpdate = tchMessage,
                      let jsonAttributes = TCHJsonAttributes(dictionary: attributes) else {
                    return
                }
                messageToUpdate.setAttributes(jsonAttributes) { result in
                    if result.error != nil {
                        //  print("Updating message attributes returned an error: \(String(describing: result.error)) - error code: \(result.resultCode)")
                    } else {
                        //  print("Message attributes updated successfully!")
                    }
                }
            })
        }
    }
    

    private func retrieveMessageIn(_ conversation: TCHConversation, messageIndex: NSNumber, completion: @escaping (TCHMessage?, Error?) -> Void) {
        print("[retrieveMessageIn] ➤ Called with index: \(messageIndex)")
        print("[retrieveMessageIn]  Conversation sync status: \(conversation.synchronizationStatus.rawValue)")

        guard conversation.synchronizationStatus == .all else {
            print("[retrieveMessageIn]  Conversation not fully synced yet")
            completion(nil, DataFetchError.conversationsClientIsNotAvailable)
            return
        }

        conversation.getMessagesCount { result, count in
            if !result.isSuccessful {
                print("[retrieveMessageIn]  Failed to get messages count")
                completion(nil, result.error)
                return
            }

            if messageIndex.intValue >= count {
                print("[retrieveMessageIn]  Message index \(messageIndex) is out of range (count: \(count))")
                completion(nil, DataFetchError.messageIndexOutOfBounds)
                return
            }

            conversation.message(withIndex: messageIndex) { result, message in
                print("[retrieveMessageIn]  Callback triggered")

                if let message = message, result.isSuccessful {
                    print("[retrieveMessageIn]  Message retrieved at index \(messageIndex)")
                    completion(message, nil)
                } else {
                    print("[retrieveMessageIn]  Failed to retrieve message: \(String(describing: result.error))")
                    completion(nil, result.error)
                }
            }
        }
    }

    
    func copyMessage() {
        conversationManager.conversationEventPublisher.send(.messageCopied)
    }
    
    func isDeleting(_ message: PersistentMessageDataItem) -> Bool {
        guard let sid = message.sid else { return false }
        return deletingMessages.contains(sid)
    }


    func deleteMessageFromTwilio(_ persistentMessage: PersistentMessageDataItem, completion: @escaping (Bool) -> Void) {
        print("[deleteMessage] ➤ Method called")
        
        guard let conversationSid = persistentMessage.conversationSid else {
            print("[deleteMessage]  conversationSid is nil – cannot continue")
            completion(false)
            return
        }
        print("[deleteMessage]  conversationSid: \(conversationSid)")
        
        guard let messageSid = persistentMessage.sid else {
            print("[deleteMessage]  message SID is nil")
            completion(false)
            return
        }
        print("[deleteMessage]  messageSid: \(messageSid)")
        
        let messageIndex = persistentMessage.messageIndex
        print("[deleteMessage]  messageIndex: \(messageIndex)")
        
        conversationManager.retrieveConversation(conversationSid) { [weak self] conversation, error in
            print("[deleteMessage]  retrieveConversation callback triggered")
            
            guard let self = self else {
                print("[deleteMessage]  Self is nil")
                completion(false)
                return
            }
            
            if let error = error {
                print("[deleteMessage]  Failed to retrieve conversation: \(error.localizedDescription)")
                completion(false)
                return
            }
            
            guard let conversation = conversation else {
                print("[deleteMessage]  Conversation is nil")
                completion(false)
                return
            }
            print("[deleteMessage]  Successfully retrieved conversation")
            
            print("[deleteMessage]  Conversation sync status: \(conversation.synchronizationStatus.rawValue)")
            
            print("Messages count: \(conversation.messagesCount)")
            
            if conversation.synchronizationStatus == .all {
                print("[deleteMessage]  conversation synchronizationStatus DONE")
            
            } else {
                print("[deleteMessage]  Conversation not fully synced yet — can't delete message directly by index.")
                completion(false)
            }

         
            let index = NSNumber(value: persistentMessage.messageIndex)
            
            print("[deleteMessage] withIndex: \(index)")
            
            conversation.message(withIndex: index) { result, message in
                print("[deleteMessage] result: \(result.isSuccessful), message: \(String(describing: message?.sid))")
                guard result.isSuccessful, let message = message else {
                    print("[deleteMessage]  Failed to retrieve message at index \(index): \(String(describing: result.error))")
                    completion(false)
                    return
                }
               
                conversation.remove(message) { result in
                    if result.isSuccessful {
                        print("[deleteMessage]  Message deleted successfully")
                        DispatchQueue.main.async {
                            self.conversationManager.conversationEventPublisher.send(.messageDeleted)
                            completion(true)
                        }
                    } else {
                        print("[deleteMessage]  Deletion failed: \(String(describing: result.error))")
                        completion(false)
                    }
                }
            }
         
        }
    }

    func sendMessage(toConversation conversationItem: PersistentConversationDataItem,
                     withText text: String?,
                     andMedia url: NSURL?,
                     withFileName filename: String?,
                     completion: @escaping (TCHError?) -> ()) {
        
        guard let sid = conversationItem.sid else { return }
        
        //  Step 1: Insert a temporary message for immediate UI feedback
        guard let text = text, !text.trimmingCharacters(in: .whitespacesAndNewlines).isEmpty else {
            return
        }

        // Step 1: Create temp message in Core Data
        let context = coreDataDelegate.managedObjectContext
        
        let tempMessage = PersistentMessageDataItem.Decode(
            sid: UUID().uuidString,
            uuid: UUID(),
            index: -1,
            direction: Int16(MessageDirection.outgoing.rawValue),
            author: AppModel.shared.myIdentity,
            body: text,
            dateCreated: Date().timeIntervalSince1970,
            dateUpdated: Date().timeIntervalSince1970,
            attachedMedia: nil,
            attributes: nil,
            status: MessageStatus.sending.rawValue

        ).message(inContext: coreDataDelegate.managedObjectContext)

        AppModel.shared.messagesManager.messages.append(tempMessage)

        //  Step 2: Send message via Twilio
        conversationManager.retrieveConversation(sid) { (conversation, error) in
            guard let conversation = conversation, error == nil else {
                print("Conversation retrieval failed: \(String(describing: error))")
                tempMessage.status = MessageStatus.failed.rawValue // mark failed
                return
            }

            if let mediaURL = url {
                conversation.prepareMessage()
                    .addMedia(
                        inputStream: InputStream(url: mediaURL as URL)!,
                        contentType: "image/jpeg",
                        filename: filename,
                        listener: .init(onStarted: {}, onProgress: { _ in }, onCompleted: { _ in }, onFailed: { _ in })
                    )
                    .buildAndSend { result, _ in
                        if result.isSuccessful {
                            tempMessage.status = MessageStatus.sent.rawValue
                        } else {
                            tempMessage.status = MessageStatus.failed.rawValue
                        }
                        
                        completion(result.error)
                    }
            } else {
                conversation.prepareMessage()
                    .setBody(text)
                    .buildAndSend { result, _ in
                        if result.isSuccessful {
                            tempMessage.status = MessageStatus.sent.rawValue
                        } else {
                            tempMessage.status = MessageStatus.failed.rawValue
                        }
                        
                        completion(result.error)
                    }
            }
        }
    }

 
    func setAllMessagesRead(for conversationSid: String?) {
        guard let conversationSid = conversationSid else {
            return
        }
        conversationManager.retrieveConversation(conversationSid) { (conversation, error) in
            guard let conversation = conversation, error == nil else {
                return
            }
            conversation.setAllMessagesReadWithCompletion { result, updatedUnreadMessageCount  in
                if result.isSuccessful {
                    //  NSLog("All messages set as read for conversation \(conversationSid)")
                } else {
                    // NSLog("Error - not able to set all messages as read for conversation \(conversationSid)")
                }
            }
        }
    }
    
    func updateMessageBody(newText: String, for messageIndex: Int64?, conversationSid: String?) {
        guard let conversationSid = conversationSid,
              let messageIndex = messageIndex else {
            return
        }
        
        conversationManager.retrieveConversation(conversationSid) { tchConversation, error in
            tchConversation?.message(withIndex: NSNumber(value: messageIndex)) { result, message in
                guard let messageToUpdate = message else {
                    //  print("Failed to retrieve message")
                    return
                }
                
                messageToUpdate.updateBody(newText) { result in
                    if result.isSuccessful {
                           print(" Message body updated successfully")
                    } else {
                         print(" Failed to update message body: \(String(describing: result.error))")
                    }
                }
            }
        }
    }
    
}
