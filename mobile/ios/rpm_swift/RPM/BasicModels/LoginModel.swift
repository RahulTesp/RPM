//
//  LoginModel.swift
//  SwiftUI-Dashboard
//
//  Created by Tesplabs on 13/03/1944 Saka.
//

import Foundation

class LoginModel : ObservableObject{
    var UserName: String = ""
    var Password: String = ""
    @Published var isAuthenticated : Bool = false
    
    func login(){
       // let defaults = UserDefaults.standard
        LoginService().login(UserName: UserName, Password: Password) { result in
            switch result {
            case.success(let token):
                print("token")
                print(token)
                
             //  defaults.setValue( token , forKey: "jsonwebtoken")
              //  DispatchQueue.main.async {
               //     self.isAuthenticated = true
               // }
                
            case.failure(let error):
                print(error.localizedDescription)
            }
    }
}
}
