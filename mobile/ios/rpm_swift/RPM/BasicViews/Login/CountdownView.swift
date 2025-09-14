import SwiftUI

let timer = Timer
    .publish(every: 1, on: .main, in: .common)
    .autoconnect()

struct Clock: View {
    var counter: Int
    var countTo: Int

    var body: some View {
        VStack {
            Text(counterToMinutes())
                .font(.custom("Avenir Next", size: 12))
              
        }
    }
    
    func counterToMinutes() -> String {
        let currentTime = countTo - counter
 
        return "\(currentTime) Sec"
    
    }
}

struct ProgressTrack: View {
    var body: some View {
        Circle()
            .fill(Color.clear)
            .frame(width: 50, height: 50)
            .overlay(
                Circle().stroke(Color("lightGreen"), lineWidth: 2)
        )
    }
}

struct ProgressBars: View {
    var counter: Int
    var countTo: Int
 
    var body: some View {
        Circle()
            .fill(Color.clear)
            .frame(width: 50, height: 50)
            .overlay(
                Circle().trim(from:0, to: progress())
                    .stroke(
                        style: StrokeStyle(
                            lineWidth: 2,
                            lineCap: .round,
                            lineJoin:.round
                        )
                )
                    .foregroundColor(
                        (completed() ? Color("ProgressColor") : Color("ProgressColor"))
                )
                    .animation(.easeInOut(duration: 0.2), value: progress())

        )
    }
    
    func completed() -> Bool {
    
        return progress() == 1
    }
    
    func progress() -> CGFloat {
        return (CGFloat(counter) / CGFloat(countTo))
    }
}


struct CountdownView: View {
    @Binding var otpCode: String
    @Binding var showText: Bool
    @Binding var showResend: Bool
    @Binding var restartTimer: Bool

    @State private var counter: Int = 0

    var countTo: Int = UserDefaults.standard.integer(forKey: "TimeLimitRP") * 60

    // ADD THIS: Timer publisher
    private let timer = Timer.publish(every: 1, on: .main, in: .common).autoconnect()

    var body: some View {
        VStack {
            ZStack {
                ProgressTrack()
                ProgressBars(counter: counter, countTo: countTo)
                Clock(counter: counter, countTo: countTo)
            }
        }
        .onReceive(timer) { _ in
            if restartTimer {
                counter = 0
                showText = false
                showResend = false
                otpCode = ""
                restartTimer = false
            } else {
                if counter < countTo {
                    counter += 1
                }
                if counter == countTo {
                    showText = true
                    showResend = true
                    otpCode = ""
                }
            }
        }
        .onDisappear {
            // Stop the timer when the view disappears
            timer.upstream.connect().cancel()
        }
    }
}
