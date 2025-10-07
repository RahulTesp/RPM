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

                        days: getDaysv7(patientSummary: dayCount ?? []),

                        item: item,

                        lineColors: ChartColors.bloodPressure, //  Colors from your struct

                        highlightedIndex: $highlightedIndex

                    )

                     
 
    .padding(.top,8)
    .onTapGesture {
                      // Handle tap gesture here
   
                      print("Tapped on index: \(highlightedIndex ?? -1)")
                  }
                  
                    
                    // SYSTOLIC/DIASTOLIC/PULSE
                    
                    HStack
                                        {
                                            // 1. SYSTOLIC: Dark Green (Matches entries1 color and desired order)
                                            HStack
                                            {
                                                Rectangle()

                                                    .fill(ChartColors.bloodPressure[0])
                                                    .frame(width: 10, height: 10)
                                                Text("Systolic") // CORRECT LABEL
                                                    .foregroundColor(.black)
                                                    .font(.system(size: 10))
                                            }
                                            
                                            // 2. DIASTOLIC: Purple (Matches entries2 color and desired order)
                                            HStack
                                            {
                                                Text(" ")
                                                Rectangle()

                                                    .fill(ChartColors.bloodPressure[1])
                                                    .frame(width: 10, height: 10)
                                                Text("Diastolic") // CORRECT LABEL
                                                    .foregroundColor(.black)
                                                    .font(.system(size: 10))
                                            }
                                            
                                            // 3. PULSE: Brown/Red (Matches entries3 color and desired order)
                                            HStack
                                            {
                                                Rectangle()

                                                    .fill(ChartColors.bloodPressure[2])
                                                    .frame(width: 10, height: 10)
                                                Text("Pulse") // CORRECT LABEL
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
                        days: getDaysv7(patientSummary: dayCount ?? []),
                        item: item,
                        lineColors: [ChartColors.bloodGlucose[0]],   //  Use your defined color for single-line charts
                        highlightedIndex: $highlightedIndex
                    )
                    
 
    .padding(.top,8)
             
                    // GLUCOSE
                                  
                                  HStack
                                  {
                              HStack
                                  {
                                 
                                  Rectangle()

                                          .fill(ChartColors.bloodGlucose[0])
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
                        entries1: getEntries(valueSummary: valSummary ?? [], entryIndex: 0),
                        entries2: getEntries(valueSummary: valSummary ?? [], entryIndex: 1),
                        days: getDaysv7(patientSummary: dayCount ?? []),
                        item: item,
                        lineColors:ChartColors.bloodGlucose,

                        highlightedIndex: $highlightedIndex
                    )

    .padding(.top,8)
                    
                    
                    // GLUCOSE
                                  
                                  HStack
                                  {
                              HStack
                                  {
                                 
                                  Rectangle()
                                      
                                      
                                                .fill(ChartColors.bloodGlucose[0])
                                                 .frame(width: 10, height: 10)
                                      Text(item.values?[0].label ?? "")
                                           
                                          .foregroundColor(.black)
                                          .font(.system(size: 10))
                                  }
                                  HStack
                                      {
                                   
                                      Rectangle()
                                   

                                              .fill(ChartColors.bloodGlucose[1])
                                                     .frame(width: 10, height: 10)
                                          Text(item.values?[1].label ?? "")
                                               
                                              .foregroundColor(.black)
                                              .font(.system(size: 10))
                                      }
                               
                                  }
                                  .padding(.bottom,23)
                    
                    
                }
                
                if(item.vitalName == "Oxygen" && item.values?.count == 1)
                {
                    let entries1 = getEntries(valueSummary: valSummary ?? [], entryIndex: 0)
                     let entries2 = getEntries(valueSummary: valSummary ?? [], entryIndex: 1)

                    MultiLineChartView1v(
                        entries1: getEntries(valueSummary: valSummary ?? [], entryIndex: 0),

                        days: getDaysv7(patientSummary: dayCount ?? []),
                        item: item,
                        lineColors:ChartColors.oxygen,

                        highlightedIndex: $highlightedIndex
                    )
   
    .padding(.top,8)
                    
                    // OXYGEN
                                  
                                  HStack
                                  {

                                      HStack
                                          {
                                         
                                          Rectangle()

                                                  .fill(ChartColors.oxygen[0])
                                                         .frame(width: 10, height: 10)
                                              Text("Oxygen")
                                                   
                                                  .foregroundColor(.black)
                                                  .font(.system(size: 10))
                                          }
                               
                                  }
                                  .padding(.bottom,23)
                    
                    
                }
                
                
                
                if(item.vitalName == "Oxygen" && item.values?.count == 2)
                {
                    let entries1 = getEntries(valueSummary: valSummary ?? [], entryIndex: 0)
                     let entries2 = getEntries(valueSummary: valSummary ?? [], entryIndex: 1)

                    MultiLineChartView2v(
                        entries1: getEntries(valueSummary: valSummary ?? [], entryIndex: 0),
                        entries2: getEntries(valueSummary: valSummary ?? [], entryIndex: 1),
                        days: getDaysv7(patientSummary: dayCount ?? []),
                        item: item,
                        lineColors:ChartColors.oxygen,

                        highlightedIndex: $highlightedIndex
                    )
   
    .padding(.top,8)
                    
                    // OXYGEN
                                  
                                  HStack
                                  {
                              HStack
                                  {
                                 
                                  Rectangle()

                                          .fill(ChartColors.oxygen[0])
                                         
                                                 .frame(width: 10, height: 10)
                                      Text("Oxygen")
                                   
                                           
                                          .foregroundColor(.black)
                                          .font(.system(size: 10))
                                  }
                                      HStack
                                          {
                                         
                                          Rectangle()

                                                  .fill(ChartColors.oxygen[1])
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
                        entries1: entries1,
                        days: getDaysv7(patientSummary: dayCount ?? []),
                        item: item,
                        lineColors: [ChartColors.weight],   //  Use your defined color for single-line charts
                        highlightedIndex: $highlightedIndex
                    )

    .padding(.top,8)
                    
                    // WEIGHT
                                  
                                  HStack
                                  {
                              HStack
                                  {
                                 
                                  Rectangle()

                                          .fill(ChartColors.weight)
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



struct MultiLineChartView3v: UIViewRepresentable {
    var entries1: [ChartDataEntry]
    var entries2: [ChartDataEntry]
    var entries3: [ChartDataEntry]
    var days: [String]
    var item: RPMVitalsChartDaysDataModel
    var lineColors: [Color]
    var onTap: ((Int) -> Void)? = nil
    @Binding var highlightedIndex: Int?
 
    func makeCoordinator() -> Coordinator {
        Coordinator(self)
    }
 
    func makeUIView(context: Context) -> LineChartView {
        let chart = LineChartView()
        chart.delegate = context.coordinator
 
        let marker = CustomMarkerView(color: .blue, font: .systemFont(ofSize: 12), days: days)
        marker.chartView = chart
        chart.marker = marker
 
        return createChart(chart: chart)
    }
 
    func updateUIView(_ uiView: LineChartView, context: Context) {
        uiView.data = addData()
 
        //  Highlight the tapped point on the correct dataset
        if let highlightedIndex = highlightedIndex {
            let datasetIndex = context.coordinator.tappedDataSetIndex ?? 0
            uiView.highlightValue(Highlight(
                x: Double(highlightedIndex),
                dataSetIndex: datasetIndex,
                stackIndex: -1
            ))
        }
    }
 
    class Coordinator: NSObject, ChartViewDelegate {
        var parent: MultiLineChartView3v
        var tappedDataSetIndex: Int? = nil
 
        init(_ parent: MultiLineChartView3v) {
            self.parent = parent
        }
 
        func chartValueSelected(_ chartView: ChartViewBase, entry: ChartDataEntry, highlight: Highlight) {
            parent.highlightedIndex = Int(highlight.x)
            tappedDataSetIndex = highlight.dataSetIndex
            parent.onTap?(highlight.dataSetIndex)
        }
 
        func chartValueNothingSelected(_ chartView: ChartViewBase) {
            parent.highlightedIndex = nil
            tappedDataSetIndex = nil
        }
    }
 
    // MARK: - Chart Setup
    func createChart(chart: LineChartView) -> LineChartView {
        chart.chartDescription.enabled = false
        chart.rightAxis.enabled = false
        chart.legend.form = .none
        chart.drawBordersEnabled = false
 
        chart.xAxis.labelPosition = .bottom
        chart.xAxis.labelRotationAngle = -80
        chart.xAxis.drawAxisLineEnabled = false
        chart.xAxis.drawGridLinesEnabled = true
        chart.xAxis.valueFormatter = CustomChartFormatterv(days: days)
        chart.xAxis.setLabelCount(days.count, force: false)
        chart.xAxis.labelTextColor = .black
        chart.xAxis.granularity = 1.0
 
        chart.leftAxis.enabled = true
        chart.leftAxis.granularityEnabled = true
        chart.leftAxis.granularity = 0.1
        chart.leftAxis.valueFormatter = CustomNumberFormatter()
 
        chart.scaleXEnabled = false
        chart.pinchZoomEnabled = false
        chart.doubleTapToZoomEnabled = false
        chart.dragEnabled = false
 
        chart.data = addData()
        return chart
    }
 
    // MARK: - Add Data
    func addData() -> LineChartData {
        LineChartData(dataSets: [
            generateLineChartDataSet(
                dataSetEntries: entries1,
                color: UIColor(lineColors[safe: 0] ?? .gray),
                label: item.values?[safe: 0]?.label ?? "Value 1"
            ),
            generateLineChartDataSet(
                dataSetEntries: entries2,
                color: UIColor(lineColors[safe: 1] ?? .gray),
                label: item.values?[safe: 1]?.label ?? "Value 2"
            ),
            generateLineChartDataSet(
                dataSetEntries: entries3,
                color: UIColor(lineColors[safe: 2] ?? .gray),
                label: item.values?[safe: 2]?.label ?? "Value 3"
            )
        ])
    }
 
    func generateLineChartDataSet(
        dataSetEntries: [ChartDataEntry],
        color: UIColor,
        label: String
    ) -> LineChartDataSet {
        let dataSet = LineChartDataSet(entries: dataSetEntries, label: "")
        dataSet.colors = [color]
        dataSet.mode = .cubicBezier
        dataSet.circleRadius = 5
        dataSet.circleHoleColor = color
        dataSet.setCircleColor(color)
        dataSet.lineWidth = 2
        dataSet.valueTextColor = color
        dataSet.lineDashLengths = [3]
        dataSet.valueFont = UIFont(name: "Avenir", size: 12)!
      
        return dataSet
    }
}



 
// MARK: - Safe Array Indexing
extension Array {
    subscript(safe index: Int) -> Element? {
        return indices.contains(index) ? self[index] : nil
    }
}




struct MultiLineChartView2v: UIViewRepresentable {

   var entries1: [ChartDataEntry]

   var entries2: [ChartDataEntry]

   var days: [String]

   var item: RPMVitalsChartDaysDataModel

   var lineColors: [Color]             //  Accepts external line colors

   var onTap: ((Int) -> Void)? = nil

   @Binding var highlightedIndex: Int?

   func makeCoordinator() -> Coordinator {

       Coordinator(self)

   }

   func makeUIView(context: Context) -> LineChartView {

       let chart = LineChartView()

       chart.delegate = context.coordinator

       //  Set up custom marker

       let marker = CustomMarkerView(color: .blue, font: .systemFont(ofSize: 12), days: days)

       marker.chartView = chart

       chart.marker = marker

       return createChart(chart: chart)

   }


    
    func updateUIView(_ uiView: LineChartView, context: Context) {
        uiView.data = addData()
        
        if let highlightedIndex = highlightedIndex {
            let datasetIndex = context.coordinator.tappedDataSetIndex ?? 0
            uiView.highlightValue(Highlight(x: Double(highlightedIndex),
                                            dataSetIndex: datasetIndex,
                                            stackIndex: -1))
        }
        

    }

   
        
    
    class Coordinator: NSObject, ChartViewDelegate {
        var parent: MultiLineChartView2v
        var tappedDataSetIndex: Int? = nil  // store last tapped dataset
     
        init(_ parent: MultiLineChartView2v) {
            self.parent = parent
        }
     
        func chartValueSelected(_ chartView: ChartViewBase, entry: ChartDataEntry, highlight: Highlight) {
            parent.highlightedIndex = Int(highlight.x)
            tappedDataSetIndex = highlight.dataSetIndex  //  store tapped dataset
            parent.onTap?(highlight.dataSetIndex)
        }
     
        func chartValueNothingSelected(_ chartView: ChartViewBase) {
            parent.highlightedIndex = nil
            tappedDataSetIndex = nil
        }
    }
    
    
    


     
    

   // MARK: - Chart Setup

   func createChart(chart: LineChartView) -> LineChartView {

       chart.chartDescription.enabled = false

       chart.rightAxis.enabled = false

       chart.legend.form = .none

       chart.drawBordersEnabled = false

       //  X-Axis

       chart.xAxis.labelPosition = .bottom

       chart.xAxis.labelRotationAngle = -80

       chart.xAxis.drawAxisLineEnabled = false

       chart.xAxis.drawGridLinesEnabled = true

       chart.xAxis.valueFormatter = CustomChartFormatterv(days: days)

       chart.xAxis.setLabelCount(days.count, force: false)

       chart.xAxis.labelTextColor = .black

       chart.xAxis.granularity = 1.0

       // Y-Axis

       chart.leftAxis.enabled = true

       chart.leftAxis.granularityEnabled = true

       chart.leftAxis.granularity = 0.1

       chart.leftAxis.valueFormatter = CustomNumberFormatter()

       //  Disable zoom/pan

       chart.scaleXEnabled = false

       chart.pinchZoomEnabled = false

       chart.doubleTapToZoomEnabled = false

       chart.dragEnabled = false

       chart.data = addData()

       return chart

   }

  
    
    func addData() -> LineChartData {
        LineChartData(dataSets: [
            generateLineChartDataSet(
                dataSetEntries: entries1,
                color: UIColor(lineColors[safe: 0] ?? .gray),
                label: item.values?[safe: 0]?.label ?? "Value 1"
            ),
            generateLineChartDataSet(
                dataSetEntries: entries2,
                color: UIColor(lineColors[safe: 1] ?? .gray),
                label: item.values?[safe: 1]?.label ?? "Value 2"
            )
        ])
    }

   
     
    func generateLineChartDataSet(
        dataSetEntries: [ChartDataEntry],
        color: UIColor,
        label: String
    ) -> LineChartDataSet {
        let dataSet = LineChartDataSet(entries: dataSetEntries, label: "")
        dataSet.colors = [color]
        dataSet.mode = .cubicBezier
        dataSet.circleRadius = 5
        dataSet.circleHoleColor = color
        dataSet.setCircleColor(color)
        dataSet.lineWidth = 2
        dataSet.valueTextColor = color
        dataSet.lineDashLengths = [3]
        dataSet.valueFont = UIFont(name: "Avenir", size: 12)!
        return dataSet
    }
    
    
}



struct MultiLineChartView1v: UIViewRepresentable {
    var entries1: [ChartDataEntry]
    var days: [String]
    var item: RPMVitalsChartDaysDataModel
    var lineColors: [Color]
    var onTap: ((Int) -> Void)? = nil
    @Binding var highlightedIndex: Int?
 
    func makeCoordinator() -> Coordinator {
        Coordinator(self)
    }
 
    func makeUIView(context: Context) -> LineChartView {
        let chart = LineChartView()
        chart.delegate = context.coordinator
 
        let marker = CustomMarkerView(color: .blue, font: .systemFont(ofSize: 12), days: days)
        marker.chartView = chart
        chart.marker = marker
 
        return createChart(chart: chart)
    }
 
    func updateUIView(_ uiView: LineChartView, context: Context) {
        uiView.data = addData()
 
        //  Highlight using tappedDataSetIndex
        if let highlightedIndex = highlightedIndex {
            let datasetIndex = context.coordinator.tappedDataSetIndex ?? 0
            uiView.highlightValue(Highlight(
                x: Double(highlightedIndex),
                dataSetIndex: datasetIndex,
                stackIndex: -1
            ))
        } else {
            uiView.highlightValue(nil)
        }
    }
 
    class Coordinator: NSObject, ChartViewDelegate {
        var parent: MultiLineChartView1v
        var tappedDataSetIndex: Int? = nil
 
        init(_ parent: MultiLineChartView1v) {
            self.parent = parent
        }
 
        func chartValueSelected(_ chartView: ChartViewBase, entry: ChartDataEntry, highlight: Highlight) {
            parent.highlightedIndex = Int(highlight.x)
            tappedDataSetIndex = highlight.dataSetIndex
            parent.onTap?(highlight.dataSetIndex)
        }
 
        func chartValueNothingSelected(_ chartView: ChartViewBase) {
            parent.highlightedIndex = nil
            tappedDataSetIndex = nil
        }
    }
 
    // MARK: - Chart Setup
    func createChart(chart: LineChartView) -> LineChartView {
        chart.chartDescription.enabled = false
        chart.rightAxis.enabled = false
        chart.legend.form = .none
        chart.drawBordersEnabled = false
 
        chart.xAxis.labelPosition = .bottom
        chart.xAxis.labelRotationAngle = -80
        chart.xAxis.drawAxisLineEnabled = false
        chart.xAxis.drawGridLinesEnabled = true
        chart.xAxis.valueFormatter = CustomChartFormatterv(days: days)
        chart.xAxis.setLabelCount(days.count, force: false)
        chart.xAxis.labelTextColor = .black
        chart.xAxis.granularity = 1.0
 
        chart.leftAxis.enabled = true
        chart.leftAxis.granularityEnabled = true
        chart.leftAxis.granularity = 0.1
        chart.leftAxis.valueFormatter = CustomNumberFormatter()
 
        chart.scaleXEnabled = false
        chart.pinchZoomEnabled = false
        chart.doubleTapToZoomEnabled = false
        chart.dragEnabled = false
 
        chart.data = addData()
        return chart
    }
 
    // MARK: - Add Data
    func addData() -> LineChartData {
        LineChartData(dataSets: [
            generateLineChartDataSet(
                dataSetEntries: entries1,
                color: UIColor(lineColors[safe: 0] ?? .gray),
                label: item.values?[safe: 0]?.label ?? "Value"
            )
        ])
    }
 
    // MARK: - Dataset Builder
    func generateLineChartDataSet(dataSetEntries: [ChartDataEntry], color: UIColor, label: String) -> LineChartDataSet {
        let dataSet = LineChartDataSet(entries: dataSetEntries, label: "")
        dataSet.colors = [color]
        dataSet.mode = .cubicBezier
        dataSet.circleRadius = 5
        dataSet.circleHoleColor = color
        dataSet.setCircleColor(color)
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
     
        // Use highlight.dataSetIndex to get the correct dataset

        guard let chartData = chartView?.data,

              highlight.dataSetIndex < chartData.dataSetCount else {

            return

        }
     
        let dataSet = chartData.dataSets[highlight.dataSetIndex]

        let entriesAtSameX = dataSet.entriesForXValue(entry.x)
     
        // There should be only one entry in this dataset for this x-value

        if let selectedEntry = entriesAtSameX.first,

           let dataDict = selectedEntry.data as? [String: Any],

           let labelString = dataDict["label"] as? String {
     
            label.text = " \(date)\n \(labelString): \(selectedEntry.y)"

        }
     
        label.textAlignment = .left

        label.lineBreakMode = .byWordWrapping
     
        // Adjust the size of the marker view

        let labelSize = label.sizeThatFits(CGSize(width: CGFloat.greatestFiniteMagnitude, height: CGFloat.greatestFiniteMagnitude))

        self.bounds.size.width = labelSize.width + 20

        self.bounds.size.height = labelSize.height + 30

        label.frame = CGRect(x: 10, y: 10, width: labelSize.width, height: labelSize.height)
     
        label.textColor = UIColor.white
     
        // Adjust the offset

        var offset = CGPoint(x: -(bounds.width / 2), y: -bounds.height - 12)

        self.offset = offset

    }


}
