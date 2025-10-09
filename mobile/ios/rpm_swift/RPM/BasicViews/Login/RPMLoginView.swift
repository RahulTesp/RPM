//
//  ContentView.swift
//  RPM
//
//  Created by Prajeesh Prabhakar on 30/05/22.
//

import SwiftUI


struct RPMLoginView: View {
    
    @Environment(\.colorScheme) var colorScheme
    @State private var shouldShowLoginAlert: Bool = false
    @State private var showingAlerts = false
    @State private var result = ""
    @EnvironmentObject var loginViewModel: RPMLoginViewModel
    @State private var alertItem: AlertItem?
    @EnvironmentObject var appModel: AppModel
    @EnvironmentObject var messagesManager: MessagesManager
    @EnvironmentObject var conversationManager: ConversationManager
    @EnvironmentObject var navigationHelper: NavigationHelper
    @EnvironmentObject var callManager: CallManager
    @EnvironmentObject var roomManager: RoomManager
    @EnvironmentObject var localParticipant: LocalParticipantManager
    @EnvironmentObject var mediaSetupViewModel: MediaSetupViewModel
    @EnvironmentObject var homeViewModel: RPMHomeViewModel
    @EnvironmentObject var sessionManager: SessionManager
    @State private var showLoginErrorAlert = false
    @State private var loginAlertTitle = ""
    @State private var loginAlertMessage = ""

    
#if DEBUG

    @State private var username: String = ""
    @State private var password: String = ""
    

    
#else
    @State private var username: String = ""
    @State private var password: String = ""
#endif

    @State private var usr: String = ""
    
    var body: some View {
        GeometryReader { geometry in
                let width = geometry.size.width
                let height = geometry.size.height
  
        VStack {

                Group {
                    Text("Log In")
                        .font(Font.custom("Rubik-Regular", size: 16))
                        .foregroundColor(.black)
                        .multilineTextAlignment(.center)
                        .padding(.vertical, 10)
                    
                    Spacer()
                    
                    Text("Hello,")
                        .foregroundColor(Color("title1"))
                        .font(Font.custom("Rubik-Regular", size: 24))
                    
                    Image("logoclynx")
                        .resizable()
                        .frame(width: 160.0, height: 150.0)
                }
                
                TextField("", text: $username, prompt: Text("Enter Username").foregroundColor(Color("ColorGrey")))
                    .colorScheme(.light)
                    .textFieldStyle(MyTextFieldStyle())
                    .cornerRadius(10)
                    .disableAutocorrection(true)
                    .frame(maxWidth: .infinity)
                 
                    .onTapGesture {
                           print("Username tapped")
                       }
                SecureInputView("Enter Password", text: $password)
                    .colorScheme(.light)
                    .frame(maxWidth: .infinity)
                    .disableAutocorrection(false)
                    .textFieldStyle(MyTextFieldStyle())
                    .cornerRadius(10)
                
                Spacer()
             
                Button(action: {
                    // Push Forgot Password screen using navigationPath
                    navigationHelper.path.append(.forgotPassword)
                }) {
                    Text("Forgot your Password ?")
                        .padding(10)
                        .foregroundColor(Color("title1"))
                        .font(Font.custom("Rubik-Regular", size: 14))
                }
                
                Button(action: handleLogin) {
                    Text("LOG IN")
                        .font(Font.custom("Rubik-Regular", size: 16))
                        .frame(height: 50)
                        .frame(maxWidth: .infinity)
                        .background(Color("buttonColor"))
                        .foregroundColor(.white)
                        .cornerRadius(15)
                       
                }
                .padding(.horizontal, 30)
                
                .alert(loginAlertTitle, isPresented: $showLoginErrorAlert) {
                    Button("OK", role: .cancel) {
                        showLoginErrorAlert = false
                    }
                } message: {
                    Text(loginAlertMessage)
                }

                .padding(.horizontal, 22)
      
            Spacer()
        }
        .navigationBarBackButtonHidden(true)
        .frame(width: width, height: height)
        .background(Color("ChatBGcolor"))
        .onAppear {
            let systemTimeZone = TimeZone.current
            print("NSTimeZone.default", NSTimeZone.default)
            print("systemTimeZone", systemTimeZone)
            print("geometry size:", width, height, "isNaN:", width.isNaN, height.isNaN)
          
            print("LoginView appeared, isAuthenticated = \(loginViewModel.isAuthenticated)")
            print(" Path:", navigationHelper.path)
        }
    }
}
    
    
    func handleLogin() {
        
        print("handleLogin")
        let validationResult = validateUsernamePassword(username, password)

        guard validationResult == "Valid" else {

            alertItem = AlertItem(
                title: "Error",
                message: validationResult,
                dismissButton: .default(Text("OK"))
            )

            shouldShowLoginAlert = true
            return
        }

        UserDefaults.standard.set(username, forKey: "pgmUserID")

        loginViewModel.login(userName: username, password: password) { token, alert in
            
      
            if let token = token {
                // Successful login
              
                UserDefaults.standard.set(username, forKey: "usernameSaved")
                UserDefaults.standard.set(password, forKey: "passwordSaved")

                print("Checking MFA flag:", UserDefaults.standard.string(forKey: "MFAENABLEDFALSE") ?? "nil")

                if UserDefaults.standard.string(forKey: "MFAENABLEDFALSE") == "MFAENABLEDFALSE" {
                    print("mfafalse")
                    signIn(chatacctoken: token)
                    
                }

                print(" navigationHelper.pathlogin: \( navigationHelper.path)")
                
            } else {
                // Login failed
                shouldShowLoginAlert = true
                
            
                loginAlertTitle = alert?.title ?? "Error"
                loginAlertMessage = alert?.message ?? ""

                showLoginErrorAlert = true

                print("loginAlertTitle",loginAlertTitle)
                print("loginAlertMessage",loginAlertMessage)
              
                
                print(self.alertItem ?? "nil alertItem")
                print("alertItem title")
                print(self.alertItem?.title ?? "nil title")
            }

            username = ""
            password = ""
            dismissKeyboard()
        }
    }

    
    func signIn(chatacctoken: String) {
        NSLog("ManualSignin")
        print("ManualSignin")
        appModel.client.create(chatacctoken: chatacctoken, delegate: appModel) { [self] result in
            DispatchQueue.main.async {
                switch result {
                case .success:
                    appModel.saveUser(appModel.client.conversationsClient?.user)
              
                    if let user = appModel.client.conversationsClient?.user {
                        print("appModelconvClientuser", user)
                    } else {
                        print("appModelconvClientuser is nil")
                    }

                case .failure(let error):
                    print("Sign in failed with error: \(error.localizedDescription)")
                    if case .failure(LoginError.accessDenied) = result {
                        
                        if let user = appModel.client.conversationsClient?.user {
                            print("appModelconvClientuserfail", user)
                        } else {
                            print("appModelconvClientuser is nil")
                        }

                        
                        NSLog("Sign in failed, erasing credentials")
                    } else {
                        // Handle other error cases
                    }
                }
            }
        }
    }

    func validateUsernamePassword(_ username: String, _ password: String) -> String {
        if username.isEmpty || password.isEmpty {
            return "Username and Password is Required"
        }
        return "Valid"
    }

    
    func dismissKeyboard() {
        DispatchQueue.main.async {
            UIApplication.shared
                .connectedScenes
                .compactMap { $0 as? UIWindowScene }
                .flatMap { $0.windows }
                .first { $0.isKeyWindow }?
                .endEditing(true)
        }
    }

}

extension Text {
    var string: String {
        // Not fully accurate, but fallback if needed
        "\(self)"
    }
}



// MARK: UITextFieldViewRepresentable
struct UITextFieldViewRepresentable: UIViewRepresentable {
    
    @Binding var text: String
    typealias UIViewType = ProtectedTextField
    
    
    func makeUIView(context: Context) -> ProtectedTextField {
        let textField = ProtectedTextField()
        textField.delegate = context.coordinator
        return textField
    }
    
    // From SwiftUI to UIKit
    func updateUIView(_ uiView: ProtectedTextField, context: Context) {
        uiView.text = text
    }
    
    // From UIKit to SwiftUI
    func makeCoordinator() -> Coordinator {
        return Coordinator(text: $text)
    }
    
    class Coordinator: NSObject, UITextFieldDelegate {
        @Binding var text: String
        
        init(text: Binding<String>) {
            self._text = text
        }
        
        func textFieldDidChangeSelection(_ textField: UITextField) {
            text = textField.text ?? ""
        }
    }
}

// Custom TextField with disabling paste action
class ProtectedTextField: UITextField {
    override func canPerformAction(_ action: Selector, withSender sender: Any?) -> Bool {
        
        if action == #selector(paste(_:)) {
            return false
        }
        if action == #selector(cut(_:)) {
            return false
        }
        return super.canPerformAction(action, withSender: sender)
    }
}
struct MyTextFieldStyle: TextFieldStyle {
    func _body(configuration: TextField<Self._Label>) -> some View {
        configuration
            .padding(12)
            .background(
                RoundedRectangle(cornerRadius: 10)
                    .stroke(Color("textFieldBG"), lineWidth: 2)
                    .background(Color("textFieldBG"))
                    .foregroundColor(Color("TextColorGray"))
                    .cornerRadius(10)
                
            )
            .padding(.vertical,5)
            .padding(.horizontal, 25)
         
    }
}


struct SecureInputView: View {
    
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
                        .textContentType(nil)
                        .foregroundColor(.black)
        
                } else {
                    TextField(title, text: $text)
                        .textContentType(nil)

                }
      
                Button(action: {
                    isSecured.toggle()
                }) {
                  
                    Image(systemName: self.isSecured ? "eye.slash" : "eye")
                        .renderingMode(.template)
                        .accentColor(Color("TextColorGray"))
            
                        .padding(.horizontal, 60)
                }
          
        }
    }
}
                      
                      
