//
//  RMPCommunicationView.swift
//  RPM
//
//  Created by Tesplabs on 13/04/1944 Saka.
//

import Foundation
import SwiftUI

struct RPMCommunicationView: View {
    
    @ObservedObject  var profList = RPMProgramInfoViewModel()
    
    var body: some View {
        
        
        if  profList.loading {
            
            Spacer()
            ProgressView()
                .tint(Color("TextColorBlack"))
               
            Text("Loading…") .foregroundColor( Color("TextColorBlack"))
            Spacer()
            
        } else
        
        
        {
            ScrollView
            {
              
                VStack(alignment: .leading)
                {
                  
                    Text("Communication Details")
                        .foregroundColor(.black)
                        .font(Font.custom("Rubik-SemiBold", size: 24))
                        .padding(.bottom,10)
                    
                    Group
                    {
                        
                        Text("Address Line 1")
                            .foregroundColor(Color("title1"))
                            .font(Font.custom("Rubik-Regular", size: 14))
                        
                        
                        Text(   profList.pgmInfo?.patientDetails.address1 ?? "")
                            .customTextfieldStyle2()
                    }
                    Group
                    {
                        
                        Text("Address Line 2")
                            .foregroundColor(Color("title1"))
                            .font(Font.custom("Rubik-Regular", size: 14))
                        
                        
                        Text(  profList.pgmInfo?.patientDetails.address2 ?? "")
                            .customTextfieldStyle2()
                    }
                    
                    Group
                    {
                        HStack
                        {
                            VStack(alignment:.leading)
                            {
                                
                                Text("Zip Code")
                                    .foregroundColor(Color("title1"))
                                    .font(Font.custom("Rubik-Regular", size: 14))
                                
                                
                                Text(  profList.pgmInfo?.patientDetails.zipCode ?? "")
                                    .customTextfieldStyle1()
                                
                                
                            }
                            VStack(alignment:.leading)
                            {
                                
                                Text("City")
                                    .foregroundColor(Color("title1"))
                                    .font(Font.custom("Rubik-Regular", size: 14))
                                
                                
                                Text(  profList.pgmInfo?.patientDetails.cityName ?? "")
                                    .customTextfieldStyle1()
                                
                            }
                            
                        }
                    }
                    Group
                    {
                        
                        Text("State")
                            .foregroundColor(Color("title1"))
                            .font(Font.custom("Rubik-Regular", size: 14))
                        
                        
                        Text(profList.pgmInfo?.patientDetails.state ?? "")
                            .customTextfieldStyle2()
                        
                        
                        Text("Time Zone")
                            .foregroundColor(Color("title1"))
                            .font(Font.custom("Rubik-Regular", size: 14))
                        
                        
                        Text(String(profList.pgmInfo?.patientDetails.timeZone ?? ""))
                            .customTextfieldStyle2()
                        
                    }
                    
                    Group
                    {
                        
                        Text("Emergency Contact 1")
                            .foregroundColor(Color("title1"))
                            .font(Font.custom("Rubik-Regular", size: 14))
                        
                        Text(profList.pgmInfo?.patientDetails.contact1Name ?? "")
                            .customTextfieldStyle2()
                        
                        Text("Relationship")
                            .foregroundColor(Color("title1"))
                            .font(Font.custom("Rubik-Regular", size: 14))
                        
                        
                        Text(profList.pgmInfo?.patientDetails.contact1RelationName ?? "")
                            .customTextfieldStyle2()
                        
                        Text("Mobile Number")
                            .foregroundColor(Color("title1"))
                            .font(Font.custom("Rubik-Regular", size: 14))
                        
                        
                        Text(profList.pgmInfo?.patientDetails.contact1Phone ?? "")
                            .customTextfieldStyle2()
                        
                    }
                    Group
                    {
                        
                        Text("Emergency Contact 2")
                            .foregroundColor(Color("title1"))
                            .font(Font.custom("Rubik-Regular", size: 14))
                        
                        
                        Text(profList.pgmInfo?.patientDetails.contact2Name ?? "")
                            .customTextfieldStyle2()
                        
                        Text("Relationship")
                            .foregroundColor(Color("title1"))
                            .font(Font.custom("Rubik-Regular", size: 14))
                        
                        
                        Text(profList.pgmInfo?.patientDetails.contact2RelationName ?? "")
                            .customTextfieldStyle2()
                        
                        Text("Mobile Number")
                            .foregroundColor(Color("title1"))
                            .font(Font.custom("Rubik-Regular", size: 14))
                        
                        
                        Text(profList.pgmInfo?.patientDetails.contact2Phone ?? "")
                     
                            .customTextfieldStyle2()
                    }
                    
                }
                .frame(maxWidth: .infinity, alignment: .leading)
                .padding(.horizontal, 16) // or whatever spacing you prefer
                .padding(.top, 10)

            }
        }
    }
}

struct RPMCommunicationView_Previews: PreviewProvider {
    static var previews: some View {
        RPMCommunicationView()
    }
}
