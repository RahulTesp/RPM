//
//  RPMMedicationsViewModel.swift
//  RPM
//
//  Created by Tesplabs on 26/07/1944 Saka.
//


import Foundation
import SwiftUI

@MainActor

final class RPMMedicationsViewModel: ObservableObject {
    @Published var medicDetls : [Medication]?
    @Published var loading = true
  
    init() {
        medictnDetails()
    }
    
    func medictnDetails()
    {
       
        let defaults = UserDefaults.standard
        guard let tkn = defaults.string(forKey: "jsonwebtoken")
                
                
        else {
            
            return
        }
        
        
        MoreManager.shared.medicationsLists(tkn: tkn){ [self] (result ) in
            DispatchQueue.main.async {
            switch result {
            case .success(let account):
            
                self.medicDetls = account
                self.loading = false
                print("loading")
                print(self.loading)
            
            case .failure(let error):
          
                self.loading = false
   
                    print("Unhandled error: \(error)")
             
                print("medicationsLists faileddd")
           
            }
        }
        }
    }
}
