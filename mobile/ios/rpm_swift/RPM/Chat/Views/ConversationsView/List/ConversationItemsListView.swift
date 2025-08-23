//
//  ConversationsItemListView.swift
//  ConversationsApp
//
//  Created by Robert Ziehl on 2022-05-03.
//  Copyright © 2022 Twilio, Inc. All rights reserved.
//

import Foundation
import SwiftUI

struct ConversationItemsListView: View {
    var items: [PersistentConversationDataItem]
    @Binding var searchText: String
    @EnvironmentObject var appModel: AppModel
    @EnvironmentObject var navigationHelper: NavigationHelper
    @EnvironmentObject var conversationManager: ConversationManager
    @State private var showingLeaveConversationDialog = false
    @EnvironmentObject var messagesManager: MessagesManager
    @State var selectedConversationForDeletion: PersistentConversationDataItem? = nil
    @Environment(\.managedObjectContext) private var context

    
    var filteredConversations: [PersistentConversationDataItem] {
        if searchText.isEmpty {
            return items
        } else {
            return items.filter {
                $0.uniqueName?.localizedCaseInsensitiveContains(searchText) ?? false || $0.friendlyName?.localizedCaseInsensitiveContains(searchText) ?? false
            }
        }
    }
    
    var body: some View {
        
 
        if(filteredConversations.isEmpty){
            
            NoSearchResultsView()
        }
            else {
                
                VStack(spacing: 10) {
                          ForEach(filteredConversations) { model in
                              let vm = conversationManager.viewModel(for: model, context: context)
                            
                ConversationRowItem(conversation: model, navigationHelper: _navigationHelper , convItemViewModel: vm)
                                  .environmentObject(messagesManager)
                                  .padding(.horizontal)
                                          .padding(.vertical, 8) //  Optional internal padding
                                          .background(Color.white)
                                          .cornerRadius(10)
                                         
               
                    .listRowBackground(Color("ChatBGcolor"))

                .swipeActions(edge: .trailing, allowsFullSwipe: false) { // MARK: ios 15+
                    Button(role: .destructive) { // MARK: ios 15+
                        selectedConversationForDeletion = model
                        showingLeaveConversationDialog = true
                        NSLog("Showing dialog")
                    } label: {
                        Label("Leave", systemImage: "rectangle.portrait.and.arrow.right")
                    }
                    .searchable(text: $searchText)

                }

            }
            .background(Color("ChatBGcolor")) //  list background
        
            .listStyle(InsetListStyle())
            .refreshable { // MARK: This only works for iOS 15+
                conversationManager.subscribeConversations(onRefresh: true)
            }
            }.padding(.vertical)
        }
    }
}


struct ConversationRowItem: View {
    var conversation: PersistentConversationDataItem
    @EnvironmentObject var navigationHelper: NavigationHelper
    @EnvironmentObject var messagesManager: MessagesManager
    @ObservedObject var convItemViewModel: ConversationItemViewModel
    
    var body: some View {
        Button {
   
            navigationHelper.path.append(.messageList(conversation: conversation))

        } label: {
            ConversationListItem(viewModel: conversation, convItemViewModel: convItemViewModel)
                .frame(minHeight: 40)
                .background(
                    RoundedRectangle(cornerRadius: 12)
                        .fill(Color("ChatBoxColour"))
                )
        }
    }
}
