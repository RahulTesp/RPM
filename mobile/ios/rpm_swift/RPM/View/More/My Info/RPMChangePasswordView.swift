//
//  RPMChangePasswordView.swift
//  RPM
//
//  Created by Tesplabs on 31/04/1944 Saka.
//

import SwiftUI

struct RPMChangePasswordView: View {
    
    @State private var result = ""
    @State private var showingAlerts = false
    @State private var showingWrongAlerts = false
    @State private var showingSuccessAlerts = false
    @State private var isLogin = false
    @State var isActive = false
    @State private var changePwdModel = RPMChangePwdViewModel()
    @EnvironmentObject var loginViewModel: RPMLoginViewModel
    @State private var currentpassword: String = ""
    @State private var newpassword: String = ""
    @State private var confirmPassword: String = ""
    @EnvironmentObject var navigationHelper: NavigationHelper
    @Environment(\.presentationMode) var presentationMode
    
    var body: some View {
        ZStack{
            BackgroundView()
            
            VStack(alignment: .leading)
            {
                Group
                {
                    Text("Change Password")
                        .foregroundColor(.black)
                        .font(.system(size:26 , weight: .semibold))
                        .padding(.bottom,15)
                   
                    Text("  Current Password")
                        .foregroundColor(Color("title1"))
                        .font(Font.system(size: 15))
                    
                    SecureInputView("Enter Current Password", text: $currentpassword)
                        .textFieldStyle(MyTextFieldStyle()).cornerRadius(10)
                        .disableAutocorrection(true)
                    
                    Text("  New Password")
                        .foregroundColor(Color("title1"))
                        .font(Font.system(size: 15))
                    
                    SecureInputView(
                        "Enter New Password",
                        text: $newpassword
                    )   .disableAutocorrection(false)
                        .textFieldStyle(MyTextFieldStyle()).cornerRadius(10)
                    
                    Text("  Confirm New Password")
                        .foregroundColor(Color("title1"))
                        .font(Font.system(size: 15))
                    
                    SecureInputView(
                        "Re-enter New Password",
                        text: $confirmPassword
                    )   .disableAutocorrection(false)
                        .textFieldStyle(MyTextFieldStyle()).cornerRadius(10)
                    
                    Group
                    {
 
                        Button("Reset", action: {
                            result = validatePassword(newpassword, confirmPassword)
                            print("result:", result)
                            
                            if result != "Password changed Successfully!" {
                                showingAlerts = true
                            }
                            
                            if result == "Password changed Successfully!" {
                                if newpassword == confirmPassword {
                                    changePwdModel.changePwd(
                                        userName: (UserDefaults.standard.string(forKey: "usernameSaved") ?? ""),
                                        oldPassword: currentpassword,
                                        newPassword: newpassword,
                                        completed: { success, _ in
                                            if success {
                                                showingSuccessAlerts = true
                                               // UserDefaults.resetStandardUserDefaults()
                                                
                                                if let bundleID = Bundle.main.bundleIdentifier {
                                                    UserDefaults.standard.removePersistentDomain(forName: bundleID)
                                                    UserDefaults.standard.synchronize()
                                                }

                                                // Navigate to login screen
                                             //   navigationHelper.path.append(.login)
                                                loginViewModel.isAuthenticated = false
                                                
                                            } else {
                                                showingWrongAlerts = true
                                            }
                                        }
                                    )
                                }
                            }
                        })
                        .frame(width: 330, height: 50)
                        .background(Color("lightGreen"))
                        .foregroundColor(Color("title1"))
                        .cornerRadius(15)
                        .padding()

                            .alert(result, isPresented: self.$showingAlerts) {
                                Button("OK", role: .cancel) {
                                }
                            }
                            .alert("Current Password may be wrong", isPresented: self.$showingWrongAlerts){
                                Button("OK", role: .cancel) {   }
                            }
                            .alert("Password changed Successfully!", isPresented: self.$showingSuccessAlerts)

                        {
                            Button("OK", role: .cancel) {  self.loginViewModel.isLoggedOut = true
                                print("  self.loginViewModel.isLoggedOut")
                               // print( self.loginViewModel.isLoggedOut)
                                
                              //  self.loginViewModel.loggedIn = false
                             //   UserDefaults.standard.set(self.loginViewModel.loggedIn, forKey: "loggedInValue" )
                                print("loggedin Value2")
                              //  print(UserDefaults.standard.bool(forKey: "loggedInValue") )
                                print("self.isActive 3")
                              //  print(self.isActive)
                                
                                UserDefaults.resetStandardUserDefaults()
                              
                             //   self.mode.wrappedValue.dismiss()
                                
                                print("self.isActive 1")
                                print(self.isActive)
                                self.isActive = true
                                print("self.isActive 2")
                                print(self.isActive) }
                        }
                    }
                    
                }
                Spacer()
            }
            
            .padding(.top,40)
            .padding(.horizontal,10)
            .navigationBarBackButtonHidden(true)
            
            .navigationBarTitleDisplayMode(.inline)
            .toolbar {
                ToolbarItem(placement: .principal) {
                    Text("Change Password")
                        .font(.system(size:18))
                        .accessibilityAddTraits(.isHeader)
                }
            }
            
            .navigationBarItems(leading:
                                  
                                    Button(action: {
                self.presentationMode.wrappedValue.dismiss()
            }) {
                
                
                Image("ArrowBack").renderingMode(.template)
                    .foregroundColor(  Color("buttonColor"))
                
            },
                           
                                trailing:
                                
                                    Button(action: {
                self.presentationMode.wrappedValue.dismiss()
            }) {
               
                Text("Cancel") .foregroundColor(Color("buttonColor"))
                
            }
                            
            )
        }
    }
    
}


// NOTE : PASSWORD VALIDATION FUCTION
func validatePassword(_ password: String,_ cpassword: String) -> String {
    let savedPassword = UserDefaults.standard.string(forKey: "passwordSaved") ?? ""
    
    // New password is same as the current saved password
      if password == savedPassword {
          return "Don't use the current password as the new one"
      }
    
    if password == (UserDefaults.standard.string(forKey: "usernameSaved") ?? "") {
        return "Username cannot be used as Password"
    }
    
    if password.contains(UserDefaults.standard.string(forKey: "usernameSaved") ?? "") {
        return "Don't use Username as Password"
    }
    
    
    //At least 8 characters
    if password.count < 8 {
        return "At least 8 characters"
    }
    
    
    //At least two letter
    if password.range(of: #"[A-Z].*[A-Z]"#, options: .regularExpression) == nil {
        return "Minimum 2 Uppercase letters"
    }
    
    //At least one digit
    if password.range(of: #"[0-9].*[0-9]"#, options: .regularExpression) == nil {
        return "Minimum 2 Numbers"
    }
    
    //No symbols charcters
    if password.range(of: #"[$@#!%*?&]"#, options: .regularExpression) == nil {
        return "At least 1 special character"
    }
    if password != cpassword {
        return "New Password and Confirm Paswwords are not same"
    }
 
    return "Password changed Successfully!"
}
