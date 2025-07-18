//
//  RPMGenerateOTPView.swift
//  RPM
//
//  Created by Prajeesh Prabhakar on 30/05/22.
//

import SwiftUI


struct RPMGenerateOTPView: View {
    
    @Environment(\.colorScheme) var colorScheme
    @State private var shouldShowLoginAlert = false
    @State private var showingAlerts = false
    @State private var result = ""
    @State private var isActive = false
    @ObservedObject private var resetModel = RPMResetPasswordViewModel()
    @State private var alertItem: AlertItem?
    @State private var username: String = ""
    @State private var password: String = ""
    @FocusState private var isUsernameFocused: Bool
    @EnvironmentObject var navigationHelper: NavigationHelper
    
    // MARK: - Computed Properties

    private var fullWidth: CGFloat {
        UIScreen.main.bounds.width
    }

    private var inputFieldWidth: CGFloat {
        UIDevice.current.userInterfaceIdiom == .phone ? UIScreen.main.bounds.width / 1.39 : UIScreen.main.bounds.width / 1.3
    }

    var body: some View {
        VStack {
            Spacer()

            // Username TextField
            TextField("", text: $username, prompt: Text("Enter Username").foregroundColor(Color("TextColorGray")))
                .colorScheme(.light)
                .textFieldStyle(MyTextFieldStyle())
                .cornerRadius(10)
                .disableAutocorrection(true)
                .focused($isUsernameFocused)
                .frame(width: inputFieldWidth)

            // Generate OTP Button
            Button(action: {
                handleGenerateOTP()
            }) {
                Text("Generate OTP")
                    .font(Font.custom("Rubik-Regular", size: 16))
                    .frame(width: inputFieldWidth, height: 50)
                    .background(Color("buttonColor"))
                    .foregroundColor(.white)
                    .cornerRadius(15)
            }
            .padding(.horizontal, 22)

            Spacer()
        }
        .navigationBarBackButtonHidden(false)
        .frame(minWidth: fullWidth)
        .background(Color("bgColorDark"))
        .alert(item: $alertItem) { alertItem in
            Alert(title: alertItem.title, message: alertItem.message, dismissButton: alertItem.dismissButton)
        }
    }

    // MARK: - Private Methods

    private func handleGenerateOTP() {
        let defaults = UserDefaults.standard
        defaults.setValue(username, forKey: "pgmUserID")

        print("pgmUserID: \(UserDefaults.standard.string(forKey: "pgmUserID") ?? "")")

        result = validateUsername(username)
        print("Validation result: \(result)")

        guard result == "Valid" else {
            showingAlerts = true
            shouldShowLoginAlert = true
            alertItem = AlertItem(title: Text("Invalid Username"), message: Text(result), dismissButton: .default(Text("OK")))
            return
        }

        resetModel.generateOTP(userName: username) { token, alert in
            if let _ = token {
                print("OTP Generation Successful")
                
                navigationHelper.path.append(Screen.resetPassword)
                
                isActive = true

                UserDefaults.standard.set(username, forKey: "usernameGP")
                print("usernameGP: \(UserDefaults.standard.string(forKey: "usernameGP") ?? "")")
            } else {
                shouldShowLoginAlert = true
                alertItem = alert
                print("OTP Generation Failed: \(alert?.title ?? Text("Unknown error"))")
            }

            username = ""
            password = ""
            dismissKeyboard()
        }
    }

    private func validateUsername(_ username: String) -> String {
        return username.isEmpty ? "Username is Required" : "Valid"
    }

    private func dismissKeyboard() {
        isUsernameFocused = false
    }
}
