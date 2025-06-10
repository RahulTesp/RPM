//
//  ConversationListView.swift
//  ConversationsApp
//
//  Created by Berkus Karchebnyy on 28.10.2021.
//  Copyright © 2021 Twilio, Inc. All rights reserved.
//

import SwiftUI
import Combine
import TwilioConversationsClient


struct ConversationsList: View {
    @EnvironmentObject var appModel: AppModel
    @EnvironmentObject var navigationHelper: NavigationHelper
    @EnvironmentObject var conversationManager: ConversationManager
    @EnvironmentObject var participantsManager: ParticipantsManager
    
    @StateObject private var viewModel = ConversationListViewModel()
    @State private var filterQuery: String = ""
    @State private var cancellableSet: Set<AnyCancellable> = []
    @State private var showNoChatsToast = false
    
    
    @Environment(\.presentationMode) var mode: Binding<PresentationMode>
    
    init() {
        UITableView.appearance().backgroundColor = .clear
        NotificationCenter.default.addObserver(
            forName: NSNotification.Name("logoutRequired"),
            object: nil,
            queue: .main
        ) { _ in
            print("LogoutRequiredNoti")
        }
    }
    
    var body: some View {
        
        ZStack {
            Color("ChatBGcolor").ignoresSafeArea()
            
            VStack(spacing: 0) {
                HStack {
                    Text("Chats")
                        .font(.system(size: 25, weight: .bold))
                        .foregroundColor(.black)
                        .padding(.leading)
                    Spacer()
                }
                .padding(.top, 8)
                
                ScrollView {
                    VStack(spacing: 0) {
                        if conversationManager.isConversationsLoading {
                            LoadingView()
                        } else if conversationManager.conversations.isEmpty {
                            if let error = appModel.conversationsError {
                                ConversationsListErrorView(error: error) {
                                    conversationManager.subscribeConversations(onRefresh: false)
                                }
                            } else {
                                
                                // Show toast when no conversations exist
                                Text("") // dummy view to force evaluation
                                    .onAppear {
                                        withAnimation {
                                            showNoChatsToast = true
                                        }
                                        DispatchQueue.main.asyncAfter(deadline: .now() + 2.5) {
                                            withAnimation {
                                                showNoChatsToast = false
                                            }
                                        }
                                    }
                                
                            }
                        } else {
                            VStack(alignment: .leading, spacing: 0) {
                                ConversationItemsListView(items: conversationManager.conversations, searchText: $filterQuery)
                                    .frame(maxWidth: .infinity)
                            }
                            .padding(.top, 6)
                            .padding(.bottom, 20)
                            .background(Color("ChatBGcolor"))
                        }
                    }
                    .padding(.horizontal)
                    .background(Color("ChatBGcolor"))
                }
            }
            
            if appModel.globalStatus == .noConnectivity {
                GlobalStatusView(
                    message: NSLocalizedString("status.error.connectivity", comment: ""),
                    kind: .error
                )
                .transition(.move(edge: .top))
            }
            
            
            if showNoChatsToast {
                VStack {
                    Spacer()
                    Text("No Chats")
                        .padding()
                        .background(Color.black.opacity(0.8))
                        .foregroundColor(.white)
                        .cornerRadius(8)
                        .padding(.bottom, 40)
                        .transition(.move(edge: .bottom).combined(with: .opacity))
                }
                .animation(.easeInOut, value: showNoChatsToast)
            }
            
            
            
        }
        .navigationTitle("Activity")
        .navigationBarTitleDisplayMode(.inline)
        .navigationBarBackButtonHidden(true)
        .navigationBarItems(
            leading: Button(action: {
                self.mode.wrappedValue.dismiss()
            }) {
                Image(systemName: "chevron.left")
            },
            
            trailing: Button(action: {
                
                print("Before navigation path: \(navigationHelper.path)")
                navigationHelper.path.append(.createConversation)
                
                print("After navigation path: \(navigationHelper.path)")
                
            }) {
                Image("AddOutline")
            }
        )
        
        .navigationDestination(for: Screen.self) { screen in
            switch screen {
            case .createConversation:
                CreateConversationView(
                    
                    items: conversationManager.conversations
                )
                .environmentObject(appModel)
                .environmentObject(conversationManager)
                .environmentObject(participantsManager)
                .environmentObject(navigationHelper)
                
            case .messageList(let conversation):
                MessageListView(conversation: conversation, viewModel: MessageListViewModel())
                    .environmentObject(appModel)
                    .environmentObject(conversationManager)
                    .environmentObject(participantsManager)
                    .environmentObject(navigationHelper)
                    .environmentObject(appModel.messagesManager)
            default:
                EmptyView()
            }
        }
        
        .onAppear {
            print("ConversationManager is accessible: \(conversationManager)")
            print("Current navigation path in ConversationsList: \(navigationHelper.path)")
        }
    }
}

struct ConversationsListErrorView: View {
    
    var error: TCHError
    var buttonAction: () -> Void
    
    var body: some View {
        VStack (alignment: .center) {
            Image(systemName: "exclamationmark.square.fill")
                .font(.system(size: 20))
                .foregroundColor(Color.red)
                .padding(EdgeInsets(top: 0, leading: 0, bottom: 4, trailing: 0))
            Text("errorCode \(String(error.code))")
                .font(.system(size: 20, weight: .bold))
            if let errorMessage = error.userInfo["TCHErrorMsgKey"] as? String {
                Text(errorMessage)
                    .font(.system(size: 16))
                    .foregroundColor(Color.blue)
                    .padding(EdgeInsets(top: 4, leading: 16, bottom: 16, trailing: 16))
            }
            Button(action: buttonAction, label: {
                Text("conversations.loading_error.buttonText")
                    .font(.system(size: 14, weight: .bold))
                    .padding(EdgeInsets(top: 0, leading: 16, bottom: 12, trailing: 16))
            })
            .padding(EdgeInsets(top: 12, leading: 16, bottom: 12, trailing: 16))
            .background(Color.gray)
            .foregroundColor(Color.white)
            .cornerRadius(4)
        }
    }
}
