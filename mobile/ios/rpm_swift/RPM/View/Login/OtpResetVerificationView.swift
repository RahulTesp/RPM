//
//  OtpResetVerificationView.swift
//  RPM
//
//  Created by Tesplabs on 18/01/24.
//

import SwiftUI
import Combine

@available(iOS 15, *)
public struct OtpResetVerificationView: View {
    
    @State var usrlokd: Bool = false
    @State private var shouldShowLoginAlert: Bool = false
    @ObservedObject private var viewModel = RPMLoginViewModel()
    @State private var alertItem: AlertItem?
    @State var isActive = false
    @State private var isLogin = false
    @FocusState private var isTextFieldFocused: Bool
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
           // let _ = Self._printChanges()
            ZStack(alignment: .center) {
                NavigationLink (
                    
                    destination:
                    
                       // RPMLoginView( isLogin: $isLogin)
                    RPMLoginView()
                    ,
                                        
                    isActive: $usrlokd
        
                )
                {
                    
                }
                
                                    NavigationLink (
                                        
                                        destination:
                                        
                                         RPMTabBarView() ,
                                                            
                                                            isActive: $isActive
                             
                                    )
                                    {
                                        
                                    }
                          
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

                    UserDefaults.standard.set(otpCode, forKey: "otpRP" )
                    print("otpRP11111",(UserDefaults.standard.string(forKey: "otpRP") ?? ""))
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
                                       
                                       UserDefaults.standard.set(otpCode, forKey: "otpRP" )
                                       print("otpRP2222",(UserDefaults.standard.string(forKey: "otpRP") ?? ""))

                                       
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
                                   self.usrlokd = true
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
