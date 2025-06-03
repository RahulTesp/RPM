//
//  SessionManager.swift
//  RPM
//
//  Created by Tesplabs on 29/05/25.
//


import Foundation
import Combine

class SessionManager: ObservableObject {
    static let shared = SessionManager()
    
    @Published var didReceiveUnauthorized: Bool = false

    private init() {}

    func handleUnauthorizedResponse() {
        DispatchQueue.main.async {
            self.didReceiveUnauthorized = true
        }
    }

    func reset() {
        didReceiveUnauthorized = false
    }
}

