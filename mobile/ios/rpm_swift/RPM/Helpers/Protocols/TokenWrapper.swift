//
//  TokenWrapperProtocol.swift
//  ConversationsApp
//
//  Copyright © Twilio, Inc. All rights reserved.
//

import Foundation

protocol TokenWrapper: AnyObject {

    static func getTokenUrlFromEnv(identity: String, password: String) -> URL?
    static func getTokenUrlFromDefaults(identity: String, password: String) -> URL?
    static func getConversationsAccessToken(chatToken : String , completion: @escaping (Result<String, LoginError>) -> Void)
    static func buildGetAccessTokenOperation() -> AsyncOperation<Credentials, String>
}
