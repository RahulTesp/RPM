package com.rpm.clynx.service;

import androidx.lifecycle.LiveData;
import androidx.lifecycle.MutableLiveData;
import androidx.lifecycle.ViewModel;
import java.util.ArrayList;
import java.util.List;
import com.rpm.clynx.adapter.ChatListAdapter;
import com.rpm.clynx.model.Chat;
import android.content.Context;
public class ChatViewModel extends ViewModel {
    private MutableLiveData<List<Chat>> chatListLiveData = new MutableLiveData<>(new ArrayList<>());
    private ChatListAdapter chatListAdapter;

    public ChatListAdapter getChatListAdapter(Context context) {
        if (chatListAdapter == null) {
            chatListAdapter = new ChatListAdapter(chatListLiveData.getValue(), context, "");
        }
        return chatListAdapter;
    }

    public LiveData<List<Chat>> getChatListLiveData() {
        return chatListLiveData;
    }

    public void addChatMessage(Chat chat) {
        List<Chat> updatedList = chatListLiveData.getValue();
        updatedList.add(chat);
        chatListLiveData.setValue(updatedList);  // Notify UI
    }
}