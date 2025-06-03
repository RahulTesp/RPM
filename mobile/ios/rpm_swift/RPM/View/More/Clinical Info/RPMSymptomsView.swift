//
//  RPMSymptomsView.swift
//  RPM
//
//  Created by Tesplabs on 26/07/1944 Saka.
//


import SwiftUI

struct RPMSymptomsView: View {
 
    @ObservedObject  var sympDetList = RPMSymptomsViewModel()
    
    var body: some View {
        
        if  sympDetList.loading {
            
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
                    
                    Text(" Symptoms")
                        .foregroundColor(.black)
                        .font(Font.custom("Rubik-SemiBold", size: 24))
                    
                    if (sympDetList.symDetls == []
                        
                        
                    )
                    {
            
                        Text("NO DATA !").foregroundColor(.red)
                            .frame(
                                maxWidth: .infinity,
                                maxHeight: .infinity,
                                alignment: .center)
                    }
                    
                    
                    ForEach(
                        sympDetList.symDetls ?? []
                        
                        
                    ) {  item in
                        
                        VStack(alignment: .leading)
                        {
                            Text(
                                
                                item.symptom
                                
                            ) .padding(.top,5)
                                .foregroundColor(Color("title1"))
                                .font(Font.custom("Rubik-Regular", size: 18))
                            Text(
                                
                                
                                self.dateSympInfo(  timeval:item.symptomStartDateTime )
                                
                                
                            ) .padding(.bottom,5)
                                .font(Font.custom("Rubik-Regular", size: 16))
                            
                            Text(
                                
                                item.rpmSymptomDescription
                                
                            ) .padding(.bottom,5)
                                .foregroundColor(Color("title1"))
                                .font(Font.custom("Rubik-Regular", size: 14))
                            
                        }
                        .foregroundColor(.black)
                        .frame(width: 320, height: 100, alignment: .leading)
                        
                        .padding(.horizontal,15)
                        .padding(.vertical,5)
                        .background(
                            RoundedRectangle(cornerRadius: 8, style: .continuous
                                            )
                            .stroke(Color("textFieldBG"), lineWidth: 2
                                   )
                            .background(.white)
                            .cornerRadius(8)
                        )
                  
                    }
                 
                }
                .padding()
                .padding(.horizontal,10)
                
                .alert(isPresented: $sympDetList.showNoInternetAlert) {
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
        }
        
    }
    
    // NOTE : SYMPTOM DATE FORMATTER
    func dateSympInfo(timeval: String) -> String
    {
        
        let dateFormatter = DateFormatter()
        dateFormatter.dateFormat = "yyyy-MM-dd'T'HH:mm:ss"
        dateFormatter.timeZone = NSTimeZone(name: "UTC") as TimeZone?
        let date = dateFormatter.date(from: timeval)
        
        // change to a readable time format and change to local time zone
        dateFormatter.dateFormat = "MMM d, yyyy, HH:mm a"
        dateFormatter.timeZone = NSTimeZone.local
        let timeStamp = dateFormatter.string(from: date!)
        return timeStamp
    }
    
}

struct RPMSymptomsView_Previews: PreviewProvider {
    static var previews: some View {
        RPMSymptomsView()
    }
}
