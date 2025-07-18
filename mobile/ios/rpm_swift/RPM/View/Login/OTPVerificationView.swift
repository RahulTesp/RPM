import SwiftUI
import Combine

@available(iOS 15, *)
public struct OtpView_SwiftUI: View {
    
  //  @State var usrlokd: Bool = false
    @State private var shouldShowLoginAlert: Bool = false
   // @ObservedObject private var viewModel = RPMLoginViewModel()
    @State private var alertItem: AlertItem?
 //  @State var isActive = false
    @EnvironmentObject var appModel: AppModel
    @State private var isLogin = false
    @FocusState private var isTextFieldFocused: Bool
    @EnvironmentObject var navigationHelper: NavigationHelper
    @EnvironmentObject var loginViewModel: RPMLoginViewModel
    
    //MARK: Fields
    enum FocusField: Hashable {
        case field
    }
    
    @FocusState private var focusedField: FocusField?
    @Binding var otpCode: String
    var otpCodeLength: Int
    var textColor: Color
    var textSize: CGFloat
    
    //MARK: Constructor
     init(otpCode: Binding<String>, otpCodeLength: Int, textColor: Color, textSize: CGFloat) {
        self._otpCode = otpCode
        print("self._otpCode",self._otpCode.wrappedValue)
        self.otpCodeLength = otpCodeLength
        self.textColor = textColor
        self.textSize = textSize
        
    }
    
    //MARK: Body
    public var body: some View {
        HStack {
       
            ZStack(alignment: .center) {

                TextField("", text: $otpCode ,
                          
                          onEditingChanged: { (changed) in
                    // Editing has finished
                    print("$changed")
                    print(changed)
                    print("Body - OTP Code:", self._otpCode.wrappedValue)
               
                    print("self.focusedField ")
                    print(self.focusedField )
                    
                    print("COMMITED!");
                    
                    print("$otpCodeFFFF",otpCode)
                    print("$otpCodeFFFF",$otpCode)
                    print("OTP Code:", $otpCode.wrappedValue)
 
                },
             
                          onCommit: {
                    print("onCommitotpCode")
                    print(otpCode)
                    
                    print($otpCode)
                })
                
              
            
                .frame(width: 0, height: 0, alignment: .center)
                .font(Font.system(size: 0))
                .accentColor(.clear)
            
                .foregroundColor(.clear)
                .multilineTextAlignment(.center)
                .keyboardType(.numberPad)
 
                .focused($focusedField, equals: .field)
 
                               .onTapGesture {
                                   self.focusedField = .field
                                   print("isTextFieldFocused")
                                   print(isTextFieldFocused)
                                   isTextFieldFocused = true
                               }
                
                               .onChange(of: otpCode) { newOtpCode in
                                   
                                   print("otpCodeLengthVAL",otpCodeLength)
                                   print("newOtpCode",newOtpCode)
                                   if newOtpCode.count == otpCodeLength {
                              
                                       loginViewModel.verifyOtp( userName: (UserDefaults.standard.string(forKey: "pgmUserID") ?? ""), otp: otpCode, completed: { token, alertItem in
                                           if let _ = token {
                                               
                                               print("otpVerifTokn",token)
                                               
                                               if(UserDefaults.standard.string(forKey: "MFAENABLEDTRUE") == "MFAENABLEDTRUE")
                                               {
                                                   print("signInOTP","signInOTP")
                                                   signInOTP(chatacctoken: token ?? "")
                                                   
                                               }
                                        
                                               //permanent token
                                               let defaults = UserDefaults.standard
                                         
                                               print(alertItem)
                                               
                                               print("isLogin otp")
                                           
                                             //  self.isActive = true
                                               navigationHelper.path.append(.tabBarView)

                                               print("self.isActive 2")
                                              // print(self.isActive)
                                              
                                               print("isAuthenticated value")
                                               print(loginViewModel.self.isAuthenticated)
                                               print(loginViewModel.isAuthenticated)
                                          
                                             //  self.loginViewModel.loggedIn = true
                                              // UserDefaults.standard.set(self.viewModel.loggedIn, forKey: "loggedInValue" )
                                               print("loggedin Value1")
                                               print(UserDefaults.standard.bool(forKey: "loggedInValue") )
                                               print("isAuthenticated value2222222")
                                               print(loginViewModel.self.isAuthenticated)
                                               print(loginViewModel.isAuthenticated)
                                           
                                           }else {
                                               shouldShowLoginAlert = true
                                               self.alertItem = alertItem
                                           }
                                      
                                       })
                                       
                                       
                                       DispatchQueue.main.asyncAfter(deadline: .now() + 0.1) {
                                           self.focusedField = .field
                                               
                                             }
                                
                                   }
                               }
              
                .task {
                    DispatchQueue.main.asyncAfter(deadline: .now() + 0.5)
                    {
                      
                        print("$otpCode2",otpCode)
                  
                        print("$otpCode2",$otpCode)
                        print("selfotpCodewrappedValue",self._otpCode.wrappedValue)
                   
                    }
                }
                .padding()
                HStack {
                    ForEach(0..<otpCodeLength) { index in
                        ZStack {
                            Text(self.getPin(at: index))
                                .font(Font.system(size: textSize))
                                .fontWeight(.semibold)
                                .foregroundColor(textColor)
                       
                                     Rectangle()
                                         .frame(width: 30, height: 35)
                                         .foregroundColor(Color.orange.opacity(0.0))
                                         .overlay(RoundedRectangle(cornerRadius: 3).stroke(Color("lightGreen"), lineWidth: 1))
                             
                                         .padding(.trailing, 5)
                                         .padding(.leading, 5)
                                         .opacity(self.otpCode.count <= index ? 1 : 1)
                      
                        }
                        .background(Color("bgColorDark"))
                        .onTapGesture {
                            print("Rectangle")
                            self.focusedField = .field
                        }
                    
                       .background(Color("bgColorDark"))
                       .onTapGesture {
                           print("Rectangle")
                           self.focusedField = .field
                       }
                    }
                }
             
            }
           
            .onSubmit {
                print("Authenticating…")
                print(otpCode)
       
            }
        }
     
        .alert(item: $alertItem) { alertItem in
          
            Alert(title: alertItem.title, message: alertItem.message, dismissButton: .default(Text("OK"), action: {
                print("alertItem.title", alertItem.title)
                print("alertItem.message", alertItem.message)
                print( (UserDefaults.standard.bool(forKey: "userLocked") ))
                               if( ((UserDefaults.standard.bool(forKey: "userLocked") ) == true))
                                {
                                   
                                   print("ok clik lok")
                                   print( (UserDefaults.standard.bool(forKey: "userLocked") ))
                                   print("Ok Click")
                                //   self.usrlokd = true
//                                   DispatchQueue.main.async {
//                                                     navigationHelper.path.append(.login)
//                                                 }
                                 

                               }
                if( ((UserDefaults.standard.bool(forKey: "otpWrong") ) == true))
                 {
                    otpCode = ""
                    print("otpCodeempty",otpCode)
                    print( (UserDefaults.standard.bool(forKey: "otpWrong") ))
                    print("Ok Click")
                   
                }
                
                else
                {
                   
                }
           
                       }))
         
        }
    
    }
    
    
    func signInOTP(chatacctoken : String) {
        print("ManualSigninOTPPAGE",chatacctoken)

        appModel.client.create(chatacctoken : chatacctoken ,delegate: appModel) { [self] result in
            DispatchQueue.main.async {
                switch result {
                case .success:
               
                    // remember current user and identity here
                    appModel.saveUser(appModel.client.conversationsClient?.user)
print("appModelconvClientuser",appModel.client.conversationsClient?.user)
        
                case .failure(let error):
           
                    if case .failure(LoginError.accessDenied) = result {
                        NSLog("Sign in failed, erasing credentials")
                 
                    } else {
                       // self.errorMessage = error.localizedDescription
                    }
                }
            }
        }
    }
    
    
    func dismissKeyboardotp() {
         UIApplication.shared.windows.filter {$0.isKeyWindow}.first?.endEditing(true) // 4
       }
    //MARK: func
    private func getPin(at index: Int) -> String {
        guard self.otpCode.count > index else {
            return ""
        }
        return self.otpCode[index]
    }
    
    private func limitText(_ upper: Int) {
        if otpCode.count > upper {
            otpCode = String(otpCode.prefix(upper))
        }
    }
}

extension String {
    subscript(idx: Int) -> String {
        String(self[index(startIndex, offsetBy: idx)])
    }
}


