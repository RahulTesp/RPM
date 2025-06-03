//
//  RPMNetworkManager.swift
//  RPM
//
//  Created by Prajeesh Prabhakar on 30/05/22.
//

import Foundation
import UIKit
import SocketIO
import SwiftUI
enum APIError: Error {
    case invalidURL
    case unableToComplete
    case invalidResponse
    case invalidUser
    case invalidPassword
    case invalidData
    case decodingError
    case lockedError
    case numberInvalidError
    case otpWrongError
    case noInternet
    case unauthorized
    
}

class NetworkManager: NSObject {
    
    static let shared           = NetworkManager()
    static let baseURL          =
    //"https://rpmappservicepreprod.azurewebsites.net"
    // "https://rpmwebapp.azurewebsites.net"
    // "https://c-lynxapi.azurewebsites.net"
    //  "https://rpmdevnew.azurewebsites.net"
    // "https://cx-dev-server.azurewebsites.net"
    // "https://cx-preprod-server.azurewebsites.net"
    // "https://md-preprod-server.azurewebsites.net"
    "https://rpm-dev-tespcare.azurewebsites.net"
    
    
    private override init() {}
    
    // NOTE : LOGIN API INTEGRATION
    
    func login(userName: String,
               password: String,
               completed: @escaping (Result<RPMLoginDataModel, APIError>) -> Void) {
        guard let url = URL(string: NetworkManager.baseURL + "/api/authorization/Patientlogin") else {
            completed(.failure(.invalidURL))
            return
        }
        
        let bodyData = "{\"UserName\": \"\(userName)\", \"Password\": \"\(password)\"}"
        let postData = bodyData.data(using: .utf8)
        
        var request = URLRequest(url: url)
        request.addValue("application/json", forHTTPHeaderField: "Content-Type")
        
        request.httpMethod = "POST"
        request.httpBody = postData
        
        let task = URLSession.shared.dataTask(with: request) { data, response, error in
            
            
            if let error = error as? URLError {
                print("URLError:", error.code)
                
                switch error.code {
                case .notConnectedToInternet, .timedOut, .cannotFindHost, .cannotConnectToHost:
                    completed(.failure(.noInternet)) // Create a new case for this
                default:
                    completed(.failure(.unableToComplete))
                }
                return
            }
            
            
            let statusCode: Int? = {
                if let httpUrlResponse = response as? HTTPURLResponse {
                    return httpUrlResponse.statusCode
                }
                return nil
            }()
            
            
            guard   statusCode != 401  else{
                
                completed(.failure(.invalidPassword))
                
                
                return
            }
            
            guard   statusCode != 403 else {
                
                completed(.failure(.lockedError))
                
                return
            }
            
            guard   statusCode != 503 else {
                
                completed(.failure(.numberInvalidError))
                
                return
            }
            
            guard let data = data
                    
            else {
                print(data as Any)
                completed(.failure(.invalidData))
                return
            }
            
            do {
                let decoder = JSONDecoder()
                let decodedResponse = try decoder.decode(RPMLoginDataModel.self, from: data)
                print("decodedResponselogin",decodedResponse)
                
                if(decodedResponse.roles?[0].id == 7)
                    
                {
                    if(decodedResponse.mfa == false)
                    {
                        
                        let pgmTypedemo = UserDefaults.standard.set(decodedResponse.roles?[0].programName, forKey: "pgmTypeString" )
                        
                        completed(.success(decodedResponse))
                    }
                    else
                    {
                        let pgmTypedemo = UserDefaults.standard.set(decodedResponse.roles?[0].programName, forKey: "pgmTypeString" )
                        UserDefaults.standard.set(decodedResponse.timeLimit, forKey: "TimeLimit" )
                        
                        completed(.success(decodedResponse))
                        
                    }
                    
                }
                else
                {
                    completed(.failure(.invalidUser))
                }
                
            } catch {
                completed(.failure(.invalidUser))
                
            }
        }
        
        task.resume()
    }
    
    // NOTE : VERIFY OTP API INTEGRATION
    
    func otpVerification(userName: String,
                         otp: String,
                         completed: @escaping (Result<RPMLoginDataModel, APIError>) -> Void) {
        guard let url = URL(string: NetworkManager.baseURL + "/api/authorization/UserloginVerifiy") else {
            completed(.failure(.invalidURL))
            return
        }
        print("verifyOtpAPICALL")
        
        let bodyData = "{\"UserName\": \"\(userName)\", \"OTP\": \"\(otp)\"}"
        let postData = bodyData.data(using: .utf8)
        
        var request = URLRequest(url: url)
        
        request.addValue((UserDefaults.standard.string(forKey: "jsonwebtokenold") ?? ""), forHTTPHeaderField: "Bearer")
        
        request.addValue("application/json", forHTTPHeaderField: "Content-Type")
        
        request.httpMethod = "POST"
        request.httpBody = postData
        
        
        
        let task = URLSession.shared.dataTask(with: request) { data, response, error in
            
            
            if let data = data,let _ = String(data: data,encoding:  .utf8)
            {
                
            }
            
            
            
            if let error = error as? URLError {
                print("URLError:", error.code)
                
                switch error.code {
                case .notConnectedToInternet, .timedOut, .cannotFindHost, .cannotConnectToHost:
                    completed(.failure(.noInternet)) // Create a new case for this
                default:
                    completed(.failure(.unableToComplete))
                }
                return
            }
            
            
            let statusCode: Int? = {
                if let httpUrlResponse = response as? HTTPURLResponse {
                    return httpUrlResponse.statusCode
                }
                return nil
            }()
            
            guard   statusCode != 401  else{
                
                completed(.failure(.otpWrongError))
                
                return
            }
            
            guard   statusCode != 403 else {
                
                completed(.failure(.lockedError))
                
                return
            }
            
            
            guard let data = data
                    
            else {
                print(data as Any)
                completed(.failure(.invalidData))
                return
            }
            
            do {
                let decoder = JSONDecoder()
                let decodedResponse = try decoder.decode(RPMLoginDataModel.self, from: data)
                print("RPMOTPDataModeldecodedResponse",decodedResponse)
                
                let pgmTypedemo = UserDefaults.standard.set(decodedResponse.roles?[0].programName, forKey: "pgmTypeString" )
                
                if(decodedResponse.roles?[0].id == 7)
                {
                    completed(.success(decodedResponse))
                }
                else
                {
                    completed(.failure(.invalidPassword))
                }
                
            } catch {
                
                completed(.failure(.invalidData))
                
            }
        }
        
        task.resume()
        
    }
    
    
    
    
    // NOTE: GENERATE OTP API CONNECTION
    func generateOTP(userName: String,
                     
                     completed: @escaping (Result<RPMGeneratePasswordDataModel, APIError>) -> Void) {
        guard let url = URL(string: NetworkManager.baseURL + "/api/authorization/forgetpassword?username=\(userName)") else {
            completed(.failure(.invalidURL))
            return
        }
        
        print("userName")
        print(userName)
        var request = URLRequest(url: url)
        request.addValue("application/json", forHTTPHeaderField: "Content-Type")
        
        request.httpMethod = "GET"
        
        let task = URLSession.shared.dataTask(with: request) { data, response, error in
            
            if let _ =  error {
                completed(.failure(.unableToComplete))
                return
            }
            print("genotpresponse")
            print(response)
            
            let statusCode: Int? = {
                if let httpUrlResponse = response as? HTTPURLResponse {
                    return httpUrlResponse.statusCode
                }
                return nil
            }()
            
            print("statusCode: \(statusCode)")
            
            guard   statusCode != 404 else {
                
                completed(.failure(.unableToComplete))
                
                return
            }
            
            guard   statusCode != 503 else {
                
                completed(.failure(.numberInvalidError))
                
                return
            }
            
            
            guard let data = data
                    
            else {
                print(data as Any)
                completed(.failure(.invalidData))
                return
            }
            
            
            do {
                let decoder = JSONDecoder()
                let decodedResponse = try decoder.decode(RPMGeneratePasswordDataModel.self, from: data)
                print("decodedResponseRPMGeneratePasswordDataModel",decodedResponse)
                
                UserDefaults.standard.set(decodedResponse.timeLimit, forKey: "TimeLimitRP" )
                
                completed(.success(decodedResponse))
                
            } catch {
                completed(.failure(.decodingError))
                
            }
        }
        
        task.resume()
    }
    
    
    
    // NOTE : RESET PASSWORD API INTEGRATION
    
    func resetPassword(userName: String,
                       otp: String, password : String,
                       completed: @escaping (Result<String, APIError>) -> Void) {
        guard let url = URL(string: NetworkManager.baseURL + "/api/authorization/userresetpasswordverifiy") else {
            completed(.failure(.invalidURL))
            return
        }
        
        let bodyData = "{\"UserName\": \"\(userName)\", \"OTP\": \"\(otp)\", \"Password\": \"\(password)\"}"
        let postData = bodyData.data(using: .utf8)
        
        var request = URLRequest(url: url)
        print("jsonwebtokenRP")
        print((UserDefaults.standard.string(forKey: "jsonwebtokenRP") ?? ""))
        request.addValue( (UserDefaults.standard.string(forKey: "jsonwebtokenRP") ?? ""), forHTTPHeaderField: "Bearer")
        
        request.addValue("application/json", forHTTPHeaderField: "Content-Type")
        
        request.httpMethod = "POST"
        request.httpBody = postData
        
        let task = URLSession.shared.dataTask(with: request) { data, response, error in
            
            if let data = data,let _ = String(data: data,encoding:  .utf8)
            {
                
            }
            
            if let _ =  error {
                completed(.failure(.unableToComplete))
                return
            }
            
            let statusCode: Int? = {
                if let httpUrlResponse = response as? HTTPURLResponse {
                    return httpUrlResponse.statusCode
                }
                return nil
            }()
            
            guard   statusCode != 401  else{
                
                completed(.failure(.otpWrongError))
                
                return
            }
            
            guard   statusCode != 403 else {
                
                completed(.failure(.lockedError))
                
                return
            }
            
            guard let data = data
                    
            else {
                print(data as Any)
                completed(.failure(.invalidData))
                return
            }
            
            do {
                let decoder = JSONDecoder()
                let decodedResponse = try decoder.decode(String.self, from: data)
                
                completed(.success(decodedResponse))
                
            } catch {
                
                completed(.failure(.invalidData))
                
            }
        }
        
        task.resume()
        
    }
    
    
    // NOTE : CHANGE PASSWORD API INTEGRATION
    
    
    func changePwd(tkn: String, userName: String,
                   oldPassword: String,    newPassword: String,
                   completed: @escaping (Result<ChangePasswordDataModel, APIError>) -> Void) {
        guard let url = URL(string: NetworkManager.baseURL + "/api/authorization/updatepassword") else {
            completed(.failure(.invalidURL))
            return
        }
        
        let bodyData = "{\"UserName\": \"\(userName)\", \"OldPassword\": \"\(oldPassword)\" , \"NewPassword\": \"\(newPassword)\"}"
        
        let postData = bodyData.data(using: .utf8)
        
        var request = URLRequest(url: url)
        request.addValue(tkn, forHTTPHeaderField: "Bearer")
        
        request.addValue("application/json", forHTTPHeaderField: "Content-Type")
        
        request.httpMethod = "POST"
        request.httpBody = postData
        
        let task = URLSession.shared.dataTask(with: request) { data, response, error in
            
            if let data = data,let _ = String(data: data,encoding:  .utf8)
            {
                
            }
            
            if let _ =  error {
                
                completed(.failure(.unableToComplete))
                
                return
                
            }
            
            guard let response = response as? HTTPURLResponse, response.statusCode == 200 else {                completed(.failure(.invalidResponse))
                
                return
            }
            print("changePwdResponse",response)
            guard let data = data
                    
            else {
                print(data as Any)
                completed(.failure(.invalidData))
                return
            }
            
            do {
                let decoder = JSONDecoder()
                let decodedResponse = try decoder.decode(ChangePasswordDataModel.self, from: data)
                print("CHANGEpwddecodedResponse",decodedResponse)
                
                completed(.success(decodedResponse))
            } catch {
                completed(.failure(.invalidData))
                
            }
        }
        print("\ntask\n")
        print(task)
        
        task.resume()
    }
    
    
    //  NOTE : LOGOUT INTEGRATION
    
    
    func logOut(tkn: String,
                
                completed: @escaping (Result<String, APIError>) -> Void) {
        guard let url = URL(string: NetworkManager.baseURL + "/api/authorization/logout") else {
            completed(.failure(.invalidURL))
            return
        }
        
        print("tknLOOGOUT",tkn)
        
        var request = URLRequest(url: url)
        
        request.addValue(tkn, forHTTPHeaderField: "Bearer")
        request.addValue("application/json", forHTTPHeaderField: "Content-Type")
        
        request.httpMethod = "POST"
        print("requestLOOGOUT",request)
        
        
        let task = URLSession.shared.dataTask(with: request) { data, response, error in
            
            print("errorLOOGOUT",error)
            print("responseLOOGOUT",response)
            if let data = data,let _ = String(data: data,encoding:  .utf8)
            {
                
            }
            
            if let error = error as? URLError {
                print("URLError:", error.code)
                
                switch error.code {
                case .notConnectedToInternet, .timedOut, .cannotFindHost, .cannotConnectToHost:
                    completed(.failure(.noInternet)) // Create a new case for this
                default:
                    completed(.failure(.unableToComplete))
                }
                return
            }
            
            
            guard let response = response as? HTTPURLResponse, response.statusCode == 200 else {                completed(.failure(.invalidResponse))
                
                return
            }
            print("responseLOGOUT",response)
            guard let data = data
                    
            else {
                print(data as Any)
                completed(.failure(.invalidData))
                return
            }
            
            if let responseString = String(data: data, encoding: .utf8) {
                print("decodedResponseLOGOUT", responseString)
                completed(.success(responseString))
            } else {
                completed(.failure(.invalidData))
            }
            
        }
        print("taskLOOGOUT",task)
        
        task.resume()
    }
    
    
    func saveFirebaseToken(fbToken: String, accessToken: String , completed: @escaping (Result<Bool, APIError>) -> Void) {
        // Construct the URL with the token as a query parameter
        guard let url = URL(string: "\(NetworkManager.baseURL)/api/notification/insertfirebasetoken?Token=\(fbToken)") else {
            completed(.failure(.invalidURL))
            return
        }
        
        var request = URLRequest(url: url)
        request.httpMethod = "POST"  // Assuming it's a POST request
        
        
        
        request.addValue(accessToken, forHTTPHeaderField: "Bearer")
        
        
        let task = URLSession.shared.dataTask(with: request) { data, response, error in
            
            if let error = error as? URLError {
                print("URLError:", error.code)
                
                switch error.code {
                case .notConnectedToInternet, .timedOut, .cannotFindHost, .cannotConnectToHost:
                    completed(.failure(.noInternet)) // Create a new case for this
                default:
                    completed(.failure(.unableToComplete))
                }
                return
            }
            
            guard let httpResponse = response as? HTTPURLResponse, (200...299).contains(httpResponse.statusCode) else {
                completed(.failure(.invalidResponse))
                return
            }
            
            DispatchQueue.main.async {
                completed(.success(true))
            }
        }
        
        task.resume()
    }
    
    
    func fetchVideoCallToken(roomName: String, completed: @escaping (Result<String, APIError>) -> Void) {
        guard let encodedRoomName = roomName.addingPercentEncoding(withAllowedCharacters: .urlQueryAllowed),
              let url = URL(string: NetworkManager.baseURL + "/api/comm/joinroom?room=\(encodedRoomName)") else {
            print(" Invalid URL")
            completed(.failure(.invalidURL))
            return
        }
        
        print(" API Request URL: \(url)")
        print(" Room Name (Encoded): \(encodedRoomName)")
        
        var request = URLRequest(url: url)
        request.httpMethod = "GET"
        
        print("VCjsonwebTOKEN",UserDefaults.standard.string(forKey: "jsonwebtoken"))
        
        if let accessToken = UserDefaults.standard.string(forKey: "jsonwebtoken") {
            print(" Using Access Token: \(accessToken)")
            
            // Add token in headers with "Bearer" as the key
            request.addValue(accessToken, forHTTPHeaderField: "Bearer")
        } else {
            print(" No Access Token found in UserDefaults")
            completed(.failure(.invalidData))
            return
        }
        
        print("Request: \(request)")
        
        let task = URLSession.shared.dataTask(with: request) { data, response, error in
            if let error = error {
                print(" API Error: \(error.localizedDescription)")
                completed(.failure(.unableToComplete))
                return
            }
            
            guard let httpResponse = response as? HTTPURLResponse else {
                print(" Invalid HTTP Response")
                completed(.failure(.invalidResponse))
                return
            }
            
            print(" HTTP Response Status Code: \(httpResponse.statusCode)")
            
            guard httpResponse.statusCode == 200 else {
                print("Valid response code: \(httpResponse.statusCode)")
                completed(.failure(.invalidResponse))
                return
            }
            
            guard let data = data else {
                print(" No data received from API")
                completed(.failure(.invalidData))
                return
            }
            
            print(" Raw Response Data: \(String(data: data, encoding: .utf8) ?? "Invalid Data")")
            
            do {
                let token = String(data: data, encoding: .utf8)?.trimmingCharacters(in: .whitespacesAndNewlines) ?? ""
                
                if token.isEmpty {
                    print(" Empty token received")
                    completed(.failure(.invalidData))
                } else {
                    print(" Received Video Call Token: \(token)")
                    completed(.success(token))
                }
            } catch {
                print(" Error decoding response as String: \(error.localizedDescription)")
                completed(.failure(.invalidData))
            }
            
        }
        
        task.resume()
    }
    
    
    func rejectCall(completed: @escaping (Result<Bool, APIError>) -> Void) {
        
        guard let rejectToUser = UserDefaults.standard.string(forKey: "callRejectToUserName"),
              let rejectTokenId = UserDefaults.standard.string(forKey: "callRejectTokenId") else {
            print(" Missing UserDefaults values for call rejection")
            
            return
        }
        
        // Construct the URL with the token as a query parameter
        guard let url = URL(string: "\(NetworkManager.baseURL)/api/comm/notifibyfirebase?toUser=\(rejectToUser)&tokenid=\(rejectTokenId)") else {
            completed(.failure(.invalidURL))
            return
        }
        
        var request = URLRequest(url: url)
        request.httpMethod = "POST"  // Assuming it's a POST request
        
        
        if let accessToken = UserDefaults.standard.string(forKey: "jsonwebtoken") {
            print(" Using Access Token: \(accessToken)")
            
            // Add token in headers with "Bearer" as the key
            request.addValue(accessToken, forHTTPHeaderField: "Bearer")
        } else {
            print(" No Access Token found in UserDefaults")
            completed(.failure(.invalidData))
            return
        }
        
        let task = URLSession.shared.dataTask(with: request) { data, response, error in
            if let _ = error {
                completed(.failure(.unableToComplete))
                return
            }
            
            guard let httpResponse = response as? HTTPURLResponse, (200...299).contains(httpResponse.statusCode) else {
                completed(.failure(.invalidResponse))
                return
            }
            
            DispatchQueue.main.async {
                completed(.success(true))
            }
        }
        
        task.resume()
    }
    
    
}



class ProfileManager: NSObject {
    
    static let shared           = ProfileManager()
    static let baseURL          =
    // "https://rpmappservicepreprod.azurewebsites.net"
    // "https://rpmwebapp.azurewebsites.net"
    //"https://c-lynxapi.azurewebsites.net"
    // "https://rpmdevnew.azurewebsites.net"
    //"https://cx-dev-server.azurewebsites.net"
    //"https://cx-preprod-server.azurewebsites.net"
    //"https://md-preprod-server.azurewebsites.net"
    "https://rpm-dev-tespcare.azurewebsites.net"
    
    private override init() {}
    
    //   NOTE : GET PATIENT PROFILE
    
    func profilelists(tkn: String,
                      completed: @escaping (Result<ProgramInfo, APIError>) -> Void) {
        guard let url = URL(string: ProfileManager.baseURL + "/api/patients/getpatient") else {
            completed(.failure(.invalidURL))
            return
        }
        
        var request = URLRequest(url: url)
        
        request.addValue(tkn, forHTTPHeaderField: "Bearer")
        print("request")
        print(request)
        
        let task = URLSession.shared.dataTask(with: request) { data, response, error in
            
            if let data = data,let _ = String(data: data,encoding:  .utf8)
            {
                print("string")
                
            }
            
            if let _ =  error {
                
                completed(.failure(.unableToComplete))
                return
            }
            
            print("error")
            
            guard let response = response as? HTTPURLResponse, response.statusCode == 200
                    
                    
            else {
                completed(.failure(.invalidResponse))
                
                
                return
            }
            
            guard let data = data , error == nil else {
                completed(.failure(.invalidData))
                return
            }
            
            do {
                
                let decoder = JSONDecoder()
                
                let decodedResponse = try decoder.decode(ProgramInfo.self, from: data)
                print("ProfilesdecodedResponse",decodedResponse)
                completed(.success(decodedResponse))
                
            } catch {
                
                completed(.failure(.decodingError))
            }
            
        }
        
        task.resume()
    }
    
}

class DashboardManager:
    NSObject
{
    
    var pgmType : String?
    
    static let shared           = DashboardManager()
    static let baseURL          =
    //  "https://rpmappservicepreprod.azurewebsites.net"
    //"https://rpmwebapp.azurewebsites.net"
    // "https://c-lynxapi.azurewebsites.net"
    // "https://rpmdevnew.azurewebsites.net"
    //"https://cx-dev-server.azurewebsites.net"
    //"https://cx-preprod-server.azurewebsites.net"
    //  "https://md-preprod-server.azurewebsites.net"
    "https://rpm-dev-tespcare.azurewebsites.net"
    
    private override init() {}
    
    //   NOTE : PROFILE AND PROGRAM
    
    func dashboard(tkn: String,
                   
                   completed: @escaping (Result<RPMProfileAndProgramDataModel, APIError>) -> Void) {
        
        print("tkndashboard",tkn)
        
        guard let url = URL(string: DashboardManager.baseURL + "/api/users/getmyprofileandprogram") else {
            completed(.failure(.invalidURL))
            return
        }
        print("urldashboard",url)
        var request = URLRequest(url: url)
        
        request.addValue(tkn, forHTTPHeaderField: "Bearer")
        
        let task = URLSession.shared.dataTask(with: request) { data, response, error in
            
            if let data = data,let _ = String(data: data,encoding:  .utf8)
            {
                
            }
            
            if let _ =  error {
                
                completed(.failure(.unableToComplete))
                return
            }
            
            guard let httpResponse = response as? HTTPURLResponse else {
                completed(.failure(.invalidResponse))
                return
            }
            
            // Check for 401 Unauthorized
            if httpResponse.statusCode == 401 {
                print("401 Unauthorized: Token expired or invalid")
                completed(.failure(.unauthorized))
                return
            }
            
            guard let response = response as? HTTPURLResponse, response.statusCode == 200
                    
            else {
                completed(.failure(.invalidResponse))
                
                return
            }
            
            guard let data = data , error == nil else {
                completed(.failure(.invalidData))
                return
            }
            print("dashboarddata",data)
            print("dashboarderror",error)
            
            
            do {
                
                let decoder = JSONDecoder()
                let decodedResponse = try decoder.decode(RPMProfileAndProgramDataModel.self, from: data)
                print("RPMProfileAndProgramDataModel",decodedResponse)
                
                let pgmType = UserDefaults.standard.set(decodedResponse.programType, forKey: "pgmTypeString" )
                
                let patientName = UserDefaults.standard.set(decodedResponse.name, forKey: "patientNameString" )
                
                let patientUserName = UserDefaults.standard.set(decodedResponse.userName, forKey: "patientUserNameString" )
                
                completed(.success(decodedResponse))
                
            } catch {
                
                completed(.failure(.decodingError))
            }
            
        }
        
        task.resume()
    }
    
    
    //  NOTE : NOTIFICATION CONNECT HUB
    
    
    func notify(tkn: String,
                
                completed: @escaping (Result<String, APIError>) -> Void) {
        
        guard let url = URL(string: DashboardManager.baseURL + "/api/authorization/connecthub") else {
            completed(.failure(.invalidURL))
            return
        }
        
        var request = URLRequest(url: url)
        
        request.addValue(tkn, forHTTPHeaderField: "Bearer")
        
        let task = URLSession.shared.dataTask(with: request) { data, response, error in
            
            if let data = data,let _ = String(data: data,encoding:  .utf8)
            {
                
            }
            
            if let _ =  error {
                
                completed(.failure(.unableToComplete))
                return
            }
            
            guard let response = response as? HTTPURLResponse, response.statusCode == 200
                    
            else {
                completed(.failure(.invalidResponse))
                
                return
            }
            
            guard let data = data , error == nil else {
                completed(.failure(.invalidData))
                return
            }
            
            do {
                let decoder = JSONDecoder()
                let decodedResponse = try decoder.decode(String.self, from: data)
                
                completed(.success(decodedResponse))
                
            } catch {
                
                print(error)
                completed(.failure(.decodingError))
            }
            
        }
        
        task.resume()
    }
    
    //  NOTE : GET NOTIFICATION
    
    
    func getnotify(tkn: String,
                   
                   completed: @escaping (Result<Notifications, APIError>) -> Void) {
        
        guard let url = URL(string: DashboardManager.baseURL + "/api/notification/user") else {
            completed(.failure(.invalidURL))
            return
        }
        
        var request = URLRequest(url: url)
        
        request.addValue(tkn, forHTTPHeaderField: "Bearer")
        
        let task = URLSession.shared.dataTask(with: request) { data, response, error in
            
            if let data = data,let _ = String(data: data,encoding:  .utf8)
            {
                
            }
            
            if let _ =  error {
                
                completed(.failure(.unableToComplete))
                return
            }
            
            guard let response = response as? HTTPURLResponse, response.statusCode == 200
                    
            else {
                completed(.failure(.invalidResponse))
                
                return
            }
            
            guard let data = data , error == nil else {
                completed(.failure(.invalidData))
                return
            }
            
            do {
                let decoder = JSONDecoder()
                let decodedResponse = try decoder.decode(Notifications.self, from: data)
                
                completed(.success(decodedResponse))
                
            } catch {
                
                print(error)
                completed(.failure(.decodingError))
            }
            
        }
        
        task.resume()
    }
    
    //  NOTE : GET NOTIFY COUNT
    
    
    func getnotifyCount(tkn: String,
                        
                        completed: @escaping (Result<NotificationCount, APIError>) -> Void) {
        
        guard let url = URL(string: DashboardManager.baseURL + "/api/notification/count") else {
            completed(.failure(.invalidURL))
            return
        }
        
        var request = URLRequest(url: url)
        
        request.addValue(tkn, forHTTPHeaderField: "Bearer")
        
        let task = URLSession.shared.dataTask(with: request) { data, response, error in
            
            if let data = data,let _ = String(data: data,encoding:  .utf8)
            {
                
            }
            
            if let _ =  error {
                
                completed(.failure(.unableToComplete))
                return
            }
            
            guard let response = response as? HTTPURLResponse, response.statusCode == 200
                    
            else {
                completed(.failure(.invalidResponse))
                
                return
            }
            
            guard let data = data , error == nil else {
                completed(.failure(.invalidData))
                return
            }
            
            do {
                let decoder = JSONDecoder()
                let decodedResponse = try decoder.decode(NotificationCount.self, from: data)
                
                completed(.success(decodedResponse))
                
            } catch {
                
                print(error)
                completed(.failure(.decodingError))
            }
            
        }
        
        task.resume()
    }
    
    
    // NOTE : GRAPH 7
    
    func graph7Vitals(tkn: String, completed: @escaping (Result<[RPMVitalsChartDaysDataModel], APIError>) -> Void) {
        let currentDate = Date()
        let calendar = Calendar.current
        
        // Calculate the start date (7 days ago from today)
        if let startDate = calendar.date(byAdding: .day, value: -6, to: currentDate) {
            // Adjust the start date to 00:00:00 local time
            let startDateTime = calendar.startOfDay(for: startDate)
            
            // Convert the start date to the desired format
            let dateFormatter = DateFormatter()
            dateFormatter.dateFormat = "yyyy-MM-dd'T'HH:mm:ss"
            dateFormatter.timeZone = TimeZone.current
            let startDateFormatted = dateFormatter.string(from: startDateTime)
            
            // Print and verify the formatted start date
            print("FormattedStartDate:", startDateFormatted)
            
            // Use your DateUtils.convertToUTC function to convert the start date to UTC
            if let adjustedUTCStartDate = DateUtils.convertToUTC(localDateStr: startDateFormatted, inputFormatStr: "yyyy-MM-dd'T'HH:mm:ss", outputFormatStr: "yyyy-MM-dd'T'HH:mm:ss") {
                
                // Print and verify the adjusted UTC start date
                print("AdjustedUTCStartDate:", adjustedUTCStartDate)
                
                // Get the end date as the current system date according to the current timezone
                let endDateTime = calendar.startOfDay(for: currentDate)
                
                // Convert the end date to the desired format
                let endDateFormatter = DateFormatter()
                endDateFormatter.dateFormat = "yyyy-MM-dd"
                endDateFormatter.timeZone = TimeZone.current
                let endDateFormatted = endDateFormatter.string(from: endDateTime)
                
                // Print and verify the formatted end date
                print("FormattedEndDate:", endDateFormatted)
                
                // Append "T00:00:00" to the end date
                let adjustedEndDateFormatted = endDateFormatted + "T23:59:59"
                
                print("adjustedEndDateFormatted:", adjustedEndDateFormatted)
                // Use your DateUtils.convertToUTC function to convert the adjusted end date to UTC
                if let adjustedUTCEndDate = DateUtils.convertToUTC(localDateStr: adjustedEndDateFormatted, inputFormatStr: "yyyy-MM-dd'T'HH:mm:ss", outputFormatStr: "yyyy-MM-dd'T'HH:mm:ss") {
                    // Print and verify the final adjusted UTC end date
                    print("FinalAdjustedUTCEndDate:", adjustedUTCEndDate)
                    
                    // Continue with the rest of your code, using adjustedUTCStartDate and adjustedUTCEndDate in the URL construction
                    if let url = URL(string: DashboardManager.baseURL + "/api/patients/getpatienthealthtrends?StartDate=" + adjustedUTCStartDate + "&EndDate=" + adjustedUTCEndDate) {
                        var request = URLRequest(url: url)
                        print("request",request)
                        // Configure your URLRequest here (e.g., add headers, set HTTP method)
                        request.addValue(tkn, forHTTPHeaderField: "Bearer")
                        
                        
                        request.timeoutInterval = 120 // in seconds
                        
                        // Continue with the rest of your URLSession code
                        let task = URLSession.shared.dataTask(with: request) { data, response, error in
                            // Handle the URLSession response here
                            if let data = data, let _ = String(data: data, encoding: .utf8) {
                                // Process the data here if needed
                            }
                            
                            if let error = error as? URLError {
                                print("URLError:", error.code)
                                
                                switch error.code {
                                case .notConnectedToInternet, .timedOut, .cannotFindHost, .cannotConnectToHost:
                                    completed(.failure(.noInternet)) // Create a new case for this
                                default:
                                    completed(.failure(.unableToComplete))
                                }
                                return
                            }
                            
                            guard let response = response as? HTTPURLResponse, response.statusCode == 200 else {
                                completed(.failure(.invalidResponse))
                                return
                            }
                            print("GRAPH7Response:",response)
                            
                            guard let data = data, error == nil else {
                                completed(.failure(.invalidData))
                                return
                            }
                            do {
                                print("decoderdecoder")
                                
                                let decoder = JSONDecoder()
                                
                                if let jsonArray = try? decoder.decode([RPMVitalsChartDaysDataModel].self, from: data) {
                                    // Case: Response is an array
                                    print("GRAPH7Decoded as array:", jsonArray)
                                    completed(.success(jsonArray))
                                } else if let jsonObject = try? decoder.decode(RPMVitalsChartDaysDataModel.self, from: data) {
                                    // Case: Response is a single object, wrap it in an array
                                    print("GTRAPH7Decoded as single object:", jsonObject)
                                    completed(.success([jsonObject])) // Wrap the single object in an array
                                } else {
                                    print("GRAPH7Decoding failed")
                                    completed(.failure(.decodingError))
                                }
                                
                            } catch {
                                print("Decoding failed with error: \(error)")
                                completed(.failure(.decodingError))
                            }
                        }
                        task.resume()
                    } else {
                        completed(.failure(.invalidURL))
                    }
                } else {
                    // Handle the case where adjusted end date conversion to UTC failed
                }
            } else {
                // Handle the case where start date conversion to UTC failed
            }
        } else {
            // Handle the case where date calculation failed
        }
    }
    
    
    
    // NOTE : GRAPH 30
    
    
    func graph30Vitals(tkn: String, completed: @escaping (Result<[RPMVitalsChartDaysDataModel], APIError>) -> Void) {
        // Get the current date (today's date)
        let currentDate = Date()
        
        // Create a Calendar instance
        let calendar = Calendar.current
        
        // Calculate the start date (30 days ago from today)
        var startDateUTC: String? = nil
        if let startDate = calendar.date(byAdding: .day, value: -29, to: currentDate) {
            // Adjust the start date to 00:00:00 local time
            let startDateTime = calendar.startOfDay(for: startDate)
            
            // Convert the start date to the desired format
            let dateFormatter = DateFormatter()
            dateFormatter.dateFormat = "yyyy-MM-dd'T'HH:mm:ss"
            dateFormatter.timeZone = TimeZone(abbreviation: "UTC")
            startDateUTC = dateFormatter.string(from: startDateTime)
            print("Start Date UTC:", startDateUTC ?? "N/A")
        }
        
        // Append "T23:59:59" to the current date
        let endDateFormatter = DateFormatter()
        endDateFormatter.dateFormat = "yyyy-MM-dd'T'HH:mm:ss"
        endDateFormatter.timeZone = TimeZone(abbreviation: "UTC")
        var endDateUTC: String? = endDateFormatter.string(from: currentDate)
        print("End Date UTC:", endDateUTC ?? "N/A")
        
        // Check if the date conversion succeeded before constructing the URL
        if let startDateUTC = startDateUTC, let endDateUTC = endDateUTC {
            if let url = URL(string: DashboardManager.baseURL + "/api/patients/getpatienthealthtrends?StartDate=" + startDateUTC + "&EndDate=" + endDateUTC) {
                var request = URLRequest(url: url)
                
                // Configure your URLRequest here (e.g., add headers, set HTTP method)
                request.addValue(tkn, forHTTPHeaderField: "Bearer")
                request.timeoutInterval = 120 // in seconds
                // Continue with the rest of your URLSession code
                let task = URLSession.shared.dataTask(with: request) { data, response, error in
                    // Handle the URLSession response here
                    if let data = data, let _ = String(data: data, encoding: .utf8) {
                        // Process the data here if needed
                    }
                    
                    if let error = error as? URLError {
                        print("URLError:", error.code)
                        
                        switch error.code {
                        case .notConnectedToInternet, .timedOut, .cannotFindHost, .cannotConnectToHost:
                            completed(.failure(.noInternet)) // Create a new case for this
                        default:
                            completed(.failure(.unableToComplete))
                        }
                        return
                    }
                    guard let response = response as? HTTPURLResponse, response.statusCode == 200 else {
                        completed(.failure(.invalidResponse))
                        return
                    }
                    print("Response:")
                    print(response)
                    guard let data = data, error == nil else {
                        completed(.failure(.invalidData))
                        return
                    }
                    do {
                        let decoder = JSONDecoder()
                        
                        if let jsonArray = try? decoder.decode([RPMVitalsChartDaysDataModel].self, from: data) {
                            // Case: Response is an array
                            print("Decoded as array:", jsonArray)
                            completed(.success(jsonArray))
                        } else if let jsonObject = try? decoder.decode(RPMVitalsChartDaysDataModel.self, from: data) {
                            // Case: Response is a single object, wrap it in an array
                            print("Decoded as single object:", jsonObject)
                            completed(.success([jsonObject])) // Wrap the single object in an array
                        } else {
                            print("Decoding failed")
                            completed(.failure(.decodingError))
                        }
                        
                    } catch {
                        completed(.failure(.decodingError))
                    }
                }
                task.resume()
            } else {
                completed(.failure(.invalidURL))
            }
        } else {
            // Handle the case where date conversion failed
        }
    }
    
    
    
    //  NOTE : GET VITAL SUMMARY
    
    func getVitalSummaryDefault(tkn: String,
                                
                                completed: @escaping (Result<VitalReadingsDataModel, APIError>) -> Void) {
        
        let todaysDate = NSDate()
        let dateFormatter = DateFormatter()
        dateFormatter.dateFormat = "yyyy-MM-dd"
        let DateInFormat = dateFormatter.string(from: todaysDate as Date)
        
        print("DateInFormat")
        
        print(DateInFormat)
        
        if let cvStartDate = DateUtils.convertToUTC(localDateStr: "\(DateInFormat)T00:00:00", inputFormatStr: "yyyy-MM-dd'T'HH:mm:ss", outputFormatStr: "yyyy-MM-dd'T'HH:mm:ss"),
           let cvEndDate = DateUtils.convertToUTC(localDateStr: "\(DateInFormat)T23:59:59", inputFormatStr: "yyyy-MM-dd'T'HH:mm:ss", outputFormatStr: "yyyy-MM-dd'T'HH:mm:ss") {
            
            
            guard let url = URL(string: DashboardManager.baseURL + "/api/patients/getpatientvitalreadings?StartDate=\(cvStartDate)&EndDate=\(cvEndDate)" ) else {
                completed(.failure(.invalidURL))
                return
            }
            print("urlgetVitalSummaryDefault" , url)
            
            var request = URLRequest(url: url)
            
            request.addValue(tkn, forHTTPHeaderField: "Bearer")
            
            request.timeoutInterval = 120 // in seconds
            
            let task = URLSession.shared.dataTask(with: request) { data, response, error in
                
                if let data = data,let _ = String(data: data,encoding:  .utf8)
                {
                    
                }
                
                if let error = error as? URLError {
                    print("getVitalSummaryDefaultURLError:", error.code)
                    
                    switch error.code {
                    case .notConnectedToInternet, .timedOut, .cannotFindHost, .cannotConnectToHost:
                        completed(.failure(.noInternet)) // Create a new case for this
                    default:
                        completed(.failure(.unableToComplete))
                    }
                    return
                }
                
                guard let response = response as? HTTPURLResponse, response.statusCode == 200
                        
                else {
                    completed(.failure(.invalidResponse))
                    
                    return
                }
                
                guard let data = data , error == nil else {
                    completed(.failure(.invalidData))
                    return
                }
                
                do {
                    let decoder = JSONDecoder()
                    let decodedResponse = try decoder.decode(VitalReadingsDataModel.self , from: data)
                    print("decodedResponseRPMVitalSummarydef",decodedResponse )
                    
                    completed(.success(decodedResponse))
                    
                } catch {
                    
                    print(error)
                    completed(.failure(.decodingError))
                }
                
            }
            
            task.resume()
        } else {
            // Handle the case where conversion to Date fails
            completed(.failure(.invalidData))
        }
    }
    
    
    //  NOTE : GET VITAL SUMMARY
    
    func getVitalSummaryList(tkn: String,  startDate : String, endDate : String,
                             
                             completed: @escaping (Result<VitalReadingsDataModel, APIError>) -> Void) {
        
        print("startDate")
        print("endDate")
        print(startDate)
        print(endDate)
        
        let dateFormatter = DateFormatter()
        dateFormatter.dateFormat = "yyyy-MM-dd'T'HH:mm:ss"
        
        if let cvStartDate = DateUtils.convertToUTC(localDateStr: "\(startDate)T00:00:00", inputFormatStr: "yyyy-MM-dd'T'HH:mm:ss", outputFormatStr: "yyyy-MM-dd'T'HH:mm:ss"),
           let cvEndDate = DateUtils.convertToUTC(localDateStr: "\(endDate)T23:59:59", inputFormatStr: "yyyy-MM-dd'T'HH:mm:ss", outputFormatStr: "yyyy-MM-dd'T'HH:mm:ss") {
            
            print("cvStartDate",cvStartDate)
            print("cvEndDate",cvEndDate)
            
            guard let url = URL(string: DashboardManager.baseURL + "/api/patients/getpatientvitalreadings?StartDate=\(cvStartDate)&EndDate=\(cvEndDate)" ) else {
                completed(.failure(.invalidURL))
                return
            }
            
            print("getVitalSummaryList",url)
            
            var request = URLRequest(url: url)
            
            request.addValue(tkn, forHTTPHeaderField: "Bearer")
            
            request.timeoutInterval = 120 // in seconds
            
            let task = URLSession.shared.dataTask(with: request) { data, response, error in
                
                if let data = data,let _ = String(data: data,encoding:  .utf8)
                {
                    
                }
                
                if let error = error as? URLError {
                    print("getVitalSummaryListURLError:", error.code)
                    
                    switch error.code {
                    case .notConnectedToInternet, .timedOut, .cannotFindHost, .cannotConnectToHost:
                        completed(.failure(.noInternet)) // Create a new case for this
                    default:
                        completed(.failure(.unableToComplete))
                    }
                    return
                }
                
                guard let response = response as? HTTPURLResponse, response.statusCode == 200
                        
                else {
                    completed(.failure(.invalidResponse))
                    
                    return
                }
                
                guard let data = data , error == nil else {
                    completed(.failure(.invalidData))
                    return
                }
                
                do {
                    let decoder = JSONDecoder()
                    let decodedResponse = try decoder.decode(VitalReadingsDataModel.self , from: data)
                    print("decodedResponseRPMVitalSummary",decodedResponse)
                    
                    completed(.success(decodedResponse))
                    
                } catch {
                    
                    print(error)
                    completed(.failure(.decodingError))
                }
                
            }
            
            task.resume()
            
            
        } else {
            // Handle the case where conversion to Date fails
            completed(.failure(.invalidData))
        }
    }
    
    
    //  NOTE : GET VITAL READINGS
    
    func getVitalReadingsList(tkn: String, startDate: String, endDate: String, completed: @escaping (Result<VitalReadingsDataModel, APIError>) -> Void) {
        
        print("startDatemr",startDate)
        print("endDatemr",endDate)
        
        
        let dateFormatter = DateFormatter()
        dateFormatter.dateFormat = "yyyy-MM-dd'T'HH:mm:ss"
        
        if let cvStartDate = DateUtils.convertToUTC(localDateStr: "\(startDate)T00:00:00", inputFormatStr: "yyyy-MM-dd'T'HH:mm:ss", outputFormatStr: "yyyy-MM-dd'T'HH:mm:ss"),
           let cvEndDate = DateUtils.convertToUTC(localDateStr: "\(endDate)T23:59:59", inputFormatStr: "yyyy-MM-dd'T'HH:mm:ss", outputFormatStr: "yyyy-MM-dd'T'HH:mm:ss") {
            
            guard let url = URL(string: DashboardManager.baseURL + "/api/patients/getpatientvitalreadings?StartDate=\(cvStartDate)&EndDate=\(cvEndDate)" ) else {
                completed(.failure(.invalidURL))
                return
            }
            
            // Use the 'url' variable as needed.
            print("mrurl",url)
            
            var request = URLRequest(url: url)
            request.addValue(tkn, forHTTPHeaderField: "Bearer")
            
            print("requestVR",request)
            let task = URLSession.shared.dataTask(with: request) { data, response, error in
                
                if let data = data, let _ = String(data: data, encoding: .utf8) {
                    
                }
                
                if let error = error as? URLError {
                    print("URLError:", error.code)
                    
                    switch error.code {
                    case .notConnectedToInternet, .timedOut, .cannotFindHost, .cannotConnectToHost:
                        completed(.failure(.noInternet)) // Create a new case for this
                    default:
                        completed(.failure(.unableToComplete))
                    }
                    return
                }
                
                guard let response = response as? HTTPURLResponse, response.statusCode == 200 else {
                    completed(.failure(.invalidResponse))
                    return
                }
                print("responseVR",response)
                guard let data = data, error == nil else {
                    completed(.failure(.invalidData))
                    return
                }
                print("dataVR",data)
                
                do {
                    let decoder = JSONDecoder()
                    let decodedResponse = try decoder.decode(VitalReadingsDataModel.self, from: data)
                    print("decodedVitalReadings",decodedResponse)
                    
                    completed(.success(decodedResponse))
                } catch {
                    print("decodedVitalReadingsERROR",error)
                    completed(.failure(.decodingError))
                }
                
            }
            print("taskVR",task)
            task.resume()
        } else {
            // Handle the case where conversion to Date fails
            completed(.failure(.invalidData))
        }
    }
    
    
    //  NOTE : TODO LIST
    
    func todolists(tkn: String,
                   
                   completed: @escaping (Result<[TodoList], APIError>) -> Void) {
        
        
        let todaysDate = NSDate()
        let dateFormatter = DateFormatter()
        dateFormatter.dateFormat = "yyyy-MM-dd"
        let DateInFormat = dateFormatter.string(from: todaysDate as Date)
        
        guard let url = URL(string: DashboardManager.baseURL + "/api/patients/gettodolist?StartDate="+DateInFormat+"T00:00:00&EndDate="+DateInFormat+"T23:59:59") else {
            completed(.failure(.invalidURL))
            return
        }
        
        var request = URLRequest(url: url)
        
        request.addValue(tkn, forHTTPHeaderField: "Bearer")
        request.timeoutInterval = 120 // in seconds
        let task = URLSession.shared.dataTask(with: request) { data, response, error in
            
            if let data = data,let _ = String(data: data,encoding:  .utf8)
            {
                
            }
            
            if let _ =  error {
                
                completed(.failure(.unableToComplete))
                return
            }
            
            guard let response = response as? HTTPURLResponse, response.statusCode == 200
                    
            else {
                completed(.failure(.invalidResponse))
                
                return
            }
            
            guard let data = data , error == nil else {
                completed(.failure(.invalidData))
                return
            }
            
            do {
                let decoder = JSONDecoder()
                let decodedResponse = try decoder.decode([TodoList].self , from: data)
                
                
                completed(.success(decodedResponse))
                
            } catch {
                
                print(error)
                completed(.failure(.decodingError))
            }
            
        }
        
        task.resume()
    }
    
    //  NOTE : TODO LIST
    func todolistsActivities(tkn: String, dt: String,
                             
                             completed: @escaping (Result<[TodoList], APIError>) -> Void) {
        
        let todaysDate = NSDate()
        let dateFormatter = DateFormatter()
        dateFormatter.dateFormat = "yyyy-MM-dd"
        let DateInFormat = dateFormatter.string(from: todaysDate as Date)
        
        guard let url = URL(string: DashboardManager.baseURL + "/api/patients/gettodolist?StartDate="+dt+"T00:00:00&EndDate="+dt+"T23:59:59") else {
            completed(.failure(.invalidURL))
            return
        }
        
        var request = URLRequest(url: url)
        
        request.addValue(tkn, forHTTPHeaderField: "Bearer")
        request.timeoutInterval = 120 // in seconds
        let task = URLSession.shared.dataTask(with: request) { data, response, error in
            
            if let data = data,let _ = String(data: data,encoding:  .utf8)
            {
                
            }
            
            
            if let error = error as? URLError {
                print("URLError:", error.code)
                
                switch error.code {
                case .notConnectedToInternet, .timedOut, .cannotFindHost, .cannotConnectToHost:
                    completed(.failure(.noInternet)) // Create a new case for this
                default:
                    completed(.failure(.unableToComplete))
                }
                return
            }
            
            
            guard let response = response as? HTTPURLResponse, response.statusCode == 200
                    
                    
            else {
                completed(.failure(.invalidResponse))
                
                return
            }
            
            guard let data = data , error == nil else {
                completed(.failure(.invalidData))
                return
            }
            
            do {
                let decoder = JSONDecoder()
                let decodedResponse = try decoder.decode([TodoList].self , from: data)
                print("tododecodedResponse", decodedResponse)
                completed(.success(decodedResponse))
                
            } catch {
                
                print(error)
                completed(.failure(.decodingError))
            }
            
        }
        
        task.resume()
    }
    
    
    
    //  NOTE : CHAT MEMBERS LIST DETAILS
    
    func membersLists(tkn: String,
                      
                      completed: @escaping (Result<[MembersListDataModel], APIError>) -> Void) {
        
        guard let url = URL(string: DashboardManager.baseURL + "/api/careteam/getpatientcareteammembers") else {
            completed(.failure(.invalidURL))
            return
        }
        
        
        var request = URLRequest(url: url)
        
        request.addValue(tkn, forHTTPHeaderField: "Bearer")
        
        let task = URLSession.shared.dataTask(with: request) { data, response, error in
            
            if let data = data,let _ = String(data: data,encoding:  .utf8)
            {
                
            }
            
            if let _ =  error {
                
                completed(.failure(.unableToComplete))
                return
            }
            
            guard let response = response as? HTTPURLResponse, response.statusCode == 200
                    
            else {
                completed(.failure(.invalidResponse))
                
                return
            }
            
            guard let data = data , error == nil else {
                completed(.failure(.invalidData))
                return
            }
            
            do {
                let decoder = JSONDecoder()
                let decodedResponse = try decoder.decode([MembersListDataModel].self , from: data)
                print("MembersList",decodedResponse)
                
                //  Save to UserDefaults
                if let encoded = try? JSONEncoder().encode(decodedResponse) {
                    UserDefaults.standard.set(encoded, forKey: "savedMemberList")
                }
                
                
                completed(.success(decodedResponse))
                
            } catch {
                
                completed(.failure(.decodingError))
            }
            
        }
        
        task.resume()
    }
    
    
    
    //  NOTE : GET CHAT SID
    
    
    func getChatSID(tkn: String, memberUserName: String,
                    
                    completed: @escaping (Result<String, APIError>) -> Void) {
        
        print("getChatSIDAPICALL")
        print("CHATGETtkn",tkn)
        print("memberUserName",memberUserName)
        // Assuming memberUserName is a String variable
        let uppercaseMemberUserName = memberUserName.uppercased()
        print("uppercaseMemberUserName", uppercaseMemberUserName)
        
        let encodedMemberUserName = memberUserName.addingPercentEncoding(withAllowedCharacters: .urlQueryAllowed) ?? ""
        
        guard let url = URL(string: DashboardManager.baseURL +
                            
                            "/api/comm/getchatsid?ToUser=\(encodedMemberUserName)"
                            
        ) else {
            
            completed(.failure(.invalidURL))
            return
        }
        
        print("chaturl",url)
        
        var request = URLRequest(url: url)
        request.addValue(tkn, forHTTPHeaderField: "Bearer")
        
        request.addValue("application/json", forHTTPHeaderField: "Content-Type")
        
        request.httpMethod = "GET"
        
        
        let task = URLSession.shared.dataTask(with: request) { data, response, error in
            print("chatResponse")
            print(response)
            
            if let error = error {
                print("Network error: \(error.localizedDescription)")
                completed(.failure(.unableToComplete))
                return
            }
            
            guard let httpResponse = response as? HTTPURLResponse else {
                completed(.failure(.invalidResponse))
                return
            }
            
            if let data = data, let responseString = String(data: data, encoding: .utf8) {
                print("Response data: \(responseString)")
                switch httpResponse.statusCode {
                case 200:
                    do {
                        if let json = try JSONSerialization.jsonObject(with: data, options: []) as? [String: String],
                           let chatSid = json["message"] {
                            completed(.success(chatSid)) //  Just send the chatSid now
                        } else {
                            completed(.failure(.invalidData)) // Invalid structure
                        }
                    } catch {
                        completed(.failure(.invalidData)) // JSON parse error
                    }
                    
                case 404:
                    // Handle the 404 case specifically if needed
                    completed(.failure(.invalidResponse))
                    // completed(.success(responseString))
                default:
                    completed(.failure(.invalidResponse))
                }
            } else {
                completed(.failure(.invalidData))
            }
            
        }
        print("chattask")
        print(task)
        
        task.resume()
    }
    
    
    
    // NOTE : CHAT UPDATE API INTEGRATION
    
    func chatUpdate(toUser: String,
                    convSID: String,
                    completed: @escaping (Result<Int, APIError>) -> Void) {
        guard let url = URL(string: NetworkManager.baseURL + "/api/comm/updatechatresource") else {
            completed(.failure(.invalidURL))
            return
        }
        print("chatUpAPICALL")
        
        let bodyData = "{\"ToUser\": \"\(toUser)\", \"ConversationSid\": \"\(convSID)\", \"ChatToken\": \"\(  UserDefaults.standard.string(forKey: "conversationToken") ?? "")\"}"
        print("chatUpbodyData",bodyData)
        
        let postData = bodyData.data(using: .utf8)
        
        var request = URLRequest(url: url)
        print("chatUprequest",request)
        request.addValue((UserDefaults.standard.string(forKey: "jsonwebtoken") ?? ""), forHTTPHeaderField: "Bearer")
        
        request.addValue("application/json", forHTTPHeaderField: "Content-Type")
        
        request.httpMethod = "POST"
        request.httpBody = postData
        
        print("chatUpPostData",postData)
        
        let task = URLSession.shared.dataTask(with: request) { data, response, error in
            
            if let error =  error {
                completed(.failure(.unableToComplete))
                return
            }
            print("chatUperror",error)
            
            guard let httpResponse = response as? HTTPURLResponse else {
                completed(.failure(.invalidResponse))
                return
            }
            print("httpResponse",httpResponse)
            guard httpResponse.statusCode == 200 else {
                completed(.failure(.invalidResponse))
                return
            }
            print("httpResponsestatusCode",httpResponse.statusCode)
            guard let data = data else {
                completed(.failure(.invalidData))
                return
            }
            print("chatUpdata",data)
            print("chatUpresponse",response)
            
            do {
                
                // Check if the response data is not empty
                guard data.isEmpty == false else {
                    completed(.failure(.invalidData))
                    return
                }
                
                let decoder = JSONDecoder()
                let decodedResponse = try decoder.decode(Int.self, from: data)
                print("chatUpdateResponse",decodedResponse)
                
                completed(.success(decodedResponse))
                
            } catch {
                
                completed(.failure(.invalidData))
                
            }
        }
        
        task.resume()
        
    }
    
    func chatHeartbeat(
        convSID: String, toUser: String, lastActiveAt: Date,
        completed: @escaping (Result<Int, APIError>) -> Void
    ) {
        guard let url = URL(string: DashboardManager.baseURL + "/api/comm/chatheartbeat") else {
            completed(.failure(.invalidURL))
            return
        }
        
        // Format the date just like Postman
        let formattedDate = formatDateLikePostman(lastActiveAt)
        print("Formatted LastActiveAt: \(formattedDate)")
        
        let payload: [String: Any] = [
            "ConversationSid": convSID,
            "UserName": toUser,
            "LastActiveAt": formattedDate  // Make sure this is in correct format like "4/18/2025, 9:42:00 AM +05:30"
        ]
        
        guard let postData = try? JSONSerialization.data(withJSONObject: payload, options: []) else {
            completed(.failure(.invalidData))
            return
        }
        
        if let jsonString = String(data: postData, encoding: .utf8) {
            print(" Request JSON: \(jsonString)")
        }
        
        guard let url = URL(string: DashboardManager.baseURL + "/api/comm/chatheartbeat") else {
            completed(.failure(.invalidURL))
            return
        }
        
        var request = URLRequest(url: url)
        request.addValue("application/json", forHTTPHeaderField: "Content-Type")
        request.addValue((UserDefaults.standard.string(forKey: "jsonwebtoken") ?? ""), forHTTPHeaderField: "Bearer")
        request.httpMethod = "POST"
        request.httpBody = postData
        
        let task = URLSession.shared.dataTask(with: request) { data, response, error in
            if let error = error {
                print(" URLSession error: \(error.localizedDescription)")
                completed(.failure(.unableToComplete))
                return
            }
            
            guard let httpResponse = response as? HTTPURLResponse else {
                print(" Invalid response type.")
                completed(.failure(.invalidResponse))
                return
            }
            
            print(" Received HTTP response with status code: \(httpResponse.statusCode)")
            
            if httpResponse.statusCode != 200 {
                if let data = data, let errorMessage = String(data: data, encoding: .utf8) {
                    print("Server error response: \(errorMessage)")
                }
                completed(.failure(.invalidResponse))
                return
            }
            
            // Optional: Print success message if available
            if let data = data,
               let responseString = String(data: data, encoding: .utf8) {
                print(" Server response: \(responseString)")
            }
            
            completed(.success(httpResponse.statusCode))
        }
        
        task.resume()
    }
    
    func deleteAllNotifications(tkn: String, completion: @escaping (Result<Void, Error>) -> Void) {
        guard let url = URL(string: DashboardManager.baseURL + "/api/notification/delete/unread?notificationId=0") else {
            completion(.failure(NSError(domain: "Invalid URL", code: -1, userInfo: nil)))
            return
        }
        
        var request = URLRequest(url: url)
        
        request.addValue("application/json", forHTTPHeaderField: "Content-Type")
        request.addValue((UserDefaults.standard.string(forKey: "jsonwebtoken") ?? ""), forHTTPHeaderField: "Bearer")
        request.httpMethod = "POST"
        
        
        
        URLSession.shared.dataTask(with: request) { _, response, error in
            if let error = error {
                completion(.failure(error))
                return
            }
            
            completion(.success(()))
        }.resume()
    }
    
    
    func sendMessageNotification(
        convSID: String,
        toUser: String,
        fromUser: String,
        message: String,
        completion: @escaping (Result<Int, Error>) -> Void
    ) {
        print("[NotifyConversation] Preparing to send message notification...")
        
        let payload: [String: Any] = [
            "ConversationSid": convSID,
            "ToUser": toUser,
            "FromUser": fromUser,
            "Message": message
        ]
        
        print("[NotifyConversation] Payload: \(payload)")
        
        guard let url = URL(string: DashboardManager.baseURL + "/api/comm/NotifyConversation") else {
            print("[NotifyConversation] Invalid URL!")
            completion(.failure(NSError(domain: "NotifyConversation", code: -1, userInfo: [
                NSLocalizedDescriptionKey: "Invalid URL"
            ])))
            return
        }
        
        var request = URLRequest(url: url)
        request.addValue((UserDefaults.standard.string(forKey: "jsonwebtoken") ?? ""), forHTTPHeaderField: "Bearer")
        request.httpMethod = "POST"
        request.setValue("application/json", forHTTPHeaderField: "Content-Type")
        
        do {
            request.httpBody = try JSONSerialization.data(withJSONObject: payload, options: [])
            print("[NotifyConversation] JSON body successfully created.")
        } catch {
            print("[NotifyConversation] Failed to serialize payload: \(error.localizedDescription)")
            completion(.failure(error))
            return
        }
        
        print("[NotifyConversation] Sending API request to: \(url.absoluteString)")
        
        let task = URLSession.shared.dataTask(with: request) { data, response, error in
            if let error = error {
                print("[NotifyConversation] Request failed with error: \(error.localizedDescription)")
                completion(.failure(error))
                return
            }
            
            guard let httpResponse = response as? HTTPURLResponse else {
                print("[NotifyConversation] Invalid HTTP response!")
                completion(.failure(NSError(domain: "NotifyConversation", code: -2, userInfo: [
                    NSLocalizedDescriptionKey: "Invalid response"
                ])))
                return
            }
            
            print("[NotifyConversation] API call succeeded with status code: \(httpResponse.statusCode)")
            
            // Optional: If you want to print response data also
            if let data = data,
               let responseString = String(data: data, encoding: .utf8) {
                print("[NotifyConversation] Response Body: \(responseString)")
            }
            
            completion(.success(httpResponse.statusCode))
        }
        
        task.resume()
    }
    
    
    
    func formatDateLikePostman(_ date: Date) -> String {
        let formatter = DateFormatter()
        formatter.dateFormat = "M/d/yyyy, h:mm:ss a Z"
        formatter.locale = Locale(identifier: "en_US_POSIX") // important to match AM/PM format
        formatter.timeZone = TimeZone.current
        return formatter.string(from: date)
    }
    
}


class MoreManager: NSObject {
    
    static let shared           = MoreManager()
    static let baseURL          =
    // "https://rpmappservicepreprod.azurewebsites.net"
    // "https://rpmwebapp.azurewebsites.net"
    //"https://c-lynxapi.azurewebsites.net"
    //  "https://rpmdevnew.azurewebsites.net"
    // "https://cx-dev-server.azurewebsites.net"
    //"https://cx-preprod-server.azurewebsites.net"
    //  "https://md-preprod-server.azurewebsites.net"
    "https://rpm-dev-tespcare.azurewebsites.net"
    
    private override init() {}
    
    //  NOTE : GET PATIENT PROGRAM DETAILS
    
    func pgmDetailsLists(tkn: String,
                         completed: @escaping (Result<ProgramInfo, APIError>) -> Void) {
        guard let url = URL(string: MoreManager.baseURL + "/api/patients/getpatient") else {
            completed(.failure(.invalidURL))
            return
        }
        
        
        
        var request = URLRequest(url: url)
        
        request.addValue(tkn, forHTTPHeaderField: "Bearer")
        print("reque")
        // print(request)
        
        let task = URLSession.shared.dataTask(with: request) { data, response, error in
            
            if let data = data,let _ = String(data: data,encoding:  .utf8)
            {
                
            }
            
            
            if let error = error as? URLError {
                print("URLError:", error.code)
                
                switch error.code {
                case .notConnectedToInternet, .timedOut, .cannotFindHost, .cannotConnectToHost:
                    completed(.failure(.noInternet)) // Create a new case for this
                default:
                    completed(.failure(.unableToComplete))
                }
                return
            }
            print("error")
            print(error)
            
            
            guard let response = response as? HTTPURLResponse, response.statusCode == 200
                    
                    
            else {
                completed(.failure(.invalidResponse))
                
                
                return
            }
            print("response")
            print(response)
            guard let data = data , error == nil else {
                completed(.failure(.invalidData))
                return
            }
            
            do {
                
                let decoder = JSONDecoder()
                
                let decodedResponse = try decoder.decode(ProgramInfo.self, from: data)
                print("\nProfilesdecodedResponse\n")
                print(decodedResponse)
                completed(.success(decodedResponse))
                
            } catch {
                
                completed(.failure(.decodingError))
            }
            
        }
        
        task.resume()
    }
    
    
    //  NOTE : PATIENT MEDICATION DETAILS
    
    func medicationsLists(tkn: String,
                          
                          completed: @escaping (Result<[Medication], APIError>) -> Void) {
        
        guard let url = URL(string: MoreManager.baseURL + "/api/patients/getpatientmedication") else {
            completed(.failure(.invalidURL))
            return
        }
        
        var request = URLRequest(url: url)
        
        
        request.addValue(tkn, forHTTPHeaderField: "Bearer")
        
        
        let task = URLSession.shared.dataTask(with: request) { data, response, error in
            
            if let data = data,let _ = String(data: data,encoding:  .utf8)
            {
                
            }
            
            if let error = error as? URLError {
                print("URLError:", error.code)
                
                switch error.code {
                case .notConnectedToInternet, .timedOut, .cannotFindHost, .cannotConnectToHost:
                    completed(.failure(.noInternet)) // Create a new case for this
                default:
                    completed(.failure(.unableToComplete))
                }
                return
            }
            
            
            guard let response = response as? HTTPURLResponse, response.statusCode == 200
                    
            else {
                completed(.failure(.invalidResponse))
                
                return
            }
            
            guard let data = data , error == nil else {
                completed(.failure(.invalidData))
                return
            }
            
            do {
                let decoder = JSONDecoder()
                let decodedResponse = try decoder.decode([Medication].self , from: data)
                print("MedicationdecodedResponse")
                print(decodedResponse)
                completed(.success(decodedResponse))
                
            } catch {
                
                completed(.failure(.decodingError))
            }
            
        }
        
        task.resume()
    }
    
    
    //  NOTE : ADDD MEDICATIONS
    
    
    func addMedications(tkn: String, medName: String,
                        schedule1: String,schedule2: String,   morning : Int, afternoon : Int , evening : Int, night : Int,  startDate: String, endDate : String, comments : String,
                        completed: @escaping (Result<Int, APIError>) -> Void) {
        guard let url = URL(string: MoreManager.baseURL + "/api/patients/addpatientmedication") else {
            completed(.failure(.invalidURL))
            return
        }
        
        let json: [String: Any] = ["Medicinename": medName,
                                   "MedicineSchedule": schedule1 ,
                                   "BeforeOrAfterMeal":schedule2,
                                   
                                   "Morning" : morning , "AfterNoon":afternoon , "Evening" : evening , "Night" : night , "StartDate" : startDate , "EndDate" : endDate , "Description" :comments ]
        
        let jsonData = try? JSONSerialization.data(withJSONObject: json)
        
        
        print("datass")
        print(medName)
        print(schedule1)
        print(schedule2)
        print(morning)
        print(afternoon)
        print(evening)
        print(night)
        print(startDate)
        print(endDate)
        print(comments)
        
        var request = URLRequest(url: url)
        request.addValue(tkn, forHTTPHeaderField: "Bearer")
        
        request.addValue("application/json", forHTTPHeaderField: "Content-Type")
        
        request.httpMethod = "POST"
        request.httpBody = jsonData
        
        let task = URLSession.shared.dataTask(with: request) { data, response, error in
            
            if let data = data,let _ = String(data: data,encoding:  .utf8)
            {
                
            }
            
            if let _ =  error {
                
                completed(.failure(.unableToComplete))
                
                return
                
            }
            
            guard let response = response as? HTTPURLResponse, response.statusCode == 200 else {                completed(.failure(.invalidResponse))
                
                return
            }
            
            guard let data = data
                    
            else {
                print(data as Any)
                completed(.failure(.invalidData))
                return
            }
            
            
            do {
                let decoder = JSONDecoder()
                let decodedResponse = try decoder.decode(Int.self, from: data)
                
                completed(.success(decodedResponse))
            } catch {
                completed(.failure(.invalidData))
                
            }
        }
        
        task.resume()
    }
    
    
    //  NOTE : GET PATIENT SYMPTOMS DETAILS
    func symptomsLists(tkn: String,
                       completed: @escaping (Result<[RPMSymptom], APIError>) -> Void) {
        guard let url = URL(string: MoreManager.baseURL + "/api/patients/getpatientsymptoms") else {
            completed(.failure(.invalidURL))
            return
        }
        var request = URLRequest(url: url)
        
        request.addValue(tkn, forHTTPHeaderField: "Bearer")
        
        print(request)
        
        let task = URLSession.shared.dataTask(with: request) { data, response, error in
            
            if let data = data,let _ = String(data: data,encoding:  .utf8)
            {}
            
            if let error = error as? URLError {
                print("URLError:", error.code)
                
                switch error.code {
                case .notConnectedToInternet, .timedOut, .cannotFindHost, .cannotConnectToHost:
                    completed(.failure(.noInternet)) // Create a new case for this
                default:
                    completed(.failure(.unableToComplete))
                }
                return
            }
            
            guard let response = response as? HTTPURLResponse, response.statusCode == 200
                    
                    
            else {
                completed(.failure(.invalidResponse))
                
                return
            }
            
            guard let data = data , error == nil else {
                completed(.failure(.invalidData))
                return
            }
            do {
                let decoder = JSONDecoder()
                let decodedResponse = try decoder.decode([RPMSymptom].self, from: data)
                completed(.success(decodedResponse))
                
            } catch {
                
                completed(.failure(.decodingError))
            }
            
        }
        
        task.resume()
    }
    
    
    //  NOTE : GET PATIENT SCHEDULES DETAILS
    
    
    func activitySchedules(tkn: String,  startDate : String, endDate : String,
                           
                           completed: @escaping (Result<[RPMSchedule], APIError>) -> Void) {
        
        guard let url = URL(string: MoreManager.baseURL + "/api/patients/getpatientschedules?StartDate=\(startDate)&EndDate=\(endDate)" ) else {
            
            completed(.failure(.invalidURL))
            return
        }
        
        var request = URLRequest(url: url)
        
        request.addValue(tkn, forHTTPHeaderField: "Bearer")
        print("request")
        print(request)
        
        let task = URLSession.shared.dataTask(with: request) { data, response, error in
            
            if let data = data,let _ = String(data: data,encoding:  .utf8)
            {
                print("string")
                
            }
            
            if let _ =  error {
                
                completed(.failure(.unableToComplete))
                return
            }
            
            print("error")
            
            guard let response = response as? HTTPURLResponse, response.statusCode == 200
                    
                    
            else {
                completed(.failure(.invalidResponse))
                
                
                return
            }
            
            guard let data = data , error == nil else {
                completed(.failure(.invalidData))
                return
            }
            
            do {
                
                let decoder = JSONDecoder()
                
                let decodedResponse = try decoder.decode([RPMSchedule].self, from: data)
                
                completed(.success(decodedResponse))
                
            } catch {
                
                completed(.failure(.decodingError))
            }
            
        }
        
        task.resume()
    }
}
extension Date {
    func toISOString(dateFormat: String) -> String {
        let dateFormatter = DateFormatter()
        dateFormatter.dateFormat = dateFormat
        return dateFormatter.string(from: self)
    }
}
extension String {
    
    func toUTC() -> Date? {
        let dateFormatter = DateFormatter()
        dateFormatter.dateFormat = "yyyy-MM-dd'T'HH:mm:ss"
        dateFormatter.timeZone = TimeZone(abbreviation: "UTC")
        return dateFormatter.date(from: self)
    }
    func index(from: Int) -> Index {
        return self.index(startIndex, offsetBy: from)
    }
    
    func substring(from: Int) -> String {
        let fromIndex = index(from: from)
        return String(self[fromIndex...])
    }
    
    func substring(to: Int) -> String {
        let toIndex = index(from: to)
        return String(self[..<toIndex])
    }
    
    func substring(with r: Range<Int>) -> String {
        let startIndex = index(from: r.lowerBound)
        let endIndex = index(from: r.upperBound)
        return String(self[startIndex..<endIndex])
    }
}
