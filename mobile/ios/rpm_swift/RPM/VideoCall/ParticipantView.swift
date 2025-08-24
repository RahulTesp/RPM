////
////  ParticipantView.swift
////  RPM
////
////  Created by Tesplabs on 08/03/25.
////
//
//  Copyright (C) 2022 Twilio, Inc.
//
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//
//      http://www.apache.org/licenses/LICENSE-2.0
//
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
//

import SwiftUI
import TwilioVideo


struct ParticipantView: View {
    @ObservedObject var viewModel: ParticipantViewModel
    var body: some View {
        ZStack {
            Color.backgroundStronger

            if let _ = viewModel.cameraTrack, !viewModel.isCameraTrackSwitchedOff {
                //  VIDEO ON — show video
                ZStack {
                    if !viewModel.shouldFillCameraVideo {
                        Color.black
                    }

                    SwiftUIVideoView(
                        videoTrack: $viewModel.cameraTrack,
                        shouldMirror: $viewModel.shouldMirrorCameraVideo,
                        fill: viewModel.shouldFillCameraVideo
                    )
                }
            } else {
                //  VIDEO OFF — show paused UI
                ZStack {
                    Color.black

                    VStack {
                        Text("Video is paused")
                            .font(.headline)
                            .foregroundColor(.gray)
                            .padding()
                    }
                }
            }

            
            
            VStack {
                HStack {
                    Spacer()
                    
                    if viewModel.isMuted {
                        Image(systemName: "mic.slash")
                            .foregroundColor(.white)
                            .padding(9)
                            .background(Color.roomBackground.opacity(0.4))
                            .clipShape(Circle())
                            .padding(8)
                    }
                    else
                    {
                        Image(systemName: "microphone")
                        .foregroundColor(.white)
                        .padding(9)
                        .background(Color.roomBackground.opacity(0.4))
                        .clipShape(Circle())
                        .padding(8)
                        }
                }
                Spacer()
                
                HStack(alignment: .bottom) {
                    HStack(spacing: 12) {

                        
                        if viewModel.networkQualityLevel.rawValue >= 0 {
                            NetworkQualityView(level: viewModel.networkQualityLevel)
                                .frame(height: 12)
                        }
                    }
                    .padding(.horizontal, 6)
                    .padding(.vertical, 2)
                    .background(Color.roomBackground.opacity(0.7))
                    .cornerRadius(2)
                    
                    Spacer()
                }
                .padding(4)
            }

            VStack {
                if viewModel.isDominantSpeaker {
                    RoundedRectangle(cornerRadius: 3)
                        .stroke(Color.borderSuccessWeak, lineWidth: 4)
                }
            }
        }
        .cornerRadius(3)
    }
}

extension ParticipantViewModel {
    static func stub(
        identity: String = "Alice",
        isMuted: Bool = false,
        isDominantSpeaker: Bool = false,
        dominantSpeakerStartTime: Date = .distantPast,
        networkQualityLevel: NetworkQualityLevel = .five
    ) -> ParticipantViewModel {
        var viewModel = ParticipantViewModel()
        viewModel.isMuted = isMuted
        viewModel.identity = identity
        viewModel.displayName = identity
        viewModel.isDominantSpeaker = isDominantSpeaker
        viewModel.dominantSpeakerStartTime = dominantSpeakerStartTime
        viewModel.networkQualityLevel = networkQualityLevel
        return viewModel
    }
}
