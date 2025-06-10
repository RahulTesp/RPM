//
//  RPMVitalsView.swift
//  RPM
//
//  Created by Tesplabs on 08/04/1944 Saka.
//

import SwiftUI


extension Date {
    
    static func getVitalCurrentDate() -> String {
        
        let dateFormatter = DateFormatter()
        
        dateFormatter.dateFormat = "EEEE, MMM dd, yyyy"
        
        return dateFormatter.string(from: Date())
        
    }
    
    // Function to set the time zone of a Date object
        func settingTimeZone(_ timeZone: TimeZone) -> Date {
            let calendar = Calendar.current
            let components = calendar.dateComponents(in: timeZone, from: self)
            return calendar.date(from: components) ?? self
        }
}



struct RPMVitalsView: View {
    @State private var currentMonthDate = Date()
    
    
    @State var cls : [String] = [""]
    @State var wakeUp = Date.now
    @State var tabViewSelectedId: Int = 2
    
    @State var showingSecVitalSummary : Bool = true
    @State var showVitalSummary : Bool = false
    @State var showingInfo : Int = 1
    @State var currentlySelectedId: Int = 1
    @State var selected : Date?
    @State var nextDays : String = Date.getVitalCurrentDate()
    @State var selectedCurrentdates : String = ""
    @State var selectedDt = Date()
    let formatter = DateFormatter()
    
    @State private var currentDate = Date.getCurrentVitalDate()
    @State private var currentDateFix = Date.getCurrentVitalDate()
   
    @State private var currentDateValue : String = Date.getCurrentDate()
    @State private var currentDayValue : String = Date.getCurrentDay()
    @State private var isFirstAppearance = true
    @StateObject var chartDays = RPMVitalsChartDaysViewModel()
    @State private var returningFromClinicalInfo = false
 
    @StateObject var vitalSummaryList = RPMVitalDaySummaryViewModel()

    @EnvironmentObject var appModel: AppModel
    @EnvironmentObject var messagesManager: MessagesManager
    @EnvironmentObject var participantsManager: ParticipantsManager
    @EnvironmentObject var conversationManager: ConversationManager
    @EnvironmentObject var navigationHelper: NavigationHelper
    

    var body: some View {
      
        ScrollView(.vertical) {
            GeometryReader { geometry in
                let screenWidth = geometry.size.width
            VStack ( alignment: .leading){
                Group
                {
                    HStack
                    {
                        
                        Button(action: {
                            navigationHelper.resetToHomeTab() //  clears nav and goes to Home tab
                        }) {
                            Image("ArrowBack")
                                .renderingMode(.template)
                                .foregroundColor(Color("buttonColor"))
                                .padding()
                        }
                        
                        
                        Spacer()
                        Text("Vitals").foregroundColor(Color("title1"))
                            .font(Font.custom("Rubik-Regular", size: 16))
                        Spacer()
                        
                    }
                    
                }
                
                Spacer()
                
                if( showingSecVitalSummary == true) {
                    
                    // DATE VIEW
                    HStack {
                        Spacer()
                        
                        Button {
                            vitalSummaryList.loading = true
                            print("currentDatefixb4",currentDateFix)
                            print("currentDateb4",currentDate)
                            print("currentDateb4",TimeZone.current)
                            
                            
                            print("currentDa",Date.getCurrentVitalDate())
                            
                            
                            currentDate = Calendar.current.date(byAdding: .day, value: -1, to: currentDate)!
                            
                            
                            print("currentDateaftr",currentDate)
                            
                            print("cdformtd",currentDate.formatted(
                                .iso8601
                                    .month()
                                    .day()
                                    .year()
                                    .dateSeparator(.dash)
                                
                            ))
                            
                            
                            vitalSummaryList.getVitalSummary(
                                
                                startDate:
                                    
                                    currentDate.formatted(
                                        .iso8601
                                            .month()
                                            .day()
                                            .year()
                                            .dateSeparator(.dash)
                                        
                                    )
                                
                                ,  endDate:
                                    
                                    currentDate.formatted(
                                        .iso8601
                                            .month()
                                            .day()
                                            .year()
                                            .dateSeparator(.dash)
                                        
                                    )
                                
                                
                                , completed: {_,_ in
                                    print("vitals chng")
                                    
                                }
                            )
                            
                            
                            showVitalSummary = true
                            
                            
                            
                        } label: {
                            Image(systemName: "chevron.left")
                                .foregroundColor(Color("title1"))
                                .font(.title3)
                            
                        }
                        
                        
                        
                        // NOTE : DATE SHOWING SECTION
                        
                        currentDate == currentDateFix  ?
                        Group
                        {
                            HStack
                            {
                                Text("TODAY")
                                    .foregroundColor(Color("title1"))
                                    .font(Font.custom("Rubik-SemiBold", size: 16))
                                Text(
                                    "- \(currentDayValue)"
                                    
                                )
                                .foregroundColor(Color("title1"))
                                .font(Font.custom("Rubik-Regular", size: 16))
                                
                                
                                Text(
                                    currentDateValue
                                    
                                )
                                .foregroundColor(Color("title1"))
                                .font(Font.custom("Rubik-Regular", size: 16))
                                
                                
                            }
                        }
                        
                        :
                        Group
                        {
                            HStack
                            {
                                Text("")
                                Text("")
                                Text(currentDate.formatted(
                                    .dateTime.weekday(.wide).day().month().year(
                                        
                                    ) ))  .foregroundColor(Color("title1"))
                            }
                            
                        }
                        
                        
                        Button {
                            vitalSummaryList.loading = true
                            currentDate = Calendar.current.date(byAdding: .day, value: 1, to: currentDate)!
                            
                            
                            vitalSummaryList.getVitalSummary(
                                
                                startDate:
                                    
                                    
                                    currentDate.formatted(
                                        .iso8601
                                            .month()
                                            .day()
                                            .year()
                                            .dateSeparator(.dash)
                                        
                                    )
                                
                                
                                
                                ,  endDate:
                                    
                                    currentDate.formatted(
                                        .iso8601
                                            .month()
                                            .day()
                                            .year()
                                            .dateSeparator(.dash)
                                        
                                    )
                                
                                
                                
                                , completed: {_,_ in
                                    //   print("vitals chng")
                                    
                                }
                            )
                            
                            
                            showVitalSummary = true
                            
                            
                        } label: {
                            Image(systemName: "chevron.right")
                                .foregroundColor(.gray)
                                .font(.title3)
                            
                        }
                        Spacer()
                    }
                    .padding(.horizontal,6)
                    
                    .onAppear()
                    {
                        
                        
                        if returningFromClinicalInfo {
                            // Logic to handle the return from Clinical Info page
                            print("Returned from Clinical Info page to Vitals page")
                            returningFromClinicalInfo = false
                        }
                        else
                        {
                            currentDate = currentDateFix
                            
                            currentDateValue =
                            vitalSummaryList.currentDateValue
                            
                            
                            vitalSummaryList.defaultVitalSumm()
                            
                            showVitalSummary = true
                            // Set the flag to false after the initial appearance
                            
                            // Set the flags accordingly
                            isFirstAppearance = false
                            
                        }
                        
                    }
                }
                else
                {
                    HStack(spacing:1.0){
                        Spacer()
                        
                        TopTabViews(text: "<7 Days", colorf: Color(.white), colorb:  Color("title1"),
                                    id: 2, tabViewSelectedId: $tabViewSelectedId,
                                    showingInfo: $showingInfo
                        )
                        
                        TopTabViews(text: "<30 Days", colorf: Color(.white), colorb:  Color("title1"),
                                    id: 3, tabViewSelectedId: $tabViewSelectedId,
                                    showingInfo: $showingInfo
                        )
                        
                        
                        
                        Spacer()
                    }
                    
                    .background(Color("bgColorDark"))
                    .cornerRadius(13)
                    
                }
                
                
                HStack
                {
                    showingSecVitalSummary == true ?
                    
                    Text("Vitals").foregroundColor(.black)
                        .font(Font.custom("Rubik-SemiBold", size: 24))
                    
                    :
                    
                    Text("Health Trends")
                        .font(.system(size: 25 , weight: .heavy))
                    
                    
                    Spacer()
                    
                    
                    HStack{
                        TopButtons(text: "ListView", colorf: Color("darkGreen"), colorb: Color("transparentGreen"),
                                   id: 1, currentlySelectedId: $currentlySelectedId,
                                   showingSection: $showingSecVitalSummary
                                   
                        )
                        TopButtons(text: "GraphOutline", colorf: Color("darkGreen"), colorb:  Color(.orange),
                                   id: 2, currentlySelectedId: $currentlySelectedId,
                                   showingSection: $showingSecVitalSummary
                        )
                        
                    }.background(Color("transparentGreen"))
                        .cornerRadius(14)
                    
                }.padding()
                
                // CONTENT VIEW
                
                
                HStack{
                    
                    
                    if showingSecVitalSummary == true &&
                        
                        (vitalSummaryList.vitalSummary?.bloodPressure == []
                         && vitalSummaryList.vitalSummary?.weight == []
                         && vitalSummaryList.vitalSummary?.bloodOxygen == []
                         
                         && vitalSummaryList.vitalSummary?.bloodGlucose == []
                         
                        ) {
                        
                        NoDataVSView(returningFromClinicalInfo: $returningFromClinicalInfo).environmentObject(navigationHelper)
                    } else {
                        if showingSecVitalSummary == true && showVitalSummary == true {
                            
                            
                            VitalDataScrollView(vitalSummaryList: vitalSummaryList,returningFromClinicalInfo: $returningFromClinicalInfo
                                                ,
                                                           viewWidth: screenWidth * 0.80
                            )  .environmentObject(navigationHelper)
                            
                            
                        }
                    }
                    
                    
                    
                    // MARK: SECTION FOR 7 DAYS CHART VIEW
                    
                    if !showingSecVitalSummary && tabViewSelectedId == 2 {
                        
                        if chartDays.vitalsGraph7.isEmpty {
                            
                            NoDataView()
                        } else {
                            
                            
                            ScrollView(.horizontal) {
                                HStack {
                                    // Ensure chartDays.vitalsGraph7 is an array, not a single object
                                    VitalsListView(vitalsList: chartDays.vitalsGraph7 ?? [] ,
                                                   viewWidth: screenWidth * 0.80)
                                }
                            }
                            
                        }
                    }
                    
                    
                    // MARK: SECTION FOR 30 DAYS CHART VIEW
                    
                    if showingSecVitalSummary != true && tabViewSelectedId != 2 {
                        
                        if chartDays.vitalsGraph30.isEmpty {
                            
                            NoDataView()
                        }
                        
                        
                        else {
                            
                            
                            ScrollView(.horizontal) {
                                HStack {
                                    // Ensure chartDays.vitalsGraph7 is an array, not a single object
                                    VitalsListView30(vitalsList: chartDays.vitalsGraph30 ?? [])
                                }
                            }
                            
                            
                        }
                        
                        
                    }
                    
                    
                }
                
            }.padding(.horizontal, 12)
            
                .alert(isPresented: $chartDays.showNoInternetAlert) {
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
            
                .alert(isPresented: $vitalSummaryList.showNoInternetAlert) {
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
        .padding(.bottom, 10)
      
        .background(Color("bgColor"))
    }
    private func format(date: Date) -> String {
        let dateFormatter = DateFormatter()
        dateFormatter.dateStyle = .medium
        return dateFormatter.string(from: date)
    }
    
    private func setDateString(sel : Date) -> String{
        let formatter = DateFormatter()
        formatter.dateFormat = "yyyy-MM-dd"
        
        self.selectedCurrentdates = formatter.string(from: sel)
        return selectedCurrentdates
    }
    
    func arrayOfDates() -> [String] {
        
        let numberOfDays: Int = 14
        let startDate = Date()
        let formatter: DateFormatter = DateFormatter()
        formatter.dateFormat = "EEE d/M"
        let calendar = Calendar.current
        var offset = DateComponents()
        var dates: [Any] = [formatter.string(from: startDate)]
        
        for i in 1..<numberOfDays {
            offset.day = i
            let nextDay: Date? = calendar.date(byAdding: offset, to: startDate)
            let nextDayString = formatter.string(from: nextDay!)
            dates.append(nextDayString)
     
        }
        let swiftArray = dates as NSArray as? [String]
        
        // Use swiftArray here
        print(swiftArray)
        
        print("dates")
        print(dates)
        return swiftArray ?? []
    }
    
    
    
    func yesterDay() -> String {
        var dayComponent = DateComponents()
       
        dayComponent.day = -1
   
        let calendar = Calendar.current
      
        let dateFormatter = DateFormatter()
        dateFormatter.locale = Locale(identifier: "en_US_POSIX")
        dateFormatter.dateFormat = "MMMM d, yyyy"
        var datesnw = dateFormatter.date(from: nextDays)
   
        let nextDay =  calendar.date(byAdding: dayComponent, to: datesnw ?? Date())!
 
        datesnw = nextDay
    
        let formatter = DateFormatter()
        formatter.locale = .current
        formatter.dateFormat = "MMMM d, yyyy"
        selectedCurrentdates = formatter.string(from: nextDay)
  
        return dateFormatter.string(from: datesnw ?? Date())
     
    }
    func getMonth(date: Date) -> String {
        let dateFormatter = DateFormatter()
        dateFormatter.dateFormat = "LLLL"
        return dateFormatter.string(from: date)
    }
    func getDayNumber(date: Date) -> Int {
        let calendar = Calendar.current
        let components = calendar.dateComponents([.day], from: date)
        return components.day ?? 0
    }
    func getDayShort(date: Date) -> String {
        let dateFormatter = DateFormatter()
        dateFormatter.dateFormat = "EEEEE"
        return dateFormatter.string(from: date)
    }

    private func getPreviousDate() -> String{
   
        let dateFormatters = DateFormatter()
        dateFormatters.dateFormat = "dd MMMM yyyy , EEEE"
        var dayComponent    = DateComponents()
        dayComponent.day    = +20 // For removing one day (yesterday): -1
        let theCalendar     = Calendar.current
        let nextDate        = theCalendar.date(byAdding: dayComponent, to: Date())

        return dateFormatters.string(from: nextDate ?? Date())
 
    }
    
    func dateFormatter (dateFormat: String, date: Date) -> String{
        let dateFormatter = DateFormatter()
        dateFormatter.dateFormat = dateFormat
        return dateFormatter.string(from: date)
    }
    
}


struct LoadingView: View {
    var body: some View {
        ProgressView()
            .tint(Color("TextColorBlack"))
        Text("Loading…")
            .foregroundColor(Color("TextColorBlack"))
            .frame(maxWidth: .infinity, maxHeight: .infinity, alignment: .center)
    }
}

struct VitalDataScrollView: View {
    @ObservedObject var vitalSummaryList: RPMVitalDaySummaryViewModel
    @Binding var returningFromClinicalInfo: Bool
    @EnvironmentObject var navigationHelper: NavigationHelper
    var viewWidth: CGFloat
    
    var body: some View {
        
        
        
        ScrollView(.horizontal) {
            HStack {
                if vitalSummaryList.loading {
                    LoadingView()
                }
                else
                {
            
                VitalDataItemView(vitalSummaryList: vitalSummaryList,returningFromClinicalInfo: $returningFromClinicalInfo,
                                  viewWidth: viewWidth
                )
                        .environmentObject(navigationHelper)
              }
            }
        }.cornerRadius(20)
        
   
    }
}
struct VitalDataItemView: View {
    
    @ObservedObject var vitalSummaryList: RPMVitalDaySummaryViewModel
    @Binding var returningFromClinicalInfo: Bool
    @EnvironmentObject var navigationHelper: NavigationHelper
    var viewWidth: CGFloat
    
    var body: some View {
      
            VStack(alignment: .leading) {
               
                ScrollView(.horizontal) {
                    HStack (spacing: 2){
                       
                        BPReadings(bpitem : vitalSummaryList.vitalSummary?.bloodPressure ?? [],  returningFromClinicalInfo: $returningFromClinicalInfo, showViewMore: true, viewWidth: viewWidth)
                            .environmentObject(navigationHelper)

                    
                        WeightReadings(wtitem: vitalSummaryList.vitalSummary?.weight ?? [],  returningFromClinicalInfo: $returningFromClinicalInfo, showViewMore: true, viewWidth: viewWidth)
                            .environmentObject(navigationHelper)

                            GlucoseReadings(gluitem: vitalSummaryList.vitalSummary?.bloodGlucose ?? [],  returningFromClinicalInfo: $returningFromClinicalInfo, showViewMore: true, viewWidth: viewWidth)
                            .environmentObject(navigationHelper)

                            OxygenReadings(oxyitem: vitalSummaryList.vitalSummary?.bloodOxygen ?? [],  returningFromClinicalInfo: $returningFromClinicalInfo, showViewMore: true, viewWidth: viewWidth)
                            .environmentObject(navigationHelper)
                    
                    }
                } .frame( height: 200)
               
    
            }

    }
}

struct TopTabViews :  View{
    
    var text : String
    var colorf: Color
    var colorb: Color
    let id: Int
    
    @Binding var tabViewSelectedId: Int
    @Binding var showingInfo:  Int
    var body : some View{
        
        
        Button(action :{
            
            self.tabViewSelectedId = self.id
         
            
            if
                
                (self.id == 1) {
                
                showingInfo = 1
                
                
                
            }
            else if
                
                
                (self.id == 2) {
                
                tabViewSelectedId = 2
                showingInfo = 2
                
            }
            
            else if (self.id == 3)
            {
                tabViewSelectedId = 3
                showingInfo = 3
                
            }
            
            
        })
        {
            
            Text(text)
         
                .foregroundColor(  id == tabViewSelectedId ?  colorf : colorb)
                .padding(12)
                .frame( minWidth: 38, minHeight: 28)
            
            
                .background(   id == tabViewSelectedId ?
                               Color("title1")
                               
                               : Color("bgColor"))
            
                .foregroundColor(id == tabViewSelectedId ? .green : .red)
        }
        .cornerRadius(15)
        
        .border(Color("ColorGrey"), width: 0.3)
        
    }
    
    
}

struct NoDataView: View {
    var body: some View {
        Text("No Readings")
            .foregroundColor(.red)
            .frame(maxWidth: .infinity, maxHeight: .infinity, alignment: .center)
    }
}


struct NoDataVSView: View {
    @Binding var returningFromClinicalInfo: Bool
    @EnvironmentObject var navigationHelper: NavigationHelper
    var body: some View {
        VStack
        {
            Text("No Readings!")
                .foregroundColor(.red)
                .frame(maxWidth: .infinity, maxHeight: .infinity, alignment: .center)
            
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



struct VitalsListView: View {
    let vitalsList: [RPMVitalsChartDaysDataModel]
    var viewWidth: CGFloat
    
    var body: some View {
        
        ScrollView(.horizontal, showsIndicators: false) {
            HStack(spacing: 16) {  // Arrange each item horizontally
                ForEach(vitalsList, id: \.vitalName) { vital in
                
    
                        chart7DaysVitalsView(item: vital,
                                             viewWidth: viewWidth)  // Pass a single item
                 
                }
            }
           
        }
        

        
    }
}

struct VitalsListView30: View {
    let vitalsList: [RPMVitalsChartDaysDataModel]
    
    var body: some View {
        
        
        
        ScrollView(.horizontal, showsIndicators: false) {
            HStack(spacing: 16) {  // Arrange each item horizontally
                ForEach(vitalsList, id: \.vitalName) { vital in
 
                    chart30DaysView(item: vital) // Pass a single item
                 
                }
            }
           
        }


    }
}


struct TopButtons :  View{
    
    var text : String
    var colorf: Color
    var colorb: Color
    let id: Int
    @Binding var currentlySelectedId: Int
    
    @Binding var showingSection:  Bool
    var body : some View{
        Group
        {
            HStack{
                Button(action :{
                    
                    self.currentlySelectedId = self.id
                    
                    if
                        
                        (self.id == 1) {
                        print(showingSection)
                        showingSection = true
                        
                        print(showingSection)
                    }
                    else{
                        showingSection = false
                        
                    }
                    
                    print("pressed")
              
                    
                })
                {
                    
                    Image(text)
                        .renderingMode(.template)
                   
                        .padding(30)
                        .frame( maxWidth: 30, maxHeight: 30)
                        .badge(10)
                    
                        .background(   id == currentlySelectedId ?
                                       //  Color.orange
                                       Color("title1")
                                       : Color("transparentGreen"))
                    
                        .foregroundColor(id == currentlySelectedId ?  Color.white :  Color("title1"))
                }
                .cornerRadius(17)
                
            }
        }
        
    }
    
}
func localToUTCtime(dateStr: String) -> String? {
    let dateFormatter = DateFormatter()
    dateFormatter.dateFormat = "yyyy-MM-dd"
    dateFormatter.calendar = Calendar.current
    dateFormatter.timeZone = TimeZone.current
    
    if let date = dateFormatter.date(from: dateStr) {
        dateFormatter.timeZone = TimeZone(abbreviation: "UTC")
        dateFormatter.dateFormat = "yyyy-MM-dd"
   
        return dateFormatter.string(from: date)
    }
    return nil
}

