//
//  ContentView.swift
//  RPM
//
//  Created by Prajeesh Prabhakar on 30/05/22.
//

import SwiftUI

struct RPMOTPView: View {
    @Environment(\.colorScheme) var colorScheme
   var  widthFull = UIScreen.main.bounds.width
    @State private var shouldShowLoginAlert: Bool = false
    
    @State private var restartTimer = false
    
    @State private var showingAlerts = false
    @State private var result = ""
    
    @State private var showText = false
    @State private var showResend = false
    
    var width: CGFloat {
          if UIDevice.current.userInterfaceIdiom == .phone {
              return UIScreen.main.bounds.width / 1.39
          } else {
              return UIScreen.main.bounds.width / 1.3
          }
      }
    
    @State var isActive = false
    @Binding var isLogin: Bool
    @ObservedObject private var viewModel = RPMLoginViewModel()
    @State private var alertItem: AlertItem?
    @State var otpCode: String = ""
    @State var otpCodeLength: Int = 8
    @State var textColor = Color.black
    @State var textSize = CGFloat(27)
    
    var body: some View {

            VStack {
             
                if viewModel.isLoggedOut {
                 
                    RPMLoginView()
               
                } else {
                 
                    Text("Log In")
                        .font(Font.custom("Rubik-Regular", size: 16))
              
                        .foregroundColor(.black)
                
                        .multilineTextAlignment(.center)
                        .padding(.vertical,10)
                  
                    Image("logocylinx")
                        .resizable()
                        .frame(width: 160.0, height: 150.0)
                        .padding(10)
                    //NOTE : COUNT DOWN PROGRESS TIMER
                    CountdownView(otpCode: $otpCode,showText : $showText, showResend: $showResend , restartTimer : $restartTimer)
                        .padding(10)
                    
                    Text("Enter the Code received on your Mobile number " + (UserDefaults.standard.string(forKey: "MobileNumber") ?? ""))
                        .foregroundColor(Color("TextColorBlack"))
                        .font(Font.custom("Rubik-Regular", size: 16))
                        .padding(10)
                        .fixedSize(horizontal: false, vertical: true)
                    
                        .multilineTextAlignment(.center)
                
                    
                    OtpView_SwiftUI(otpCode: $otpCode, otpCodeLength: otpCodeLength, textColor: textColor, textSize: textSize)
                        .padding(10)
                    Spacer()
                    if showText {
                        Text("Not Received a Verification Code?")
                            .foregroundColor(Color("TextColorBlack"))
                            .font(Font.custom("Rubik-Regular", size: 14))
                            .padding(10)
                    } else {
                        Text("")
                            .foregroundColor(Color("TextColorBlack"))
                            .font(Font.custom("Rubik-Regular", size: 14))
                            .padding(10)
                    }
             
                    if showText {
                      
                        Button(action: {
                            print("Button action")
                      
                            self.showText = false
                            self.showResend = false
                            self.restartTimer = true
                            
                            showingAlerts = true
                            self.shouldShowLoginAlert = true //trigger Alert
                      
                            print(UserDefaults.standard.string(forKey: "pgmUserID") ?? "")
                            
                            print(UserDefaults.standard.string(forKey: "passwordSaved") ?? "")
                            
                            viewModel.login(userName: (UserDefaults.standard.string(forKey: "pgmUserID") ?? ""), password: (UserDefaults.standard.string(forKey: "passwordSaved") ?? ""), completed: { token, alertItem in
                                if let _ = token {
                                    
                                    print("isLogin")
                                    print(isLogin)
                                
                                    self.isActive = true
                                    print("self.isActive 2")
                                    print(self.isActive)
                                
                                    print("isAuthenticated value")
                                    print(viewModel.self.isAuthenticated)
                                    print(viewModel.isAuthenticated)
                                
                                    self.viewModel.loggedIn = true
                                    UserDefaults.standard.set(self.viewModel.loggedIn, forKey: "loggedInValue" )
                                    print("loggedin Value1")
                                    print(UserDefaults.standard.bool(forKey: "loggedInValue") )
                                    print("isAuthenticated value2222222")
                                    print(viewModel.self.isAuthenticated)
                                    print(viewModel.isAuthenticated)
                                    
                                    print("alertItem1")
                                    print(self.alertItem)
                                
                                }else {
                                    shouldShowLoginAlert = true
                                    self.alertItem = alertItem
                                    print("alertItem2")
                                    print(self.alertItem)
                                    print("alertItem title ")
                                    print(self.alertItem?.title)
                                    
                                }
                            
                            })
                      
                        }) {
                            Text("Resend OTP")
                            
                                .font(Font.custom("Rubik-Regular", size: 16))
                                .frame(width: width, height: 50)
                            
                                .background(  showText == true ? Color("lightGreen") : Color("bgColorDark") )
                                .foregroundColor(showText == true ? Color("title1") : Color("bgColorDark"))
                          
                                .cornerRadius(15)
                         
                        }
                    }
                   
                    else
                    {
                        Button(action: {
                            
                            print("Button action")
                      
                            
                        }) {
                            Text("Resend OTP")
                            
                                .font(Font.custom("Rubik-Regular", size: 16))
                                .frame(width: width, height: 50)
                            
                                .background(  showText == true ? Color("lightGreen") : Color("bgColorDark") )
                                .foregroundColor(showText == true ? Color("title1") : Color("bgColorDark") )
                       
                                .cornerRadius(15)
                         
                        }
               
                }
      
                }
        
                Spacer()
            
            }.navigationBarBackButtonHidden(true)
         
                .frame(minWidth: widthFull)
            
                .background(Color("bgColorDark"))
      
        }
    
    // NOTE : OTP VALIDATION FUCTION
    func validateOTP(_ otp: String ) -> String {
        
        
        if otp.isEmpty  {
            return "OTP is Required"
        }
        return "Valid"
    }
    
    func dismissKeyboard() {
         UIApplication.shared.windows.filter {$0.isKeyWindow}.first?.endEditing(true) // 4
       }
    
}

struct MyTextFieldotpStyle: TextFieldStyle {
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
       
    }
}
                      
                      
