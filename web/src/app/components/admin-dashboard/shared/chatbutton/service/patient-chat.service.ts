// twilio.service.ts
import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable, Subscription } from 'rxjs';
import {
  Client,
  Message,
  Conversation,
  Participant,
  ConnectionState,
} from '@twilio/conversations';
import { RPMService } from '../../../sevices/rpm.service';
import { getMessaging, onMessage } from 'firebase/messaging';
interface TempMessage extends Partial<Message> {
  sending: boolean;
  failed?: boolean;
  sid: string; // Temporary identifier to replace later
}

@Injectable({
  providedIn: 'root',
})
export class PatientChatService {
  public client: Client | null = null;
  public chatListSubject = new BehaviorSubject<
    Array<{
      chat: Conversation;
      unreadCount: number | null;
      lastMessage: string | null;
    }>
  >([]);
  public messagesSubject = new BehaviorSubject<Array<Message>>([]);
  // private isTypingSubject = new BehaviorSubject<boolean>(false);
  private unreadCountSubject = new BehaviorSubject<number>(0);
  public currentConversationSubject = new BehaviorSubject<Conversation | null>(
    null
  );
  private loadingSubject = new BehaviorSubject<boolean>(false);
  private errorSubject = new BehaviorSubject<string>('');
  private messageListenerRegistered = false;

  public chatList$ = this.chatListSubject.asObservable();
  public messages$ = this.messagesSubject.asObservable();
  // public isTyping$ = this.isTypingSubject.asObservable();
  public unreadCount$ = this.unreadCountSubject.asObservable();
  public currentConversation$ = this.currentConversationSubject.asObservable();
  public loading$ = this.loadingSubject.asObservable();
  public error$ = this.errorSubject.asObservable();
  private chatPanelOpenSubject = new BehaviorSubject<boolean>(false);
  public chatPanelOpen$ = this.chatPanelOpenSubject.asObservable();

  private userName: string = '';
  private currentTime: Date = new Date();
  private currentPatientUser: string = '';

  constructor(private rpm: RPMService) {}

  public initialized = false;

  // Ensure the service initializes only once and stays active
  async ensureInitialized(curremtPatientUser: any): Promise<void> {
    this.currentPatientUser = curremtPatientUser;
    let totalUnreadCount = 0;
    this.unreadCountSubject.next(totalUnreadCount);
    if (!this.userName) {
      const storedName = sessionStorage.getItem('useraccessrights');
      const parsedData = storedName ? JSON.parse(storedName) : null;
      if (storedName) this.userName = parsedData.Username;
    }

    if (this.initialized) return;

    try {
      const token = await this.getToken();
      console.log("Token Chat");
      console.log(token)
      await this.initializeClient(token);

      this.setupPushNotifications();
      this.initialized = true;

      // await this.getTotalUnreadMessages(); // Get initial counts
    } catch (error) {
      console.error('Failed to initialize Twilio service:', error);
    }
  }

  // Token management
  async getToken(): Promise<string> {
    this.setLoading(true);
    try {
       const token = await this.rpm.rpm_get(`/api/comm/getchattoken?app=web`) as { message: string };
     // const token = await this.rpm.rpm_get(`/api/comm/getchattoken?app=web`) as string;
      console.log('Token');
      console.log(token);
     // return token;
       return token.message as string;
    } catch (error) {
      console.error('❌ Error getting chat token:', error);
      throw error;
    } finally {
      this.setLoading(false);
    }
  }

  async refreshToken(): Promise<string> {
    this.setLoading(true);
    try {
      const data = (await this.rpm.rpm_get(
        `/api/comm/regeneratechattoken?app=web`
      )) as { message: string };
      if (!data) throw new Error('❌ Failed to retrieve chat token.');

      if (this.client) {
        await this.client.updateToken(data.message);
      }
      return data.message;
    } catch (error) {
      console.error('❌ Error regenerating chat token:', error);
      throw error;
    } finally {
      this.setLoading(false);
    }
  }

  // Client initialization
  async initializeClient(token: string): Promise<void> {
    this.setLoading(true);
    try {
      this.client = new Client(token);
      // Wait until the client is ready
      this.client.on('stateChanged', async (state) => {
        console.log(' Client state changed:', state);
        if (state === 'failed') {
          console.error(' Twilio Client Initialization Failed!');
          await this.handleClientFailure();
        }
        if (state === 'initialized') {
          console.log('Twilio Client is Ready!');
          this.setupEventListeners(); // Attach event listeners
        }
      });

      // this.client.on('initialized', () => {
      //   console.log('Twilio Client Fully Initialized!');
      //   this.setupEventListeners(); // Attach event listeners
      // });
    } catch (error) {
      console.error(' Error initializing Twilio client:', error);
      throw error;
    } finally {
      this.setLoading(false);
    }
  }

  async handleClientFailure() {
    console.warn('Retrying Twilio Client Initialization...');

    try {
      const newToken = await this.refreshToken(); // Fetch a fresh token
      await this.initializeClient(newToken);
      console.log(' Twilio Client Successfully Reconnected!');
    } catch (error) {
      console.error(' Twilio Client Reconnection Failed:', error);
    }
  }

  // Event listeners
  private setupEventListeners(): void {
    if (!this.client) return;

    if (!this.messageListenerRegistered) {
      this.client.on('messageAdded', this.handleMessageAdded);
      this.messageListenerRegistered = true;
    }

    // CLIENT INITIALIZATION EVENTS
    this.client.on('initialized', () => {
      console.log('Twilio Chat Client Initialized');
    });

    this.client.on('initFailed', (error: any) => {
      console.error(' Client initialization failed:', error);
      this.setError('Failed to initialize chat client');
    });

    // CONNECTION EVENTS
    this.client.on('connectionStateChanged', (state: ConnectionState) => {
      console.log('Connection State Changed:', state);
    });

    this.client.on('connectionError', (error: any) => {
      console.error(' Connection Error:', error);
      this.setError('Connection error');
    });

    this.client.on('conversationJoined', (conversation) => {
      console.log('Joined Conversation:', conversation.sid);
    });

    // TOKEN EVENTS
    this.client.on('tokenAboutToExpire', async () => {
      console.warn('Token is about to expire. Refreshing token...');
      await this.refreshToken();
    });

    this.client.on('tokenExpired', async () => {
      console.error('Token Expired - Fetching new token...');
      this.client?.removeAllListeners();
      const token = await this.refreshToken();
      await this.initializeClient(token);
    });



    // CONVERSATION EVENTS
    this.client.on('conversationAdded', async (conv: Conversation) => {
      console.log('📢 New Conversation Added:', conv);

      const currentConversationId = await this.getExistingConversation(
        this.client,
        this.userName,
        this.currentPatientUser
      );
      if (!currentConversationId) {
        return;
      }
      if (currentConversationId == conv.sid) {
        this.getUnreadMessagesForConversation(currentConversationId);
      }

      const chatList = this.chatListSubject.getValue();

      if (conv.dateCreated && conv.dateCreated > this.currentTime) {
        await conv.setAllMessagesUnread();
        const updatedChatList = [
          { chat: conv, unreadCount: 0, lastMessage: '' },
          ...chatList,
        ];
        this.chatListSubject.next(updatedChatList);
      }
    });

    // TYPING EVENTS
    // this.client.on('typingStarted', (user: Participant) => {
    //   try {
    //     const currentConversation = this.currentConversationSubject.getValue();
    //     if (user.conversation.sid === currentConversation?.sid) {
    //       this.isTypingSubject.next(true);
    //     }
    //   } catch (error) {
    //     console.error(' Error handling typing event:', error);
    //   }
    // });

    // this.client.on('typingEnded', (user: Participant) => {
    //   try {
    //     const currentConversation = this.currentConversationSubject.getValue();
    //     if (user.conversation.sid === currentConversation?.sid) {
    //       this.isTypingSubject.next(false);
    //     }
    //   } catch (error) {
    //     console.error(' Error handling typing event:', error);
    //   }
    // });
  }

  private handleMessageAdded = async (msg: Message) => {
    console.log('New Message Received:', msg);

    const currentConversation = this.currentConversationSubject.getValue();
    const chatList = this.chatListSubject.getValue();
    const messages = this.messagesSubject.getValue();
    const isChatOpen = this.chatPanelOpenSubject.getValue();

    if (!this.userName) {
      const storedName = sessionStorage.getItem('useraccessrights');
      const parsedData = storedName ? JSON.parse(storedName) : null;
      if (parsedData) this.userName = parsedData.Username;
    }

    const isIncoming = msg.author !== this.userName;
    console.log(
      `Message author: ${msg.author}, currentUser: ${this.userName}, isIncoming: ${isIncoming}`
    );

    if (currentConversation?.sid === msg.conversation.sid && isChatOpen) {
      this.messagesSubject.next([...messages, msg]);
      await currentConversation.updateLastReadMessageIndex(msg.index);

      const updatedChatList = chatList.map((el) =>
        el.chat.sid === currentConversation.sid
          ? { ...el, lastMessage: msg.body || el.lastMessage }
          : el
      );
      this.chatListSubject.next(updatedChatList);
    } else if (isIncoming) {
      const updatedChatList = chatList.map((el) => {
        if (el.chat.sid === msg.conversation.sid) {
          return {
            ...el,
            lastMessage: msg.body || el.lastMessage,
            unreadCount: (el.unreadCount || 0) + 1,
          };
        }
        return el;
      });
      this.chatListSubject.next(updatedChatList);
      this.updateTotalUnreadCount();
    } else {
      const updatedChatList = chatList.map((el) =>
        el.chat.sid === msg.conversation.sid
          ? {
              ...el,
              lastMessage: msg.body || el.lastMessage,
            }
          : el
      );
      this.chatListSubject.next(updatedChatList);
    }
  };

  async fetchUserChats(conversationSid: string): Promise<void> {
    this.setLoading(true);

    try {
      if (!this.client) throw new Error('Client not initialized');

      //  Clear previous chat list and messages
      this.chatListSubject.next([]);
      this.currentConversationSubject.next(null);
      this.messagesSubject.next([]);

      //  Fetch conversation directly by SID
      const conversation = await this.client.getConversationBySid(
        conversationSid
      );
      console.log(' Fetched conversation:', conversation);

      //  Create a single chat entry
      const chatList = [
        {
          chat: conversation,
          unreadCount: 0,
          lastMessage: '',
        },
      ];

      //  Update the chat list
      this.chatListSubject.next(chatList);

      //  Open the conversation
      await this.openChat(conversation);

      console.log(' Chat opened successfully');
    } catch (error) {
      console.error(' Error fetching conversation:', error);
      this.setError('Failed to load chat');
    } finally {
      this.setLoading(false);
    }
  }


  // Add this helper method




  async openChat(conversation: Conversation): Promise<void> {
    this.currentConversationSubject.next(conversation);
    await this.fetchMessages();
  }

  async fetchMessages(skip?: number): Promise<void> {
    this.setLoading(true);

    try {
      const currentConversation = this.currentConversationSubject.getValue();
      if (!currentConversation) throw new Error('No active conversation.');

      const currentMessages = skip ? this.messagesSubject.getValue() : [];
      const result = await currentConversation.getMessages(30, skip);

      const allMessages = skip
        ? [...result.items, ...currentMessages]
        : result.items;

      this.messagesSubject.next(allMessages);

      if (!skip) {
        let resetTo =
          allMessages.length >= 1
            ? allMessages[allMessages.length - 1].index
            : 0;
        await currentConversation.updateLastReadMessageIndex(resetTo);

        const chatList = this.chatListSubject.getValue();
        const updatedChatList = chatList.map((el) =>
          el.chat.sid === currentConversation.sid
            ? { ...el, unreadCount: 0 }
            : el
        );

        this.chatListSubject.next(updatedChatList);
        console.log('Fetch Messages :');
        this.updateTotalUnreadCount();
      }
    } catch (error) {
      console.error(' Error fetching messages:', error);
      this.setError('Failed to load messages');
    } finally {
      this.setLoading(false);
    }
  }

  async sendMessage(message: string): Promise<void> {
    console.log('Message');
    console.log(message);
    if (!message.trim()) return;

    try {
      const currentConversation = this.currentConversationSubject.getValue();
      if (!currentConversation) {
        throw new Error('No active conversation selected.');
      }

      // Get current messages
      const currentMessages = this.messagesSubject.getValue();

      // 🟢 Send message via Twilio API (returns index number)
      const messageIndex = await currentConversation.sendMessage(message);

      // 🟢 Fetch the actual message from Twilio
      const fetchedMessages = await currentConversation.getMessages();
      const sentMessage = fetchedMessages.items.find(
        (m) => m.index === messageIndex
      );

      if (!sentMessage) {
        throw new Error('Message sent but not found in conversation history.');
      }

      // 🟢 Update chat list with the new message
      const chatList = this.chatListSubject.getValue();
      console.log(' Chat List');
      console.log(chatList);
      const updatedChatList = chatList.map((el) =>
        el.chat.sid === currentConversation.sid
          ? { ...el, lastMessage: message.trim() }
          : el
      );
      console.log('Update Chat List');
      console.log(chatList);

      this.chatListSubject.next(updatedChatList);
      this.chatNotify(currentConversation.sid,this.currentPatientUser,this.userName,message);
    } catch (error) {
      console.error('Error sending message:', error);
      this.setError('Failed to send message. Try again.');

      // 🛑 Remove temporary message if sending failed
      this.messagesSubject.next(
        this.messagesSubject.getValue().filter((m) => m.body !== message.trim())
      );
    }
  }

  async createNewChat(
    currentUserName: string,
    userName: string
  ): Promise<void> {
    this.setLoading(true);

    try {
      if (!this.client) throw new Error('Client not initialized');

      const user = await this.client.getUser(currentUserName);
      console.log('username'+userName);
      console.log('currentUserName'+currentUserName)
      const channel = await this.client.createConversation({
        friendlyName: `${userName}-${currentUserName}`,
      });

      await channel.join();
      await channel.setAllMessagesUnread();
      await channel.add(currentUserName);

      this.currentConversationSubject.next(channel);

      // Update chat list
      const chatList = this.chatListSubject.getValue();
      this.chatListSubject.next([
        { chat: channel, unreadCount: 0, lastMessage: '' },
        ...chatList,
      ]);

      await this.updateChatResource(channel, currentUserName);
      await this.openChat(channel);
    } catch (error) {
      console.error(' Error creating new chat:', error);
      this.setError('User not found or chat creation failed.');
    } finally {
      this.setLoading(false);
    }
  }

  async updateChatResource(
    channel: Conversation,
    patientUsername: string
  ): Promise<void> {
    try {
      const token = await this.getToken();
      const req_body = {
        ToUser: patientUsername,
        ConversationSid: channel.sid,
        ChatToken: token,
      };

      await this.rpm.rpm_post(`/api/comm/updatechatresource`, req_body);
    } catch (error) {
      console.error(' Error updating chat resource:', error);
      throw error;
    }
  }

  async getChatId(currentUserName: string): Promise<string> {
    try {
      const response = (await this.rpm.rpm_get(
        `/api/comm/getchatsid?ToUser=${currentUserName}`
      )) as  { message: string };

      return response.message;
    } catch (error) {
      console.error(' Error getting chat ID:', error);
      throw error;
    }
  }

  async getUnreadMessagesForConversation(conversationId: any): Promise<number> {
    try {
      // const currentConversation = this.currentConversationSubject.getValue();

      if (!this.client) return 0;
      if (!conversationId) return 0;
      console.log('getExisting Conversation ');

      console.log('conservation id');
      console.log(conversationId);
      // Get the specific conversation by SID
      const conversation = await this.client.getConversationBySid(
        conversationId
      );
      const lastReadMessageIndex = await conversation.lastReadMessageIndex; // Last message read by user
      const messages = await conversation.getMessages(); // Fetch messages
      const latestMessageIndex =
        messages.items.length > 0
          ? messages.items[messages.items.length - 1].index
          : -1;

      if (lastReadMessageIndex === null) {
        return latestMessageIndex + 1; // All messages are unread
      }

      const unreadCount = latestMessageIndex - lastReadMessageIndex;
      let totalUnreadCount = unreadCount > 0 ? unreadCount : 0;
      this.unreadCountSubject.next(totalUnreadCount);
      return totalUnreadCount;
    } catch (error) {
      console.error(`Error fetching unread messages for conversation:`, error);
      return 0;
    }
  }

  private updateTotalUnreadCount(): void {
    const chatList = this.chatListSubject.getValue();
    console.log('ChatList Updated TotalCount');
    console.log(chatList);
    const totalUnread = chatList.reduce(
      (total, chat) => total + (chat.unreadCount || 0),
      0
    );
    console.log('updateTotalUnreadCount TotalCount');
    console.log(totalUnread);
    this.unreadCountSubject.next(totalUnread);
  }

  setupPushNotifications(): void {
    const messaging = getMessaging();
    onMessage(messaging, (payload) => {
      if (this.client) {
        this.client.handlePushNotification(payload);
      }
    });
  }

  setUserName(name: string): void {
    this.userName = name;
  }

  private setLoading(isLoading: boolean): void {
    this.loadingSubject.next(isLoading);
  }

  private setError(error: string): void {
    this.errorSubject.next(error);
    setTimeout(() => this.errorSubject.next(''), 3000);
  }

  cleanup(): void {
    // if (this.client) {
    //   this.client.removeAllListeners();
    //   this.client.shutdown();
    // }
  }
  setChatPanelOpen(isOpen: boolean): void {
    console.log(`Setting chat panel open: ${isOpen}`);
    this.chatPanelOpenSubject.next(isOpen);
  }

  shutdown(): void {
    if (this.client) {
      this.client.removeAllListeners();
      this.client.shutdown();
      this.client = null;
      this.initialized = false;
    }
  }

  async deleteMessage(messageIndex: number): Promise<void> {
    try {
      const currentConversation = this.currentConversationSubject.getValue();
      if (!currentConversation) {
        throw new Error('No active conversation.');
      }

      // Get the message by index
      const messages = this.messagesSubject.getValue();
      const messageToDelete = messages.find(
        (msg) => msg.index === messageIndex
      );

      if (!messageToDelete) {
        throw new Error('Message not found.');
      }

      // Remove the message
      await messageToDelete.remove();

      // Update the messages list
      const updatedMessages = messages.filter(
        (msg) => msg.index !== messageIndex
      );
      this.messagesSubject.next(updatedMessages);

      console.log('Message deleted successfully');
    } catch (error) {
      console.error(' Error deleting message:', error);
      this.setError('Failed to delete message.');
      throw error;
    }
  }

  async editMessage(messageIndex: number, newBody: string): Promise<void> {
    try {
      const currentConversation = this.currentConversationSubject.getValue();
      if (!currentConversation) {
        throw new Error('No active conversation.');
      }

      // Get the message by index
      const messages = this.messagesSubject.getValue();
      const messageToEdit = messages.find((msg) => msg.index === messageIndex);

      if (!messageToEdit) {
        throw new Error('Message not found.');
      }

      // Update the message in Twilio
      await messageToEdit.updateBody(newBody);

      // Completely refresh all messages
      await this.fetchMessages();

      console.log('Message edited successfully');
    } catch (error) {
      console.error('❌ Error editing message:', error);
      this.setError('Failed to edit message.');
      throw error;
    }
  }

  private async getExistingConversation(client: any, user1: any, user2: any) {
    const conversations = await client.getSubscribedConversations();
    console.log('getExisting Conversation ');

    for (let conversation of conversations.items) {
      const participants = await conversation.getParticipants();
      const participantIdentities = participants.map(
        (p: { identity: any }) => p.identity
      );

      if (
        participantIdentities.includes(user1) &&
        participantIdentities.includes(user2)
      ) {

        return conversation.sid; // Return existing conversation ID
      }
    }

    console.log('No existing conversation found');
    return null;
  }


  chatNotify(currentSid: string, username: string,fromUser:string,message:string) {
    const payload = {
      "ConversationSid": currentSid,
      "ToUser": username,
      "FromUser": fromUser,
      "Message":message
    };

    console.log("Sending heartbeat with payload:", payload);

    this.rpm.rpm_post('/api/comm/NotifyConversation', payload)
      .then(data => {
        console.log('Notification Sent');
      })
      .catch(err => {
        console.error('Error Notification Sent', err);
      });
  }
}
