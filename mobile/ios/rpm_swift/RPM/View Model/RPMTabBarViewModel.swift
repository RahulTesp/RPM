//
//  RPMTabBarViewModel.swift
//  RPM
//
//  Created by Tesplabs on 05/07/1944 Saka.
//



import Foundation
import SwiftUI

class TabBarViewModel: ObservableObject {
    @Published var tabSelection: Int = 0
    @Published var selectedItem: String? = nil
    
    func gotoRootView() {
        withAnimation {
            UIApplication.shared.windows.first?.rootViewController?.dismiss(animated: true, completion: nil)
            selectedItem = nil
        }
    }
}
