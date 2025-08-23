//
//  RoomView.swift
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

import SwiftUI
import Combine

/// Room screen that is shown when a user connects to a video room.
struct RoomView: View {
    @EnvironmentObject var viewModel: RoomViewModel
    @EnvironmentObject var speakerLayoutViewModel: SpeakerLayoutViewModel
    @EnvironmentObject var mediaSetupViewModel: MediaSetupViewModel
    
    @EnvironmentObject var callManager: CallManager
    @EnvironmentObject var roomManager: RoomManager
    @EnvironmentObject var localParticipant: LocalParticipantManager
    
    @Environment(\.presentationMode) var presentationMode
    @Environment(\.horizontalSizeClass) var horizontalSizeClass
    @Environment(\.verticalSizeClass) var verticalSizeClass
    let roomName: String
    private let app = UIApplication.shared
    private let spacing: CGFloat = 6
    
    @State private var showExitConfirmation = false
    @Binding var isShowingPictureInPicture: Bool
    
    private var isPortraitOrientation: Bool {
        verticalSizeClass == .regular && horizontalSizeClass == .compact
    }
    
    var body: some View {
        GeometryReader { geometry in
            ZStack {
                Color.roomBackground.ignoresSafeArea()
                
                // TODO: Improve layout?
                PictureInPictureSourceView(participant: $speakerLayoutViewModel.dominantSpeaker)

                VStack(spacing: 0) {
                    VStack(spacing: 0) {
                       RoomStatusView(roomName: roomName)
                            .padding(.horizontal, spacing)
                        
                        switch viewModel.layout {
                        case .gallery:
                            GalleryLayoutView(spacing: spacing)
                        case .speaker:
                            SpeakerLayoutView(spacing: spacing)
                        }
                    }
                    .padding(.leading, geometry.safeAreaInsets.leading)
                    .padding(.trailing, geometry.safeAreaInsets.trailing)
                    .padding(.top, geometry.safeAreaInsets.top.isZero ? 3 : 0)
                    
                    RoomToolbar {
                        RoomToolbarButton(
                            image: Image(systemName: "phone.down.fill"),
                            role: .destructive
                        ) {
                            viewModel.disconnect()
                        }
                        MicToggleButton()
                        CameraToggleButton()
                        
                        Menu {
                            switch viewModel.layout {
                            case .gallery:
                                Button(
                                    action: { viewModel.switchToLayout(.speaker) },
                                    label: { Label("Speaker View", systemImage: "person") }
                                )
                            case .speaker:
                                Button(
                                    action: { viewModel.switchToLayout(.gallery) },
                                    label: { Label("Gallery View", systemImage: "square.grid.2x2") }
                                )
                            }
                        } label: {
                            RoomToolbarButton(image: Image(systemName: "ellipsis"))
                        }
                    }
                    
                    // For toolbar bottom that is below safe area
                    Color.background
                        .frame(height: geometry.safeAreaInsets.bottom)
                }
                .edgesIgnoringSafeArea([.horizontal, .bottom]) // So toolbar sides and bottom extend beyond safe area

               StatsContainerView(isShowingStats: $viewModel.isShowingStats)
                
                if viewModel.state == .connecting {
                    ProgressHUD(title: "Joining Meeting")
                }
            }
        }

        .onReceive(NotificationCenter.default.publisher(for: UIApplication.willResignActiveNotification)) { _ in
            print("App will resign active")

            if let pipVC = SwiftUIPictureInPictureView.controllerRef {
                pipVC.startPiPIfPossible()
            } else {
                print("⚠️ PiP controller is not available")
            }
        }

        .onReceive(NotificationCenter.default.publisher(for: Notification.Name("RemoteParticipantDisconnected"))) { notification in
            print(" Remote participant disconnected – triggering viewModel.disconnect()")
            
            // Optional: Log which participant disconnected
            if let userInfo = notification.userInfo,
               let identity = userInfo["participant"] as? String {
                print("Disconnected participant: \(identity)")
            }
            
            viewModel.disconnect()
        }

        .onAppear {
     
            app.isIdleTimerDisabled = true // Disable lock screen
            viewModel.connect(roomName: roomName)
            
            if !localParticipant.isMicOn {
                localParticipant.isMicOn = true
            }
            if !localParticipant.isCameraOn {
                localParticipant.isCameraOn = true
            }

        }
        .onDisappear {
            app.isIdleTimerDisabled = false
        }
   
        .alert(isPresented: $viewModel.isShowingError) {
          
            Alert(error: viewModel.error!) {
                print("viewModel.error")
            
            }
        }
        .onChange(of: viewModel.isShowingRoom) { isShowingRoom in
            print("isShowingRoom1" ,isShowingRoom)
            if !isShowingRoom {
                print("isShowingRoom2" ,isShowingRoom)
                presentationMode.wrappedValue.dismiss()
            }
        }
    }
}

extension RoomViewModel {
    static func stub(
        state: State = .connected,
        layout: Layout = .gallery,
        isShowingStats: Bool = false
    ) -> RoomViewModel {
        let viewModel = RoomViewModel()
        viewModel.state = state
        viewModel.layout = layout
        viewModel.isShowingStats = isShowingStats
        return viewModel
    }
}
