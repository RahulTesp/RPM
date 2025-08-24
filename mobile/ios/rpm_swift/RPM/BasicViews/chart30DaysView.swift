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
                    Text(values[index].label)
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
