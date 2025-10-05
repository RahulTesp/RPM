//
//  RMPMoreView.swift
//  RPM
//
//  Created by Prajeesh Prabhakar on 04/06/22.
//

import SwiftUI
import TwilioVideo

struct RPMMoreView: View {

    @EnvironmentObject var loginViewModel: RPMLoginViewModel
    @EnvironmentObject var appModel: AppModel
    @Environment(\.presentationMode) var mode: Binding<PresentationMode>
    @EnvironmentObject var navigationHelper: NavigationHelper
    @EnvironmentObject var callManager: CallManager
    @EnvironmentObject var roomManager: RoomManager
    @EnvironmentObject var localParticipant: LocalParticipantManager
    @EnvironmentObject var mediaSetupViewModel: MediaSetupViewModel
    @State private var isLogin = false
    @State private var isActive = false
    @State private var returningFromClinicalInfo = false
    @EnvironmentObject var homeViewModel: RPMHomeViewModel
    @EnvironmentObject var sessionManager: SessionManager

    var body: some View {
        VStack(alignment: .leading) {
            AccountPanel()

            ScrollView {
                contentTabs
                logoutButton
            }
        }
        .padding(.bottom, 100)
        .background(Color("bgColor"))
        .navigationBarBackButtonHidden(true)
        .ignoresSafeArea(.all)
        
        .onAppear(){
            print(" MOREONAPEARPath:", navigationHelper.path)
        }
    }

    // MARK: - Tabs
    @ViewBuilder
    private var contentTabs: some View {
        ProfileTabs(
            text: "ProgramInfoOutline",
            title2: "Program Info",
            title3: "Your Core Team & Program details",
            fontColor1: .black,
            fontColor2: Color("darkGreen"),
            bgColor: Color.white,
              route: .programInfoView
        ).environmentObject(navigationHelper)

        ProfileTabs(
            text: "clinicalInfoOutline",
            title2: "Clinical Info",
            title3: "All Clinical & Medication data",
            fontColor1: .black,
            fontColor2: Color("darkGreen"),
            bgColor: Color.white,
          
            route: .clinicalinfoView(false)

        ).environmentObject(navigationHelper)

        ProfileTabs(
            text: "MyInfoOutline",
            title2: "My Info",
            title3: "Update your Profile & Password",
            fontColor1: .black,
            fontColor2: Color("darkGreen"),
            bgColor: Color.white,
         
            route: .myInfoView

        ).environmentObject(navigationHelper)
    }

    // MARK: - Logout Button
    private var logoutButton: some View {
        Button(action: performLogout) {
            Text("LOG OFF")
                .font(Font.custom("Rubik-Regular", size: 16))
                .frame(width: 320, height: 45)
                .background(Color("buttonColor"))
                .foregroundColor(.white)
                .cornerRadius(10)
                .padding(.vertical, 10)
        }
    }

   
    // MARK: - Logout Logic
    
    func performLogout() {
        Task { @MainActor in
            sessionManager.logoutWithFirebaseToken(
                appModel: appModel,
                homeViewModel: homeViewModel,
                navigationHelper: navigationHelper,
                loginViewModel: loginViewModel,
                callManager: callManager,
                roomManager: roomManager
            )
        }
    
    }
}

//TOP SECTION


struct AccountPanel: View {
    @EnvironmentObject var navigationHelper: NavigationHelper
    @State var isLinkActive = false
    @State private var isActive: Bool = false
    
    var body: some View {
        VStack {
            // Top row with back arrow + title
            HStack {
                Button(action: {
                    navigationHelper.resetToHomeTab()   //  back to Home tab
                }) {
                    
                    Image("ArrowBack").renderingMode(.template)
                        .foregroundColor(Color("buttonColor"))
                    
                        .font(.system(size: 18, weight: .medium))
                    
                    
                }
                
                Spacer()
                
                Text("Account")
                    .font(Font.custom("Rubik-Regular", size: 16))
                    .foregroundColor(.black)
                
                Spacer()
   
            }
            .padding(.top, 30)
            .padding(.bottom, 20)
            .padding(.horizontal, 10)
            
            // Rest of your account panel UI...
            HStack {
                Image(systemName: "person")
                    .clipShape(Circle())
                    .shadow(radius: 30)
                    .frame(width: 40.0, height: 32.0)
                    .padding(.horizontal, 10)
                    .overlay(Circle().stroke(Color.red, lineWidth: 5))
                
                VStack(alignment: .leading) {
                    Text("Hi, " + (UserDefaults.standard.string(forKey: "patientNameString") ?? ""))
                        .font(Font.custom("Rubik-Regular", size: 18))
                        .tracking(0.3)
                        .foregroundColor(Color("darkGreen"))
                    
                    Text(UserDefaults.standard.string(forKey: "patientUserNameString") ?? "")
                        .fontWeight(.black)
                        .font(Font.custom("Rubik-SemiBold", size: 16))
                        .foregroundColor(Color("TextColorBlack"))
                }
                Spacer(minLength: 22)
            }
            .frame(maxWidth: .infinity, minHeight: 55)
            .padding(10)
            .padding(.top, 10)
            .background(Color("avgGreen"))
            .cornerRadius(15)
            .padding(.horizontal, 20)
            .padding(.bottom, 10)
            
            HStack {
                Spacer()
                PanelButtons(text: "BellOutline", colorf: Color("darkGreen"), colorb: Color("transparentGreen"), label: "Notification")
                Spacer()
                PanelButtons(text: "FeedbackOutline", colorf: Color("darkGreen"), colorb: Color("transparentGreen"), label: "Feedback")
                Spacer()
                PanelButtons(text: "HelpOutline", colorf: Color("darkGreen"), colorb: Color("transparentGreen"), label: "Help")
                Spacer()
                PanelButtons(text: "SettingsOutline", colorf: Color("darkGreen"), colorb: Color("transparentGreen"), label: "Settings")
                Spacer()
            }
            .padding(.top, 5)
            .padding(.bottom, 22)
        }
        .padding(.top, 40)
        .background(Color("lightGreen"))
        .clipShape(RoundedRectangle(cornerRadius: 20))
    }
}


struct PanelButtons :  View{
    @State private var showToast = false
    var text : String
    var colorf: Color
    var colorb: Color
    var label: String
    var body : some View{
        
        VStack
        {
            
            Button(action :{
                showToast = true
                           DispatchQueue.main.asyncAfter(deadline: .now() + 2) {
                               showToast = false
                           }
                
                
            }
     
            
            )
            {
                
                Image(text)
                    .renderingMode(.template)
                    .foregroundColor(colorf)
                    .padding(30)
                    .frame( maxWidth: 45, maxHeight: 45)
                    .badge(10)
                
                    .background(colorb)
                    .cornerRadius(14)
            }
            
     
            
            Text(label)
                .font(.system(size : 12, design: .rounded))
                .foregroundColor(colorf)
            
            
            if showToast {
                           Text("Coming Soon!")
                   
                    .padding(.vertical,5)
                    .padding(.horizontal,5)
                  
                               .background(Color("title1"))
                               .foregroundColor(.white)
                               .cornerRadius(10)
                               .transition(.move(edge: .bottom))
                               .animation(.spring(), value: showToast)
                       }
     
        }
        
    }
}

// BOTTOM SECTION

struct ProfileTabs: View {
    var text: String
    var title2: String
    var title3: String
    
    var fontColor1: Color
    var fontColor2: Color
    var bgColor: Color

    @EnvironmentObject var navigationHelper: NavigationHelper
    var route: Screen

    var body: some View {
        VStack {
            HStack {
                Button(action: {}) {
                    Image(text)
                        .frame(maxWidth: 45, maxHeight: 45)
                }

                Button(action: {
                    navigationHelper.path.append(route)
                }) {
                    VStack(alignment: .leading) {
                        Text(title2)
                            .font(Font.custom("Rubik-Regular", size: 16))
                            .foregroundColor(fontColor1)

                        Text(title3)
                            .font(Font.custom("Rubik-Regular", size: 12))
                            .foregroundColor(fontColor2)
                    }
                }

                Spacer()
            }
            .frame(minWidth: 44, minHeight: 75)
            .padding(.horizontal, 12)
            .background(Color("textFieldBG"))
            .cornerRadius(15)
        }
        .padding(.horizontal, 12)
    }
}
