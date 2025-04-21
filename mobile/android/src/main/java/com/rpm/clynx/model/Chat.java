package com.rpm.clynx.model;

import android.util.Log;

public class Chat {
    private String chatMemberName, sid;
    private String timestamp, lastMsgText, friendlyUserName; // Formatted timestamp for display
    private int unreadCount;
    private long timestampMillis; // Store raw timestamp for sorting
    private boolean isTyping;

    public Chat(String friendlyUserName, String chatMemberName, String timestamp, int unreadCount, long timestampMillis, boolean isTyping, String lastMsgText, String sid) {
        this.chatMemberName = chatMemberName;
        this.timestamp = timestamp;
        this.unreadCount = unreadCount;
        this.timestampMillis = timestampMillis;
        this.isTyping = isTyping;
        this.lastMsgText = lastMsgText;
        this.sid = sid;
        this.friendlyUserName = friendlyUserName;
        Log.d("Chat List Values",chatMemberName+timestamp+unreadCount+timestampMillis);
    }
    public String getChatMemberName() { return chatMemberName; }
    public String getTimestamp() { return timestamp; }
    public int getUnreadCount() { return unreadCount; }
    public long getTimestampMillis() { return timestampMillis; }
    public String getLastMsgText() { return lastMsgText; }
    public void setLastMsgText(String lastMsgText) {
        this.lastMsgText = lastMsgText;
        if (lastMsgText != null) {
            Log.d("setLastMsgText:-", lastMsgText);
        } else {
            Log.d("setLastMsgText:-", "No text available");
        }
    }

    public void setUnreadCount(int unreadCount) { this.unreadCount = unreadCount; }
    public void setTimeStamp(String timestamp) { this.timestamp = timestamp; }
    public void setSID(String sid) { this.sid = sid; }
    public String getSID() { return sid; }
    public void setTimeStampMillis(long timestampMillis) { this.timestampMillis = timestampMillis; }
    public void setFriendlyUsername(String friendlyUserName) { this.friendlyUserName = friendlyUserName; }
    public String getFriendlyUsername() { return friendlyUserName; }
    public boolean isTyping() {
        Log.d("isTyping","isTyping");
        return isTyping;
    }
    public void setTyping(boolean typing) {
        Log.d("setTyping", String.valueOf(typing));
        isTyping = typing;
    }
}
