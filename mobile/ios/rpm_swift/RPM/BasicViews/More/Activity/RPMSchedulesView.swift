//
//  RPMSchedulesView.swift
//  RPM
//
//  Created by Tesplabs on 26/07/1944 Saka.
//



import SwiftUI

struct RPMSchedulesView: View {
    
    // NOTE : RETURN START DATE OF MONTH
    private var startDateOfMonth: String {
        let components = Calendar.current.dateComponents([.year, .month], from: currentMonthDate)
        
        let startOfMonth = Calendar.current.date(from: components)!
        return format(date: startOfMonth)
    }
    // NOTE : RETURN END DATE OF MONTH
    private var endDateOfMonth: String {
        var components = Calendar.current.dateComponents([.year, .month], from: currentMonthDate)
        components.month = (components.month ?? 0) + 1
        components.hour = (components.hour ?? 0) - 1
        let endOfMonth = Calendar.current.date(from: components)!
        return format(date: endOfMonth)
    }
    
    // NOTE : RETURN WIDTH OF SCREEN

    @State private var currentMonthDate = Date()
    @State private var currentMonthDateFix = Date()
    @ObservedObject  var scheduleList = RPMSchedulesViewModel()
    
    var body: some View {
        
        GeometryReader { geometry in
                let width = geometry.size.width
         
        ScrollView
        {
            VStack(alignment: .leading)
            {
              HStack {
                    Spacer()
                    
                    Button {
                        
                        scheduleList.loading = true
                        
                        currentMonthDate = Calendar.current.date(byAdding: .month, value: -1, to: currentMonthDate)!
                        print("currentMonthDate1")
                        print(currentMonthDate)
                        print("startDateOfMonth")
                        print(startDateOfMonth)
                        print("endDateOfMonth")
                        print(endDateOfMonth)
                        
                        scheduleList.scheduleDetails(
                            
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
                        scheduleList.loading = true
                        currentMonthDate = Calendar.current.date(byAdding: .month, value: 1, to: currentMonthDate)!
                        print("currentMonthDate2")
                        print(currentMonthDate)
                        
                        scheduleList.scheduleDetails(
                            
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
                
                
                Text("   Schedules")
                    .foregroundColor(.black)
                    .font(Font.custom("Rubik-SemiBold", size: 24))
                
                
                if scheduleList.schedDetls == []
                {
                    Text("NO DATA !").foregroundColor(.red)
                        .frame(
                            maxWidth: .infinity,
                            maxHeight: .infinity,
                            alignment: .center)
                }
              
                else
                
                {
                    if
                        scheduleList.loading {
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
                       
                        Group
                        {
                          
                            ForEach(
                                scheduleList.schedDetls ?? []
                                
                            ) { item in
                                
                                Text(
                                    dateSchedInfo(timeval:
                                                    
                                    item.scheduleDate)
                                    
                                ) .padding(.top,5)
                                    .foregroundColor(Color("title1"))
                                    .font(Font.custom("Rubik-Regular", size: 16))
                                
                                ForEach(
                                    item.schedueInfos
                                    
                                ) { items in
                                    
                                    VStack(alignment: .leading)
                                    {
                                        
                                        HStack
                                        {
                                            Text(
                                                
                                                items.scheduleType
                                                
                                            ) .padding(.top,5)
                                                .foregroundColor(Color("buttonColor"))
                                                .font(Font.custom("Rubik-Regular", size: 18))
                                            
                                                .padding(.top,2)
                                            
                                            
                                        }
                                        HStack
                                        {
                                            Spacer()
                                            
                                            Text(
                                                items.scheduleTime
                                                
                                                
                                            )  .font(Font.custom("Rubik-Regular", size: 16))
                                                .padding(.top,2)
                                        }
                                        
                                        Text(
                                            items.assignedByName
                                            
                                            
                                        )  .font(Font.custom("Rubik-Regular", size: 16))
                                            .padding(.bottom,2)
                                        
                                        
                                    }
                                    .foregroundColor(.black)
                                    .frame(width: 320, height: 80, alignment: .leading)
                                    
                                    .padding(.horizontal,10)
                                    .padding(.vertical,5)
                                    .background(
                                        RoundedRectangle(cornerRadius: 8, style: .continuous
                                                        )
                                        .stroke(Color("textFieldBG"), lineWidth: 2
                                               )
                                        .background(.white)
                                        .cornerRadius(8)
                                        
                                    )
                                  
                                }
                                
                            }
                          
                        }
                        .padding(.horizontal,15)
                    
                    }
                }
              
            }
            .frame(width: width-10)
            .padding(.bottom,33)
            .padding()
            .padding(.horizontal,10)
           
        }
    }
    }
    
   // NOTE : SCHEDULE DATE FORMATTING FUNCTION
    func dateSchedInfo(timeval: String) -> String
    {
        let dateFormatter = DateFormatter()
        dateFormatter.dateFormat = "yyyy-MM-dd'T'HH:mm:ss"
        dateFormatter.timeZone = NSTimeZone(name: "UTC") as TimeZone?
        let date = dateFormatter.date(from: timeval)
        
        // change to a readable time format and change to local time zone
        dateFormatter.dateFormat = "MMM d, yyyy"
        dateFormatter.timeZone = NSTimeZone.local
        let timeStamp = dateFormatter.string(from: date!)
      
        return timeStamp
      
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

struct RPMSchedulesView_Previews: PreviewProvider {
    static var previews: some View {
        RPMSchedulesView()
    }
}




