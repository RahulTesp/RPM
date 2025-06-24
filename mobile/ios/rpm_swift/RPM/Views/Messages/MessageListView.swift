//
//  MessageListView.swift
//  ConversationsApp
//
//  Created by Berkus Karchebnyy on 14.10.2021.
//  Copyright © 2021 Twilio, Inc. All rights reserved.
//


import SwiftUI
import Combine
import TwilioConversationsClient

enum MessageAction {
    case edit, remove
}

enum MessageStatus: String {
    case sending
    case sent
    case failed
}


struct Messages: Identifiable {
    let id = UUID()
    let messages: [PersistentMessageDataItem]
}

struct MessageListView: View {
    
    // MARK: Observable
    @EnvironmentObject var appModel: AppModel
    @EnvironmentObject var conversationManager: ConversationManager
    @EnvironmentObject var messagesManager: MessagesManager
    @EnvironmentObject var participantsManager: ParticipantsManager
    @Environment(\.presentationMode) var mode: Binding<PresentationMode>
    
    //The message list view model should be an Observed object because we don't want to retain the view model once the view is destroyed
    @ObservedObject private var viewModel: MessageListViewModel
    private var conversation: PersistentConversationDataItem
    
    @State private var textToSend = ""
    @State private var showingPickerSheet = false
    @State private var cancellableSet: Set<AnyCancellable> = []
    private let messagesPaginationSize: UInt = 40
    private let unreadSectionHeaderId = "unreadSectionHeader"
    @State private var showUnreadMessageSection = true
    @State private var messageCount: Int = 0
    @State private var isCancelled = false
    
    @State private var isShowingSendError = false
    @State private var sendError: TCHError? = nil
    
    @State private var scrollOffset: CGFloat = 0  // Track scroll position
    @State private var showScrollToBottom: Bool = false  // Controls arrow visibility
    @State private var bottomOffset: CGFloat? = nil
    
    @State private var heartbeatTimer: Timer? = nil
    @State private var topOffset: CGFloat? = nil
    @State private var showNewMessageBanner: Bool = false
    
    // The display name is computed here to avoid issues with initialization order.
    @State private var displayName: String = ""
    // MARK: View
    
    var isLoadingMessages: Bool {
        viewModel.readMessages.isEmpty && viewModel.unreadReceivedMessages.isEmpty
    }
    
    var body: some View {
       
        let markAllMessagesReadTask = self.markAllMessagesReadTask
        
        ZStack(alignment: .center) {
            ZStack(alignment: .top) {
                VStack(alignment: .center) {
                    if isConversationNew() {
                        VStack() {
                            Spacer()
                            // AddParticipantsButton()
                        }
                        .padding()
                    } else {
                        ScrollViewReader { proxy in
                            ScrollView(.vertical) {
                                
                                
                                GeometryReader { geometry in
                                    let offset = geometry.frame(in: .named("scrollView")).minY
                                    // print("Offset inside GeometryReader: \(offset)")
                                    return Color.clear
                                        .preference(key: ScrollOffsetPreferenceKey.self, value: offset)
                                }
                                .frame(height: 1)
                                
                                PullToRefresh(coordinateSpaceName: "messageListPullToRefresh")
                                    .refreshable {
                                        print("PullToRefreshcalled")
                                        
                                        print(" Current loaded message indexes = \(messagesManager.messages.map { $0.messageIndex })")

                                        
                                        guard let lastMessage = messagesManager.messages.first else { return }
                                        let beforeIndex = lastMessage.messageIndex
                                        print("messagesManager.messages.first?.messageIndex = \(beforeIndex)")
                                        
                                        messagesManager.loadMessages(
                                            for: conversation,
                                            before: beforeIndex,
                                            max: messagesPaginationSize
                                        )
                                    }

                                HStack (alignment: .top, spacing: 0){
                                    
                                    if messagesManager.messages.isEmpty {
                                        ProgressView("Loading messages...Will take 2 minutes")
                                            .frame(maxWidth: .infinity, maxHeight: .infinity)
                                    } else {
                                    LazyVStack(alignment: .leading) {
                                       
                                        ForEach(Array(messagesManager.messages.enumerated()), id: \.element) { index, message in
                                            messageRowView(index: index, message: message, conversation: conversation)
                                        }
                                       
                                        Color.clear.frame(height: 1).id("BottomAnchor")
                                        
                                    }
                                    
                                    .frame(maxWidth: .infinity, alignment: .leading)
                                    
                                }
                                }
                                .frame(maxWidth: .infinity, alignment: .leading)
                                
                                
                                .onAppear {
                                    guard let lastMessage = messagesManager.messages.last else { return }
                                    
                                    if let lastId = viewModel.readMessages.last?.messageIndex {
                                        // print("scrollToBottom to id:", lastId)
                                        proxy.scrollTo(lastId, anchor: .bottom)
                                    }
                                    
                                     print("viewModel.unreadReceivedMessages.count", viewModel.unreadReceivedMessages.count)
                                  
                                    let isFirstLoad = (messageCount == 0)
                                    let hasUnreadMessages = viewModel.unreadReceivedMessages.count > 0
                                    let isOutgoing = lastMessage.direction == MessageDirection.outgoing.rawValue
                                    
                                     print("lastMessage:", lastMessage)
                           
                                    let shouldAutoScroll = isOutgoing || isFirstLoad || isNearBottom || hasUnreadMessages
                                    //  print("shouldAutoScroll:", shouldAutoScroll)
                                    
                                    if shouldAutoScroll {
                                        DispatchQueue.main.asyncAfter(deadline: .now() + 0.1) {
                                            //  print("Scrolling to last message: \(lastMessage)")
                                          
                                            withAnimation {
                                                proxy.scrollTo(lastMessage, anchor: .bottom)
                                                showNewMessageBanner = false
                                            }
                                            if isOutgoing {
                                                Task {
                                                    await markAllMessagesReadTask
                                                }
                                            }
                                        }
                                    } else {
                                        showNewMessageBanner = true
                                    }
                                    
                                    messageCount = appModel.messagesManager.messages.count
                                    
                                    print("messageCount:", messageCount)
                                }
                         
                            }
                            
                            .coordinateSpace(name: "scrollView") //  This is good
                       
                            .onPreferenceChange(ScrollOffsetPreferenceKey.self) { offset in
                                scrollOffset = offset
                                
                                if bottomOffset == nil || offset < bottomOffset! {
                                    bottomOffset = offset
                                }
                                
                                if topOffset == nil || offset > topOffset! {
                                    topOffset = offset
                                }
                                
                                if let bottom = bottomOffset, let top = topOffset {
                                    let diffBottom = abs(bottom - offset)
                                    let nearBottom = diffBottom < 50
                                    let nearTop = abs(top - offset) < 50
                                    
                                    print(" Bottom offset: \(bottom), Top offset: \(top), Current offset: \(offset), DiffBottom: \(diffBottom)")
                                    
                                    showScrollToBottom = !nearBottom
                                    
                                    if viewModel.unreadReceivedMessages.count > 0 {
                                        showUnreadMessageSection = !nearBottom || nearTop
                                    } else {
                                        showUnreadMessageSection = false
                                    }
                                }
                            }
                           
                            .overlay(
                                VStack {
                                    Spacer()
                                    if showScrollToBottom {
                                        ScrollToBottomButton {
                                            withAnimation {
                                                    proxy.scrollTo("BottomAnchor", anchor: .bottom)
                                                    showNewMessageBanner = false
                                                        }

                                        }
                                    }
                                },
                                alignment: .bottomTrailing
                            )
                            
                            .overlay(
                                VStack {
                                    Spacer()
                                    if showNewMessageBanner {
                                        Button(action: {
                                            withAnimation {
                                                proxy.scrollTo("BottomAnchor", anchor: .bottom)
                                                showNewMessageBanner = false
                                            }
                                        }) {
                                            Text("New Messages")
                                                .padding(.horizontal, 16)
                                                .padding(.vertical, 10)
                                                .background(Color("title1"))
                                                .foregroundColor(.white)
                                                .cornerRadius(20)
                                                .shadow(radius: 2)
                                        }
                                        .padding(.trailing, 12)
                                        .padding(.bottom, 24)
                                        .transition(.move(edge: .bottom).combined(with: .opacity))
                                    }
                                },
                                alignment: .bottomTrailing
                            )
                         
                            .onChange(of: messagesManager.messages.count) { newCount in
                                print("Messages count changed from \(messageCount) to \(newCount)")
                                
                                // If messages decreased or no change, just update and exit
                                guard newCount > messageCount else {
                                    messageCount = newCount
                                    return
                                }
                                
                                // Get the last message safely
                                guard let lastMessage = messagesManager.messages.last else {
                                    messageCount = newCount
                                    return
                                }
                                
                                let isFirstLoad = (messageCount == 0)
                                let shouldAutoScroll = lastMessage.direction == MessageDirection.outgoing.rawValue || isFirstLoad || isNearBottom
                                
                                func scrollToBottom() {
                                    withAnimation {
                                        proxy.scrollTo(lastMessage, anchor: .bottom)
                                        showNewMessageBanner = false
                                    }

                                    Task {
                                        await markAllMessagesReadTask
                                    }
    
                                }
                                
                                if shouldAutoScroll {
                                    DispatchQueue.main.async {
                                        scrollToBottom()
                                    }
                                } else {
                                    // User isn’t near bottom, show banner instead of scrolling
                                    showNewMessageBanner = true
                                }
                                
                                // Update count for next comparison
                                messageCount = newCount
                            }
                           
                            .coordinateSpace(name: "messageListPullToRefresh")
                        }
                        
                    }
                    
                    MessageInputBar(
                        textToSend: $textToSend,
                        viewModel: viewModel,
                        appModel: appModel,
                        conversation: conversation,
                        showingPickerSheet: $showingPickerSheet
                    )
                    
                }
                .background(Color("MessagesBgColor"))
                .navigationTitle(Text(appModel.selectedConversation?.displayName ?? "")) // MARK: ios 14+
                .navigationBarTitleDisplayMode(.inline) // MARK: ios 14+
                .navigationBarBackButtonHidden(true)
                .navigationBarItems(leading: Button(action : {
                    self.mode.wrappedValue.dismiss()
                    isCancelled = true
                }){
                    HStack {
                        Image(systemName:"chevron.left")
                        Text("")
                            .font(.system(size: 16))
                        
                    }
                })
                
                .onAppear(perform: {
                    
                    handleOnAppear()
                    
                    heartbeatTimer = Timer.scheduledTimer(withTimeInterval: 10.0, repeats: true) { _ in
                           sendHeartbeat()
                       }
              
                })
                .onDisappear {
                    heartbeatTimer?.invalidate()
                    heartbeatTimer = nil
                }
                getStatusBanner(event: viewModel.currentConversationEvent)
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
    
    var isNearBottom: Bool {
        guard let bottom = bottomOffset else { return false }
        return abs(bottom - scrollOffset) < 50
    }
   
    var markAllMessagesReadTask: DispatchWorkItem {
        DispatchWorkItem {
            if !isCancelled {
                print("setallreadmsg")
                messagesManager.setAllMessagesRead(for: conversation.sid)
                showUnreadMessageSection = false
            }
        }
    }
    
    @ViewBuilder
    func messageRowViewBase(
        index: Int,
        message: PersistentMessageDataItem,
        messages: [PersistentMessageDataItem],
        conversation: PersistentConversationDataItem,
        showSeparator: Bool = true,
        onAppearAction: (() -> Void)? = nil
    ) -> some View {
        
        let previousDate = index > 0 ? messages[index - 1].dateCreated : nil
        let isFirstMessageOfDay = previousDate == nil || !Calendar.current.isDate(message.dateCreated ?? Date.now, inSameDayAs: previousDate!)
        
        VStack(alignment: .leading, spacing: 4) {
            
            if showSeparator && isFirstMessageOfDay {
                HStack {
                    Rectangle()
                        .fill(Color("ChatDateLineColor"))
                        .frame(height: 1)
                        .frame(maxWidth: .infinity)
                    
                    Text(formattedDate(message.dateCreated ?? Date.now))
                        .font(.caption)
                        .foregroundColor(Color("title1"))
                        .padding(.horizontal, 8)
                        .padding(.vertical, 2)
                        .background(Color("lightGreen"))
                        .cornerRadius(8)
                    
                    Rectangle()
                        .fill(Color("ChatDateLineColor"))
                        .frame(height: 1)
                        .frame(maxWidth: .infinity)
                }
                .padding(.horizontal)
                .padding(.bottom, 8)
            }
            
            MessageBubbleView(
                viewModel: MessageBubbleViewModel(
                    message: message,
                    currentUser: appModel.myIdentity,
                    isFirstMessageOfDay: isFirstMessageOfDay,
                    conversation: conversation
                )
            )
            .environmentObject(appModel.messagesManager)
            .frame(maxWidth: .infinity)
            .id(message.messageIndex)
            .onAppear {
                onAppearAction?()
            }
        }
    }
    
    
    @ViewBuilder
    func messageRowView(index: Int, message: PersistentMessageDataItem, conversation: PersistentConversationDataItem) -> some View {
        
        messageRowViewBase(
            index: index,
            message: message,
            messages: messagesManager.messages,
            conversation: conversation,
            onAppearAction: {
                DispatchQueue.main.asyncAfter(deadline: .now() + 2, execute: markAllMessagesReadTask)
            }
        )
      
    }
    
    
    @ViewBuilder
    func messageRowViewItem(index: Int, message: PersistentMessageDataItem, conversation: PersistentConversationDataItem) -> some View {
        
        messageRowViewBase(
            index: index,
            message: message,
            messages: viewModel.readMessages,
            conversation: conversation
        )
    
    }
    
    
    @ViewBuilder
    func messageRowViewForSection(index: Int, message: PersistentMessageDataItem, messages: [PersistentMessageDataItem], conversation: PersistentConversationDataItem) -> some View {
        
        messageRowViewBase(
            index: index,
            message: message,
            messages: messages,
            conversation: conversation,
            onAppearAction: {
                if messages.last == message {
                    DispatchQueue.main.asyncAfter(deadline: .now() + 2, execute: markAllMessagesReadTask)
                }
            }
        )
      
    }
    
    
    
    @ViewBuilder
    func messageRowViewForUnread(index: Int, message: PersistentMessageDataItem, messages: [PersistentMessageDataItem], conversation: PersistentConversationDataItem) -> some View {
        
        messageRowViewBase(
            index: index,
            message: message,
            messages: messages,
            conversation: conversation,
            onAppearAction: {
                if messages.last == message {
                    DispatchQueue.main.asyncAfter(deadline: .now(), execute: markAllMessagesReadTask)
                }
            }
        )
     
    }
    
    
    @ViewBuilder
    func messageRowViewForUnreadSent(index: Int, message: PersistentMessageDataItem, messages: [PersistentMessageDataItem], conversation: PersistentConversationDataItem) -> some View {
        
        messageRowViewBase(
            index: index,
            message: message,
            messages: messages,
            conversation: conversation
        )
        
    }
    
    
    //  Optional date formatter
    func formattedDate(_ date: Date) -> String {
        let formatter = DateFormatter()
        formatter.dateStyle = .medium
        return formatter.string(from: date)
    }
    
   
    private func scrollToBottom(proxy: ScrollViewProxy) {
        if let lastId = viewModel.readMessages.last?.messageIndex {
            print("scrollToBottom to id:", lastId)
            proxy.scrollTo(lastId, anchor: .bottom)
        }
    }
    
 
    // MARK: Init
    init(conversation: PersistentConversationDataItem, viewModel: MessageListViewModel) {
        self.conversation = conversation
        self.viewModel = viewModel
    }
    
    /**
     Conversation is new if there are no messages & only has 1 participant
     When a new conversation is created, it has 1 participant by default, that is who created that conversation.
     */
    private func isConversationNew() -> Bool {
        return appModel.messagesManager.messages.count == 0 && (appModel.selectedConversation?.participantsCount ?? 0) < 2
    }
  
    private func handleOnAppear() {
        print("messagesManager = \(String(describing: appModel.messagesManager))")
        print("messagesManager", messagesManager)
       
        print(" My Identity at onAppear: \(AppModel.shared.myIdentity)")
        
        // Continue all your setup logic here, like:
        messagesManager.subscribeMessages(inConversation: conversation)
        participantsManager.subscribeParticipants(inConversation: conversation)
        
        messagesManager.$messages
            .sink { messages in
                viewModel.prepareMessages(conversation, messages, participantsManager.participants)
            }
            .store(in: &cancellableSet)
        
        participantsManager.$participants
            .sink { _ in
                viewModel.prepareMessages(conversation, messagesManager.messages, participantsManager.participants)
            }
            .store(in: &cancellableSet)
        
       messagesManager.loadLastMessagePageIn(
            conversation,
            max: max(messagesPaginationSize, UInt(conversation.unreadMessagesCount))
        )
        
        let parsedConversation = PersistanceDataAdapter.transform(from: conversation)
       
        viewModel.setConversation(parsedConversation)
        
        //        appModel.typingPublisher
        //            .sink { typing in viewModel.registerForTyping(typing) }
        //            .store(in: &cancellableSet)
        
        conversationManager.conversationEventPublisher
            .sink { event in viewModel.registerForConversationEvents(event) }
            .store(in: &cancellableSet)
    }
    
    private func sendHeartbeat() {
        let sid: String = conversation.sid ?? ""
        let user = UserDefaults.standard.string(forKey: "patientUserNameString") ?? "UnknownUser"
        
        let formatter = DateFormatter()
        formatter.dateFormat = "M/d/yyyy, h:mm:ss a ZZZ"
        let timestamp: String = formatter.string(from: Date())
        
        viewModel.updateUserActivity(
            conversationSid: sid,
            userName: user,
            lastActiveAt: timestamp
        )
    }
  
}


struct MessageInputBar: View {
    @Binding var textToSend: String
    @ObservedObject var viewModel: MessageListViewModel // Your actual view model type
    var appModel: AppModel
    var conversation: PersistentConversationDataItem
    @Binding var showingPickerSheet: Bool
    @State private var textEditorHeight: CGFloat = 110
    
    
    var body: some View {
        VStack {

            HStack(alignment: .center) {
                Button(action: {
                    showingPickerSheet = true
                }) {
                    // Image("addAttachment")
                }
                .padding(.leading, 16)
                
                HStack(alignment: .bottom, spacing: 5) {
                    ZStack(alignment: .topLeading) {
                 
                        // Placeholder
                           if textToSend.isEmpty {
                               Text("Type Your Message")
                                   .foregroundColor(.gray)
                                        .font(.system(size: 17)) // Match UITextView font
                                        .frame(maxWidth: .infinity, alignment: .leading)
                                        .padding(.horizontal, 16)
                                        .padding(.top, 14)      // Small top padding to compensate UITextView inset
                                        .padding(.bottom, 6)    // Less bottom padding to balance
                                        .frame(height: textEditorHeight, alignment: .topLeading)
                                        .frame(maxWidth: 290)
                                        .zIndex(1)
                           }
                  
                        GrowingTextEditor(
                            text: $textToSend,
                            dynamicHeight: $textEditorHeight,
                            maxHeight: 110 // Approx height for 5 lines
                        )
                        .frame(height: textEditorHeight)
                        .padding(.top, 10)
                        .padding(.bottom, 10)
                        .padding(.horizontal, 10)
                        .background(Color.white)
                        .overlay(
                            RoundedRectangle(cornerRadius: 14)
                                .stroke(Color.gray.opacity(0.3), lineWidth: 1)
                        )
                        .cornerRadius(14)
                        .frame(maxWidth: 290)
                      
                        .zIndex(0) // Ensure TextEditor is below the placeholder
                        .onAppear {
                            UITextView.appearance().backgroundColor = .clear
                        }
                        .onChange(of: textToSend) { _ in
                            appModel.typing(in: appModel.selectedConversation)
                        }
                     
                    }
                    
                    
                    // Send button
                    Button(action: {
                        appModel.messagesManager.sendMessage(
                            toConversation: conversation,
                            withText: textToSend,
                            andMedia: viewModel.selectedImageURL,
                            withFileName: viewModel.selectedFileName
                        )
                        
                        { _ in
                          
                            let sentText = textToSend
                            textToSend = ""
                            let sid = conversation.sid ?? ""
                            let toUser = appModel.selectedConversation?.title.components(separatedBy: "-").first ?? ""
                            let fromUser = UserDefaults.standard.string(forKey: "patientUserNameString") ?? "UnknownUser"
                            viewModel.notifyMessageSent(conversationSid: sid, toUser: toUser, fromUser: fromUser, message: sentText)
                            viewModel.clearSelectedImage()
                        }
                    }) {
                        Image("Icons_Send")
                            .resizable()
                            .frame(width: 28, height: 28) // Optional: control icon size
                            .padding(.vertical,10)
                            .padding(.horizontal,8)
                            .background(Color.white) // White background
                            .clipShape(Circle())     // Circular shape
                            .shadow(radius: 1)       // Optional: subtle shadow for depth
                    }
                  
                    .padding(.trailing, 16)
                    .padding(.leading, 8)// Add margin on the right side of the Send button
                }
                
            }
            .padding(.top, 8) // Adding top padding to create space above the text editor
        }
        .frame(maxWidth: .infinity)
        .padding(.bottom, 15)
        .background(Color("TFbgColor"))
    }
}

struct GrowingTextEditor: UIViewRepresentable {
    @Binding var text: String
    @Binding var dynamicHeight: CGFloat
    let maxHeight: CGFloat
    
    func makeUIView(context: Context) -> UITextView {
        let textView = UITextView()
        textView.font = UIFont.systemFont(ofSize: 17)
        textView.isScrollEnabled = false
        textView.backgroundColor = .clear
        textView.delegate = context.coordinator
        textView.setContentCompressionResistancePriority(.defaultLow, for: .horizontal)
        return textView
    }
    
    func updateUIView(_ uiView: UITextView, context: Context) {
        if uiView.text != text {
            uiView.text = text
        }
        
        // Only calculate after layout
        DispatchQueue.main.async {
            self.recalculateHeight(view: uiView)
        }
    }
    
    
    func makeCoordinator() -> Coordinator {
        return Coordinator(parent: self)
    }
    
    private func recalculateHeight(view: UITextView) {
        // Avoid recalculating height with zero width
        guard view.bounds.width > 0 else {
            DispatchQueue.main.async {
                self.dynamicHeight = self.maxHeight // fallback to max
            }
            return
        }
        
        let size = view.sizeThatFits(CGSize(width: view.bounds.width, height: .infinity))
        DispatchQueue.main.async {
            self.dynamicHeight = min(size.height, maxHeight)
            view.isScrollEnabled = size.height > maxHeight
        }
    }
    
    
    class Coordinator: NSObject, UITextViewDelegate {
        var parent: GrowingTextEditor
        
        init(parent: GrowingTextEditor) {
            self.parent = parent
        }
        
        func textViewDidChange(_ textView: UITextView) {
            self.parent.text = textView.text
            self.parent.recalculateHeight(view: textView)
        }
    }
}


struct ScrollToBottomButton: View {
    var action: () -> Void
    
    var body: some View {
        Button(action: action) {
            Image(systemName: "chevron.down.circle.fill")
                .resizable()
                .frame(width: 45, height: 45)
                .foregroundColor(Color("title1"))
                .background(Color.white.opacity(0.8))
                .clipShape(Circle())
                .shadow(radius: 4)
        }
        .padding()
    }
}


// Preference key to track scroll position
struct ScrollOffsetPreferenceKey: PreferenceKey {
    typealias Value = CGFloat
    static var defaultValue: CGFloat = 0
    
    static func reduce(value: inout CGFloat, nextValue: () -> CGFloat) {
        // Use the minimum value so that a negative offset from the first view is preserved.
        value = min(value, nextValue())
       // print(" ScrollOffsetPreferenceKey reduce triggered — new value: \(value)")
    }
}

struct UnreadSectionHeaderView: View {
    var unreadMessagesCount: Int
    var scrollToBottom: (() -> Void)?  //  Add this property
    
    var body: some View {
        HStack() {
            Text(String(format: NSLocalizedString("New Messages", comment: ""), "\(unreadMessagesCount)"))
                .frame(maxWidth: .infinity, alignment: .center)
                .font(.system(size: 14, weight: .bold))
                .foregroundColor(Color.green)
            
            // Drop-down arrow button
            if let scrollToBottom = scrollToBottom {  //  Unwrap before using
                Button(action: {
                    withAnimation {
                        scrollToBottom()
                    }
                }) {
                    Image(systemName: "chevron.down.circle.fill")
                        .foregroundColor(.green)
                        .font(.title2)
                }
            }
         
            Spacer()
        }
        .padding(.horizontal, 0)
        .padding(.vertical, 5)
        .background(Color.gray)
    }
}

@ViewBuilder private func getStatusBanner(event: ConversationEvent?) -> some View {
    if event == .messageCopied {
        withAnimation {
            GlobalStatusView(message: NSLocalizedString("conversation.status.message_copied", comment: "Notification indicating that the message was successfully copied"), kind: .success)
        }
    } else if event == .messageDeleted {
        withAnimation {
            GlobalStatusView(message: NSLocalizedString("Deleted Message", comment: "Notification indicating that the message was successfully deleted"), kind: .success)
        }
    } else {
        EmptyView()
    }
}

// MARK: Preview
struct MessageListView_Previews: PreviewProvider {
    static var previews: some View {
        let appModel = AppModel(inMemory: true)
        let items: [PersistentConversationDataItem.Decode] = load("testConversations.json")
        let messageListViewModel = MessageListViewModel()
        
        MessageListView(conversation: items[0].conversation(inContext: appModel.getManagedContext()), viewModel: messageListViewModel)
            .environmentObject(appModel)
            .environmentObject(appModel.conversationManager)
            .environmentObject(appModel.messagesManager)
            .environmentObject(appModel.participantsManager)
    }
}
