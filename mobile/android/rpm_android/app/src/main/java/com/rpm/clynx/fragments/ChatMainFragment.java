package com.rpm.clynx.fragments;

import androidx.fragment.app.Fragment;
import androidx.lifecycle.ViewModelProvider;
import androidx.localbroadcastmanager.content.LocalBroadcastManager;
import androidx.recyclerview.widget.LinearLayoutManager;
import androidx.recyclerview.widget.RecyclerView;
import android.annotation.SuppressLint;
import android.content.BroadcastReceiver;
import android.content.Context;
import android.content.Intent;
import android.content.IntentFilter;
import android.content.SharedPreferences;
import android.os.Build;
import android.os.Bundle;
import android.os.Handler;
import android.os.Looper;
import android.util.Log;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.ImageButton;
import android.widget.ProgressBar;
import android.widget.TextView;
import android.widget.Toolbar;
import com.rpm.clynx.R;
import com.rpm.clynx.adapter.ChatListAdapter;
import com.rpm.clynx.model.Chat;
import com.rpm.clynx.service.ChatViewModel;
import com.rpm.clynx.service.TwilioChatManager;
import com.rpm.clynx.utility.DataBaseHelper;
import com.rpm.clynx.utility.NetworkAlert;
import com.rpm.clynx.utility.NetworkUtils;
import com.rpm.clynx.utility.SystemBarColor;
import com.rpm.clynx.utility.ConversationsClientManager;
import com.twilio.conversations.ConversationsClient;
import org.jetbrains.annotations.Nullable;
import java.util.ArrayList;
import java.util.List;

public class ChatMainFragment extends Fragment {
    private DataBaseHelper db;
    private SharedPreferences pref;
    private SharedPreferences.Editor editor;
    private String Token;
    private ProgressBar progressBar;
    private ImageButton chatAdd;
    private TextView chatC;
    private RecyclerView recyclerView;
    private ChatListAdapter chatListAdapter;
    private List<Chat> chatList = new ArrayList<>();
    private ChatViewModel chatViewModel;
    boolean isChatLoad ;
    String Patent_Id;
    private final Handler handler = new Handler();
    private static final int RETRY_DELAY = 1000; // Retry every 1 second
    QuickstartConversationsManager quickstartConversationsManager;
    public ChatMainFragment() {
        // Required empty public constructor
    }
    private BroadcastReceiver chatReceiver = new BroadcastReceiver() {
        @Override
        public void onReceive(Context context, Intent intent) {
            String senderName = intent.getStringExtra("sender_name");
            String messageText = intent.getStringExtra("message_text");
            Log.d("senderName", senderName);
            Log.d("messageText", messageText);
        }
    };

    @SuppressLint("MissingInflatedId")
    @Override
    public View onCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState) {
        View view = inflater.inflate(R.layout.fragment_chat_main, container, false);
        SystemBarColor.setSystemBarColor(requireActivity(), R.color.profile_card_body_bg);
        Log.d("ChatMainFragment", "Fragment Created");
        progressBar = view.findViewById(R.id.progressChatList);
        progressBar.setVisibility(View.VISIBLE);
        chatC = view.findViewById(R.id.itemMsgCount);
        chatAdd = view.findViewById(R.id.chatAddBtn);
        recyclerView = view.findViewById(R.id.fragmentChat);
        pref = requireContext().getSharedPreferences("RPMUserApp", requireContext().MODE_PRIVATE);
        editor = pref.edit();
        Token = pref.getString("Token", null);
        db = new DataBaseHelper(requireContext());
        Patent_Id = pref.getString("Patent_id", null); // Get logged-in user ID
        quickstartConversationsManager = new QuickstartConversationsManager(requireContext());
        recyclerView.setLayoutManager(new LinearLayoutManager(requireContext()));
        chatViewModel = new ViewModelProvider(this).get(ChatViewModel.class);
        chatListAdapter = chatViewModel.getChatListAdapter(requireContext());
        recyclerView.setAdapter(chatListAdapter);

        chatViewModel.getChatListLiveData().observe(getViewLifecycleOwner(), chatList -> {
            Log.d("getChatListLiveData", "getChatListLiveData updated");
            chatListAdapter.notifyDataSetChanged();
        });

        // Find the android.widget.Toolbar
        Toolbar toolbar = view.findViewById(R.id.toolbarChat);

        // Navigate back properly
        toolbar.setNavigationOnClickListener(v -> {
            requireActivity().getSupportFragmentManager().popBackStack();
        });

        TwilioChatManager.getInstance().setChatViewModel(chatViewModel);
        TwilioChatManager.getInstance().setupTwilioListeners();

        chatAdd.setOnClickListener(view1 -> {
            Intent intent = new Intent(requireContext(), ChatAddActivity.class);
            startActivity(intent);
        });

        return view;
    }


    @Override
    public void onResume() {
        super.onResume();
        Log.d("ONRESUMEchatListAdapter", String.valueOf(chatListAdapter));
        LocalBroadcastManager.getInstance(requireContext()).registerReceiver(chatReceiver, new IntentFilter("NEW_CHAT_MESSAGE"));
        ConversationsClient convClientVal = ConversationsClientManager.getInstance().getConvClient();
        Log.d("convClientValINCHATFRAGMENT", String.valueOf(convClientVal));
        if (convClientVal == null) {
           waitForClientInitialization();
        } else {
            checkAndLoadConversations(convClientVal);
        }
    }

    private void checkAndLoadConversations(ConversationsClient convClientVal) {
        boolean isSynced = ConversationsClientManager.getInstance().isClientSynced();
        Log.d("checkAndLoadConversations", "isSynced: " + isSynced);
           if (isSynced) {
               Log.d("isClientSynced",String.valueOf(isSynced));
            Log.d("checkAndLoadConversations", "Sync completed, loading conversations...");
            hideLoading();
            loadConversations();
        } else {
            Log.d("checkAndLoadConversations", "Sync not completed, retrying in 2 seconds...");
            new Handler(Looper.getMainLooper()).postDelayed(() -> checkAndLoadConversations(convClientVal), 2000);  // Retry after 2 seconds
        }
    }

    private void waitForClientInitialization() {
        handler.postDelayed(new Runnable() {
            @Override
            public void run() {
                ConversationsClient convClientVal = ConversationsClientManager.getInstance().getConvClient();
                Log.d("waitForClientInitialization", "Checking if client is ready...");
                if (convClientVal == null) {
                    Log.d("ClientCheck", "Client is null, fetching token...");
                    quickstartConversationsManager.retrieveAccessTokenFromServerNew(requireContext(), Token, new TokenResponseListener() {
                        @Override
                        public void receivedTokenResponse(boolean success, @Nullable Exception exception) {
                            Log.d("receivedTokenResponse","CHATMAINPAGE");
                            if (success) {
                                Log.d("TokenRetrieval", "Token retrieved successfully, checking sync status...");
                                handler.postDelayed(new Runnable() {  //  Fix: Use a new Runnable instance
                                    @Override
                                    public void run() {
                                        checkClientSyncStatus();
                                    }
                                }, RETRY_DELAY);
                            } else {
                                Log.e("TokenFetchError", "Failed to retrieve token");
                                if (exception != null) {
                                    Log.e("TokenFetchError", exception.getLocalizedMessage());
                                }
                                // Retry token retrieval after some delay
                                handler.postDelayed(new Runnable() {  //  Fix: Use a new Runnable instance
                                    @Override
                                    public void run() {
                                        waitForClientInitialization();
                                    }
                                }, RETRY_DELAY);
                            }
                        }
                    });
                } else {
                    checkClientSyncStatus(); // Start checking sync status if client is not null
                }
            }
        }, 0); // Start immediately
    }

    // Continuously check until the client is synced
    private void checkClientSyncStatus() {
        handler.postDelayed(new Runnable() {
            @Override
            public void run() {
                boolean isSynced = ConversationsClientManager.getInstance().isClientSynced();
                Log.d("checkAndLoadConversations", "isSynced: " + isSynced);
                if (isSynced) {
                    Log.d("ClientSyncCheck", "Client is synchronized, loading conversations...");
                    loadConversations();
                } else {
                    Log.d("ClientSyncCheck", "Client not synced yet, retrying...");
                    handler.postDelayed(this, RETRY_DELAY); // Retry every 1 second
                }
            }
        }, 0);
    }

    private void hideLoading() {
        if (progressBar != null) {
            progressBar.setVisibility(View.GONE);
        }
    }

    private void loadConversations() {
        Log.d("loadConversations", "loadConversations called");
        ConversationsClient convClientVal = ConversationsClientManager.getInstance().getConvClient();

        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.Q) {
            Context ctx = getContext();
            if (ctx != null && isAdded()) {
                quickstartConversationsManager.loadAllConversations(
                        convClientVal, ctx, recyclerView, chatListAdapter,
                        chatList, progressBar, isChatLoad, Patent_Id
                );
            } else {
                Log.w("loadConversations", "Fragment not attached, skipping load.");
            }
        }
    }
}
