//
//  ConversationVM.swift
//  ConversationsApp
//
//  Copyright © Twilio, Inc. All rights reserved.
//

import UIKit
import SwiftUI
import TwilioConversationsClient

final class MessageListViewModel: ObservableObject, Identifiable {
    
    var conversation: Conversation? = nil
    @Published var tchConversation: TCHConversation? = nil

    // MARK: Observable
    
    // Image picker
    @Published var isPresentingImagePicker = false
    @Published var selectedImage: UIImage?
    @Published var selectedImageURL: NSURL?
    @Published var isPresentingImagePreview = false
    @Published var selectedFileName: String?
 
    // Typing
    @Published private var typingParticipants = Set<Participant>()
    
    // ConversationEvents
    @Published var currentConversationEvent: ConversationEvent? = nil
    
    //Read/Unread messages
    @Published var readMessages = [PersistentMessageDataItem]()
    @Published var unreadReceivedMessages = [PersistentMessageDataItem]()
    @Published var unreadSection = Array<Messages>()
    @Published var unreadSentMessages = [PersistentMessageDataItem]()
    
    var typingText: String {
        if typingParticipants.count > 1 {
            let participantsTypingText = NSLocalizedString("conversation.participants.typing", comment: "")
            return "\(typingParticipants.count) \(participantsTypingText)"
        } else if typingParticipants.count == 1,
                  let participant = typingParticipants.first,
                  let identity = participant.identity {
            let participantTypingText = NSLocalizedString("conversation.participant.typing", comment: "")
            return "\(identity) \(participantTypingText)"
        }
        return ""
    }
//    var isAnyParticipantTyping: Bool {
//        return typingParticipants.count > 0
//    }
    
    
    var isAnyParticipantTyping: Bool {
        let isTyping = typingParticipants.count > 0
        print(" isAnyParticipantTyping = \(isTyping) (count = \(typingParticipants.count))")
        return isTyping
    }

    func updateUserActivity(conversationSid: String, userName: String, lastActiveAt: String) {
        print("Calling updateUserActivity for SID: \(conversationSid), user: \(userName), lastActiveAt: \(lastActiveAt)")
        let now = Date()
        DashboardManager.shared.chatHeartbeat(
            convSID: conversationSid,
            toUser: userName,
            lastActiveAt: now
        ) { result in
            DispatchQueue.main.async {
                switch result {
                case .success(let responseCode):
                    print(" Chat heartbeat successfully with status code: \(responseCode)")
                case .failure(let error):
                    print(" Failed to update chat heartbeat: \(error.localizedDescription)")
                }
            }
        }
    }


    func notifyMessageSent(conversationSid: String, toUser: String, fromUser: String, message: String) {
        DashboardManager.shared.sendMessageNotification(
            convSID: conversationSid,
            toUser: toUser,
            fromUser: fromUser,
            message: message
        ) { result in
            DispatchQueue.main.async {
                switch result {
                case .success(let statusCode):
                    print(" Message notification sent. Status code: \(statusCode)")
                case .failure(let error):
                    print(" Failed to send message notification: \(error.localizedDescription)")
                }
            }
        }
    }


    func sendMessageNotification(conversationSid: String, toUser: String, fromUser: String, message: String) {
        let payload: [String: Any] = [
            "ConversationSid": conversationSid,
            "ToUser": toUser,
            "FromUser": fromUser,
            "Message": message
        ]

        guard let url = URL(string: "https://your-api-url.com/api/comm/sendmessage") else {
            print(" Invalid URL")
            return
        }

        var request = URLRequest(url: url)
        request.httpMethod = "POST"
        request.setValue("application/json", forHTTPHeaderField: "Content-Type")

        do {
            request.httpBody = try JSONSerialization.data(withJSONObject: payload, options: [])
        } catch {
            print("Failed to encode body: \(error)")
            return
        }

        URLSession.shared.dataTask(with: request) { data, response, error in
            if let error = error {
                print(" Error sending notification: \(error)")
                return
            }

            if let httpResponse = response as? HTTPURLResponse {
                print(" Message notification sent, status code: \(httpResponse.statusCode)")
            }
        }.resume()
    }

    
    func didSelectImage(_ image: UIImage?, _ url: NSURL?, _ fileName: String?) {
        selectedImage = image
        selectedImageURL = url
        selectedFileName = fileName
        isPresentingImagePicker = false
        
        DispatchQueue.main.asyncAfter(deadline: .now() + 0.1) {
            self.isPresentingImagePreview = true
        }
    }
    
    func clearSelectedImage() {
        selectedImage = nil
        selectedImageURL = nil
        selectedFileName = nil
    }
    
    // adapted from: https://stackoverflow.com/a/45277557
    func getAttachmentFileSize() -> String {
        var result = NSLocalizedString("message.attachment.fallback_size", comment: "Placeholder string used as a fallback in case we cannot get the file size of the attachment")
        
        guard let url = selectedImageURL, let selectedImagePath = url.path else {
            return result
        }
        
        do {
            let attributes = try FileManager.default.attributesOfItem(atPath: selectedImagePath)
            
            if let fileSize = attributes[FileAttributeKey.size] as? NSNumber {
                let formatter = ByteCountFormatter()
                formatter.countStyle = ByteCountFormatter.CountStyle.file
                result = formatter.string(fromByteCount: Int64(truncating: fileSize))
            }
        } catch {
            print("Could not retrieve size of attachment: \(error)")
        }
        
        return result
    }
    
    func getAttachmentFileExtension() -> String {
        guard let url = selectedImageURL, let fileExtension = url.pathExtension else {
            return ""
        }
        
        return fileExtension.uppercased()
    }
    
    func loadTCHConversation(using manager: ConversationManager) {
        guard let sid = conversation?.sid else { return }
        manager.retrieveConversation(sid) { [weak self] conversation, error in
            DispatchQueue.main.async {
                if let conversation = conversation {
                    self?.tchConversation = conversation
                } else {
                    print(" Could not load TCHConversation: \(error?.localizedDescription ?? "Unknown error")")
                }
            }
        }
    }

    
    // MARK: Typing
    func setConversation(_ conversation: Conversation) {
        self.conversation = conversation
    }
    
    // MARK: Conversation Events
    func registerForConversationEvents(_ event: ConversationEvent) {
        DispatchQueue.main.async {
            self.currentConversationEvent = event
            
            DispatchQueue.main.asyncAfter(deadline: .now() + GlobalStatusView.ttl) {
                self.currentConversationEvent = nil
            }
        }
    }
    
    // MARK: - Unread messages section
    func prepareMessages(_ conversation: PersistentConversationDataItem, _ messages: [PersistentMessageDataItem], _ participants: [PersistentParticipantDataItem]) {
        let lastReadMessageIndex = participants.filter({ participant in
            return participant.identity == AppModel.shared.myIdentity
        }).first?.lastReadMessage
        
        readMessages = messages.filter( { message in
            if message.conversationSid == conversation.sid {
                return message.messageIndex <= lastReadMessageIndex ?? 0
            } else {
                return false
            }
        })
        
        unreadReceivedMessages = messages.filter({ message in
            if (message.conversationSid == conversation.sid && message.direction == MessageDirection.incoming.rawValue) {
                return message.messageIndex > lastReadMessageIndex ?? 0
            } else {
                return false
            }
        })
        
        unreadSection = [Messages(messages: unreadReceivedMessages)]
        
        unreadSentMessages = messages.filter({ message in
            if (message.conversationSid == conversation.sid && message.direction == MessageDirection.outgoing.rawValue) {
                return message.messageIndex > lastReadMessageIndex ?? 0
            } else {
                return false
            }
        })
    }
}
