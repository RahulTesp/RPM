//
//  MessageBubbleViewModel.swift
//  ConversationsApp
//
//  Created by Berkus Karchebnyy on 07.10.2021.
//  Copyright © 2021 Twilio, Inc. All rights reserved.
//

import Foundation
import SwiftUI
import TwilioConversationsClient

// MARK: - Media

enum MessageType {
    case text, image, file
}

enum MediaAttachmentState {
    case notDownloaded, downloading, downloaded
}

struct MediaUploadStatus {
    let totalBytes: Int
    let bytesUploaded: Int
    let url: URL?

    var percentage: Double {
        if totalBytes == 0 {
            return 0
        }
        let pct = round(Double(bytesUploaded) / Double(totalBytes) * 100)
        return pct
    }

    static func from(mediaProperties: MediaMessageProperties) ->  MediaUploadStatus {
        return MediaUploadStatus(
            totalBytes: mediaProperties.messageSize ,
            bytesUploaded: mediaProperties.uploadedSize,
            url: mediaProperties.mediaURL
        )
    }
    
    static func invalid() -> MediaUploadStatus {
        return MediaUploadStatus(totalBytes: 0, bytesUploaded: 0, url: nil)
    }
}

struct ReactionDetailModel: Identifiable {
    
    let id = UUID()
    let identity: String
}

struct Reaction: Identifiable, Hashable {
    var id = UUID()
    var reaction: String
    var count: Int
}

class ReactionsViewModel: ObservableObject {
    @Published var reactions: [Reaction]
    
    init(reactions:  [Reaction]) {
        self.reactions = reactions
    }
}


// MARK: - Message View Model

final class MessageBubbleViewModel: ObservableObject, Identifiable, Hashable, Equatable {
    @Published var source: PersistentMessageDataItem
    var currentUser: String
    @Published var attachmentState: MediaAttachmentState = .notDownloaded
    @Published var image: UIImage?

    @Published var isEditing = false
    @Published var editedText: String = ""

    let isFirstMessageOfDay: Bool
    let conversation: PersistentConversationDataItem
    
    public var text: NSMutableAttributedString {
        get {
            let body = source.body ?? "<No message provided, should NOT happen (TODO: recheck for multi-media case)>"
            let attributedString  = NSMutableAttributedString(string: body)
            let detector = try! NSDataDetector(types: NSTextCheckingResult.CheckingType.link.rawValue)
            let matches = detector.matches(in: body, options: [], range: NSRange(location: 0, length: body.utf16.count))
            var link: URL?

            for match in matches {
                guard let range = Range(match.range, in: body) else { continue }
                link = match.url
                let textRange = NSRange(range, in: body)

                attributedString.addAttribute(.link, value: link!, range: textRange)
                attributedString.addAttribute(.underlineStyle, value: NSUnderlineStyle.single.rawValue, range: textRange)
                attributedString.addAttribute(.underlineColor, value: getURLUnderlineColor(), range: textRange)
            }
            return attributedString
        }
    }
    
    var messageStatus: MessageStatus? {
        guard let raw = source.status?.lowercased() else { return nil }
        print("Message status: \(String(describing: raw))")

        return MessageStatus(rawValue: raw)
    }
    
    
    var formattedDateString: String {
        let formatter = DateFormatter()
        formatter.dateStyle = .medium
        formatter.timeStyle = .none
        return formatter.string(from: source.dateCreated ?? Date.now)
    }

    private func getURLUnderlineColor() -> UIColor {
        if direction == .outgoing {
            return UIColor(Color.green)
        } else {
            return UIColor(Color.black)
        }
    }

    public var author: String {
        get {
            source.author ?? "<unknown>"
        }
    }

    public var direction: MessageDirection {
        get {
            source.direction == MessageDirection.outgoing.rawValue ? .outgoing : .incoming
        }
    }

    public var formattedDate: String {
        guard let date = source.dateCreated else {
            return ""
        }
        let dateFormatter = DateFormatter()
        dateFormatter.dateStyle = .none  //  don't show date
        dateFormatter.timeStyle = .short //  show time like "7:23 PM"
        return dateFormatter.string(from: date)
    }


    public var icon: Image {
        get {
            switch contentCategory {
            case .text:
                return Image(systemName: "person")
            case .image:
                return Image(systemName: "person.crop.square")
            case .file:
                return Image(systemName: "doc.text")
            }
        }
    }
    
    var contentCategory: MessageType {
        get {
            guard !(media?.sid ?? "").isEmpty else {
                return .text
            }
            if let mediaType = media?.contentType, ["image/jpeg", "image/png"].contains(mediaType) {
                return .image
            }
            return .file
        }
    }

    var mediaIconName: String {
        switch attachmentState {
        case .downloaded, .downloading: return "doc"
        case .notDownloaded: return "square.and.arrow.down"
        }
    }
    var mediaAttachmentName: String {
        return media?.filename ?? String()
    }
    
    var mediaAttachmentSize: String {
        return ByteCountFormatter.string(fromByteCount: media?.size ?? 0, countStyle: .file)
    }
    
    var messageIndex: Int64 {
        return source.messageIndex
    }
    
    var deliveryDetails: String {
        get {
            let status = NSLocalizedString("message.send.status.sent", comment: "Sent")
            guard let date = source.dateUpdated ?? source.dateCreated else {
                return String()
            }
            let dateFormatter = DateFormatter()
            dateFormatter.timeStyle = .medium
            if Calendar.current.isDateInToday(date) {
                dateFormatter.dateStyle = .none
            } else {
                dateFormatter.dateStyle = .medium
            }
            return "\(status) \(dateFormatter.string(from: date))"
        }
    }
    
    init(message: PersistentMessageDataItem, currentUser: String, isFirstMessageOfDay: Bool = false,    conversation: PersistentConversationDataItem) {
         self.source = message
         self.currentUser = currentUser
        self.isFirstMessageOfDay = isFirstMessageOfDay
         self.conversation = conversation
     }

    // MARK: - Hashable
    func hash(into hasher: inout Hasher) {
        hasher.combine(self.source.uuid)
    }
    // MARK: - Equatable
    static func == (lhs: MessageBubbleViewModel, rhs: MessageBubbleViewModel) -> Bool {
        return lhs.source.uuid == rhs.source.uuid && lhs.source.messageIndex == rhs.source.messageIndex
    }
    
    //MARK: - Private
    private var media: PersistentMediaDataItem? {
        //For Demo Apps v1 we'll assume that mediaAttachment for a message only has 1 attachment.
        guard let attachedMedia = source.attachedMedia,
              let media = Array(attachedMedia).first as? PersistentMediaDataItem else {
                  return nil
        }
        return media
    }
}
