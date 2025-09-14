//
//  RPMPlanView.swift
//  RPM
//
//  Created by Tesplabs on 25/07/1944 Saka.
//

import SwiftUI

struct RPMPlanView: View {
    
    @EnvironmentObject var accountListVM: RPMHomeViewModel
    var body: some View {
        
        VStack (alignment:.leading){
            
            HStack{
                
                Text(accountListVM.accounts?.programName ?? "" )
                    .font(Font.custom("Rubik-Regular", size: 16))
                    .foregroundColor(.black)
                
                Spacer()
             
                HStack
                {
                    Text(accountListVM.accounts?.status ?? "" )
                        .padding(5)
                        .font(Font.custom("Rubik-Regular", size: 14))
                        .foregroundColor(ColourStatus.getForegroundColor(for: accountListVM.accounts?.status))
                        .frame( minWidth :  70)
                        .fixedSize(horizontal: true, vertical: false)
                        .background(ColourStatus.getBackgroundColor(for: accountListVM.accounts?.status))
                        .cornerRadius(15)
                }.clipShape(RoundedRectangle(cornerRadius:20))
                
                
            }
        }
        .navigationBarBackButtonHidden(true)
        .padding(.horizontal,10)
        .padding(.vertical,5)
        .frame(maxWidth: .infinity, minHeight: 70)
        .background(Color("avgGreen"))
        .cornerRadius(15)
    }
}


struct ProgressBar: View {
    var value: Float
    var progressColor : Color
    var indicatorColor : Color
    var body: some View {
        GeometryReader { geometry in
            ZStack(alignment: .leading) {
                Rectangle().frame(width: geometry.size.width , height: geometry.size.height)
                    .opacity(0.3)
                    .background(progressColor)
                
                Rectangle().frame(width: min(CGFloat(self.value)*geometry.size.width, geometry.size.width), height: geometry.size.height)
                    .foregroundColor(indicatorColor)
                    .animation(.linear, value: value)
                   
            }.cornerRadius(45.0)
        }
    }
}

struct RPMPlanView_Previews: PreviewProvider {
    static var previews: some View {
        RPMPlanView()
    }
}
