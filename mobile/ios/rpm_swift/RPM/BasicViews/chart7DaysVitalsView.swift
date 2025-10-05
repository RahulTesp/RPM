//
//  chart7DaysVitalsView.swift
//  RPM
//
//  Created by Tesplabs on 05/08/1945 Saka.
//


import SwiftUI
import Charts

struct chart7DaysVitalsView: View {
    
    @State private var highlightedIndex: Int?
    
    var item: RPMVitalsChartDaysDataModel
    
    var viewWidth: CGFloat
    
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
            .frame(width: viewWidth, height: 50)  // Ensure consistent width
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
            .frame(width: viewWidth, height: 300) // Ensure same width
            .background(Color.white)  // White background
            .cornerRadius(0) // No corner radius to keep separation clear
         
            
            // **3rd HStack: Legend (White Background)**
            HStack {
                if let values = item.values {
                    legendView(for: values)
                }
            }
            .frame(width: viewWidth, height: 30) // Ensure same width
            .padding(.bottom, 10)
            .background(Color.white) // White background
            .cornerRadius(10, corners: [.bottomLeft, .bottomRight]) // Round bottom corners
        }
   
        .frame(width: viewWidth)  // Ensure proper width
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
            
                        .fill(getColor(for: index, vitalName: item.vitalName ?? "", valuesCount: item.values?.count ?? 1))
                        .frame(width: 10, height: 10)
                    Text(values[index].label)
                        .foregroundColor(.black)
                        .font(.system(size: 10))
                }
            }
            Spacer()
        }
        .padding(8)
    }
    

    
    func getColor(for index: Int, vitalName: String, valuesCount: Int) -> Color {
        switch vitalName {
        case "Oxygen":
            let oxygenColors: [Color] = [
                Color(red: 0.176, green: 0.498, blue: 0.757), // Oxygen line color
                Color(red: 0.91, green: 0.478, blue: 0.643)  // Pulse line color
            ]
            return oxygenColors[index % oxygenColors.count]
            
        case "Blood Pressure":
            let bpColors: [Color] = [
                Color(red: 0.20, green: 0.34, blue: 0.10), // Green
                Color(red: 0.55, green: 0.35, blue: 0.96), // Purple
                Color(red: 0.57, green: 0.00, blue: 0.23)  // Red
            ]
            return bpColors[index % bpColors.count]
            
        case "Blood Glucose":
               // Dynamic BG colors based on number of values
               let bgColors: [Color] = valuesCount == 2 ?
                   [Color(red: 0.91, green: 0.478, blue: 0.643),  // Non-Fasting
                    Color(red: 0.176, green: 0.498, blue: 0.757)] // Fasting
                   :
                   [Color(red: 0.176, green: 0.498, blue: 0.757)] // Only one line
               return bgColors[index % bgColors.count]
            
        default:
            let defaultColors: [Color] = [
                Color(red: 0.20, green: 0.34, blue: 0.10), // Green
                Color(red: 0.55, green: 0.35, blue: 0.96), // Purple
                Color(red: 0.57, green: 0.00, blue: 0.23)  // Red
            ]
            return defaultColors[index % defaultColors.count]
        }
    }


}

