// import { Component, Input, OnDestroy, OnInit } from '@angular/core';
// import { PatientChatService } from 'src/app/components/admin-dashboard/shared/chatbutton/service/patient-chat.service';
// import { RPMService } from 'src/app/components/admin-dashboard/sevices/rpm.service';
// import { DatePipe } from '@angular/common';
// import moment from 'moment';
// import { Subscription } from 'rxjs';
// interface Message {
//   Message: string;
//   DateTime: string;
//   Author: string;
// }


// @Component({
//   selector: 'app-activityinfo-chat',
//   templateUrl: './activityinfo-chat.component.html',
//   styleUrl: './activityinfo-chat.component.scss'
// })
// export class ActivityinfoChatComponent implements OnInit, OnDestroy {
// public showMessagePanel=false;
// @Input() dataSourceTable:any;
// @Input() http_chat_data:any;
// @Input() patientname:string;
// dateCompare = 'undefined';
// private chatHistoryDataSubscription: Subscription;
// displaychatColumns = [
//   'ContactName',
//   'LastMessage',
//   'Date',
//   'Time',
//   'ConversationSid',
//   'actions',
// ];
// public dataSources:any;
// public dataSourceChatPaneldata:any;
// constructor( private patientchatservice: PatientChatService,private rpm: RPMService,public datepipe: DatePipe,
// ){


// }
//   ngOnDestroy(): void {
//     throw new Error('Method not implemented.');
//   }
//   ngOnInit(): void {
//     this.chatHistoryDataSubscription = this.patientchatservice.chatHistoryData$.subscribe(data => {
//       if (data) {
//         this.http_chat_data = data;

//         console.log('Chat data updated:', this.http_chat_data);

//       }
//     });
//   }


// userName:string='';
//   myprofile:any;
//   messages: Array<Message> = [];


//  getMessagesByConversationSid(conversationSid: string): Message[] {
//   this.showMessagePanel = false;
//   console.log(this.http_chat_data)
//   const convo = this.http_chat_data.find((c: { ConversationSid: string; }) => c.ConversationSid === conversationSid);
//   console.log(convo)
//   this.dataSourceChatPaneldata = convo;
//   this.showMessagePanel = true;
//   return convo ? convo.Messages : [];
// }
// closeChatPanel()
// {
//   this.showMessagePanel = false;
// }
// shouldShowDate(dateTime: string, index: number): boolean {
//   if (index === 0) return true; // always show for the first message

//   const currentDate = this.datepipe.transform(dateTime, 'yyyy-MM-dd');
//   const prevDate = this.datepipe.transform(
//     this.dataSourceChatPaneldata.Messages[index - 1]?.DateTime,
//     'yyyy-MM-dd'
//   );

//   return currentDate !== prevDate;
// }
// convertToLocal(dateStr: string): Date {
//   return new Date(dateStr); // Automatically converted to browser local time
// }
// convertToLocalTime(utcTimeString: any,date:string): string {
//   if (!utcTimeString) {
//     return ''; // Return empty string instead of null
//   }

//   try {
//     let cleanUtcString = utcTimeString.toString();
//     let dateObj;

//     // Remove timezone offset if present (like +0000)
//     if (cleanUtcString.includes('+')) {
//       cleanUtcString = cleanUtcString.split('+')[0].trim();
//     }

//     // Handle common UTC formats to create proper Date object
//     if (cleanUtcString.endsWith('Z')) {
//       // ISO format with Z suffix (e.g., 2025-04-30T06:38:11Z)
//       dateObj = new Date(cleanUtcString);
//     } else if (cleanUtcString.includes('T')) {
//       // ISO format without Z (e.g., 2025-04-30T06:38:11)
//       dateObj = new Date(cleanUtcString + 'Z');
//     } else {
//       // Standard date format (e.g., 4/30/2025 6:38:11 AM)
//       dateObj = new Date(cleanUtcString + ' UTC');
//     }

//     // Check if valid date
//     if (isNaN(dateObj.getTime())) {
//       console.error('Invalid date format:', utcTimeString);
//       return utcTimeString.toString(); // Ensure string return for invalid dates
//     }

//     // Use DatePipe to transform the Date object with the desired format
//     let result;
//     if(date == 'date')
//     {
//        result = this.datepipe.transform(dateObj, "MMM d, y");
//     }else if(date == 'time')
//     {
//       result = this.datepipe.transform(dateObj, "h:mm a");
//     }


//     // Handle potential null from datePipe.transform()
//     return result || utcTimeString.toString(); // Return original string if transform returns null
//   } catch (error) {
//     console.error('Error converting date:', error);
//     return utcTimeString.toString(); // Ensure string return on error
//   }
// }
// }
// import { Component, Input, OnDestroy, OnInit } from '@angular/core';
// import { PatientChatService } from 'src/app/components/admin-dashboard/shared/chatbutton/service/patient-chat.service';
// import { RPMService } from 'src/app/components/admin-dashboard/sevices/rpm.service';
// import { DatePipe } from '@angular/common';
// import moment from 'moment';
// import { Subscription } from 'rxjs';

// interface Message {
//   Message: string;
//   DateTime: string;
//   Author: string;
// }

// @Component({
//   selector: 'app-activityinfo-chat',
//   templateUrl: './activityinfo-chat.component.html',
//   styleUrl: './activityinfo-chat.component.scss'
// })
// export class ActivityinfoChatComponent implements OnInit, OnDestroy {
//   public showMessagePanel = false;
//   @Input() dataSourceTable: any;
//   @Input() http_chat_data: any;
//   @Input() patientname: string;

//   // Flag to indicate data refresh is needed
//   public dataRefreshNeeded = false;

//   // Timestamp of last update to track changes
//   private lastUpdateTimestamp: number = 0;

//   dateCompare = 'undefined';
//   private chatHistoryDataSubscription: Subscription;
//   displaychatColumns = [
//     'ContactName',
//     'LastMessage',
//     'Date',
//     'Time',
//     'ConversationSid',
//     'actions',
//   ];

//   public dataSources: any;
//   public dataSourceChatPaneldata: any;

//   constructor(
//     private patientchatservice: PatientChatService,
//     private rpm: RPMService,
//     public datepipe: DatePipe,
//   ) {}

//   ngOnDestroy(): void {
//     // Properly unsubscribe to prevent memory leaks
//     if (this.chatHistoryDataSubscription) {
//       this.chatHistoryDataSubscription.unsubscribe();
//     }
//   }

//   ngOnInit(): void {
//     this.chatHistoryDataSubscription = this.patientchatservice.chatHistoryData$.subscribe(data => {
//       if (data) {
//         // Store previous data for comparison
//         const previousData = this.http_chat_data ? JSON.stringify(this.http_chat_data) : null;

//         // Update with new data
//         this.http_chat_data = data;
//         console.log('Chat data updated:', this.http_chat_data);

//         // Compare with previous data to see if it changed
//         const currentData = JSON.stringify(this.http_chat_data);
//         const dataHasChanged = previousData !== currentData;

//         // Set refresh flag if data changed or it's the first load
//        // if (dataHasChanged) {
//           this.dataRefreshNeeded = true;
//           this.lastUpdateTimestamp = Date.now();
//           console.log('Data has changed, refresh needed!', this.lastUpdateTimestamp);

//           // Trigger any refresh actions needed
//           this.refreshData();
//        // }
//       }
//     });
//   }

//   // Method to refresh data and update UI
//   refreshData(): void {
//     console.log('Refreshing data and UI...');

//     // Reset refresh flag after handling the refresh
//     this.dataRefreshNeeded = false;

//     // Refresh any other components or update UI as needed
//     // For example, if a chat panel is open, refresh its messages
//     if (this.showMessagePanel && this.dataSourceChatPaneldata) {
//       // Re-fetch the conversation data
//       const conversationSid = this.dataSourceChatPaneldata.ConversationSid;
//       if (conversationSid) {
//         this.getMessagesByConversationSid(conversationSid);
//       }
//     }

//     // Notify parent components or perform other actions as needed
//     this.notifyDataRefresh();
//   }

//   // Method to notify other components about the refresh
//   notifyDataRefresh(): void {
//     // You could emit an event to parent components if needed
//     // For example: this.dataRefreshed.emit(this.lastUpdateTimestamp);

//     // Or you could update a service to indicate the refresh
//     // this.patientchatservice.notifyDataRefreshed(this.lastUpdateTimestamp);
//   }

//   userName: string = '';
//   myprofile: any;
//   messages: Array<Message> = [];

//   getMessagesByConversationSid(conversationSid: string): Message[] {
//     this.showMessagePanel = false;
//     console.log(this.http_chat_data);
//     const convo = this.http_chat_data.find((c: { ConversationSid: string; }) => c.ConversationSid === conversationSid);
//     console.log(convo);
//     this.dataSourceChatPaneldata = convo;
//     this.showMessagePanel = true;
//     return convo ? convo.Messages : [];
//   }

//   closeChatPanel() {
//     this.showMessagePanel = false;
//   }

//   shouldShowDate(dateTime: string, index: number): boolean {
//     if (index === 0) return true; // always show for the first message

//     const currentDate = this.datepipe.transform(dateTime, 'yyyy-MM-dd');
//     const prevDate = this.datepipe.transform(
//       this.dataSourceChatPaneldata.Messages[index - 1]?.DateTime,
//       'yyyy-MM-dd'
//     );

//     return currentDate !== prevDate;
//   }

//   convertToLocal(dateStr: string): Date {
//     return new Date(dateStr); // Automatically converted to browser local time
//   }

//   convertToLocalTime(utcTimeString: any, date: string): string {
//     if (!utcTimeString) {
//       return ''; // Return empty string instead of null
//     }

//     try {
//       let cleanUtcString = utcTimeString.toString();
//       let dateObj;

//       // Remove timezone offset if present (like +0000)
//       if (cleanUtcString.includes('+')) {
//         cleanUtcString = cleanUtcString.split('+')[0].trim();
//       }

//       // Handle common UTC formats to create proper Date object
//       if (cleanUtcString.endsWith('Z')) {
//         // ISO format with Z suffix (e.g., 2025-04-30T06:38:11Z)
//         dateObj = new Date(cleanUtcString);
//       } else if (cleanUtcString.includes('T')) {
//         // ISO format without Z (e.g., 2025-04-30T06:38:11)
//         dateObj = new Date(cleanUtcString + 'Z');
//       } else {
//         // Standard date format (e.g., 4/30/2025 6:38:11 AM)
//         dateObj = new Date(cleanUtcString + ' UTC');
//       }

//       // Check if valid date
//       if (isNaN(dateObj.getTime())) {
//         console.error('Invalid date format:', utcTimeString);
//         return utcTimeString.toString(); // Ensure string return for invalid dates
//       }

//       // Use DatePipe to transform the Date object with the desired format
//       let result;
//       if (date == 'date') {
//         result = this.datepipe.transform(dateObj, "MMM d, y");
//       } else if (date == 'time') {
//         result = this.datepipe.transform(dateObj, "h:mm a");
//       }

//       // Handle potential null from datePipe.transform()
//       return result || utcTimeString.toString(); // Return original string if transform returns null
//     } catch (error) {
//       console.error('Error converting date:', error);
//       return utcTimeString.toString(); // Ensure string return on error
//     }
//   }
// }
import { Component, Input, OnDestroy, OnInit, ChangeDetectorRef, AfterViewInit } from '@angular/core';
import { PatientChatService } from 'src/app/components/admin-dashboard/shared/chatbutton/service/patient-chat.service';
import { RPMService } from 'src/app/components/admin-dashboard/sevices/rpm.service';
import { DatePipe } from '@angular/common';
import moment from 'moment';
import { Subscription } from 'rxjs';

interface Message {
  Message: string;
  DateTime: string;
  Author: string;
}

@Component({
  selector: 'app-activityinfo-chat',
  templateUrl: './activityinfo-chat.component.html',
  styleUrl: './activityinfo-chat.component.scss'
})
export class ActivityinfoChatComponent implements OnInit, OnDestroy, AfterViewInit {
  public showMessagePanel = false;
  @Input() dataSourceTable: any;
  @Input() http_chat_data: any;
  @Input() patientname: string;

  // Flag to indicate data refresh is needed
  public dataRefreshNeeded = false;

  // Timestamp of last update to track changes
  private lastUpdateTimestamp: number = 0;

  dateCompare = 'undefined';
  private chatHistoryDataSubscription: Subscription;
  displaychatColumns = [
    'ContactName',
    'LastMessage',
    'Date',
    'Time',
    'ConversationSid',
    'actions',
  ];

  public dataSources: any;
  public dataSourceChatPaneldata: any;

  constructor(
    private patientchatservice: PatientChatService,
    private rpm: RPMService,
    public datepipe: DatePipe,
    private cdr: ChangeDetectorRef // Add ChangeDetectorRef
  ) {}

  ngOnDestroy(): void {
    // Properly unsubscribe to prevent memory leaks
    if (this.chatHistoryDataSubscription) {
      this.chatHistoryDataSubscription.unsubscribe();
    }
  }

  ngAfterViewInit(): void {
    // Initial UI update after view initialization
    this.cdr.detectChanges();
  }

  ngOnInit(): void {
    this.chatHistoryDataSubscription = this.patientchatservice.chatHistoryData$.subscribe(data => {
      if (data) {
        // Store previous data for comparison
        const previousData = this.http_chat_data ? JSON.stringify(this.http_chat_data) : null;

        // Update with new data
        this.http_chat_data = data;

        // Compare with previous data to see if it changed
        const currentData = JSON.stringify(this.http_chat_data);
        const dataHasChanged = previousData !== currentData;

        // Set refresh flag if data changed or it's the first load
        // if (dataHasChanged) {
        this.dataRefreshNeeded = true;
        this.lastUpdateTimestamp = Date.now();

        // Trigger any refresh actions needed
        this.refreshData();
        // }

        // Mark for check to ensure UI updates
        this.cdr.markForCheck();
      }
    });
  }

  // Method to refresh data and update UI
  refreshData(): void {
    console.log('Refreshing data and UI...');

    // For async operations
    setTimeout(() => {
      // Refresh any other components or update UI as needed
      // For example, if a chat panel is open, refresh its messages
      if (this.showMessagePanel && this.dataSourceChatPaneldata) {
        // Re-fetch the conversation data
        const conversationSid = this.dataSourceChatPaneldata.ConversationSid;
        if (conversationSid) {
          this.getMessagesByConversationSid(conversationSid);
        }
      }

      // Force Angular to check for changes and update the view
      this.cdr.detectChanges();

      // Reset refresh flag after handling the refresh
      this.dataRefreshNeeded = false;

      // Notify parent components or perform other actions as needed
      this.notifyDataRefresh();
    }, 0);
  }

  // Method to notify other components about the refresh
  notifyDataRefresh(): void {
    // You could emit an event to parent components if needed
    // For example: this.dataRefreshed.emit(this.lastUpdateTimestamp);

    // Or you could update a service to indicate the refresh
    // this.patientchatservice.notifyDataRefreshed(this.lastUpdateTimestamp);
  }

  userName: string = '';
  myprofile: any;
  messages: Array<Message> = [];

  getMessagesByConversationSid(conversationSid: string): Message[] {
    this.showMessagePanel = false;

    const convo = this.http_chat_data.find((c: { ConversationSid: string; }) => c.ConversationSid === conversationSid);

    this.dataSourceChatPaneldata = convo;
    this.showMessagePanel = true;

    // Force change detection
    this.cdr.detectChanges();

    return convo ? convo.Messages : [];
  }

  closeChatPanel() {
    this.showMessagePanel = false;
    this.cdr.detectChanges(); // Force UI update
  }

  shouldShowDate(dateTime: string, index: number): boolean {
    if (index === 0) return true; // always show for the first message

    const currentDate = this.datepipe.transform(dateTime, 'yyyy-MM-dd');
    const prevDate = this.datepipe.transform(
      this.dataSourceChatPaneldata.Messages[index - 1]?.DateTime,
      'yyyy-MM-dd'
    );

    return currentDate !== prevDate;
  }

  convertToLocal(dateStr: string): Date {
    return new Date(dateStr); // Automatically converted to browser local time
  }

  convertToLocalTime(utcTimeString: any, date: string): string {
    if (!utcTimeString) {
      return ''; // Return empty string instead of null
    }

    try {
      let cleanUtcString = utcTimeString.toString();
      let dateObj;

      // Remove timezone offset if present (like +0000)
      if (cleanUtcString.includes('+')) {
        cleanUtcString = cleanUtcString.split('+')[0].trim();
      }

      // Handle common UTC formats to create proper Date object
      if (cleanUtcString.endsWith('Z')) {
        // ISO format with Z suffix (e.g., 2025-04-30T06:38:11Z)
        dateObj = new Date(cleanUtcString);
      } else if (cleanUtcString.includes('T')) {
        // ISO format without Z (e.g., 2025-04-30T06:38:11)
        dateObj = new Date(cleanUtcString + 'Z');
      } else {
        // Standard date format (e.g., 4/30/2025 6:38:11 AM)
        dateObj = new Date(cleanUtcString + ' UTC');
      }

      // Check if valid date
      if (isNaN(dateObj.getTime())) {
        console.error('Invalid date format:', utcTimeString);
        return utcTimeString.toString(); // Ensure string return for invalid dates
      }

      // Use DatePipe to transform the Date object with the desired format
      let result;
      if (date == 'date') {
        result = this.datepipe.transform(dateObj, "MMM d, y");
      } else if (date == 'time') {
        result = this.datepipe.transform(dateObj, "h:mm a");
      }

      // Handle potential null from datePipe.transform()
      return result || utcTimeString.toString(); // Return original string if transform returns null
    } catch (error) {
      console.error('Error converting date:', error);
      return utcTimeString.toString(); // Ensure string return on error
    }
  }
}
