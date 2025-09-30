//
//  chart7DaysView.swift
//  RPM
//
//  Created by Tesplabs on 05/08/1945 Saka.
//

import SwiftUI


import Charts

struct chart7DaysView: View {
    
    
    var  widthFull = UIScreen.main.bounds.width
    var width: CGFloat {
        if UIDevice.current.userInterfaceIdiom == .phone {
            return UIScreen.main.bounds.width / 1.1
        } else {
            return UIScreen.main.bounds.width / 1.3
        }
    }
    
    
    var  heightFull = UIScreen.main.bounds.height
    var height: CGFloat {
        if UIDevice.current.userInterfaceIdiom == .phone {
            return UIScreen.main.bounds.height / 1.39
        } else {
            return UIScreen.main.bounds.height / 1.3
        }
    }
    
    @State private var highlightedIndex: Int?

    var item : RPMVitalsChartDaysDataModel
    
    var body: some View {
        
        VStack{
        
            if(  item.time == nil && item.values?[0].data == nil)
                
            {
          
                Text("No Readings").foregroundColor(.red)
            }
        
            else{
           
                let valSummary  = item.values
                let dayCount  = item.time
           
                if(item.vitalName == "Blood Pressure" && valSummary?.count == 3)
                {
                    let entries1 = getEntries(valueSummary: valSummary ?? [], entryIndex: 0)
                     let entries2 = getEntries(valueSummary: valSummary ?? [], entryIndex: 1)
                     let entries3 = getEntries(valueSummary: valSummary ?? [], entryIndex: 2)

                    MultiLineChartView3v(
                        
                        entries1: entries1,
                             entries2: entries2,
                             entries3: entries3,
                                    
                                    
                                       days:
                    
                            getDaysv7(patientSummary: dayCount ?? []), item: item,highlightedIndex: $highlightedIndex

)
 
    .padding(.top,8)
    .onTapGesture {
                      // Handle tap gesture here
   
                      print("Tapped on index: \(highlightedIndex ?? -1)")
                  }
                  
                    
                    // SYSTOLIC/DIASTOLIC/PULSE
                                  
                                  HStack
                                  {
                              HStack
                                  {
                                 Text(" ")
                                      
                                  Rectangle()
                                                 .fill(Color(#colorLiteral(red: 0.1960784346, green: 0.3411764801, blue: 0.1019607857, alpha: 1)))
                                                 .frame(width: 10, height: 10)
                                      Text("Systolic")
                                           
                                          .foregroundColor(.black)
                                          .font(.system(size: 10))
                                  }
                                  HStack
                                      {
                                   
                                      Rectangle()
                                                     .fill(Color(#colorLiteral(red: 0.5568627715, green: 0.3529411852, blue: 0.9686274529, alpha: 1)))
                                                     .frame(width: 10, height: 10)
                                          Text("Diastolic")
                                               
                                              .foregroundColor(.black)
                                              .font(.system(size: 10))
                                      }
                                  HStack
                                      {
                                 
                                      Rectangle()
                                                     .fill(Color(#colorLiteral(red: 0.5725490451, green: 0, blue: 0.2313725501, alpha: 1)))
                                                     .frame(width: 10, height: 10)
                                          Text("Pulse")
                                               
                                              .foregroundColor(.black)
                                              .font(.system(size: 10))
                                      }
                                      Spacer()
                                  }
                                  .padding(.bottom,23)
            
                }
                
         
                if(item.vitalName == "Blood Glucose" && item.values?.count == 1)
                {
                    
                    let entries1 = getEntries(valueSummary: valSummary ?? [], entryIndex: 0)
                
                    
                    
                    MultiLineChartView1v(
                 
                        entries1: entries1,
                   
                                       days:
                    
                            getDaysv7(patientSummary: dayCount ?? []), item: item,highlightedIndex: $highlightedIndex



)
 
    .padding(.top,8)
             
                    // GLUCOSE
                                  
                                  HStack
                                  {
                              HStack
                                  {
                                 
                                  Rectangle()
                                  
                                                 .fill(Color(#colorLiteral(red: 0.3098039329, green: 0.01568627544, blue: 0.1294117719, alpha: 1)))
                                                 .frame(width: 10, height: 10)
                                      Text(item.values?[0].label ?? "")
                                           
                                          .foregroundColor(.black)
                                          .font(.system(size: 10))
                                  }

                               
                                  }
                                  .padding(.bottom,23)
         
                }
                
                
                if(item.vitalName == "Blood Glucose" && item.values?.count == 2)
                {
                    let entries1 = getEntries(valueSummary: valSummary ?? [], entryIndex: 0)
                     let entries2 = getEntries(valueSummary: valSummary ?? [], entryIndex: 1)
                    MultiLineChartView2v(
                        
                 
                        entries1: entries1,
                        entries2: entries2,

                                       days:
                    
                            getDaysv7(patientSummary: dayCount ?? []), item: item,highlightedIndex: $highlightedIndex

)

    .padding(.top,8)
                    
                    
                    // GLUCOSE
                                  
                                  HStack
                                  {
                              HStack
                                  {
                                 
                                  Rectangle()
                                      
                                      
    
                                                 .fill(Color(#colorLiteral(red: 0.1764705926, green: 0.4980392158, blue: 0.7568627596, alpha: 1)))
                                                 .frame(width: 10, height: 10)
                                      Text(item.values?[0].label ?? "")
                                           
                                          .foregroundColor(.black)
                                          .font(.system(size: 10))
                                  }
                                  HStack
                                      {
                                   
                                      Rectangle()
                                   
                                                     .fill(Color(#colorLiteral(red: 0.9098039269, green: 0.4784313738, blue: 0.6431372762, alpha: 1)))
                                                     .frame(width: 10, height: 10)
                                          Text(item.values?[1].label ?? "")
                                               
                                              .foregroundColor(.black)
                                              .font(.system(size: 10))
                                      }
                               
                                  }
                                  .padding(.bottom,23)
                    
                    
                }
                
                if(item.vitalName == "Oxygen")
                {
                    let entries1 = getEntries(valueSummary: valSummary ?? [], entryIndex: 0)
                     let entries2 = getEntries(valueSummary: valSummary ?? [], entryIndex: 1)
                    MultiLineChartView2v(
                        
                        entries1: entries1,
                        entries2: entries2,
              
                                       days:
                    
                            getDaysv7(patientSummary: dayCount ?? []), item: item,highlightedIndex: $highlightedIndex

)
   
    .padding(.top,8)
                    
                    // OXYGEN
                                  
                                  HStack
                                  {
                              HStack
                                  {
                                 
                                  Rectangle()
                                          .fill(Color(#colorLiteral(red: 0.9098039269, green: 0.4784313738, blue: 0.6431372762, alpha: 1)))
                                                 .frame(width: 10, height: 10)
                                      Text("Oxygen")
                                           
                                          .foregroundColor(.black)
                                          .font(.system(size: 10))
                                  }
                                      HStack
                                          {
                                         
                                          Rectangle()
                                                  .fill(Color(#colorLiteral(red: 0.1764705926, green: 0.4980392158, blue: 0.7568627596, alpha: 1)))
                                                         .frame(width: 10, height: 10)
                                              Text("Pulse")
                                                   
                                                  .foregroundColor(.black)
                                                  .font(.system(size: 10))
                                          }
                               
                                  }
                                  .padding(.bottom,23)
                    
                    
                }
                
                
                if(item.vitalName == "Weight")
                {
                    let entries1 = getEntries(valueSummary: valSummary ?? [], entryIndex: 0)
                   
                    MultiLineChartView1v(
               
                        entries1 :  entries1,
                   
                                    
                                       days:
                    
                            getDaysv7(patientSummary: dayCount ?? []), item: item,highlightedIndex: $highlightedIndex


)

    .padding(.top,8)
                    
                    // WEIGHT
                                  
                                  HStack
                                  {
                              HStack
                                  {
                                 
                                  Rectangle()
                                                 .fill(Color(#colorLiteral(red: 0.3098039329, green: 0.01568627544, blue: 0.1294117719, alpha: 1)))
                                                 .frame(width: 10, height: 10)
                                      Text("Weight")
                                           
                                          .foregroundColor(.black)
                                          .font(.system(size: 10))
                                  }
                                
                               
                                  }
                                  .padding(.bottom,23)
                    
                }
            
            }
          
        }
    
  
    }
    
}


func getEntries3(valueSummary:[Valuev] , count : Int)->[ChartDataEntry]{
    var entries:[ChartDataEntry]=[]

    for (index,data) in valueSummary[0].data.enumerated(){

        let data_new = ChartDataEntry(x:  Double(index) ,y: (Double (data ?? "" ) ?? 0.0 ))
                entries.append(data_new)
                print("entries3",entries)
             
            }
 
    return entries
   
}


func getEntries2(valueSummary:[Valuev] , count : Int)->[ChartDataEntry]{
    var entries:[ChartDataEntry]=[]

    for (index,data) in valueSummary[1].data.enumerated(){

        print("count")
        print(count)
        let data_new = ChartDataEntry(x:  Double(index) ,y: (Double (data ?? "" ) ?? 0.0 ))
                entries.append(data_new)
                print("entries2")
        
            }
  
    return entries

}


func getEntries(valueSummary: [Valuev], entryIndex: Int) -> [ChartDataEntry] {
    guard entryIndex < valueSummary.count else {
        return []
    }

    let dataPoints = valueSummary[entryIndex].data
    let label = valueSummary[entryIndex].label

    var entries: [ChartDataEntry] = []

    for (index, data) in dataPoints.enumerated() {
        if let dataStr = data, let value = Double(dataStr) {
            let entry = ChartDataEntry(x: Double(index), y: value, data: ["label": label])
            entries.append(entry)
        } else {
            // skip null → no entry added
            print("Skipping null at index \(index)")
        }
    }

    return entries
}



func getEntries1(valueSummary:[Valuev] , count : Int)->[ChartDataEntry]{
    var entries:[ChartDataEntry]=[]

    for (index,data) in valueSummary[2].data.enumerated(){

        let data_new = ChartDataEntry(x:  Double(index) ,y: (Double (data ?? "" ) ?? 0.0 ))
                entries.append(data_new)
                print("entries1")
      
            }
 
    return entries
   
}

func getDaysv7(patientSummary:[String])->[String]{
    
    var days:[String]=[]
    for patientdata in patientSummary{

        days.append(  convertChartDateFormat(inputDate: patientdata) ?? ""  )
            print("day")
            print(days)
        
    }
    return days
}


struct MultiLineChartView3v : UIViewRepresentable {
    
    var entries1 : [ChartDataEntry]
    var entries2 : [ChartDataEntry]
    var entries3 : [ChartDataEntry]
    var days: [String]
    var item: RPMVitalsChartDaysDataModel
  
    // Add a closure property to handle tap events
       var onTap: ((Int) -> Void)? = nil
       
       // Add a binding property to control the highlighted data point
       @Binding var highlightedIndex: Int?
    
    func makeCoordinator() -> Coordinator {
          Coordinator(self)
      }
    
    func makeUIView(context: Context) -> LineChartView {
        print("\ndays.count")
        print(days.count)
        let chart = LineChartView()
       
print("itemlabelnames",item.values?.map { $0.label } ?? [])
     
        // Set up the custom marker view
        let marker = CustomMarkerView(color: .blue, font: .systemFont(ofSize: 12), days: days)
              marker.chartView = chart
              chart.marker = marker
        
        return createChart(chart: chart)
    }
    
    func updateUIView(_ uiView: LineChartView, context: Context) {
        
        uiView.data = addData()
        
        // Highlight the data point based on the binding value
               if let highlightedIndex = highlightedIndex {
                   uiView.highlightValue(Highlight(x: Double(highlightedIndex), dataSetIndex: 0, stackIndex: -1))
               }
        
    }
    
    class Coordinator: NSObject {
           var parent: MultiLineChartView3v

           init(_ parent: MultiLineChartView3v) {
               self.parent = parent
           }

           @objc func handleTapGesture(_ gesture: UITapGestureRecognizer) {
               print("Tapped on chart")
               let location = gesture.location(in: parent.createChart(chart: LineChartView()))
               if let chart = gesture.view as? LineChartView {
                   let xAxisValue = chart.getHighlightByTouchPoint(location)?.x ?? 0
                   
                   
                   
                   if xAxisValue.isFinite {
                       parent.highlightedIndex = Int(xAxisValue)
                       parent.onTap?(Int(xAxisValue))
                   }

               }
           }
       }
    
    func createChart(chart: LineChartView) -> LineChartView{
        print("createChart3")
        chart.chartDescription.enabled = false
        chart.xAxis.drawGridLinesEnabled = true
        chart.xAxis.drawLabelsEnabled = true
        chart.xAxis.drawAxisLineEnabled = false
        chart.xAxis.labelPosition = .bottom
        chart.xAxis.labelRotationAngle = -80
        chart.rightAxis.enabled = false
        chart.leftAxis.enabled = true
        chart.drawBordersEnabled = false
        chart.legend.form = .none
        chart.xAxis.valueFormatter = CustomChartFormatterv(days: days)
        chart.xAxis.granularityEnabled = true
        chart.xAxis.setLabelCount(days.count, force: false)
        chart.xAxis.labelTextColor = UIColor.black
        chart.xAxis.granularity = 1.0
        
        let yAxisFormatter = NumberFormatter()
        yAxisFormatter.minimumFractionDigits = 1
      
        chart.leftAxis.valueFormatter = CustomNumberFormatter()
        chart.leftAxis.granularityEnabled = true
        chart.leftAxis.granularity = 0.1  // Set to an appropriate granularity

        print("createChart3 adddatacall")
        
        chart.data = addData()
        chart.scaleXEnabled = false
        chart.pinchZoomEnabled = false
        chart.doubleTapToZoomEnabled = false
        chart.dragEnabled = false // If you want to lock the chart entirely

        return chart
    }
    
    func addData() -> LineChartData{
        
        print("createChart3 adddatacalled")
        
        let data = LineChartData(dataSets: [
            
            //Schedule Trips Line
            generateLineChartDataSet(dataSetEntries: entries1, color: UIColor(Color(#colorLiteral(red: 0.1960784346, green: 0.3411764801, blue: 0.1019607857, alpha: 1)))
                                  
                                    ),
            
               generateLineChartDataSet(dataSetEntries: entries2, color: UIColor(Color(#colorLiteral(red: 0.5568627715, green: 0.3529411852, blue: 0.9686274529, alpha: 1)))
        
               ),
            generateLineChartDataSet(dataSetEntries: entries3, color: UIColor(Color(#colorLiteral(red: 0.5725490451, green: 0, blue: 0.2313725501, alpha: 1)))
   
            ),
         
        ])
        return data
    }
    
    func generateLineChartDataSet(dataSetEntries: [ChartDataEntry], color: UIColor
                                  //, fillColor: UIColor
    ) -> LineChartDataSet{
        
        print("createChart3 generateLineChartDataSet")
        
        let dataSet =
        
        LineChartDataSet(entries: dataSetEntries, label: "")
        
        print("createChart3 LineChartDataSet")
        dataSet.colors = [color]
        dataSet.mode = .cubicBezier
        dataSet.circleRadius = 5
        dataSet.circleHoleColor = color
//        dataSet.circleHoleColor = UIColor(Color(#colorLiteral(red: 0.003921568627, green: 0.231372549, blue: 0.431372549, alpha: 1)))
//     
//        dataSet.setCircleColor(UIColor.clear)
        dataSet.lineWidth = 2
        dataSet.valueTextColor = color
        dataSet.lineDashLengths = [3]
        dataSet.valueFont = UIFont(name: "Avenir", size: 12)!
        print("createChart3 dataSet")
        return dataSet
    }
    
}



struct MultiLineChartView2v : UIViewRepresentable {
    
    var entries1 : [ChartDataEntry]
    var entries2 : [ChartDataEntry]
    var days: [String]
    var item: RPMVitalsChartDaysDataModel
  
    
    // Add a closure property to handle tap events
       var onTap: ((Int) -> Void)? = nil
       
       // Add a binding property to control the highlighted data point
       @Binding var highlightedIndex: Int?
    
    
    func makeCoordinator() -> Coordinator {
          Coordinator(self)
      }
    
    func makeUIView(context: Context) -> LineChartView {
        print("\ndays.count")
        print(days.count)
        let chart = LineChartView()
        
   
      
print("itemlabelnames",item.values?.map { $0.label } ?? [])
     
    
        // Set up the custom marker view
        let marker = CustomMarkerView(color: .blue, font: .systemFont(ofSize: 12), days: days)
              marker.chartView = chart
              chart.marker = marker
      
        return createChart(chart: chart)
    }
    
    func updateUIView(_ uiView: LineChartView, context: Context) {
     
        uiView.data = addData()
        
        // Highlight the data point based on the binding value
               if let highlightedIndex = highlightedIndex {
                   uiView.highlightValue(Highlight(x: Double(highlightedIndex), dataSetIndex: 0, stackIndex: -1))
               }
     
    }
    
    class Coordinator: NSObject {
           var parent: MultiLineChartView2v

           init(_ parent: MultiLineChartView2v) {
               self.parent = parent
           }

           @objc func handleTapGesture(_ gesture: UITapGestureRecognizer) {
               print("Tapped on chart")
               let location = gesture.location(in: parent.createChart(chart: LineChartView()))
               if let chart = gesture.view as? LineChartView {
                   let xAxisValue = chart.getHighlightByTouchPoint(location)?.x ?? 0
                   // Update the highlighted index
                   parent.highlightedIndex = Int(xAxisValue)
                   // Call onTap closure with the tapped index
                   parent.onTap?(Int(xAxisValue))
               }
           }
       }
    
    
    func createChart(chart: LineChartView) -> LineChartView{
        chart.chartDescription.enabled = false
        chart.xAxis.drawGridLinesEnabled = true
        chart.xAxis.drawLabelsEnabled = true
        chart.xAxis.drawAxisLineEnabled = false
        chart.xAxis.labelPosition = .bottom
        chart.xAxis.labelRotationAngle = -80
        chart.rightAxis.enabled = false
        chart.leftAxis.enabled = true
        chart.drawBordersEnabled = false
       chart.legend.form = .none
 
        chart.xAxis.granularityEnabled = true
      
        chart.xAxis.valueFormatter = CustomChartFormatterv(days: days)
       
        chart.xAxis.setLabelCount(days.count, force: false)
     
        chart.xAxis.labelTextColor = UIColor.black
    
        chart.xAxis.granularity = 1.0
    
        
        chart.data = addData()
        // Set the maximum number of initially visible data points
           let initialVisibleDataPoints = 15
           chart.setVisibleXRangeMaximum(Double(initialVisibleDataPoints))

           // Enable horizontal scrolling (disable zooming)
           chart.scaleXEnabled = true

        return chart
    }
    
    func addData() -> LineChartData{
        let data = LineChartData(dataSets: [
            
            //Schedule Trips Line
            generateLineChartDataSet(dataSetEntries: entries1, color: UIColor(Color(#colorLiteral(red: 0.1764705926, green: 0.4980392158, blue: 0.7568627596, alpha: 1)))
                                   
                                    ),
            
               generateLineChartDataSet(dataSetEntries: entries2, color: UIColor(Color(#colorLiteral(red: 0.9098039269, green: 0.4784313738, blue: 0.6431372762, alpha: 1)))
          
               ),
       
         
        ])
        return data
    }
    
    func generateLineChartDataSet(dataSetEntries: [ChartDataEntry], color: UIColor
                                  //, fillColor: UIColor
    ) -> LineChartDataSet{
        
        
        
        let dataSet =
        
        LineChartDataSet(entries: dataSetEntries, label: "")
        
        
        dataSet.colors = [color]
        dataSet.mode = .cubicBezier
        dataSet.circleRadius = 5
        
        dataSet.circleHoleColor = color
//        dataSet.circleHoleColor = UIColor(Color(#colorLiteral(red: 0.003921568627, green: 0.231372549, blue: 0.431372549, alpha: 1)))
//   
//        dataSet.setCircleColor(UIColor.clear)
        dataSet.lineWidth = 2
        dataSet.valueTextColor = color
        dataSet.lineDashLengths = [3]
        dataSet.valueFont = UIFont(name: "Avenir", size: 12)!
        return dataSet
    }
    
}



struct MultiLineChartView1v : UIViewRepresentable {
    
    var entries1 : [ChartDataEntry]
    
   
    var days: [String]
    
    
    var item: RPMVitalsChartDaysDataModel
  
    
    
    // Add a closure property to handle tap events
       var onTap: ((Int) -> Void)? = nil
       
       // Add a binding property to control the highlighted data point
       @Binding var highlightedIndex: Int?
    
    
    func makeCoordinator() -> Coordinator {
          Coordinator(self)
      }
    
    
    func makeUIView(context: Context) -> LineChartView {
        print("\ndays.count")
        print(days.count)
        let chart = LineChartView()
   
      
print("itemlabelnames",item.values?.map { $0.label } ?? [])
     
        
        // Set up the custom marker view
        let marker = CustomMarkerView(color: .blue, font: .systemFont(ofSize: 12), days: days)
              marker.chartView = chart
              chart.marker = marker
        return createChart(chart: chart)
    }
    
    func updateUIView(_ uiView: LineChartView, context: Context) {
        print("updateUIView on chart")
     
        uiView.data = addData()
        
        // Highlight the data point based on the binding value
               if let highlightedIndex = highlightedIndex {
                   uiView.highlightValue(Highlight(x: Double(highlightedIndex), dataSetIndex: 0, stackIndex: -1))
               }
    }
    
    class Coordinator: NSObject {
           var parent: MultiLineChartView1v

           init(_ parent: MultiLineChartView1v) {
               self.parent = parent
           }

           @objc func handleTapGesture(_ gesture: UITapGestureRecognizer) {
               print("Tapped on chart")
               let location = gesture.location(in: parent.createChart(chart: LineChartView()))
               if let chart = gesture.view as? LineChartView {
                   let xAxisValue = chart.getHighlightByTouchPoint(location)?.x ?? 0
                   // Update the highlighted index
                   parent.highlightedIndex = Int(xAxisValue)
                   // Call onTap closure with the tapped index
                   parent.onTap?(Int(xAxisValue))
               }
           }
       }
    
    func createChart(chart: LineChartView) -> LineChartView{
        print("createChart")
             
        chart.chartDescription.enabled = false
        chart.xAxis.drawGridLinesEnabled = true
        chart.xAxis.drawLabelsEnabled = true
        chart.xAxis.drawAxisLineEnabled = false
        chart.xAxis.labelPosition = .bottom
        chart.xAxis.labelRotationAngle = -80
        chart.rightAxis.enabled = false
        chart.leftAxis.enabled = true
        chart.drawBordersEnabled = false
       chart.legend.form = .none
        
        chart.xAxis.granularityEnabled = true
        chart.xAxis.granularity = 1.0
        chart.xAxis.valueFormatter = CustomChartFormatterv(days: days)
      
        chart.xAxis.setLabelCount(days.count, force: false)
 
        chart.xAxis.labelTextColor = UIColor.black
    
        
        chart.data = addData()
        // Set the maximum number of initially visible data points
           let initialVisibleDataPoints = 15
           chart.setVisibleXRangeMaximum(Double(initialVisibleDataPoints))

           // Enable horizontal scrolling (disable zooming)
           chart.scaleXEnabled = true

        return chart
    }
    
    func addData() -> LineChartData{
        let data = LineChartData(dataSets: [
            
            //Schedule Trips Line
            generateLineChartDataSet(dataSetEntries: entries1, color: UIColor(Color(#colorLiteral(red: 0.3098039329, green: 0.01568627544, blue: 0.1294117719, alpha: 1)))
                              
                                    ),
            
           
        ])
        return data
    }
    
    func generateLineChartDataSet(dataSetEntries: [ChartDataEntry], color: UIColor
                                  //, fillColor: UIColor
    ) -> LineChartDataSet{
        
        
        
        let dataSet =
        
        LineChartDataSet(entries: dataSetEntries, label: "")
        
        
        dataSet.colors = [color]
        dataSet.mode = .cubicBezier
        dataSet.circleRadius = 5
        dataSet.circleHoleColor = color
//        dataSet.circleHoleColor = UIColor(Color(#colorLiteral(red: 0.003921568627, green: 0.231372549, blue: 0.431372549, alpha: 1)))
//   
//        dataSet.setCircleColor(UIColor.clear)
        dataSet.lineWidth = 2
        dataSet.valueTextColor = color
        dataSet.lineDashLengths = [3]
        dataSet.valueFont = UIFont(name: "Avenir", size: 12)!
        return dataSet
    }
    
}

class CustomNumberFormatter: NSObject, AxisValueFormatter {
    func stringForValue(_ value: Double, axis: AxisBase?) -> String {
        return String(format: "%.1f", value)  // Format to one decimal place
    }
}


class CustomChartFormatterv: NSObject, AxisValueFormatter {
    var days: [String]
    
    init(days: [String]) {
        self.days = days
        
        print( "self.days" )
        print( self.days )
    }
    
    public func stringForValue(_ value: Double, axis: AxisBase?) -> String {
     
        print("String(value)]")
        print(String(value))
        print("days chartline view")
        print(days)
        print("days.count")
        print(days.count)
        print("String(days.count")
        print(String(days.count))
        
        print("int String(days.count")

        print("(value)")
        print(value)
        print("Int(value)")
        print(Int(value))
        print("String(value)")
        print(String(value))
        
        
        print("int days.count")
        print(Int(days.count))
    
        print("String( Double days.count)")
        print(String(Double(days.count)))
        print("String(  days.count)")
        print(String(days.count))
        print("String(Int Double days.count)")
        print(String(Int(Double(days.count))))
        
        print("days[Double(days.count)]")

        print("days[Int(value)]")

       print(days[0])
      
        return days[Int(value)]
    }
}


// highliter moved to left, now ok
class CustomMarkerView: MarkerView {
    private var label: UILabel
    private var days: [String]

    init(color: UIColor, font: UIFont, days: [String]) {
        self.label = UILabel()
        self.days = days

        // Adjust the initial height to ensure visibility
        super.init(frame: CGRect(x: 0, y: 0, width: 200, height: 200))

        label.textColor = UIColor.white
        label.font = font
        label.numberOfLines = 0
        addSubview(label)
        backgroundColor = UIColor.gray.withAlphaComponent(1.6) // Adjust the alpha value as needed

       // backgroundColor = UIColor.gray
        layer.cornerRadius = 5
        layer.borderColor = UIColor.white.cgColor
        layer.borderWidth = 1
    }

    required init?(coder aDecoder: NSCoder) {
        fatalError("init(coder:) has not been implemented")
    }

    override func refreshContent(entry: ChartDataEntry, highlight: Highlight) {
        let date = days[min(max(Int(entry.x), 0), days.count - 1)]

        guard let dataDict = entry.data as? [String: Any],
              let labelString = dataDict["label"] as? String else {
            return
        }

        label.text = " \(date)\n \(labelString): \(entry.y)"
        label.textAlignment = .left

        let entriesAtSameIndex = chartView?.data?.dataSets
            .flatMap { $0.entriesForXValue(entry.x) } ?? []

        // Check if there are entries at the same x-value
        if !entriesAtSameIndex.isEmpty {
            var labelText = " \(date)\n"

            // Iterate over all entries at the same x-value
            for otherEntry in entriesAtSameIndex {
                guard let otherDataDict = otherEntry.data as? [String: Any],
                      let otherLabel = otherDataDict["label"] as? String else {
                    continue
                }

                // Check if the y-values are the same
                if otherEntry.y == entry.y {
                    labelText += " \(otherLabel): \(entry.y)"
                }
            }

            label.text = labelText
            label.textAlignment = .left  // Set the text alignment back to left for the entire label
            label.lineBreakMode = .byWordWrapping
        }

        // Calculate the size of the label's text
        let labelSize = label.sizeThatFits(CGSize(width: CGFloat.greatestFiniteMagnitude, height: CGFloat.greatestFiniteMagnitude))

        // Adjust the size of the marker view based on the label's text size
        self.bounds.size.width = labelSize.width + 20 // Add some padding
        self.bounds.size.height = labelSize.height + 30 // Add some padding

        label.frame = CGRect(x: 10, y: 10, width: labelSize.width, height: labelSize.height)

        label.textColor = UIColor.white

        // Adjust the offset
  
        var offset = CGPoint(x: 0, y: -bounds.height - 12)

        offset.x = -(bounds.width / 2) - 10 // Adjusted by subtracting 10 to move it to the left
        offset.y = min(offset.y, -(bounds.height / 2))
        offset.y = max(offset.y, -(bounds.height / 2) - 12)

        self.offset = offset

    }
}
