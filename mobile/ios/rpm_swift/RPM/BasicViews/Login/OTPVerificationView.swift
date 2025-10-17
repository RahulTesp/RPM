
import SwiftUI
 
@available(iOS 15, *)
public struct OtpView_SwiftUI: View {
    @Binding var otpCode: String
    var otpCodeLength: Int
    var textColor: Color
    var textSize: CGFloat
    @FocusState private var isTextFieldFocused: Bool
    @EnvironmentObject var loginViewModel: RPMLoginViewModel
    @EnvironmentObject var navigationHelper: NavigationHelper
    @State private var alertItem: AlertItem?
    @EnvironmentObject var appModel: AppModel
    // MARK: Constructor
    public init(otpCode: Binding<String>, otpCodeLength: Int, textColor: Color, textSize: CGFloat) {
        self._otpCode = otpCode
        self.otpCodeLength = otpCodeLength
        self.textColor = textColor
        self.textSize = textSize
    }
    public var body: some View {
        ZStack {
            // Invisible TextField for input
            TextField("", text: $otpCode)
                .keyboardType(.numberPad)
                .textContentType(.oneTimeCode)
                .accentColor(.clear)
                .foregroundColor(.clear)
                .frame(width: 1, height: 1) // non-zero
                .focused($isTextFieldFocused)
                .onChange(of: otpCode) { newValue in
                    // Limit length
                    if newValue.count > otpCodeLength {
                        otpCode = String(newValue.prefix(otpCodeLength))
                    }
                    // Auto verify when full
                    if otpCode.count == otpCodeLength {
                        verifyOTP()
                    }
                }
            // OTP boxes
            HStack(spacing: 8) {
                ForEach(0..<otpCodeLength, id: \.self) { index in
                    ZStack {
                        Rectangle()
                            .stroke(Color("lightGreen"), lineWidth: 1)
                            .frame(width: 40, height: 50)
                        Text(getPin(at: index))
                            .font(.system(size: textSize))
                            .foregroundColor(textColor)
                    }
                    .onTapGesture { isTextFieldFocused = true }
                }
            }
        }
        .onAppear { isTextFieldFocused = true } // focus on appear
        .alert(item: $alertItem) { item in
            Alert(title: Text(item.title),
                  message: item.message.map { Text($0) },
                  dismissButton: .default(Text("OK")))
        }
    }
    private func getPin(at index: Int) -> String {
        return index < otpCode.count ? String(otpCode[index]) : ""
    }
    private func verifyOTP() {
        loginViewModel.verifyOtp(
            userName: UserDefaults.standard.string(forKey: "pgmUserID") ?? "",
            otp: otpCode
        ) { token, alert in
            if let token = token {
                if UserDefaults.standard.string(forKey: "MFAENABLEDTRUE") == "MFAENABLEDTRUE" {
                    signInOTP(chatacctoken: token)
                }
                navigationHelper.path.append(.tabBarView)
            } else {
                alertItem = alert
            }
        }
    }
    private func signInOTP(chatacctoken: String) {
        // Your chat login logic
        print("ManualSigninOTPPAGE",chatacctoken)
         
                appModel.client.create(chatacctoken : chatacctoken ,delegate: appModel) { [self] result in
                    DispatchQueue.main.async {
                        switch result {
                        case .success:
                            // remember current user and identity here
                            appModel.saveUser(appModel.client.conversationsClient?.user)
                            if let user = appModel.client.conversationsClient?.user {
                                print("appModelconvClientuser", user)
                            } else {
                                print("appModelconvClientuser is nil")
                            }
         
                        case .failure(let error):
                            NSLog("Sign in failed with error: \(error.localizedDescription)")
                            if case .failure(LoginError.accessDenied) = result {
                                NSLog("Sign in failed, erasing credentials")
                            } else {
                            }
                        }
                    }
                }
    }
}
 
extension String {
    subscript(idx: Int) -> String {
        String(self[index(startIndex, offsetBy: idx)])
    }
}
