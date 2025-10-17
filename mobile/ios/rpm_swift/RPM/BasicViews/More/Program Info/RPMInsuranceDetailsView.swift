////
////  RPMInsuranceDetailsView.swift
////  RPM
////
////  Created by Tesplabs on 26/07/1944 Saka.
////
//

import SwiftUI
 
struct RPMInsuranceDetailsView: View {

    @StateObject private var pgmDetList = RPMProgramInfoViewModel()

    var body: some View {

        GeometryReader { geometry in

            if pgmDetList.loading {

                VStack {

                    Spacer()

                    ProgressView()

                        .tint(Color("TextColorBlack"))

                    Text("Loading…")

                        .foregroundColor(Color("TextColorBlack"))

                        .padding(.vertical, 15)

                    Spacer()

                }

            } else {

                ScrollView {

                    VStack(alignment: .leading) {

                        Text("Insurance Details")

                            .foregroundColor(.black)

                            .font(Font.custom("Rubik-SemiBold", size: 24))

                            .padding(.bottom, 10)

                        
                        if let insuranceInfos = pgmDetList.pgmInfo?.patientInsurenceDetails.patientInsurenceInfos,

                           !insuranceInfos.isEmpty {
                         
                            // Primary insurance

                            if let primary = insuranceInfos.first(where: { $0.isPrimary ?? false }) {

                                InsuranceSection(title: "Primary Insurance",

                                                 vendorName: primary.insuranceVendorName ?? "")

                            }
                         
                            // Secondary insurance

                            let secondaryList = insuranceInfos.filter { $0.isPrimary == false }

                            if !secondaryList.isEmpty {

                                VStack(alignment: .leading, spacing: 8) {

                                    Text("Secondary Insurance (Optional)")

                                        .foregroundColor(Color("title1"))

                                        .font(Font.custom("Rubik-Regular", size: 14))

                                    ForEach(secondaryList) { item in

                                        InsuranceBox(vendorName: item.insuranceVendorName ?? "")

                                    }

                                }

                                .padding(.horizontal, 10)

                                .padding(.vertical, 5)

                            }

                        }

                         
                        
 else {

                            Text("NO DATA!")

                                .foregroundColor(.red)

                                .frame(maxWidth: .infinity,

                                       maxHeight: .infinity,

                                       alignment: .center)

                        }

                    }

                    .frame(maxWidth: .infinity, alignment: .leading)

                    .padding(.horizontal, 16)

                    .padding(.top, 10)

                }

            }

        }

    }

}
 
struct InsuranceSection: View {

    let title: String

    let vendorName: String

    var body: some View {

        VStack(alignment: .leading, spacing: 8) {

            Text(title)

                .foregroundColor(Color("title1"))

                .font(Font.custom("Rubik-Regular", size: 14))

            InsuranceBox(vendorName: vendorName)

        }

        .padding(.horizontal, 10)

        .padding(.vertical, 5)

    }

}
 
struct InsuranceBox: View {

    let vendorName: String

    var body: some View {

        Text(vendorName)

            .font(Font.custom("Rubik-Regular", size: 16))

            .foregroundColor(.black)

            .frame(width: 320, height: 40, alignment: .leading)

            .padding(.horizontal, 5)

            .padding(.vertical, 5)

            .background(

                RoundedRectangle(cornerRadius: 8, style: .continuous)

                    .stroke(Color("textFieldBG"), lineWidth: 2)

                    .background(Color("textFieldBG"))

                    .cornerRadius(8)

            )

    }

}
 
struct RPMInsuranceDetailsView_Previews: PreviewProvider {

    static var previews: some View {

        RPMInsuranceDetailsView()

    }

}
