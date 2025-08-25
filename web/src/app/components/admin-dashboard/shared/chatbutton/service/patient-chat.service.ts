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
import {PatientDataDetailsService} from '../../../../admin-dashboard/components/patient-detail-page/Models/service/patient-data-details.service'
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
  private chatHistoryDataSubject = new BehaviorSubject<any>(null);

  // Observable that components can subscribe to
  public chatHistoryData$: Observable<any> = this.chatHistoryDataSubject.asObservable();
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
  public LoginStatus = true;
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
      // console.log("Token Chat");
      // console.log(token)
      await this.initializeClient(token);

     // this.setupPushNotifications();
      this.initialized = true;
    } catch (error) {
      console.error('Failed to initialize Twilio service:', error);
    }
  }

  // Token management
  async getToken(): Promise<string> {
    this.setLoading(true);
    try {
       const token = await this.rpm.rpm_get(`/api/comm/getchattoken?app=web`) as { message: string };
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
      if (!data.message) throw new Error('❌ Failed to retrieve chat token.');

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
      console.log('New Client Creation');
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
     // const newToken = await this.refreshToken();
      const newToken = await this.getToken();// Fetch a fresh token
      await this.initializeClient(newToken);
      console.log(' Twilio Client Successfully Reconnected!');
    } catch (error) {
      console.error(' Twilio Client Reconnection Failed:', error);
    }
  }
private messageAddedListenerCount: number = 0;
  // Event listeners
  private setupEventListeners(): void {
    if (!this.client) return;
     console.log('message Listener');
     console.log(this.messageListenerRegistered)
     this.removeMessageListeners();
    this.client.on('messageAdded', async (msg: Message) => {
      console.log('message Listener1');
     console.log(this.messageListenerRegistered)
       if (!this.messageListenerRegistered)
       {
               this.messageAddedListenerCount++;
               console.log(`messageAdded event received. Total events: ${this.messageAddedListenerCount}`);

              // Get the current conversation ID
               const currentConversation = this.currentConversationSubject.getValue();
               const currentConversationId = currentConversation?.sid;
               console.log(`Message belongs to current conversation (${currentConversationId}). Processing...`);
               this.handleMessageAdded(msg);

       }

       });
      this.client.on('messageRemoved', this.handleMessageRemoved);
      this.client.on('messageUpdated', this.handleMessageUpdated);



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
     // console.log('Joined Conversation:', conversation.sid);
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



    this.client.on('conversationAdded', async (conv: Conversation) => {
     // console.log('📢 New Conversation Added:', conv.sid);


      // Get participants for this conversation
      try {
        const participants = await conv.getParticipants();
       // console.log('Conversation participants count:', participants.length);

        // Log each participant's identity
        const participantIdentities = participants.map(p => p.identity);
        //console.log('Participant identities:', participantIdentities);

        // Normalize identities for comparison
        const normalizedUserName = this.userName ? String(this.userName).trim().toLowerCase() : '';
        const normalizedPatientUser = this.currentPatientUser ?
          String(this.currentPatientUser).trim().toLowerCase() : '';

        const normalizedParticipants = participantIdentities.map(id =>
          id ? String(id).trim().toLowerCase() : '');

        // Check if this conversation includes both current users
        const hasCurrentUser = normalizedParticipants.includes(normalizedUserName);
        const hasPatientUser = normalizedParticipants.includes(normalizedPatientUser);

        // If both users are in this conversation, we can use this conversation directly
        if (hasCurrentUser && hasPatientUser) {
          console.log('This is the conversation for current users:', conv.sid);
          this.currentConversationSubject.next(conv);
          // Get unread messages for this conversation
          this.getUnreadMessagesForConversation(conv.sid);

          // Add to chat list if not already there
          const chatList = this.chatListSubject.getValue() || [];

          // Check if this conversation is already in the list
          const conversationExists = chatList.some(item => item.chat.sid === conv.sid);

          if (!conversationExists) {
            console.log('Adding conversation to chat list');
            const updatedChatList = [
              {
                chat: conv,
                unreadCount: 0,
                lastMessage: '',
              },
              ...chatList,
            ];

            this.chatListSubject.next(updatedChatList);
          }

          return; // Exit early since we found and handled the conversation
        }
      } catch (error) {
        console.error('Error getting conversation participants:', error);
      }

      // If we didn't find the conversation above, use the existing method
      try {
        // const currentConversationId = await this.getExistingConversation(
        //   this.client,
        //   this.userName,
        //   this.currentPatientUser
        // );

        // console.log('username:', this.userName);
        // console.log('currentPatientUser:', this.currentPatientUser);
        // console.log('📢 New currentConversationId Added:', currentConversationId);

        // if (!currentConversationId) {
        //   console.log('No existing conversation found for current users');
        //   return;
        // }

        // if (currentConversationId === conv.sid) {
        //   console.log('Conversation matches the one for current users');
        //   this.getUnreadMessagesForConversation(currentConversationId);
        // }

        const chatList = this.chatListSubject.getValue() || [];
       // console.log('chatList count:', chatList ? chatList.length : 0);

        if (conv.dateCreated && conv.dateCreated > this.currentTime) {
          await conv.setAllMessagesUnread();

          // Check if this conversation is already in the list
          const conversationExists = chatList.some(item => item.chat.sid === conv.sid);

          if (!conversationExists) {
            const updatedChatList = [
              {
                chat: conv,
                unreadCount: 0,
                lastMessage: '',
              },
              ...chatList,
            ];

            console.log('Adding new conversation to chat list');
            this.chatListSubject.next(updatedChatList);
          }
        }
      } catch (error) {
        console.error('Error in conversationAdded handler:', error);
      }
    });

    // TYPING EVENTS
    this.client.on('typingStarted', (user: Participant) => {
      try {
        const currentConversation = this.currentConversationSubject.getValue();
        if (user.conversation.sid === currentConversation?.sid) {
          console.log('Typing')
        }
      } catch (error) {
        console.error(' Error handling typing event:', error);
      }
    });

    this.client.on('typingEnded', (user: Participant) => {
      try {
        const currentConversation = this.currentConversationSubject.getValue();
        if (user.conversation.sid === currentConversation?.sid) {
           console.log('Typing')
        }
      } catch (error) {
        console.error(' Error handling typing event:', error);
      }
    });
  }
private removeMessageListeners(): void {
  if (this.client && this.messageListenerRegistered) {
    console.log('Removing existing message listeners');
    this.client.off('messageAdded', this.handleMessageAdded);
    this.client.off('messageRemoved', this.handleMessageRemoved);
    this.client.off('messageUpdated', this.handleMessageUpdated);
    this.messageListenerRegistered = false;
  }
}


  private handleMessageAdded = async (msg: Message) => {
    console.log('New Message Received:', msg);

    // Get current state

    const currentConversation = this.currentConversationSubject.getValue();
    const chatList = this.chatListSubject.getValue() || [];
    this.fetchMessages().catch(error => {
      console.error('Error in background message fetching:', error);
    });
    const messages = this.messagesSubject.getValue();
    const isChatOpen = this.chatPanelOpenSubject.getValue();


    // Set username if needed
    if (!this.userName) {
      const storedName = sessionStorage.getItem('useraccessrights');
      const parsedData = storedName ? JSON.parse(storedName) : null;
      if (parsedData) this.userName = parsedData.Username;
    }

    const isIncoming = msg.author !== this.userName;
    console.log(
      `Message author: ${msg.author}, currentUser: ${this.userName}, isIncoming: ${isIncoming}`
    );

    // Check if the conversation exists in the chat list
    const conversationExists = chatList.some(item => item.chat.sid === msg.conversation.sid);
    console.log('Conversation exists in chat list:', conversationExists);

    // If conversation doesn't exist in chat list, add it first
    if (!conversationExists) {
      try {
        console.log('Adding new conversation to chat list:', msg.conversation.sid);

        // We already have the conversation object from the message
        const newChatList = [
          {
            chat: msg.conversation,
            unreadCount: isIncoming ? 1 : 0,
            lastMessage: msg.body || ''
          },
          ...chatList
        ];
        this.chatListSubject.next(newChatList);

        // Update total unread count if incoming message
        if (isIncoming) {

          this.updateTotalUnreadCount();
        }

        // After adding the conversation, continue to process the message
        this.getPatientChat(this.currentPatientUser);
        return;
      } catch (error) {
        console.error('Error adding conversation to chat list:', error);
      }
    }

    // Now handle existing conversations
    if (currentConversation?.sid === msg.conversation.sid && isChatOpen) {
      // Current conversation is open
      this.messagesSubject.next([...messages, msg]);
      await currentConversation.updateLastReadMessageIndex(msg.index);

      const updatedChatList = chatList.map((el) =>
        el.chat.sid === currentConversation.sid
          ? { ...el, lastMessage: msg.body || el.lastMessage }
          : el
      );
      console.log('updatedChatList - handleMessageAdded-currentConversation-msg.conversation.sid');
      console.log(updatedChatList);
      this.chatListSubject.next(updatedChatList);
    } else if (isIncoming) {
      // Incoming message for non-active conversation
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
      console.log('updatedChatList - handleMessageAdded2');
      console.log(updatedChatList);

      this.chatListSubject.next(updatedChatList);
      this.updateTotalUnreadCount();
    } else {
      // Outgoing message for non-active conversation
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
    this.messageListenerRegistered = true;
    this.getPatientChat(this.currentPatientUser);
  };
// Optimized version of the chat service methods
async fetchUserChats(conversationSid: string): Promise<void> {
  this.setLoading(true);
  try {
    if (!this.client) throw new Error('Client not initialized');

    this.resetState();
    console.time('conversation-fetch');
    const conversation = await this.client.getConversationBySid(conversationSid);
    console.log(conversation)
    console.timeEnd('conversation-fetch');

    if (!conversation) {
      throw new Error(`Conversation with SID ${conversationSid} not found`);
    }
    this.chatListSubject.next([{
      chat: conversation,
      unreadCount: 0,
      lastMessage: '',
    }]);

    this.openChat(conversation);

  } catch (error) {
    console.log('Fetch User Chat Details Error')
    console.error('Error fetching conversation:', error);
    this.setError('Failed to load chat');
  } finally {
    this.setLoading(false);
  }
}
private resetState(): void {
  this.chatListSubject.next([]);
  this.currentConversationSubject.next(null);
  this.messagesSubject.next([]);
}

async openChat(conversation: Conversation): Promise<void> {
  this.currentConversationSubject.next(conversation);
  this.fetchMessages().catch(error => {
    console.error('Error in background message fetching:', error);
  });
}

async fetchMessages(skip?: number): Promise<void> {
  const startTime = performance.now();
  const pageSize = 50; // Adjust based on your needs

  try {
    const currentConversation = this.currentConversationSubject.getValue();
    if (!currentConversation) throw new Error('No active conversation.');

    const currentMessages = skip ? this.messagesSubject.getValue() : [];

    // Add pagination parameters
    const options = {
      pageSize,
      page: skip ? Math.floor(currentMessages.length / pageSize) : 0
    };

    console.time('messages-fetch');
    const result = await currentConversation.getMessages(pageSize);
    console.timeEnd('messages-fetch');

    // Process messages in batches to avoid UI freezes
    const allMessages = skip
      ? [...result.items, ...currentMessages]
      : result.items;

    // Update messages
    this.messagesSubject.next(allMessages);

    // Update read status only if we're loading the first batch and have messages
    if (!skip && allMessages.length > 0) {
      this.updateReadStatus(currentConversation, allMessages);
    }

  } catch (error) {
    console.error('Error fetching messages:', error);
    this.setError('Failed to load messages');
  } finally {
    const endTime = performance.now();
    //console.log(`fetchMessages took ${(endTime - startTime).toFixed(2)} ms`);
    this.setLoading(false);
  }
}

private async updateReadStatus(conversation: Conversation, messages: any[]): Promise<void> {
  try {
    const isChatOpen = this.chatPanelOpenSubject.getValue();
    console.log('Chat Panel Open updateReadStatus');
    console.log(isChatOpen)
     if (isChatOpen) {
        const lastMessage = messages[messages.length - 1];

    // Only update if we have a valid index
    if (lastMessage && typeof lastMessage.index === 'number') {
      await conversation.updateLastReadMessageIndex(lastMessage.index);

      // Update unread counts
      const chatList = this.chatListSubject.getValue();
      this.chatListSubject.next(
        chatList.map((el) =>
          el.chat.sid === conversation.sid
            ? { ...el, unreadCount: 0 }
            : el
        )
      );

      this.updateTotalUnreadCount();
    }
     }

  } catch (error) {
    console.error('Error updating read status:', error);
    // Don't throw - this is a non-critical operation
  }
}


  async sendMessage(message: string): Promise<void> {

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

      const updatedChatList = chatList.map((el) =>
        el.chat.sid === currentConversation.sid
          ? { ...el, lastMessage: message }
          : el
      );
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

      // Verify both users exist before creating the conversation
      //const currentUser = await this.client.getUser(currentUserName);

      // Create the conversation channel
      const channel = await this.client.createConversation({
        friendlyName: `${userName}-${currentUserName}`,
      });

      // Join the channel
      await channel.join();
      await channel.setAllMessagesUnread();

      // Add the other user if it's a different user
      if (userName !== currentUserName) {
        await channel.add(currentUserName);

      }

      // Make sure current user is added (this might be redundant if join() adds the user)
      // await channel.add(currentUserName);

      // Update UI state
      this.currentConversationSubject.next(channel);

      // Update chat list
      const chatList = this.chatListSubject.getValue();
      this.chatListSubject.next([
        { chat: channel, unreadCount: 0, lastMessage: '' },
        ...chatList,
      ]);

      await this.updateChatResource(channel, currentUserName);
      await this.openChat(channel);
      this.setError('');
      this.LoginStatus = true;
    } catch (error) {
      this.LoginStatus = false;
      console.error('Error creating new chat:', error);
      this.setError('Patient login required. Please have the patient sign in to the mobile app to begin the conversation.')

    //  this.setError(error instanceof Error ? error.message : 'Chat creation failed.');
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

      throw error;
    }
  }

  async getUnreadMessagesForConversation(conversationId: any): Promise<number> {
    try {

      if (!this.client) return 0;
      if (!conversationId) return 0;

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
      console.log('Last Message latestMessageIndex');
      console.log(latestMessageIndex);
      console.log('lastReadMessageIndex');
      console.log(lastReadMessageIndex)
      let totalUnreadCount = unreadCount > 0 ? unreadCount : 0;
      this.unreadCountSubject.next(totalUnreadCount);
     await this.updateChatListUnreadCount(conversationId, totalUnreadCount, messages.items);

      return totalUnreadCount;
    } catch (error) {
      console.error(`Error fetching unread messages for conversation:`, error);
      return 0;
    }
  }
private async updateChatListUnreadCount(
  conversationId: string,
  unreadCount: number,
  messages?: any[]
): Promise<void> {
  try {
    const chatList = this.chatListSubject.getValue() || [];

    // Find the conversation in the chat list
    const conversationIndex = chatList.findIndex(
      item => item.chat.sid === conversationId
    );

    if (conversationIndex !== -1) {
      // Update existing conversation in chat list
      const updatedChatList = [...chatList];
      updatedChatList[conversationIndex] = {
        ...updatedChatList[conversationIndex],
        unreadCount: unreadCount
      };

      // Optionally update last message if messages are provided
      if (messages && messages.length > 0) {
        const lastMessage = messages[messages.length - 1];
        updatedChatList[conversationIndex].lastMessage =
          lastMessage.body || '(Media message)';
      }

      this.chatListSubject.next(updatedChatList);
      console.log(`Updated chat list unread count for conversation ${conversationId}: ${unreadCount}`);
    } else {
      // If conversation not in chat list, optionally add it
      console.log(`Conversation ${conversationId} not found in chat list`);

      // Uncomment below if you want to add missing conversations to chat list
      /*
      try {
        const conversation = await this.client.getConversationBySid(conversationId);
        const newChatItem = {
          chat: conversation,
          unreadCount: unreadCount,
          lastMessage: messages && messages.length > 0
            ? (messages[messages.length - 1].body || '(Media message)')
            : ''
        };

        const updatedChatList = [newChatItem, ...chatList];
        this.chatListSubject.next(updatedChatList);
        console.log(`Added new conversation to chat list: ${conversationId}`);
      } catch (error) {
        console.error('Error adding conversation to chat list:', error);
      }
      */
    }
  } catch (error) {
    console.error('Error updating chat list unread count:', error);
  }
}
  private updateTotalUnreadCount(): void {
    const chatList = this.chatListSubject.getValue();
    console.log('ChatList Updated TotalCount');
    console.log(chatList);


     const totalUnread = chatList.reduce(
    (total, chat) => {
      const chatUnread = chat.unreadCount || 0;

       console.log(`Total ${total} from chat unread ${chat.unreadCount}`);
      return total + chatUnread;
    },
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

  public setError(error: string): void {
    this.errorSubject.next(error);
   // setTimeout(() => this.errorSubject.next(''), 3000);
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

  private handleMessageRemoved = async (message: Message): Promise<void> => {
    try {
      console.log('Message Removed:', message.sid);
      const currentConversation = this.currentConversationSubject.getValue();

      // Only update messages if the message belongs to the current conversation
      if (currentConversation?.sid === message.conversation.sid) {
        const currentMessages = this.messagesSubject.getValue();

        // Filter out the removed message by SID
        const updatedMessages = currentMessages.filter(
          (msg) => msg.sid !== message.sid
        );

        // Update the messages subject with the filtered array
        this.messagesSubject.next(updatedMessages);
        await this.getUnreadMessagesForConversation(currentConversation.sid);
      }

      // Also update chat list if needed
      this.updateChatListAfterMessageRemoval(message);
      this.getPatientChat(this.currentPatientUser)
    } catch (error) {
      console.error('Error handling removed message:', error);
    }
  };
  private updateChatListAfterMessageRemoval(message: Message): void {
    const chatList = this.chatListSubject.getValue();

    // Find the chat in the list
    const conversation = chatList.find(
      (chatItem) => chatItem.chat.sid === message.conversation.sid
    );

    if (conversation) {
      // If the removed message was the last message, we need to update
      // Get the new last message
      message.conversation.getMessages(1).then((result) => {
        const updatedChatList = chatList.map((chatItem) => {
          if (chatItem.chat.sid === message.conversation.sid) {
            const lastMsg = result.items[0];
            return {
              ...chatItem,
              lastMessage: lastMsg ? lastMsg.body || '(Media message)' : ''
            };
          }
          return chatItem;
        });

        this.chatListSubject.next(updatedChatList);

      });
    }
  }
  private handleMessageUpdated = (event: { message: Message }): void => {
    console.log('Message Edited')
    try {
      const message = event.message;
      console.log('Message Updated:', message.sid);
      const currentConversation = this.currentConversationSubject.getValue();
      console.log('current conversation handleMessageUpdated')
      console.log(currentConversation)

      // Only update if the message belongs to the current conversation
     // if (currentConversation?.sid === message.conversation.sid) {
        this.fetchMessages().catch(error => {
          console.error('Error in background message fetching:', error);
        });
        const currentMessages = this.messagesSubject.getValue();
        console.log('current Updated Message');
        console.log(currentMessages)

        // Replace the updated message in the array
        const updatedMessages = currentMessages.map((msg) =>
          msg.sid === message.sid ? message : msg
        );
        this.messagesSubject.next(updatedMessages);
        this.getPatientChat(this.currentPatientUser)
     // }
    } catch (error) {
      console.error('Error handling updated message:', error);
    }
  };
  async getPatientChat(patientUsername: string): Promise<any> {
    try {
      // Make the API call to get chat data
      // Replace this with your actual API call
      // const url = `${apiUrl}?ToUser=${patientId}`;
      //     return await this.rpmService.rpm_get(url);
      const response = await this.rpm.rpm_get(`/api/comm/getallchats?ToUser=${patientUsername}`);
      this.messageListenerRegistered = false;

      // Update the BehaviorSubject with new data
      this.updateChatData(response);

      return response;
    } catch (error:any) {
      if(error.status == 404)
      {
        console.log(error.error.message)
      }else{
      console.error('❌ Error in PatientChatService.getPatientChat:', error);

      }
      throw error;
    }
  }


  updateChatData(newChatData: any): void {
    this.chatHistoryDataSubject.next(newChatData);
  }


  getCurrentChatData(): any {
    return this.chatHistoryDataSubject.getValue();
  }
}
