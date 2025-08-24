//
//  RMPPersonalInfoView.swift
//  RPM
//
//  Created by Tesplabs on 13/04/1944 Saka.
//

import Foundation
import SwiftUI

struct RPMPersonalInfoView: View {

    @State private var actionPwd: Bool = false
    @ObservedObject  var profList = RPMProgramInfoViewModel()
    @EnvironmentObject var navigationHelper: NavigationHelper

    var body: some View {
        GeometryReader { geometry in
        
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
                    Group
                    {
                        Text("Personal Information")
                            .foregroundColor(.black)
                            .font(Font.custom("Rubik-SemiBold", size: 24))
                        
                        Image(systemName:"person")
                            .resizable()
                            .renderingMode(.template)
                            .foregroundColor(  Color("TextColorBlack"))
                            .frame(width: 120.0, height: 90.0)
                        
                            .padding(.bottom,20)
                    }
                    Group
                    {
                        
                        Text("First Name")
                            .foregroundColor(Color("title1"))
                            .font(Font.custom("Rubik-Regular", size: 14))
                        
                        
                        Text(
                            profList.pgmInfo?.patientDetails.firstName ?? ""
                      
                        )
                        
                        .customTextfieldStyle2()
                        
                    }
                    Group
                    {
                        
                        HStack
                        {
                            VStack(alignment:.leading)
                            {
                                
                                Text("Middle Name")
                                    .foregroundColor(Color("title1"))
                                    .font(Font.custom("Rubik-Regular", size: 14))
                                
                                
                                Text(
                                    profList.pgmInfo?.patientDetails.middleName ?? ""
                                    
                                    
                                )
                                .customTextfieldStyle1()
                                
                            }
                            VStack(alignment:.leading)
                            {
                                
                                Text("Last Name")
                                    .foregroundColor(Color("title1"))
                                    .font(Font.custom("Rubik-Regular", size: 14))
                                
                                
                                Text(
                                    profList.pgmInfo?.patientDetails.lastName ?? ""
                                    
                                )
                                .customTextfieldStyle1()
                            }
                            
                        }
                        
                    }
                    Group
                    {
                        Text("Date of Birth")
                            .foregroundColor(Color("title1"))
                            .font(Font.custom("Rubik-Regular", size: 14))
                        
                        
                        Text(String(convertdobformat(inputDate: profList.pgmInfo?.patientDetails.dob ?? "")))
                            .customTextfieldStyle2()
                
                        
                        Text("Gender")
                            .foregroundColor(Color("title1"))
                            .font(Font.custom("Rubik-Regular", size: 14))
                        
                        
                        
                        profList.pgmInfo?.patientDetails.gender ?? "" == "Female" ?
                        Text(
                            
                            "Female"
                            
                        )  .customTextfieldStyle2()
                        
                        :
                        Text(
                            
                            "Male"
                            
                        )
                        .customTextfieldStyle2()
                        
                        HStack
                        {
                            VStack(alignment:.leading)
                            {
                                
                                Text("Height")
                                    .foregroundColor(Color("title1"))
                                    .font(Font.custom("Rubik-Regular", size: 14))
                                
                                
                                Text(
                                    
                                    String(
                                        
                                        profList.pgmInfo?.patientDetails.height ?? 0
                                        
                                    ) +
                                    " Feet"
                                    
                                )
                                .customTextfieldStyle1()
                                
                            }
                            VStack(alignment:.leading)
                            {
                                
                                Text("Weight")
                                    .foregroundColor(Color("title1"))
                                    .font(Font.custom("Rubik-Regular", size: 14))
                                
                                
                                Text( String(
                                    profList.pgmInfo?.patientDetails.weight ?? 0
                                    
                                )  + " Pounds")
                                .customTextfieldStyle1()
                            }
                            
                        }
                        
                    }
                    
                    Group
                    {
                        Text("Email ID")
                            .foregroundColor(Color("title1"))
                            .font(Font.custom("Rubik-Regular", size: 14))
                        
                        
                        Text(
                            profList.pgmInfo?.patientDetails.email ?? ""
            
                        )
                        .customTextfieldStyle2()
                    }
                    Group
                    {
                        Text("Mobile Number(Primary)")
                            .foregroundColor(Color("title1"))
                            .font(Font.custom("Rubik-Regular", size: 14))
                        
                        Text(
                            profList.pgmInfo?.patientDetails.mobileNo ?? ""
                            
                        )
                        .customTextfieldStyle2()
                    }
                    Group
                    {
                        Text("Alternate Number(optional)")
                            .foregroundColor(Color("title1"))
                            .font(Font.custom("Rubik-Regular", size: 14))
                        
                        
                        Text(
                            profList.pgmInfo?.patientDetails.alternateMobNo ?? ""
                            
                        )
                        .customTextfieldStyle2()
                        
                        
                        
                    }
             
                    
                    Button(action: {
                        navigationHelper.path.append(.changePassword)
                        
                    }) {
                        Text("Change Password")
                            .frame(width: 330, height: 50)
                            .background(Color("lightGreen"))
                            .foregroundColor(Color("title1"))
                            .cornerRadius(15)
                            .font(Font.custom("Rubik-Regular", size: 16))
                    }
                 
                }
         
                .background(Color("bgColorDark"))
                .frame(maxWidth: .infinity, alignment: .leading)
                .padding(.horizontal, 16) // or whatever spacing you prefer
                .padding(.top, 10)
                .navigationBarBackButtonHidden(true)
                
            }    .navigationBarBackButtonHidden(true)
            
        }
    }
    }
}


struct MyProfileTextFieldStyle: TextFieldStyle {
    func _body(configuration: TextField<Self._Label>) -> some View {
        configuration
            .padding(12)
            .background(
                RoundedRectangle(cornerRadius: 10, style: .continuous)
                    .stroke(Color("textFieldBG"), lineWidth: 2)
                    .background(Color("textFieldBG"))
                    .cornerRadius(10)
            )
            .padding(.vertical,5)
        
    }
}


