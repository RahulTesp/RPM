//
//  RPMMedicationsAddView.swift
//  RPM
//
//  Created by Tesplabs on 26/07/1944 Saka.
//


import SwiftUI
import Combine

struct RPMMedicationsAddView: View {
    @Environment(\.presentationMode) var presentationMode
    @State  var count: Int = 1
    @State private var medname: String = ""
    @State private var comments: String = ""
    @State private var showingAlerts = false
    @State private var showingErrorAlerts = false
    @State var value1 = ""
    @State var value2 = ""
    var placeholder = "Select Interval"
    var dropDownList1 = ["Monthly","Weekly", "Daily", "Alternative"]
    var dropDownList2 = ["After Meal", "Before Meal"]
    @State private var selectedDate: Date? = nil
    @State private var isDatePickerVisible = false
    @State private var isDatePickerPresented = false
    @State var isMng :Bool = false
    @State var isNoon :Bool = false
    @State var isEve :Bool = false
    @State var isNight :Bool = false
    func toggleMng(){isMng = !isMng}
    func toggleNoon(){isNoon = !isNoon}
    func toggleEve(){isEve = !isEve}
    func toggleNight(){isNight = !isNight}
    @State private var keyboardHeight: CGFloat = 0
    @State private var isValidationEnabled = false
    @ObservedObject private var medicAddModel = RPMMedicationsAddViewModel()
    @State private var startDate: Date?
    @State private var hidenDate: Date = Date()
    @State private var showDate: Bool = false

    var body: some View {
        // Background
        ZStack{
            BackgroundView()
            
            ScrollView
            {
                
                VStack ( alignment: .leading){
                    
                    
                    Text(" Add Medication")
                        .frame(maxWidth: .infinity, alignment: .leading)
                        .font(.system(size: 25 , weight: .heavy))
                        .foregroundColor(Color("TextColorBlack"))
                    
                    Group
                    {
                        
                        Text("Medicine Name")
                            .foregroundColor(Color("title1"))
                            .font(Font.system(size: 15))
                            .padding(.top,10)
                        
                        TextField("", text: $medname,prompt: Text("Select Medicine Name").foregroundColor(Color("TextColorGray")))
                       
                            .textFieldStyle(MyTextFieldStyle()).cornerRadius(10)
                            .disableAutocorrection(true)
                        
                            .frame(width: 320)
                            .colorScheme(.light)
                        
                        // Error message for Medicine Name
                     
                                     if isValidationEnabled && medname.isEmpty {
                                         Text("Medicine Name is required")
                                             .foregroundColor(.red)
                                             .font(.system(size: 12))
                                     }
                        
                        HStack
                        {
                            
                            Text("Schedule")
                                .foregroundColor(Color("title1"))
                                .font(Font.system(size: 15))
                                .padding(.top,3)
                        }
                        
                        HStack
                        {
                            
                            Menu {
                                ForEach(dropDownList1, id: \.self){ client in
                                    Button(client) {
                                        self.value1 = client
                                        print("value1")
                                        print(value1)
                                        //dismissKeyboard()
                                     
                                    }
                                }
                            } label: {
                                VStack(spacing: 3){
                                    HStack{
                                        Text(value1.isEmpty ? placeholder : value1)
                                            .foregroundColor(value1.isEmpty ? .gray : .black)
                                      
                                        Image(systemName: "chevron.down")
                                            .foregroundColor(Color("title1"))
                                            .font(Font.system(size: 16, weight: .light))
                                    }
                                    
                                    
                                    .frame(width: 140, height: 40)
                                    
                                    .padding(.horizontal,10)
                                    .padding(.vertical,5)
                                    .background(
                                        RoundedRectangle(cornerRadius: 8, style: .continuous
                                                        )
                                        .stroke(Color("textFieldBG"), lineWidth: 2
                                               )
                                        .background(Color("textFieldBG"))
                                        .cornerRadius(8)
                                        
                                    )
                                   
                                    .padding(.horizontal)
                                    Rectangle()
                                        .fill(Color.orange)
                                 
                                }
                                .frame(width: 140, height: 40)
                            }
                            
                            Spacer()
                            
                            
                            Menu {
                                ForEach(dropDownList2, id: \.self){ client in
                                    Button(client) {
                                        self.value2 = client
                                    }
                                }
                            } label: {
                                VStack(spacing: 3){
                                    HStack{
                                        Text(value2.isEmpty ? placeholder : value2)
                                            .foregroundColor(value2.isEmpty ? .gray : .black)
                                        
                                        
                                        Image(systemName: "chevron.down")
                                            .foregroundColor(Color("title1"))
                                            .font(Font.system(size: 16, weight: .light))
                                    }
                                  
                                    .frame(width: 140, height: 40)
                                    
                                    .padding(.horizontal,10)
                                    .padding(.vertical,5)
                                    .background(
                                        RoundedRectangle(cornerRadius: 8, style: .continuous
                                                        )
                                        .stroke(Color("textFieldBG"), lineWidth: 2
                                               )
                                        .background(Color("textFieldBG"))
                                        .cornerRadius(8)
                                        
                                    )
                                   
                                    .padding(.horizontal)
                                    Rectangle()
                                        .fill(Color.orange)
                                 
                                }
                                .frame(width: 140, height: 40)
                            }
                            
                        }
                       
                        HStack
                        {
                            if isValidationEnabled && value1.isEmpty {
                                Text("Schedule is required")
                                    .foregroundColor(.red)
                                    .font(.system(size: 12))
                            }
                            Spacer()
                            if isValidationEnabled && value2.isEmpty {
                                Text("Schedule is required")
                                    .foregroundColor(.red)
                                    .font(.system(size: 12))
                            }
                        }
                    }.padding(.horizontal,10)
                    
                    HStack
                    {
                        Text("Intervals")
                            .foregroundColor(Color("title1"))
                            .font(Font.system(size: 15))
                        
                    } .padding(.horizontal,10)
                        .padding(.vertical,5)
                    HStack(spacing : 30)
                    {
                        
                        Button(action: toggleMng){
                            VStack{
                                Text("Morning") .font(Font.system(size: 12))
                                    .foregroundColor(Color("buttonColor"))
                                Image(systemName: isMng ? "checkmark.square": "square")
                                    .renderingMode(.template)
                                    .foregroundColor(Color("title1"))
                                
                            }
                        }
                        Button(action: toggleNoon){
                            VStack{
                                Text("Afternoon")  .font(Font.system(size: 12)) .foregroundColor(Color("buttonColor"))
                                Image(systemName: isNoon ? "checkmark.square": "square")
                                    .renderingMode(.template)
                                    .foregroundColor(Color("title1"))
                            }
                        }
                        Button(action: toggleEve){
                            VStack{
                                Text("Evening")
                                    .font(Font.system(size: 12))
                                    .foregroundColor(Color("buttonColor"))
                                Image(systemName: isEve ? "checkmark.square": "square")
                                    .renderingMode(.template)
                                    .foregroundColor(Color("title1"))
                            }
                        }
                        Button(action: toggleNight){
                            VStack{
                                Text("Night") .font(Font.system(size: 12)).foregroundColor(Color("buttonColor"))
                                Image(systemName: isNight ? "checkmark.square": "square")
                                    .renderingMode(.template)
                                    .foregroundColor(Color("title1"))
                            }
                        }
                        
                    }.padding(.horizontal,10)
                    
                        .padding(.vertical,5)
                    
                    if isValidationEnabled && (isMng == false && isNoon == false && isEve == false && isNight == false){
                        Text("Interval is required")
                            .foregroundColor(.red)
                            .font(.system(size: 12))
                    }
                    HStack
                    {
                        Text("Medication Duration")
                            .foregroundColor(Color("title1"))
                            .font(Font.system(size: 15))
                    }
                    .padding(.vertical,5)
                    .padding(.horizontal,10)
                    
                    HStack
                    {
                        ZStack {
                            HStack {
                              
                                if showDate {
                                    Button {
                                        showDate = false
                                        startDate = nil
                                    } label: {

                                    }
                                    DatePicker(
                                        "label",
                                        selection: $hidenDate,
                                        in: Date()...,
                                        displayedComponents: .date
                                    )
                                    .labelsHidden()
                                    .onChange(of: hidenDate) { newDate in
                                        startDate = newDate
                                    }
                                    
                                } else {
                                    Button {
                                        showDate = true
                                        startDate = hidenDate
                                    } label: {
                                        Text("Start Date")
                                            .multilineTextAlignment(.center)
                                            .foregroundColor(.gray)
                                    }
                                    .frame(width: 140, height: 34)
                                    .background(
                                        RoundedRectangle(cornerRadius: 8)
                                            .fill(Color("textFieldBG"))
                                    )
                                    .multilineTextAlignment(.trailing)
                                }
                            }
                        }
               
                        Spacer()
                        
                                                HStack
                                                {
                                                   Spacer()
                                                    
                                                Button(action : {
                                                    self.count += 1
                                                })
                                                {
                                                    Image(systemName: "plus")
                                                        .renderingMode(.template)
                                                        .foregroundColor(.black)
                                                }
                        
                                               Text("\(count)")
                                                        .foregroundColor(.gray)
                        
                        
                                                Button(action : {
                        
                        
                                    if(count <= 1) {
                                    return
                                    } else {
                                self.count -= 1
                                }
                        
                                                })
                                                {
                                                    Image(systemName: "minus")  .renderingMode(.template)
                                                        .foregroundColor(.black)
                                                }
                        Spacer()
                        if(value1 == "Monthly")
                                                    {
                            Text(
                                
                                "Month"
                                
                                
                                
                            )
                            .foregroundColor(.gray)
                        }
                                                    else if(value1 == "Weekly")
                                                    {
                                                        Text(
                                                            
                                                            "Week"
                                                            
                                                            
                                                            
                                                        )
                                                        .foregroundColor(.gray)
                                                    }
                                                    else if(value1 == "Daily")
                                                    {
                                                        Text(
                                                            
                                                            "Day"
                                                            
                                                            
                                                            
                                                        )
                                                        .foregroundColor(.gray)
                                                    }
                                                    else if(value1 == "Alternative")
                                                    {
                                                        Text(
                                                            
                                                            "Day"
                                                         
                                                            
                                                        )
                                                        .foregroundColor(.gray)
                                                    }
                                                }.frame(width: 140, height: 34)
                            .background(
                                RoundedRectangle(cornerRadius: 8)
                                    .fill(Color("textFieldBG"))
                            )
                                                //.background(.green)
                        
                        
                        
                        
                    } .padding(.horizontal,10)
                
                        .padding(.vertical,5)
                  
                    if isValidationEnabled && (startDate == nil){
                        Text("Star Date is required")
                            .foregroundColor(.red)
                            .font(.system(size: 12))
                    }
                    
                    Group
                    {
                        
                        Text("Comments")
                            .foregroundColor(Color("title1"))
                            .font(Font.system(size: 15))
                        
                        
                        TextField("Add Comments", text: $comments)
                        
                        
                            .colorScheme(.light)
                            .textFieldStyle(MyTextFieldStyle()).cornerRadius(10)
                            .disableAutocorrection(true)
                        
                            .frame(width: 320, height:40)
                        if isValidationEnabled && comments.isEmpty{
                            Text("Comments is required")
                                .foregroundColor(.red)
                                .font(.system(size: 12))
                        }
                        
                    }
                    
                    Button(action: {
                 
                        if validateForm() {
                              // All fields are filled, make the API call
                           
                            print(medname)
                            print(value1)
                            print(value2)
                            print(isMng == true ? 1 : 0)
                            print(isNoon == true ? 1 : 0)
                            print(isEve == true ? 1 : 0)
                            print(isNight == true ? 1 : 0)
                            
                            print(self.dateMedInfo(  timeval:  startDate ?? Date()))

                            print(comments)
                            
                            
                            
                            let endDate = calculateEndDate(startDate: startDate ?? Date(), schedvalue: value1, countvalue: count)
                            print("End Date:", endDate)
                            
                            
                    print(self.dateMedInfo(  timeval:  endDate))
                        
                            medicAddModel.addMed(medName: medname,
                                                 schedule1: "\(value1)",

                                                 schedule2: "\(value2)",


                                                 morning : isMng == true ? 1 : 0 ,
                                                 afternoon : isNoon == true ? 1 : 0 , evening : isEve == true ? 1 : 0 , night : isNight == true ? 1 : 0 ,

                                                 startDate: self.dateMedInfo(  timeval:  startDate ?? Date()), endDate: self.dateMedInfo(  timeval:  endDate) ,  comments : comments, completed: { success, _ in
                                print("showingAlerttruechng")
                                if success {
                                       // Handle the success case and go back to the previous view
                                       presentationMode.wrappedValue.dismiss()
                                   } else {
                                       // Handle the failure case if needed
                                   }
                                
                                print(success)

                                // Reset the validation flag
                                   isValidationEnabled = false

                            })
                      
                            self.medname = ""
                            self.value1 = ""
                            self.value2 = ""
                            
                            self.isMng = false
                            self.isNoon = false
                            self.isEve = false
                            self.isNight = false
                            self.startDate = Date()
                            self.comments = ""
                            count = 1;
                           
                          } else {
                              // Show an error message or highlight fields with errors
                              showingErrorAlerts = true
                          }
                
                    },
                           
                           label: {
                        Text("ADD")
                    }
                   
                    ).alert("Medicine Added Successfully!", isPresented: $showingAlerts) {
                        Button("OK", role: .cancel) { }
                    }
                    .alert("Please enter valid Data!", isPresented: $showingErrorAlerts) {
                        Button("OK", role: .cancel) { }
                    }
                    
                    .frame(width: 320, height: 55)
                    .background(Color("buttonColor"))
                    .foregroundColor(Color("ButtonTextColor"))
                    .cornerRadius(10)
                    .padding(.vertical,10)
            
                }
                
                .padding(.horizontal,10)
                    .padding(.vertical,5)
                    .padding()
                        .padding(.bottom, keyboardHeight)
                          .onReceive(Publishers.keyboardHeight) { self.keyboardHeight = $0 }
                
                    .onTapGesture {
                       
                        UIApplication.shared.sendAction(#selector(UIResponder.resignFirstResponder), to:nil, from:nil, for:nil)
                    }
                
            }.padding(.horizontal, 12)
            
                .padding(.vertical, 32)
            
                .navigationBarBackButtonHidden(true)
            
                .navigationBarTitleDisplayMode(.inline)
                .toolbar {
                    ToolbarItem(placement: .principal) {
                        Text("Medications")
                            .font(.system(size:18))
                            .accessibilityAddTraits(.isHeader)
                            .foregroundColor(Color("TextColorBlack"))
                    }
                }
        }
        .onTapGesture {
            
            
            isDatePickerVisible = false
        }
     
        .ignoresSafeArea(.keyboard, edges: .bottom)
        .navigationBarItems(leading:
                                HStack
                            {
            Button(action: {
                self.presentationMode.wrappedValue.dismiss()
            }) {
                
                Image("ArrowBack") .renderingMode(.template)
                    .foregroundColor(  Color("buttonColor"))
                
            }
            
        }
                            
        )
     
        .ignoresSafeArea(.keyboard, edges: .bottom)
    
    }
    
    //NOTE: FORM VALIDATION
    func validateForm() -> Bool {
        
        guard isValidationEnabled else {
              return true
          }
      
        var isValid = true

        if medname.isEmpty {
            isValid = false
            // You can show an error message for this field
        }
        if comments.isEmpty {
            isValid = false
            // You can show an error message for this field
        }
        if value1.isEmpty {
            isValid = false
            // You can show an error message for this field
        }
        
        if value2.isEmpty {
            isValid = false
            // You can show an error message for this field
        }
        if !isMng && !isNoon && !isEve && !isNight {
              isValid = false
              // You can show an error message indicating that at least one of these must be true
          }

        // Add similar checks for other fields

        return isValid
    }

    
    
   // NOTE : END DATE CALCULATION
    func calculateEndDate(startDate: Date, schedvalue: String, countvalue: Int) -> Date {
        print("countvalue", +countvalue)
      
        var calendar = Calendar.current
        
        if let utcTimeZone = TimeZone(identifier: "UTC") {
            calendar.timeZone = utcTimeZone
        } else {
            print("startDate1", startDate)
            return startDate // Default to the same start date in case of an error.
        }
        
        var components = DateComponents()
        print("schedvalue", schedvalue)
        
        if schedvalue == "Monthly" {
            if countvalue > 1 {
                components.month = countvalue - 1
            }
        } else if schedvalue == "Weekly" {
            if countvalue > 1 {
                       components.day = (countvalue - 1) * 7
                   }
        } else if schedvalue == "Daily" {
            if countvalue > 1 {
                      components.day = countvalue - 1
                  }
        } else if schedvalue == "Alternative" {
            
            if countvalue > 1 {
                      components.day = (countvalue - 1) * 2
                  }
           
        }
        
        // Calculate the end date by adding the components to the start date
        if let endDate = calendar.date(byAdding: components, to: startDate) {
            print("endDate1", endDate)
            return endDate
        }
        
        print("startDate2", startDate)
        // Default to returning the same start date if the calculation fails
        return startDate
    }



    
    // NOTE : MEDICATION DATE FORMATTER
    func dateMedInfo(timeval: Date) -> String
    {

        let dateFormatter = DateFormatter()
     
        // change to a readable time format and change to local time zone
        dateFormatter.dateFormat = "yyyy-MM-dd"
        //  dateFormatter.timeZone = NSTimeZone.local
        let timeStamp = dateFormatter.string(from: timeval)
        
        
        return timeStamp
        
    }
    
 
    private func formattedSelectedDate(_ date: Date) -> String {
           let formatter = DateFormatter()
           formatter.dateStyle = .medium
           return formatter.string(from: date)
       }
}

extension Publishers {
    // 1.
    static var keyboardHeight: AnyPublisher<CGFloat, Never> {
        // 2.
        let willShow = NotificationCenter.default.publisher(for: UIApplication.keyboardWillShowNotification)
            .map { $0.keyboardHeight }
        
        let willHide = NotificationCenter.default.publisher(for: UIApplication.keyboardWillHideNotification)
            .map { _ in CGFloat(0) }
        
        // 3.
        return MergeMany(willShow, willHide)
            .eraseToAnyPublisher()
    }
}

extension Notification {
    var keyboardHeight: CGFloat {
        return (userInfo?[UIResponder.keyboardFrameEndUserInfoKey] as? CGRect)?.height ?? 0
    }
}

struct RPMMedicationsAddView_Previews: PreviewProvider {
    static var previews: some View {
        RPMMedicationsAddView()
    }
}
