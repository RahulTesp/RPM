//
//  RPMInsuranceDetailsView.swift
//  RPM
//
//  Created by Tesplabs on 26/07/1944 Saka.
//


import SwiftUI

struct RPMInsuranceDetailsView: View {
 
    @ObservedObject  var pgmDetList = RPMProgramInfoViewModel()
    
    var body: some View {
        GeometryReader { geometry in
        if  pgmDetList.loading {
            
            Spacer()
            ProgressView()
                .tint(Color("TextColorBlack"))
            
            Text("Loading…") .foregroundColor( Color("TextColorBlack"))
                .padding(.vertical,15)
            Spacer()
            
        } else
        
        {
            
            ScrollView
            {
                VStack(alignment: .leading)
                {
                    Group
                    {
                        Text(" Insurance Details")
                            .foregroundColor(.black)
                            .font(Font.custom("Rubik-SemiBold", size: 24))
                    }
               
                    if(  pgmDetList.pgmInfo?.patientInsurenceDetails.patientInsurenceInfos == [] )
                        
                    {
                     
                        Text("NO DATA !").foregroundColor(.red) .frame(
                            maxWidth: .infinity,
                            maxHeight: .infinity,
                            alignment: .center)
                    }
               
                    else{
                 
                        ForEach(
                            pgmDetList.pgmInfo?.patientInsurenceDetails.patientInsurenceInfos ?? []
                            
                            
                        ) {  item in
                            
                            
                            Text(
                                "Primary Insurance")
                            .foregroundColor(Color("title1"))
                            .font(Font.custom("Rubik-Regular", size: 14))
                            
                            
                            if(
                                item.isPrimary == true
                                
                                
                            ) {
                                
                                Group{
                                    
                                    Text(
                                 
                                        item.insuranceVendorName
                                        
                                    )
                                    .font(Font.custom("Rubik-Regular", size: 16))
                                    .foregroundColor(.black)
                                    .frame(width: 320, height: 40, alignment: .leading)
                                    
                                    .padding(.horizontal,5)
                                    .padding(.vertical,5)
                                    .background(
                                        RoundedRectangle(cornerRadius: 8, style: .continuous
                                                        )
                                        .stroke(Color("textFieldBG"), lineWidth: 2
                                               )
                                        .background(Color("textFieldBG"))
                                        .cornerRadius(8)
                                        
                                    )
                             
                                }
                            }
                        
                            if(
                                item.isPrimary == false)
                            {
                                
                                Group
                                {
                                    
                                    Text(
                                        "Secondary Insurance (Optional)")
                                    .foregroundColor(Color("title1"))
                                    .font(Font.custom("Rubik-Regular", size: 14))
                                    
                                    
                                    ForEach(
                                        pgmDetList.pgmInfo?.patientInsurenceDetails.patientInsurenceInfos ?? []
                                        
                                        
                                    ) {  items in
                                        
                                        
                                        
                                        if(
                                            items.isPrimary == false
                                            
                                            
                                        ) {
                                       
                                            Text(
                                                
                                                items.insuranceVendorName
                                                
                                            )
                                            .font(Font.custom("Rubik-Regular", size: 16))
                                            .foregroundColor(.black)
                                            .frame(width: 320, height: 40, alignment: .leading)
                                            
                                            .padding(.horizontal,5)
                                            .padding(.vertical,5)
                                            .background(
                                                RoundedRectangle(cornerRadius: 8, style: .continuous
                                                                )
                                                .stroke(Color("textFieldBG"), lineWidth: 2
                                                       )
                                                .background(Color("textFieldBG"))
                                                .cornerRadius(8)
                                                
                                            )
                                            
                                        }
                                        
                                    }
                                    
                                    
                                }.padding(.horizontal,10)
                                    .padding(.vertical,5)
                                
                                
                            }
                        }.padding(.horizontal,10)
                            .padding(.vertical,5)
                    }
                  
                }
                
                .frame(maxWidth: .infinity, alignment: .leading)
                .padding(.horizontal, 16) // or whatever spacing you prefer
                .padding(.top, 10)
 
            }
          
        }
    }
    }
    
}

struct RPMInsuranceDetailsView_Previews: PreviewProvider {
    static var previews: some View {
        RPMInsuranceDetailsView()
    }
}




