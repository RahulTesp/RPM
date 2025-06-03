//
//  RMPTodoListView.swift
//  RPM
//
//  Created by Prajeesh Prabhakar on 04/06/22.
//

import SwiftUI


func getWeek(startDate: Date) -> [Date] {
    var calendar = Calendar.current
    calendar.firstWeekday = 2 // Start from Monday

    // Find the Monday of the current week
    let components = calendar.dateComponents([.yearForWeekOfYear, .weekOfYear], from: startDate)
    guard let weekStart = calendar.date(from: components) else {
        return []
    }

    // Create an array of dates from Monday to Sunday
    return (0..<7).compactMap { calendar.date(byAdding: .day, value: $0, to: weekStart) }
}

func getDayShort(date: Date) -> String {
    let formatter = DateFormatter()
    formatter.locale = Locale(identifier: "en_US_POSIX") // or use current locale
    formatter.dateFormat = "E" // short day format like "Mon"
    return formatter.string(from: date)
}


func getDayNumber(date: Date) -> Int {
    let calendar = Calendar.current
    return calendar.component(.day, from: date)
}


struct RPMTodoListView: View {

    @StateObject var todoList = RPMTodoListViewModel()
    @State private var currentDate = Date()
    @State private var selectedIndex: Int?
    @State private var showTodoList: Bool = false
    @Environment(\.presentationMode) var mode: Binding<PresentationMode>
    @EnvironmentObject var appModel: AppModel // Add this line
    @EnvironmentObject var messagesManager: MessagesManager // Add this line
    @EnvironmentObject var participantsManager: ParticipantsManager // Add this line
    @EnvironmentObject var conversationManager: ConversationManager
    @EnvironmentObject var navigationHelper: NavigationHelper
    
    // Local @State for alert presentation
     @State private var showNoInternetAlert: Bool = false

    private var dates: [Date] {
        getWeek(startDate: currentDate)
    }

    private var formattedCurrentDate: String {
        let formatter = DateFormatter()
        formatter.dateFormat = "yyyy-MM-dd"
        return formatter.string(from: currentDate)
    }

    var body: some View {
        GeometryReader { geometry in
                let width = geometry.size.width
                let height = geometry.size.height
        VStack(alignment: .leading) {
            headerView
            
            WeekDatePickerView(
                dates: dates,
                currentDate: $currentDate,
                selectedIndex: $selectedIndex,
                showTodoList: $showTodoList,
                todoList: todoList,
                widthFull: width
            )
            
            dateHeaderControls
            
            if todoList.loadingtodoAct {
                loadingView
            } else {
                if todoList.todoAct?.isEmpty ?? true {
                    Text("0 Activities !")
                        .foregroundColor(.red)
                        .frame(maxWidth: .infinity, maxHeight: .infinity, alignment: .center)
                } else {
                    activityListView
                }
            }
        }
        
        // Bind alert to local @State, not directly to view model's published property
        .alert(isPresented: $showNoInternetAlert) {
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
         
        .onReceive(todoList.$showNoInternetAlert) { newValue in
            showNoInternetAlert = newValue
        }
   
    }
    }

    struct WeekDatePickerView: View {
        let dates: [Date]
        @Binding var currentDate: Date
        @Binding var selectedIndex: Int?
        @Binding var showTodoList: Bool
        var todoList: RPMTodoListViewModel
        let widthFull: CGFloat

        var body: some View {
            ScrollView(.horizontal) {
                HStack {
                    ForEach(dates.indices, id: \.self) { index in
                        let date = dates[index]
                        let isToday = Calendar.current.isDateInToday(date)
                        let isSelected = index == selectedIndex

                        VStack {
                            Text(getDayShort(date: date))
                                .foregroundColor(Color("title1"))
                                .font(.caption2)
                                .padding(.bottom, 10)

                            Button {
                                currentDate = date
                                selectedIndex = index
                                showTodoList = true
                                
                                // Format the date and fetch new data
                                let formatter = DateFormatter()
                                formatter.dateFormat = "yyyy-MM-dd"
                                let formatted = formatter.string(from: date)
                                
                                todoList.loadingtodoAct = true
                                todoList.todoActivities(dt: formatted) { _, _ in
                                 
                                    todoList.loadingtodoAct = false
                                    print("todoActivitiesloadeddone")
                                }
                            } label: {
                                Text("\(getDayNumber(date: date))")
                                    .foregroundColor(isSelected ? .white : (isToday && selectedIndex == nil ? .white : Color("title1")))
                                    .background(
                                        Circle()
                                            .fill(isSelected || (isToday && selectedIndex == nil) ? Color("title1") : Color.clear)
                                            .frame(width: 30, height: 30)
                                    )
                            }
                            .font(.title3)
                        }
                        .frame(width: 50, height: 100)
                    }
                }
                .frame(width: widthFull - 40, height: 100)
                .padding(.leading, 10) // Add padding on the left side
                .padding(.trailing, 10) // Add padding on the left side
                .padding(.horizontal, 10)
                .onAppear {
                    // Load initial data on appear
                    todoList.loadingtodoAct = true
                    let formatter = DateFormatter()
                    formatter.dateFormat = "yyyy-MM-dd"
                    let formatted = formatter.string(from: currentDate)
                    todoList.todoActivities(dt: formatted) { _, _ in
                        todoList.loadingtodoAct = false
                    }
                }
            }
        }
    }

    private var headerView: some View {
        HStack {
            
            Button(action: {
                navigationHelper.resetToHomeTab() // 👈 clears nav and goes to Home tab
            }) {
                Image("ArrowBack")
                    .renderingMode(.template)
                    .foregroundColor(Color("buttonColor"))
                    .padding()
            }

            Spacer()

            Text("Todo List")
                .foregroundColor(Color("title1"))
                .font(Font.custom("Rubik-Regular", size: 16))
                .padding(.top, 6)

            Spacer()
        }
    }

    private var dateHeaderControls: some View {
        HStack {
            Spacer()

            Button {
                if let newDate = Calendar.current.date(byAdding: .day, value: -1, to: currentDate) {
                    currentDate = newDate
                    updateSelectedIndex()
                    fetchDataForCurrentDate()
                }

            } label: {
                Image(systemName: "chevron.left")
                    .foregroundColor(Color("title1"))
                    .font(.title3)
            }

            if Calendar.current.isDateInToday(currentDate) {
                Group {
                    Text("TODAY")
                        .foregroundColor(Color("title1"))
                        .font(Font.custom("Rubik-SemiBold", size: 16))
                    Text("- \(Date.getCurrentDay())")
                        .foregroundColor(Color("title1"))
                        .font(Font.custom("Rubik-Regular", size: 16))
                    Text(Date.getCurrentDate())
                        .foregroundColor(Color("title1"))
                        .font(Font.custom("Rubik-Regular", size: 16))
                }
            } else {
                Text(currentDate.formatted(.dateTime.weekday(.wide).day().month().year()))
                    .foregroundColor(Color("title1"))
            }

            Button {
                if let newDate = Calendar.current.date(byAdding: .day, value: 1, to: currentDate) {
                    currentDate = newDate
                    updateSelectedIndex()
                    fetchDataForCurrentDate()
                }

            } label: {
                Image(systemName: "chevron.right")
                    .foregroundColor(.gray)
                    .font(.title3)
            }

            Spacer()
        }
        .padding(.bottom, 6)
        .frame(maxWidth: .infinity, alignment: .center) // This centers the HStack horizontally
    }


    private var activityListView: some View {
        ScrollView {
            VStack(alignment: .leading) {
                HStack {
                    Text("    \(todoList.todoAct?.count ?? 0)")
                        .foregroundColor(.black)
                        .font(Font.custom("Rubik-SemiBold", size: 24))
                    Text((todoList.todoAct?.count ?? 0) <= 1 ? " Activity" : " Activities")
                        .foregroundColor(.black)
                        .font(Font.custom("Rubik-SemiBold", size: 24))
                    Spacer()
                }

                ForEach(todoList.todoAct ?? []) { item in
                    ToDoScrollView(
                        schedType: item.scheduleType ?? "",
                        date: item.date ?? "",
                        desc: item.decription ?? ""
                    )
                }
            }
            .padding(.horizontal)
        }
    }

    private var loadingView: some View {
        VStack {
            ProgressView()
                .tint(Color("TextColorBlack"))
            Text("Loading…")
                .foregroundColor(Color("TextColorBlack"))
        }
        .frame(maxWidth: .infinity, maxHeight: .infinity, alignment: .center)
    }

    private func updateSelectedIndex() {
        if let index = dates.firstIndex(where: { Calendar.current.isDate($0, inSameDayAs: currentDate) }) {
            selectedIndex = index
        }
    }

    private func fetchDataForCurrentDate() {
        let formatter = DateFormatter()
        formatter.dateFormat = "yyyy-MM-dd"
        let formatted = formatter.string(from: currentDate)
        todoList.loadingtodoAct = true
        todoList.todoActivities(dt: formatted) { _, _ in
            todoList.loadingtodoAct = false
        }
    }
}
