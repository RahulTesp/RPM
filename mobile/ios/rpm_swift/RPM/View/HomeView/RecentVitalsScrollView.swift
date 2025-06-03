//
//  RecentVitalsScrollView.swift
//  RPM
//
//  Created by Tesplabs on 17/03/1944 Saka.
//

import SwiftUI
struct RecentVitalsScrollView: View {
    

    var item: RPMVitalsChartDaysDataModel
    
    var body: some View {
        GeometryReader { geometry in
                let widthFull = geometry.size.width
                let heightFull = geometry.size.height
            
        VStack
        {
            HStack {
                
                VStack(alignment : .leading)
                {
                    Text((item.latestvm?.vitalName ?? item.vitalName) ?? "No Vital")
                        .font(Font.custom("Rubik-Regular", size: 18))
                        .foregroundColor(Color("darkGreen"))
                        .padding(.vertical,1)
                    HStack
                    {
                        
                        Text(item.latestvm?.value ?? "")
                        
                            .foregroundColor(Color("darkGreen"))
                            .font(Font.custom("Rubik-Regular", size: 32))
                        
                        Text(item.latestvm?.unit ?? "")
                            .foregroundColor(Color("darkGreen"))
                            .font(Font.custom("Rubik-Regular", size: 16))
                        Spacer()
                    }
                    
                    Text( item.latestvm?.date != nil ?    TimeAgoUtils.calculateTimeAgo(localDateString: item.latestvm?.date  ?? "") : "")
                        .font(Font.custom("Rubik-Regular", size: 16))
                        .foregroundColor(.black)
                }
                .padding(.vertical,13)
                .padding(.horizontal,13)
                
            }
            .frame(width: 300)
            .background(Color("LGreen"))
            .cornerRadius(15)
          
            if(item.time != [] && item.values != [])
            {
                HStack
                {
                    Spacer()
                    Text("< 7 days")
                        .padding(3)
                        .font(Font.custom("Rubik-Regular", size: 10))
                        .foregroundColor(.white)
                        .frame( minWidth :  65)
                        .fixedSize(horizontal: true, vertical: false)
                        .background(Color("title1"))
                        .cornerRadius(17)
                }
                
            }
            
            
            if   (item.time == [] && item.values == [])  {
                HStack {
                   
                    Text("No Readings") .foregroundColor(Color.red)
                    
                }
                .frame(maxWidth: .infinity, minHeight: 200)
              
            } else {
                HStack {
                    chart7DaysView(item: item)
                }
                .frame(maxWidth: .infinity)
            }
        }
        
        .padding(.top,3)
        .background(.white)
        .padding(.bottom,3)
        .padding(.vertical,3)
        .cornerRadius(15)
    }
}
    
    
    func timeDiff(timeval: String) -> String
    {
        print("date")
        
        var localTimeZoneAbbreviation: String { return TimeZone.current.abbreviation() ?? "" }
        print("localTimeZoneAbbreviation")
    
        let now = Date.now
        // let now = (Date.now, format: .dateTime.day().month().year())
        print("now")
        print(now)
        
        var dateFormatter = DateFormatter()
        dateFormatter.dateFormat = "HH:mm:ss"   // HH for 24h clock
        
        let date = Date()
        let currentTimeString = dateFormatter.string(from: date)
        print("currentTimeString")
        print(currentTimeString)
        
        print("vital utc time")
        print( timeval )
        print("utcToLocal")
        print(utcToLocal(dateStr: timeval ) ?? "")
        var comingTime =  utcToLocal(dateStr: timeval ) ?? ""
        print("comingTime")
        print(comingTime)
        
        //  let dateAsString = "13:15"
        let dateAsStrings = currentTimeString
        // let dateFormatter = DateFormatter()
        dateFormatter.dateFormat = "HH:mm:ss"
        
        let datess = dateFormatter.date(from: dateAsStrings)
        // dateFormatter.dateFormat = "h:mm a"
        dateFormatter.dateFormat = "HH:mm:ss"
        let current12 = dateFormatter.string(from: datess!)
        print("12 hour formatted current12Date:",current12)
        
        //  let dateAsString = "13:15"
        let dateAsString = comingTime
        // let dateFormatter = DateFormatter()
        dateFormatter.dateFormat = "HH:mm:ss"
        
        let dates = dateFormatter.date(from: dateAsString)
        
        // dateFormatter.dateFormat = "h:mm a"
        dateFormatter.dateFormat = "HH:mm:ss"
        let utc12 = dateFormatter.string(from: dates!)
        print("12 hour formatted Date:",utc12)
        
        //TIME DIFF
        
        let dateDiff = findDateDiff(time1Str: String(current12), time2Str: String(utc12))
        print("dateDiff")
        print(dateDiff)
        return dateDiff
    }
}

struct VitalReadingsScrollView: View {
    var systolic : Int
    var diastolic : Int
    var pulse : Int
    var systolicstatus :  String
    var diastolicstatus :  String
    var pulsestatus :  String
    var status : String
    var time : String
    
    var body: some View {
        
        ZStack(alignment: .leading) {
            Capsule()
                .frame(width: 3, height: 105)
                .foregroundColor(ColourStatus.getLineColorForStatus(status))
            VStack(alignment : .leading){
                HStack
                {
                    Text(String(systolic))
                        .font(Font.custom("Rubik-Regular", size: 18))
                        .foregroundColor(ColourStatus.getColorForStatus(systolicstatus))
                    Text("/")
                        .foregroundColor(ColourStatus.getColorForStatus(diastolicstatus))
                        .font(Font.custom("Rubik-Regular", size: 18))
                    Text(String(diastolic))
                        .font(Font.custom("Rubik-Regular", size: 18))
                        .foregroundColor(ColourStatus.getColorForStatus(diastolicstatus))
                        .padding(.vertical,0.5)
                    Text("mmHg")
                        .foregroundColor(ColourStatus.getColorForStatus(diastolicstatus))
                        .font(Font.custom("Rubik-Regular", size: 18))
                    Spacer()
                    Text(String(convertUTCtoLOCAL(inputDate: time ?? "") ?? ""))
                        .font(Font.custom("Rubik-Regular", size: 16))
                        .foregroundColor(.black)
                        .padding(.vertical,0.5)
                }
                
                HStack
                {
                    Text(String(pulse))
                        .font(Font.custom("Rubik-Regular", size: 18))
                        .foregroundColor(ColourStatus.getColorForStatus(pulsestatus))
                        .padding(.vertical,0.5)
                    Text("bpm")
                        .foregroundColor(ColourStatus.getColorForStatus(pulsestatus))
                        .font(Font.custom("Rubik-Regular", size: 18))
                }
                
            } .padding(.leading,13)
                .padding(.vertical,10)
        }
            .frame(maxWidth: .infinity, minHeight: 80)
            .padding(.trailing,10)
            .background(.white)
    }
}

struct WeightReadingsScrollView: View {
    var wt : Float
    var rt : String
    var weightstatus : String
    
    var body: some View {
        
        ZStack(alignment: .leading) {
            Capsule()
                .frame(width: 3, height: 70)
                .foregroundColor(ColourStatus.getLineColorForStatus(weightstatus))
            
            VStack(alignment : .leading){
                
                HStack
                {
                    Text(String(wt))
                        .font(Font.custom("Rubik-Regular", size: 18))
                        .foregroundColor(ColourStatus.getColorForStatus(weightstatus))
                        .padding(.vertical,0.5)
                    Text("lbs")    .foregroundColor(ColourStatus.getColorForStatus(weightstatus))
                        .font(Font.custom("Rubik-Regular", size: 18))
                    
                    Spacer()
                    
                    Text(String(convertUTCtoLOCAL(inputDate: rt) ?? ""))
                        .font(Font.custom("Rubik-Regular", size: 16))
                        .foregroundColor(.black)
                        .padding(.vertical,0.5)
                    
                }
            }.padding(.leading,13)
                .padding(.vertical,10)
               
        } .frame(maxWidth: .infinity, minHeight: 70)
            .padding(.trailing,10)
         
            .background(.white)
    }
}

struct GlucoseReadingsScrollView: View {
    
    
    var bgmdl : Int
    var schedule : String
    var rt : String
    var gluStatus : String

    var body: some View {
        ZStack(alignment: .leading) {
            Capsule()
                .frame(width: 3, height: 70)
                .foregroundColor(ColourStatus.getLineColorForStatus(gluStatus))
            
            VStack(alignment : .leading){
                
                HStack
                {
                    Text(String(bgmdl))
                        .font(Font.custom("Rubik-Regular", size: 18))
                        .foregroundColor(ColourStatus.getColorForStatus(gluStatus))
                        .padding(.vertical,0.5)
                    Text("mgdl")    .foregroundColor(ColourStatus.getColorForStatus(gluStatus))
                        .font(Font.custom("Rubik-Regular", size: 18))
                    
                    Spacer()
                    
                    Text(String(convertUTCtoLOCAL(inputDate: rt) ?? ""))
                        .font(Font.custom("Rubik-Regular", size: 16))
                        .foregroundColor(.black)
                        .padding(.vertical,0.5)
                    
                }
                
                HStack
                {
                    Text(String(schedule))
                        .font(Font.custom("Rubik-Regular", size: 18))
                        .foregroundColor(ColourStatus.getColorForStatus(gluStatus))
                        .padding(.vertical,0.5)
                }
           
            }.padding(.leading,13)
                .padding(.vertical,10)
        
        }.frame(maxWidth: .infinity, minHeight: 70)
            .padding(.trailing,10)
         
            .background(.white)
    }
}

struct OxygenReadingsScrollView: View {
    
    var oxy : Int
    var pulse : Int
    var rt : String
    var pulseStatus : String
    var oxygenStatus : String
    var status : String
 
    var body: some View {
        ZStack(alignment: .leading) {
            Capsule()
                .frame(width: 3, height: 70)
                .foregroundColor(ColourStatus.getLineColorForStatus(status))
            
            VStack(alignment : .leading){
                HStack
                {
                    Text(String(oxy))
                        .font(Font.custom("Rubik-Regular", size: 18))
                        .foregroundColor(ColourStatus.getColorForStatus(oxygenStatus))
                        .padding(.vertical,0.5)
                    Text("%")    .foregroundColor(ColourStatus.getColorForStatus(oxygenStatus))
                        .font(Font.custom("Rubik-Regular", size: 18))
                    
                    Spacer()
                    
                    Text(String(convertUTCtoLOCAL(inputDate: rt) ?? ""))
                        .font(Font.custom("Rubik-Regular", size: 16))
                        .foregroundColor(.black)
                        .padding(.vertical,0.5)
                    
                }
                
                HStack
                {
                    Text(String(pulse))
                        .font(Font.custom("Rubik-Regular", size: 18))
                        .foregroundColor(ColourStatus.getColorForStatus(pulseStatus))
                        .padding(.vertical,0.5)
                    Text("bpm")      .foregroundColor(ColourStatus.getColorForStatus(pulseStatus))
                        .font(Font.custom("Rubik-Regular", size: 18))
                }
                
            }.padding(.leading,13)
                .padding(.vertical,10)
            
        }.frame(maxWidth: .infinity, minHeight: 70)
            .padding(.trailing,10)
         
            .background(.white)
    }
}
