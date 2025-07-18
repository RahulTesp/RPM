//
//  SwiftUIVideoView.swift
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
import TwilioVideo

/// A SwiftUI video view that is automatically removed from the video track when the view is no longer in use.
struct SwiftUIVideoView: UIViewRepresentable {
  
    @Binding var videoTrack: VideoTrack?
    @Binding var shouldMirror: Bool
    var fill: Bool = true

    func makeUIView(context: Context) -> VideoTrackStoringVideoView {
        let videoView = VideoTrackStoringVideoView()
        videoView.contentMode = fill ? .scaleAspectFill : .scaleAspectFit
        return videoView
    }

    func updateUIView(_ uiView: VideoTrackStoringVideoView, context: Context) {
        uiView.videoTrack = videoTrack
        uiView.shouldMirror = shouldMirror
        print("SwiftUIVideoView: updateUIView called with videoTrack: \(String(describing: videoTrack))")
    }
    


    static func dismantleUIView(_ uiView: VideoTrackStoringVideoView, coordinator: ()) {
        print("SwiftUIVideoView: dismantleUIView called, removing renderer from video track")
        uiView.videoTrack?.removeRenderer(uiView)
    }
}

/// A `VideoView` that stores a reference to the `VideoTrack` it renders.
///
/// This makes it easy to update view state when the `VideoTrack` changes. Just set the `VideoTrack` and the
/// view will handle`addRenderer` and `removeRenderer` automatically.
///
/// It also provides a `VideoTrack` reference to `SwiftUIVideoView` so that `SwiftUIVideoView` can
/// remove the `VideoView` from the `VideoTrack` when `dismantleUIView` is called.`
class VideoTrackStoringVideoView: VideoView {
    var videoTrack: VideoTrack? {
        didSet {
            guard oldValue != videoTrack else {
                print("VideoTrackStoringVideoView: videoTrack is the same, no update needed")
                return }
            
            
            if let oldTrack = oldValue {
                          print("VideoTrackStoringVideoView: Removing renderer from old track: \(oldTrack)")
                          oldTrack.removeRenderer(self)
                      }
            
        //    oldValue?.removeRenderer(self)
            
            if let videoTrack = videoTrack {
                print("VideoTrackStoringVideoView: Adding renderer to new track: \(videoTrack)")
                videoTrack.addRenderer(self)
            
        } else {
                       print("VideoTrackStoringVideoView: New videoTrack is nil")
                   }
        }
    }
}
