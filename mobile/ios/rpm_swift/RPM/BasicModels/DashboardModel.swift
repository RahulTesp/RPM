//
//  DashboardModel.swift
//  SwiftUI-Dashboard
//
//  Created by Tesplabs on 12/03/1944 Saka.
//

//import Foundation
import SwiftUI
struct DashboardModel: Codable , Identifiable {
    let id: UUID
    var name: String
    var userName: String
    var programName: String
    var completedDuration: String
    var status: String
}


//Class Api {
//    func getPosts (){
//        guard let url = URL(string: "")
//        else
//        {
//            return
//            
//        }
//        
//        URLSession.shared.dataTask(with: url) {
//            (data, _, _ ) in
//            
//            let posts = try! JSONDecoder().decode([DashboardModel].self , from : data!)
//            print(posts)
//            
//        }
//        .resume()
//    }
//    
//}








