//
//  RemoteParticipantManagerDelegate.swift
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

protocol RemoteParticipantManagerDelegate: AnyObject {
    func participantDidChange(_ participant: RemoteParticipantManager)
}

/// Determines remote participant state and sends updates to delegate.
///
/// Also stores dominant speaker state received by the room so that participants contain all participant state
/// which is better for the UI. See `isDominantSpeaker` and `dominantSpeakerStartTime`.
class RemoteParticipantManager: NSObject {
    
    var identity: String { participant.identity }
    var isMicOn: Bool {
        guard let track = participant.remoteAudioTracks.first else { return false }
        
        return track.isTrackSubscribed && track.isTrackEnabled
    }
    
    var isVideoMuted: Bool {
        return !(cameraTrack?.isEnabled ?? false)
    }
    
    
    var cameraTrack: VideoTrack? {
        let track = participant.firstAvailableVideoTrack()
        if let remoteTrack = track as? RemoteVideoTrack {
            print("cameraTrack isSwitchedOff = \(remoteTrack.isSwitchedOff)")
        } else {
            print("No remote video track or not RemoteVideoTrack")
        }
        return track
    }


    private var cameraTrackSwitchedOff = false

    var isCameraTrackSwitchedOff: Bool {
        cameraTrackSwitchedOff
    }

    var presentationTrack: VideoTrack? {
           nil // or handle if you expect a separate screen share track
       }
    
    var isDominantSpeaker = false {
        didSet {
            dominantSpeakerStartTime = Date()
            delegate?.participantDidChange(self)
        }
    }
    var dominantSpeakerStartTime: Date = .distantPast
    var networkQualityLevel: NetworkQualityLevel {
        participant.networkQualityLevel
    }
    private let participant: RemoteParticipant
    private weak var delegate: RemoteParticipantManagerDelegate?

    var trackNameMapping: [String: String] = [:] //  Define it here
    
    init(participant: RemoteParticipant, delegate: RemoteParticipantManagerDelegate) {
        self.participant = participant
        self.delegate = delegate
        super.init()
        participant.delegate = self
        
        print("Remote Participant Connected: \(participant.identity)")
    }
}

extension RemoteParticipantManager: RemoteParticipantDelegate {
    func didSubscribeToVideoTrack(
        videoTrack: RemoteVideoTrack,
        publication: RemoteVideoTrackPublication,
        participant: RemoteParticipant
    ) {
  
        print("REMOTE Subscribed to video track: \(videoTrack.name) from \(participant.identity)")
        print("Received remote video track: \(videoTrack.description)")
     
        print("Remote video track is enabled: \(String(describing: videoTrack.isEnabled))")
        
        print("Subscribed to video track: \(publication.trackName) from \(participant.identity)")

        print(" REMOTE Subscribed to video track: \(videoTrack.name) from \(participant.identity)")
            print(" Track Name: \(publication.trackName), Track SID: \(publication.trackSid)")


        //  Lookup the expected name using Track SID
        let expectedName = trackNameMapping[publication.trackSid] ?? "Unknown"
        print(" Expected track type: \(expectedName)")

    
        self.delegate?.participantDidChange(self)
     
    }
    
    func didUnsubscribeFromVideoTrack(
        videoTrack: RemoteVideoTrack,
        publication: RemoteVideoTrackPublication,
        participant: RemoteParticipant
    ) {
        print("REMOTE Unsubscribed from video track: \(videoTrack.name) of \(participant.identity)")
        
        
        delegate?.participantDidChange(self)
    }
    func remoteParticipant(_ participant: RemoteParticipant, didPublishVideoTrack publication: RemoteVideoTrackPublication) {
        print("Remote Parti published video track: \(publication.trackName)")
    }

    func remoteParticipantDidEnableVideoTrack(
        participant: RemoteParticipant,
        publication: RemoteVideoTrackPublication
    ) {
        print("\(participant.identity) enabled their camera")
        delegate?.participantDidChange(self)
    }
    
    func remoteParticipantDidDisableVideoTrack(
        participant: RemoteParticipant,
        publication: RemoteVideoTrackPublication
    ) {
        
        
        print("\(participant.identity) disabled their camera")
        delegate?.participantDidChange(self)
    }

    func remoteParticipantSwitchedOnVideoTrack(participant: RemoteParticipant, track: RemoteVideoTrack) {
        
        print("\(participant.identity) switched ON their video track")
    
        cameraTrackSwitchedOff = false
        delegate?.participantDidChange(self)
    }

    func remoteParticipantSwitchedOffVideoTrack(participant: RemoteParticipant, track: RemoteVideoTrack) {
        
        print("\(participant.identity) switched OFF their video track")
  
        cameraTrackSwitchedOff = true
        delegate?.participantDidChange(self)
    }
    
    func didSubscribeToAudioTrack(
        audioTrack: RemoteAudioTrack,
        publication: RemoteAudioTrackPublication,
        participant: RemoteParticipant
    ) {
        print("Subscribed to audio track from \(participant.identity)")
        delegate?.participantDidChange(self)
    }
    
    func didUnsubscribeFromAudioTrack(
        audioTrack: RemoteAudioTrack,
        publication: RemoteAudioTrackPublication,
        participant: RemoteParticipant
    ) {
        print("Unsubscribed from audio track of \(participant.identity)")
        delegate?.participantDidChange(self)
    }

    func remoteParticipantDidEnableAudioTrack(
        participant: RemoteParticipant,
        publication: RemoteAudioTrackPublication
    ) {
        print("\(participant.identity) unmuted their microphone")
        delegate?.participantDidChange(self)
    }
    
    func remoteParticipantDidDisableAudioTrack(
        participant: RemoteParticipant,
        publication: RemoteAudioTrackPublication
    ) {
        print("\(participant.identity) muted their microphone")
        delegate?.participantDidChange(self)
    }
    
    func remoteParticipantNetworkQualityLevelDidChange(
        participant: RemoteParticipant,
        networkQualityLevel: NetworkQualityLevel
    ) {
        print("Network quality for \(participant.identity) changed to: \(networkQualityLevel.rawValue)")
        delegate?.participantDidChange(self)
    }
    
    
    func remoteParticipantDidPublishVideoTrack(participant: RemoteParticipant, publication: RemoteVideoTrackPublication) {
        print("DParticipant \(participant.identity) published video track: \(publication.trackSid)")

        if let remoteTrack = publication.remoteTrack {
            print("DReceived remote video track: \(remoteTrack)")
        } else {
            print("DRemote track is nil!")
        }
    }
    
}

extension RemoteParticipant {
    func firstAvailableVideoTrack() -> RemoteVideoTrack? {
        for track in remoteVideoTracks {
            print("Found video track: \(track.trackName)")
            return track.remoteTrack // First available video track
        }
        print(" No video tracks available")
        return nil
    }
}
