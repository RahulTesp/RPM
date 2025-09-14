
// This file was generated from JSON Schema using quicktype, do not modify it directly.
// To parse the JSON, add this file to your project and do:
//
//   let todoListModel = try? newJSONDecoder().decode(TodoListModel.self, from: jsonData)

import Foundation

// MARK: - TodoListModelElement
struct TodoList: Codable , Identifiable, Equatable {
    
    static func == (lhs: TodoList, rhs: TodoList) -> Bool {
        return true
    }
    
    func hash(into hasher: inout Hasher) {
        
    }
    let id = UUID()
   
    let date, scheduleType, decription: String?
    
    enum CodingKeys: String, CodingKey {
    
        case date = "Date"
        case scheduleType = "ActivityName"
        case decription = "Description"
    }
}

typealias TodoListModel = [TodoList]
