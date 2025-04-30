import { Component, Input, OnInit } from '@angular/core';
import { PatientChatService } from 'src/app/components/admin-dashboard/shared/chatbutton/service/patient-chat.service';
import { RPMService } from 'src/app/components/admin-dashboard/sevices/rpm.service';
import { DatePipe } from '@angular/common';
import moment from 'moment';
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
export class ActivityinfoChatComponent {
public showMessagePanel=false;
@Input() dataSourceTable:any;
@Input() http_chat_data:any;
@Input() patientname:string;
dateCompare = 'undefined';

displaychatColumns = [
  'ContactName',
  'LastMessage',
  'Date',
  'Time',
  'ConversationSid',
  'actions',
];
public dataSources:any;
public dataSourceChatPaneldata:any;
constructor( private patientchatservice: PatientChatService,private rpm: RPMService,public datepipe: DatePipe,
){
//this.getUserName();

}


userName:string='';
  myprofile:any;
  messages: Array<Message> = [];
  // getUserName() {
  //   this.rpm.rpm_get('/api/users/getmyprofiles').then((data) => {
  //     this.myprofile = data;
  //     this.userName = this.myprofile.UserName

  //   });
  // }


 getMessagesByConversationSid(conversationSid: string): Message[] {
  this.showMessagePanel = false;
  console.log(this.http_chat_data)
  const convo = this.http_chat_data.find((c: { ConversationSid: string; }) => c.ConversationSid === conversationSid);
  console.log(convo)
  this.dataSourceChatPaneldata = convo;
  this.showMessagePanel = true;
  return convo ? convo.Messages : [];
}
closeChatPanel()
{
  this.showMessagePanel = false;
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
convertToLocalTime(utcTimeString: any,date:string): string {
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
    if(date == 'date')
    {
       result = this.datepipe.transform(dateObj, "MMM d, y");
    }else if(date == 'time')
    {
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
