//
//  chart30DaysView.swift
//  RPM
//
//  Created by Tesplabs on 05/08/1945 Saka.
//


import SwiftUI
import Charts

struct chart30DaysView: View {
    
    
    @State private var highlightedIndex: Int?
    var item: RPMVitalsChartDaysDataModel
    var viewWidth: CGFloat
    
    var body: some View {
        VStack(spacing: 0) {
            // Vital name header
            HStack {
                Text(item.vitalName ?? "No Vital")
                    .foregroundColor(Color("darkGreen"))
                    .font(.system(size: 20, weight: .medium))
                    .padding()
                    .frame(maxWidth: .infinity, alignment: .leading)
            }
            .frame(height: 50)
            .background(Color("ColorGreen"))
            .cornerRadius(10, corners: [.topLeft, .topRight])
            
            // Chart
   
            HStack {
                if item.time == nil || item.values?.isEmpty == true {
                    ZStack {
                        Color.white
                        Text("No Readings")
                            .foregroundColor(.red)
                            .font(.system(size: 16, weight: .semibold))
                    }
                    .frame(width: viewWidth, height: 400)
                } else {
                    let valSummary = item.values ?? []
                    let dayCount = item.time ?? []
                    let dynamicWidth = CGFloat(max(dayCount.count, 7)) * 30

                    ScrollView(.horizontal, showsIndicators: true) {
                        getChartView(for: item.vitalName ?? "", values: valSummary, days: dayCount)
                            .frame(width: dynamicWidth, height: 400)
                            .background(Color.white)
                    }
                    .frame(width: viewWidth, height: 400) // viewport size
                }
            }


        
              // Legend
              HStack {
                  if let values = item.values {
                      legendView(for: values)
                  }
              }
              .frame(height: 30)
              .padding(.bottom, 10)
              .background(Color.white)
              .cornerRadius(10, corners: [.bottomLeft, .bottomRight])
          }
          .frame(width: viewWidth) 
          .padding()
      }
    
    
    /// **Function to get the appropriate chart view dynamically**
    @ViewBuilder
    func getChartView(for vitalName: String, values: [Valuev], days: [String]) -> some View {
        switch vitalName {
        case "Blood Pressure" where values.count == 3:
            MultiLineChartView3v(
                entries1: getEntries(valueSummary: values, entryIndex: 0),
                entries2: getEntries(valueSummary: values, entryIndex: 1),
                entries3: getEntries(valueSummary: values, entryIndex: 2),
                days: getDaysv7(patientSummary: days),
                item: item,
                lineColors: ChartColors.bloodPressure,
                highlightedIndex: $highlightedIndex
            )
            .padding(.top, 8)
        case "Blood Glucose":
            if values.count == 1 {
                MultiLineChartView1v(
                    entries1: getEntries(valueSummary: values, entryIndex: 0),
                    days: getDaysv7(patientSummary: days),
                    item: item,
                    lineColors: [ChartColors.bloodGlucose[0]], // Always use blue for single glucose
                    highlightedIndex: $highlightedIndex
                )
                .padding(.top, 8)
            } else if values.count == 2 {
                MultiLineChartView2v(
                    entries1: getEntries(valueSummary: values, entryIndex: 0),
                    entries2: getEntries(valueSummary: values, entryIndex: 1),
                    days: getDaysv7(patientSummary: days),
                    item: item,
                    lineColors: ChartColors.bloodGlucose, // Blue + Pink
                    highlightedIndex: $highlightedIndex
                )
                .padding(.top, 8)
            }
        case "Oxygen":
            if values.count == 1 {
                MultiLineChartView1v(
                    entries1: getEntries(valueSummary: values, entryIndex: 0),
                    days: getDaysv7(patientSummary: days),
                    item: item,
                    lineColors: [ChartColors.oxygen[0]],
                    highlightedIndex: $highlightedIndex
                )
                .padding(.top, 8)
            }
            else if values.count == 2 {
                MultiLineChartView2v(
                    entries1: getEntries(valueSummary: values, entryIndex: 0),
                    entries2: getEntries(valueSummary: values, entryIndex: 1),
                    days: getDaysv7(patientSummary: days),
                    item: item,
                    lineColors: ChartColors.oxygen,
                    highlightedIndex: $highlightedIndex
                )
                .padding(.top, 8)
            }
                
                
        case "Weight":
            MultiLineChartView1v(
                entries1: getEntries(valueSummary: values, entryIndex: 0),
                days: getDaysv7(patientSummary: days),
                item: item,
                lineColors: [ChartColors.weight],
                highlightedIndex: $highlightedIndex
            )
            .padding(.top, 8)
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
            return ChartColors.oxygen[index % ChartColors.oxygen.count]
        case "Blood Pressure":
            return ChartColors.bloodPressure[index % ChartColors.bloodPressure.count]
        case "Blood Glucose":
            if valuesCount == 1 {
                return ChartColors.bloodGlucose[0] // Fasting (Blue)
            } else {
                return ChartColors.bloodGlucose[index % ChartColors.bloodGlucose.count]
            }
        case "Weight":
            return ChartColors.weight
        default:
            return ChartColors.singleLine
        }
    }
    
    
}
