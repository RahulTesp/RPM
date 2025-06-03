//
//  ConversationsEmptyView.swift
//  ConversationsApp
//
//  Created by Robert Ziehl on 2022-05-03.
//  Copyright © 2022 Twilio, Inc. All rights reserved.
//

import Foundation
import SwiftUI

struct ConversationsEmptyView: View {
    @Binding var showingCreateConversationSheet: Bool
    
    var body: some View {
        VStack(spacing: 0) {
            Spacer()
            Text("No Chats")
                .font(.system(size: 20, weight: .bold))
                .foregroundColor(.blue)
            Spacer()
        }
    }
}
