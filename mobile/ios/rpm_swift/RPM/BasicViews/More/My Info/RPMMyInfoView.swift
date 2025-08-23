//
//  RMPMyInfoView.swift
//  RPM
//
//  Created by Tesplabs on 11/04/1944 Saka.
//

import SwiftUI


struct RPMMyInfoView: View {
  
    @State var currentlySelectedId: Int = 1
    @State var showingInfo : Int = 1
    @State var currentlytextId: Int = 0
    @Environment(\.presentationMode) var presentationMode
    @EnvironmentObject var navigationHelper: NavigationHelper
    
    var body: some View {
        GeometryReader { geometry in
                let width = geometry.size.width
                let height = geometry.size.height
      
        ZStack{
            BackgroundView()
            
            VStack{
                
                // TOP 3 TABS : PERSONAL / COMMUNICATION / PREFERENCE
                HStack(spacing:1.0){
                    MyInfoTabs(text: "Personal", colorf: Color(.white), colorb: Color("title1"),
                               id: 1, currentlySelectedId: $currentlySelectedId,
                               showingInfo: $showingInfo
                               
                    )
                    MyInfoTabs(text: "Communication", colorf: Color(.white), colorb:  Color("title1"),
                               id: 2, currentlySelectedId: $currentlySelectedId,
                               showingInfo: $showingInfo
                    )
                    
                    MyInfoTabs(text: "Preference", colorf: Color(.white), colorb:  Color("title1"),
                               id: 3, currentlySelectedId: $currentlySelectedId,
                               showingInfo: $showingInfo
                    )
                    
                }
                
                .background(Color("bgColorDark"))
                .cornerRadius(13)
                
                Divider()
                
                if(   showingInfo == 1 )
               
                {
                    //PERSONAL INFO VIEW PAGE
                    RPMPersonalInfoView()
                        .environmentObject(navigationHelper)
                }
             
                else if
                    
                    ( showingInfo == 2 )
                   
                {
                    //COMMUNICATION VIEW PAGE
                    RPMCommunicationView()
                }
              
                else
                {
                    //PREFERENCE VIEW PAGE
                    RPMPreferenceView()
                }
             
            }    .background(Color("bgColorDark"))
            
                .frame(width: width, height: height)
            
                .padding(.vertical, 6)
            
                .navigationBarBackButtonHidden(true)
            
                .navigationBarTitleDisplayMode(.inline)
                .toolbar {
                    ToolbarItem(placement: .principal) {
                        Text("My Info")
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
            
        }
        
     }
    }
    
}

struct MyInfoTabs :  View{
    
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
            
            else
            {
                showingInfo = 3
                
            }
            
            
        })
        {
            
            Text(text)
                .font(Font.custom("Rubik-Regular", size: 14))
                .foregroundColor(  id == currentlySelectedId ?  colorf : colorb)
                .padding(12)
                .frame( minWidth: 38, minHeight: 28)
            
            
                .background(   id == currentlySelectedId ?
                               Color("title1")
                               
                               : Color("bgColor"))
            
                .foregroundColor(id == currentlySelectedId ? .green : .red)
        }
        .cornerRadius(15)
     
    }
}
