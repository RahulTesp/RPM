package com.rpm.clynx.utility;

import android.util.Log;
import com.twilio.conversations.ConversationsClient;

public class ConversationsClientManager {
    private static ConversationsClient conversationsClient;
    private static ConversationsClientManager instance;
    ConversationsClient cvc;
    String chatTok;
    private volatile boolean isSynced = false; // Store sync status globally
    private volatile boolean isConvSynced = false;
    public ConversationsClientManager() {
        // Private constructor to prevent instantiation
    }

    public static synchronized ConversationsClientManager getInstance() {
        if (instance == null) {
            instance = new ConversationsClientManager();
        }
        return instance;
    }

    public void setConvClient(ConversationsClient clc)
    {
        Log.d("clc ConversationsClientManager ", String.valueOf(clc));
        this.cvc = clc;
        Log.d("cvc ConversationsClientManager ", String.valueOf(cvc));

    }
    public void setChatToken(String Token)
    {
        this.chatTok = Token;
        Log.d("setchatTok", chatTok);

    }
    public String getChatToken()
    {
        Log.d("getchatTok", chatTok);
        return this.chatTok;
    }
    public ConversationsClient getConvClient()
    {
        Log.d("cvc new return ", String.valueOf(this.cvc));
        return this.cvc;
    }

    public void clearConversationsClient() {
        Log.d("ConversationsClientManager", "Clearing ConversationsClient instance");
        this.cvc = null;
        this.chatTok = null;
        conversationsClient = null;
    }
    // Add getter and setter for isSynced
    public boolean isClientSynced() {
        Log.d("isClientSynced", "Returning: " + isSynced);
        return isSynced;
    }

    public void setClientSynced(boolean value) {
        Log.d("setClientSynced", "Setting isSynced to: " + value);
        this.isSynced = value;
    }

    public void setConvSynced(boolean value) {
        Log.d("setConvSynced", "Setting setConvSynced to: " + value);
        this.isConvSynced = value;
    }

    public boolean isConvSynced() {
        Log.d("isConvSynced", "Returning: " + isConvSynced);
        return isConvSynced;
    }
}
