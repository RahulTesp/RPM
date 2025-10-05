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
        GeometryReader { geometry in
            
            
            if  medicDetList.loading {
                
                VStack(spacing: 20) {
                    Spacer()
                    ProgressView()
                        .tint(Color("TextColorBlack"))
                    Text("Loading…")
                        .foregroundColor(Color("TextColorBlack"))
                    Spacer()
                }
                .frame(maxWidth: .infinity, maxHeight: .infinity)
              
                
            }
            
            else
            
            {
                
                ScrollView
                {
                    VStack(alignment: .leading, spacing: 20)
                    {
                        
                        Text("Medications")
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
                                                                    
                                                                ) .padding(.bottom,5)
                                                                    .foregroundColor(.black)
                                                                    .font(Font.custom("Rubik-Regular", size: 16))
                                                                Text("-")
                                                                Text(
                                                                    extractDatePartFromTimestamp(timestamp: item.endDate) ?? ""
                                                                    
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
                                                                
                                                                    .background(item.night == true ? Color("GreenLight") : .gray)
                                                                    .cornerRadius(15)
                                                                
                                                                    .overlay(
                                                                        RoundedRectangle(cornerRadius: 16)
                                                                            .stroke(.black, lineWidth: 1))
                                                                
                                                            }   .padding(.bottom,8)
                                                            
                                                            
                                                        }.foregroundColor(.black)
                            
                            
                                .padding(.horizontal, 16)
                            
                            
                                .background(
                                    RoundedRectangle(cornerRadius: 8, style: .continuous)
                                        .fill(isExpired(endDateString: item.endDate) ? Color.gray.opacity(0.3) : Color.white)
                                        .overlay(
                                            RoundedRectangle(cornerRadius: 8)
                                                .stroke(Color("textFieldBG"), lineWidth: 2)
                                        )
                                )
                            
                            
                        }
                        
                    }
                    
                    .padding(.horizontal, 16)
                    .padding(.top, 10)
                    .frame(width: geometry.size.width - 32)
                
                }
                
                .scrollIndicators(.hidden)
              
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
    
    
    func isExpired(endDateString: String) -> Bool {
            let formatter = DateFormatter()
            // Use the format that matches your timestamp string (usually best without explicit timeZone)
            formatter.dateFormat = "yyyy-MM-dd'T'HH:mm:ss"
            
            guard let endDateWithTime = formatter.date(from: endDateString) else {
                return false // Cannot parse, so assume not expired (i.e., white)
            }
            
            let calendar = Calendar.current
            let currentDate = Date()
            
            // Get the start of the day for both dates.
            let startOfEndDate = calendar.startOfDay(for: endDateWithTime)
            let startOfCurrentDate = calendar.startOfDay(for: currentDate)
            
            // Returns true only if the current date is strictly AFTER the end date.
            return startOfCurrentDate > startOfEndDate
        }
    

}

struct RPMMedicationsView_Previews: PreviewProvider {
    static var previews: some View {
        RPMMedicationsView()
    }
}
