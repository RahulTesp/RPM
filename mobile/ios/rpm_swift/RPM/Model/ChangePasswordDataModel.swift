//
//  ChangePasswordDataModel.swift
//  RPM
//
//  Created by Tesplabs on 26/12/23.
//

import Foundation


struct ChangePasswordDataModel: Codable {
    let status: String

    enum CodingKeys: String, CodingKey {
        case status = "Status"
    }
}
