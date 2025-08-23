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
    @EnvironmentObject var memberDetList: MembersListViewModel
    
    var items: [PersistentConversationDataItem]
    
    @State private var conversationTitle = ""
    @State private var errorText = ""
    @State private var createButtonDisabled = false
    @State private var selectedMemberUserName: String? = nil
    @State private var longPressedMemberUserName: String? = nil
    @State private var isLoading = false

    
    var body: some View {
        ZStack {
            VStack {
                if memberDetList.loading {
                    ProgressView("Loading members...")
                        .progressViewStyle(CircularProgressViewStyle())
                        .padding()
                } else {
                    if let members = memberDetList.memberDetails, !members.isEmpty {
                        List(members) { item in
                            HStack(spacing: 12) {
                                if selectedMemberUserName == item.memberUserName ||
                                   longPressedMemberUserName == item.memberUserName {
                                    Image("Icons_Check A")
                                        .foregroundColor(.green)
                                } else {
                                    Image("Icons_Check_password")
                                        .resizable()
                                        .frame(width: 24, height: 24)
                                        .clipShape(Circle())
                                }

                                Text(item.memberName)
                                    .foregroundColor(
                                        (selectedMemberUserName == item.memberUserName ||
                                         longPressedMemberUserName == item.memberUserName)
                                        ? .black : Color("title1")
                                    )

                                Spacer()
                            }
                            .padding()
                            .background(
                                RoundedRectangle(cornerRadius: 12)
                                    .fill((selectedMemberUserName == item.memberUserName ||
                                           longPressedMemberUserName == item.memberUserName)
                                          ? Color.gray.opacity(0.2) : Color.clear)
                            )
                            .contentShape(Rectangle())
                            .onTapGesture {
                                guard !isLoading else { return }  // Disable tap during loading
                                isLoading = true
                                selectedMemberUserName = item.memberUserName
                                longPressedMemberUserName = nil
                                fetchChatSid(memberUserName: item.memberUserName)
                            }
                            .onLongPressGesture {
                                longPressedMemberUserName = item.memberUserName
                                selectedMemberUserName = nil
                            }
                            .listRowInsets(EdgeInsets(top: 3, leading: 0, bottom: 3, trailing: 0))
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
            
            // Overlay loading spinner
            if isLoading {
                Color.black.opacity(0.3)
                    .edgesIgnoringSafeArea(.all)
                
                ProgressView("Loading...")
                    .progressViewStyle(CircularProgressViewStyle())
                    .padding()
                    .background(RoundedRectangle(cornerRadius: 12).fill(Color.white))
            }
        }
    }


    private func fetchChatSid(memberUserName: String) {
        print("[fetchChatSid] Started for member: \(memberUserName)")
        
        memberDetList.getChatSidForMember(memberUsername: memberUserName) { chatSid, success, alertMessage in
            print(" [getChatSidForMember] Completion Handler Called - Success: \(success), SID: \(chatSid)")
            
            if success {
                let cleanedString = chatSid.replacingOccurrences(of: "\"", with: "")
                 print(" [Success] Cleaned Chat SID: \(cleanedString)")
                
                if let conversationDataItem = conversationManager.fetchConversationDataItem(by: cleanedString) {
                      print(" [CoreData] Found conversation with SID: \(cleanedString)")
                    
                    DispatchQueue.main.async {
                          print(" [UI] Navigating to Message List View with existing conversation.")
                        DispatchQueue.main.async {
                            self.isLoading = false  // stop loading
                            
                            navigateToMessageListView(with: conversationDataItem)
                        }
                    }
                }
                
                else
                {
                    conversationManager.retrieveConversation(cleanedString) { twilioConversation, error in
                        if let twilioConversation = twilioConversation {
                            print(" Found conversation on Twilio with SID: \(cleanedString)")

                            // Get Core Data context from AppModel
                            let context = appModel.getManagedContext()

                            // Save/update the conversation in Core Data
                            if let newConversation = PersistentConversationDataItem.from(
                                conversation: twilioConversation,
                                inContext: context
                            ) {
                                DispatchQueue.main.async {
                                    self.navigateToMessageListView(with: newConversation)
                                }
                            }
                        } else {
                            print(" Conversation not found on Twilio. You may need to create a new one.")
                        }
                    }

                }

            } else {
                   print(" [Fallback] Chat SID not found. Creating a new conversation for user: \(memberUserName)")
                
                createConversation(conversationTitle: memberUserName) { newSID in
                     print(" [createConversation] Completion Handler Called - newSID: \(newSID ?? "nil")")
                    
                    guard
                        let newSID = newSID,
                        let newConversation = conversationManager.fetchConversationDataItem(by: newSID)
                    else {
                         print(" [Error] Failed to create or fetch new conversation.")
                        return
                    }
                    DispatchQueue.main.async {
                        print(" [UI] Navigating to Message List View with newly created conversation.")
                        self.isLoading = false
                        navigateToMessageListView(with: newConversation)
                    }

                }
            }
        }
    }
    

    private func navigateToMessageListView(with conversation: PersistentConversationDataItem) {
        
        // Remove CreateConversationView from stack
        if let last = navigationHelper.path.last, case .createConversation = last {
            navigationHelper.path.removeLast()
        }
        
        navigationHelper.path.append(.messageList(conversation: conversation))
       
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

