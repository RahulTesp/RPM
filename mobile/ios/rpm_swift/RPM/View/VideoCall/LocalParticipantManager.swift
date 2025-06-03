//
//  LocalParticipantManager.swift
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

import Combine
import TwilioVideo

/// Maintains local participant state and uses a publisher to notify subscribers of state changes.
///
/// The microphone and camera may be configured before or after connecting to a video room.
class LocalParticipantManager: NSObject, ObservableObject {
    let changePublisher = PassthroughSubject<LocalParticipantManager, Never>()
    @Published var isMicOn = false {
        didSet {
            guard oldValue != isMicOn else {
                return
            }

            if isMicOn {
                guard let micTrack = LocalAudioTrack(options: nil, enabled: true, name: TrackName.mic) else {
                    return
                }
                print(" Publishing Audio Track: \(micTrack.name)")
                self.micTrack = micTrack
                participant?.publishAudioTrack(micTrack)
            } else {
                guard let micTrack = micTrack else {
                    return
                }
                
                participant?.unpublishAudioTrack(micTrack)
                self.micTrack = nil
            }
            
            changePublisher.send(self)
        }
    }
    @Published var isCameraOn = false {
        didSet {
            guard oldValue != isCameraOn else { return }

            if isCameraOn {
                print(" Enabling existing video track")

                // If cameraManager already exists, just re-enable the track
                if let cameraManager = cameraManager {
                    cameraManager.track.isEnabled = true
                } else {
                    // First-time setup
                    guard let newCameraManager = CameraManager(position: cameraPosition) else {
                        print(" Failed to create CameraManager")
                        return
                    }
                    self.cameraManager = newCameraManager
                    newCameraManager.delegate = self
                    let publicationOptions = LocalTrackPublicationOptions(priority: .low)
                    participant?.publishVideoTrack(newCameraManager.track, publicationOptions: publicationOptions)
                }

            } else {
                print(" Disabling video track (not unpublishing)")
                cameraManager?.track.isEnabled = false
                // Do NOT unpublish or destroy cameraManager
            }

            changePublisher.send(self)
        }
    }


    func cleanUpTracks() {
        print("[LocalParticipantManager] Cleaning up local tracks")

        print("[LocalParticipantManager] micTrack is \(micTrack == nil ? "nil" : "NOT nil")")
        if let micTrack = micTrack {
            participant?.unpublishAudioTrack(micTrack)
            self.micTrack = nil
            print("[LocalParticipantManager] Unpublished and cleared micTrack")
        }

        print("[LocalParticipantManager] cameraManager is \(cameraManager == nil ? "nil" : "NOT nil")")
        if let cameraManager = cameraManager {
            participant?.unpublishVideoTrack(cameraManager.track)
            cameraManager.stop()
            self.cameraManager = nil
            print("[LocalParticipantManager] Unpublished video track and stopped camera capture")
        }

        participant = nil

        print("[LocalParticipantManager] Cleanup complete")
    }

    @Published var cameraPosition: AVCaptureDevice.Position = .front {
        didSet {
            cameraManager?.position = cameraPosition
        }
    }
    var networkQualityLevel: NetworkQualityLevel {
        participant?.networkQualityLevel ?? .unknown
    }
    var participant: LocalParticipant? {
        didSet {
            participant?.delegate = self
        }
    }
    var cameraTrack: LocalVideoTrack? { cameraManager?.track }
    private(set) var identity: String?
    private(set) var micTrack: LocalAudioTrack?
    private var cameraManager: CameraManager?
    
    func configure(identity: String) {
        self.identity = identity
    }
    
    func setHold(isOnHold: Bool) {
        micTrack?.isEnabled = !isOnHold
        cameraTrack?.isEnabled = !isOnHold
    }
}

extension LocalParticipantManager: LocalParticipantDelegate {
    func localParticipantDidFailToPublishVideoTrack(
        participant: LocalParticipant,
        videoTrack: LocalVideoTrack,
        error: Error
    ) {
        print("Failed to publish video track: \(error)")
    }
    
    func localParticipantDidFailToPublishAudioTrack(
        participant: LocalParticipant,
        audioTrack: LocalAudioTrack,
        error: Error
    ) {
        print("Failed to publish audio track: \(error)")
    }
    
    func localParticipantNetworkQualityLevelDidChange(
        participant: LocalParticipant,
        networkQualityLevel: NetworkQualityLevel
    ) {
        changePublisher.send(self)
    }
}

extension LocalParticipantManager: CameraManagerDelegate {
    func trackSourceWasInterrupted(track: LocalVideoTrack) {
        track.isEnabled = false
        changePublisher.send(self)
    }

    func trackSourceInterruptionEnded(track: LocalVideoTrack) {
        track.isEnabled = true
        changePublisher.send(self)
    }
}
