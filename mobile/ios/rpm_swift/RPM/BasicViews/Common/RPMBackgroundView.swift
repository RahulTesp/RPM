    //
    //  RMPBackgroundView.swift
    //  RPM
    //
    //  Created by Prajeesh Prabhakar on 30/05/22.
    //

    import SwiftUI

    struct BackgroundView: View {
        var body: some View {

            LinearGradient(gradient: Gradient(colors: [Color("bgColorDark"), Color("bgColor")]), startPoint: .topLeading, endPoint: .bottomTrailing)
                .edgesIgnoringSafeArea(.all)
        }
    }

