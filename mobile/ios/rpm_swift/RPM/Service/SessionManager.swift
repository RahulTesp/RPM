////
////  SessionManager.swift
////  RPM
////
////  Created by Tesplabs on 29/05/25.
////
//
//





import Foundation
import Combine
import SwiftUI

class SessionManager: ObservableObject {
    
    static let shared = SessionManager()
    
    @Published var didReceiveUnauthorized: Bool = false
    
    private init() {}
    
    // MARK: - Token Handling
    
    private var token: String? {
        UserDefaults.standard.string(forKey: "jsonwebtoken")
    }
    
    /// Checks if current JWT token is valid (not expired)
    func isTokenValid() -> Bool {
        guard let token = token else { return false }
        let parts = token.split(separator: ".")
        guard parts.count == 3 else { return false }
        
        let payloadPart = parts[1]
        guard let payloadData = Data(base64Encoded: padBase64(String(payloadPart))) else { return false }
        
        if let json = try? JSONSerialization.jsonObject(with: payloadData, options: []),
           let dict = json as? [String: Any],
           let exp = dict["exp"] as? TimeInterval {
            // Compare with current time
            return Date().timeIntervalSince1970 < exp
        }
        return false
    }
    
    /// Pads base64 string if needed
    private func padBase64(_ str: String) -> String {
        var s = str
        let pad = 4 - s.count % 4
        if pad < 4 { s += String(repeating: "=", count: pad) }
        return s
    }
    
    // MARK: - Auto Logout
    
    /// Force logout if token expired
    func logoutIfTokenExpired() {
        if !isTokenValid() {
            print("Token expired locally, triggering logout")
            DispatchQueue.main.async {
                self.didReceiveUnauthorized = true
                print("Token expired locally,didReceiveUnauthorized", self.didReceiveUnauthorized)
            }
        }
    }
    
    @MainActor
    func logout(appModel: AppModel,
                homeViewModel: RPMHomeViewModel,
                navigationHelper: NavigationHelper,
                loginViewModel: RPMLoginViewModel,
                callManager: CallManager,
                roomManager: RoomManager) {
        
        print("Logging out from SessionManager...")

        homeViewModel.reset()

        loginViewModel.logout { response, alert in
            print("Logout server call finished")
        }

        // Always cleanup
        appModel.signOutChat()
        callManager.disconnect()
        roomManager.disconnect()
        navigationHelper.path = []
        loginViewModel.isAuthenticated = false
    }
    
    // MARK: - API Error Handling
    
    func handleAPIError(_ error: Error) {
        // Handle URL errors
        if let urlError = error as? URLError {
            print("URLError:", urlError)
        }
        
        // Handle your APIError enum
        if let apiError = error as? APIError {
            switch apiError {
            case .unauthorized:
                print("Received 401 from API, triggering logout")
                DispatchQueue.main.async {
                    self.didReceiveUnauthorized = true
                }
            default:
                break
            }
        }
        
        // Fallback: Check HTTP 401 in NSError userInfo
        if let urlResponse = (error as NSError).userInfo["HTTPURLResponse"] as? HTTPURLResponse,
           urlResponse.statusCode == 401 {
            print("HTTP 401 detected, triggering logout")
            DispatchQueue.main.async {
                self.didReceiveUnauthorized = true
            }
        }
    }
    
        func handleUnauthorizedResponse() {
            DispatchQueue.main.async {
                self.didReceiveUnauthorized = true
            }
        }
    
    // MARK: - Scene / Foreground Handling
    
    func checkSessionOnForeground() {
        logoutIfTokenExpired()
    }
    
    // MARK: - Reset
    
    func reset() {
        didReceiveUnauthorized = false
    }
}
