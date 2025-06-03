//
//  RPMDeviceDetailsView.swift
//  RPM
//
//  Created by Tesplabs on 26/07/1944 Saka.
//



import SwiftUI

struct RPMDeviceDetailsView: View {
 
    @State var show : Int = 1
    @State private var userStartDate = Date()
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
                        
                        
                        Text(" Device Details")
                            .foregroundColor(.black)
                            .font(Font.custom("Rubik-SemiBold", size: 24))
                        
                    }
                    if(  pgmDetList.pgmInfo?.patientDevicesDetails.patientDeviceInfos == [] )
                        
                    {
                        
                        
                        Text("NO DATA !").foregroundColor(.red) .frame(
                            maxWidth: .infinity,
                            maxHeight: .infinity,
                            alignment: .center)
                    }
                
                    else{
                   
                        if let patientDeviceInfos = pgmDetList.pgmInfo?.patientDevicesDetails.patientDeviceInfos as? [PatientDeviceInfo] {
                            ForEach(Array(patientDeviceInfos.enumerated()), id: \.element.deviceNumber) { index, item in
                                
                                
                                Text("DEVICE \(index + 1)")
                                    .font(Font.custom("Rubik-Regular", size: 15))
                                    .foregroundColor(Color("buttonColor"))
                                    .padding(.bottom, 10)
                                    .padding(.top,6)
                                
                              
                                makeView(items: item)
                            }
                        
                        }
                        
                     
                    }
                    
                    
                }
                
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
            
            .frame(maxWidth: .infinity, alignment: .leading)
            .padding(.horizontal, 16) // or whatever spacing you prefer
            .padding(.top, 10)
            
        }
    }
    }
    
    func makeView(items : PatientDeviceInfo) -> some View
    {
        return Group
        {
        
     
        Text("Vital Monitoring")
            .foregroundColor(Color("title1"))
            .font(Font.custom("Rubik-Regular", size: 14))
        
        
        Text(
            
            items.vitalName
            
        )
        
        .foregroundColor(.black)
        .font(Font.custom("Rubik-Regular", size: 16))
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
                
                VStack(alignment:.leading)
                {
                    
                    Text("Device ID")
                        .foregroundColor(Color("title1"))
                        .font(Font.custom("Rubik-Regular", size: 14))
                    
                    
                    Text(
                        
                        items.deviceNumber
                        
                    )
                    
                    .foregroundColor(.black)
                    .font(Font.custom("Rubik-Regular", size: 16))
                    .frame(width: 150, height: 50, alignment: .leading)
                    
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
                
                
                VStack(alignment:.leading)
                {
                    
                    Text("Device Type")
                        .foregroundColor(Color("title1"))
                        .font(Font.custom("Rubik-Regular", size: 14))
                    
                    
                    Text(
                        
                        items.deviceCommunicationType
                        
                    )
                    .foregroundColor(.black)
                    .font(Font.custom("Rubik-Regular", size: 16))
                    .frame(width: 150, height: 50, alignment: .leading)
                    
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
            
        }
        
        Group
        {
            Text("Device")
                .foregroundColor(Color("title1"))
                .font(Font.custom("Rubik-Regular", size: 14))
            
            
            
            Text(
                items.deviceName
                
                
            )    .font(Font.custom("Rubik-Regular", size: 16))
            
          
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
        Group
            {
                
                
                Text("Device Status")
                    .foregroundColor(Color("title1"))
                    .font(Font.custom("Rubik-Regular", size: 14))
               
                Text(
                    items.deviceStatus
                    
                )    .font(Font.custom("Rubik-Regular", size: 16))
                
                
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
    }  .padding(.horizontal,10)
            .padding(.vertical,5)
    }
}

struct RPMDeviceDetailsView_Previews: PreviewProvider {
    static var previews: some View {
        RPMDeviceDetailsView()
    }
}


