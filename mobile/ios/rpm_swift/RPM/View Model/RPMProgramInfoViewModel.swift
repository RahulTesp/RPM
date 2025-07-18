//
//  RPMProgramInfoViewModel.swift
//  RPM
//
//  Created by Tesplabs on 09/08/1944 Saka.
//



import Foundation
import SwiftUI

@MainActor
final class RPMProgramInfoViewModel: ObservableObject {
    @Published var pgmInfo : ProgramInfo?
    @Published var loading = true
    @Published var showNoInternetAlert = false
    init() {
        pgmInfoDetails()
    }
    
    func pgmInfoDetails()
    {
   
        let defaults = UserDefaults.standard
        guard let tkn = defaults.string(forKey: "jsonwebtoken")
              
        else {
            
            return
        }
      
        MoreManager.shared.pgmDetailsLists(tkn: tkn){ [self] (result ) in
            DispatchQueue.main.async {
            switch result {
            case .success(let account):
       
                self.pgmInfo = account
                self.loading = false
                print("loading")
                print(self.loading)
              
            case .failure(let error):
          
                self.loading = false
                switch error {
                case .noInternet:
                    self.showNoInternetAlert = true
                default:
                    // Optionally show another alert or just log
                    print("Unhandled error: \(error)")
                 
                }
                print("faileddd pgmDetailsLists ")
         
            }
        }
        }
        
    }
}



