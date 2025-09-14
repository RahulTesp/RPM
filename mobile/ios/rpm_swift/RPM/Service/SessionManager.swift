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

    func handleAPIError(_ error: Error) {
        // Check if error is due to expired session
        if let urlError = error as? URLError {
            print("URLError:", urlError)
        }

        if let apiError = error as? APIError {
            switch apiError {
            case .unauthorized:
                DispatchQueue.main.async {
                    self.didReceiveUnauthorized = true
                }
            default:
                break
            }
        }

        // Alternatively, use a generic check for HTTP 401
        if let urlResponse = (error as NSError).userInfo["HTTPURLResponse"] as? HTTPURLResponse,
           urlResponse.statusCode == 401 {
            DispatchQueue.main.async {
                self.didReceiveUnauthorized = true
            }
        }
    }
    
    func reset() {
        didReceiveUnauthorized = false
    }
}

