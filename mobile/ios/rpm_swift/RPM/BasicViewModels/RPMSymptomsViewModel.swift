
//  RPMSymptomsViewModel.swift
//  RPM
//
//  Created by Tesplabs on 26/07/1944 Saka.
//


import Foundation
import SwiftUI

@MainActor

final class RPMSymptomsViewModel: ObservableObject {
    @Published var symDetls : [RPMSymptom]?
    @Published var loading = true
  
    init() {
        sympDetails()
    }
    
    func sympDetails()
    {
       
        let defaults = UserDefaults.standard
        guard let tkn = defaults.string(forKey: "jsonwebtoken")
             
        else {
            
            return
        }
        
        
        MoreManager.shared.symptomsLists(tkn: tkn){ [self] (result ) in
            DispatchQueue.main.async {
            switch result {
            case .success(let account):
               
                self.symDetls = account
                self.loading = false
                print("loading")
                print(self.loading)
                
            case .failure(let error):
         
                self.loading = false

                    print("Unhandled error: \(error)")
            
                print("faileddd")
           
            }
        }
        }
        
    }
}



