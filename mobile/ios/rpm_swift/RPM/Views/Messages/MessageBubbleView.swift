//
//  MessageBubbleCell.swift
//  ConversationsApp
//
//  Copyright © Twilio, Inc. All rights reserved.
//

import SwiftUI

struct MessageBubbleView: View {
    
    @ObservedObject var viewModel: MessageBubbleViewModel
    @EnvironmentObject var appModel: AppModel
    @EnvironmentObject var messagesManager: MessagesManager
    
    @State private var whichSheet: ShowWhichDetails = .message
    @State private var showingMessageDetailsSheet = false
    @State private var showingReactionsDetailsView = false
    @State private var showingDeleteConfirmation = false
    @State private var showingFileAttachment = false
    
    @State private var isMessageSelected: Bool = false
    
    
    var body: some View {
        HStack(alignment: .top) {
            if viewModel.direction == .incoming {
                VStack {
                    Spacer().frame(height: 24)
                }
                .padding(.leading, 16)
                
                
                ZStack(alignment: .bottomLeading) {
                    VStack(alignment: .leading) {
                        
                        ZStack(alignment: .topLeading) {
                            ChatBubble()
                                .fill(Color("ColorGreen"))
                            
                            VStack(alignment: .leading, spacing: 4) {
                                if viewModel.contentCategory != .text {
                                    // Handle non-text content here if needed
                                }
                                
                                MessageDateView(viewModel, isInbound: true)
                                MessageTextView(viewModel, isMessageSelected: $isMessageSelected)
                            }
                            .padding(EdgeInsets(top: 16, leading: 20, bottom: 12, trailing: 16)) // Make room inside bubble
                        }
                        .frame(maxWidth: UIScreen.main.bounds.width * 0.5, alignment: .leading)
                        
                    }
                    .padding(.bottom, 4)
                 
                }
                .padding(.bottom, 12)
                Spacer()
            }
            
            
            else { // Outgoing Message
                VStack
                {
                    
                    HStack {
                   
                        
                        Spacer() // Pushes the entire message bubble to the right
                        
                        HStack(alignment: .bottom, spacing: 8) {
                            
                            //  Status Icon
                            if let status = viewModel.messageStatus {
                                statusIcon(for: status)
                                    .padding(.bottom, 4)
                            }
  
                            ZStack(alignment: .topLeading) {
                                ChatBubbleRight()
                                    .fill(Color("ChatBoxColour"))
                                
                                VStack(alignment: .leading) {
                                    if viewModel.contentCategory != .text {
                                        // MediaAttachmentView(viewModel)
                                    }
                                    
                                    MessageDateView(viewModel, isInbound: false)
                                    MessageTextView(viewModel, isMessageSelected: $isMessageSelected)
                                        .environmentObject(messagesManager)
                                }
                                .padding(EdgeInsets(top: 16, leading: 10, bottom: 12, trailing: 16)) // Make room inside bubble
                            }
                            .frame(maxWidth: UIScreen.main.bounds.width * 0.5, alignment: .trailing)
                            .padding(.bottom, 4)
                            
                            
                        }
                    }
                    .frame(maxWidth: .infinity, alignment: .trailing) // Ensures entire HStack is pushed right
                    .padding(.trailing, 8) // Ensures alignment to the right edge
                }
            }
            
        }
        
        .padding(.bottom, 4)
        .onLongPressGesture {
            whichSheet = .message
            showingMessageDetailsSheet.toggle()
        }
        

    }
    
    @ViewBuilder
    func statusIcon(for status: MessageStatus) -> some View {
        switch status {
        case .sending:
            Image(systemName: "clock")
                .foregroundColor(.gray)
                .font(.caption2)

        case .sent:
            Image(systemName: "checkmark.circle.fill")
                .foregroundColor(.green)
                .font(.caption2)

        case .failed:
            Image(systemName: "exclamationmark.circle.fill")
                .foregroundColor(.red)
                .font(.caption2)
        }
    }

    // MARK: - Actions

    func copyAction() {
        messagesManager.copyMessage()
    }
    
    func deleteAction() {
        showingDeleteConfirmation.toggle()
    }
}

fileprivate enum ShowWhichDetails {
    case message, reactions
}

struct PlaceholderImage: View {
    var body: some View {
        Image(systemName: "questionmark.app")
            .resizable()
            .frame(width: 64, height: 64)
            .scaledToFit()
    }
}

struct MessageTextView: View {
    @ObservedObject var viewModel: MessageBubbleViewModel
    @EnvironmentObject var appModel: AppModel
    @EnvironmentObject var messagesManager: MessagesManager
    @State private var editedText: String = ""
    @State private var isEditing: Bool = false
    @FocusState private var isTextEditorFocused: Bool
    @State private var showDeleteConfirmation = false

    var isDeleting: Bool {
        messagesManager.isDeleting(viewModel.source)
    }
    
    @Binding var isMessageSelected: Bool // Binding to control visibility of Edit/Delete buttons

    var body: some View {
        
        ZStack {
            
            if viewModel.contentCategory == .text {
                
                if viewModel.direction == .outgoing {
                    HStack(alignment: .bottom, spacing: 8) {
                        Spacer() // Push everything to the right
                 
                        HStack {
                            Spacer() // Pushes text to the left, icons stay on the right
                            
                            VStack(spacing: 8) {
                                ZStack(alignment: .topTrailing) {
                                    if isEditing {
                                        VStack(alignment: .leading) {
                                            TextEditor(text: $editedText)
                                                .focused($isTextEditorFocused)
                                                .frame(height: 100)
                                                .overlay(RoundedRectangle(cornerRadius: 8).stroke(Color.gray))
                                                .padding(.bottom, 8)
                                                .onAppear {
                                                     isTextEditorFocused = true
                                                 }

                                            HStack {
                                                Button("Cancel") {
                                                    isEditing = false
                                                }
                                                .foregroundColor(.red)
                                                
                                                Spacer()
                                                
                                                Button("Save") {
                                                    print("Save tapped")
                                                    let messageIndex = viewModel.source.messageIndex
                                                    let conversationSid = viewModel.source.conversationSid
                                                    
                                                    if let sid = conversationSid {
                                                        messagesManager.updateMessageBody(
                                                            newText: editedText,
                                                            for: messageIndex,
                                                            conversationSid: sid
                                                        )
                                                        viewModel.source.body = editedText
                                                        isEditing = false
                                                    }
                                                }
                                                .foregroundColor(
                                                    editedText.trimmingCharacters(in: .whitespacesAndNewlines).isEmpty
                                                    ? .gray
                                                    : .blue
                                                )
                                                
                                                .disabled(editedText.trimmingCharacters(in: .whitespacesAndNewlines).isEmpty)
                                            }
                                        }
                                    } else {
                                        HStack(alignment: .top) {
                                            Text(viewModel.source.body ?? viewModel.text.string)
                                                .padding(.vertical, 2)
                                                .foregroundColor(Color("ChatLastMsgColor"))
                                            Spacer()
                                            
                                            if isMessageSelected { // Only show icons when the message is selected
                                                HStack(spacing: 8) {
                                                    Button(action: {
                                                        print("Edit tapped")
                                                        editedText = viewModel.source.body ?? viewModel.text.string
                                                    
                                                            isEditing = true
     
                                                    }) {
                                                        Image("Icons_Edit A")
                                                            .resizable()
                                                            .frame(width: 16, height: 16)
                                                            .foregroundColor(.white)
                                                            .padding(6)
                                                            .clipShape(Circle())
                                                    }
                                                    
                                                    Button(action: {
                                                        showDeleteConfirmation = true
                                                    }) {
                                                        Image("Icons_Delete A")
                                                            .resizable()
                                                            .frame(width: 16, height: 16)
                                                            .foregroundColor(.white)
                                                            .padding(6)
                                                            .clipShape(Circle())
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        .background(RoundedRectangle(cornerRadius: 8).fill(Color("ChatBoxColour")))
                        .padding(2)
                        .frame(maxWidth: UIScreen.main.bounds.width * 0.6, alignment: .leading) // Keeps everything aligned
                        .onTapGesture {
                            // Toggle the visibility of the edit/delete buttons
                            withAnimation {
                                isMessageSelected.toggle()
                            }
                        }
                    }
                    .frame(maxWidth: .infinity, alignment: .trailing) // Ensures entire HStack is right aligned
                    .padding(.trailing, 4)
                    
                    HStack{
                        if isDeleting {
                             
                               Text("Deleting...")
                                   .foregroundColor(.red)
                                   .bold()
                           }
                           
                    } .padding(.top, 2)
                }
                
                
                else {
                    Text(.init(viewModel.text))
                        .padding(EdgeInsets(top: 8, leading: 8, bottom: 8, trailing: 8))
                        .foregroundColor(Color("ChatLastMsgColor"))
                    
                        .accentColor(Color.blue)
                        .font(.system(size: 16))
                        .contextMenu {
                            Button(action: {
                                UIPasteboard.general.string = viewModel.text.string // Convert to String
                            }) {
                                Label("Copy", systemImage: "doc.on.doc")
                            }
                        }
                }
            }
            
        }
        .confirmationDialog("Delete Message?", isPresented: $showDeleteConfirmation, titleVisibility: .visible) {
            Button("Delete", role: .destructive) {
                let message = viewModel.source
                
                print("UIMSGINDEX",message.messageIndex)
                print("UImessage",message)
                
                messagesManager.deletingMessages.insert(message.sid ?? "")
           
                messagesManager.deleteMessageFromTwilio(message) { success in
                
                    messagesManager.deletingMessages.remove(message.sid ?? "")
                    
                    if success {
                      //   Update UI, show confirmation, etc.
                        print("Deletion success")

                    } else {
                        // Show error UI
                        print("Deletion failed")
                    }
                }

            }
            Button("Cancel", role: .cancel) { }
        }

    }

    init(_ model: MessageBubbleViewModel, isMessageSelected: Binding<Bool>) {
        self.viewModel = model
        self._isMessageSelected = isMessageSelected // Note the underscore (_) prefix here
    }
    
}

struct ChatBubble: Shape {
    func path(in rect: CGRect) -> Path {
        let cornerRadius: CGFloat = 12
        let arrowWidth: CGFloat = 16
        let arrowHeight: CGFloat = 16
        
        var path = Path()
        
        // Start at top-left after triangle
        path.move(to: CGPoint(x: arrowWidth + cornerRadius, y: 0))
        
        // Top edge to top-right (rounded)
        path.addLine(to: CGPoint(x: rect.width - cornerRadius, y: 0))
        path.addArc(center: CGPoint(x: rect.width - cornerRadius, y: cornerRadius),
                    radius: cornerRadius,
                    startAngle: .degrees(270),
                    endAngle: .degrees(0),
                    clockwise: false)
        
        // Right edge
        path.addLine(to: CGPoint(x: rect.width, y: rect.height - cornerRadius))
        path.addArc(center: CGPoint(x: rect.width - cornerRadius, y: rect.height - cornerRadius),
                    radius: cornerRadius,
                    startAngle: .degrees(0),
                    endAngle: .degrees(90),
                    clockwise: false)
        
        // Bottom edge
        path.addLine(to: CGPoint(x: cornerRadius, y: rect.height))
        path.addArc(center: CGPoint(x: cornerRadius, y: rect.height - cornerRadius),
                    radius: cornerRadius,
                    startAngle: .degrees(90),
                    endAngle: .degrees(180),
                    clockwise: false)
        
        // Left edge up to the triangle base
        path.addLine(to: CGPoint(x: 0, y: arrowHeight + cornerRadius))
        
        // Triangle pointer
        path.addLine(to: CGPoint(x: 0, y: arrowHeight / 2))
        path.addLine(to: CGPoint(x: -arrowWidth, y: arrowHeight / 2))
        path.addLine(to: CGPoint(x: 0, y: 0))
        
        // Join back to top-left corner
        path.addLine(to: CGPoint(x: arrowWidth + cornerRadius, y: 0))
        
        return path
    }
}

struct ChatBubbleRight: Shape {
    func path(in rect: CGRect) -> Path {
        let cornerRadius: CGFloat = 12
        let arrowWidth: CGFloat = 16       // Length of tail (how far it sticks out)
        let arrowHeight: CGFloat = 16      // Height of the triangle
        let arrowBaseOffset: CGFloat = 8   // NEW: how wide the base is (was full arrowHeight before)

        var path = Path()

        // Start from top-left corner
        path.move(to: CGPoint(x: cornerRadius, y: 0))

        // Top edge to just before triangle
        path.addLine(to: CGPoint(x: rect.width - arrowWidth - cornerRadius, y: 0))

        // Top-right corner curve
        path.addArc(center: CGPoint(x: rect.width - arrowWidth - cornerRadius, y: cornerRadius),
                    radius: cornerRadius,
                    startAngle: .degrees(270),
                    endAngle: .degrees(0),
                    clockwise: false)

        // Right edge down
        path.addLine(to: CGPoint(x: rect.width - arrowWidth, y: rect.height - cornerRadius))
        path.addArc(center: CGPoint(x: rect.width - arrowWidth - cornerRadius, y: rect.height - cornerRadius),
                    radius: cornerRadius,
                    startAngle: .degrees(0),
                    endAngle: .degrees(90),
                    clockwise: false)

        // Bottom edge
        path.addLine(to: CGPoint(x: cornerRadius, y: rect.height))
        path.addArc(center: CGPoint(x: cornerRadius, y: rect.height - cornerRadius),
                    radius: cornerRadius,
                    startAngle: .degrees(90),
                    endAngle: .degrees(180),
                    clockwise: false)

        // Left edge up
        path.addLine(to: CGPoint(x: 0, y: cornerRadius))
        path.addArc(center: CGPoint(x: cornerRadius, y: cornerRadius),
                    radius: cornerRadius,
                    startAngle: .degrees(180),
                    endAngle: .degrees(270),
                    clockwise: false)

        // Triangle tail (top-right, with slimmer base)
        let triangleTop = CGPoint(x: rect.width - arrowWidth, y: 0)
        let triangleTip = CGPoint(x: rect.width, y: arrowHeight / 2)
        let triangleBottom = CGPoint(x: rect.width - arrowWidth, y: arrowBaseOffset)

        path.addLine(to: triangleTop)
        path.addLine(to: triangleTip)
        path.addLine(to: triangleBottom)

        // Continue downward after tail to rejoin rectangle
        path.addLine(to: CGPoint(x: rect.width - arrowWidth, y: arrowBaseOffset + cornerRadius))

        return path
    }
}

struct MessageDateView: View {
    private var viewModel: MessageBubbleViewModel
    private var isIncoming: Bool
    
    var body: some View {
        if (isIncoming) {    //Incoming message date
            Text(viewModel.formattedDate)
                .lineLimit(1)
                .foregroundColor(Color("title1"))
                .font(.system(size: 12))
                .padding(EdgeInsets(top: 4, leading: 8, bottom: 4, trailing: 6))
        } else {    //Outgoing message date
            Text(viewModel.formattedDate)
                .lineLimit(1)
                .foregroundColor(Color("title1"))
            
                .font(.system(size: 12))
                .padding(EdgeInsets(top: 4, leading: 8, bottom: 4, trailing: 6))
        }
    }
    
    init(_ model: MessageBubbleViewModel, isInbound: Bool) {
        self.viewModel = model
        self.isIncoming = isInbound
    }
}
