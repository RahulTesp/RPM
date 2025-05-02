package com.rpm.clynx.adapter;

import android.content.Context;
import android.content.Intent;
import android.util.Log;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.TextView;
import androidx.annotation.NonNull;
import androidx.recyclerview.widget.RecyclerView;
import com.rpm.clynx.R;
import com.rpm.clynx.fragments.MessageActivity;
import com.rpm.clynx.model.Chat;
import java.util.List;

public class ChatListAdapter extends RecyclerView.Adapter<ChatListAdapter.ChatViewHolder> {

    private List<Chat> chatList;
    private Context context;
    private String conversationSid;

    public ChatListAdapter(List<Chat> chatList, Context context, String SID ) {
        this.chatList = chatList;
        this.context = context;
        this.conversationSid = SID;
    }

    @NonNull
    @Override
    public ChatViewHolder onCreateViewHolder(@NonNull ViewGroup parent, int viewType) {
        View view = LayoutInflater.from(parent.getContext()).inflate(R.layout.chat_item, parent, false);
        return new ChatViewHolder(view);
    }

    @Override
    public void onBindViewHolder(@NonNull ChatViewHolder holder, int position) {
        Chat chat = chatList.get(position);
        Log.d("ChatListAdapterTBEFOR", "Timestamp: " + chat.getTimestamp());
        Log.d("ChatListAdapterCBEFORE", "Unread Count: " + chat.getUnreadCount());
        holder.chatMemberName.setText(chat.getChatMemberName());
        if (chat.getUnreadCount() > 0) {
            holder.unreadCount.setText(String.valueOf(chat.getUnreadCount()));
            holder.unreadCount.setVisibility(View.VISIBLE);
        } else {
            holder.unreadCount.setVisibility(View.GONE);
        }
        Log.d("chat.isTyping()", "At position " + position + ": " + chat.isTyping());

        if (chat.isTyping()) {
            holder.itemTypingIndicator.setVisibility(View.VISIBLE);
        } else {
            holder.itemTypingIndicator.setVisibility(View.GONE);
        }

        Log.d("ChatListAdapter", "Timestamp: " + chat.getTimestamp());
        Log.d("ChatListAdapter", "Unread Count: " + chat.getUnreadCount());
        Log.d("ChatListAdapter", "lastMsgText: " + chat.getLastMsgText());

        if (chat.getTimestamp() != null && !chat.getTimestamp().isEmpty()) {
            holder.timestamp.setText(chat.getTimestamp());
        } else {
            holder.timestamp.setText("Just now");
        }

        if (chat.getLastMsgText() != null && !chat.getLastMsgText().isEmpty()) {
            holder.lastMsgText.setText(chat.getLastMsgText());
        } else {
            holder.lastMsgText.setText("Just now");
        }

        // Handle click event
        holder.itemView.setOnClickListener(v -> {
            chatList.get(position).setUnreadCount(0);  // Reset unread count
            Intent intent = new Intent(context, MessageActivity.class);
            intent.putExtra("memberUsername", chat.getChatMemberName());  // Pass memberUsername
            intent.putExtra("FriendlyUsername", chat.getFriendlyUsername());
            intent.putExtra("conversationSid", conversationSid); // Pass the SID
            context.startActivity(intent);
        });
    }

    public void updateChatList(List<Chat> newChatList) {
        Log.d("updateChatList", "New list size: " + (newChatList != null ? newChatList.size() : "null"));
        this.chatList.clear();
        if (newChatList != null) {
            this.chatList.addAll(newChatList);
        }
        notifyDataSetChanged();
    }


    @Override
    public int getItemCount() {
        return chatList.size();
    }

    public static class ChatViewHolder extends RecyclerView.ViewHolder {
        TextView chatMemberName, timestamp, unreadCount,itemTypingIndicator, lastMsgText ;

        public ChatViewHolder(@NonNull View itemView) {
            super(itemView);
            chatMemberName = itemView.findViewById(R.id.itemChatMemNm);
            timestamp = itemView.findViewById(R.id.itemMsgTime);
            unreadCount = itemView.findViewById(R.id.itemMsgCount);
            itemTypingIndicator = itemView.findViewById(R.id.itemTypingIndicator);
            lastMsgText = itemView.findViewById(R.id.itemLastMsg);
        }
    }
}