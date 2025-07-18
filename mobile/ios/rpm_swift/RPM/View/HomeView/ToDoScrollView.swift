//
//  ToDoScrollView.swift
//  RPM
//
//  Created by Tesplabs on 17/03/1944 Saka.
//

import SwiftUI

struct ToDoScrollView: View {
    var schedType : String
    var date : String
    var desc : String

    var body: some View {
        VStack (alignment:.leading){
            
            HStack{
                
                Text(schedType)
                    .font(Font.custom("Rubik-Regular", size: 18))
                    .foregroundColor(Color("title1"))
                
                Spacer()

                Text(converttodoDateFormat(inputDate: date) )
                    .font(Font.custom("Rubik-Regular", size: 16))
                    .foregroundColor(.black)
            }
            
            Text(desc)
                .font(Font.custom("Rubik-Regular", size: 16))
                .foregroundColor(.black)
                .padding(.vertical,0.5)
            
            
        }
        .padding(.horizontal,10)
        .padding(.vertical,6)
        .background(.white)
        .cornerRadius(15)
    }
}

