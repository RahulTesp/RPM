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
    @StateObject private var model = AppModel.shared
    @State var showChatAlert = false
    @State var isChatLogout = false
    @StateObject var messageListViewModel = MessageListViewModel()
    @StateObject var navigationHelper = NavigationHelper()
    @UIApplicationDelegateAdaptor(AppDelegate.self) var appDelegate  // Added AppDelegate for Firebase
    @StateObject private var callManager = CallManager()
    @StateObject private var roomManager = RoomManager()
    @StateObject private var localParticipant = LocalParticipantManager()
    @StateObject private var mediaSetupViewModel = MediaSetupViewModel()
    @StateObject var homeViewModel = RPMHomeViewModel()
    @AppStorage("wasInBackground") private var wasInBackground: Bool = false
    @AppStorage("isLoggedIn") private var isLoggedIn: Bool = false
    @Environment(\.scenePhase) var scenePhase
    @StateObject private var sessionManager = SessionManager.shared
    @StateObject private var networkMonitor = NetworkMonitor()
    @StateObject var memberDetList = MembersListViewModel()


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
                    .environmentObject(networkMonitor)
                    .environmentObject(sessionManager)
                    .environmentObject(memberDetList)
                
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
                
                    .onChange(of: sessionManager.didReceiveUnauthorized) { isUnauthorized in
                        if isUnauthorized {
                            print("Received 401 Unauthorized. Logging out...")

                            homeViewModel.reset()

                            // Step 1: Logout API call
                            loginStateViewModel.logout { response, alert in
                                print("Logout server call finished")

                                if let alert = alert {
                                    print("Logout failed: \(alert.title)")  // log for debugging
                                }

                                // Step 2: Always clean app state, even if logout fails
                                Task { @MainActor in
                                    model.signOutChat()
                                    callManager.disconnect()
                                    roomManager.disconnect()
                                    
                                    navigationHelper.path = []
                                    loginStateViewModel.isAuthenticated = false
                                    
                                    print("navigationHelper.path after logout: \(navigationHelper.path)")
                                }
                            }
                        }
                    }

                    .navigationDestination(for: Screen.self) { screen in
                        switch screen {
                        case .loginView:
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
                            RPMOTPView()
                                .environmentObject(navigationHelper)
                                .environmentObject(loginStateViewModel)
                            
                        case .resetPassword:
                            RPMResetPasswordView()
                                .environmentObject(navigationHelper)
                                .environmentObject(loginStateViewModel)
                            
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
                                .environmentObject(navigationHelper)
                            
                        case .myInfoView:
                            RPMMyInfoView()
                                .environmentObject(navigationHelper)
                           
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
                                .environmentObject(loginStateViewModel)
                                .environmentObject(model)
                                .environmentObject(callManager)
                                .environmentObject(roomManager)
                            
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
                            CreateConversationView(items: model.conversationManager.conversations)
                            .environmentObject(model)
                            .environmentObject(model.conversationManager)
                            .environmentObject(model.messagesManager)
                            .environmentObject(model.participantsManager)
                            .environmentObject(navigationHelper)
                            .environmentObject(memberDetList)
                      
                        case .messageList(let conversation):
                            makeMessageListView(for: conversation)

                        }
                    }
            }
        }
    }
    
    @ViewBuilder
    private func makeMessageListView(for conversation: PersistentConversationDataItem) -> some View {
        let context = model.getManagedContext()
        let convItemViewModel = model.conversationManager.viewModel(for: conversation, context: context)

        MessageListView(
            conversation: conversation,
            viewModel: messageListViewModel,
            convItemViewModel: convItemViewModel
        )
        .environmentObject(model)
        .environmentObject(model.conversationManager)
        .environmentObject(model.messagesManager)
        .environmentObject(model.participantsManager)
        .environmentObject(navigationHelper)
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
    @EnvironmentObject var homeViewModel: RPMHomeViewModel
    @EnvironmentObject var memberDetList: MembersListViewModel
    @EnvironmentObject var networkMonitor: NetworkMonitor
    @State private var showNoInternetAlert = false
    @EnvironmentObject var sessionManager: SessionManager
    
    var body: some View {
  
        ZStack(alignment: .bottom) {
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
                    .environmentObject(homeViewModel)
                    .environmentObject(loginViewModel)
                    .environmentObject(memberDetList)
                    .environmentObject(sessionManager)
                    
                    .onAppear {
                        print("AppModel in RPMTabBarView:", appModel)
                        
                        // Trigger members API now that user is logged in
                                           memberDetList.memDetails()
                    }
            } else {
           
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
                    .environmentObject(homeViewModel)
                    .environmentObject(loginViewModel)
                
                    .onAppear {
                        print("AppModel in RPMLoginView:", appModel)
                    }
            }
        }
             ConnectivityBanner(isConnected: networkMonitor.isConnected)
                  .padding(.bottom, 60)
            
    }
         .animation(.easeInOut, value: networkMonitor.isConnected)
        
         .onChange(of: networkMonitor.isConnected) { isConnected in
                      showNoInternetAlert = !isConnected
                  }
            .alert(isPresented: $showNoInternetAlert) {
                     Alert(
                         title: Text("No Internet Connection"),
                         message: Text("Please turn on Wi-Fi or Mobile Data."),
                         primaryButton: .default(Text("Open Settings")) {
                             if let url = URL(string: UIApplication.openSettingsURLString),
                                UIApplication.shared.canOpenURL(url) {
                                 UIApplication.shared.open(url)
                             }
                         },
                         secondaryButton: .cancel()
                     )
                 }
        }
    }


struct ConnectivityBanner: View {
    let isConnected: Bool

    var body: some View {
        VStack {
            if !isConnected {
                Text("No Internet Connection")
                    .font(.footnote) // Smaller text size
                    .frame(maxWidth: .infinity)
                    .padding(.vertical, 6) // Less vertical padding = smaller banner height
                    .background(Color.red)
                    .foregroundColor(.white)
                    .transition(.move(edge: .bottom))
            }
        }
        .frame(maxWidth: .infinity)
    }
}



enum Screen: Hashable{
    case loginView
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

