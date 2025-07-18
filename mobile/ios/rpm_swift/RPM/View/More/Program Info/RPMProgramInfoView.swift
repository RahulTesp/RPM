//
//  RPMProgramInfoView.swift
//  RPM
//
//  Created by Tesplabs on 25/07/1944 Saka.
//

import SwiftUI


struct RPMProgramInfoView: View {
    
    @State var currentlySelectedId: Int = 1
    @State var showingInfo : Int = 1
    @State var currentlytextId: Int = 0
    @Environment(\.presentationMode) var presentationMode
    @EnvironmentObject var accountListVM: RPMHomeViewModel
    @EnvironmentObject var navigationHelper: NavigationHelper
    
    var body: some View {
        GeometryReader { geometry in
                let width = geometry.size.width
                let height = geometry.size.height
        ZStack{
            BackgroundView()
            
            VStack{
                
                // NOTE : TOP 5 TABS : PROGRAM DETAILS / DEVICE DETAILS / VITAL SCHEDULES / INSURANCE DETAILS / PROGRAM TIMELINE
                
                HStack(spacing:20.0){
                    PgmInfoTabs(text: "Icons_Program", colorf: Color(.white), colorb: Color("title1"),
                                id: 1, currentlySelectedId: $currentlySelectedId,
                                showingInfo: $showingInfo
                                
                    )
                    
                    
                    if(UserDefaults.standard.string(forKey: "pgmTypeString") == "RPM" )
                    {
                        PgmInfoTabs(text: "Icons_Devices", colorf: Color(.white), colorb:  Color("title1"),
                                    id: 2, currentlySelectedId: $currentlySelectedId,
                                    showingInfo: $showingInfo
                        )
                        
                    }
                    
                    if(UserDefaults.standard.string(forKey: "pgmTypeString") ?? "" == "RPM")
                    {
                        PgmInfoTabs(text: "VitalOutline", colorf: Color(.white), colorb:  Color("title1"),
                                    id: 3, currentlySelectedId: $currentlySelectedId,
                                    showingInfo: $showingInfo
                        )
                    }
                    PgmInfoTabs(text: "Icons_Insurance", colorf: Color(.white), colorb:  Color("title1"),
                                id: 4, currentlySelectedId: $currentlySelectedId,
                                showingInfo: $showingInfo
                    )
              
                    PgmInfoTabs(text: "Icons_Timeline", colorf: Color(.white), colorb:  Color("title1"),
                                id: 6, currentlySelectedId: $currentlySelectedId,
                                showingInfo: $showingInfo
                    )
                    
                }
                
                .background(Color("bgColorDark"))
                .cornerRadius(13)
              
                Divider()
               
                if(   showingInfo == 1 )
                    
             
                {
                    //NOTE : PROGRAM DETAILS VIEW PAGE
                    RPMProgramDetailsView()
                        .environmentObject(accountListVM)  
                }
                
                
                else if
                    
                    ( showingInfo == 2 )
                     
                {
                    //NOTE : DEVICE DETAILS VIEW PAGE
                    RPMDeviceDetailsView()
                }
                
                
                else if(showingInfo == 3)
                {
                    //NOTE : VITAL SCHEDULES VIEW PAGE
                    
                    
                    RPMVitalSchedulesView()
                
                }
                
                else if(showingInfo == 4)
                {
                    //NOTE : INSURANCE DETAILS VIEW PAGE
                    RPMInsuranceDetailsView()
                }
        
                
                else
                {
                    //NOTE : PROGRAM TIMELINE VIEW PAGE
                    RPMProgramTimelineView()
                }
                
            }
            
            
            .frame(width: width)
            
            .padding(.vertical, 6)
            
            
            .navigationBarBackButtonHidden(true)
            
            .navigationBarTitleDisplayMode(.inline)
            .toolbar {
                ToolbarItem(placement: .principal) {
                    Text("Program Info")
                        .font(Font.custom("Rubik-Regular", size: 16))
                        .accessibilityAddTraits(.isHeader)
                        .foregroundColor( Color("TextColorBlack"))
                }
            }
            
            
            .navigationBarItems(leading:
                                    HStack
                                {
                Button(action: {
                    self.presentationMode.wrappedValue.dismiss()
                }) {
                    
                    
                    Image("ArrowBack") .renderingMode(.template)
                        .foregroundColor(  Color("buttonColor"))
                    
                }
                
            }
                                
            )
            
            .padding(.horizontal,16)
           
            .onAppear()
            {
            
                    print(" pgminfoAPEARPath:", navigationHelper.path)
                
            }
        }
    }
    }
}

struct PgmInfoTabs :  View{
    
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

struct RPMProgramInfoView_Previews: PreviewProvider {
    static var previews: some View {
        RPMProgramInfoView()
        
        
    }
    
}
