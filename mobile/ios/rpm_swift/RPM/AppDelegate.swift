//
//  AppDelegate.swift
//  RPM
//
//  Created by Tesplabs on 10/03/25.
//

import UIKit
import Firebase
import UserNotifications

class AppDelegate: NSObject, UIApplicationDelegate, UNUserNotificationCenterDelegate, MessagingDelegate {
    
    var lastMessageId: String?
    
    func application(_ application: UIApplication,
                     didFinishLaunchingWithOptions launchOptions: [UIApplication.LaunchOptionsKey: Any]? = nil) -> Bool {
        
        print("AppDelegate: didFinishLaunchingWithOptions")
        
        FirebaseApp.configure()
        
        UNUserNotificationCenter.current().delegate = self
        Messaging.messaging().delegate = self
        
        requestNotificationPermission()
        
        return true
    }
    
    func requestNotificationPermission() {
        let center = UNUserNotificationCenter.current()
        center.requestAuthorization(options: [.alert, .badge, .sound]) { granted, error in
            if granted {
                DispatchQueue.main.async {
                    UIApplication.shared.registerForRemoteNotifications()
                }
            } else {
                print(" Notification permission not granted: \(error?.localizedDescription ?? "No error")")
            }
        }
    }
    
    func applicationWillTerminate(_ application: UIApplication) {
        print("App will terminate")
        UserDefaults.standard.set(true, forKey: "wasTerminated")
    }
    
    
    // Called when APNs successfully registers the device and provides a device token
    func application(_ application: UIApplication, didRegisterForRemoteNotificationsWithDeviceToken deviceToken: Data) {
        print("APNs Device Token Received: \(deviceToken)")
        Messaging.messaging().apnsToken = deviceToken  //  Set APNs token for Firebase
    }
    
    //  Called when APNs registration fails
    func application(_ application: UIApplication, didFailToRegisterForRemoteNotificationsWithError error: Error) {
        print("Failed to Register for Remote Notifications: \(error.localizedDescription)")
    }
    
    
    // MARK: FCM Token
    func messaging(_ messaging: Messaging, didReceiveRegistrationToken fcmToken: String?) {
        print(" FCM Token: \(fcmToken ?? "")")
        // send to backend
    }
    
    // MARK: Receive Foreground Notification
    func userNotificationCenter(_ center: UNUserNotificationCenter,
                                willPresent notification: UNNotification,
                                withCompletionHandler completionHandler: @escaping (UNNotificationPresentationOptions) -> Void) {
        print(" Foreground NotificationApp: \(notification.request.content.userInfo)")
        NotificationManager.shared.handle(notification: notification)
        completionHandler([.banner, .sound])
    }
    
    // MARK: Tapped Notification
    func userNotificationCenter(_ center: UNUserNotificationCenter,
                                didReceive response: UNNotificationResponse,
                                withCompletionHandler completionHandler: @escaping () -> Void) {
        print("Notification Tapped: \(response.notification.request.content.userInfo)")
        completionHandler()
    }
    
    
    
    func application(_ application: UIApplication,
                     didReceiveRemoteNotification userInfo: [AnyHashable: Any],
                     fetchCompletionHandler completionHandler: @escaping (UIBackgroundFetchResult) -> Void) {
        
        
        guard let messageId = userInfo["gcm.message_id"] as? String else {
            completionHandler(.noData)
            return
        }
        
        if lastMessageId == messageId {
            print(" Duplicate notification ignored: \(messageId)")
            completionHandler(.noData)
            return
        }
        
        lastMessageId = messageId
        
        print(" Handling New Remote Notification: \(userInfo)")
        
        // Extract custom fields
        guard let title = userInfo["Title"] as? String,
              let body = userInfo["Body"] as? String else {
            print(" Missing Title or Body in payload")
            completionHandler(.noData)
            return
        }
        
        
        let state = UIApplication.shared.applicationState
        
        if state == .active {
            // Foreground – trigger custom alert
            NotificationManager.shared.handleForegroundNotification(
                title: title,
                body: body,
                userInfo: userInfo
            )
        } else {
            print(" Notification ID: \(userInfo["gcm.message_id"] ?? "N/A")")
            
            // Background – show local notification
            NotificationManager.shared.handleBackgroundNotification(userInfo: userInfo)
        }
        
        completionHandler(.newData)
        
    }
    
}
