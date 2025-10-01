////
////  RPMProgramTimelineView.swift
////  RPM
////
////  Created by Tesplabs on 26/07/1944 Saka.
////
//
//

//
//  RPMProgramTimelineView.swift
//  RPM
//
//  Created by Tesplabs on 26/07/1944 Saka.
//
 
import SwiftUI
 
struct RPMProgramTimelineView: View {
 
    @ObservedObject var pgmDetList = RPMProgramInfoViewModel()
    var body: some View {
        GeometryReader { geometry in
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
                        Text("Program Timeline")
                            .foregroundColor(.black)
                            .font(Font.custom("Rubik-SemiBold", size: 24))
                        // ---------------- Prescription ----------------
                        if let status = pgmDetList.pgmInfo?.patientPrescribtionDetails.status, !status.isEmpty {
                            SectionPrescription(geometry: geometry, pgmDetList: pgmDetList)
                        }
                        // ---------------- Enrollment ----------------
                        if let status = pgmDetList.pgmInfo?.patientEnrolledDetails.status, !status.isEmpty {
                            SectionEnrollment(geometry: geometry, pgmDetList: pgmDetList)
                        }
                        // ---------------- Active ----------------
                        if let status = pgmDetList.pgmInfo?.activePatientDetails.status, !status.isEmpty {
                            SectionActive(geometry: geometry, pgmDetList: pgmDetList)
                        }
                        // ---------------- Ready for Discharge ----------------
                        if let status = pgmDetList.pgmInfo?.readyForDischargePatientDetails.status, !status.isEmpty {
                            SectionSimple(title: "Ready To Discharge",
                                          date: pgmDetList.pgmInfo?.readyForDischargePatientDetails.assignedDate ?? "",
                                          person: pgmDetList.pgmInfo?.readyForDischargePatientDetails.assigneeName ?? "",
                                          manager: pgmDetList.pgmInfo?.readyForDischargePatientDetails.managerName ?? "",
                                          geometry: geometry)
                        }
                        // ---------------- On Hold ----------------
                        if let status = pgmDetList.pgmInfo?.onHoldPatientDetais.status, !status.isEmpty {
                            SectionSimple(title: "On Hold",
                                          date: pgmDetList.pgmInfo?.onHoldPatientDetais.assignedDate ?? "",
                                          person: pgmDetList.pgmInfo?.onHoldPatientDetais.assigneeName ?? "",
                                          manager: pgmDetList.pgmInfo?.onHoldPatientDetais.managerName ?? "",
                                          geometry: geometry)
                        }
                        // ---------------- Inactive ----------------
                        if let status = pgmDetList.pgmInfo?.inActivePatientDetais.status, !status.isEmpty {
                            SectionSimple(title: "Inactive",
                                          date: pgmDetList.pgmInfo?.inActivePatientDetais.assignedDate ?? "",
                                          person: pgmDetList.pgmInfo?.inActivePatientDetais.assigneeName ?? "",
                                          manager: pgmDetList.pgmInfo?.inActivePatientDetais.managerName ?? "",
                                          geometry: geometry)
                        }
                        // ---------------- Discharged ----------------
                        if let status = pgmDetList.pgmInfo?.dischargedPatientDetails.status, !status.isEmpty {
                            SectionSimple(title: "Discharged",
                                          date: pgmDetList.pgmInfo?.dischargedPatientDetails.assignedDate ?? "",
                                          person: pgmDetList.pgmInfo?.dischargedPatientDetails.assigneeName ?? "",
                                          manager: pgmDetList.pgmInfo?.dischargedPatientDetails.managerName ?? "",
                                          geometry: geometry)
                        }
                    }
                    .padding(.horizontal, 16)
                    .padding(.top, 10)
                    .frame(width: geometry.size.width - 32)
                }
                .scrollIndicators(.hidden)
            }
        }
    }
    // MARK: - Date Helpers
        func formattedConsultationDate(dateString: String) -> String {
            let dateFormatter = DateFormatter()
            dateFormatter.dateFormat = "MM/dd/yyyy hh:mm:ss a"
            if let date = dateFormatter.date(from: dateString) {
                dateFormatter.dateFormat = "MMM d, yyyy"
                return dateFormatter.string(from: date)
            } else {
                return ""
            }
        }

    }

// MARK: - Section Extracted Views

private struct SectionPrescription: View {
    let geometry: GeometryProxy
    let pgmDetList: RPMProgramInfoViewModel
    
    var body: some View {
        VStack(alignment: .leading, spacing: 10) {
            // --- Prescription Title ---
            Text("Prescription")
                .foregroundColor(Color("buttonColor"))
                .font(Font.custom("Rubik-SemiBold", size: 16))
            
            // --- Date ---
            Text("Date")
                .foregroundColor(Color("title1"))
                .font(Font.custom("Rubik-Regular", size: 14))
            
            Text(datePrescribed(timeval: pgmDetList.pgmInfo?.patientPrescribtionDetails.prescribedDate ?? ""))
                .font(Font.custom("Rubik-Regular", size: 16))
                .foregroundColor(.black)
                .padding(8)
                .frame(width: geometry.size.width - 32, alignment: .leading)
                .background(RoundedRectangle(cornerRadius: 8).fill(Color("textFieldBG")))
            
            // --- Physician ---
            Text("Physician")
                .foregroundColor(Color("title1"))
                .font(Font.custom("Rubik-Regular", size: 14))
            
            Text(pgmDetList.pgmInfo?.patientPrescribtionDetails.physician ?? "")
                .font(Font.custom("Rubik-Regular", size: 16))
                .foregroundColor(.black)
                .padding(8)
                .frame(width: geometry.size.width - 32, alignment: .leading)
                .background(RoundedRectangle(cornerRadius: 8).fill(Color("textFieldBG")))
            
            // --- Clinic & Clinic Code in HStack ---
            HStack(spacing: 8) {
                VStack(alignment: .leading) {
                    Text("Clinic")
                        .foregroundColor(Color("title1"))
                        .font(Font.custom("Rubik-Regular", size: 14))
                    
                    Text(pgmDetList.pgmInfo?.patientPrescribtionDetails.clinic ?? "")
                        .font(Font.custom("Rubik-Regular", size: 16))
                        .foregroundColor(.black)
                        .padding(8)
                        .frame(width: ((geometry.size.width - 32) - 8) / 2, alignment: .leading)
                        .background(
                            RoundedRectangle(cornerRadius: 8)
                                .fill(Color("textFieldBG"))
                                .overlay(
                                    RoundedRectangle(cornerRadius: 8)
                                        .stroke(Color("textFieldBG"), lineWidth: 2)
                                )
                        )
                }
                
                VStack(alignment: .leading) {
                    Text("Clinic Code")
                        .foregroundColor(Color("title1"))
                        .font(Font.custom("Rubik-Regular", size: 14))
                    
                    Text(pgmDetList.pgmInfo?.patientPrescribtionDetails.clinicCode ?? "")
                        .font(Font.custom("Rubik-Regular", size: 16))
                        .foregroundColor(.black)
                        .padding(8)
                        .frame(width: ((geometry.size.width - 32) - 8) / 2, alignment: .leading)
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
            
            // --- Order Date ---
            Text("Order Date")
                .foregroundColor(Color("title1"))
                .font(Font.custom("Rubik-Regular", size: 14))
            
         
                                    Text(
                                        formattedConsultationDate(  dateString: pgmDetList.pgmInfo?.patientPrescribtionDetails.consultationDate ?? "")
            
                                    )
            
                .font(Font.custom("Rubik-Regular", size: 16))
                .foregroundColor(.black)
                .padding(8)
                .frame(width: geometry.size.width - 32, alignment: .leading)
                .background(
                    RoundedRectangle(cornerRadius: 8)
                        .fill(Color("textFieldBG"))
                        .overlay(
                            RoundedRectangle(cornerRadius: 8)
                                .stroke(Color("textFieldBG"), lineWidth: 2)
                        )
                )
            
            // --- Diagnosis List ---
            Text("Diagnosis")
                .foregroundColor(Color("title1"))
                .font(Font.custom("Rubik-Regular", size: 14))
            
            ForEach(pgmDetList.pgmInfo?.patientPrescribtionDetails.patientDiagnosisInfos ?? []) { item in
                Text("\(item.diagnosisCode) / \(item.diagnosisName)")
                    .font(Font.custom("Rubik-Regular", size: 16))
                    .foregroundColor(.black)
                    .padding(8)
                    .frame(width: geometry.size.width - 32, alignment: .leading)
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
    }
    
    // --- Helper for Prescribed Date ---
    func datePrescribed(timeval: String) -> String {
        if timeval.isEmpty || timeval.starts(with: "0001-01-01") { return "" }
        let formats = ["yyyy-MM-dd'T'HH:mm:ss.SSS", "yyyy-MM-dd'T'HH:mm:ss.SS", "yyyy-MM-dd'T'HH:mm:ss"]
        
        let formatter = DateFormatter()
        formatter.timeZone = TimeZone(identifier: "UTC")
        
        var date: Date? = nil
        for f in formats {
            formatter.dateFormat = f
            if let parsed = formatter.date(from: timeval) {
                date = parsed
                break
            }
        }
        guard let finalDate = date else { return "" }
        
        formatter.dateFormat = "MMM d, yyyy"
        formatter.timeZone = .current
        return formatter.string(from: finalDate)
    }
    
    // --- Helper for Consultation Date ---
        func formattedConsultationDate(dateString: String) -> String {
               let dateFormatter = DateFormatter()
               dateFormatter.dateFormat = "MM/dd/yyyy hh:mm:ss a"
    
               if let date = dateFormatter.date(from: dateString) {
                   dateFormatter.dateFormat = "MMM d, yyyy"
                   return dateFormatter.string(from: date)
               } else {
                   return ""
               }
           }
}

 
private struct SectionEnrollment: View {
    let geometry: GeometryProxy
    let pgmDetList: RPMProgramInfoViewModel
    var body: some View {
        VStack(alignment: .leading, spacing: 10) {
            Text("Enrollment")
                .foregroundColor(Color("buttonColor"))
                .font(Font.custom("Rubik-SemiBold", size: 16))
            let enrollmentPersonal = (pgmDetList.pgmInfo?.patientEnrolledDetails.patientEnrolledInfos.isEmpty == false)
            ? pgmDetList.pgmInfo!.patientEnrolledDetails.patientEnrolledInfos[0].enrollmentPersonal
            : ""
            Text("Date").foregroundColor(Color("title1"))
                .font(Font.custom("Rubik-Regular", size: 14))
          //  Text(pgmDetList.pgmInfo?.patientEnrolledDetails.assignedDate ?? "")
            Text(dateEnrolled(timeval: pgmDetList.pgmInfo?.patientEnrolledDetails.assignedDate ?? ""))
                .font(Font.custom("Rubik-Regular", size: 16))
                .foregroundColor(.black)
                .padding(8)
                .frame(width: geometry.size.width - 32, alignment: .leading)
                .background(RoundedRectangle(cornerRadius: 8).fill(Color("textFieldBG")))
            Text("Enrollment Personal").foregroundColor(Color("title1"))
                .font(Font.custom("Rubik-Regular", size: 14))
            Text(enrollmentPersonal)
                .font(Font.custom("Rubik-Regular", size: 16))
                .foregroundColor(.black)
                .padding(8)
                .frame(width: geometry.size.width - 32, alignment: .leading)
                .background(RoundedRectangle(cornerRadius: 8).fill(Color("textFieldBG")))
        }
    }
 
    
    func dateEnrolled(timeval: String) -> String {

        print("activetimeval", timeval)

        let dateFormatter = DateFormatter()

        dateFormatter.timeZone = TimeZone(abbreviation: "UTC")

        let date: Date?

        // Check if string contains fractional seconds

        if timeval.contains(".") {

            dateFormatter.dateFormat = "yyyy-MM-dd'T'HH:mm:ss.SSS"

            date = dateFormatter.date(from: timeval)

        } else {

            dateFormatter.dateFormat = "yyyy-MM-dd'T'HH:mm:ss"

            date = dateFormatter.date(from: timeval)

        }

        guard let validDate = date else {

            print("Failed to parse date")

            return ""

        }

        // Convert to local readable format

        dateFormatter.dateFormat = "MMM d, yyyy"

        dateFormatter.timeZone = TimeZone.current

        let timeStamp = dateFormatter.string(from: validDate)

        print("activetimeStamp", timeStamp)

        return timeStamp

    }

 
}
private struct SectionActive: View {
    let geometry: GeometryProxy
    let pgmDetList: RPMProgramInfoViewModel
    var body: some View {
        VStack(alignment: .leading, spacing: 10) {
            Text("Active")
                .foregroundColor(Color("buttonColor"))
                .font(Font.custom("Rubik-SemiBold", size: 16))
            Text("Date").foregroundColor(Color("title1"))
                .font(Font.custom("Rubik-Regular", size: 14))
         //   Text(pgmDetList.pgmInfo?.activePatientDetails.assignedDate ?? "")
            Text(dateActive(timeval: pgmDetList.pgmInfo?.activePatientDetails.assignedDate ?? ""))
                .font(Font.custom("Rubik-Regular", size: 16))
                .foregroundColor(.black)
                .padding(8)
                .frame(width: geometry.size.width - 32, alignment: .leading)
                .background(RoundedRectangle(cornerRadius: 8).fill(Color("textFieldBG")))
            Text("Assigned Personal").foregroundColor(Color("title1"))
                .font(Font.custom("Rubik-Regular", size: 14))
            Text(pgmDetList.pgmInfo?.activePatientDetails.assigneeName ?? "")
                .font(Font.custom("Rubik-Regular", size: 16))
                .foregroundColor(.black)
                .padding(8)
                .frame(width: geometry.size.width - 32, alignment: .leading)
                .background(RoundedRectangle(cornerRadius: 8).fill(Color("textFieldBG")))

        }
    }
    func dateActive(timeval: String) -> String {
        print("activetimeval", timeval)
        // treat empty or sentinel .NET date as "no value"
        if timeval.isEmpty || timeval.starts(with: "0001-01-01") {
            return ""
        }

        // Try common formats (from most-specific to least)
        let formats = [
            "yyyy-MM-dd'T'HH:mm:ss.SSSSSS", // microsec
            "yyyy-MM-dd'T'HH:mm:ss.SSS",    // milliseconds
            "yyyy-MM-dd'T'HH:mm:ss.SS",     // 2-digit fraction (rare)
            "yyyy-MM-dd'T'HH:mm:ss"         // seconds, e.g. "2022-02-01T00:00:00"
        ]

        let parser = DateFormatter()
        parser.locale = Locale(identifier: "en_US_POSIX")      // important for stable parsing
        parser.timeZone = TimeZone(abbreviation: "UTC")       // input is UTC

        var parsedDate: Date? = nil
        for fmt in formats {
            parser.dateFormat = fmt
            if let d = parser.date(from: timeval) {
                parsedDate = d
                break
            }
        }

        // Last-resort: try ISO8601DateFormatter (handles Z and many variants)
        if parsedDate == nil {
            if #available(iOS 11.0, *) {
                let iso = ISO8601DateFormatter()
                iso.formatOptions = [.withInternetDateTime, .withFractionalSeconds]
                parsedDate = iso.date(from: timeval)
                if parsedDate == nil {
                    iso.formatOptions = [.withInternetDateTime]
                    parsedDate = iso.date(from: timeval)
                }
            }
        }

        guard let date = parsedDate else {
            print("date parsing failed for:", timeval)
            return "" // or return the raw string if you prefer
        }

        // Output in local timezone
        let out = DateFormatter()
        out.locale = Locale.current
        out.timeZone = .current
        out.dateFormat = "MMM d, yyyy" // change to include time if needed e.g. "MMM d, yyyy, h:mm a"
        let timeStamp = out.string(from: date)
        print("activetimeStamp", timeStamp)
        return timeStamp
    }

}
 
private struct SectionSimple: View {
    let title: String
    let date: String
    let person: String
    let manager: String
    let geometry: GeometryProxy
    var body: some View {
        VStack(alignment: .leading, spacing: 10) {
            Text(title)
                .foregroundColor(Color("buttonColor"))
                .font(Font.custom("Rubik-SemiBold", size: 16))
            Text("Date").foregroundColor(Color("title1"))
                .font(Font.custom("Rubik-Regular", size: 14))
           // Text(date)
            Text((dateSectionSimple(timeval : date)))
            
                .font(Font.custom("Rubik-Regular", size: 16))
                .foregroundColor(.black)
                .padding(8)
                .frame(width: geometry.size.width - 32, alignment: .leading)
                .background(RoundedRectangle(cornerRadius: 8).fill(Color("textFieldBG")))
            Text("Assigned Personal").foregroundColor(Color("title1"))
                .font(Font.custom("Rubik-Regular", size: 14))
            Text(person)
                .font(Font.custom("Rubik-Regular", size: 16))
                .foregroundColor(.black)
                .padding(8)
                .frame(width: geometry.size.width - 32, alignment: .leading)
                .background(RoundedRectangle(cornerRadius: 8).fill(Color("textFieldBG")))

        }
    }
    
    func dateSectionSimple(timeval: String) -> String {
        // Handle empty or default .NET date
        if timeval.isEmpty || timeval.starts(with: "0001-01-01") {
            return ""
        }
        
        // Try multiple formats
        let formats = [
            "yyyy-MM-dd'T'HH:mm:ss.SSS", // full milliseconds
            "yyyy-MM-dd'T'HH:mm:ss.SS",  // 2-digit milliseconds
            "yyyy-MM-dd'T'HH:mm:ss"      // no milliseconds
        ]
        
        var date: Date? = nil
        let formatter = DateFormatter()
        formatter.timeZone = TimeZone(identifier: "UTC")
        
        for format in formats {
            formatter.dateFormat = format
            if let parsed = formatter.date(from: timeval) {
                date = parsed
                break
            }
        }
        
        guard let finalDate = date else {
            return ""
        }
        
        // Convert to local & display
        formatter.dateFormat = "MMM d, yyyy"
        formatter.timeZone = .current
        return formatter.string(from: finalDate)
    }

}
struct RPMProgramTimelineView_Previews: PreviewProvider {
    static var previews: some View {
        RPMProgramTimelineView()
    }
}
