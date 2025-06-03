//
//  ConversationListItemView.swift
//  ConversationsApp
//
//  Created by Berkus Karchebnyy on 28.10.2021.
//  Copyright © 2021 Twilio, Inc. All rights reserved.
//

import SwiftUI

struct ConversationListItem: View {
    @ObservedObject var viewModel: PersistentConversationDataItem
    @EnvironmentObject var appModel: AppModel
    
    var body: some View {
        VStack {
            HStack {
                if viewModel.muted {
                    Image(systemName: "bell.slash")
                        .font(.system(size: 16))
                        .foregroundColor(Color.black)
                    Text(viewModel.title)
                        .lineLimit(1)
                        .font(.system(size: 16))
                        .foregroundColor(Color.orange)
                } else {
                    let titleUsername = viewModel.title.components(separatedBy: "-").first ?? ""
                    let displayName = getMemberNameFromUsername(titleUsername) ?? titleUsername
                    
                    
                    VStack(alignment: .leading, spacing: 2) {
                        
                        HStack
                        {
                            Text(displayName)
                            
                            //                        Text(viewModel.title.components(separatedBy: "-").first ?? "")
                                .lineLimit(1)
                                .font(.system(size: 22))
                                .foregroundColor(Color("title1"))
                            
                            Spacer()
                            
                            Text("\(viewModel.lastMessageDateFormatted)")
                                .font(.system(size: 16))
                                .foregroundColor(Color("ChatLastMsgColor"))
                        }
                        Text(viewModel.lastMessageText)
                            .lineLimit(1)
                            .font(.system(size: 15))
                            .foregroundColor(Color("ChatLastMsgColor"))
                    }  .padding(.top, 4)
                    
                    
                }
                Spacer()
                
            }
            .padding(.bottom, 1)
            HStack {
                
                
                Spacer()
                
                if (viewModel.unreadMessagesCount > 0) {
                    Text("\(viewModel.unreadMessagesCount)")
                        .font(.system(size: 14))
                        .padding(.horizontal, 8)
                        .background(Color.cyan)
                        .foregroundColor(Color("buttonColor"))
                        .cornerRadius(16)
                }
            }
        }
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
    
    
    func getParticipantCountString(_ count: Int64) -> String {
        // empty conversations report back as having 0 participants so round up to 1
        let modifiedCount = max(count, 1)
        let string = modifiedCount == 1 ? "conv.part_cnt.snglr" : "conv.part_cnt.plrl"
        return String(format: NSLocalizedString(string, comment: "Text stating the number of participants"), "\(modifiedCount)")
    }
}

// Mark: Last Message View
struct LastMessageView: View {
    @ObservedObject var viewModel: PersistentConversationDataItem
    @EnvironmentObject var appModel: AppModel
    
    var body: some View {
        if(viewModel.lastMessageContentAuthor == appModel.myIdentity){
            //Outgoing message
            switch viewModel.lastMessageContentType {
            case .text:
                Text(viewModel.lastMessagePreview ?? "")
                    .lineLimit(1)
                    .font(.system(size: 14))
                    .foregroundColor(Color.red)
            case .image:
                Image(systemName: viewModel.lastMessageContentIcon)
                    .foregroundColor(Color.gray)
                    .font(Font.system(size: 16))
                Text("\("You") conversations.image.sharing.label")
                    .lineLimit(1)
                    .font(.system(size: 14))
                    .foregroundColor(Color.green)
            default:
                Image(systemName: viewModel.lastMessageContentIcon)
                    .foregroundColor(Color.blue)
                    .font(Font.system(size: 16))
                Text("\("You") conversations.file.sharing.label")
                    .lineLimit(1)
                    .font(.system(size: 14))
                    .foregroundColor(Color.blue)
            }
        } else {
            //Incoming message
            switch viewModel.lastMessageContentType {
            case .text:
                Text(viewModel.lastMessagePreview ?? "")
                    .lineLimit(1)
                    .font(.system(size: 14))
                    .foregroundColor(Color.brown)
            case .image:
                Image(systemName: viewModel.lastMessageContentIcon)
                    .foregroundColor(Color.yellow)
                    .font(Font.system(size: 16))
                Text("\(viewModel.lastMessageContentAuthor) conversations.image.sharing.label")
                    .lineLimit(1)
                    .font(.system(size: 14))
                    .foregroundColor(Color.orange)
            default:
                Image(systemName: viewModel.lastMessageContentIcon)
                    .foregroundColor(Color.black)
                    .font(Font.system(size: 16))
                Text("\(viewModel.lastMessageContentAuthor) conversations.file.sharing.label")
                    .lineLimit(1)
                    .font(.system(size: 14))
                    .foregroundColor(Color.red)
            }
        }
    }
}

struct ConversationListItem_Previews: PreviewProvider {
    static var previews: some View {
        let appModel = AppModel(inMemory: true)
        let bubbles: [PersistentConversationDataItem.Decode] = load("testConversations.json")
        let managedObjectContext = appModel.getManagedContext()
        
        List {
            ForEach(0..<100) { n in
                ConversationListItem(viewModel: bubbles[0].conversation(inContext: managedObjectContext))
                ConversationListItem(viewModel: bubbles[1].conversation(inContext: managedObjectContext))
                ConversationListItem(viewModel: bubbles[2].conversation(inContext: managedObjectContext))
            }
        }
        .previewLayout(.fixed(width: 400, height: 700))
        .environmentObject(appModel)
    }
}
