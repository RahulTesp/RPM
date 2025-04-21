import { Component, Input, OnInit } from '@angular/core';
import { PatientChatService } from 'src/app/components/admin-dashboard/shared/chatbutton/service/patient-chat.service';
import { RPMService } from 'src/app/components/admin-dashboard/sevices/rpm.service';
import { DatePipe } from '@angular/common';
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

}


userName:string='';
  myprofile:any;
  messages: Array<Message> = [];
  getUserName() {
    this.rpm.rpm_get('/api/users/getmyprofiles').then((data) => {
      this.userName = this.myprofile.UserName;
    });
  }


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
}
