//
//  RPMProgramTimelineView.swift
//  RPM
//
//  Created by Tesplabs on 26/07/1944 Saka.
//



import SwiftUI

struct RPMProgramTimelineView: View {

    @ObservedObject  var pgmDetList = RPMProgramInfoViewModel()
    
    var body: some View {
        GeometryReader { geometry in
         
        if  pgmDetList.loading {
            
            VStack(spacing: 20) {
                Spacer()
                ProgressView()
                    .tint(Color("TextColorBlack"))
                Text("Loading…")
                    .foregroundColor(Color("TextColorBlack"))
                Spacer()
            }
            .frame(maxWidth: .infinity, maxHeight: .infinity)

        } else
        
        {
            
            ScrollView
            {
                VStack(alignment: .leading, spacing: 20)
                {
                    
                    Text("Program Timeline")
                        .foregroundColor(.black)
                        .font(Font.custom("Rubik-SemiBold", size: 24))
                    
                    
                    
                    Text("Prescription").foregroundColor(Color("buttonColor"))
                        .font(Font.custom("Rubik-SemiBold", size: 16))
                     
                 
                    Group
                    {
                        
                        Text("Date")
                            .foregroundColor(Color("title1"))
                            .font(Font.custom("Rubik-Regular", size: 14))
                        
                        
                        Text(
                            self.dateTimlnInfo(  timeval:     pgmDetList.pgmInfo?.patientPrescribtionDetails.prescribedDate ?? "")
                          
                        )
                        .font(Font.custom("Rubik-Regular", size: 16))
                        .foregroundColor(.black)
                        .padding(8)
                        .frame(width: geometry.size.width - 32, alignment: .leading)
                        
                        .background(
                            RoundedRectangle(cornerRadius: 8)
                                .fill(Color("textFieldBG"))
                                .overlay(
                                    RoundedRectangle(cornerRadius: 8)
                                        .stroke(Color("textFieldBG"), lineWidth: 2)
                                )
                        )
                        

                        Text("Physician")
                            .foregroundColor(Color("title1"))
                            .font(Font.custom("Rubik-Regular", size: 14))
                        
                        
                        Text(
                       
                            pgmDetList.pgmInfo?.patientPrescribtionDetails.physician ?? ""
                            
                        )
                        .font(Font.custom("Rubik-Regular", size: 16))
                        .foregroundColor(.black)
                        .padding(8)
                        .frame(width: geometry.size.width - 32, alignment: .leading)
                        .background(
                            RoundedRectangle(cornerRadius: 8)
                                .fill(Color("textFieldBG"))
                                .overlay(
                                    RoundedRectangle(cornerRadius: 8)
                                        .stroke(Color("textFieldBG"), lineWidth: 2)
                                )
                        )

                        HStack
                        {
                            VStack(alignment:.leading)
                            {
                                
                                Text("Clinic")
                                    .foregroundColor(Color("title1"))
                                    .font(Font.custom("Rubik-Regular", size: 14))
                                
                                
                                Text(
                                    
                                    pgmDetList.pgmInfo?.patientPrescribtionDetails.clinic ?? ""
                                    
                                )
                                .font(Font.custom("Rubik-Regular", size: 16))
                                .foregroundColor(.black)

                                .padding(8)
                                .frame(width: ((geometry.size.width - 32) - 8) / 2, alignment: .leading)
                    
                                .background(
                                    RoundedRectangle(cornerRadius: 8)
                                        .fill(Color("textFieldBG"))
                                        .overlay(
                                            RoundedRectangle(cornerRadius: 8)
                                                .stroke(Color("textFieldBG"), lineWidth: 2)
                                        )
                                )
                                
                            }
                            VStack(alignment:.leading)
                            {
                                
                                Text("Clinic Code")
                                    .foregroundColor(Color("title1"))
                                    .font(Font.custom("Rubik-Regular", size: 14))
                                
                                
                                Text(
                                    
                                    pgmDetList.pgmInfo?.patientPrescribtionDetails.clinicCode ?? ""
                                    
                                ) .font(Font.custom("Rubik-Regular", size: 16))
                                    .foregroundColor(.black)

                                    .padding(8)
                                    .frame(width: ((geometry.size.width - 32) - 8) / 2, alignment: .leading)
                             
                                    .background(
                                        RoundedRectangle(cornerRadius: 8)
                                            .fill(Color("textFieldBG"))
                                            .overlay(
                                                RoundedRectangle(cornerRadius: 8)
                                                    .stroke(Color("textFieldBG"), lineWidth: 2)
                                            )
                                    )
                            }
                            
                        }
                        
                    }
                    
                    Group
                    {
                        
                        Text("Order Date")
                            .foregroundColor(Color("title1"))
                            .font(Font.custom("Rubik-Regular", size: 14))
                        
                        
                        Text(
                            formattedConsultationDate(  dateString: pgmDetList.pgmInfo?.patientPrescribtionDetails.consultationDate ?? "")
                          
                        )
                        .font(Font.custom("Rubik-Regular", size: 16))
                        .foregroundColor(.black)

                        .padding(8)
                        .frame(width: geometry.size.width - 32, alignment: .leading)
                        .background(
                            RoundedRectangle(cornerRadius: 8)
                                .fill(Color("textFieldBG"))
                                .overlay(
                                    RoundedRectangle(cornerRadius: 8)
                                        .stroke(Color("textFieldBG"), lineWidth: 2)
                                )
                        )
                        
                        Text("Diagnosis")
                            .foregroundColor(Color("title1"))
                            .font(Font.custom("Rubik-Regular", size: 14))
                        
                        
                        ForEach(
                            pgmDetList.pgmInfo?.patientPrescribtionDetails.patientDiagnosisInfos ?? []
                            
                            
                        ) {  item in
                          
                            Text(
                            
                                "\(item.diagnosisCode) / \(item.diagnosisName)"
                                
                            )
                            .font(Font.custom("Rubik-Regular", size: 16))
                            .foregroundColor(.black)
                            .padding(8)
                            .frame(width: geometry.size.width - 32, alignment: .leading)
                            .background(
                                RoundedRectangle(cornerRadius: 8)
                                    .fill(Color("textFieldBG"))
                                    .overlay(
                                        RoundedRectangle(cornerRadius: 8)
                                            .stroke(Color("textFieldBG"), lineWidth: 2)
                                    )
                            )
                        }
                   
                    }

                    Text("Enrollment").foregroundColor(Color("buttonColor"))
                        .font(Font.custom("Rubik-SemiBold", size: 16))
                   
                    let enrollmentPersonal = (pgmDetList.pgmInfo?.patientEnrolledDetails.patientEnrolledInfos.isEmpty == false)
                    ? pgmDetList.pgmInfo!.patientEnrolledDetails.patientEnrolledInfos[0].enrollmentPersonal ?? ""
                    : ""
                    
                    
                    Group
                    {
                        
                        Text("Date")
                            .foregroundColor(Color("title1"))
                            .font(Font.custom("Rubik-Regular", size: 14))
                        
                        
                        Text(
                            self.dateTimlnInfo(  timeval:  pgmDetList.pgmInfo?.patientEnrolledDetails.assignedDate ?? "")
                            
                            
                        )
                        .font(Font.custom("Rubik-Regular", size: 16))
                        .foregroundColor(.black)

                        .padding(8)
                        .frame(width: geometry.size.width - 32, alignment: .leading)
                        .background(
                            RoundedRectangle(cornerRadius: 8)
                                .fill(Color("textFieldBG"))
                                .overlay(
                                    RoundedRectangle(cornerRadius: 8)
                                        .stroke(Color("textFieldBG"), lineWidth: 2)
                                )
                        )
                      
                        Text("Enrollment Personal")
                            .foregroundColor(Color("title1"))
                            .font(Font.custom("Rubik-Regular", size: 14))
                      
                        Text(enrollmentPersonal
                           
                        )
                        .font(Font.custom("Rubik-Regular", size: 16))
                        .foregroundColor(.black)

                        .padding(8)
                        .frame(width: geometry.size.width - 32, alignment: .leading)
                        .background(
                            RoundedRectangle(cornerRadius: 8)
                                .fill(Color("textFieldBG"))
                                .overlay(
                                    RoundedRectangle(cornerRadius: 8)
                                        .stroke(Color("textFieldBG"), lineWidth: 2)
                                )
                        )
                        
                    }
                    
                    Text("Active").foregroundColor(Color("buttonColor"))
                        .font(Font.custom("Rubik-SemiBold", size: 16))
                 
                    Group
                    {
                        
                        Text("Date")
                            .foregroundColor(Color("title1"))
                            .font(Font.custom("Rubik-Regular", size: 14))
                        
                        
                        Text(
                            self.activeDateTimlnInfo(  timeval:        pgmDetList.pgmInfo?.activePatientDetails.assignedDate ?? "")
                      
                        )
                        .font(Font.custom("Rubik-Regular", size: 14))
                        .foregroundColor(.black)

                        .padding(8)
                        .frame(width: geometry.size.width - 32, alignment: .leading)
                        .background(
                            RoundedRectangle(cornerRadius: 8)
                                .fill(Color("textFieldBG"))
                                .overlay(
                                    RoundedRectangle(cornerRadius: 8)
                                        .stroke(Color("textFieldBG"), lineWidth: 2)
                                )
                        )
                       
                        Text("Care Team Personal")
                            .foregroundColor(Color("title1"))
                            .font(Font.custom("Rubik-Regular", size: 14))
                        
                        
                        Text(
                            
                            pgmDetList.pgmInfo?.activePatientDetails.assigneeName ?? ""
                            
                        )
                        .font(Font.custom("Rubik-Regular", size: 16))
                        .foregroundColor(.black)

                        .padding(8)
                        .frame(width: geometry.size.width - 32, alignment: .leading)
                        .background(
                            RoundedRectangle(cornerRadius: 8)
                                .fill(Color("textFieldBG"))
                                .overlay(
                                    RoundedRectangle(cornerRadius: 8)
                                        .stroke(Color("textFieldBG"), lineWidth: 2)
                                )
                        )
                        
                        Text("Care Team Manager")
                            .foregroundColor(Color("title1"))
                            .font(Font.custom("Rubik-Regular", size: 14))
                        
                        
                        Text(
                            
                            pgmDetList.pgmInfo?.activePatientDetails.managerName ?? ""
                            
                        )
                        .font(Font.custom("Rubik-Regular", size: 16))
                        .foregroundColor(.black)

                        .padding(8)
                        .frame(width: geometry.size.width - 32, alignment: .leading)
                        .background(
                            RoundedRectangle(cornerRadius: 8)
                                .fill(Color("textFieldBG"))
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
    }
    }
    
    
    func formattedConsultationDate(dateString: String) -> String {
           let dateFormatter = DateFormatter()
           dateFormatter.dateFormat = "MM/dd/yyyy hh:mm:ss a"
           
           if let date = dateFormatter.date(from: dateString) {
               dateFormatter.dateFormat = "MMM d, yyyy"
               return dateFormatter.string(from: date)
           } else {
               return ""
           }
       }
    
    func dateTimlnInfo(timeval: String) -> String
    {
        print("activetimeval",timeval)
        let dateFormatter = DateFormatter()
        dateFormatter.dateFormat = "yyyy-MM-dd'T'HH:mm:ss"
        dateFormatter.timeZone = NSTimeZone(name: "UTC") as TimeZone?
        let date = dateFormatter.date(from: timeval) ?? Date()
        
        // change to a readable time format and change to local time zone
        dateFormatter.dateFormat = "MMM d, yyyy"
        dateFormatter.timeZone = NSTimeZone.local
        let timeStamp = dateFormatter.string(from: date)
      
        print("activetimeStamp",timeStamp)
        return timeStamp
 
    }
    func activeDateTimlnInfo(timeval: String) -> String
    {
        print("activetimeval",timeval)
        let dateFormatter = DateFormatter()
        dateFormatter.dateFormat = "yyyy-MM-dd'T'HH:mm:ss.SS"
        dateFormatter.timeZone = NSTimeZone(name: "UTC") as TimeZone?
        let date = dateFormatter.date(from: timeval) ?? Date()
        
        // change to a readable time format and change to local time zone
        dateFormatter.dateFormat = "MMM d, yyyy"
        dateFormatter.timeZone = NSTimeZone.local
        let timeStamp = dateFormatter.string(from: date)
      
        print("activetimeStamp",timeStamp)
        return timeStamp
 
    }
  
}

struct RPMProgramTimelineView_Previews: PreviewProvider {
    static var previews: some View {
        RPMProgramTimelineView()
    }
}



