//
//  ParticipantViewModel.swift
//  RPM
//
//  Created by Tesplabs on 08/03/25.
//


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

import TwilioVideo
import Foundation
import Combine

class ParticipantViewModel: ObservableObject, Identifiable {
    let id = UUID()
    
    @Published var identity = ""
    @Published var displayName = ""
    @Published var isYou = false
    @Published var isMuted = false
    @Published var dominantSpeakerStartTime: Date = .distantPast
    @Published var isDominantSpeaker = false
    @Published var cameraTrack: VideoTrack? = nil
    @Published var shouldMirrorCameraVideo = false
    @Published var shouldFillCameraVideo = false
    @Published var networkQualityLevel: NetworkQualityLevel = .unknown

    /// Computed property — no need to make it @Published
    var isCameraTrackSwitchedOff: Bool {
        if isYou {
            let result = cameraTrack == nil || !(cameraTrack?.isEnabled ?? false)
            print("[LOCAL] isCameraTrackSwitchedOff = \(result)")
            return result
        } else {
            let result = (cameraTrack as? RemoteVideoTrack)?.isSwitchedOff ?? false
            print("[REMOTE] isCameraTrackSwitchedOff = \(result)")
            return result
        }
    }

    init() {}

    init(participant: LocalParticipantManager, shouldHideYou: Bool = false) {
        identity = participant.identity ?? ""
        displayName = identity + (shouldHideYou ? "" : " (You)")
        isYou = true
        isMuted = !participant.isMicOn

        if let cameraTrack = participant.cameraTrack, cameraTrack.isEnabled {
            self.cameraTrack = cameraTrack
        }
        
        shouldMirrorCameraVideo = true
        networkQualityLevel = participant.networkQualityLevel
    }

    init(participant: RemoteParticipantManager) {
        identity = participant.identity
        displayName = participant.identity
        isMuted = !participant.isMicOn
        isDominantSpeaker = participant.isDominantSpeaker
        dominantSpeakerStartTime = participant.dominantSpeakerStartTime
        cameraTrack = participant.cameraTrack
        shouldFillCameraVideo = true
        networkQualityLevel = participant.networkQualityLevel

        
        print("ParticipantViewModel init for \(identity) => isMuted: \(isMuted), isDominantSpeaker: \(isDominantSpeaker), cameraTrack: \(String(describing: cameraTrack)), isCameraTrackSwitchedOff: \(isCameraTrackSwitchedOff)")


    }
}

extension ParticipantViewModel: Equatable {
    static func == (lhs: ParticipantViewModel, rhs: ParticipantViewModel) -> Bool {
        lhs.identity == rhs.identity
    }
}
