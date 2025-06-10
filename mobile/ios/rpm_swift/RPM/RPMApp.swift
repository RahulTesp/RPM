//
//  RPMApp.swift
//  RPM
//
//  Created by Prajeesh Prabhakar on 30/05/22.
//

import SwiftUI
import Firebase

// MAIN PAGE

@main
struct RPMApp: App {
    @StateObject var loginStateViewModel = RPMLoginViewModel()
    @State private var isLogin = false
    @StateObject private var model: AppModel // For iOS 14+
    @State var showChatAlert = false
    @State var isChatLogout = false
    @StateObject var messageListViewModel = MessageListViewModel()
    @StateObject var navigationHelper = NavigationHelper()
    @UIApplicationDelegateAdaptor(AppDelegate.self) var appDelegate  // Add AppDelegate for Firebase
    @StateObject private var callManager = CallManager()
    @StateObject private var roomManager = RoomManager()
    @StateObject private var localParticipant = LocalParticipantManager()
    @StateObject private var mediaSetupViewModel = MediaSetupViewModel()
    @StateObject var homeViewModel = RPMHomeViewModel()
    @AppStorage("wasInBackground") private var wasInBackground: Bool = false
    @AppStorage("isLoggedIn") private var isLoggedIn: Bool = false
    @Environment(\.scenePhase) var scenePhase
    @StateObject private var sessionManager = SessionManager.shared

    // Only one init, which sets up the app model.
    init() {
        _model = StateObject(wrappedValue: AppModel.shared)
        
        if !UserDefaults.standard.bool(forKey: "wasInBackground") {
                  // Reset termination flag if not backgrounded
                  UserDefaults.standard.set(false, forKey: "wasTerminated")
              }
    }

    var body: some Scene {
        WindowGroup {
            NavigationStack(path: $navigationHelper.path) {
                // Main content of the app goes here
                ApplicationSwitcher()
                    .environmentObject(loginStateViewModel)
                    .environmentObject(model)  // Pass AppModel to the environment
                    .environmentObject(navigationHelper)
                    .environmentObject(model.conversationManager)
              
                    .environmentObject(callManager)
                    .environmentObject(roomManager)
                    .environmentObject(localParticipant)
                    .environmentObject(mediaSetupViewModel)
                
                    .environmentObject(homeViewModel)
              
                    .onAppear {
                        print("AppModel in RPMTabBar:", model)
                        // Log AppModel details for debugging
                                               print("AppModel Initialized: \(model)")
                                        
                                               print("Global Status: \(model.globalStatus)")
                                               print("My Identity: \(model.myIdentity)")
                                               print("Conversation Manager: \(model.conversationManager != nil ? "Initialized" : "Not Initialized")")
                                               print("Messages Manager: \(model.messagesManager != nil ? "Initialized" : "Not Initialized")")
                                               print("Participants Manager: \(model.participantsManager != nil ? "Initialized" : "Not Initialized")")
                    }
                    .onChange(of: scenePhase) { newPhase in
                        switch newPhase {
                        case .active:
                            let wasInBackground = UserDefaults.standard.bool(forKey: "wasInBackground")
                               let wasTerminated = UserDefaults.standard.bool(forKey: "wasTerminated")

                               print("App is active")
                               print("wasInBackground: \(wasInBackground)")
                               print("wasTerminated: \(wasTerminated)")
                            if wasInBackground && wasTerminated {
                                // This means app was killed in background and relaunched
                                UserDefaults.standard.set(false, forKey: "wasInBackground")
                                UserDefaults.standard.set(false, forKey: "wasTerminated")

                                print("App relaunched after being killed. Logging out...")
                                isLoggedIn = false
                                
                                // Call logout
                                loginStateViewModel.logout { _, _ in
                                    print("App MAIN LOGOUT")
                                    DispatchQueue.main.async {
                                        navigationHelper.path = [.login] // Navigate to login screen
                                        print("MAINnavigationHelper.path: \( navigationHelper.path)")
                                    }
                                }
                            }
                            else {
                                    // Normal resume from background
                                    UserDefaults.standard.set(false, forKey: "wasInBackground")
                                }

                        case .background:
                            print("App entered background")
                            wasInBackground = true
                            UserDefaults.standard.set(true, forKey: "wasInBackground")
                        case .inactive:
                            print("App is inactive")

                        @unknown default:
                            break
                        }
                    }

                    .onChange(of: sessionManager.didReceiveUnauthorized) { isUnauthorized in
                        if isUnauthorized {
                            print(" Received 401 Unauthorized. Logging out...")

                            isLoggedIn = false
                            loginStateViewModel.logout { _, _ in
                                DispatchQueue.main.async {
                                    navigationHelper.path = [.login]
                                    print("🚪 Logged out and navigated to login.")
                                    sessionManager.reset()  // Reset to avoid repeated triggers
                                }
                            }
                        }
                    }

                    .navigationDestination(for: Screen.self) { screen in
                        switch screen {
                        case .login:
                            //RPMLoginView(isLogin: $isLogin)
                            RPMLoginView()
                                .environmentObject(model)
                                .environmentObject(model.conversationManager)
                                .environmentObject(model.messagesManager)
                                .environmentObject(model.participantsManager)
                                .environmentObject(navigationHelper)
                                .environmentObject(callManager)
                                .environmentObject(roomManager)
                                .environmentObject(localParticipant)
                                .environmentObject(mediaSetupViewModel)
                                .environmentObject(homeViewModel)
                                .environmentObject(loginStateViewModel)
                        case .forgotPassword:
                                  RPMGenerateOTPView()
                    
                                .environmentObject(navigationHelper)
                            
                        case .otp:
                              
                            RPMOTPView(isLogin: $isLogin)
                        case .resetPassword:
                             
                            RPMResetPasswordView()
                                .environmentObject(navigationHelper)
                            
                        case .tabBarView:
                             
                            RPMTabBarView()
                                .environmentObject(model)
                                .environmentObject(model.conversationManager)
                                .environmentObject(model.messagesManager)
                                .environmentObject(model.participantsManager)
                                .environmentObject(navigationHelper)
                                .environmentObject(callManager)
                                .environmentObject(roomManager)
                                .environmentObject(localParticipant)
                                .environmentObject(mediaSetupViewModel)
                                .environmentObject(homeViewModel)
                                .environmentObject(loginStateViewModel)
                            
                        case .programInfoView:
                                   RPMProgramInfoView()
                                .environmentObject(homeViewModel)
                            
                        case .myInfoView:
                            RPMMyInfoView()
                           
                        case .clinicalinfoView(let value):
                              RPMClinicalinfoView(returningFromClinicalInfo: .constant(value))
                                .environmentObject(navigationHelper)
                            
                        case .medicationAdd:
                                 
                            
                            RPMMedicationsAddView()
                            
                        case .personalInfoView:
                                 
                            
                            RPMPersonalInfoView()
                                .environmentObject(navigationHelper)
                            
                        case .changePassword:
                                 
                            
                            RPMChangePasswordView()
                                .environmentObject(navigationHelper)
                            
                        case .notificationView:
                                 
                            RPMNotificationView()
                            
                        case .conversationsList:
                            ConversationsList()
                                .environmentObject(model)
                                .environmentObject(model.conversationManager)
                                .environmentObject(model.messagesManager)
                                .environmentObject(model.participantsManager)
                                .environmentObject(navigationHelper)
                            
                            
                        case .createConversation:
                            CreateConversationView(
                                items: model.conversationManager.conversations
                              
                            )
                            .environmentObject(model)
                            .environmentObject(model.conversationManager)
                            .environmentObject(model.messagesManager)
                            .environmentObject(model.participantsManager)
                            .environmentObject(navigationHelper)
                      
                          
                        case .messageList(let conversation):
                            MessageListView(
                                conversation: conversation,
                                viewModel: messageListViewModel
                            )
                            .environmentObject(model)
                                .environmentObject(model.conversationManager)
                                .environmentObject(model.messagesManager)
                                .environmentObject(model.participantsManager)
                                .environmentObject(navigationHelper)
                        }
                    }
            }
        }
    }
}


struct ApplicationSwitcher: View {
    @State private var isLogin = false
    @EnvironmentObject var loginViewModel: RPMLoginViewModel
    @EnvironmentObject var appModel: AppModel
    @EnvironmentObject var navigationHelper: NavigationHelper
    @EnvironmentObject var conversationManager: ConversationManager

    @EnvironmentObject var callManager: CallManager
    @EnvironmentObject var roomManager: RoomManager
    @EnvironmentObject var localParticipant: LocalParticipantManager
    @EnvironmentObject var mediaSetupViewModel: MediaSetupViewModel
    
    @EnvironmentObject var accountListVM: RPMHomeViewModel
    @EnvironmentObject var memberListViewModel: RPMHomeViewModel
    @StateObject var memberDetList = MembersListViewModel()
    
    var body: some View {
            VStack {
                if loginViewModel.isAuthenticated {
                    RPMTabBarView()
                        .environmentObject(appModel)
                        .environmentObject(navigationHelper)
                        .environmentObject(conversationManager)
                        .environmentObject(appModel.messagesManager)
                        .environmentObject(appModel.participantsManager)
                    
                        .environmentObject(callManager)
                        .environmentObject(roomManager)
                        .environmentObject(localParticipant)
                        .environmentObject(mediaSetupViewModel)
                    
                        .environmentObject(accountListVM)
                        .environmentObject(loginViewModel)
                        .environmentObject(memberDetList)
                    
                        .onAppear {
                            print("AppModel in RPMTabBarView:", appModel)
                        }
                } else {
                  //  RPMLoginView(isLogin: $isLogin)
                    RPMLoginView()
                        .environmentObject(appModel)
                        .environmentObject(navigationHelper)
                        .environmentObject(appModel.messagesManager)
                        .environmentObject(appModel.participantsManager)
                        .environmentObject(conversationManager)
                    
                        .environmentObject(callManager)
                        .environmentObject(roomManager)
                        .environmentObject(localParticipant)
                        .environmentObject(mediaSetupViewModel)
                    
                        .environmentObject(accountListVM)
                        .environmentObject(loginViewModel)
                        .onAppear {
                            print("AppModel in RPMLoginView:", appModel)
                        }
                }
            }
        }
    }
    

enum Screen: Hashable{
    case login
    case createConversation
    case messageList(conversation: PersistentConversationDataItem)
    case conversationsList
    case forgotPassword
    case resetPassword
    case otp
    case tabBarView
    case notificationView
    case clinicalinfoView(Bool)
    case programInfoView
    case myInfoView
    case medicationAdd
    case personalInfoView
    case changePassword
}

