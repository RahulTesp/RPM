//
//  SwiftUIPictureInPictureView.swift
//  RPM
//
//  Created by Tesplabs on 08/03/25.
//


import SwiftUI


struct SwiftUIPictureInPictureView: UIViewControllerRepresentable {
    static var controllerRef: PictureInPictureViewController?

    @EnvironmentObject var callManager: CallManager
    @EnvironmentObject var roomManager: RoomManager

    func makeUIViewController(context: Context) -> PictureInPictureViewController {
        let storyboard = UIStoryboard(name: "Main", bundle: nil)
        let controller = storyboard.instantiateViewController(withIdentifier: "PictureInPictureViewController") as! PictureInPictureViewController
        controller.callManager = callManager
        controller.roomManager = roomManager

        //  Store reference
        SwiftUIPictureInPictureView.controllerRef = controller
        return controller
    }

    func updateUIViewController(_ uiViewController: PictureInPictureViewController, context: Context) {}
}
