package com.rpm.clynx.service;

import android.util.Log;

import com.rpm.clynx.adapter.ChatListAdapter;

import java.text.SimpleDateFormat;

import java.util.ArrayList;

import java.util.Date;

import java.util.HashSet;

import java.util.List;

import java.util.Locale;

import java.util.Set;

import com.rpm.clynx.model.Chat;

import com.rpm.clynx.utility.ConversationsClientManager;

import com.twilio.conversations.Conversation;

import com.twilio.conversations.ConversationListener;

import com.twilio.conversations.ConversationsClient;

import com.twilio.conversations.Message;

import com.twilio.conversations.Participant;

public class TwilioChatManager {

    private static TwilioChatManager instance;

    private static List<Chat> chatList = new ArrayList<>();

    private ChatListAdapter chatListAdapter;

    private ConversationsClient conversationsClient;

    private final Set<Conversation> registeredConversations = new HashSet<>();

    private ChatViewModel chatViewModel;

    private TwilioChatManager() {

        conversationsClient = ConversationsClientManager.getInstance().getConvClient();

    }

    public static TwilioChatManager getInstance() {

        if (instance == null) {

            instance = new TwilioChatManager();

        }

        return instance;

    }

    // Set ViewModel from Activity

    public void setChatViewModel(ChatViewModel viewModel) {

        this.chatViewModel = viewModel;

    }

    public void setupTwilioListeners() {

        Log.d("TwilioChatManager", "Setting up Twilio listeners...");

        if (conversationsClient != null) {

            conversationsClient.getMyConversations().forEach(conversation -> {

                Log.d("sync STATUS", "Checking conversation sync status: " + conversation.getSid());

                if (conversation.getSynchronizationStatus() == Conversation.SynchronizationStatus.ALL) {

                    addConversationListener(conversation);

                } else {

                    Log.d("NOTsynchronized", "Conversation not fully synchronized: " + conversation.getSid());

                    // Register a listener to wait for synchronization

                    conversation.addListener(new ConversationListener() {

                        @Override

                        public void onSynchronizationChanged(Conversation updatedConversation) {

                            if (updatedConversation.getSynchronizationStatus() == Conversation.SynchronizationStatus.ALL) {

                                Log.d("TwilioChatManager", "Conversation is now synchronized: " + updatedConversation.getSid());

                                addConversationListener(updatedConversation);

                            }

                        }

                        @Override

                        public void onMessageAdded(Message message) {}

                        @Override

                        public void onMessageUpdated(Message message, Message.UpdateReason reason) {}

                        @Override

                        public void onMessageDeleted(Message message) {}

                        @Override

                        public void onParticipantAdded(Participant participant) {}

                        @Override

                        public void onParticipantUpdated(Participant participant, Participant.UpdateReason reason) {}

                        @Override

                        public void onParticipantDeleted(Participant participant) {}

                        @Override

                        public void onTypingStarted(Conversation conversation, Participant participant) {}

                        @Override

                        public void onTypingEnded(Conversation conversation, Participant participant) {}

                    });

                }

            });

        }

    }

    // Helper method to add the actual listener once synchronized

    private void addConversationListener(Conversation conversation) {

        if (!registeredConversations.contains(conversation)) {

            conversation.addListener(new ConversationListener() {

                @Override

                public void onMessageAdded(Message message) {

                    Log.d("TwilioChatManager", "New message received: " + message.getBody());

                    if (chatViewModel != null) {

                        chatViewModel.addChatMessage(new Chat("","Sender", message.getBody(), 0, 0, false,"","", conversation));

                    } else {

                        Log.e("TwilioChatManager", "ChatViewModel is null. Cannot update UI.");

                    }

                }

                @Override

                public void onMessageUpdated(Message message, Message.UpdateReason reason) {

                    Log.d("TwilioChatManager", "Message updated: " + message.getBody());

                }

                @Override

                public void onMessageDeleted(Message message) {}

                @Override

                public void onParticipantAdded(Participant participant) {}

                @Override

                public void onParticipantUpdated(Participant participant, Participant.UpdateReason reason) {}

                @Override

                public void onParticipantDeleted(Participant participant) {}

                @Override

                public void onTypingStarted(Conversation conversation, Participant participant) {}

                @Override

                public void onTypingEnded(Conversation conversation, Participant participant) {}

                @Override

                public void onSynchronizationChanged(Conversation conversation) {}

            });

            registeredConversations.add(conversation);

        }

    }

    public void setChatListAdapter(ChatListAdapter adapter) {

        this.chatListAdapter = adapter;

    }

    public ChatListAdapter getChatListAdapter() {

        return chatListAdapter;

    }

    public static List<Chat> getChatList() {

        return chatList;

    }


    public void setChatList(List<Chat> newList) {

        chatList = newList;

    }

    public static String formatTimestamp(long timestampMillis) {

        SimpleDateFormat sdf = new SimpleDateFormat("MM/dd/yyyy HH:mm", Locale.getDefault());

        return sdf.format(new Date(timestampMillis));

    }

    public void clearChatList() {

        if (chatList != null) {

            chatList.clear();

        }

        if (chatListAdapter != null) {

            chatListAdapter.notifyDataSetChanged();

        }

    }

}

