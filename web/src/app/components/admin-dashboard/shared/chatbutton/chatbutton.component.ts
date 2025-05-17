// chat.component.ts
import {
  Component,
  OnInit,
  OnDestroy,
  ViewChild,
  ElementRef,
  Input,
  Output,
  EventEmitter,
} from '@angular/core';
import { Subscription } from 'rxjs';
import { PatientChatService } from './service/patient-chat.service';
import { RPMService } from '../../sevices/rpm.service';
import { Client, Message, Conversation } from '@twilio/conversations';

@Component({
  selector: 'app-chatbutton',
  templateUrl: './chatbutton.component.html',
  styleUrls: ['./chatbutton.component.scss'],
})
export class ChatbuttonComponent implements OnInit, OnDestroy {
  @Input() currentuserName: any;
  @Input() chatVariable: any;
  @Input() fcmtoken: string = '';
  @Output() unreadMessageUpdated = new EventEmitter<number>();
  @Output() CloseChatPanlBlock = new EventEmitter();

  @ViewChild('scrollBottom') private scrollBottom: ElementRef;
  @ViewChild('scrollBottom', { static: false })
  private scrollContainer!: ElementRef;

  // Chat state
  chatList: Array<{
    chat: Conversation;
    unreadCount: number | null;
    lastMessage: string | null;
  }> = [];
  messages: Array<Message> = [];
  currentConversation: Conversation | null = null;
  isTyping: boolean = false;
  isLoading: boolean = false;
  error: string = '';
  message: string = '';
  unreadMessagesCount: number = 0;
  heartbeatIntervalId:any;

  // User state
  userName: string = '';
  myprofile: any;
  dateCompare = 'undefined';
  isUserAtBottom: boolean = true;

  // Twilio state
  token: any;
  currentSid: any;
  currentTime: Date = new Date();

  private subscriptions: Subscription[] = [];

  constructor(
    private patientChatService: PatientChatService,
    private rpm: RPMService
  ) {}

  ngOnInit(): void {
    this.scrollToBottom();
    this.getUserName();
    this.openChatPanel();

    // Subscribe to service observables
    this.subscriptions.push(
      this.patientChatService.chatList$.subscribe(
        (list) => (this.chatList = list)
      ),
      // this.patientChatService.messages$.subscribe(
      //   (msgs) => (this.messages = msgs)
      // ),
      // Add this subscription to log messages
this.patientChatService.messages$.subscribe(
  (msgs) => {
    // Assign messages to class property
    this.messages = msgs;

    // Print message details to console
    console.log('===== MESSAGES UPDATE RECEIVED =====');
    console.log(`Total messages: ${msgs.length}`);

    if (msgs.length > 0) {
      // Print the last 5 messages (or all if fewer than 5)
      const messagesToShow = msgs.slice(-5);
      console.log('Last 5 messages:');

      messagesToShow.forEach((message, index) => {
        const position = msgs.length - messagesToShow.length + index + 1;
        console.log(`[${position}/${msgs.length}] From: ${message.body || 'Unknown'}`);

        console.log('-----------------------------------');
      });
    } else {
      console.log('No messages in the conversation.');
    }

    console.log('====================================');
  }
),
      this.patientChatService.currentConversation$.subscribe(
        (conv) => (this.currentConversation = conv)
      ),

      this.patientChatService.error$.subscribe((err) => (this.error = err)),
      this.patientChatService.unreadCount$.subscribe((count) => {
        this.unreadMessagesCount = count;
        this.unreadMessageUpdated.emit(count);
      })
    );
  }

  ngAfterViewChecked() {
    if (this.isUserAtBottom) {
      this.scrollToBottom();
    }
  }

  ngOnDestroy() {
    this.stopChatHeartBeat();

    // Clean up subscriptions
    this.subscriptions.forEach((sub) => sub.unsubscribe());
  }

  // User interaction methods
  async sendMessage() {
    if (!this.message.trim()) return;

    await this.patientChatService.sendMessage(this.message);
    this.message = '';
  }

  async openChat(conv: Conversation) {
    await this.patientChatService.openChat(conv);
  }

  loadMessages() {
    if (this.messages.length > 0 && this.messages[0].index > 0) {
      this.patientChatService.fetchMessages(this.messages[0].index - 1);
    }
  }

  // UI Helpers
  private scrollToBottom(): void {
    if (this.scrollContainer?.nativeElement) {
      const container = this.scrollContainer.nativeElement;
      container.scrollTo({
        top: container.scrollHeight,
        behavior: 'smooth',
      });
    }
  }

  onUserScroll(): void {
    const container = this.scrollContainer?.nativeElement;
    if (!container) return;

    this.isUserAtBottom =
      container.scrollHeight - container.scrollTop <=
      container.clientHeight + 10;
  }

  scrollToLatestMessage() {
    this.isUserAtBottom = true;
    this.scrollToBottom();
  }

  getDatefrom(message: any): boolean {
    if (!message?.dateUpdated) return false;

    const formattedDate = new Date(message.dateUpdated).toLocaleDateString();

    if (formattedDate === this.dateCompare) {
      return false;
    }

    this.dateCompare = formattedDate;
    return true;
  }



  // Initialization methods
  getUserName() {
    return new Promise((resolve) => {
      this.rpm.rpm_get('/api/users/getmyprofiles').then((data) => {
        this.myprofile = data;
        this.userName = this.myprofile.UserName;
        this.patientChatService.setUserName(this.userName);
        resolve(this.userName);
      });
    });
  }

  async openChatPanel() {
    this.chatVariable = true;
    this.currentTime = new Date();
    this.isLoading = true;

    try {
      // Clear existing data first
      this.patientChatService.chatListSubject.next([]);
      this.patientChatService.currentConversationSubject.next(null);
      this.patientChatService.messagesSubject.next([]);
      this.messages = [];
      this.currentConversation = null;

      // Initialize chat service
      await this.patientChatService.ensureInitialized(this.currentuserName);

      // Explicitly tell the service which user's conversations to load
      console.log('Loading conversations for user:', this.currentuserName);

      try {
        // Get the current SID for this specific user
        this.currentSid = await this.patientChatService.getChatId(
          this.currentuserName
        );

        // Force a fresh load of conversations
        await this.patientChatService.fetchUserChats(this.currentSid);
        this.startChatHeartBeat( this.currentSid,this.userName);
      } catch (error: any) {
        if (error.status === 404) {
          //console.error('this Patient is not logged in to Mobile App yet, please ask him to Login for Establishing a conversation.');
           await this.newChat();
        }
      }
    } catch (error) {
      console.error('Failed to initialize chat:', error);
    } finally {
      this.isLoading = false;
    }

    this.scrollToBottom();
  }



// Update the chatHeartBeat method to use proper error handling
chatHeartBeat(currentSid: string, username: string) {
  const payload = {
    "ConversationSid": currentSid,
    "UserName": username,
    "LastActiveAt": new Date().toLocaleString('en-US', {
      timeZone: 'Asia/Kolkata',
      year: 'numeric',
      month: 'numeric',
      day: 'numeric',
      hour: 'numeric',
      minute: 'numeric',
      second: 'numeric',
      hour12: true
    }) + " +05:30"
  };

  console.log("Sending heartbeat with payload:", payload);

  this.rpm.rpm_post('/api/comm/chatheartbeat', payload)
    .then(data => {
      console.log('Heartbeat sent successfully at:', new Date().toLocaleString());
    })
    .catch(err => {
      console.error('Error sending heartbeat:', err);
    });
}
  async startChatHeartBeat(currentSid: string, username: string) {
    // Initial call
    this.chatHeartBeat(currentSid, username);

    // Set up interval for every 15 seconds (15000 milliseconds)
    const heartbeatInterval = setInterval(() => {
      this.chatHeartBeat(currentSid, username);
    }, 10 * 1000);

    this.heartbeatIntervalId = heartbeatInterval;
    console.log('Heartbeat started at 10-second intervals');

    return heartbeatInterval;
  }

  // If you need to stop the heartbeat
  stopChatHeartBeat() {
    if (this.heartbeatIntervalId) {
      clearInterval(this.heartbeatIntervalId);
      this.heartbeatIntervalId = null;
      console.log('Heartbeat stopped');
    }
  }
  async newChat() {
    // Wait for getUserName to complete
    await this.getUserName();
    console.log('Username');
    console.log(this.userName);
    await this.patientChatService.createNewChat(
      this.currentuserName,
      this.userName
    );
  }
  closeChatPanelEmitter() {
    this.stopChatHeartBeat();
    this.CloseChatPanlBlock.emit();
    this.closeChatPanel();
  }

  closeChatPanel() {
    this.patientChatService.cleanup();
  }
  // In your chat component
  deleteMessage(message: Message): void {
    // Confirm deletion
    if (confirm('Are you sure you want to delete this message?')) {
      this.patientChatService
        .deleteMessage(message.index)
        .then(() => {
          console.log('Message deleted');
        })
        .catch((error) => {
          console.error('Failed to delete message:', error);
        });
    }
  }
  // Edit Chat Functionality
  editingMessage: Message | null = null;
  editMessageText: string = '';
  // Add these methods
  startEditingMessage(message: Message): void {
    // Only allow editing your own messages
    if (message.author === this.userName) {
      this.editingMessage = message;
      this.editMessageText = message.body || '';
    }
  }

  cancelEditing(): void {
    this.editingMessage = null;
    this.editMessageText = '';
  }

  saveEdit(): void {
    if (!this.editingMessage || !this.editMessageText.trim()) return;

    this.patientChatService
      .editMessage(this.editingMessage.index, this.editMessageText)
      .then(() => {
        console.log('Message edited successfully');
        this.editingMessage = null;
        this.editMessageText = '';
      })
      .catch((error) => {
        console.error('Failed to edit message:', error);
      });
  }
}
