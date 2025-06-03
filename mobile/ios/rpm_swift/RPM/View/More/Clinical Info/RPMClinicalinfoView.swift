//
//  RPMClinicalinfoView.swift
//  RPM
//
//  Created by Tesplabs on 18/07/1944 Saka.
//


import SwiftUI


struct RPMClinicalinfoView: View {

    @State private var action: Bool = false
    @State private var currentlySelectedId: Int = UserDefaults.standard.string(forKey: "pgmTypeString") == "RPM" ? 1 : 2
    @State private var showingInfo: Int = UserDefaults.standard.string(forKey: "pgmTypeString") == "RPM" ? 1 : 2
    @Binding var returningFromClinicalInfo: Bool
    @Environment(\.presentationMode) var presentationMode
    @EnvironmentObject var navigationHelper: NavigationHelper
    
    var body: some View {
        GeometryReader { geometry in
                let width = geometry.size.width
                let height = geometry.size.height
        ZStack{
            BackgroundView()
            
            VStack{
                // NOTE : TOP 3 TABS : VITAL READINGS / MEDICATION DETAILS / SYMPTOM DETAILS
                
                HStack(spacing:20.0){
                    if(UserDefaults.standard.string(forKey: "pgmTypeString") ?? "" == "RPM")
                    {
                        
                        ClinicalInfoTabs(text: "VitalOutline", colorf: Color(.white), colorb: Color("title1"),
                                         id: 1, currentlySelectedId: $currentlySelectedId,
                                         showingInfo: $showingInfo
                                         
                        )
                    }
                    
                    
                    ClinicalInfoTabs(text: "Icons_Medication", colorf: Color(.white), colorb:  Color("title1"),
                                     id: 2, currentlySelectedId: $currentlySelectedId,
                                     showingInfo: $showingInfo
                    )
                    
                    ClinicalInfoTabs(text: "Icons_Symptoms", colorf: Color(.white), colorb:  Color("title1"),
                                     id: 3, currentlySelectedId: $currentlySelectedId,
                                     showingInfo: $showingInfo
                    )
                    
                    
                }
                
                .background(Color("bgColorDark"))
                .cornerRadius(13)
                
                
                
                Divider()
                
                
                if(   ( UserDefaults.standard.string(forKey: "pgmTypeString")  == "RPM") && (showingInfo == 1) )
          
                {
                
                    // NOTE : VITAL READINGS VIEW PAGE
                    RPMVitalMonthReadingsView()
                   
                }
                else if
                    
                    ( showingInfo == 2 )
                     
                        
                {
                    // NOTE : MEDICATION VIEW PAGE
                    RPMMedicationsView()
                }
             
                else if(  showingInfo == 3 )
                {
                    // NOTE : SYMPTOMS VIEW PAGE
                    RPMSymptomsView()
                }
                
            }
           
            .frame(width: width)
            .padding(.vertical, 6)
            .navigationBarBackButtonHidden(true)
            .navigationBarTitleDisplayMode(.inline)
            .toolbar {
                ToolbarItem(placement: .principal) {
                    Text("Clinical Info")
                        .font(Font.custom("Rubik-Regular", size: 16))
                        .accessibilityAddTraits(.isHeader)
                        .foregroundColor( Color("TextColorBlack"))
                }
            }
            
            
            .navigationBarItems(leading:
                                    
                                    Button(action: {
                returningFromClinicalInfo = true
                self.presentationMode.wrappedValue.dismiss()
            }) {
                
                
                Image("ArrowBack") .renderingMode(.template)
                    .foregroundColor(  Color("buttonColor"))
                
            }
                                , trailing:
                            
                                Button(action: {
                navigationHelper.path.append(.medicationAdd)
                
            }) {
                Image("Icons_Add")
                    .renderingMode(.template)
                    .foregroundColor(Color("buttonColor"))
            }
            )
            .padding(.horizontal,16)
            
        }
    }
    }
}

struct ClinicalInfoTabs :  View{
    
    var text : String
    var colorf: Color
    var colorb: Color
    let id: Int
    @Binding var currentlySelectedId: Int
    
    @Binding var showingInfo:  Int
    var body : some View{
        
        
        Button(action :{
            
            self.currentlySelectedId = self.id
            
            
            if
                
                (self.id == 1) {
                
                showingInfo = 1
            
                
            }
            else if
                
                
                (self.id == 2) {
                
                
                showingInfo = 2
                
            }
            
            
            else if
                
                
                (self.id == 3) {
                
                
                showingInfo = 3
                
            }
            
            else if
                
                
                (self.id == 4) {
                
                
                showingInfo = 4
                
            }
            else if
                
                
                (self.id == 5) {
                
                
                showingInfo = 5
                
            }
            else
            {
                showingInfo = 6
                
            }
            
            
        })
        {
            
            
            
            Image(text)
                .renderingMode(.template)
                .frame( maxWidth: 40, maxHeight: 40)
            
            
                .cornerRadius(35)
                .foregroundColor(  id == currentlySelectedId ?  colorf : colorb)
            
            
                .background(   id == currentlySelectedId ?
                               Color("title1")
                               
                               : Color("TFbgColor"))
            
            
        }
        .cornerRadius(35)
    
    }
}
