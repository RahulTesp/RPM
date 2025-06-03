//
//  chart7DaysVitalsView.swift
//  RPM
//
//  Created by Tesplabs on 05/08/1945 Saka.
//


import SwiftUI
import Charts

struct chart7DaysVitalsView: View {
    
    var widthFull = UIScreen.main.bounds.width
    var width: CGFloat {
        UIDevice.current.userInterfaceIdiom == .phone ? UIScreen.main.bounds.width / 1.1 : UIScreen.main.bounds.width / 1.3
    }
    
    var heightFull = UIScreen.main.bounds.height
    var height: CGFloat {
        UIDevice.current.userInterfaceIdiom == .phone ? UIScreen.main.bounds.height / 1.39 : UIScreen.main.bounds.height / 1.3
    }
    
    @State private var highlightedIndex: Int?
    
    var item: RPMVitalsChartDaysDataModel
    
    var body: some View {
        VStack(spacing: 0) {  // Stack elements without extra spacing
            
            // **1st HStack: Vital Name (Green Background)**
            HStack {
                Text(item.vitalName ?? "No Vital")
                    .foregroundColor(Color("darkGreen"))
                    .font(.system(size: 20, weight: .medium))
                    .padding()
                    .frame(maxWidth: .infinity, alignment: .leading)
            }
            .frame(width: 400, height: 50)  // Ensure consistent width
            .background(Color("ColorGreen"))
            .cornerRadius(10, corners: [.topLeft, .topRight])
           
            
            // **2nd HStack: Chart (White Background)**
            HStack {
                if item.time == nil || item.values?.isEmpty == true {
                    Text("No Readings").foregroundColor(.red)
                } else {
                    let valSummary = item.values ?? []
                    let dayCount = item.time ?? []
                    
                    let chartView = getChartView(for: item.vitalName ?? "", values: valSummary, days: dayCount)
                    
                    chartView
                        .onTapGesture {
                            print("Tapped on index: \(highlightedIndex ?? -1)")
                        }
                }
            }
            .frame(width: 400, height: 300) // Ensure same width
            .background(Color.white)  // White background
            .cornerRadius(0) // No corner radius to keep separation clear
         
            
            // **3rd HStack: Legend (White Background)**
            HStack {
                if let values = item.values {
                    legendView(for: values)
                }
            }
            .frame(width: 400, height: 30) // Ensure same width
            .padding(.bottom, 10)
            .background(Color.white) // White background
            .cornerRadius(10, corners: [.bottomLeft, .bottomRight]) // Round bottom corners
        }
       // .shadow(radius: 5) // Optional shadow
        .frame(width: 400)  // Ensure proper width
    }
    
    /// **Function to get the appropriate chart view dynamically**
    @ViewBuilder
    func getChartView(for vitalName: String, values: [Valuev], days: [String]) -> some View {
        switch values.count {
        case 3 where vitalName == "Blood Pressure":
            MultiLineChartView3v(
                entries1: getEntries(valueSummary: values, entryIndex: 0),
                entries2: getEntries(valueSummary: values, entryIndex: 1),
                entries3: getEntries(valueSummary: values, entryIndex: 2),
                days: getDaysv7(patientSummary: days),
                item: item,
                highlightedIndex: $highlightedIndex
            )   .padding()
        case 2:
            MultiLineChartView2v(
                entries1: getEntries(valueSummary: values, entryIndex: 0),
                entries2: getEntries(valueSummary: values, entryIndex: 1),
                days: getDaysv7(patientSummary: days),
                item: item,
                highlightedIndex: $highlightedIndex
            )   .padding()
        case 1:
            MultiLineChartView1v(
                entries1: getEntries(valueSummary: values, entryIndex: 0),
                days: getDaysv7(patientSummary: days),
                item: item,
                highlightedIndex: $highlightedIndex
            )   .padding()
        default:
            Text("Unsupported Vital Type")
        }
    }
    
    /// **Function to generate the legend dynamically**
    @ViewBuilder
    func legendView(for values: [Valuev]) -> some View {
        HStack {
            ForEach(values.indices, id: \.self) { index in
                HStack {
                    Rectangle()
                        .fill(getColor(for: index))
                        .frame(width: 10, height: 10)
                    Text(values[index].label ?? "")
                        .foregroundColor(.black)
                        .font(.system(size: 10))
                }
            }
            Spacer()
        }
        .padding(8)
    }
    
    /// **Function to get a color for a specific vital value**
    func getColor(for index: Int) -> Color {
        let colors: [Color] = [
            Color(red: 0.20, green: 0.34, blue: 0.10), // Green
            Color(red: 0.55, green: 0.35, blue: 0.96), // Purple
            Color(red: 0.57, green: 0.00, blue: 0.23)  // Red
        ]
        return colors[index % colors.count] // Cycle through colors
    }

}

