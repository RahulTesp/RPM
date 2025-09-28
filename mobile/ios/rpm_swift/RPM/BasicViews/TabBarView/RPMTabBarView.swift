//
//  RMPTabBarView.swift
//  RPM
//
//  Created by Prajeesh Prabhakar on 30/05/22.
//

import SwiftUI

class NavigationHelper: ObservableObject {
    @Published var path: [Screen] = []
    @Published var selectedTab: Int = 0
    
    var currentScreen: String? {
        return path.last.map { "\($0)" }
    }
    
    func resetToHomeTab() {
          self.path.removeAll()
          self.selectedTab = 0       //  This will switch to the Home tab
      }
}


struct RPMTabBarView: View {
    @State private var imageName = "HomeOutline"
    @State private var isPressed = false

    @EnvironmentObject var appModel: AppModel
    @EnvironmentObject var navigationHelper: NavigationHelper
    @EnvironmentObject var conversationManager: ConversationManager
    @EnvironmentObject var messagesManager: MessagesManager
    @EnvironmentObject var participantsManager: ParticipantsManager
    
    @EnvironmentObject var callManager: CallManager
    @EnvironmentObject var roomManager: RoomManager
    @EnvironmentObject var localParticipant: LocalParticipantManager
    @EnvironmentObject var mediaSetupViewModel: MediaSetupViewModel

    @EnvironmentObject var accountListVM: RPMHomeViewModel
    @EnvironmentObject var loginViewModel: RPMLoginViewModel
    @EnvironmentObject var memberDetList: MembersListViewModel
    @EnvironmentObject var sessionManager: SessionManager
    @EnvironmentObject var notifList: NotificationViewModel
    @State private var selectedTab = 0

    var body: some View {
        VStack {
            TabView (selection: $navigationHelper.selectedTab) {
                RPMHomeView()
                    .tag(0)
                    .environmentObject(appModel)
                    .environmentObject(navigationHelper)
                    .environmentObject(conversationManager)
                    .environmentObject(messagesManager)
                    .environmentObject(participantsManager)
                   
                    .environmentObject(callManager)
                    .environmentObject(accountListVM)
           
                    .environmentObject(roomManager)
                    .environmentObject(localParticipant)
                    .environmentObject(mediaSetupViewModel)
                    .environmentObject(loginViewModel)
                    .environmentObject(memberDetList)
                    .environmentObject(sessionManager)
                    .environmentObject(notifList)
                
                    .onAppear {
                        print("AppModelinRPMHome:", appModel)
                
                        print("conversationManagerinRPMHome:", conversationManager)
                        navigationHelper.selectedTab = 0 
                    }
                    .tabItem {
                        Image("HomeOutline")
                        Text("Home")
                    }

                RPMTodoListView()
                    .tag(1)
                    .environmentObject(appModel)
                    .environmentObject(navigationHelper)
                    .environmentObject(conversationManager)
                    .environmentObject(messagesManager)
                    .environmentObject(participantsManager)
                    .environmentObject(sessionManager)
                
                    .tabItem {
                        Image("ToDoOutline")
                        Text("Todo List")
                    }

                if UserDefaults.standard.string(forKey: "pgmTypeString") == "RPM" {
                    RPMVitalsView()
                        .tag(2)
                        .environmentObject(appModel)
                        .environmentObject(navigationHelper)
                        .environmentObject(conversationManager)
                        .environmentObject(messagesManager)
                        .environmentObject(participantsManager)
                        .environmentObject(sessionManager)
                        .tabItem {
                            Image("VitalOutline")
                            Text("Vitals")
                        }
                }

                RPMMoreView()
                    .tag(3)
                    .environmentObject(appModel)
                    .environmentObject(navigationHelper)
                    .environmentObject(conversationManager)
                    .environmentObject(messagesManager)
                    .environmentObject(participantsManager)
               
                    .environmentObject(callManager)
                    .environmentObject(roomManager)
                    .environmentObject(localParticipant)
                    .environmentObject(mediaSetupViewModel)
                
                    .environmentObject(accountListVM)
                    .environmentObject(loginViewModel)
                    .environmentObject(sessionManager)
                
                    .tabItem {
                        Image("MenuOutline")
                        Text("More")
                    }
            }
            .toolbar(.visible, for: .tabBar)
            .toolbarBackground(Color("ChatBGcolor"), for: .tabBar)
            .padding(.bottom, 10)
            .background(Color("ChatBGcolor"))
            .navigationBarBackButtonHidden(true)
        }
        .navigationBarBackButtonHidden(true)
        .navigationBarHidden(true)
        .onAppear()
        {
            print(" tabONAPEARPath:", navigationHelper.path)
        }
    }
}

struct IsPressedRegisterStyle : ButtonStyle {
    @Binding var isPressed : Bool
    func makeBody(configuration: Self.Configuration) -> some View {
        
        configuration.label
            .onChange(of: configuration.isPressed, perform: {newVal in
                isPressed = newVal
                print("isPresseddddd")
                print(isPressed)
            })
    }
}
struct TabBarAccessor: UIViewControllerRepresentable {
    var callback: (UITabBar) -> Void
    private let proxyController = ViewController()

    func makeUIViewController(context: UIViewControllerRepresentableContext<TabBarAccessor>) ->
                              UIViewController {
        proxyController.callback = callback
        return proxyController
    }
    
    func updateUIViewController(_ uiViewController: UIViewController, context: UIViewControllerRepresentableContext<TabBarAccessor>) {
    }
    
    typealias UIViewControllerType = UIViewController

    private class ViewController: UIViewController {
        var callback: (UITabBar) -> Void = { _ in }

        override func viewWillAppear(_ animated: Bool) {
            super.viewWillAppear(animated)
            if let tabBar = self.tabBarController {
                self.callback(tabBar.tabBar)
            }
        }
    }
}
