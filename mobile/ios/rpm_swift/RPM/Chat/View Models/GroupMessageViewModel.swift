//
//  GroupMessageViewModel.swift
//  RPM
//
//  Created by Tesplabs on 14/02/24.
//


import Foundation

class GroupedMessageViewModel: ObservableObject, Identifiable {
    @Published var date: Date
    @Published var messages: [MessageBubbleViewModel]

    init(date: Date, messages: [MessageBubbleViewModel]) {
        self.date = date
        self.messages = messages
    }
}
