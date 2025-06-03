//
//  ConversationsClientWrapper.swift
//  ConversationsApp
//
//  Copyright © Twilio, Inc. All rights reserved.
//

import Foundation
import TwilioConversationsClient

class ConversationsClientWrapper: NSObject, ObservableObject {

    private(set) var conversationsClient: TwilioConversationsClient?
    
    var tokenWrapper: TokenWrapper.Type = TokenWrapperImpl.self
    
    // Provide a public method to access conversationsClient
//       func getConversationsClient() -> TwilioConversationsClient? {
//           return conversationsClient
//       }

    
    // MARK: - ConversationsProvider
    func create(chatacctoken : String , delegate: TwilioConversationsClientDelegate, completion: @escaping (LoginResult) -> Void) {
        create(chatacctoken : chatacctoken , tokenWrapper: TokenWrapperImpl.self, delegate: delegate, completion: completion)
    }

    func create(chatacctoken : String , tokenWrapper: TokenWrapper.Type, delegate: TwilioConversationsClientDelegate, completion: @escaping (LoginResult) -> Void) {
        self.tokenWrapper = tokenWrapper
        tokenWrapper.getConversationsAccessToken(chatToken : chatacctoken) { (result) in
           
            print("getresult",result)
            switch result {
            case .failure(let error):
              
                print("getConvsAccessTokenerror",error)
               // self.updateToken(shouldLogout: true)
                completion(.failure(error))
                
            case .success(let conversationsToken):
                
                print("conversationsToken",conversationsToken)
                
                
                let tokenParts = conversationsToken.components(separatedBy: ".")
                print("tokenParts",tokenParts)
                if tokenParts.count > 1, let payloadData = Data(base64Encoded: tokenParts[1], options: .ignoreUnknownCharacters) {
                    do {
                        if let payload = try JSONSerialization.jsonObject(with: payloadData, options: []) as? [String: Any],
                           let expirationTimestamp = payload["exp"] as? TimeInterval {
                            let expirationDate = Date(timeIntervalSince1970: expirationTimestamp)
                            print("Token Expiration Date:", expirationDate)
                        }
                    } catch {
                        print("Error decoding payload:", error)
                    }
                }

             
                print("quoterep",conversationsToken.replacingOccurrences(of: "\"", with: ""))
                let convToken = conversationsToken.replacingOccurrences(of: "\"", with: "")
                print("convToken",convToken)
       // SET CONVERSATION TOKEN FOR CHAT UPDATE
                
                UserDefaults.standard.setValue(convToken, forKey: "conversationToken")
             
                let properties = TwilioConversationsClientProperties()
               properties.commandTimeout = 10000
               
                properties.dispatchQueue = DispatchQueue(label: "TwilioConversationsDispatchQueue")
         
                TwilioConversationsClient.conversationsClient(withToken: convToken,
                                                              properties: properties,
                                                              delegate: delegate) { [weak self] result, client in
                    DispatchQueue.main.async {
                        
                        print("clientCreationResult",result.description)
                        print("clientCreationResult",result.resultText ?? "")
                       
                        if result.isSuccessful, let client = client  {
                             print("createclientsuccess",client)
                            self?.conversationsClient = client
                            AppModel.shared.client.conversationsClient = client

                            client.delegate = delegate

                            print("clientdelegate",delegate)

                            completion(.success)
                        } else {
                            print("createclientfailed",result.error!.localizedDescription)
                            completion(.failure(result.error!))
                            
                            
                            self?.updateToken(shouldLogout: true)
                        }
                    }
                }
            }
        }
    }
  
    func updateToken(shouldLogout: Bool) {

        let getAccessTokenOp = tokenWrapper.buildGetAccessTokenOperation()
        print("getAccessTokenOp",getAccessTokenOp)
      
        retry(operation: getAccessTokenOp) { result in
            switch result {
            case .success(let tokenUpdated):
                print("UPgetAcctoken",tokenUpdated)
                print("UPquoterep",tokenUpdated.replacingOccurrences(of: "\"", with: ""))
                
                
                // SET CONVERSATION UPDATED TOKEN FOR CHAT UPDATE
                         
                         UserDefaults.standard.setValue(tokenUpdated.replacingOccurrences(of: "\"", with: ""), forKey: "conversationToken")
                
                let tokenUp = tokenUpdated.replacingOccurrences(of: "\"", with: "")
                retry(operation: self.buildUpdateTokenOperation(with: tokenUp)) { updateTokenResult in
                    switch updateTokenResult {
                    case .success():
                        NSLog("Token updated")
                    case .failure(let error):
                        NSLog("Get token error during token update: \(error)")
                        self.handleUpdateTokenFailure(shouldLogout: shouldLogout)
                        
                    }
                }
            case.failure(let error):
                self.handleUpdateTokenFailure(shouldLogout: shouldLogout)
                NSLog("Get token error while getAccessToken during token update: \(error)")
            }
        }
    }

    func shutdown() {
     
        conversationsClient?.shutdown()
        conversationsClient = nil
    
    }

    // MARK: - Credentials Events
    func conversationsClientTokenWillExpire(_ client: TwilioConversationsClient) {
        print("TokenWillExpireclient",client)
        self.updateToken(shouldLogout: false)
    }

    func conversationsClientTokenExpired(_ client: TwilioConversationsClient) {
        print("TokenExpiredclient",client)
        self.updateToken(shouldLogout: false)
    }

    // MARK: - Helpers

    private func handleUpdateTokenFailure(shouldLogout: Bool) {
        print("handleUpdateTokenFailure1",shouldLogout)
        if !shouldLogout {
            print("handleUpdateTokenFailure2",shouldLogout)
            return
        }
        print("handleUpdateTokenFailure3",shouldLogout)
        NotificationCenter.default.post(name: NSNotification.Name(rawValue: "logoutRequired"), object: nil)
   
    }
    
    private func buildUpdateTokenOperation(with token: String) -> AsyncOperation<String, Void> {
        return AsyncOperation(input: token, task: { input, callback in

            if self.conversationsClient == nil {
                print("conversationsClient is nil, recreating...")
                let properties = TwilioConversationsClientProperties()
                properties.dispatchQueue = DispatchQueue(label: "TwilioConversationsDispatchQueue")

                print("AppModel.shared.",AppModel.shared )
                
                TwilioConversationsClient.conversationsClient(
                    withToken: token,
                    properties: properties,
                    delegate:AppModel.shared  // Make sure you store `delegate` during create()
                ) { [weak self] result, client in
                    if result.isSuccessful, let client = client {
                        self?.conversationsClient = client
                        print("Client recreated successfully.")
                        self?.updateClientToken(client: client, token: token, callback: callback)
                    } else {
                        print("Failed to recreate client: \(result.error?.localizedDescription ?? "Unknown error")")
                        callback(.failure(LoginError.unableToUpdateTokenError))
                    }
                }
            } else {
                print("Client already exists. Updating token directly.")
                self.updateClientToken(client: self.conversationsClient!, token: token, callback: callback)
            }
        })
    }

    private func updateClientToken(client: TwilioConversationsClient, token: String, callback: @escaping (Result<Void, Error>) -> Void) {
        client.updateToken(token) { result in
            if result.isSuccessful {
                print("Token updated successfully.")
                callback(.success(()))
            } else {
                print("Failed to update token: \(result.error?.localizedDescription ?? "Unknown error")")
                callback(.failure(LoginError.unableToUpdateTokenError))
            }
        }
    }

}
