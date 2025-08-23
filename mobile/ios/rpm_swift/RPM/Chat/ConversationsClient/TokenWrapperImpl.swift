//
//  TokenWrapperImpl.swift
//  ConversationsApp
//
//  Copyright © Twilio, Inc. All rights reserved.
//

import Foundation

struct Credentials {
    var username: String
    var password: String
}

extension TokenWrapper {

    private static func constructLoginUrl(_ url: String, identity: String, password: String) -> URL? {
        guard var urlComponents = URLComponents(string: url) else {
            return nil
        }

        var queryItems: [URLQueryItem] = urlComponents.queryItems ??  []
        queryItems.append(URLQueryItem(name: "identity", value: identity))
        queryItems.append(URLQueryItem(name: "password", value: password))

        urlComponents.queryItems = queryItems

        // Apple: "According to RFC 3986, the plus sign is a valid character within a query, and doesn't need to be percent-encoded."
        //      https://developer.apple.com/documentation/foundation/nsurlcomponents/1407752-queryitems
        // W3C: "Within the query string, the plus sign is reserved as shorthand notation for a space. Therefore, real plus signs must be encoded."
        //      https://www.w3.org/Addressing/URL/4_URI_Recommentations.html

        // Let's follow W3C and force '+' to be percent-encoded, as well as '?' and '/'.
        let allowedCharacterSet = CharacterSet(charactersIn: "+?/").inverted
        urlComponents.percentEncodedQuery = urlComponents.percentEncodedQuery?.addingPercentEncoding(withAllowedCharacters: allowedCharacterSet)

        return urlComponents.url
    }

    static func getTokenUrlFromEnv(identity: String, password: String) -> URL? {
        guard let tokenServiceUrl = ProcessInfo.processInfo.environment["ACCESS_TOKEN_SERVICE_URL"], !tokenServiceUrl.isEmpty else {
            return nil
        }
        return constructLoginUrl(tokenServiceUrl, identity: identity, password: password)
    }

    static func getTokenUrlFromDefaults(identity: String, password: String) -> URL? {
        // Get token service absolute URL from settings
        guard let tokenServiceUrl = UserDefaults.standard.string(forKey: "ACCESS_TOKEN_SERVICE_URL"), !tokenServiceUrl.isEmpty else {
            return nil
        }
        return constructLoginUrl(tokenServiceUrl, identity: identity, password: password)
    }

    
  static func getConversationsAccessToken(chatToken : String ,
                
    completion: @escaping (Result<String, LoginError>) -> Void) {
        
      print("getcnvacctoken",chatToken)
      
      let encodedchatToken = chatToken.addingPercentEncoding(withAllowedCharacters: .urlQueryAllowed) ?? ""
      
      print("encodedchatToken",encodedchatToken)
      
        
        guard let url = URL(string: NetworkManager.baseURL + "/api/comm/getchattoken?app=\("ios")") else {
            completion(.failure(.tokenServiceUrlIsInvalid))
            return
        }
      print("getchattoknurl",url)
            
        
        var request = URLRequest(url: url)
        
        request.addValue(encodedchatToken, forHTTPHeaderField: "Bearer")
        request.addValue("application/json", forHTTPHeaderField: "Content-Type")
        
        request.httpMethod = "GET"
        print("requestgetchattoken",request)
      
        
        let task = URLSession.shared.dataTask(with: request) { data, response, error in
       
            if let data = data,let _ = String(data: data,encoding:  .utf8)
            {
                
            }
            
            if let _ =  error {
                
                completion(.failure(.unavailable))
                
                return
                
            }
            
            guard let response = response as? HTTPURLResponse, response.statusCode == 200 else {                completion(.failure(.accessDenied))
           
                return
            }
            
            print("CHATresponse2",response)
            
            guard let response = response as? HTTPURLResponse else {
                completion(.failure(.unavailable))
                print("CHATresponse3",response)
                return
            }

            if response.statusCode == 401 {
                print("CHATresponse4",response)
                completion(.failure(.accessDenied))
                return
            }
            if response.statusCode == 404 {
                print("CHATresponse5",response)
                completion(.failure(.accessDenied))
                return
            }
            guard let data = data, let token = String(data: data, encoding: .utf8) else {
                completion(.failure(.unavailable))
         
                return
            }

            //  Extract token using JSONSerialization
                do {
                    if let json = try JSONSerialization.jsonObject(with: data, options: []) as? [String: Any],
                       let token = json["message"] as? String {
                        print("Extracted chat token:", token)
                        completion(.success(token))
                    } else {
                        completion(.failure(.accessDenied))
                    }
                } catch {
                    print("JSON parsing failed:", error)
                    completion(.failure(.accessDenied))
                }
            
        }
        print("taskgetchattoken",task)
     
        task.resume()
    }
    
    
    
    static func getConversationsAccessTokenUpdate(tkn: String,
                  
                  completion: @escaping (Result<String, LoginError>) -> Void) {
          
        print("getTokenUpdate",tkn)
          
          guard let url = URL(string: NetworkManager.baseURL + "/api/comm/regeneratechattoken?app=\("ios")") else {
              completion(.failure(.tokenServiceUrlIsInvalid))
              return
          }
     
          var request = URLRequest(url: url)
          
          request.addValue(tkn, forHTTPHeaderField: "Bearer")
          request.addValue("application/json", forHTTPHeaderField: "Content-Type")
          
          request.httpMethod = "GET"
          print("requestUpdatetoken",request)
        
          
          let task = URLSession.shared.dataTask(with: request) { data, response, error in
         
              if let data = data,let _ = String(data: data,encoding:  .utf8)
              {
                  
              }
              
              if let _ =  error {
                  
                  completion(.failure(.unavailable))
                  
                  return
                  
              }
              
              guard let response = response as? HTTPURLResponse, response.statusCode == 200 else {                completion(.failure(.accessDenied))
              
                  return
              }
              
              print("UpdateCHATresponse2",response)
              
              guard let response = response as? HTTPURLResponse else {
                  completion(.failure(.unavailable))
                  print("UpdateCHATresponse3",response)
                  return
              }

              if response.statusCode == 401 {
                  print("UpdateCHATresponse4",response)
                  completion(.failure(.accessDenied))
                  return
              }
              if response.statusCode == 404 {
                  print("UpdateCHATresponse5",response)
                  completion(.failure(.accessDenied))
                  return
              }
              guard let data = data, let token = String(data: data, encoding: .utf8) else {
                  completion(.failure(.unavailable))
              
                  return
              }

              //  Extract token using JSONSerialization
                    do {
                        if let json = try JSONSerialization.jsonObject(with: data, options: []) as? [String: Any],
                           let token = json["message"] as? String {
                            print("Extracted chat token:", token)
                            completion(.success(token))
                        } else {
                            completion(.failure(.accessDenied))
                        }
                    } catch {
                        print("JSON parsing failed:", error)
                        completion(.failure(.accessDenied))
                    }

          }
          print("taskgetchattoken",task)
       
          task.resume()
      }

    static func buildGetAccessTokenOperation() -> AsyncOperation<Credentials, String> {
        return AsyncOperation(
            input: Credentials(username: "", password: ""),
            task: { input, callback in
                self.getConversationsAccessTokenUpdate(tkn: UserDefaults.standard.string(forKey: "jsonwebtoken") ?? "") { result in
                    switch result {
                    
                    case .failure(let error):
                        print("buildGetAccessTokenOperationresultfailure",result)
                        print("buildGetAccessTokenOperationerror",error)
                        callback(.failure(error))
                    case.success(let token):
                        print("buildGetAccessTokenOperationresultsuccess",result)
                        print("buildGetAccessTokenOperationtoken",token)
                        callback(.success(token))
                    }
                }
            })
    }
}

class TokenWrapperImpl: TokenWrapper {}
