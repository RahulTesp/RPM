//
//  RPMMedicationsView.swift
//  RPM
//
//  Created by Tesplabs on 26/07/1944 Saka.
//


import SwiftUI

struct RPMMedicationsView: View {
    @State private var name: String = "Sam"
    @State private var middlename: String = "Joseph"
    @State private var lastname: String = "Alexander"
    @State private var dob: String = "Jan 30, 1954"
    @State private var gender: String = "Male"
    @State private var height: String = "6'2'' Feet"
    @State private var weight: String = "178 Pounds"
    @State private var mail: String = "samalexander@rpm.com"
    @State private var num1: String = "+1(234)222-561"
    @State private var num2: String = "+1(234)222-561"
    @ObservedObject  var medicDetList = RPMMedicationsViewModel()
    
    var body: some View {
        
        if  medicDetList.loading {
            
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
                
                    Text(" Medications")
                        .foregroundColor(.black)
                        .font(Font.custom("Rubik-SemiBold", size: 24))
                  
                    if (medicDetList.medicDetls == []
                     
                    )
                    {
                   
                        Text("NO DATA !").foregroundColor(.red)
                            .frame(
                                maxWidth: .infinity,
                                maxHeight: .infinity,
                                alignment: .center)
                    }
                    
                    ForEach(
                        medicDetList.medicDetls ?? []
                        
                        
                    ) {  item in
                      
                        VStack(alignment: .leading)
                        {
                            
                            Text(
                                
                                item.medicinename
                                
                            ) .padding(.top,5)
                                .foregroundColor(Color("title1"))
                                .font(Font.custom("Rubik-Regular", size: 18))
                            HStack
                            {
                                Text(
                                    
                                    item.medicineSchedule
                                ) .padding(.bottom,5)
                                    .foregroundColor(.black)
                                    .font(Font.custom("Rubik-Regular", size: 16))
                                Text(
                                    
                                   ", "
                                ) .padding(.bottom,5)
                                    .foregroundColor(.black)
                                    .font(Font.custom("Rubik-Regular", size: 16))
                                Text(
                                    
                                    item.beforeoraftermeal
                                ) .padding(.bottom,5)
                                    .foregroundColor(.black)
                                    .font(Font.custom("Rubik-Regular", size: 16))
                                
                            }
                            HStack
                            {
                                
                                Text(
                                    extractDatePartFromTimestamp(timestamp: item.startDate) ?? ""
                                    // item.startDate
                                ) .padding(.bottom,5)
                                    .foregroundColor(.black)
                                    .font(Font.custom("Rubik-Regular", size: 16))
                                Text("-")
                                Text(
                                    extractDatePartFromTimestamp(timestamp: item.endDate) ?? ""
                                    //  item.endDate
                                ) .padding(.bottom,5)
                                    .foregroundColor(.black)
                                    .font(Font.custom("Rubik-Regular", size: 16))
                            }
                            HStack(spacing: 17)
                            {
                                Text("M")
                                    .font(Font.custom("Rubik-Regular", size: 16))
                                    .padding(5)
                                    .font(.subheadline)
                                    .foregroundColor(.white)
                                    .frame( minWidth : 50)
                                    .fixedSize(horizontal: true, vertical: false)
                                
                                    .background(item.morning == true ? Color("GreenLight") : .gray)
                                
                                
                                    .cornerRadius(15)
                                
                                    .overlay(
                                        RoundedRectangle(cornerRadius: 16)
                                            .stroke(.black, lineWidth: 1))
                                
                                Text("AF")
                                    .font(Font.custom("Rubik-Regular", size: 16))
                                    .padding(5)
                                    .font(.subheadline)
                                    .foregroundColor(.white)
                                    .frame( minWidth : 50)
                                    .fixedSize(horizontal: true, vertical: false)
                                    .background(item.afterNoon == true ? Color("GreenLight") : .gray)
                                    .cornerRadius(15)
                                
                                    .overlay(
                                        RoundedRectangle(cornerRadius: 16)
                                            .stroke(.black, lineWidth: 1))
                                
                                Text("E")
                                    .font(Font.custom("Rubik-Regular", size: 16))
                                    .padding(5)
                                    .font(.subheadline)
                                    .foregroundColor(.white)
                                    .frame( minWidth : 50)
                                    .fixedSize(horizontal: true, vertical: false)
                                    .background(item.evening == true ? Color("GreenLight") : .gray)
                                    .cornerRadius(15)
                                
                                    .overlay(
                                        RoundedRectangle(cornerRadius: 16)
                                            .stroke(.black, lineWidth: 1))
                                
                                Text("N")
                                    .font(Font.custom("Rubik-Regular", size: 16))
                                    .padding(5)
                                    .font(.subheadline)
                                    .foregroundColor(.white)
                                    .frame( minWidth : 50)
                                    .fixedSize(horizontal: true, vertical: false)
                                    .background(item.night == true ? Color("GreenLight") : .gray)
                                    .cornerRadius(15)
                                
                                    .overlay(
                                        RoundedRectangle(cornerRadius: 16)
                                            .stroke(.black, lineWidth: 1))
                                
                            }   .padding(.bottom,8)
                            
                            
                        }.foregroundColor(.black)
                        
                            .frame(width: 320, height: 120, alignment: .leading)
                        
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
                .alert(isPresented: $medicDetList.showNoInternetAlert) {
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
    func extractDatePartFromTimestamp(timestamp: String) -> String? {
        let dateFormatter = DateFormatter()
        dateFormatter.dateFormat = "yyyy-MM-dd'T'HH:mm:ss"

        if let date = dateFormatter.date(from: timestamp) {
            dateFormatter.dateFormat = "dd MMM,yyyy"
            return dateFormatter.string(from: date)
        }

        return nil
    }
}

struct RPMMedicationsView_Previews: PreviewProvider {
    static var previews: some View {
        RPMMedicationsView()
    }
}




