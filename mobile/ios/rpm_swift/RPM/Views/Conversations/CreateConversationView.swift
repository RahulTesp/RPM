//
//  CreateConversationView.swift
//  ConversationsApp
//
//  Created by Berkus Karchebnyy on 03.11.2021.
//  Copyright © 2021 Twilio, Inc. All rights reserved.
//

import SwiftUI


struct CreateConversationView: View {
    @EnvironmentObject var conversationManager: ConversationManager
    @EnvironmentObject var participantsManager: ParticipantsManager
    @EnvironmentObject var appModel: AppModel
    @EnvironmentObject var navigationHelper: NavigationHelper
    @StateObject var memberDetList = MembersListViewModel()
    
    var items: [PersistentConversationDataItem]
    
    @State private var conversationTitle = ""
    @State private var errorText = ""
    @State private var createButtonDisabled = false
    
    
    var body: some View {
        VStack {
            if memberDetList.loading {
                ProgressView("Loading members...")
                    .progressViewStyle(CircularProgressViewStyle())
                    .padding()
            } else {
                if let members = memberDetList.memberDetails, !members.isEmpty {
                    List(members) { item in
                        Button(action: {
                            fetchChatSid(memberUserName: item.memberUserName)
                        }) {
                            Text(item.memberName)
                                .padding()
                                .frame(maxWidth: .infinity, alignment: .leading)
                                .background(
                                    RoundedRectangle(cornerRadius: 12)
                                        .fill(Color.gray.opacity(0.2))
                                )
                                .foregroundColor(Color("title1"))
                        }
                        .listRowSeparator(.hidden)
                        .listRowBackground(Color.clear)
                    }
                    .listStyle(PlainListStyle())

                } else {
                    Text("No members found.")
                        .foregroundColor(.gray)
                        .padding()
                }
            }
        }
        .padding()
        .navigationTitle("Select Contact")
        .onAppear {
            print("""
               CreateConversationView appeared.
               Environment Objects:
               conversationManager: \(conversationManager)
               participantsManager: \(participantsManager)
               """)
        }
    }

    private func fetchChatSid(memberUserName: String) {
        print("[fetchChatSid] Started for member: \(memberUserName)")
        
        memberDetList.getChatSidForMember(memberUsername: memberUserName) { chatSid, success, alertMessage in
            // print(" [getChatSidForMember] Completion Handler Called - Success: \(success), SID: \(chatSid ?? "nil")")
            
            if success {
                let cleanedString = chatSid.replacingOccurrences(of: "\"", with: "")
                // print(" [Success] Cleaned Chat SID: \(cleanedString)")
                
                if let conversationDataItem = conversationManager.fetchConversationDataItem(by: cleanedString) {
                    //  print(" [CoreData] Found conversation with SID: \(cleanedString)")
                    
                    DispatchQueue.main.async {
                        //  print(" [UI] Navigating to Message List View with existing conversation.")
                        navigateToMessageListView(with: conversationDataItem)
                    }
                } else {
                    // print(" [CoreData] Conversation not found for SID: \(cleanedString). Attempting to create a new one...")
                    
                    createConversation(conversationTitle: memberUserName) { newSID in
                        // print(" [createConversation] Completion Handler Called - newSID: \(newSID ?? "nil")")
                        
                        guard
                            let newSID = newSID,
                            let newConversation = conversationManager.fetchConversationDataItem(by: newSID)
                        else {
                            //   print(" [Error] Failed to create or fetch new conversation.")
                            return
                        }
                        
                        DispatchQueue.main.async {
                            // print(" [UI] Navigating to Message List View with newly created conversation.")
                            navigateToMessageListView(with: newConversation)
                        }
                    }
                }
            } else {
                //   print(" [Fallback] Chat SID not found. Creating a new conversation for user: \(memberUserName)")
                
                createConversation(conversationTitle: memberUserName) { newSID in
                    // print(" [createConversation] Completion Handler Called - newSID: \(newSID ?? "nil")")
                    
                    guard
                        let newSID = newSID,
                        let newConversation = conversationManager.fetchConversationDataItem(by: newSID)
                    else {
                        // print(" [Error] Failed to create or fetch new conversation.")
                        return
                    }
                    
                    DispatchQueue.main.async {
                        //print(" [UI] Navigating to Message List View with newly created conversation.")
                        navigateToMessageListView(with: newConversation)
                    }
                }
            }
        }
    }
    

    private func navigateToMessageListView(with conversation: PersistentConversationDataItem) {
        print("Navigating to Message List View with conversation SID: \(conversation.sid)")
        // Add the conversation to the navigation path to trigger the navigation
        
        // Remove CreateConversationView from stack
        if let last = navigationHelper.path.last, case .createConversation = last {
            navigationHelper.path.removeLast()
        }
        
        navigationHelper.path.append(.messageList(conversation: conversation))
        
        //print("Navigation path updated: \(navigationPath)")
        print("Navigation path updated with conversation: \(conversation.sid)")
    }
    
    
    private func createConversation(conversationTitle: String, completion: @escaping (String?) -> ()) {
        createButtonDisabled = true
        print("Creating conversation with title: \(conversationTitle)")
        
        conversationManager.createAndJoinConversation(friendlyName: conversationTitle) { result in
            createButtonDisabled = false
            
            switch result {
            case .success(let conversationSID):
                DispatchQueue.main.async {
                    print("Conversation created successfully with SID: \(conversationSID)")
                    completion(conversationSID)
                    
                    memberDetList.chatUpdate(toUser: conversationTitle, convSid: conversationSID) { statuscode, success, _ in
                        print("Chat update success: \(success), status code: \(statuscode)")
                    }
                }
                
                participantsManager.addChatParticipant(conversationTitle, conversation: conversationSID) { error in
                    if let error = error {
                        print("Error adding participant: \(error)")
                    } else {
                        print("Added \(conversationTitle) to conversation successfully")
                    }
                }
                
            case .failure(let error):
                self.errorText = error.localizedDescription
                print("Failed to create conversation: \(self.errorText)")
                completion(nil)
            }
        }
    }
}

