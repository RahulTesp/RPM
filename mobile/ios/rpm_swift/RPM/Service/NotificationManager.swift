//
//  NotificationManager.swift
//  RPM
//
//  Created by Tesplabs on 10/03/25.
//


import Combine
import Foundation
import UserNotifications

class NotificationManager: NSObject, ObservableObject, UNUserNotificationCenterDelegate {
    
    static let shared = NotificationManager()
    @Published var showAlert: Bool = false  // Controls alert visibility
    @Published var notificationTitle: String = ""  // Stores the notification title
    @Published var notificationBody: String = ""
    @Published var roomName: String?  // Room name for navigation
    @Published var callRejectToUserName: String = ""
    @Published var callRejectTokenId: String = ""
    var lastNotificationIdentifier: String? // Store last notification ID
    // Declare globally to track last notification ID
    var lastProcessedNotificationID: String?
    private var shownSessionIds = Set<String>()
    private var alertDismissTimer: Timer?


    override init() {
        super.init()
        print("currentdelegate")
    }
    
    func handle(notification: UNNotification) {
        let userInfo = notification.request.content.userInfo
        print("HandlE Foreground Noti")

        if let title = userInfo["Title"] as? String,
           title.lowercased().contains("message") {
            // Detected chat message
            print(" Chat message detected, refreshing UI")
            NotificationCenter.default.post(name: Notification.Name("RefreshNotifications"), object: nil)
        }

    }

    
    func handleForegroundNotification(title: String, body: String, userInfo: [AnyHashable: Any]) {
        print("Handling Foreground Notification")
        NotificationCenter.default.post(name: Notification.Name("RefreshNotifications"), object: nil)

        var extractedRoomName = ""
        var callRejectToUserName = ""
        var callRejectTokenId = ""
        var shouldShowAlert = false

        if let atSymbolIndex = body.firstIndex(of: "@"),
           let hashSymbolIndex = body.firstIndex(of: "#") {

            let beforeAt = body[..<atSymbolIndex]
            extractedRoomName = String(beforeAt)

            let tokenStart = body.index(after: atSymbolIndex)
            let tokenEnd = body.index(before: hashSymbolIndex)
            callRejectTokenId = String(body[tokenStart...tokenEnd])

            let userNameParts = extractedRoomName.split(separator: "_", maxSplits: 2)
            if userNameParts.count >= 2 {
                callRejectToUserName = "\(userNameParts[0])_\(userNameParts[1])"
            }

            let afterHash = body[body.index(after: hashSymbolIndex)...]
            shouldShowAlert = afterHash.caseInsensitiveCompare("True") == .orderedSame
        }

        DispatchQueue.main.async {
            self.notificationTitle = title
            self.roomName = extractedRoomName
            self.callRejectToUserName = callRejectToUserName
            self.callRejectTokenId = callRejectTokenId
            self.showAlert = shouldShowAlert

            print("showAlert updated to: \(shouldShowAlert)")

            // Case 1: Alert should be shown → schedule auto-dismiss
            if shouldShowAlert {
                self.scheduleAutoDismissAlert()
            }
            // Case 2: Alert should be hidden immediately
            else {
                self.alertDismissTimer?.invalidate()
                self.showAlert = false
            }
        }

        print("Notification Parsed")
        print("Room: \(self.roomName ?? "nil")")
        print("User: \(self.callRejectToUserName)")
        print("Token ID: \(self.callRejectTokenId)")
        print("self.showAlert: \(self.showAlert)")

        NetworkManager.shared.fetchVideoCallToken(roomName: extractedRoomName) { result in
            DispatchQueue.main.async {
                switch result {
                case .success(let token):
                    print("Token Fetched: \(token)")
                    UserDefaults.standard.set(token, forKey: "videoCallToken")
                    UserDefaults.standard.set(extractedRoomName, forKey: "roomName")
                    UserDefaults.standard.set(callRejectToUserName, forKey: "callRejectToUserName")
                    UserDefaults.standard.set(callRejectTokenId, forKey: "callRejectTokenId")
                case .failure(let error):
                    print("Token Fetch Failed: \(error)")
                }
            }
        }
    }

    func scheduleAutoDismissAlert() {
        print("scheduleAutoDismissAlert.")
        alertDismissTimer?.invalidate()

        alertDismissTimer = Timer(timeInterval: 120, repeats: false) { [weak self] _ in
            print("Auto-dismissing alert after 2 minutes via Timer")
            DispatchQueue.main.async {
                self?.showAlert = false
            }
        }

        if let timer = alertDismissTimer {
            RunLoop.main.add(timer, forMode: .common)
            print("Timer manually added to main run loop.")
        }
    }

    
    func handleBackgroundNotification(userInfo: [AnyHashable: Any]) {
        guard let body = userInfo["Body"] as? String else { return }

             // Extract your custom session ID before the `#`
             let sessionId = body.components(separatedBy: "#").first ?? body

             if shownSessionIds.contains(sessionId) {
                 print(" Already handled session: \(sessionId), skipping.")
                 return
             }

             shownSessionIds.insert(sessionId)
             print(" Handling unique session: \(sessionId)")

             let content = UNMutableNotificationContent()
             content.title = "Missed Call"
             content.body = "Video Call"
             content.sound = .default

             let request = UNNotificationRequest(
                 identifier: sessionId,
                 content: content,
                 trigger: nil
             )

             UNUserNotificationCenter.current().add(request)
    
}
        
        
        func userNotificationCenter(_ center: UNUserNotificationCenter,
                                    willPresent notification: UNNotification,
                                    withCompletionHandler completionHandler: @escaping (UNNotificationPresentationOptions) -> Void) {
            
            let content = notification.request.content
            let body = content.body
            let currentNotificationIdentifier = notification.request.identifier
            
            print("currentNotificationIdentifier: \(currentNotificationIdentifier)")
            
            // Check if this notification was already processed
            if lastProcessedNotificationID == currentNotificationIdentifier {
                print("Duplicate notification ignored: \(currentNotificationIdentifier)")
                completionHandler([]) // Ignore duplicate
                return
            }
            
            // Remove all previously delivered notifications before displaying the new one
            UNUserNotificationCenter.current().removeAllDeliveredNotifications()
            
            // Schedule auto-removal after 10 seconds
            DispatchQueue.main.asyncAfter(deadline: .now() + 10) {
                UNUserNotificationCenter.current().removeDeliveredNotifications(withIdentifiers: [currentNotificationIdentifier])
                print("Notification removed after 10 seconds: \(currentNotificationIdentifier)")
            }
            
            // Update last processed ID
            lastProcessedNotificationID = currentNotificationIdentifier
            print("Foreground Notification Received:")
            print("Notification Received: \(content.userInfo)")
            print("Title: \(content.title)")
            print("Body: \(body)")
            
            // Extract relevant data from the notification body
            var extractedRoomName = ""
            var callRejectToUserName = ""
            var callRejectTokenId = ""
            
            if let atSymbolIndex = body.firstIndex(of: "@") {
                let beforeAt = body[..<atSymbolIndex]  // Get everything before "@"
                extractedRoomName = String(beforeAt)  // Full room name
                callRejectTokenId = String(body[body.index(after: atSymbolIndex)...]) // Everything after "@"
                
                // Extract callRejectToUserName (first two underscore-separated parts)
                let underscoreParts = extractedRoomName.split(separator: "_", maxSplits: 2, omittingEmptySubsequences: false)
                if underscoreParts.count >= 2 {
                    callRejectToUserName = String(underscoreParts[0]) + "_" + String(underscoreParts[1])
                }
            }
         
                self.notificationTitle = content.title
                self.roomName = extractedRoomName
                self.callRejectToUserName = callRejectToUserName
                self.callRejectTokenId = callRejectTokenId
                
                print("self.roomName: \(self.roomName ?? "nil")")
                print("self.callRejectToUserName: \(self.callRejectToUserName)")
                print("self.callRejectTokenId: \(self.callRejectTokenId)")
                
                self.showAlert = true  // Ensure alert is triggered
                
                // Call API to fetch video call token
                NetworkManager.shared.fetchVideoCallToken(roomName: extractedRoomName) { result in
                    DispatchQueue.main.async {
                        switch result {
                        case .success(let token):
                            print("Video Call Token: \(token)")
                            // Save token in UserDefaults
                            UserDefaults.standard.set(token, forKey: "videoCallToken")
                            UserDefaults.standard.set(extractedRoomName, forKey: "roomName")
                            UserDefaults.standard.set(callRejectToUserName, forKey: "callRejectToUserName")
                            UserDefaults.standard.set(callRejectTokenId, forKey: "callRejectTokenId")
                            
                        case .failure(let error):
                            print("Failed to fetch token: \(error)")
                        }
           
                }
            }
            
            // Ensure only the latest notification is displayed
            completionHandler([.banner, .sound])
        }
      
        
        //This is called when a notification is **tapped** while in background or killed state
        func userNotificationCenter(_ center: UNUserNotificationCenter,
                                    didReceive response: UNNotificationResponse,
                                    withCompletionHandler completionHandler: @escaping () -> Void) {
            let content = response.notification.request.content
            
            print("Notification Opened by User:")
            print("Title: \(content.title)")
            print("Body: \(content.body)")
         
                self.notificationTitle = content.title
                self.showAlert = true
       
            completionHandler()
        }
    }

