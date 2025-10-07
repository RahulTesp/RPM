
//
//  chart7DaysVitalsView.swift
//  RPM
//
//  Created by Tesplabs on 05/08/1945 Saka.
//
 
import SwiftUI
import Charts
 
// MARK: - Chart Colors
struct ChartColors {
    static let bloodPressure: [Color] = [
        Color(red: 0.196, green: 0.341, blue: 0.102),  // Systolic (Green)
        Color(red: 0.557, green: 0.353, blue: 0.969),  // Diastolic (Purple)
        Color(red: 0.573, green: 0.0, blue: 0.231)     // Pulse (Red)
    ]
    static let bloodGlucose: [Color] = [
        Color(red: 0.176, green: 0.498, blue: 0.757),  // Fasting (Blue)
        Color(red: 0.91, green: 0.478, blue: 0.643)    // Post-Meal (Pink)
    ]
    static let oxygen: [Color] = [
        Color(red: 0.176, green: 0.498, blue: 0.757),  // Oxygen (Blue)
        Color(red: 0.91, green: 0.478, blue: 0.643)    // Pulse (Pink)
    ]
    static let singleLine: Color = Color(red: 0.196, green: 0.341, blue: 0.102) // Default Green
    static let weight: Color = Color(red: 0.31, green: 0.016, blue: 0.129)      // Maroon/Brown
}
 
// MARK: - Chart View
struct chart7DaysVitalsView: View {
    @State private var highlightedIndex: Int?
    var item: RPMVitalsChartDaysDataModel
    var viewWidth: CGFloat
    var body: some View {
        VStack(spacing: 0) {
            //  Vital Name Header
            HStack {
                Text(item.vitalName ?? "No Vital")
                    .foregroundColor(Color("darkGreen"))
                    .font(.system(size: 20, weight: .medium))
                    .padding()
                    .frame(maxWidth: .infinity, alignment: .leading)
            }
            .frame(width: viewWidth, height: 50)
            .background(Color("ColorGreen"))
            .cornerRadius(10, corners: [.topLeft, .topRight])
            // Chart Section
            HStack {
                if item.time == nil || item.values?.isEmpty == true {
                    Text("No Readings")
                        .foregroundColor(.red)
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
            .frame(width: viewWidth, height: 300)
            .background(Color.white)
            //  Legend Section
            HStack {
                if let values = item.values {
                    legendView(for: values)
                }
            }
            .frame(width: viewWidth, height: 30)
            .padding(.bottom, 10)
            .background(Color.white)
            .cornerRadius(10, corners: [.bottomLeft, .bottomRight])
        }
        .frame(width: viewWidth)
    }
    // MARK: - Chart Selector
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
            MultiLineChartView2v(
                entries1: getEntries(valueSummary: values, entryIndex: 0),
                entries2: getEntries(valueSummary: values, entryIndex: 1),
                days: getDaysv7(patientSummary: days),
                item: item,
                lineColors: ChartColors.oxygen,
                highlightedIndex: $highlightedIndex
            )
            .padding(.top, 8)
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
    // MARK: - Legend View
    @ViewBuilder
    func legendView(for values: [Valuev]) -> some View {
        HStack {
            ForEach(values.indices, id: \.self) { index in
                HStack(spacing: 4) {
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
    // MARK: - Color Resolver
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
