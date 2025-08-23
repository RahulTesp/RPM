//
//  RPMDeviceDetailsView.swift
//  RPM
//
//  Created by Tesplabs on 26/07/1944 Saka.
//



import SwiftUI

struct RPMDeviceDetailsView: View {
    @State var show: Int = 1
    @State private var userStartDate = Date()
    @ObservedObject var pgmDetList = RPMProgramInfoViewModel()

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
                .frame(width: geometry.size.width)
            } else {
                ScrollView {
                    VStack(alignment: .leading, spacing: 20) {
                        Text("Device Details")
                            .foregroundColor(.black)
                            .font(Font.custom("Rubik-SemiBold", size: 24))

                        if pgmDetList.pgmInfo?.patientDevicesDetails.patientDeviceInfos.isEmpty ?? true {
                            Text("NO DATA!")
                                .foregroundColor(.red)
                                .frame(maxWidth: .infinity, alignment: .center)
                        } else {
                            if let patientDeviceInfos = pgmDetList.pgmInfo?.patientDevicesDetails.patientDeviceInfos {
                                ForEach(Array(patientDeviceInfos.enumerated()), id: \.element.deviceNumber) { index, item in
                                    VStack(alignment: .leading) {
                                        Text("DEVICE \(index + 1)")
                                            .font(Font.custom("Rubik-Regular", size: 15))
                                            .foregroundColor(Color("buttonColor"))

                                        makeView(items: item, width: geometry.size.width)

                                    }
                                    .frame(width: geometry.size.width - 32)
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

    func makeView(items: PatientDeviceInfo, width: CGFloat) -> some View {
        let contentWidth = width - 32

        return VStack(alignment: .leading, spacing: 16) {
            Group {
                Text("Vital Monitoring")
                    .foregroundColor(Color("title1"))
                    .font(Font.custom("Rubik-Regular", size: 14))
                   
                Text(items.vitalName)
                    .foregroundColor(.black)
                    .font(Font.custom("Rubik-Regular", size: 16))
                    .padding(8)
                    .frame(width: contentWidth, alignment: .leading)
                    .background(
                        RoundedRectangle(cornerRadius: 8)
                            .fill(Color("textFieldBG"))
                            .overlay(
                                RoundedRectangle(cornerRadius: 8)
                                    .stroke(Color("textFieldBG"), lineWidth: 2)
                            )
                    )
            }
        
            HStack(spacing: 16) {
                VStack(alignment: .leading, spacing: 8) {
                    Text("Device ID")
                        .foregroundColor(Color("title1"))
                        .font(Font.custom("Rubik-Regular", size: 14))

                    Text(items.deviceNumber)
                        .foregroundColor(.black)
                        .font(Font.custom("Rubik-Regular", size: 16))
                        .padding(8)
                        .frame(width: (contentWidth - 8) / 2, alignment: .leading)
                        .background(
                            RoundedRectangle(cornerRadius: 8)
                                .fill(Color("textFieldBG"))
                                .overlay(
                                    RoundedRectangle(cornerRadius: 8)
                                        .stroke(Color("textFieldBG"), lineWidth: 2)
                                )
                        )
                }

                VStack(alignment: .leading, spacing: 8) {
                    Text("Device Type")
                        .foregroundColor(Color("title1"))
                        .font(Font.custom("Rubik-Regular", size: 14))

                    Text(items.deviceCommunicationType)
                        .foregroundColor(.black)
                        .font(Font.custom("Rubik-Regular", size: 16))
                        .padding(8)
                        .frame(width: (contentWidth - 8) / 2, alignment: .leading)
                        .background(
                            RoundedRectangle(cornerRadius: 8)
                                .fill(Color("textFieldBG"))
                                .overlay(
                                    RoundedRectangle(cornerRadius: 8)
                                        .stroke(Color("textFieldBG"), lineWidth: 2)
                                )
                        )
                }
            }
            .frame(width: contentWidth, alignment: .leading)
            
            Group {
                Text("Device")
                    .foregroundColor(Color("title1"))
                    .font(Font.custom("Rubik-Regular", size: 14))

                Text(items.deviceName)
                    .foregroundColor(.black)
                    .font(Font.custom("Rubik-Regular", size: 16))
                    .padding(8)
                    .frame(width: contentWidth, alignment: .leading)
                    .background(
                        RoundedRectangle(cornerRadius: 8)
                            .fill(Color("textFieldBG"))
                            .overlay(
                                RoundedRectangle(cornerRadius: 8)
                                    .stroke(Color("textFieldBG"), lineWidth: 2)
                            )
                    )
            }

            Group {
                Text("Device Status")
                    .foregroundColor(Color("title1"))
                    .font(Font.custom("Rubik-Regular", size: 14))

                Text(items.deviceStatus)
                    .foregroundColor(.black)
                    .font(Font.custom("Rubik-Regular", size: 16))
                    .padding(8)
                    .frame(width: contentWidth, alignment: .leading)
                    .background(
                        RoundedRectangle(cornerRadius: 8)
                            .fill(Color("textFieldBG"))
                            .overlay(
                                RoundedRectangle(cornerRadius: 8)
                                    .stroke(Color("textFieldBG"), lineWidth: 2)
                            )
                    )
            }
        }
        .frame(width: contentWidth)
        .padding(.vertical, 5)
     
    }

}


struct RPMDeviceDetailsView_Previews: PreviewProvider {
    static var previews: some View {
        RPMDeviceDetailsView()
    }
}


