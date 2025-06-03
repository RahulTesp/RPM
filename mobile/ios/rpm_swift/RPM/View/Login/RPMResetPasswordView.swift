//
//  RPMResetPasswordView.swift
//  RPM
//
//  Created by Tesplabs on 18/12/1944 Saka.
//



import SwiftUI

struct RPMResetPasswordView: View {
    @Environment(\.colorScheme) var colorScheme
    private var widthFull = UIScreen.main.bounds.width
    private var width: CGFloat {
        UIDevice.current.userInterfaceIdiom == .phone ? UIScreen.main.bounds.width / 1.39 : UIScreen.main.bounds.width / 1.3
    }

    @StateObject private var resetModel = RPMResetPasswordViewModel()

    @State private var shouldShowLoginAlert = false
    @State private var showingAlerts = false
    @State private var resetSuccessAlerts = false
    @State private var result = ""
    @State private var isPasswordValid = false
    @State private var isLinkActive = false
    @State private var isActive = false
    @State private var isResetpwd = false
    @State private var isLogin = false
    @State private var showToast = false
    @State private var alertItem: AlertItem?
    @State private var otpCode = ""
    @State private var otpCodeLength = 8
    @State private var textColor = Color.black
    @State private var textSize: CGFloat = 27
    @State private var isOn = false
    @State private var restartTimer = false
    @State private var showText = false
    @State private var showResend = false
    @State private var newpassword = ""
    @State private var confirmpassword = ""
    @State private var usr = ""

    @EnvironmentObject var navigationHelper: NavigationHelper
    
    var body: some View {
        VStack {
            Spacer()
            otpSection
            passwordInputSection
            Spacer()
            actionButton
            Spacer()
        }
        .navigationBarBackButtonHidden(true)
        .frame(minWidth: widthFull)
        .background(Color("bgColorDark"))
        .sheet(isPresented: $showToast) {
            ToastView(message: "Hello, World!")
        }
    }

    private var otpSection: some View {
        Group {
            CountdownView(otpCode: $otpCode, showText: $showText, showResend: $showResend, restartTimer: $restartTimer)
                .padding(10)

            Text("Enter the Code received on your Mobile number " + (UserDefaults.standard.string(forKey: "MobileNumberRP") ?? ""))
                .foregroundColor(Color("TextColorBlack"))
                .font(Font.custom("Rubik-Regular", size: 16))
                .padding(10)
                .fixedSize(horizontal: false, vertical: true)
                .multilineTextAlignment(.center)

            OtpResetVerificationView(otpCode: $otpCode, otpCodeLength: otpCodeLength, textColor: textColor, textSize: textSize)
                .padding(10)
        }
    }

    private var passwordInputSection: some View {
        Group {
            SecureInputResetView("New Password", text: $newpassword)
                .colorScheme(.light)
                .disableAutocorrection(false)
                .frame(width: width)
                .textFieldStyle(MyTextFieldStyle())
                .cornerRadius(10)
                .onChange(of: newpassword) { newValue in
                    isPasswordValid = validatePasswordStrength(newpassword: newValue)
                }

            if !isPasswordValid {
                Text("Password must be at least 8 characters long and contain at least 2 uppercase letters , 2 numbers and 1 special character.")
                    .foregroundColor(.red)
                    .padding(.vertical, 6)
                    .padding(.horizontal, 30)
                    .font(Font.custom("Rubik-Regular", size: 10))
            }

            SecureInputResetView("Confirm Password", text: $confirmpassword)
                .colorScheme(.light)
                .disableAutocorrection(false)
                .autocapitalization(.none)
                .frame(width: width)
                .textFieldStyle(MyTextFieldStyle())
                .cornerRadius(10)
        }
    }


    private var actionButton: some View {
        Group {
            if showResend {
                resendOTPButton
            } else {
                resetPasswordButton
            }
        }
    }

    private var resendOTPButton: some View {
        Button(action: resendOTP) {
            Text("Resend OTP")
                .font(Font.custom("Rubik-Regular", size: 16))
                .frame(width: width, height: 50)
                .background(Color("buttonColor"))
                .foregroundColor(.white)
                .cornerRadius(15)
                .padding(.horizontal, 22)
        }
        .alert(item: $alertItem) { $0.alert }
    }

    private var resetPasswordButton: some View {
        Button(action: resetPassword) {
            Text("Reset Password")
                .font(Font.custom("Rubik-Regular", size: 16))
                .frame(width: width, height: 50)
                .background(Color("buttonColor"))
                .foregroundColor(.white)
                .cornerRadius(15)
                .padding(.horizontal, 22)
        }
        .alert(item: $alertItem) { $0.alert }
        .alert(result, isPresented: $showingAlerts) { Button("OK", role: .cancel) {} }
        .alert("Password Reset Successfully", isPresented: $isResetpwd) { Button("OK", role: .cancel) {} }
    }

    private func resendOTP() {
        restartTimer = true
        resetModel.generateOTP(userName: UserDefaults.standard.string(forKey: "usernameGP") ?? "") { token, alert in
            if token != nil {
                self.isActive = true
            } else {
                self.shouldShowLoginAlert = true
                self.alertItem = alert
            }
            dismissKeyboard()
        }
    }

    private func resetPassword() {
        result = validatePassword(newpassword, confirmpassword)
        if result != "Valid" {
            showingAlerts = true
            shouldShowLoginAlert = true
            return
        }

        isResetpwd = resetModel.isOn
        resetModel.resetPassword(
            userName: UserDefaults.standard.string(forKey: "usernameGP") ?? "",
            otp: UserDefaults.standard.string(forKey: "otpRP") ?? "",
            password: newpassword,
            isResetpwd: $isResetpwd
        ) { token, alert in
            if token != nil {
                
                self.isActive = true
                
                let shouldGoToLogin = UserDefaults.standard.bool(forKey: "resetSuccess")
                             if shouldGoToLogin {
                                 navigationHelper.path.append(.login)
                                
                             } else {
                                 navigationHelper.path.append(.resetPassword)
                               
                             }
                
                
            } else {
                self.shouldShowLoginAlert = true
                self.alertItem = alert
            }
            dismissKeyboard()
        }
    }

    private func validatePasswordStrength(newpassword: String) -> Bool {
        guard newpassword.count >= 8 else { return false }
        guard newpassword.range(of: #"[A-Z].*[A-Z]"#, options: .regularExpression) != nil else { return false }
        guard newpassword.range(of: #"[0-9].*[0-9]"#, options: .regularExpression) != nil else { return false }
        guard newpassword.range(of: #"[$@#!%*?&]"#, options: .regularExpression) != nil else { return false }
        return true
    }

    private func validatePassword(_ newpassword: String, _ confirmpassword: String) -> String {
        if otpCode.isEmpty { return "OTP is Required" }
        if newpassword.isEmpty { return "New Password is Required" }
        if confirmpassword.isEmpty { return "Confirm Password is Required" }
        if newpassword.count < 8 { return "At least 8 characters" }
        if newpassword.range(of: #"[A-Z].*[A-Z]"#, options: .regularExpression) == nil { return "Minimum 2 Uppercase letters" }
        if newpassword.range(of: #"[0-9].*[0-9]"#, options: .regularExpression) == nil { return "Minimum 2 Numbers" }
        if newpassword.range(of: #"[$@#!%*?&]"#, options: .regularExpression) == nil { return "At least 1 special character" }
        if newpassword != confirmpassword { return "New Password and Confirm Password does not match" }
        let username = UserDefaults.standard.string(forKey: "usernameSaved") ?? ""
        if newpassword == username || newpassword.contains(username) {
            return "Don't use Username as Password"
        }
        return "Valid"
    }

    private func dismissKeyboard() {
        UIApplication.shared.windows.first { $0.isKeyWindow }?.endEditing(true)
    }
}

extension AlertItem {
    var alert: Alert {
        Alert(title: title, message: message, dismissButton: dismissButton)
    }
}


struct SecureInputResetView: View {
    
    @Binding private var text: String
    @State private var isSecured: Bool = true
    private var title: String
    
    init(_ title: String, text: Binding<String>) {
        self.title = title
        self._text = text
    }
    
    var body: some View {
        ZStack(alignment: .trailing) {
            if isSecured {
                SecureField(title, text: $text)
                    .opacity(1)  // Ensure visibility when password is hidden
                    .onTapGesture {
                        // Enable text input
                        UIApplication.shared.sendAction(#selector(UIResponder.becomeFirstResponder), to: nil, from: nil, for: nil)
                    }
            } else {
                TextField(title, text: $text)
                    .opacity(1)  // Ensure visibility when password is visible
                    .onTapGesture {
                        // Enable text input
                        UIApplication.shared.sendAction(#selector(UIResponder.becomeFirstResponder), to: nil, from: nil, for: nil)
                    }
            }
            
            Button(action: {
                isSecured.toggle()
            }) {
                Image(systemName: self.isSecured ? "eye.slash" : "eye")
                    .renderingMode(.template)
                    .accentColor(Color("TextColorGray"))
                    .padding(.horizontal, 8)
            }
        }
        .padding(.horizontal)  // Add padding to ensure the button doesn't overlay the text field
    }
}

struct ToastView: View {
    var message: String
    
    var body: some View {
        ZStack {
            RoundedRectangle(cornerRadius: 25, style: .continuous)
                .foregroundColor(Color.black.opacity(0.7))
            Text(message)
                .foregroundColor(.white)
                .font(.headline)
                .padding()
        }
        .frame(minWidth: 0, maxWidth: .infinity, minHeight: 0, maxHeight: .infinity, alignment: .center)
        .edgesIgnoringSafeArea(.all)
    }
}
