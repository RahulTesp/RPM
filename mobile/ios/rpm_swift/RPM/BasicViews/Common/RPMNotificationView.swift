//
//  RMPNotificationView.swift
//  RPM
//
//  Created by Tesplabs on 07/04/1944 Saka.
//


import SwiftUI


struct RPMNotificationView: View {
    
    @Environment(\.presentationMode) var presentationMode
    @ObservedObject var notifList = NotificationViewModel()
    @State private var showDeleteConfirmation = false
    @State private var isLoading = true //  loading state
    
    var body: some View {
        ZStack {
            BackgroundView()
            
            if isLoading {
                // Show loading spinner
                
                VStack {
                    ProgressView()
                        .tint(Color("TextColorBlack"))
                    Text("Loading…")
                        .foregroundColor(Color("TextColorBlack"))
                }
                .frame(maxWidth: .infinity, maxHeight: .infinity, alignment: .center)
                

            } else {
                // Actual content
                VStack(alignment: .leading) {
                    ScrollView {
                        Text("\(notifList.notif?.totalUnRead ?? 0) Unread")
                            .font(.system(size: 25 , weight: .heavy))
                            .foregroundColor(Color("TextColorBlack"))
                        
                        NotificationListView(data: notifList.notif?.data ?? [])
                            .id(notifList.notif?.totalUnRead)
                    }
                }
                .padding(.horizontal, 12)
                .padding(.vertical, 32)
                .navigationBarBackButtonHidden(true)
                .navigationBarTitleDisplayMode(.inline)
                .toolbar {
                    ToolbarItem(placement: .principal) {
                        Text("Notifications")
                            .font(.system(size: 18))
                            .foregroundColor(Color("TextColorBlack"))
                    }
                    ToolbarItem(placement: .navigationBarTrailing) {
                        Button(action: {
                            showDeleteConfirmation = true
                        }) {
                            Text("Delete All")
                                .font(.system(size: 16))
                                .foregroundColor(Color("buttonColor"))
                        }
                    }
                }
                .alert(isPresented: $showDeleteConfirmation) {
                    Alert(
                        title: Text("Confirm Delete"),
                        message: Text("Do you really want to delete all?"),
                        primaryButton: .destructive(Text("Yes")) {
                            notifList.deleteAllNotifications()
                        },
                        secondaryButton: .cancel()
                    )
                }
                .navigationBarItems(leading: HStack {
                    Button(action: {
                        self.presentationMode.wrappedValue.dismiss()
                    }) {
                        Image("ArrowBack")
                    }
                })
            }
        }
        .onAppear {
            loadData()
        }
        .onReceive(NotificationCenter.default.publisher(for: Notification.Name("RefreshNotifications"))) { _ in
            loadData()
        }
        .ignoresSafeArea(.keyboard, edges: .bottom)
    }

    // MARK: - Helper
    private func loadData() {
        isLoading = true
        notifList.getnotify {
            // You should modify getnotify to accept a completion handler
            DispatchQueue.main.async {
                isLoading = false
            }
        }
    }
}


struct NotificationListView: View {
    let data: [Datum]

    var body: some View {
        ForEach(data) { day in
            VStack(alignment: .leading) {
                HStack {
                    Text(convertDateFormat(inputDate: day.notificationDate ?? ""))
                        .padding(.top, 10)
                        .foregroundColor(Color("title1"))
                    Spacer()
                }

                ForEach(day.notificationList) { notification in
                    NotificationScrollView(
                        time: convertUTCtoLocalNotificationDate(inputDate: notification.createdOn ?? "") ?? "",
                        decription: notification.notificationListDescription ?? "",
                        fontColor1: Color("darkGreen"),
                        fontColor2: .black,
                        bgColor: Color(.white)
                    )
                }
            }
        }
    }
}



func convertDateFormat(inputDate: String) -> String {
    
    print("convinputDate"+inputDate)
    
    let olDateFormatter = DateFormatter()
    olDateFormatter.dateFormat = "yyyy-MM-dd'T'HH:mm:ss"
    
    if let oldDate = olDateFormatter.date(from: inputDate) {
        let convertDateFormatter = DateFormatter()
        convertDateFormatter.dateFormat = "MMM dd, yyyy"
        
        return convertDateFormatter.string(from: oldDate)
    } else {
        // Handle the case where the date conversion fails
        return "Invalid Date"
    }
}

func converttodoDateFormat(inputDate: String) -> String {
    
    print("convinputDate"+inputDate)
    let olDateFormatter = DateFormatter()
    olDateFormatter.dateFormat = "yyyy-MM-dd'T'HH:mm:ss"
    
    if let oldDate = olDateFormatter.date(from: inputDate) {
        let convertDateFormatter = DateFormatter()
        convertDateFormatter.dateFormat = "MMM dd"
        
        return convertDateFormatter.string(from: oldDate)
    } else {
        // Handle the case where the date conversion fails
        return "Invalid Date"
    }
}

func convertChartDateFormat(inputDate: String) -> String? {
    print("inputDate.count")
    print(inputDate.count)
    if(inputDate.count >= 19)
    {
        let index = inputDate.index(inputDate.startIndex, offsetBy: 19)
        let mySubstring = inputDate[..<index] // Hello
        print("mySubstring")
        print(mySubstring)
        
        let dateFormatter = DateFormatter()
        dateFormatter.dateFormat = "yyyy-MM-dd'T'HH:mm:ss"
        
        dateFormatter.timeZone = TimeZone(abbreviation: "UTC")
        
        if let date = dateFormatter.date(from: String(mySubstring)) {
            dateFormatter.timeZone = TimeZone.current
            dateFormatter.dateFormat = "MMM-dd - h:mm a"
        
            return dateFormatter.string(from: date)
    }
 
    }
    return nil
}

func convertUTCtoLOCAL(inputDate: String) -> String? {
    // Create a date formatter for the input date format
    let apiDateFormatter = DateFormatter()
    apiDateFormatter.dateFormat = "yyyy-MM-dd'T'HH:mm:ss"
    
    // Set the time zone for the input date formatter
    apiDateFormatter.timeZone = TimeZone(abbreviation: "UTC") // Assuming the API date is in UTC

    // Parse the input date
    if let date = apiDateFormatter.date(from: inputDate) {
        // Create a date formatter for the local date format
        let localDateFormatter = DateFormatter()
        localDateFormatter.dateFormat = "MMM dd, h:mm a"
        
        // Set the time zone for the local date formatter to the system's current time zone
        localDateFormatter.timeZone = TimeZone.current
        
        // Convert the date to the local time zone
        let localDate = localDateFormatter.string(from: date)
        return localDate
    }

    return nil
}

func convertUTCtoLocalNotificationDate(inputDate: String) -> String? {
    // Create a date formatter for the input date format
    let apiDateFormatter = DateFormatter()
    apiDateFormatter.dateFormat = "yyyy-MM-dd'T'HH:mm:ss"
    
    // Set the time zone for the input date formatter
    apiDateFormatter.timeZone = TimeZone(abbreviation: "UTC") // Assuming the API date is in UTC

    // Parse the input date
    if let date = apiDateFormatter.date(from: inputDate) {
        // Create a date formatter for the local date format
        let localDateFormatter = DateFormatter()
        localDateFormatter.dateFormat = "h:mm a"
        
        // Set the time zone for the local date formatter to the system's current time zone
        localDateFormatter.timeZone = TimeZone.current
        
        // Convert the date to the local time zone
        let localDate = localDateFormatter.string(from: date)
        return localDate
    }

    return nil
}


func dateOfBirthConv(inputDate: String) -> String? {
    print("dateOfBirthConv")
    print(inputDate)
   
        let dateFormatter = DateFormatter()
        dateFormatter.dateFormat = "yyyy-MM-dd'T'HH:mm:ss"
        
        dateFormatter.timeZone = TimeZone(abbreviation: "UTC")
        
        if let date = dateFormatter.date(from: String(inputDate)) {
            dateFormatter.timeZone = TimeZone.current
            dateFormatter.dateFormat = "MMM dd,yyyy"
        
            return dateFormatter.string(from: date)
    }
    return nil
}


func convertUTCtoLocaldDOB(inputDate: String) -> String? {
    // Create a date formatter for the input date format
    let apiDateFormatter = DateFormatter()
    apiDateFormatter.dateFormat = "yyyy-MM-dd'T'HH:mm:ss"
    
    // Set the time zone for the input date formatter
    apiDateFormatter.timeZone = TimeZone(abbreviation: "UTC") // Assuming the API date is in UTC

    // Parse the input date
    if let date = apiDateFormatter.date(from: inputDate) {
        // Create a date formatter for the local date format
        let localDateFormatter = DateFormatter()
        localDateFormatter.dateFormat = "MMM d, yyyy"
        
        // Set the time zone for the local date formatter to the system's current time zone
        localDateFormatter.timeZone = TimeZone.current
        
        // Convert the date to the local time zone
        let localDate = localDateFormatter.string(from: date)
        return localDate
    }

    return nil
}


func convertdobformat(inputDate: String) -> String {
    
    print("convinputDate"+inputDate)

    let olDateFormatter = DateFormatter()
    olDateFormatter.dateFormat = "yyyy-MM-dd'T'HH:mm:ss"
    
    if let oldDate = olDateFormatter.date(from: inputDate) {
        let convertDateFormatter = DateFormatter()
        convertDateFormatter.dateFormat = "MMM d,yyyy"
        
        return convertDateFormatter.string(from: oldDate)
    } else {
        // Handle the case where the date conversion fails
        return "Invalid Date"
    }
}

func convertVRformat(inputDate: String) -> String? {
    print("inputDate.count")
    print(inputDate.count)
   
        let dateFormatter = DateFormatter()
        dateFormatter.dateFormat = "yyyy-MM-dd'T'HH:mm:ss"
        
        dateFormatter.timeZone = TimeZone(abbreviation: "UTC")
        
        if let date = dateFormatter.date(from: String(inputDate)) {
            dateFormatter.timeZone = TimeZone.current
            dateFormatter.dateFormat = "h:mm a"
        
            return dateFormatter.string(from: date)

    }
    return nil
}
