//
//  RPMActivityView.swift
//  RPM
//
//  Created by Tesplabs on 26/07/1944 Saka.
//



import SwiftUI


struct RPMActivityView: View {
    
    @State var currentlySelectedId: Int = 1
    @State var showingInfo : Int = 1
    @State var currentlytextId: Int = 0
    @Environment(\.presentationMode) var presentationMode
    var body: some View {
        GeometryReader { geometry in
                let width = geometry.size.width
                let height = geometry.size.height
            
        ZStack{
            BackgroundView()
            
            VStack{
                
                // NOTE : TOP 1 TAB : ACTIVITY SCHEDULES
                HStack(spacing:20.0){
                    ActivityTabs(text: "Icons_Calendar", colorf: Color(.white), colorb: Color("title1"),
                                 id: 1, currentlySelectedId: $currentlySelectedId,
                                 showingInfo: $showingInfo
                                 
                    )
                  
                }
                
                .background(Color("bgColorDark"))
                .cornerRadius(13)
                
                .padding(.top,60)
                
                Divider()
               
                if(   showingInfo == 1 )
                
                {
                    // NOTE : ACTIVITY SCHEDULES VIEW
                    RPMSchedulesView()
                }
          
            }
            
          .frame(width: width, height: height)
            
            .padding(.vertical, 6)
            
            
            .navigationBarBackButtonHidden(true)
            
            .navigationBarTitleDisplayMode(.inline)
            .toolbar {
                ToolbarItem(placement: .principal) {
                    Text("Activity")
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

struct ActivityTabs :  View{
    
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

struct RPMActivityView_Previews: PreviewProvider {
    static var previews: some View {
        RPMActivityView()
        
    }
}
