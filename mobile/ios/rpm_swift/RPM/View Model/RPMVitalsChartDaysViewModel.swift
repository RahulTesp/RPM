//
//  RPMVitalsGraphViewModel.swift
//  RPM
//
//  Created by Tesplabs on 11/07/1944 Saka.
//


import Foundation
import SwiftUI

@MainActor

final class RPMVitalsChartDaysViewModel: ObservableObject {
    @Published var vitalsGraph7: [RPMVitalsChartDaysDataModel] = []
    @Published var vitalsGraph30: [RPMVitalsChartDaysDataModel] = []
    @Published var showNoInternetAlert = false
    //@Published var vitalsGraph7 : RPMVitalsChartDaysDataModel?
    //@Published var vitalsGraph30 : RPMVitalsChartDaysDataModel?
   // @Published var vitalsLVM : LatestVitalMeasurel?
    @Published var loading = true
    
    init() {
      graph7Days()
      graph30Days()
    }
    
    
    func graph7Days() {
        guard let tkn = UserDefaults.standard.string(forKey: "jsonwebtoken") else { return }

        DashboardManager.shared.graph7Vitals(tkn: tkn) { result in
            DispatchQueue.main.async {
            switch result {
            case .success(let account):
                self.vitalsGraph7 = account
                self.loading = false
                print("account7CHART:", account)
            case .failure(let error):
                self.loading = false
                if case .noInternet = error {
                    self.showNoInternetAlert = true
                } else {
                    print("Unhandled error: \(error)")
                }
            }
        }
        }
    }



    func graph30Days() {
        guard let tkn = UserDefaults.standard.string(forKey: "jsonwebtoken") else {
            return
        }

        DashboardManager.shared.graph30Vitals(tkn: tkn) { result in
       
            DispatchQueue.main.async {
                switch result {
                case .success(let account):
                    self.vitalsGraph30 = account
                    self.loading = false
                
                    print("vitals30Graph success:", account)

                case .failure(let error):
                    self.loading = false
              
                    switch error {
                    case .noInternet:
                        self.showNoInternetAlert = true
                    default:
                        print("Unhandled error: \(error)")
                    }
                    print("vitals30Graph failed")
                }
            }
        }
    }


    func recentGraphVitals() {
        guard let tkn = UserDefaults.standard.string(forKey: "jsonwebtoken") else {
            return
        }

        DashboardManager.shared.graph30Vitals(tkn: tkn) { result in

            DispatchQueue.main.async {
                switch result {
                case .success(let account):
                    self.vitalsGraph30 = account
                    self.loading = true
               
                    print("recentGraphVitals success")

                case .failure(let error):
                    self.loading = true
             
                    switch error {
                    case .noInternet:
                        self.showNoInternetAlert = true
                    default:
                        print("Unhandled error in recentGraphVitals: \(error)")
                    }
                }
           }
        }
    }
}



