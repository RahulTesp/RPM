
//
//  CallManager.swift
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



import CallKit
import Combine
import TwilioVideo

class CallManager: NSObject, ObservableObject {
    let connectPublisher = PassthroughSubject<Void, Never>()
    let disconnectPublisher = PassthroughSubject<Error?, Never>()
    
    private let controller = CXCallController(queue: .main)
    private let provider: CXProvider
    private let audioDevice = DefaultAudioDevice()
    private var roomManager: RoomManager!
    private var localParticipant: LocalParticipantManager { roomManager.localParticipant }
    private var callUUID: UUID?
    private var subscriptions = Set<AnyCancellable>()
    let remoteParticipantDisconnectPublisher = PassthroughSubject<RemoteParticipantManager, Never>()
    static var isAudioDeviceSet = false
    
    override init() {
        print("Initializing CallManager")

        let configuration = CXProviderConfiguration()
        configuration.maximumCallGroups = 1
        configuration.maximumCallsPerCallGroup = 1
        configuration.supportsVideo = true
        configuration.supportedHandleTypes = [.generic]

        if let icon = UIImage(named: "CallKitIcon") {
            configuration.iconTemplateImageData = icon.pngData()
            print("CallKit icon set successfully")
        } else {
            print("Warning: CallKit icon not found")
        }
        
        provider = CXProvider(configuration: configuration)
        super.init()
        provider.setDelegate(self, queue: nil)
        print("CXProvider delegate set")

        TwilioVideoSDK.setLogLevel(.info)
        print("TwilioVideoSDK log level set to info")

        if !Self.isAudioDeviceSet {
            TwilioVideoSDK.audioDevice = audioDevice
            Self.isAudioDeviceSet = true
            print("TwilioVideoSDK audioDevice set")
        } else {
            print("TwilioVideoSDK audioDevice was already set")
        }
        
        print("TwilioVideoSDK.audioDevice:", TwilioVideoSDK.audioDevice)
    }

    
    func configure(roomManager: RoomManager) {
        print("Configuring CallManager with RoomManager")
        self.roomManager = roomManager
        
        roomManager.roomConnectPublisher
            .sink { [weak self] in
                print("Room connected")
                guard let self = self else { return }
                self.provider.reportOutgoingCall(with: self.callUUID!, connectedAt: nil)
                self.connectPublisher.send()
            }
            .store(in: &subscriptions)
        
        roomManager.roomDisconnectPublisher
            .sink { [ weak self] error in
                print("Room disconnected with error: \(String(describing: error))")
                guard let self = self, let error = error else { return }
                self.provider.reportCall(with: self.callUUID!, endedAt: nil, reason: .failed)
                self.handleError(error)
            }
            .store(in: &subscriptions)
    }
    
    func connect(roomName: String) {
        print("Attempting to connect to room: \(roomName)")
     
        let videoPermission = AVCaptureDevice.authorizationStatus(for: .video)
        let audioPermission = AVAudioSession.sharedInstance().recordPermission
     
        guard videoPermission == .authorized, audioPermission == .granted else {
            print("Permissions not granted")
     
            AVCaptureDevice.requestAccess(for: .video) { videoGranted in
                AVAudioSession.sharedInstance().requestRecordPermission { audioGranted in
                    DispatchQueue.main.async { [weak self] in
                        guard videoGranted && audioGranted else {
                            print("User denied one or more permissions.")
                            return
                        }
                        // Only call the setup method once
                        self?.startCallKitTransaction(roomName: roomName)
                    }
                }
            }
            return
        }
     
        // Permissions already granted
        startCallKitTransaction(roomName: roomName)
    }
     
    // MARK: - Separate method for CallKit transaction
    private func startCallKitTransaction(roomName: String) {
        //  Prevent multiple active calls
        guard callUUID == nil else {
            print("Call already in progress, ignoring duplicate request")
            return
        }
     
        //  Activate audio session first
        do {
            try AVAudioSession.sharedInstance().setCategory(.playAndRecord, mode: .videoChat, options: [.allowBluetooth, .defaultToSpeaker])
            try AVAudioSession.sharedInstance().setActive(true)
        } catch {
            print("Failed to activate audio session: \(error)")
            return
        }
     
        //  Generate a unique UUID
        callUUID = UUID()
        let handle = CXHandle(type: .generic, value: roomName)
        let startCallAction = CXStartCallAction(call: callUUID!, handle: handle)
        startCallAction.isVideo = true
        let transaction = CXTransaction(action: startCallAction)
     
        print("Requesting CallKit transaction for call: \(callUUID!.uuidString)")
     
        //  Request transaction safely
        controller.request(transaction) { [weak self] error in
            if let error = error {
                print("Error starting call: \(error)")
                self?.handleError(error)
                self?.callUUID = nil // reset UUID on failure
            } else {
                print("Transaction request successful")
            }
        }
    }
    
    
    
    func setMute(isMuted: Bool) {
        if let callUUID = callUUID {
            let setMutedCallAction = CXSetMutedCallAction(call: callUUID, muted: isMuted)
            let transaction = CXTransaction(action: setMutedCallAction)

            controller.request(transaction) { error in
                if let error = error {
                    print("Error requesting mute: \(error)")
                }
            }
        } else {
            // Before joining call
            localParticipant.isMicOn = !isMuted
            if let micTrack = localParticipant.micTrack {
                micTrack.isEnabled = !isMuted
            }
        }
    }

    func disconnect() {
        print("Attempting to disconnect call")

        guard let callUUID = callUUID else {
            print("No active call UUID found. Triggering UI dismiss directly.")
            disconnectPublisher.send(nil)  // notify disconnect anyway
            return
        }

        let endCallAction = CXEndCallAction(call: callUUID)
        let transaction = CXTransaction(action: endCallAction)

        controller.request(transaction) { [weak self] error in
            if let error = error {
                print("Error disconnecting call: \(error)")
                self?.handleError(error)  // this calls disconnectPublisher.send(error)
            } else {
                // Success, notify disconnect with nil error
                self?.disconnectPublisher.send(nil)
            }
        }
    }
    
    private func cleanUp() {
        print("Cleaning up call")
        callUUID = nil
        audioDevice.isEnabled = false
        roomManager.disconnect()
        localParticipant.isMicOn = false
        localParticipant.isCameraOn = false
    }
    
    private func handleError(_ error: Error) {
        print("Handling error: \(error)")
        cleanUp()
        disconnectPublisher.send(error)
    }
}

extension CallManager: CXProviderDelegate {
    func providerDidReset(_ provider: CXProvider) {
        print("Provider did reset")
    }

    func provider(_ provider: CXProvider, perform action: CXStartCallAction) {
        print("Performing CXStartCallAction for room: \(action.handle.value)")
        guard let roomManager = roomManager else {
            action.fail()
            print("Failed to start call: RoomManager is nil")
            return
        }
        
        //  Retrieve the saved token from UserDefaults
           guard let videoCallAccessToken = UserDefaults.standard.string(forKey: "videoCallToken") else {
               action.fail()
               print("Failed to start call: No access token found in UserDefaults")
               return
           }
        
        print("videoCallAccessTokenCALLING : \(videoCallAccessToken)")
        
        if let rawToken = UserDefaults.standard.string(forKey: "videoCallToken") {
            let cleanedToken = rawToken.trimmingCharacters(in: CharacterSet(charactersIn: "\""))
            print("Cleaned Token: \(cleanedToken)")

            provider.reportOutgoingCall(with: action.callUUID, startedConnectingAt: nil)
            roomManager.connect(roomName: action.handle.value, accessToken: cleanedToken, uuid: action.callUUID)
            action.fulfill()
        }
    }
    
    func provider(_ provider: CXProvider, perform action: CXEndCallAction) {
        print("Performing CXEndCallAction for call UUID: \(action.callUUID)")
        guard action.callUUID == callUUID else {
            print("Failed to end call: UUID mismatch")
            action.fail()
            return
        }
        cleanUp()
        disconnectPublisher.send(nil)
        action.fulfill()
    }
    
    func provider(_ provider: CXProvider, perform action: CXSetHeldCallAction) {
        print("Performing CXSetHeldCallAction: isOnHold = \(action.isOnHold)")
        guard action.callUUID == callUUID else {
            print("Failed to hold call: UUID mismatch")
            action.fail()
            return
        }
        localParticipant.setHold(isOnHold: action.isOnHold)
        action.fulfill()
    }
    
    func provider(_ provider: CXProvider, perform action: CXSetMutedCallAction) {
        print("Performing CXSetMutedCallAction: isMuted = \(action.isMuted)")
        
        localParticipant.isMicOn = !action.isMuted

        if let micTrack = localParticipant.micTrack {
            micTrack.isEnabled = !action.isMuted
            print(" MicTrack is now enabled: \(micTrack.isEnabled)")
        } else {
            print(" Warning: micTrack is nil during mute toggle")
        }

        action.fulfill()
    }

    func provider(_ provider: CXProvider, didActivate audioSession: AVAudioSession) {
        print("Audio session activated")
        audioDevice.isEnabled = true
    }
    
    func provider(_ provider: CXProvider, didDeactivate audioSession: AVAudioSession) {
        print("Audio session deactivated")
        audioDevice.isEnabled = false
    }
}
