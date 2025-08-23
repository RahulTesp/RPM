//
//  RMPPreferenceView.swift
//  RPM
//
//  Created by Tesplabs on 13/04/1944 Saka.
//

import Foundation
import SwiftUI

struct RPMPreferenceView: View {
    
    @ObservedObject  var profList = RPMProgramInfoViewModel()
    
    init() {
        UITextView.appearance().backgroundColor = .clear
    }
    
    var body: some View {
        
        if  profList.loading {
            Group
            {
                Spacer()
                ProgressView()
                    .tint(Color("TextColorBlack"))
                
                Text("Loading…") .foregroundColor( Color("TextColorBlack"))
                Spacer()
            }
            
        } else
        
        {
            ScrollView
            {
                
                VStack(alignment: .leading)
                {
               
                    Text("My Preferences")
                        .foregroundColor(.black)
                        .font(Font.custom("Rubik-SemiBold", size: 24))
                        .padding(.bottom,10)
                    
                    Group
                    {
                        
                        Text("Call Time")
                            .foregroundColor(Color("title1"))
                            .font(Font.custom("Rubik-Regular", size: 14))
                        
                        
                        Text(profList.pgmInfo?.patientDetails.callTime ?? "")
                            .customTextfieldStyle2()
                    }
                    Group
                    {
                        Text("Language")
                            .foregroundColor(Color("title1"))
                            .font(Font.custom("Rubik-Regular", size: 14))
                        
                        
                        Text(profList.pgmInfo?.patientDetails.language ?? "")
                            .customTextfieldStyle2()
                    }
                        Group
                    {
                        Text("Preference 1")
                            .foregroundColor(Color("title1"))
                            .font(Font.custom("Rubik-Regular", size: 14))
                        
                        
                        Text(profList.pgmInfo?.patientDetails.preference1 ?? "")
                            .customTextfieldStyle2()
                    }
                    Group
                    {
                        Text("Preference 2")
                            .foregroundColor(Color("title1"))
                            .font(Font.custom("Rubik-Regular", size: 14))
                        
                        
                        Text(profList.pgmInfo?.patientDetails.preference2 ?? "")
                            .customTextfieldStyle2()
                        
                    }
                    Group
                    {
                            Text("Preference 3")
                                .foregroundColor(Color("title1"))
                                .font(Font.custom("Rubik-Regular", size: 14))
                            
                            
                            Text(profList.pgmInfo?.patientDetails.preference3 ?? "")
                            .customTextfieldStyle2()
                        }
                    
                        Group
                        {
                            Text("Quick Notes")
                            
                            
                                .foregroundColor(Color("title1"))
                                .font(Font.custom("Rubik-Regular", size: 14))
                            
                            
                            Text( profList.pgmInfo?.patientDetails.notes ?? "")
                                .customTextfieldStyle2()
                            
                        }
              
                }
            
            }
            .frame(maxWidth: .infinity, alignment: .leading)
            .padding(.horizontal, 16) // or whatever spacing you prefer
            .padding(.top, 10)
        }
    }
}

struct RPMPreferenceView_Previews: PreviewProvider {
    static var previews: some View {
        RPMPreferenceView()
    }
}
struct MyPreferenceTextFieldStyle: TextFieldStyle {
    func _body(configuration: TextField<Self._Label>) -> some View {
        configuration
            .padding(25)
            .background(
                RoundedRectangle(cornerRadius: 10, style: .continuous)
                    .stroke(Color("textFieldBG"), lineWidth: 2)
                    .background(Color("textFieldBG"))
                    .cornerRadius(10)
            )
            .padding(.vertical,8)
        
    }
}

