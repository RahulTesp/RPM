//
//  SwiftUIPictureInPictureView.swift
//  RPM
//
//  Created by Tesplabs on 08/03/25.
//


import SwiftUI

struct SwiftUIPictureInPictureView: UIViewControllerRepresentable {
    @EnvironmentObject var callManager: CallManager
    @EnvironmentObject var roomManager: RoomManager
    
    func makeUIViewController(context: Context) -> PictureInPictureViewController {
        print("PictureView")
        let storyboard = UIStoryboard(name: "Main", bundle: nil)
        let controller = storyboard.instantiateViewController(withIdentifier: "PictureInPictureViewController") as! PictureInPictureViewController
        controller.callManager = callManager
        controller.roomManager = roomManager
        return controller
    }

    func updateUIViewController(_ uiViewController: PictureInPictureViewController, context: Context) {

    }
}
