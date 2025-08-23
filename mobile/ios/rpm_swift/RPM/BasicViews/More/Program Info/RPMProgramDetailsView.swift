//
//  RPMProgramDetailsView.swift
//  RPM
//
//  Created by Tesplabs on 25/07/1944 Saka.
//

import Foundation
import SwiftUI


struct RPMProgramDetailsView: View {

    @ObservedObject var pgmDetList = RPMProgramInfoViewModel()
    @EnvironmentObject var accountListVM: RPMHomeViewModel

    var body: some View {
        GeometryReader { geometry in
            Group {
                if pgmDetList.loading {
                    VStack(spacing: 20) {
                        Spacer()
                        ProgressView()
                            .tint(Color("TextColorBlack"))
                        Text("Loading…")
                            .foregroundColor(Color("TextColorBlack"))
                        Spacer()
                    }
                    .frame(maxWidth: .infinity, maxHeight: .infinity)
                } else {
                    ScrollView {
                        VStack(alignment: .leading, spacing: 20) {
                            
                            Text("Program Details")
                                .foregroundColor(.black)
                                .font(Font.custom("Rubik-SemiBold", size: 24))

                            HStack {
                                Text(accountListVM.accounts?.programName ?? "")
                                    .font(Font.custom("Rubik-Regular", size: 16))
                                    .foregroundColor(.black)

                                Spacer()

                                Text(accountListVM.accounts?.status ?? "")
                                    .padding(.horizontal, 12)
                                    .padding(.vertical, 6)
                                    .font(Font.custom("Rubik-Regular", size: 14))
                                    .foregroundColor(ColourStatus.getForegroundColor(for: accountListVM.accounts?.status))
                                    .background(ColourStatus.getBackgroundColor(for: accountListVM.accounts?.status))
                                    .cornerRadius(15)
                            }
                            .padding(.vertical, 8)
                            .padding(.horizontal, 12)
                            .background(Color("avgGreen"))
                            .cornerRadius(15)
                            .frame(width: geometry.size.width - 32)


                            Group {
                                Text("Duration")
                                    .foregroundColor(Color("title1"))
                                    .font(Font.custom("Rubik-Regular", size: 14))

                                Text("\(pgmDetList.pgmInfo?.patientProgramdetails.duration ?? 0) Months")
                                    .font(Font.custom("Rubik-Regular", size: 16))
                                    .foregroundColor(.black)
                                    .padding(.vertical, 8)
                                    .padding(.horizontal, 12)
                                    .frame(width: geometry.size.width - 32, alignment: .leading)
                                    .background(RoundedRectangle(cornerRadius: 8).fill(Color("textFieldBG")).overlay(
                                        RoundedRectangle(cornerRadius: 8).stroke(Color("textFieldBG"), lineWidth: 2)))
                            }

                            HStack(spacing: 16) {
                                VStack(alignment: .leading) {
                                    Text("Start Date")
                                        .foregroundColor(Color("title1"))
                                        .font(Font.custom("Rubik-Regular", size: 14))

                                    Text(datePgmInfo(timeval: pgmDetList.pgmInfo?.patientProgramdetails.startDate ?? ""))
                                        .font(Font.custom("Rubik-Regular", size: 16))
                                        .foregroundColor(.black)
                                        .padding(.vertical, 8)
                                        .padding(.horizontal, 12)
                                        .frame(maxWidth: .infinity, alignment: .leading)
                                        .background(RoundedRectangle(cornerRadius: 8).fill(Color("textFieldBG")).overlay(
                                            RoundedRectangle(cornerRadius: 8).stroke(Color("textFieldBG"), lineWidth: 2)))
                                }

                                VStack(alignment: .leading) {
                                    Text("End Date")
                                        .foregroundColor(Color("title1"))
                                        .font(Font.custom("Rubik-Regular", size: 14))

                                    Text(convertdobformat(inputDate: pgmDetList.pgmInfo?.patientProgramdetails.endDate ?? ""))
                                        .font(Font.custom("Rubik-Regular", size: 16))
                                        .foregroundColor(.black)
                                        .padding(.vertical, 8)
                                        .padding(.horizontal, 12)
                                        .frame(maxWidth: .infinity, alignment: .leading)
                                        .background(RoundedRectangle(cornerRadius: 8).fill(Color("textFieldBG")).overlay(
                                            RoundedRectangle(cornerRadius: 8).stroke(Color("textFieldBG"), lineWidth: 2)))
                                }
                            }
                            .frame(width: geometry.size.width - 32)

                            Text(UserDefaults.standard.string(forKey: "pgmTypeString") == "RPM" ? "Vitals Monitored" : "Conditions Monitored")
                                .foregroundColor(Color("title1"))
                                .font(Font.system(size: 15))
                                .padding(.bottom, 8)

                            ForEach(pgmDetList.pgmInfo?.patientProgramdetails.patientVitalInfos.filter { $0.selected } ?? []) { item in
                                Text(item.vital)
                                    .font(Font.custom("Rubik-Regular", size: 16))
                                    .foregroundColor(.black)
                                    .padding(.vertical, 8)
                                    .padding(.horizontal, 12)
                                    .frame(width: geometry.size.width - 32, alignment: .leading)
                                    .background(RoundedRectangle(cornerRadius: 8).fill(Color("textFieldBG")).overlay(
                                        RoundedRectangle(cornerRadius: 8).stroke(Color("textFieldBG"), lineWidth: 2)))
                            }

                            Group {
                                Text("Care Team")
                                    .foregroundColor(Color("buttonColor"))
                                    .font(Font.custom("Rubik-SemiBold", size: 15))
                                    .padding(.top, 20)

                                Text("Physician")
                                    .foregroundColor(Color("title1"))
                                    .font(Font.custom("Rubik-Regular", size: 14))

                                Text(pgmDetList.pgmInfo?.patientPrescribtionDetails.physician ?? "")
                                    .foregroundColor(.black)
                                    .font(Font.custom("Rubik-Regular", size: 16))
                                    .padding(.vertical, 8)
                                    .padding(.horizontal, 12)
                                    .frame(width: geometry.size.width - 32, alignment: .leading)
                                    .background(RoundedRectangle(cornerRadius: 8).fill(Color("textFieldBG")).overlay(
                                        RoundedRectangle(cornerRadius: 8).stroke(Color("textFieldBG"), lineWidth: 2)))
                            }

                            Group {
                                Text("Care Team Personal")
                                    .foregroundColor(Color("title1"))
                                    .font(Font.custom("Rubik-Regular", size: 14))
                                    .padding(.top, 16)

                                Text(pgmDetList.pgmInfo?.patientProgramdetails.assignedMember ?? "")
                                    .foregroundColor(.black)
                                    .font(Font.custom("Rubik-Regular", size: 16))
                                    .padding(.vertical, 8)
                                    .padding(.horizontal, 12)
                                    .frame(width: geometry.size.width - 32, alignment: .leading)
                                    .background(RoundedRectangle(cornerRadius: 8).fill(Color("textFieldBG")).overlay(
                                        RoundedRectangle(cornerRadius: 8).stroke(Color("textFieldBG"), lineWidth: 2)))
                            }

                            Group {
                                Text("Goals")
                                    .foregroundColor(Color("title1"))
                                    .font(Font.custom("Rubik-Regular", size: 14))
                                    .padding(.top, 16)

                                ForEach(pgmDetList.pgmInfo?.patientProgramGoals.goalDetails ?? []) { item in
                                    VStack(alignment: .leading, spacing: 10) {
                                        Text(item.goal)
                                            .font(Font.custom("Rubik-SemiBold", size: 16))
                                            .foregroundColor(.black)

                                        Text(item.goalDetailDescription)
                                            .font(Font.custom("Rubik-Regular", size: 16))
                                            .foregroundColor(.black)
                                            .multilineTextAlignment(.leading)
                                            .fixedSize(horizontal: false, vertical: true)
                                    }
                                    .padding(.vertical, 10)
                                    .padding(.horizontal, 12)
                                    .frame(width: geometry.size.width - 32, alignment: .leading)
                                    .background(RoundedRectangle(cornerRadius: 8).fill(Color("textFieldBG")).overlay(
                                        RoundedRectangle(cornerRadius: 8).stroke(Color("textFieldBG"), lineWidth: 2)))
                                    .padding(.bottom, 20)
                                }
                            }
                        }
                        .frame(width: geometry.size.width, alignment: .leading)
                        .padding(.top, 20)
                    }

                }
            }
        }
    }

    func datePgmInfo(timeval: String) -> String {
        let dateFormatter = DateFormatter()
        dateFormatter.dateFormat = "yyyy-MM-dd'T'HH:mm:ss"
        dateFormatter.timeZone = TimeZone(identifier: "UTC")

        guard let date = dateFormatter.date(from: timeval) else {
            return "Invalid date"
        }

        dateFormatter.dateFormat = "MMM d, yyyy"
        dateFormatter.timeZone = TimeZone.current
        return dateFormatter.string(from: date)
    }
}


struct DynamicHeightModifier: ViewModifier {
    func body(content: Content) -> some View {
        GeometryReader { geometry in
            content
                .frame(height: geometry.size.height + 15)
        }
    }
}


struct RPMProgramDetailsView_Previews: PreviewProvider {
    static var previews: some View {
        RPMProgramDetailsView()
    }
}


