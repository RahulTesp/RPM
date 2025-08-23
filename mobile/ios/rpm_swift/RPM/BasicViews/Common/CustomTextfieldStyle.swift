//
//  CustomTextfieldStyle.swift
//  RPM
//
//  Created by Tesplabs on 16/11/23.
//

import Foundation
import SwiftUI
struct CustomTextfieldStyle1: ViewModifier {
    func body(content: Content) -> some View {
        content
            .font(Font.custom("Rubik-Regular", size: 16))
            .foregroundColor(.black)
            .frame(width: 150, height: 40, alignment: .leading)
            .padding(.horizontal, 5)
            .padding(.vertical, 5)
            .background(
                RoundedRectangle(cornerRadius: 8, style: .continuous)
                    .stroke(Color("textFieldBG"), lineWidth: 2)
                    .background(Color("textFieldBG"))
                    .cornerRadius(8)
            )
    }
}

extension View {
    func customTextfieldStyle1() -> some View {
        self.modifier(CustomTextfieldStyle1())
    }
}
struct CustomTextfieldStyle2: ViewModifier {
    func body(content: Content) -> some View {
        content
            .font(Font.custom("Rubik-Regular", size: 16))
            .foregroundColor(.black)
            .frame(width: 320, height: 40, alignment: .leading)
            .padding(.horizontal, 5)
            .padding(.vertical, 5)
            .background(
                RoundedRectangle(cornerRadius: 8, style: .continuous)
                    .stroke(Color("textFieldBG"), lineWidth: 2)
                    .background(Color("textFieldBG"))
                    .cornerRadius(8)
            )
    }
}

extension View {
    func customTextfieldStyle2() -> some View {
        self.modifier(CustomTextfieldStyle2())
    }
}
