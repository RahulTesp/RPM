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
                let width = geometry.size.width
                let height = geometry.size.height
        if  pgmDetList.loading {
            
            Spacer()
            ProgressView()
                .tint(Color("TextColorBlack"))
            
            Text("Loading…") .foregroundColor( Color("TextColorBlack"))
                .padding(.vertical,15)
            Spacer()
            
        } else
        
        {
            
            ScrollView
            {
                VStack(alignment: .leading)
                {
                    
                    Text(" Program Timeline")
                        .foregroundColor(.black)
                        .font(Font.custom("Rubik-SemiBold", size: 24))
                    
                    
                    
                    Text(" Prescription").foregroundColor(Color("buttonColor"))
                        .font(Font.custom("Rubik-SemiBold", size: 16))
                        .padding(.top,6)
                 
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
                   
                        Text("Physician")
                            .foregroundColor(Color("title1"))
                            .font(Font.custom("Rubik-Regular", size: 14))
                        
                        
                        Text(
                       
                            pgmDetList.pgmInfo?.patientPrescribtionDetails.physician ?? ""
                            
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
                                
                                Text("Clinic")
                                    .foregroundColor(Color("title1"))
                                    .font(Font.custom("Rubik-Regular", size: 14))
                                
                                
                                Text(
                                    
                                    pgmDetList.pgmInfo?.patientPrescribtionDetails.clinic ?? ""
                                    
                                )
                                .font(Font.custom("Rubik-Regular", size: 16))
                                .foregroundColor(.black)
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
                                
                                Text("Clinic Code")
                                    .foregroundColor(Color("title1"))
                                    .font(Font.custom("Rubik-Regular", size: 14))
                                
                                
                                Text(
                                    
                                    pgmDetList.pgmInfo?.patientPrescribtionDetails.clinicCode ?? ""
                                    
                                ) .font(Font.custom("Rubik-Regular", size: 16))
                                    .foregroundColor(.black)
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
                        
                    }.padding(.horizontal,10)
                        .padding(.vertical,5)
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
                            .frame(width: 320, height: 70, alignment: .leading)
                            
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
                    
                    Text(" Enrollment").foregroundColor(Color("buttonColor"))
                        .font(Font.custom("Rubik-SemiBold", size: 16))
                        .padding(.top,6)
                    
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
                        
                        
                        Text("Enrollment Personal")
                            .foregroundColor(Color("title1"))
                            .font(Font.custom("Rubik-Regular", size: 14))
                      
                        Text(enrollmentPersonal
                           
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
                      
                    }.padding(.horizontal,10)
                        .padding(.vertical,5)
                    
                    
                    Text(" Active").foregroundColor(Color("buttonColor"))
                        .font(Font.custom("Rubik-SemiBold", size: 16))
                        .padding(.top,6)
                    
                 
                    Group
                    {
                        
                        Text("Date")
                            .foregroundColor(Color("title1"))
                            .font(Font.custom("Rubik-Regular", size: 14))
                        
                        
                        Text(
                            self.dateTimlnInfo(  timeval:        pgmDetList.pgmInfo?.activePatientDetails.assignedDate ?? "")
                      
                        )
                        .font(Font.custom("Rubik-Regular", size: 14))
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
                       
                        Text("Care Team Personal")
                            .foregroundColor(Color("title1"))
                            .font(Font.custom("Rubik-Regular", size: 14))
                        
                        
                        Text(
                            
                            pgmDetList.pgmInfo?.activePatientDetails.assigneeName ?? ""
                            
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
                        
                        Text("Care Team Manager")
                            .foregroundColor(Color("title1"))
                            .font(Font.custom("Rubik-Regular", size: 14))
                        
                        
                        Text(
                            
                            pgmDetList.pgmInfo?.activePatientDetails.managerName ?? ""
                            
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
                    }.padding(.horizontal,10)
                        .padding(.vertical,5)
                    
                    
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
  
        let dateFormatter = DateFormatter()
        dateFormatter.dateFormat = "yyyy-MM-dd'T'HH:mm:ss"
        dateFormatter.timeZone = NSTimeZone(name: "UTC") as TimeZone?
        let date = dateFormatter.date(from: timeval) ?? Date()
        
        // change to a readable time format and change to local time zone
        dateFormatter.dateFormat = "MMM d, yyyy"
        dateFormatter.timeZone = NSTimeZone.local
        let timeStamp = dateFormatter.string(from: date)
  
        return timeStamp
 
    }
  
}

struct RPMProgramTimelineView_Previews: PreviewProvider {
    static var previews: some View {
        RPMProgramTimelineView()
    }
}



