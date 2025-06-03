//
//  NotificationScrollView.swift
//  RPM
//
//  Created by Tesplabs on 30/04/1944 Saka.
//

import SwiftUI

struct NotificationScrollView: View {
    
    var time: String
    var decription: String
    var fontColor1 : Color
    var fontColor2 : Color
    var bgColor: Color
    
    
    var body: some View {
        VStack (alignment:.leading){
            
            HStack{
                
                Spacer()
                
                Text(time)
                    .font(.system(.subheadline, design: .rounded))
                    .foregroundColor(fontColor2)
                
            }
            
            Text(decription)
                .font(.system(.subheadline, design: .rounded))
                .foregroundColor(fontColor2)
                .padding(.vertical,0.5)
        }
        .frame(maxWidth: .infinity, minHeight: 80)
        .padding(.horizontal,10)
        .padding(.vertical,10)
        .background(bgColor)
        .cornerRadius(15)
    }
}
