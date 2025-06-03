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
                        .frame(maxWidth: UIScreen.main.bounds.width * 0.6, alignment: .leading)
                        
                    }.padding(.bottom, 4)
                    HStack {
                        //                        TappableReactionView(viewModel: viewModel, showingReactionsDetailsView: $showingReactionsDetailsView, showingDetailSheet: $showingMessageDetailsSheet, whichSheet: $whichSheet)
                        Spacer()
                    }
                }
                .padding(.bottom, 22)
                Spacer()
            }
            
            
            else { // Outgoing Message
                VStack
                {
                    
                    HStack {
                        
                        
                        
                        Spacer() // Pushes the entire message bubble to the right
                        
                        HStack(alignment: .bottom, spacing: 8) {
                            
                            ZStack(alignment: .topLeading) {
                                ChatBubbleRight()
                                    .fill(Color("ChatBoxColour"))
                                
                                VStack(alignment: .leading) {
                                    if viewModel.contentCategory != .text {
                                        // MediaAttachmentView(viewModel)
                                    }
                                    
                                    MessageDateView(viewModel, isInbound: false)
                                    MessageTextView(viewModel, isMessageSelected: $isMessageSelected)
                                }
                                .padding(EdgeInsets(top: 16, leading: 10, bottom: 12, trailing: 16)) // Make room inside bubble
                            }
                            .frame(maxWidth: UIScreen.main.bounds.width * 0.6, alignment: .trailing)
                            .padding(8)
                            
                            
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
        
        .alert(isPresented: $showingDeleteConfirmation) {
            Alert(
                title: Text("message.details.delete_title"),
                message: Text("message.details.delete_description"),
                primaryButton: .default(
                    Text("Cancel"),
                    action: {}
                ),
                secondaryButton: .destructive(
                    Text("message.details.delete"),
                    action: { messagesManager.deleteMessage(viewModel.source) }
                )
            )
        }
    }
    
    // MARK: - Actions
    
    // func tapReactionAction() {
    //        messagesManager.updateMessage(attributes: viewModel.source.attributesDictionary, for: viewModel.source.messageIndex, conversationSid: viewModel.source.conversationSid)
    // }
    
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
    // private var viewModel: MessageBubbleViewModel
    @ObservedObject var viewModel: MessageBubbleViewModel
    @EnvironmentObject var appModel: AppModel
    
    @State private var editedText: String = ""
    @State private var isEditing: Bool = false
    @FocusState private var isTextEditorFocused: Bool
    @State private var showDeleteConfirmation = false
    
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
                                                    DispatchQueue.main.asyncAfter(deadline: .now() + 0.1) {
                                                        isTextEditorFocused = true
                                                    }
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
                                                        appModel.messagesManager.updateMessageBody(
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
                                                        
                                                        withAnimation(nil) {
                                                            isEditing = true
                                                        }
                                                        
                                                        DispatchQueue.main.asyncAfter(deadline: .now() + 0.1) {
                                                            isTextEditorFocused = true
                                                        }
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
                appModel.messagesManager.deleteMessage(message)
            }
            Button("Cancel", role: .cancel) { }
        }
        
        
    }
    
    //    init(_ model: MessageBubbleViewModel) {
    //        self.viewModel = model
    //    }
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
        let arrowWidth: CGFloat = 12
        let arrowHeight: CGFloat = 10
        
        var path = Path()
        
        // Start from top-left corner (rounded)
        path.move(to: CGPoint(x: cornerRadius, y: 0))
        path.addArc(center: CGPoint(x: cornerRadius, y: cornerRadius),
                    radius: cornerRadius,
                    startAngle: .degrees(270),
                    endAngle: .degrees(180),
                    clockwise: true)
        
        // Left edge
        path.addLine(to: CGPoint(x: 0, y: rect.height - cornerRadius))
        path.addArc(center: CGPoint(x: cornerRadius, y: rect.height - cornerRadius),
                    radius: cornerRadius,
                    startAngle: .degrees(180),
                    endAngle: .degrees(90),
                    clockwise: true)
        
        // Bottom edge
        path.addLine(to: CGPoint(x: rect.width - arrowWidth - cornerRadius, y: rect.height))
        path.addArc(center: CGPoint(x: rect.width - arrowWidth - cornerRadius, y: rect.height - cornerRadius),
                    radius: cornerRadius,
                    startAngle: .degrees(90),
                    endAngle: .degrees(0),
                    clockwise: true)
        
        // Right edge
        path.addLine(to: CGPoint(x: rect.width - arrowWidth, y: arrowHeight + cornerRadius))
        
        // Triangle (tail) on top-right
        path.addLine(to: CGPoint(x: rect.width, y: arrowHeight / 2))
        path.addLine(to: CGPoint(x: rect.width - arrowWidth, y: 0))
        
        // Skip rounding top-right, connect back to top-left edge
        path.addLine(to: CGPoint(x: cornerRadius, y: 0))
        
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
                .padding(EdgeInsets(top: 6, leading: 8, bottom: 5, trailing: 6))
        } else {    //Outgoing message date
            Text(viewModel.formattedDate)
                .lineLimit(1)
                .foregroundColor(Color("title1"))
            
                .font(.system(size: 12))
                .padding(EdgeInsets(top: 6, leading: 8, bottom: 5, trailing: 6))
        }
    }
    
    init(_ model: MessageBubbleViewModel, isInbound: Bool) {
        self.viewModel = model
        self.isIncoming = isInbound
    }
}
