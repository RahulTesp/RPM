////
////  RPMVitalSchedulesView.swift
////  RPM
////
////  Created by Tesplabs on 26/07/1944 Saka.
////
//
//

//
//  RPMVitalSchedulesView.swift
//  RPM
//
//  Created by Tesplabs on 26/07/1944 Saka.
//
 

//

//  RPMVitalSchedulesView.swift

//  RPM

//
 
import SwiftUI
 
// MARK: - Main View

struct RPMVitalSchedulesView: View {
 
    @ObservedObject var pgmDetList = RPMProgramInfoViewModel()
 
    var body: some View {

        GeometryReader { geometry in

            if pgmDetList.loading {

                LoadinggView(width: geometry.size.width)

            } else {

                ScrollView {

                    VStack(alignment: .leading, spacing: 20) {

                        Text("Vital Schedules")

                            .foregroundColor(.black)

                            .font(Font.custom("Rubik-SemiBold", size: 24))
 
                        if pgmDetList.pgmInfo?.patientVitalDetails.patientVitalInfos.isEmpty ?? true {

                            Text("NO DATA!")

                                .foregroundColor(.red)

                                .frame(maxWidth: .infinity, alignment: .center)

                        } else {
                            
                            //                            if let patientSchedulesInfos = pgmDetList.pgmInfo?.patientVitalDetails.patientVitalInfos {
                            //
                            //                                ForEach(Array(patientSchedulesInfos.enumerated()), id: \.element.scheduleId) { index, item in
                            //
                            //                                    ScheduleRowView(schedule: item, index: index, width: geometry.size.width)
                            //
                            //                                }
                            //
                            //                            }
                            //
                            //                        }
                            if let patientSchedulesInfos = pgmDetList.pgmInfo?.patientVitalDetails.patientVitalInfos {
                                
                                ForEach(Array(patientSchedulesInfos.enumerated()), id: \.element.vitalId) { index, item in
                                    ScheduleRowView(schedule: item, index: index, width: geometry.size.width)
                                }
                            }
                        }
                        

                    }

                    .padding(.horizontal, 16)

                    .padding(.top, 10)

                    .frame(width: geometry.size.width - 32)

                }

            }

        }

    }

}
 
// MARK: - Loading View

struct LoadinggView: View {

    let width: CGFloat

    var body: some View {

        VStack {

            Spacer()

            ProgressView()

                .tint(Color("TextColorBlack"))

            Text("Loading…")

                .foregroundColor(Color("TextColorBlack"))

                .padding(.vertical, 15)

            Spacer()

        }

        .frame(width: width)

    }

}
 
// MARK: - Schedule Row View

struct ScheduleRowView: View {

    let schedule: PatientVitalInfo

    let index: Int

    let width: CGFloat
 
    var body: some View {

        VStack(alignment: .leading, spacing: 16) {

            Text("Schedule \(index + 1)")

                .font(Font.custom("Rubik-Regular", size: 15))

                .foregroundColor(Color("buttonColor"))
 
            VStack(alignment: .leading, spacing: 16) {

                VitalSectionView(vitalName: schedule.vitalName, contentWidth: width - 32)

                ScheduleSectionView(scheduleName: schedule.scheduleName,

                                    vitalScheduleName: schedule.vitalScheduleName,

                                    contentWidth: width - 32)

                IntervalsSectionView(morning: schedule.morning,

                                     afternoon: schedule.afternoon,

                                     evening: schedule.evening,

                                     night: schedule.night)

                NormalRangeSectionView(measures: schedule.vitalMeasureInfos, contentWidth: width - 32)

            }

            .frame(width: width - 32)

        }

        .frame(width: width - 32)

    }

}
 
// MARK: - Vital Section

struct VitalSectionView: View {

    let vitalName: String

    let contentWidth: CGFloat
 
    var body: some View {

        VStack(alignment: .leading, spacing: 4) {

            Text("Vital Monitoring")

                .foregroundColor(Color("title1"))

                .font(Font.custom("Rubik-Regular", size: 14))
 
            Text(vitalName)

                .font(Font.custom("Rubik-Regular", size: 16))

                .foregroundColor(.black)

                .padding(8)

                .frame(width: contentWidth, alignment: .leading)

                .background(

                    RoundedRectangle(cornerRadius: 8)

                        .fill(Color("textFieldBG"))

                        .overlay(RoundedRectangle(cornerRadius: 8).stroke(Color("textFieldBG"), lineWidth: 2))

                )

        }

    }

}
 
// MARK: - Schedule Section

struct ScheduleSectionView: View {

    let scheduleName: String

    let vitalScheduleName: String

    let contentWidth: CGFloat
 
    var body: some View {

        VStack(alignment: .leading, spacing: 8) {

            Text("Schedule")

                .foregroundColor(Color("title1"))

                .font(Font.custom("Rubik-Regular", size: 14))
 
            Text(scheduleName)

                .font(Font.custom("Rubik-Regular", size: 16))

                .foregroundColor(.black)

                .padding(8)

                .frame(width: contentWidth, alignment: .leading)

                .background(

                    RoundedRectangle(cornerRadius: 8)

                        .fill(Color("textFieldBG"))

                        .overlay(RoundedRectangle(cornerRadius: 8).stroke(Color("textFieldBG"), lineWidth: 2))

                )
 
            Text(vitalScheduleName)

                .font(Font.custom("Rubik-Regular", size: 16))

                .foregroundColor(.black)

                .padding(8)

                .frame(width: contentWidth, alignment: .leading)

                .background(

                    RoundedRectangle(cornerRadius: 8)

                        .fill(Color("textFieldBG"))

                        .overlay(RoundedRectangle(cornerRadius: 8).stroke(Color("textFieldBG"), lineWidth: 2))

                )

        }

    }

}
 
// MARK: - Intervals Section

struct IntervalsSectionView: View {

    let morning: Bool

    let afternoon: Bool

    let evening: Bool

    let night: Bool
 
    var body: some View {

        VStack(alignment: .leading, spacing: 8) {

            Text("Intervals")

                .foregroundColor(Color("title1"))

                .font(Font.custom("Rubik-Regular", size: 14))
 
            HStack {

                IntervalLabel(title: "Morning", active: morning)

                IntervalLabel(title: "Afternoon", active: afternoon)

                IntervalLabel(title: "Evening", active: evening)

                IntervalLabel(title: "Night", active: night)

            }

        }

    }

}
 
// MARK: - Interval Label

struct IntervalLabel: View {

    let title: String

    let active: Bool
 
    var body: some View {

        Text(title)

            .font(Font.custom("Rubik-Regular", size: 16))

            .foregroundColor(active ? .black : .gray)

            .frame(height: 40, alignment: .leading)

            .padding(.horizontal, 5)

            .padding(.vertical, 5)

            .background(

                RoundedRectangle(cornerRadius: 8)

                    .fill(Color("textFieldBG"))

                    .overlay(RoundedRectangle(cornerRadius: 8).stroke(Color("textFieldBG"), lineWidth: 2))

            )

    }

}
 
// MARK: - Normal Range Section

struct NormalRangeSectionView: View {

    let measures: [VitalMeasureInfo]

    let contentWidth: CGFloat
 
    var body: some View {

        VStack(alignment: .leading, spacing: 8) {

            Text("Normal Range")

                .foregroundColor(Color("title1"))

                .font(Font.custom("Rubik-Regular", size: 14))
 
            ForEach(measures) { measure in

                HStack {

                    Text("\(measure.measureName) (\(measure.unitName))")

                        .font(Font.custom("Rubik-Regular", size: 16))

                        .foregroundColor(.black)
 
                    Spacer()
 
                    Text("\(measure.normalMinimum) - \(measure.normalMaximum)")

                        .font(Font.custom("Rubik-Regular", size: 16))

                        .foregroundColor(Color("GreenLight"))

                }

                .padding(8)

                .frame(width: contentWidth, alignment: .leading)

                .background(

                    RoundedRectangle(cornerRadius: 8)

                        .fill(Color("textFieldBG"))

                        .overlay(RoundedRectangle(cornerRadius: 8).stroke(Color("textFieldBG"), lineWidth: 2))

                )

            }

        }

    }

}
 
// MARK: - Preview

struct RPMVitalSchedulesView_Previews: PreviewProvider {

    static var previews: some View {

        RPMVitalSchedulesView()

    }

}

 


//import SwiftUI
//
//struct RPMVitalSchedulesView: View {
// 
//    @ObservedObject  var pgmDetList = RPMProgramInfoViewModel()
//    
//    var body: some View {
//        GeometryReader { geometry in
//        
//        if  pgmDetList.loading {
//            VStack {
//                Spacer()
//                ProgressView()
//                    .tint(Color("TextColorBlack"))
//                Text("Loading…")
//                    .foregroundColor(Color("TextColorBlack"))
//                    .padding(.vertical, 15)
//                Spacer()
//            }
//            .frame(width: geometry.size.width)
//            
//
//            
//        } else
//        
//        {
//            
//            ScrollView
//            {
//                
//                VStack(alignment: .leading, spacing: 20) {
//                    Text("Vital Schedules")
//                        .foregroundColor(.black)
//                        .font(Font.custom("Rubik-SemiBold", size: 24))
//
//                    if pgmDetList.pgmInfo?.patientDevicesDetails.patientDeviceInfos.isEmpty ?? true {
//                        Text("NO DATA!")
//                            .foregroundColor(.red)
//                            .frame(maxWidth: .infinity, alignment: .center)
//                    } else {
//                        //if let patientSchedulesInfos = pgmDetList.pgmInfo?.patientVitalDetails.patientVitalInfos as? [PatientVitalDetailsPatientVitalInfo] {
//                        if let patientSchedulesInfos = pgmDetList.pgmInfo?.patientVitalDetails.patientVitalInfos  {
//                        ForEach(Array(patientSchedulesInfos.enumerated()), id: \.element.scheduleID) { index, item in
//                                VStack(alignment: .leading) {
//                                    Text("Schedule \(index + 1)")
//                                        .font(Font.custom("Rubik-Regular", size: 15))
//                                        .foregroundColor(Color("buttonColor"))
//
//                                
//                                    makeScheduleView(items: item, width: geometry.size.width)
//
//                                }
//                                .frame(width: geometry.size.width - 32)
//                            }
//                        }
//                    }
//                }
//                .padding(.horizontal, 16)
//                .padding(.top, 10)
//                .frame(width: geometry.size.width - 32)
//                
// 
//                 
//                
//            }
//
//            
//        }
//        
//    }
//        }
//        
//    }
//    
//    func makeScheduleView(items : PatientVitalInfo, width: CGFloat) -> some View
//    {
//        let contentWidth = width - 32
//        
//        return VStack(alignment: .leading, spacing: 16) {
//            
//            Group {
//            Text("Vital Monitoring")
//                .foregroundColor(Color("title1"))
//                .font(Font.custom("Rubik-Regular", size: 14))
//            
//            
//            Text(
//                
//                items.vitalName
//                
//            )
//            .font(Font.custom("Rubik-Regular", size: 16))
//            .foregroundColor(.black)
//        
//            .padding(8)
//            .frame(width: contentWidth, alignment: .leading)
//         
//            .background(
//                RoundedRectangle(cornerRadius: 8)
//                    .fill(Color("textFieldBG"))
//                    .overlay(
//                        RoundedRectangle(cornerRadius: 8)
//                            .stroke(Color("textFieldBG"), lineWidth: 2)
//                    )
//            )
//        }
//    
//   
//    Text("Schedule")
//        .foregroundColor(Color("title1"))
//        .font(Font.custom("Rubik-Regular", size: 14))
//    
//    
//    Text(
//        
//        items.scheduleName
//        
//    )
//    .font(Font.custom("Rubik-Regular", size: 16))
//    .foregroundColor(.black)
//    .padding(8)
//    .frame(width: contentWidth, alignment: .leading)
//
//    .background(
//        RoundedRectangle(cornerRadius: 8)
//            .fill(Color("textFieldBG"))
//            .overlay(
//                RoundedRectangle(cornerRadius: 8)
//                    .stroke(Color("textFieldBG"), lineWidth: 2)
//            )
//    )
//    
//    Text(
//        
//        items.vitalScheduleName
//        
//    )
//    .font(Font.custom("Rubik-Regular", size: 16))
//    .foregroundColor(.black)
//    .padding(8)
//    .frame(width: contentWidth, alignment: .leading)
//
//    .background(
//        RoundedRectangle(cornerRadius: 8)
//            .fill(Color("textFieldBG"))
//            .overlay(
//                RoundedRectangle(cornerRadius: 8)
//                    .stroke(Color("textFieldBG"), lineWidth: 2)
//            )
//    )
//    
//    Group
//    {
//        HStack
//        {
//            
//            
//            Text("Intervals")
//                .foregroundColor(Color("title1"))
//                .font(Font.custom("Rubik-Regular", size: 14))
//            
//        }
//    }
//    Group
//    {
//        HStack
//        {
//            Text(
//                
//                "Morning"
//                
//            )
//            .font(Font.custom("Rubik-Regular", size: 16))
//            .foregroundColor(   items.morning == true ?   .black : .gray)
//            .frame(height: 40, alignment: .leading)
//            
//            .padding(.horizontal,5)
//            .padding(.vertical,5)
//            .background(
//                RoundedRectangle(cornerRadius: 8, style: .continuous
//                                )
//                .stroke(Color("textFieldBG"), lineWidth: 2
//                       )
//                .background(Color("textFieldBG"))
//                .cornerRadius(8)
//                
//            )
//            
//            
//            Text(
//                
//                "Afternoon"
//                
//            )
//            .font(Font.custom("Rubik-Regular", size: 16))
//            .foregroundColor(   items.afternoon == true ?   .black : .gray)
//            .frame( height: 40, alignment: .leading)
//            
//            .padding(.horizontal,5)
//            .padding(.vertical,5)
//            .background(
//                RoundedRectangle(cornerRadius: 8, style: .continuous
//                                )
//                .stroke(Color("textFieldBG"), lineWidth: 2
//                       )
//                .background(Color("textFieldBG"))
//                .cornerRadius(8)
//                
//            )
//            
//            Text(
//                
//                "Evening"
//                
//            )
//            .font(Font.custom("Rubik-Regular", size: 16))
//            .foregroundColor(   items.evening == true ?   .black : .gray)
//            .frame(height: 40, alignment: .leading)
//            
//            .padding(.horizontal,5)
//            .padding(.vertical,5)
//            .background(
//                RoundedRectangle(cornerRadius: 8, style: .continuous
//                                )
//                .stroke(Color("textFieldBG"), lineWidth: 2
//                       )
//                .background(Color("textFieldBG"))
//                .cornerRadius(8)
//                
//            )
//            
//            Text(
//                
//                "Night"
//                
//            )
//            .font(Font.custom("Rubik-Regular", size: 16))
//            .foregroundColor(   items.night == true ?   .black : .gray)
//            .frame( height: 40, alignment: .leading)
//            
//            .padding(.horizontal,5)
//            .padding(.vertical,5)
//            .background(
//                RoundedRectangle(cornerRadius: 8, style: .continuous
//                                )
//                .stroke(Color("textFieldBG"), lineWidth: 2
//                       )
//                .background(Color("textFieldBG"))
//                .cornerRadius(8)
//                
//            )
//        
//        }
//    }
//    Text("Normal Range")
//        .foregroundColor(Color("title1"))
//        .font(Font.custom("Rubik-Regular", size: 14))
//    
//    
//    ForEach(
//        items.vitalMeasureInfos
//       
//    ) {  itemss in
//        
//        Group
//        {
//            
//            HStack
//            {
//                
//                
//                Text(
//                    "\(itemss.measureName) ( \(itemss.unitName) )"
//                    
//                    
//                )      .font(Font.custom("Rubik-Regular", size: 16))
//                    .foregroundColor(.black)
//                Spacer()
//                
//                Text(
//                    String(
//                        "\(itemss.normalMinimum ) - \( itemss.normalMaximum)")
//                    
//                )     .font(Font.custom("Rubik-Regular", size: 16))
//                    .foregroundColor(Color("GreenLight"))
//            }
//        
//            .padding(8)
//            .frame(width: contentWidth, alignment: .leading)
//       
//            .background(
//                RoundedRectangle(cornerRadius: 8)
//                    .fill(Color("textFieldBG"))
//                    .overlay(
//                        RoundedRectangle(cornerRadius: 8)
//                            .stroke(Color("textFieldBG"), lineWidth: 2)
//                    )
//            )
//        
//    }
//    }
//
//}
//        .frame(width: contentWidth)
//        .padding(.vertical, 5)
//
//}
//
//struct RPMVitalSchedulesView_Previews: PreviewProvider {
//    static var previews: some View {
//        RPMVitalSchedulesView()
//    }
//}
//
//
//
