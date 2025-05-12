package com.rpm.clynx.service;

import android.util.Log;
import androidx.lifecycle.LiveData;
import androidx.lifecycle.MutableLiveData;
import androidx.lifecycle.ViewModel;
import java.util.HashMap;
import java.util.Map;

public class UnreadMessageViewModel extends ViewModel {
    private final MutableLiveData<Map<String, Integer>> unreadCounts = new MutableLiveData<>(new HashMap<>());

    public LiveData<Map<String, Integer>> getUnreadCounts() {
        return unreadCounts;
    }

    public void updateUnreadCount(String conversationSid, int count) {
        Log.d("updateUnreadCount","updateUnreadCount");
        Map<String, Integer> currentCounts = unreadCounts.getValue();
        if (currentCounts != null) {
            currentCounts.put(conversationSid, count);
            unreadCounts.setValue(currentCounts);
        }
    }

    public int getTotalUnread() {
        return unreadCounts.getValue().values().stream().mapToInt(Integer::intValue).sum();
    }
}
