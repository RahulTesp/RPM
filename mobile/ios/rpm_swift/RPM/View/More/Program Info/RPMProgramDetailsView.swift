//
//  RPMProgramDetailsView.swift
//  RPM
//
//  Created by Tesplabs on 25/07/1944 Saka.
//

import Foundation
import SwiftUI

struct RPMProgramDetailsView: View {

    @ObservedObject  var pgmDetList = RPMProgramInfoViewModel()
    @EnvironmentObject var accountListVM: RPMHomeViewModel
    
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
                        
                        Text(" Program Details")
                            .foregroundColor(.black)
                            .font(Font.custom("Rubik-SemiBold", size: 24))
              
                            RPMPlanView()
                                .environmentObject(accountListVM)  
                                .padding(.horizontal,10)
                                .padding(.vertical,6)
                 
                    }
                  
                    Group
                    {
                        
                        Text("Duration")
                            .foregroundColor(Color("title1"))
                            .font(Font.custom("Rubik-Regular", size: 14))
                        
                        Text(
                            String(
                                
                                pgmDetList.pgmInfo?.patientProgramdetails.duration ?? 0
                                
                            ) + " Months"
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
             
                        HStack
                        {
                            VStack(alignment:.leading)
                            {
                                
                                Text("Start Date")
                                    .foregroundColor(Color("title1"))
                                    .font(Font.custom("Rubik-Regular", size: 14))
                                
                                
                                Text(
                           
                                    self.datePgmInfo(  timeval:    pgmDetList.pgmInfo?.patientProgramdetails.startDate ?? "")
                             
                                )
                                .font(Font.custom("Rubik-Regular", size: 16))
                                .foregroundColor(.black)
                                .frame(width: 150, height: 40, alignment: .leading)
                                
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
                                
                                Text("End Date")
                                    .foregroundColor(Color("title1"))
                                    .font(Font.custom("Rubik-Regular", size: 14))
                                
                                
                                Text(
                                    
                                    convertdobformat(inputDate: pgmDetList.pgmInfo?.patientProgramdetails.endDate ?? "")
                      
                                    
                                )  .font(Font.custom("Rubik-Regular", size: 16))
                                    .foregroundColor(.black)
                                    .frame(width: 150, height: 40, alignment: .leading)
                                
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
                  
                        Text(   UserDefaults.standard.string(forKey: "pgmTypeString") ?? "" == "RPM" ? "Vitals Monitored" : "Conditions Monitored")
                            .foregroundColor(Color("title1"))
                            .font(Font.system(size: 15))
                 
                        ForEach(
                            pgmDetList.pgmInfo?.patientProgramdetails.patientVitalInfos.filter { $0.selected } ?? []
                        ) { item in
                            if item.selected {
                                VStack(alignment: .leading) {
                                    Text(item.vital)
                                        .font(Font.custom("Rubik-Regular", size: 16))
                                    
                                    // Add other views within the VStack as needed
                                    
                                }
                                .foregroundColor(.black)
                                .frame(width: 320, height: 40, alignment: .leading)
                                .padding(.horizontal, 5)
                                .padding(.vertical, 5)
                                .background(
                                    RoundedRectangle(cornerRadius: 8, style: .continuous)
                                        .stroke(Color("textFieldBG"), lineWidth: 2)
                                        .background(Color("textFieldBG"))
                                        .cornerRadius(8)
                                )
                            }
                        }
                 
                    }
                 
                        .padding(.vertical,5)
                    Group
                    {
                        
                        Text("Care Team").foregroundColor(Color("buttonColor"))
                            .font(Font.custom("Rubik-SemiBold", size: 15))
                        Text("Physician")
                            .foregroundColor(
                                Color("title1"))
                            .font(Font.custom("Rubik-Regular", size: 14))
                        
                        
                        Text(
                            pgmDetList.pgmInfo?.patientPrescribtionDetails.physician ?? ""
                            
                            
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
                    
                    }
                  
                        .padding(.vertical,5)
                    Group
                    {
                        Text("Care Team Personal")
                            .foregroundColor(Color("title1"))
                            .font(Font.custom("Rubik-Regular", size: 14))
                        
                        
                        Text(
                            pgmDetList.pgmInfo?.patientProgramdetails.assignedMember ?? ""
                            
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
                      
                    }
                 
                        .padding(.vertical,5)
                  
                    Group {
                        Text("Goals")
                            .foregroundColor(Color("title1"))
                            .font(Font.custom("Rubik-Regular", size: 14))
                        
                        ForEach(pgmDetList.pgmInfo?.patientProgramGoals.goalDetails ?? []) { item in
                            VStack(alignment: .leading, spacing: 10) {
                                Text(item.goal)
                                    .font(Font.custom("Rubik-SemiBold", size: 16))
                                    .foregroundColor(.black)
                                
                                Text(item.goalDetailDescription)
                                    .font(Font.custom("Rubik-Regular", size: 16))
                                    .foregroundColor(.black)
                                    .lineLimit(nil) // Allow the text to wrap to the next line
                                    .multilineTextAlignment(.leading) // Adjust alignment as needed
                                    .fixedSize(horizontal: false, vertical: true) // Allow the text to wrap
                            }
                            .frame(maxWidth: .infinity, alignment: .leading)
                            .padding(.horizontal, 5)
                            .padding(.vertical, 5)
                            .background(
                                RoundedRectangle(cornerRadius: 8, style: .continuous)
                                    .stroke(Color("textFieldBG"), lineWidth: 2)
                                    .background(Color("textFieldBG"))
                                    .cornerRadius(8)
                            )
                            .modifier(DynamicHeightModifier())
                            .padding(.bottom, 30) // Adjust vertical padding after each item
                            .padding(.top, 35)
                        }
                    }
               
                    .padding(.vertical, 5)
                
                }
         
                    .padding(.vertical,5)
                
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
    

    func datePgmInfo(timeval: String) -> String {
        let dateFormatter = DateFormatter()
        dateFormatter.dateFormat = "yyyy-MM-dd'T'HH:mm:ss"
        dateFormatter.timeZone = TimeZone(identifier: "UTC")
        
        guard let date = dateFormatter.date(from: timeval) else {
            return "Invalid date"
        }

        dateFormatter.dateFormat = "MMM d, yyyy"
        dateFormatter.timeZone = TimeZone.current
        return dateFormatter.string(from: date)
    }

    
}


struct DynamicHeightModifier: ViewModifier {
    func body(content: Content) -> some View {
        GeometryReader { geometry in
            content
                .frame(height: geometry.size.height + 15)
        }
    }
}


struct RPMProgramDetailsView_Previews: PreviewProvider {
    static var previews: some View {
        RPMProgramDetailsView()
    }
}


