
//
//  RPMVitalMonthReadingsView.swift
//  RPM
//
//  Created by Tesplabs on 28/07/1944 Saka.
//

import SwiftUI

struct RPMVitalMonthReadingsView: View {
    
    // NOTE : RETURN START DATE OF MONTH
    private var startDateOfMonth: String {
        let components = Calendar.current.dateComponents([.year, .month], from: currentMonthDate)
        
        let startOfMonth = Calendar.current.date(from: components)!
        print("startOfMontheeee")
        print(startOfMonth)
        return format(date: startOfMonth)
    }
    // NOTE : RETURN END DATE OF MONTH
    private var endDateOfMonth: String {
        var components = Calendar.current.dateComponents([.year, .month], from: currentMonthDate)
        components.month = (components.month ?? 0) + 1
        components.hour = (components.hour ?? 0) - 1
        let endOfMonth = Calendar.current.date(from: components)!
        print("enddteeee")
        print(endOfMonth)
        return format(date: endOfMonth)
    }
    
    // NOTE : RETURN START DATE OF CURRENT MONTH
    private var startDateOfMonths: String {
        let components = Calendar.current.dateComponents([.year, .month], from: currentMonthDateFix)
        
        let startOfMonth = Calendar.current.date(from: components)!
        return format(date: startOfMonth)
    }
    // NOTE : RETURN END DATE OF CURRENT MONTH
    private var endDateOfMonths: String {
        var components = Calendar.current.dateComponents([.year, .month], from: currentMonthDateFix)
        components.month = (components.month ?? 0) + 1
        components.hour = (components.hour ?? 0) - 1
        let endOfMonth = Calendar.current.date(from: components)!
        return format(date: endOfMonth)
    }
    
    
    @State var currentDate = Date()
    
    @State private var currentMonthDate = Date()
    @State private var currentMonthDateFix = Date()
    @State var wakeUpMonth = Date.now
    
    @ObservedObject  var vitalReadingsList = RPMVitalReadingsViewModel()
    @State private var returningFromClinicalInfo = false
    
    var body: some View {
        
        ZStack{
            BackgroundView()
            VStack ( alignment: .leading){
             
                HStack {
                    Spacer()
                    
                    Button {
                        vitalReadingsList.loading = true
                        currentMonthDate = Calendar.current.date(byAdding: .month, value: -1, to: currentMonthDate)!
                        print("currentMonthDate1")
                        print(currentMonthDate)
                        print("startDateOfMonth")
                        print(startDateOfMonth)
                        print("endDateOfMonth")
                        print(endDateOfMonth)
                   
                        vitalReadingsList.getVitalReadings(
                            
                            startDate:
                                
                                convertVitalMonthRdngsDate(inputDate: startDateOfMonth)

                            ,  endDate:
                                
                                convertVitalMonthRdngsDate(inputDate: endDateOfMonth)
     
                            , completed: {_,_ in
                                print("vitals chng")
                                
                            }
                        )
                
                    } label: {
                        Image(systemName: "chevron.left")
                            .foregroundColor(Color("title1"))
                            .font(.title3)
                        
                    }
                    
                    Spacer()
                    
                    currentMonthDate == currentMonthDateFix  ?
                    
                    HStack
                    {
                   
                        Text(Date.getCurrentMonth())
                            .foregroundColor(Color("title1"))
                    }
                    :
                    HStack
                    {
                        
                        Text(currentMonthDate.formatted(
                            .dateTime.month(.wide).year(
                                
                            ) )) .foregroundColor(Color("title1"))
                   
                    }
                  
                    Spacer()
                   
                    Button {
                        vitalReadingsList.loading = true
                        currentMonthDate = Calendar.current.date(byAdding: .month, value: 1, to: currentMonthDate)!
                        print("currentMonthDate2")
                        print(currentMonthDate)
                        print("howruuuu")
                        vitalReadingsList.getVitalReadings(
                            
                            startDate:
                                convertVitalMonthRdngsDate(inputDate: startDateOfMonth) ,  endDate:   convertVitalMonthRdngsDate(inputDate: endDateOfMonth) , completed: {_,_ in
                                    print("vitals chng")
                                    
                                }
                        )
                   
                    } label: {
                        Image(systemName: "chevron.right")
                            .foregroundColor(.gray)
                            .font(.title3)
                        
                    }
                    Spacer()
                }
                .padding(.horizontal,6)
                .padding(.vertical,6)
               
                Text("   Vital Readings")
                    .foregroundColor(.black)
                    .font(Font.custom("Rubik-SemiBold", size: 24))
             
                if (vitalReadingsList.vitalReadings?.bloodPressure == []
                    && vitalReadingsList.vitalReadings?.weight == []
                    && vitalReadingsList.vitalReadings?.bloodOxygen == []
                    
                    && vitalReadingsList.vitalReadings?.bloodGlucose == []
          
                )
                {
                    Text("No Readings !").foregroundColor(.red)
                        .frame(
                            maxWidth: .infinity,
                            maxHeight: .infinity,
                            alignment: .center)
                }
                
                else
                
                {
                    if
                        vitalReadingsList.loading {
                        VStack
                        {
                            ProgressView()
                                .tint(Color("TextColorBlack"))
                               
                            Text("Loading…") .foregroundColor( Color("TextColorBlack"))
                        }
                        .frame(
                            maxWidth: .infinity,
                            maxHeight: .infinity,
                            alignment: .center)
                    }
                    
                    else{
                
                        ScrollView(.horizontal)
                        {
                       
                            HStack
                            {
                               
                                    BPReadings(bpitem : vitalReadingsList.vitalReadings?.bloodPressure ?? [], returningFromClinicalInfo: $returningFromClinicalInfo)
                                    
                                                WeightReadings(wtitem : vitalReadingsList.vitalReadings?.weight ?? [], returningFromClinicalInfo: $returningFromClinicalInfo)
                                   
                                                GlucoseReadings(gluitem : vitalReadingsList.vitalReadings?.bloodGlucose ?? [], returningFromClinicalInfo: $returningFromClinicalInfo)
                                             
                                                OxygenReadings(oxyitem : vitalReadingsList.vitalReadings?.bloodOxygen ?? [], returningFromClinicalInfo: $returningFromClinicalInfo)
                           
                            }
                      
                        }
                    }
                }
     
            }
            
            .cornerRadius(15)
            
            .alert(isPresented: $vitalReadingsList.showNoInternetAlert) {
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
        
        .ignoresSafeArea(.keyboard, edges: .bottom)
    }
 
    private func format(date: Date) -> String {
        let dateFormatter = DateFormatter()
        dateFormatter.dateStyle = .medium
        print("coming date view")
        print(date)
        print("coming date formtd view")
        print( date.formatted(.dateTime.day().month().year()))
        return date.formatted(.dateTime.day().month().year())
    }
    
}

struct WeightReadings: View {
    
    var wtitem : [Weight]
    @Binding var returningFromClinicalInfo: Bool
    @EnvironmentObject var navigationHelper: NavigationHelper
    var showViewMore: Bool = false
    
    var body: some View {
        
        VStack
        {
            ScrollView
            {
                VStack(spacing: 0) {
                HStack
                {
                   
                    Text(
                        " Weight" ) .foregroundColor(Color("darkGreen"))
                        .font(.system(size: 20,weight: .medium))
                    Spacer()
                    
                }
                
                .frame(maxWidth: .infinity, minHeight: 45)
                .background(Color("ColorGreen"))
                .cornerRadius(8, corners: [.topLeft, .topRight])
              
                VStack(spacing: 0) {
                    
                    ForEach(
                        wtitem
                     
                    ) { item in
                    
                        HStack
                        {
                  
                            WeightReadingsScrollView(
                         
                                wt : Float(item.bWlbs),
                                rt : String(item.readingTimes),
                                weightstatus : String(item.weightstatus)
                              
                            )
                       
                        }
                        
                    }
                }
                    // Conditionally show "View More..."
                    if showViewMore {
                        Button(action: {
                            navigationHelper.path.append(.clinicalinfoView(returningFromClinicalInfo))
                        }) {
                            HStack {
                                Text(" View More...")
                                    .foregroundColor(.gray)
                                    .font(Font.custom("Rubik-Regular", size: 15))
                                Spacer()
                            }
                            .padding(.horizontal, 10)
                            .padding(.vertical, 10)
                            .background(Color(red: 237/255, green: 239/255, blue: 245/255))
                            .cornerRadius(8)
                            .frame(maxWidth: .infinity, alignment: .leading) // Makes it expand and align left
                        }
                    }
            }
            }
          
        }  .frame(width: 400)
            .background(Color("textFieldBG"))
            .padding(.horizontal, 15)
        
            .cornerRadius(15)
    }
}

struct GlucoseReadings: View {
    
    var gluitem : [BloodGlucose]
    @Binding var returningFromClinicalInfo: Bool
    @EnvironmentObject var navigationHelper: NavigationHelper
    var showViewMore: Bool = false
    
    var body: some View {
        
        VStack
        {
            ScrollView
            {
                VStack(spacing: 0) {
                HStack
                {
                   
                    Text(
                        " Blood Glucose" ) .foregroundColor(Color("darkGreen"))
                        .font(.system(size: 20,weight: .medium))
                    Spacer()
                    
                }
                
                .frame(maxWidth: .infinity, minHeight: 45)
                .background(Color("ColorGreen"))
                .cornerRadius(8, corners: [.topLeft, .topRight])
                
                
                VStack(spacing: 0) {
                    
                    ForEach(
                        gluitem
                        
                        
                    ) { item in
                   
                        
                        HStack
                        {
                            
                            GlucoseReadingsScrollView(
                              
                                bgmdl : Int(item.bgmdl),
                                schedule :  String(item.schedule),
                                rt : String(item.readingTimeG),
                                gluStatus : String(item.gluStatus)
                              
                            )
                          
                        }
                     
                    }
                }
                    // Conditionally show "View More..."
                    if showViewMore {
                        Button(action: {
                            navigationHelper.path.append(.clinicalinfoView(returningFromClinicalInfo))
                        }) {
                            HStack {
                                Text(" View More...")
                                    .foregroundColor(.gray)
                                    .font(Font.custom("Rubik-Regular", size: 15))
                                Spacer()
                            }
                            .padding(.horizontal, 10)
                            .padding(.vertical, 10)
                            .background(Color(red: 237/255, green: 239/255, blue: 245/255))
                            .cornerRadius(8)
                            .frame(maxWidth: .infinity, alignment: .leading) // Makes it expand and align left
                        }
                    }
            }
            }
    
        }  .frame(width: 400)
            .background(Color("textFieldBG"))
            .padding(.horizontal, 15)
        
            .cornerRadius(15)
    }
}

struct OxygenReadings: View {
    
    var oxyitem : [BloodOxygen]
    @Binding var returningFromClinicalInfo: Bool
    @EnvironmentObject var navigationHelper: NavigationHelper
    var showViewMore: Bool = false
    
    var body: some View {
        
        VStack
        {
            ScrollView
            {
                VStack(spacing: 0) {
                    HStack
                    {
                        Text(
                            " Oxygen" ) .foregroundColor(Color("darkGreen"))
                            .font(.system(size: 20,weight: .medium))
                        Spacer()
                        
                    }
                    
                    .frame(maxWidth: .infinity, minHeight: 45)
                    .background(Color("ColorGreen"))
                    .cornerRadius(8, corners: [.topLeft, .topRight])
                    
                    VStack(spacing: 0) {
                        
                        ForEach(
                            oxyitem
                            
                            
                        ) { item in

                        
                        HStack
                        {
                        
                            OxygenReadingsScrollView(
                              
                                oxy : Int(item.oxygen),
                                pulse : Int(item.pulseO),
                                rt : String(item.readingTimeO),
                                pulseStatus : String(item.pulseStatus),
                                oxygenStatus : String(item.oxygenStatus),
                                status : String(item.status)
                              
                            )
                        }
                    }
                }
                    // Conditionally show "View More..."
                    if showViewMore {
                        Button(action: {
                            navigationHelper.path.append(.clinicalinfoView(returningFromClinicalInfo))
                        }) {
                            HStack {
                                Text(" View More...")
                                    .foregroundColor(.gray)
                                    .font(Font.custom("Rubik-Regular", size: 15))
                                Spacer()
                            }
                            .padding(.horizontal, 10)
                            .padding(.vertical, 10)
                            .background(Color(red: 237/255, green: 239/255, blue: 245/255))
                            .cornerRadius(8)
                            .frame(maxWidth: .infinity, alignment: .leading) // Makes it expand and align left
                        }
                    }
            }
        }
         
        }  .frame(width: 400)
            .background(Color("textFieldBG"))
            .padding(.horizontal, 15)
        
            .cornerRadius(15)
    }
}


struct BPReadings: View {
    
    var bpitem : [BloodPressure]
    @Binding var returningFromClinicalInfo: Bool
    @EnvironmentObject var navigationHelper: NavigationHelper
    var showViewMore: Bool = false
    
    var body: some View {
        
        VStack
        {
            ScrollView
            {
                
                VStack(spacing: 0) {
                
                HStack
                {
            
                    Text(
                        " Blood Pressure" )
                    .foregroundColor(Color("darkGreen"))
                    .font(.system(size: 20,weight: .medium))
                    //.font(Font.custom("Rubik-Regular", size: 20 )
                    Spacer()
                    
                }
            
                .frame(maxWidth: .infinity, minHeight: 45)
                .background(Color("ColorGreen"))
                .cornerRadius(8, corners: [.topLeft, .topRight])
                
                VStack(spacing: 0) {
                    
                    ForEach(
                        bpitem
                      
                    ) { item in
                        
                        HStack
                        {
                        
                            VitalReadingsScrollView(
                                
                                systolic : item.systolic,
                                diastolic : item.diastolic,
                                pulse : item.pulse,
                                systolicstatus: item.systolicStatus,
                                diastolicstatus: item.diastolicStatus,
                                pulsestatus: item.pulseStatus,
                                status:item.status,
                                time : item.readingTime
                                
                                
                            )
                      
                        }
                        
                    }
                }
                    // Conditionally show "View More..."
                    if showViewMore {
                        Button(action: {
                            navigationHelper.path.append(.clinicalinfoView(returningFromClinicalInfo))
                        }) {
                            HStack {
                                Text(" View More...")
                                    .foregroundColor(.gray)
                                    .font(Font.custom("Rubik-Regular", size: 15))
                                Spacer()
                            }
                            .padding(.horizontal, 10)
                            .padding(.vertical, 10)
                            .background(Color(red: 237/255, green: 239/255, blue: 245/255))
                            .cornerRadius(8)
                            .frame(maxWidth: .infinity, alignment: .leading) // Makes it expand and align left
                        }
                    }
                    
                    
            }
            }
        } .frame(width: 400)
            .background(Color("textFieldBG"))
            .padding(.horizontal, 15)
            .cornerRadius(15)
    }
}

struct RPMVitalMonthReadingsView_Previews: PreviewProvider {
    static var previews: some View {
        RPMVitalMonthReadingsView()
    }
}

