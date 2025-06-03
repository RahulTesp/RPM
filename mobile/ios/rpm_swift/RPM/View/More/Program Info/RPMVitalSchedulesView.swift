//
//  RPMVitalSchedulesView.swift
//  RPM
//
//  Created by Tesplabs on 26/07/1944 Saka.
//


import SwiftUI

struct RPMVitalSchedulesView: View {
 
    @ObservedObject  var pgmDetList = RPMProgramInfoViewModel()
    
    var body: some View {
        GeometryReader { geometry in
                let width = geometry.size.width
                let height = geometry.size.height
        if  pgmDetList.loading {
            Group
            {
                Spacer()
                ProgressView()
                    .tint(Color("TextColorBlack"))
                
                Text("Loading…") .foregroundColor( Color("TextColorBlack"))
                    .padding(.vertical,15)
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
                        Text(" Vital Schedules")
                            .foregroundColor(.black)
                            .font(Font.custom("Rubik-SemiBold", size: 24))
                 
                    }
                
                    if let patientSchedulesInfos = pgmDetList.pgmInfo?.patientVitalDetails.patientVitalInfos as? [PatientVitalDetailsPatientVitalInfo] {
                        ForEach(Array(patientSchedulesInfos.enumerated()), id: \.element.scheduleID) { index, item in
                            
                            Text("Schedule \(index + 1)")
                                .font(Font.custom("Rubik-Regular", size: 15))
                                .foregroundColor(Color("buttonColor"))
                                .padding(.bottom, 10)
                                .padding(.top,6)
                            
                            makeScheduleView(items: item)
                       
                        }
                     
                    }
                    
                }
                
                
                .frame(maxWidth: .infinity, alignment: .leading)
                .padding(.horizontal, 16) // or whatever spacing you prefer
                .padding(.top, 10)
    
                    .alert(isPresented: $pgmDetList.showNoInternetAlert) {
                        Alert(
                            title: Text("No Internet Connection"),
                            message: Text("Please turn on Wi-Fi or Mobile Data."),
                            primaryButton: .default(Text("Open Settings")) {
                                if let url = URL(string: UIApplication.openSettingsURLString),
                                   UIApplication.shared.canOpenURL(url) {
                                    UIApplication.shared.open(url)
                                }
                            },
                            secondaryButton: .cancel()
                        )
                    }
                
            }
            .padding(.horizontal,10)
            
        }
        
    }
        }
        
    }
    
    func makeScheduleView(items : PatientVitalDetailsPatientVitalInfo) -> some View
    {
        return Group
        {
    
    Text("Vital Monitoring")
        .foregroundColor(Color("title1"))
        .font(Font.custom("Rubik-Regular", size: 14))
    
    
    Text(
        
        items.vitalName
        
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
    
   
    Text("Schedule")
        .foregroundColor(Color("title1"))
        .font(Font.custom("Rubik-Regular", size: 14))
    
    
    Text(
        
        items.scheduleName
        
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
    
    Text(
        
        items.vitalScheduleName
        
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
    
    Group
    {
        HStack
        {
            
            
            Text("Intervals")
                .foregroundColor(Color("title1"))
                .font(Font.custom("Rubik-Regular", size: 14))
            
        }
    }
    Group
    {
        HStack
        {
            Text(
                
                "Morning"
                
            )
            .font(Font.custom("Rubik-Regular", size: 16))
            .foregroundColor(   items.morning == true ?   .black : .gray)
            .frame(height: 40, alignment: .leading)
            
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
            
            
            Text(
                
                "Afternoon"
                
            )
            .font(Font.custom("Rubik-Regular", size: 16))
            .foregroundColor(   items.afternoon == true ?   .black : .gray)
            .frame( height: 40, alignment: .leading)
            
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
            
            Text(
                
                "Evening"
                
            )
            .font(Font.custom("Rubik-Regular", size: 16))
            .foregroundColor(   items.evening == true ?   .black : .gray)
            .frame(height: 40, alignment: .leading)
            
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
            
            Text(
                
                "Night"
                
            )
            .font(Font.custom("Rubik-Regular", size: 16))
            .foregroundColor(   items.night == true ?   .black : .gray)
            .frame( height: 40, alignment: .leading)
            
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
    Text("Normal Range")
        .foregroundColor(Color("title1"))
        .font(Font.custom("Rubik-Regular", size: 14))
    
    
    ForEach(
        items.vitalMeasureInfos ?? []
       
    ) {  itemss in
        
        Group
        {
            
            HStack
            {
                
                
                Text(
                    "\(itemss.measureName) ( \(itemss.unitName) )"
                    
                    
                )      .font(Font.custom("Rubik-Regular", size: 16))
                    .foregroundColor(.black)
                Spacer()
                
                Text(
                    String(
                        "\(itemss.normalMinimum ) - \( itemss.normalMaximum)")
                    
                )     .font(Font.custom("Rubik-Regular", size: 16))
                    .foregroundColor(Color("GreenLight"))
            }
            
            
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
    }.padding(.horizontal,10)
            .padding(.vertical,5)
}

}

struct RPMVitalSchedulesView_Previews: PreviewProvider {
    static var previews: some View {
        RPMVitalSchedulesView()
    }
}



