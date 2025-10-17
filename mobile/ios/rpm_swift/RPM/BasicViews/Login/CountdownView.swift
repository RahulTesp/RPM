

import SwiftUI
 
struct CountdownView: View {
    @Binding var otpCode: String
    @Binding var showText: Bool
    @Binding var showResend: Bool
    @Binding var restartTimer: Bool
 
    @State private var counter: Int = 0
    @State private var timerActive: Bool = true
 
    var countTo: Int {
            // First try to get TimeLimit from UserDefaults
            if let timeLimit = UserDefaults.standard.object(forKey: "TimeLimit") as? Int {
                return timeLimit * 60 // convert minutes to seconds
            }
            // Otherwise fallback to TimeLimitRP
            let timeLimitRP = UserDefaults.standard.integer(forKey: "TimeLimitRP")
            return max(timeLimitRP * 60, 30) // minimum 30 sec
        }
    

 
    // Timer publisher
    let timer = Timer.publish(every: 1, on: .main, in: .common).autoconnect()
 
    var body: some View {
        VStack {
            ZStack {
                ProgressTrack()
                ProgressBars(counter: counter, countTo: countTo)
                Clock(counter: counter, countTo: countTo)
            }
        }
        .onReceive(timer) { _ in
            guard timerActive else { return }
 
            if restartTimer {
                counter = 0
                otpCode = ""
                showText = false
                showResend = false
                restartTimer = false
            } else {
                if counter < countTo {
                    counter += 1
                } else {
                    showText = true
                    showResend = true
                    otpCode = ""
                    timerActive = false // stop counting after completion
                }
            }
        }
        .onDisappear {
            timerActive = false
        }
    }
}
 
struct Clock: View {
    var counter: Int
    var countTo: Int
 
    var body: some View {
        Text("\(countTo - counter) sec")
            .font(.custom("Avenir Next", size: 12))
            .foregroundColor(.black)
    }
}
 
struct ProgressTrack: View {
    var body: some View {
        Circle()
            .stroke(Color("lightGreen"), lineWidth: 2)
            .frame(width: 50, height: 50)
            .background(Circle().fill(Color.clear))
    }
}
 
struct ProgressBars: View {
    var counter: Int
    var countTo: Int
 
    var body: some View {
        Circle()
            .trim(from: 0, to: progress())
            .stroke(
                style: StrokeStyle(lineWidth: 2, lineCap: .round, lineJoin: .round)
            )
            .foregroundColor(Color("ProgressColor"))
            .frame(width: 50, height: 50)
            .rotationEffect(.degrees(-90))
            .animation(.linear(duration: 0.2), value: counter)
    }
 
    private func progress() -> CGFloat {
        guard countTo > 0 else { return 0 }
        return CGFloat(counter) / CGFloat(countTo)
    }
}

