//
//  RMPButtonView.swift
//  RPM
//
//  Created by Prajeesh Prabhakar on 30/05/22.
//

import SwiftUI

struct RMPButtonView: View {
    var title: String
    var destination: View
    init<V>(title : String, destination : V) where V: View {
        self.title = title
        self.destination = destination
    }
    
    var body: some View {
        NavigationLink(destination: destination, label: {
            Text(title)
                .bold()
                .font(.title2)
                .frame(width: 280, height: 50)
                .background(Color(.systemRed))
                .foregroundColor(.white)
                .cornerRadius(10)
            
        })
    }
}

