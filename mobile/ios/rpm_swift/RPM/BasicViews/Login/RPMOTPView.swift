

////
////  ContentView.swift
////  RPM
////
////  Created by Prajeesh Prabhakar on 30/05/22.
////
//


import SwiftUI

struct RPMOTPView: View {
    @EnvironmentObject var loginViewModel: RPMLoginViewModel
    @EnvironmentObject var navigationHelper: NavigationHelper
    @EnvironmentObject var appModel: AppModel
    @State var otpCode: String = ""
    let otpCodeLength = 8
    @State var showText = false
    @State var showResend = false
    @State var restartTimer = false
    var body: some View {
        VStack {
            if loginViewModel.isLoggedOut {
                RPMLoginView()
            } else {
                Text("Log In").font(.title)
                Image("logoclynx").resizable().frame(width: 160, height: 150)
                CountdownView(
                    otpCode: $otpCode,
                    showText: $showText,
                    showResend: $showResend,
                    restartTimer: $restartTimer
                ).padding()
                Text("Enter the code sent to \(UserDefaults.standard.string(forKey: "MobileNumber") ?? "")")
                    .multilineTextAlignment(.center)
                OtpView_SwiftUI(
                    otpCode: $otpCode,
                    otpCodeLength: otpCodeLength,
                    textColor: .black,
                    textSize: 27
                )
                .environmentObject(navigationHelper)
                .environmentObject(loginViewModel)
                .padding()
                if showText {
                    Button("Resend OTP") {
                        restartTimer = true
                        loginViewModel.login(
                            userName: UserDefaults.standard.string(forKey: "pgmUserID") ?? "",
                            password: UserDefaults.standard.string(forKey: "passwordSaved") ?? ""
                        ) { token, alert in
                            if alert != nil {
                                // handle alert
                            }
                        }
                    }
                    .frame(height: 50)
                    .frame(maxWidth: .infinity)
                    .background(Color("lightGreen"))
                    .foregroundColor(.white)
                    .cornerRadius(15)
                    .padding()
                }
            }
            Spacer()
        }
        .navigationBarBackButtonHidden(true)
        .background(Color("bgColorDark").edgesIgnoringSafeArea(.all))
    }
}
