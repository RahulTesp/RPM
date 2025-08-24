//
//  ParticipantViewWrapper.swift
//  RPM
//
//  Created by Tesplabs on 01/07/25.
//

import Combine

class ParticipantViewWrapper: ObservableObject {
    @Published var model: ParticipantViewModel

    init(_ model: ParticipantViewModel) {
        self.model = model
    }
}

