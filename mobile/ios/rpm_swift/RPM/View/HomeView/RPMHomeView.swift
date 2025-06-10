//
//  RMPHomeView.swift
//  RPM
//
//  Created by Prajeesh Prabhakar on 04/06/22.
//

import SwiftUI
import SocketIO

extension Date {
    
    static func getCurrentDate() -> String {
        
        let dateFormatter = DateFormatter()
        
        dateFormatter.dateFormat = "MMM dd, yyyy"
        
        return dateFormatter.string(from: Date())
        
    }
    
    static func getCurrentVitalDate() -> Date {
        // Get the current calendar
        var calendar = Calendar.current
        
        // Set the time zone of the calendar to the current system time zone
        calendar.timeZone = TimeZone.current
        
        // Get the current date with the correct time zone
        return calendar.startOfDay(for: Date())
    }

    static func getCurrentDay() -> String {
        
        let dateFormatter = DateFormatter()
        
        dateFormatter.dateFormat = "EEEE,"
        
        return dateFormatter.string(from: Date())
        
    }
    
    static func getCurrentMonth() -> String {
        
        let dateFormatter = DateFormatter()
        
        dateFormatter.dateFormat = "MMMM yyyy"
        
        return dateFormatter.string(from: Date())
        
    }
}



struct RPMHomeView: View {
    
    @StateObject  var notifList = NotificationViewModel()
    @EnvironmentObject var homeViewModel: RPMHomeViewModel
    @EnvironmentObject var appModel: AppModel
    @EnvironmentObject var messagesManager: MessagesManager
    @EnvironmentObject var participantsManager: ParticipantsManager
    @EnvironmentObject var conversationManager: ConversationManager
    @EnvironmentObject var navigationHelper: NavigationHelper
    @State private var action: Bool = false
    @StateObject var todoList = RPMTodoListViewModel()
    @StateObject var chartDays = RPMVitalsChartDaysViewModel()
    @StateObject var memberDetList = MembersListViewModel()
    @StateObject private var loginViewModel = RPMLoginViewModel()
    @StateObject private var notificationManager = NotificationManager.shared
    @State private var isMediaSetup = false
    @State private var roomName: String = ""
    @State private var isShowingMediaSetup = false
    @State private var isShowingRoom = false
    @State private var isShowingPictureInPicture = false
    @EnvironmentObject var callManager: CallManager
    @EnvironmentObject var roomManager: RoomManager
    @EnvironmentObject var localParticipant: LocalParticipantManager
    @EnvironmentObject var mediaSetupViewModel: MediaSetupViewModel
    @State private var showAlertProxy: Bool = false
    @State private var showRejectionAlert = false
    @State private var rejectionMessage = ""
    @ObservedObject var sessionManager = SessionManager.shared

    
    var body: some View {
        ZStack {
        VStack {
            if  homeViewModel.loading
                    
            {
                
                LoadingCircleView(homeViewModel: homeViewModel)
            }
            
            else
            {
                
                VStack(alignment: .leading, spacing: 0)
                {
                    
                    VStack(spacing: 0){
                        // Notification Bell Section
                        HStack {
                            Spacer()
                            
                            Button(action: {
                                navigationHelper.path.append(.notificationView)
                                
                            }) {
                                Image("BellOutline")
                            }
                            
                            // Notification count bubble
                            
                            if let unreadCount = notifList.notif?.totalUnRead, unreadCount > 0 {
                                Text("\(unreadCount)")
                                    .padding(5)
                                    .font(.system(size: 12))
                                    .background(Color("buttonColor"))
                                    .foregroundColor(.white)
                                    .clipShape(Circle())
                                    .offset(x: -20, y: -10)
                            }
                         
                            
                        }
                    
                        .padding(.horizontal)
                        .frame(width: UIScreen.main.bounds.width, height: 20)
                        .padding(.top, 50)
                        
                        // Profile Info or Loader
                        if homeViewModel.loading {
                            ProgressView()
                            Text("Loading…")
                        } else {
                            profileInfoSection
                        }
                        
                        // Top Buttons
                        
                        HStack {
                            
                            
                            ZStack(alignment: .topTrailing) {
                                TopButton(
                                    text: "ChatOutline",
                                    colorf: Color("darkGreen"),
                                    colorb: Color("transparentGreen"),
                                    flagval: 1
                                )
                                .environmentObject(appModel)
                                .environmentObject(navigationHelper)
                                .environmentObject(conversationManager)
                              
                                
                                if conversationManager.unreadCount > 0 {
                                    Text("\(conversationManager.unreadCount)")
                                        .font(.caption2).bold()
                                        .foregroundColor(.white)
                                        .padding(6)
                                        .background(Color.red)
                                        .clipShape(Circle())
                                        .offset(x: 10, y: -10)  // Adjust position of badge
                                        .transition(.scale)
                                }
                                
                            }
                            
                            TopButton(
                                text: "PhoneOutline",
                                colorf: Color("darkGreen"),
                                colorb: Color("transparentGreen"),
                                flagval: 2
                            )
                            .environmentObject(appModel)
                            .environmentObject(navigationHelper)
                            .environmentObject(conversationManager)
                        }
                      
                        .padding(.vertical)
                   
                    }
                    .background(Color("lightGreen"))
                  
                    ScrollView
                    {
                        VStack
                        {
                            
                            if  homeViewModel.loading {
                                
                                ProgressView()
                                
                            }
                            else{
                                
                                VStack (alignment:.leading){
                                    
                                    HStack{
                                        
                                        Text(homeViewModel.accounts?.programName ?? "" )
                                        
                                            .foregroundColor(.black)
                                            .font(Font.custom("Rubik-Regular", size: 16))
                                        Spacer()
                                        
                                        
                                        HStack
                                        {
                                            Text(homeViewModel.accounts?.status ?? "" )
                                                .padding(5)
                                                .font(Font.custom("Rubik-Regular", size: 14))
                                            
                                                .foregroundColor(ColourStatus.getForegroundColor(for: homeViewModel.accounts?.status))
                                            
                                                .frame( minWidth :  70)
                                                .fixedSize(horizontal: true, vertical: false)
                                            
                                                .background(ColourStatus.getBackgroundColor(for: homeViewModel.accounts?.status))
                                            
                                        }.clipShape(RoundedRectangle(cornerRadius:20))
                                    }
                                    
                                }
                                .navigationBarBackButtonHidden(true)
                                .frame(maxWidth: .infinity, minHeight: 60)
                                .padding(.horizontal)
                                .padding(.vertical,5)
                                .background(Color("avgGreen"))
                                .cornerRadius(15)
                                .padding(.horizontal,10)
                                .padding(.vertical,6)
                            }
                        }
                        
                        .background( Color("lightGreen"))
                    
                        .cornerRadius(10, corners: [.bottomLeft, .bottomRight])
                        
                        ScrollView {
                            dateHeader
                            todoSection
                            if isRPMProgram {
                                recentVitalsSection
                            }
                        }
                        
                    }
                 
                }
                .background(Color("ChatBGcolor"))
                .onAppear {
                    
                    localParticipant.isMicOn = true
                    localParticipant.isCameraOn = true
                    
                    todoList.refresh()
                    
                    //  Move all necessary configurations here before navigation
                    callManager.configure(roomManager: roomManager)
                    roomManager.configure(localParticipant: localParticipant)
                    //           localParticipant.configure(identity: AuthStore.shared.userDisplayName)
                    mediaSetupViewModel.configure(localParticipant: localParticipant)
                    
                    //localParticipant.isMicOn = true
                    //localParticipant.isCameraOn = true
                    
                    if let deepLink = DeepLinkStore.shared.consumeDeepLink() {
                        if case let .room(name) = deepLink {
                            self.roomName = name
                        }
                    }
                }
                
                
                .fullScreenCover(isPresented: $isShowingRoom) {
                    
                    RoomViewDependencyWrapper(roomName: roomName, isShowingPictureInPicture: $isShowingPictureInPicture)
                        .environmentObject(callManager)
                        .environmentObject(roomManager)
                        .environmentObject(localParticipant)
                        .environmentObject(mediaSetupViewModel)
                }
                
                .edgesIgnoringSafeArea(.bottom)
                
                .hiddenNavigationBarStyle()
                
                .ignoresSafeArea(.all)
                
                .navigationBarHidden(true)
                
                .navigationBarBackButtonHidden(true)
                
            }
            
        }
            
            //  TOAST OVERLAY SECTION
                   if showRejectionAlert {
                       VStack {
                           Spacer()
                           Text(rejectionMessage)
                               .padding()
                               .background(Color.black.opacity(0.8))
                               .foregroundColor(.white)
                               .cornerRadius(10)
                               .padding(.bottom, 50)
                               .transition(.opacity)
                               .animation(.easeInOut, value: showRejectionAlert)
                       }
                   }
               
        
    }
                  .alert(notificationManager.notificationTitle,
                                isPresented: $showAlertProxy,
                                actions: {
                                    Button("No", role: .cancel) {
                                        print("No tapped")
                                        showAlertProxy = false
                                        notificationManager.showAlert = false
                                        
                                        if let room = notificationManager.roomName {
                                            homeViewModel.rejectCall(roomName: room) { success in
                                                rejectionMessage = success
                                                    ? "Rejected call notification sent successfully."
                                                    : "Failed to reject call. Please try again."
                                                DispatchQueue.main.async {
                                                    showRejectionAlert = true
                                                    
                                                    //  Automatically hide after 3 seconds
                                                       DispatchQueue.main.asyncAfter(deadline: .now() + 3) {
                                                           showRejectionAlert = false
                                                       }
                                                    
                                                }
                                            }
                                        }
                                    }
                                    Button("Yes") {
                                        print("Yes tapped")
                                        showAlertProxy = false
                                        notificationManager.showAlert = false
                                        
                                        if let room = notificationManager.roomName {
                                            self.roomName = room
                                        }
                                        
                                        DispatchQueue.main.asyncAfter(deadline: .now() + 0.3) {
                                            isShowingRoom = true
                                        }
                                    }
                                },
                                message: {
                                    Text("Do you want to join?")
                                })
        
        
        
     
        
                  .onChange(of: sessionManager.didReceiveUnauthorized) { isUnauthorized in
                      if isUnauthorized {
                          
                          loginViewModel.logout { res, val in
                              DispatchQueue.main.async {
                                  print("Logout Result: \(res ?? "nil")")

                                  loginViewModel.isLoggedOut = true
                                  loginViewModel.loggedIn = false

                                  if let bundleID = Bundle.main.bundleIdentifier {
                                      UserDefaults.standard.removePersistentDomain(forName: bundleID)
                                  }

                                  DispatchQueue.global(qos: .background).async {
                                      
                                      print("morecallManager: \(callManager)")
                                      print("moreroomManager: \(roomManager)")
                                      print("morelocalParticipant: \(localParticipant)")

                                      callManager.disconnect()
                               
                                      roomManager.disconnect()
                                    
                                      appModel.signOut()
                                  
                                      DispatchQueue.main.async {
                                          homeViewModel.reset()

                                                      //  Critical: Reset navigation on main thread
                                                      navigationHelper.path = []
                                          navigationHelper.selectedTab = 0
                                                      navigationHelper.path.append(.login)
                                          print(" navigationHelper.path: \( navigationHelper.path)")
                                                  }
                                      
                                  }
                              }
                          }
                      }
                  }

        
                  .onReceive(notificationManager.$showAlert) { newValue in
                      showAlertProxy = newValue
                  }

        
                  .onReceive(NotificationCenter.default.publisher(for: Notification.Name("RefreshNotifications"))) { _ in
                      print("RefreshNotifications")
                      DispatchQueue.main.async {
                          notifList.getnotify()
                      }
                  }

                  .onAppear {
              
                      homeViewModel.dashboard()
                      notifList.getnotify()
                  
                      
                  }
    }
    
    private var dateHeader: some View {
        HStack {
            Text(Date.getCurrentDay())
                .textCase(.uppercase)
            Text(Date.getCurrentDate())
            Spacer()
        }
        .foregroundColor(Color("buttonColor"))
        .font(Font.custom("Rubik-Regular", size: 16))
        .padding(.horizontal, 10)
        .padding(.vertical, 10)
    }

    private var todoSection: some View {
        VStack(alignment: .leading) {
            todoHeader
            if todoList.todoAct?.isEmpty ?? true {
                Text("NO DATA !").foregroundColor(.red).padding(.horizontal)
            } else if todoList.loadingtodoAct {
                ProgressView()
                Text("Loading…")
            } else {
                ScrollView(.horizontal) {
                    HStack {
                        ForEach(todoList.todoAct ?? []) { item in
                            ToDoScrollView(
                                schedType: item.scheduleType ?? "",
                                date: item.date ?? "",
                                desc: item.decription ?? ""
                            )
                        }
                    }
                    .padding(.horizontal)
                    .padding(.vertical, 8)
                }.scrollIndicators(.hidden)
                    .background(Color("ChatDateLineColor"))
                 

            }
        }
    }

    private var todoHeader: some View {
        HStack {
            Text("TODO LIST -")
                .font(Font.custom("Rubik-SemiBold", size: 14))

            Text("\(todoList.todoAct?.count ?? 0)")
            Text((todoList.todoAct?.count ?? 0) <= 1 ? " Activity" : " Activities")

            Spacer()
        }
        .foregroundColor(Color("title1"))
        .font(Font.custom("Rubik-Regular", size: 14))
        .padding(.horizontal)
        .padding(.vertical, 6)
    }

    private var recentVitalsSection: some View {
        VStack(alignment: .leading) {
            Text("RECENT VITALS")
                .foregroundColor(Color("title1"))
                .font(Font.custom("Rubik-SemiBold", size: 14))
                .padding(.vertical, 6)
                .frame(maxWidth: .infinity, alignment: .leading)
            
            if chartDays.vitalsGraph7.isEmpty {
                Text("No Readings !").foregroundColor(.red).padding(.horizontal)
            } else if chartDays.loading {
                ProgressView()
                Text("Loading…")
            } else {
                ScrollView(.horizontal) {
                    HStack(spacing: 0) {
                        ForEach(chartDays.vitalsGraph7, id: \.vitalName) { item in
                            RecentVitalsScrollView(item: item)
                                .frame(width: 280, height: 700)
                        }
                    }
                }
            }
        }  .padding(.horizontal,15)
    }

    private var isRPMProgram: Bool {
        UserDefaults.standard.string(forKey: "pgmTypeString") == "RPM"
    }

    // MARK: - Profile Info Section
    private var profileInfoSection: some View {
        HStack {
            Image(systemName: "person")
                .clipShape(Circle())
                .shadow(radius: 30)
                .frame(width: 40, height: 32)
                .padding(.horizontal, 10)
                .overlay(Circle().stroke(Color.red, lineWidth: 5))
            
            VStack(alignment: .leading) {
                Text("Hi, \(homeViewModel.accounts?.name ?? "")")
                    .font(Font.custom("Rubik-Regular", size: 24))
                    .tracking(0.3)
                    .foregroundColor(Color("darkGreen"))
                
                Text(homeViewModel.accounts?.userName ?? "")
                    .foregroundColor(.black)
                    .font(Font.custom("Rubik-SemiBold", size: 18))
            }
            
            Spacer()
        }
      
    }
}


struct LoadingCircleView: View {
    @ObservedObject var homeViewModel: RPMHomeViewModel

    var body: some View {
        ZStack {
            Circle()
                .stroke(Color(.systemGray5), lineWidth: 14)
                .frame(width: 100, height: 100)
            Circle()
                .trim(from: 0, to: 0.2)
                .stroke(Color("darkGreen"), lineWidth: 7)
                .frame(width: 100, height: 100)
                .rotationEffect(Angle(degrees: homeViewModel.loading ? 0 : 360))
                .animation(Animation.linear(duration: 1).repeatForever(autoreverses: false), value: homeViewModel.loading)
        }
    }
}

struct UnreadCountView: View {
    @EnvironmentObject var conversationManager: ConversationManager

    var body: some View {
        Text("Unread count: \(conversationManager.unreadCount)")
            .padding()
            .foregroundColor(.blue)
    }
}


struct CustomCorners : Shape{
    var corners: UIRectCorner
    var size: CGFloat
    func path (in rect: CGRect) -> Path{
        let path = UIBezierPath(roundedRect: rect,byRoundingCorners:  corners,
                                cornerRadii: CGSize(width: size, height: size))
        return Path(path.cgPath)
        
    }
}

struct TopButton: View {
    var text: String
    var colorf: Color
    var colorb: Color
    var flagval: Int
    var phoneNumber = "+16232676578"
    @State var isChatActive = false
    @EnvironmentObject var appModel: AppModel
    @EnvironmentObject var navigationHelper: NavigationHelper
    @EnvironmentObject var conversationManager: ConversationManager

    var body: some View {
        Button(action: {
            print("appModel3", appModel)
            
            if flagval == 2 {
                let phone = "tel://"
                let phoneNumberformatted = phone + phoneNumber
                guard let url = URL(string: phoneNumberformatted) else { return }
                
                if UIApplication.shared.canOpenURL(url) {
                    UIApplication.shared.open(url)
                } else {
                    print("Can't open url on this device")
                }
            } else {
                // Navigate to ConversationsList by appending to the navigationPath
                navigationHelper.path.append(.conversationsList)
            }
        }) {
            Image(text)
                .renderingMode(.template)
                .foregroundColor(colorf)
                .padding(30)
                .frame(maxWidth: 123, maxHeight: 34)
                .badge(10)
                .background(colorb)
                .cornerRadius(17)
        }
    }
}


extension View {
    func cornerRadius(_ radius: CGFloat, corners: UIRectCorner) -> some View {
        clipShape( RoundedCorner(radius: radius, corners: corners) )
    }
}


struct RoundedCorner: Shape {
    
    var radius: CGFloat = .infinity
    var corners: UIRectCorner = .allCorners
    
    func path(in rect: CGRect) -> Path {
        let path = UIBezierPath(roundedRect: rect, byRoundingCorners: corners, cornerRadii: CGSize(width: radius, height: radius))
        return Path(path.cgPath)
    }
}
struct HiddenNavigationBar: ViewModifier {
    func body(content: Content) -> some View {
        content
            .navigationBarTitle("", displayMode: .inline)
            .navigationBarHidden(true)
    }
}
extension View {
    func hiddenNavigationBarStyle() -> some View {
        modifier( HiddenNavigationBar() )
    }
}
struct NotificationNumLabel : View {
    @Binding var number : Int
    var body: some View {
        ZStack {
            Capsule().fill(Color.red).frame(width: 30 * CGFloat(numOfDigits()), height: 45, alignment: .topTrailing).position(CGPoint(x: 150, y: 0))
            Text("\(number)")
                .foregroundColor(Color.white)
                .font(Font.system(size: 35).bold()).position(CGPoint(x: 150, y: 0))
        }
    }
    func numOfDigits() -> Float {
        let numOfDigits = Float(String(number).count)
        return numOfDigits == 1 ? 1.5 : numOfDigits
    }
}


func utcToLocal(dateStr: String) -> String? {
    let dateFormatter = DateFormatter()
    dateFormatter.dateFormat = "H:mm:ss"
    dateFormatter.timeZone = TimeZone(abbreviation: "UTC")
    
    if let date = dateFormatter.date(from: dateStr) {
        dateFormatter.timeZone = TimeZone.current
        dateFormatter.dateFormat = "HH:mm:ss"
        return dateFormatter.string(from: date)
    }
    return nil
}

func findDateDiff(time1Str: String, time2Str: String) -> String {
    print("hkhjkjhkhk")
    let timeformatter = DateFormatter()
    timeformatter.dateFormat = "HH:mm:ss"
    
    guard let time1 = timeformatter.date(from: time1Str),
          let time2 = timeformatter.date(from: time2Str) else { return "" }
    print("time1")
    print(time1)
    print("time2")
    print(time2)
    
    //You can directly use from here if you have two dates
    let interval = time2.timeIntervalSince(time1)
    let hour = interval / 3600;
    let minute = interval.truncatingRemainder(dividingBy: 3600) / 60
    let intervalInt = Int(interval)
    print("intervalInt")
    print(intervalInt)
    print("difff")
    print(" \(Int(hour)) Hours \(Int(minute)) Minutes")
    
  let newtimeVal =   ((" \(Int(hour)) Hours \(Int(minute)) Minutes").substring(with: 1..<2) == "-")
    ?
    
    
    (" \(Int(hour)) Hours \(Int(minute)) Minutes") + " ago"
    :
    
    
    "In " + (" \(Int(hour)) Hours \(Int(minute)) Minutes")
    
    
    
    print(" \(intervalInt < 0 ? "-" : "+")\(Int(hour)) Hours \(Int(minute)) Minutes")
    
    
    let timeDiffVal = newtimeVal.replacingOccurrences(of: "-", with: "", options: NSString.CompareOptions.literal, range: nil)
     print("newtimeVal new")
     print(newtimeVal)
    
    
    return timeDiffVal
    
}
