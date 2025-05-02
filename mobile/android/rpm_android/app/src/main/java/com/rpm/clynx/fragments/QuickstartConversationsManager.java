package com.rpm.clynx.fragments;
import static com.rpm.clynx.utility.Links.BASE_URL;
import static com.rpm.clynx.utility.Links.CHAT_UPDATE;
import static com.rpm.clynx.utility.Links.NotifyConversation_URL;
import static com.rpm.clynx.utility.Links.chatreg_token_url;
import android.app.Activity;
import android.content.Context;
import android.content.SharedPreferences;
import android.os.Build;
import android.os.Handler;
import android.os.Looper;
import android.util.Log;
import android.view.View;
import android.widget.ProgressBar;
import android.widget.Toast;
import androidx.annotation.RequiresApi;
import androidx.appcompat.app.AppCompatActivity;
import androidx.fragment.app.FragmentActivity;
import androidx.lifecycle.ViewModelProvider;
import androidx.lifecycle.ViewModelStoreOwner;
import androidx.recyclerview.widget.RecyclerView;
import com.android.volley.AuthFailureError;
import com.android.volley.DefaultRetryPolicy;
import com.android.volley.NetworkResponse;
import com.android.volley.Request;
import com.android.volley.RequestQueue;
import com.android.volley.Response;
import com.android.volley.VolleyError;
import com.android.volley.toolbox.HttpHeaderParser;
import com.android.volley.toolbox.JsonObjectRequest;
import com.android.volley.toolbox.StringRequest;
import com.android.volley.toolbox.Volley;
import com.google.firebase.messaging.FirebaseMessaging;
import com.rpm.clynx.adapter.ChatListAdapter;
import com.rpm.clynx.auth.StatusCodeRequest;
import com.rpm.clynx.model.Chat;
import com.rpm.clynx.service.TimeFormatter;
import com.rpm.clynx.service.UnreadMessageViewModel;
import com.rpm.clynx.utility.ConversationsClientManager;
import com.rpm.clynx.utility.DashboardViewModel;
import com.rpm.clynx.utility.FileLogger;
import com.rpm.clynx.utility.Links;
import com.rpm.clynx.utility.MyApplication;
import com.twilio.conversations.CallbackListener;
import com.twilio.conversations.Conversation;
import com.twilio.conversations.ConversationListener;
import com.twilio.conversations.ConversationsClient;
import com.twilio.conversations.ConversationsClientListener;
import com.twilio.util.ErrorInfo;
import com.twilio.conversations.Message;
import com.twilio.conversations.Participant;
import com.twilio.conversations.StatusListener;
import com.twilio.conversations.User;
import org.jetbrains.annotations.Nullable;
import org.json.JSONArray;
import org.json.JSONException;
import org.json.JSONObject;

import java.io.File;
import java.io.IOException;
import java.time.ZonedDateTime;
import java.time.format.DateTimeFormatter;
import java.util.ArrayList;
import java.util.Collections;
import java.util.Date;
import java.util.HashMap;
import java.util.List;
import java.util.Map;

interface QuickstartConversationsManagerListener {
    void receivedNewMessage(boolean equals);
    void messageSentCallback();
    void reloadMessages();
}

interface TokenResponseListener {
    void receivedTokenResponse(boolean success, @Nullable Exception exception);
}

interface AccessTokenListener {
    void receivedAccessToken(@Nullable String token, @Nullable Exception exception);
}

class QuickstartConversationsManager extends  AppCompatActivity {
    // This is the unique name of the conversation  we are using
    private static String CONVERSATION_SID = "";
    private static String Patent_Id;
    private String savedUserName, lastMessageText;
    final private ArrayList<Message> messages = new ArrayList<>();
    private ConversationsClient conversationsClient;
    public Conversation conversation;
    private QuickstartConversationsManagerListener conversationsManagerListener;
    private String tokenURLnew = "";
    private String tokenURL = "";
    String chataccessToken;
    ProgressBar progressBarChat;
    ChatListAdapter chatListAdapter;
    MessageActivity.MessagesAdapter messagesAdapter;
    Context currentContext;
    RecyclerView chatRs ;
    List<Chat> chatList;
    boolean isChatLoaded;

    private class TokenResponse {
        String token;
    }
    Context contxt;
    Context contxtreg;
    String appAccessToken;
    SharedPreferences pref;
    SharedPreferences.Editor editor;
    Context contextValue;
    long unreadMessageCount;
    private TokenResponseListener tokenResponseListener;  // Store the listener
    private DashboardViewModel dashboardViewModelIn; // Reference to the ViewModel
    private volatile boolean isSynced = false;

    // Track synchronization status
    public QuickstartConversationsManager(Context context) {
        contextValue = context;
    }
    // Constructor to initialize the ViewModel reference
    public QuickstartConversationsManager(DashboardViewModel viewModel) {
        this.dashboardViewModelIn = viewModel;
        System.out.println("dashboardViewModel");
        System.out.println(dashboardViewModelIn);
    }

    void retrieveAccessTokenFromServer(final Context context,  String tokn ,
                                       final TokenResponseListener listener) {
        contxt = context;
        appAccessToken = tokn;

        // Set the chat token URL in your strings.xml file
        String chatTokenURL = Links.BASE_URL + Links.chat_token_url;
        tokenURL = chatTokenURL ;

        new Thread(new Runnable() {
            @Override
            public void run() {
                retrieveToken(appAccessToken , contxt , new AccessTokenListener() {
                    @Override
                    public void receivedAccessToken(@Nullable String tkn,
                                                    @Nullable Exception exception) {
                        if (tkn != null) {
                            Log.d("receivedAccessToken", tkn);
                            Log.d("contxt", contxt.toString());
                            Log.d("QuickstartClient", String.valueOf(QuickstartConversationsManager.this.conversationsClient));

// Set the desired command timeout value in seconds
                            int commandTimeoutInSeconds = 10000;

// Create the properties builder
                            ConversationsClient.Properties.Builder builder = ConversationsClient.Properties.newBuilder();

// Set the command timeout using the setCommandTimeout method
                            builder.setCommandTimeout(commandTimeoutInSeconds);

// Build the properties object
                            ConversationsClient.Properties props = builder.createProperties();

// Now you can create the ConversationsClient instance with the updated properties
                            ConversationsClient.create(contxt, tkn, props, mConversationsClientCallback);

                            Log.d("propstimeout", String.valueOf(props.getCommandTimeout()));
                            Log.d("props", props.toString());
                            Log.d("mConversationsClientCallback", mConversationsClientCallback.toString());

                            listener.receivedTokenResponse(true,null);
                        } else {
                            listener.receivedTokenResponse(false, exception);
                        }
                    }
                });
            }
        }).start();
    }

    void retrieveAccessTokenFromServerNew(final Context context, String tokn ,
                                          final TokenResponseListener listener) {
        contxt = context;
        appAccessToken = tokn;
        String chatRegTokenURL = Links.BASE_URL + Links.chatreg_token_url;
        tokenResponseListener = listener;  //  Store the listener

        tokenURLnew = chatRegTokenURL ;

        new Thread(new Runnable() {
            @Override
            public void run() {
                retrieveTokenNEW(appAccessToken , contxt , new AccessTokenListener() {
                    @Override
                    public void receivedAccessToken(@Nullable String tkn,
                                                    @Nullable Exception exception) {
                        if (tkn != null) {
                            Log.d("receivedAccessTokenNew", tkn);
                            Log.d("contxtNew", contxt.toString());
                            Log.d("QuickstartClientNew", String.valueOf(QuickstartConversationsManager.this.conversationsClient));

                            ConversationsClient.Properties props = ConversationsClient.Properties.newBuilder()

                                    .createProperties();

                            ConversationsClient.create(contxt, tkn, props, mConversationsClientCallback);

                            Log.d("props", props.toString());
                            Log.d("mConversationsClientCallback", mConversationsClientCallback.toString());
                            progressBarChat.setVisibility(View.GONE);
                            listener.receivedTokenResponse(true,null);
                        } else {
                            if (tokenResponseListener != null) {
                                tokenResponseListener.receivedTokenResponse(false, exception);
                            }
                        }
                    }
                });
            }
        }).start();
    }

    public void regenerateToken(String tok ,  Context regcntxt , AccessTokenListener listener) {
        Log.d("Contextcntxt", "Contextcntxt");
        contxtreg = regcntxt;
        Log.d("contxtreg", String.valueOf(contxtreg));
        Log.d("cntxt", String.valueOf(regcntxt));
        Log.d("tokregenerateToken", tok);
        String REG_TOK_URL = Links.BASE_URL + chatreg_token_url;

        JsonObjectRequest jsonObjectRequest = new JsonObjectRequest(Request.Method.GET, tokenURL,
                null,  // No parameters
                new Response.Listener<JSONObject>() {
                    @Override
                    public void onResponse(JSONObject response) {
                        Log.d("REG_TOK_URL", REG_TOK_URL);
                        try {
                            String getchattoken = response.getString("message");
                            Log.d("REG tokenURL", getchattoken.toString());

                            if (getchattoken != null ) {
                                String stringWithoutQuotes = getchattoken.replace("\"", "");
                                System.out.println("CONNECT stringWithoutQuotes");
                                System.out.println(stringWithoutQuotes);
                                String responseBody = stringWithoutQuotes;
                                TokenResponse tokenResponse = new TokenResponse();
                                tokenResponse.token =  responseBody;
                                Log.d("Response11regenerateToken", "Response from server: " + responseBody);
                                String accessToken =  tokenResponse.token;
                                Log.d("Response22regenerateToken", "Retrieved access token from server: " + accessToken);
                                listener.receivedAccessToken(accessToken, null);
                            }
                        } catch (Exception e) {
                            listener.receivedAccessToken(null, e);
                            //  Toast.makeText(ResetPassword.this, "Invalid", Toast.LENGTH_SHORT).show();
                            e.printStackTrace();
                        }
                    }
                },
                new com.android.volley.Response.ErrorListener() {
                    @Override
                    public void onErrorResponse(VolleyError error) {
                        Log.d("error","error");
                    }

                }) {
            @Override
            public String getBodyContentType() {
                return "application/json";
            }

            @Override
            public Map<String, String> getHeaders() throws AuthFailureError {
                Map<String, String> headers = new HashMap<>();
                headers.put("Bearer", tok);
                Log.d("tokn", headers.toString());
                Log.d("tokn", tok);
                return headers;
            }
        };

        // Initialize the request queue and add the jsonObjectRequest to it
        RequestQueue requestQueue = Volley.newRequestQueue(contxtreg);
// Set the retry policy
        jsonObjectRequest.setRetryPolicy(new DefaultRetryPolicy(0, -1, DefaultRetryPolicy.DEFAULT_BACKOFF_MULT));
// Add the request to the queue
        requestQueue.add(jsonObjectRequest);
    }


    private void retrieveTokenNEW(String tok , final Context cntxt , AccessTokenListener listener) {

        Log.d("retrieveToken tok", tok);
// Create a new JsonObjectRequest to handle JSON responses
        JsonObjectRequest jsonObjectRequest = new JsonObjectRequest(Request.Method.GET, tokenURLnew,
                null,  // No parameters for the GET request
                new Response.Listener<JSONObject>() {
                    @Override
                    public void onResponse(JSONObject response) {
                        Log.d("tokenURL", tokenURL);
                        try {
                            String regenchattoken = response.getString("message");
                            Log.d("retrieveToken", regenchattoken.toString());

                            if (regenchattoken != null ) {
                                String stringWithoutQuotes = regenchattoken.replace("\"", "");
                                System.out.println("retrieveToken stringWithoutQuotes");
                                System.out.println(stringWithoutQuotes);
                                String responseBody = stringWithoutQuotes;
                                TokenResponse tokenResponse = new TokenResponse();
                                tokenResponse.token =  responseBody;
                                Log.d("ResponseretrieveToken", "Response from server: " + responseBody);
                                chataccessToken =  tokenResponse.token;
                                ConversationsClientManager.getInstance().setChatToken(chataccessToken);
                                Log.d("Response22retrieveToken", "Retrieved access token from server: " + chataccessToken);
                                listener.receivedAccessToken(chataccessToken, null);
                            }
                        } catch (Exception e) {
                            e.printStackTrace();
                        }
                    }
                },
                new com.android.volley.Response.ErrorListener() {
                    @Override
                    public void onErrorResponse(VolleyError error) {
                        Log.d("error",error.toString());
                        listener.receivedAccessToken(null, error);
                    }

                }) {
            @Override
            public String getBodyContentType() {
                return "application/json";
            }

            @Override
            public Map<String, String> getHeaders() throws AuthFailureError {
                Map<String, String> headers = new HashMap<>();
                headers.put("Bearer", tok);
                Log.d("tokn", headers.toString());
                Log.d("tokn", tok);
                return headers;
            }
        };

        // Initialize the request queue and add the jsonObjectRequest to it
        RequestQueue requestQueue = Volley.newRequestQueue(cntxt);
// Set the retry policy
        jsonObjectRequest.setRetryPolicy(new DefaultRetryPolicy(0, -1, DefaultRetryPolicy.DEFAULT_BACKOFF_MULT));
// Add the request to the queue
        requestQueue.add(jsonObjectRequest);
    }

    private void retrieveToken(String tok , final Context cntxt , AccessTokenListener listener) {
        Log.d("retrieveTokentok", tok);
        Log.d("tokenURLQM1", tokenURL);
        JsonObjectRequest jsonObjectRequest = new JsonObjectRequest(Request.Method.GET, tokenURL,
                null,  // No parameters
                new Response.Listener<JSONObject>() {
                    @Override
                    public void onResponse(JSONObject response) {
                        Log.d("tokenURLQM2", tokenURL);
                        try {
                            String getchattoken = response.getString("message");
                            Log.d("retrieveTokenQM", getchattoken);
                            if (getchattoken != null ) {
                                String stringWithoutQuotes = getchattoken.replace("\"", "");
                                System.out.println("retrieveToken stringWithoutQuotes");
                                System.out.println(stringWithoutQuotes);
                                String responseBody = stringWithoutQuotes;
                                TokenResponse tokenResponse = new TokenResponse();
                                tokenResponse.token =  responseBody;
                                Log.d("ResponseretrieveToken", "Response from server: " + responseBody);
                                chataccessToken =  tokenResponse.token;
                                ConversationsClientManager.getInstance().setChatToken(chataccessToken);
                                Log.d("Response22retrieveToken", "Retrieved access token from server: " + chataccessToken);
                                listener.receivedAccessToken(chataccessToken, null);
                            }
                        } catch (Exception e) {
                            e.printStackTrace();
                        }
                    }
                },
                new com.android.volley.Response.ErrorListener() {
                    @Override
                    public void onErrorResponse(VolleyError error) {
                        Log.d("errorGETCHATTOKEN",error.toString());
                    }

                }) {
            @Override
            public String getBodyContentType() {
                return "application/json";
            }

            @Override
            public Map<String, String> getHeaders() throws AuthFailureError {
                Map<String, String> headers = new HashMap<>();
                headers.put("Bearer", tok);
                Log.d("tokn", headers.toString());
                Log.d("toknZ", tok);
                return headers;
            }
        };

        // Initialize the request queue and add the jsonObjectRequest to it
        RequestQueue requestQueue = Volley.newRequestQueue(cntxt);
// Set the retry policy
        jsonObjectRequest.setRetryPolicy(new DefaultRetryPolicy(0, -1, DefaultRetryPolicy.DEFAULT_BACKOFF_MULT));
// Add the request to the queue
        requestQueue.add(jsonObjectRequest);
    }



    void sendMessage(String messageBody, Context context, String conversationSid,
                     String token, String userId, String toUser,
                     MessageActivity.MessagesAdapter messagesAdapter) {

        Log.d("sendMessage1", messageBody);
        FileLogger.d("sendMessage1", messageBody);

        if (conversation != null) {
            Log.d("messageBodysend", messageBody);
            FileLogger.d("messageBodysend", messageBody);

            conversation.prepareMessage()
                    .setBody(messageBody)
                    .build()
                    .send(new CallbackListener<Message>() {
                        @Override
                        public void onSuccess(Message message) {
                            FileLogger.d("sendMessage onSuccess", message.toString());
                            Log.d("sendMessage onSuccess", message.toString());

                            updateUnreadMessageCount(context);

                            if (conversationsManagerListener != null) {
                                conversationsManagerListener.messageSentCallback();
                            }

                            String utcTime = null;
                            if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.O) {
                                utcTime = getCurrentLocalTimeWithOffset();
                            }

                            sendUserNotifyConversation(conversationSid, userId, utcTime, token, context, toUser, messageBody);
                        }
                    });
        } else {
            Log.e("sendMessage", "Conversation is null. Cannot send message.");
        }
    }

    @RequiresApi(api = Build.VERSION_CODES.O)
    public String getCurrentLocalTimeWithOffset() {
        ZonedDateTime now = ZonedDateTime.now(); // Gets the current local time with zone
        DateTimeFormatter formatter = DateTimeFormatter.ofPattern("M/d/yyyy, h:mm:ss a XXX");
        return now.format(formatter);
    }

    private void sendUserNotifyConversation(String conversationSid, String userName, String lastActiveAt, String token, Context context,String toUser,String messageBody) {
        String API_URL = BASE_URL + NotifyConversation_URL;  // Replace with your actual endpoint
        JSONObject jsonBody = new JSONObject();
        try {
            jsonBody.put("ConversationSid", conversationSid);
            jsonBody.put("ToUser", toUser);
            jsonBody.put("FromUser", userName);
            jsonBody.put("Message", messageBody);

            Log.d("jsonBodyNotifyConversation",  jsonBody.toString());
            FileLogger.d("jsonBodyNotifyConversation",  jsonBody.toString());
        } catch (JSONException e) {
            e.printStackTrace();
            return;
        }

        StringRequest stringRequest = new StringRequest(
                Request.Method.POST,
                API_URL,
                response -> {
                    Log.d("NotifyConversationStatus", "Response received (may be empty): " + response);
                    FileLogger.d("NotifyConversationStatus", "Response received (may be empty): " + response);
                },
                error -> {
                    Log.e("NotifyConversationStatus", "Error: " + error.toString());
                    if (error.networkResponse != null) {
                        Log.e("NotifyConversationStatus", "Status code: " + error.networkResponse.statusCode);
                        String responseData = new String(error.networkResponse.data != null ? error.networkResponse.data : new byte[0]);
                        Log.e("NotifyConversationStatus", "Error body: " + responseData);
                        FileLogger.d("NotifyConversationStatus", "Error body: " + responseData);
                    }
                }
        ) {
            @Override
            public byte[] getBody() throws AuthFailureError {
                return jsonBody.toString().getBytes();
            }

            @Override
            public String getBodyContentType() {
                return "application/json";
            }

            @Override
            public Map<String, String> getHeaders() throws AuthFailureError {
                Map<String, String> headers = new HashMap<>();
                headers.put("Bearer", token);  // Your custom header
                return headers;
            }

            @Override
            protected Response<String> parseNetworkResponse(NetworkResponse response) {
                // Even if response is empty, treat it as success
                String responseString = "";
                if (response != null && response.data != null) {
                    responseString = new String(response.data);
                }
                Log.d("NotifyConversationStatus", "Raw status code: " + response.statusCode);
                FileLogger.d("NotifyConversationStatus", "Raw status code: " + response.statusCode);
                return Response.success(responseString, HttpHeaderParser.parseCacheHeaders(response));
            }
        };


//        StringRequest stringRequest = new StringRequest(
//                Request.Method.POST,
//                API_URL,
//                response -> {
//                    Log.d("NotifyConversationStatus", "Response: " + response); // even if empty
//                    FileLogger.d("NotifyConversationStatus", "Response: " + response);
//                },
//                error -> {
//                    Log.e("NotifyConversationStatus", "Error: " + error.toString());
//                }
//        ) {
//            @Override
//            public byte[] getBody() throws AuthFailureError {
//                return jsonBody.toString().getBytes();
//            }
//
//            @Override
//            public String getBodyContentType() {
//                return "application/json";
//            }
//
//            @Override
//            public Map<String, String> getHeaders() throws AuthFailureError {
//                Map<String, String> headers = new HashMap<>();
//                headers.put("Bearer", token);
//                return headers;
//            }
//        };

        // Creating a request queue and adding the request to the queue
        RequestQueue requestQueue = Volley.newRequestQueue(context);
        stringRequest.setRetryPolicy(new DefaultRetryPolicy(0, -1, DefaultRetryPolicy.DEFAULT_BACKOFF_MULT));
        requestQueue.add(stringRequest);
    }

    private void updateUnreadMessageCount(Context context) {
        Long lastMessageIndex = conversation.getLastMessageIndex();
        if (lastMessageIndex == null) {
            Log.e("Error", "Last message index is null. Cannot advance read message index.");
            return; // Exit early to prevent a crash
        }

        conversation.advanceLastReadMessageIndex(lastMessageIndex, new CallbackListener<Long>() {
            @Override
            public void onSuccess(Long unreadMsgNumber) {
                Log.d("Unread Messages", "Unread count reset to 0 after sending a message.");

                // Update ViewModel if needed
                UnreadMessageViewModel unreadViewModel = new ViewModelProvider((FragmentActivity) context)
                        .get(UnreadMessageViewModel.class);
                unreadViewModel.updateUnreadCount(CONVERSATION_SID, 0);
            }

            @Override
            public void onError(ErrorInfo errorInfo) {
                Log.e("Error", "Failed to update unread messages: " + errorInfo.getMessage());
            }
        });
    }

    public void setLastReadIndex(String conversationSid, Context context) {
        ConversationsClient convClient = ConversationsClientManager.getInstance().getConvClient();

        if (convClient != null) {
            convClient.getConversation(conversationSid, new CallbackListener<Conversation>() {
                @Override
                public void onSuccess(Conversation conversation) {
                    if (conversation != null) {
                        new Thread(() -> {
                            while (conversation.getSynchronizationStatus() != Conversation.SynchronizationStatus.ALL) {
                                try {
                                    Thread.sleep(1000);
                                } catch (InterruptedException e) {
                                    e.printStackTrace();
                                }
                            }

                            Long lastIndex = conversation.getLastMessageIndex();
                            if (lastIndex != null) {
                                conversation.advanceLastReadMessageIndex(lastIndex, new CallbackListener<Long>() {
                                    @Override
                                    public void onSuccess(Long unreadMsgCount) {
                                        Log.d("SetLastReadIndex", "Unread count after update: " + unreadMsgCount);
                                        // Optionally update view model or notify UI
                                    }

                                    @Override
                                    public void onError(ErrorInfo errorInfo) {
                                        Log.e("SetLastReadIndex", "Error updating last read index: " + errorInfo.getMessage());
                                    }
                                });
                            }
                        }).start();
                    }
                }

                @Override
                public void onError(ErrorInfo errorInfo) {
                    Log.e("SetLastReadIndex", "Failed to fetch conversation: " + errorInfo.getMessage());
                }
            });
        }
    }


    void getConv(MessageActivity.MessagesAdapter messagesAdapter, String CONVERSATION_SID, String TO_USER, Context conteXt, String convToken, ProgressBar progressBarMsg) {
        this.messagesAdapter = messagesAdapter;
        this.currentContext = conteXt;

        ConversationsClient convClient = ConversationsClientManager.getInstance().getConvClient();
        Log.d("convClient", String.valueOf(convClient));

        if (convClient != null) {
            convClient.getConversation(CONVERSATION_SID, new CallbackListener<Conversation>() {
                @Override
                public void onSuccess(Conversation conversation) {
                    Log.d("Conversation Retrieved", String.valueOf(conversation));

                    if (conversation != null) {
                        Log.d("Conversation Not Null", "Proceeding...");
                        // Poll synchronization status until fully synchronized
                        new Thread(() -> {
                            while (conversation.getSynchronizationStatus() != Conversation.SynchronizationStatus.ALL) {
                                try {
                                    Thread.sleep(2000); // Retry every 2 seconds
                                } catch (InterruptedException e) {
                                    e.printStackTrace();
                                }
                            }});

                        // Once synchronized, proceed with your logic
                        Log.d("Conversation Synchronized", "Proceeding with message retrieval");


                        // Fetch total messages count
                        conversation.getMessagesCount(new CallbackListener<Long>() {
                            @Override
                            public void onSuccess(Long totalMsgCount) {
                                Log.d("Total Messages", String.valueOf(totalMsgCount));
                            }

                            @Override
                            public void onError(ErrorInfo errorInfo) {
                                Log.e("Error", "Fetching total messages failed: " + errorInfo.getMessage());
                            }
                        });

                        Long lastMsgIndex = conversation.getLastMessageIndex();
                        if (lastMsgIndex != null) {
                            Log.d("Last Message Index", String.valueOf(lastMsgIndex));
                            if (conversation.getSynchronizationStatus() == Conversation.SynchronizationStatus.ALL) {
                                Log.d("conversationSynchronized", String.valueOf(lastMsgIndex));
                                conversation.advanceLastReadMessageIndex(lastMsgIndex, new CallbackListener<Long>() {
                                    @Override
                                    public void onSuccess(Long unreadMsgNumber) {
                                        Log.d("Unread Messages Cleared", "Unread count set to: " + unreadMsgNumber);

                                        UnreadMessageViewModel unreadViewModel = new ViewModelProvider((FragmentActivity) conteXt)
                                                .get(UnreadMessageViewModel.class);
                                        unreadViewModel.updateUnreadCount(CONVERSATION_SID, 0);

                                    }

                                    @Override
                                    public void onError(ErrorInfo errorInfo) {
                                        Log.e("Error", "Failed to update unread messages: " + errorInfo.getMessage());
                                    }
                                });
                            }
                        } else {
                            Log.e("Error", "No last message index found.");
                        }

                        // Check conversation status and act accordingly
                        if (conversation.getStatus() == Conversation.ConversationStatus.JOINED ||
                                conversation.getStatus() == Conversation.ConversationStatus.NOT_PARTICIPATING) {
                            Log.d("Conversation Status", "Already in conversation: " + CONVERSATION_SID);

                            QuickstartConversationsManager.this.conversation = conversation;
                            QuickstartConversationsManager.this.conversation.addListener(mDefaultConversationListener);
                            QuickstartConversationsManager.this.loadPreviousMessages(conversation, messagesAdapter);
                            progressBarMsg.setVisibility(View.GONE);
                        } else {
                            Log.d("Joining Conversation", "Status: " + conversation.getStatus());
                            joinConversation(conversation, TO_USER, messagesAdapter);
                            progressBarMsg.setVisibility(View.GONE);
                        }
                    }
                }

                @Override
                public void onError(ErrorInfo errorInfo) {
                    Log.e("Error", "Retrieving conversation failed: " + errorInfo.getMessage());
                }
            });
        } else {
            Toast.makeText(conteXt, "Please Wait!", Toast.LENGTH_SHORT).show();
        }
    }

    public boolean isConversationParticipating(String conversationSid) {
        Conversation conversation = getConversationBySid(conversationSid); // Your logic to fetch the conversation
        if (conversation != null && (conversation.getStatus() == Conversation.ConversationStatus.JOINED ||
                conversation.getStatus() == Conversation.ConversationStatus.NOT_PARTICIPATING)) {
            return true;
        }
        return false;
    }

    public Conversation getConversationBySid(String conversationSid) {
        ConversationsClient convClient = ConversationsClientManager.getInstance().getConvClient();

        if (convClient != null) {
            // Fetch all conversations or look through the active ones
            List<Conversation> conversations = convClient.getMyConversations(); // This method might vary based on the SDK
            for (Conversation conversation : conversations) {
                if (conversation.getSid().equals(conversationSid)) {
                    return conversation; // Return the conversation if SID matches
                }
            }
        }

        // Return null if no conversation found with the given SID
        return null;
    }

    public void loadAllConversations(ConversationsClient conversationsClient, Context contx, RecyclerView chatRs, ChatListAdapter chatListAdapter, List<Chat> chatList, ProgressBar progressBar, boolean isChatLoad, String PatientId) {
        this.chatRs = chatRs;
        this.chatListAdapter = chatListAdapter;
        this.chatList = chatList;
        this.progressBarChat = progressBar;
        this.isChatLoaded = isChatLoad;

        // Log for null or empty values
        Log.d("loadAllConversations", "Null/Empty Check => " +
                "conversationsClient: " + (conversationsClient == null ? "NULL" : "OK") + ", " +
                "context: " + (contx == null ? "NULL" : "OK") + ", " +
                "recyclerView: " + (chatRs == null ? "NULL" : "OK") + ", " +
                "chatListAdapter: " + (chatListAdapter == null ? "NULL" : "OK") + ", " +
                "chatList: " + (chatList == null ? "NULL" : (chatList.isEmpty() ? "EMPTY" : "Size=" + chatList.size())) + ", " +
                "progressBar: " + (progressBar == null ? "NULL" : "OK") + ", " +
                "isChatLoad: " + isChatLoad + ", " +
                "PatientId: " + (PatientId == null ? "NULL" : (PatientId.isEmpty() ? "EMPTY" : "OK")));

        try {
            List<Conversation> conversationList = conversationsClient.getMyConversations();
            Log.d("conversationListsize", String.valueOf(conversationList.size()));

            if (conversationList.isEmpty()) {
                progressBarChat.setVisibility(View.GONE);
                Toast.makeText(contx, "No Chats", Toast.LENGTH_LONG).show();
                return;
            }

            Log.d("conversationList", "Total conversations fetched: " + conversationList.size());
            Log.d("conversationListfull", "conversationListfull " + conversationList);
            for (Conversation conversation : conversationList) {
                // Remove existing listeners before adding a new one
                conversation.removeAllListeners();
                // Fetch unread messages initially
                conversation.getUnreadMessagesCount(new CallbackListener<Long>() {
                    @Override
                    public void onSuccess(Long unreadMsgCount) {
                        if (unreadMsgCount == null) {
                            unreadMsgCount = 0L; // Default value
                        }
                        handleInitialUnreadMessages(conversation, unreadMsgCount, contx, chatRs);
                        progressBarChat.setVisibility(View.GONE);
                    }

                    @Override
                    public void onError(ErrorInfo errorInfo) {
                        Log.d("Error fetching unread count", errorInfo.toString());
                    }
                });

                // Add a listener for message updates
                conversation.addListener(new ConversationListener() {
                    @Override
                    public void onMessageAdded(Message message) {
                        Log.d("TwilioChatMain", "New message received: " + message.getBody());
                        Log.d("DashonMessageAdded", "DashonMessageAdded");
                        String sender = message.getAuthor();
                        String loggedInUser = conversationsClient.getMyIdentity();
                        Log.d("sender", sender);
                        Log.d("loggedInUser", loggedInUser);
                        Log.d("Patent_Id", PatientId);
                        updateLastMessageUI(conversation, message); // Update UI with the new message

                        // Only update unread count if the sender is NOT the logged-in user
                        if (!loggedInUser.equals(sender) && !PatientId.equals(sender)) {
                            Log.d("MESSAGESEND", "MESSAGESEND");
                            // Fetch the unread count and update UI
                            conversation.getUnreadMessagesCount(new CallbackListener<Long>() {
                                @Override
                                public void onSuccess(Long unreadMsgCount) {
                                    if (unreadMsgCount == null || unreadMsgCount == 0) {
                                        unreadMsgCount = 1L; // Ensure the first message gets counted
                                    }
                                    handleUnreadMessages(conversation, unreadMsgCount, contx, chatRs, sender);
                                }

                                @Override
                                public void onError(ErrorInfo errorInfo) {
                                    Log.d("Error fetching unread count", errorInfo.toString());
                                }
                            });
                        }
                    }

                    @Override
                    public void onMessageUpdated(Message message, Message.UpdateReason reason) {
                        Log.d("TwilioonMessageUpdated", "Message updated: " + message.getBody());

                        Activity activityLatest = MyApplication.getLatestActivity();
                        Log.d("activityLatest", String.valueOf(activityLatest));

                        if (activityLatest instanceof MessageActivity) {
                            Log.d("msgupdate", "Message updated");
                            ((MessageActivity) activityLatest).refreshMessageInAdapter(message);
                        }
                        Log.d("MessageUpdate", "Updated message attributes: " + message.getAttributes().toString());
                    }

                    @Override
                    public void onMessageDeleted(Message message) {
                        Log.d("onMessageDeleted", "Message deleted: " + message.getSid());
                        // Find the deleted message in your list
                        Activity activityLatest = MyApplication.getLatestActivity();
                        Log.d("activityLatest", String.valueOf(activityLatest));

                        if (activityLatest instanceof MessageActivity) {
                            Log.d("msgdelete", "msgdelete");
                            ((MessageActivity) activityLatest).deleteMessageFromList(message.getSid());
                        }
                    }

                    @Override
                    public void onParticipantAdded(Participant participant) {}

                    @Override
                    public void onParticipantUpdated(Participant participant, Participant.UpdateReason reason) {}

                    @Override
                    public void onParticipantDeleted(Participant participant) {}

                    public void onTypingStarted(Conversation conversation, Participant participant) {
                        for (int i = 0; i < chatList.size(); i++) {
                            Chat chatModel = chatList.get(i);

                            Log.d("Before Update", chatModel.getChatMemberName() + " isTyping: " + chatModel.isTyping());
                            if (chatModel.getChatMemberName().equals(participant.getIdentity())) {
                                chatList.get(i).setTyping(true); // If Chat has a setter
                                Log.d("After Update", chatList.get(i).getChatMemberName() + " isTyping: " + chatList.get(i).isTyping());
                                final int index = i;
                                new Handler(Looper.getMainLooper()).post(() -> {
                                    chatListAdapter.notifyItemChanged(index); // Notify RecyclerView
                                    Log.d("notifyItemChanged", "Updated UI at position: " + index);
                                });
                                break;
                            }
                        }

                        // Now update typing indicator in MessageActivity
                        new Handler(Looper.getMainLooper()).post(() -> {
                            MessageActivity messageActivity = MessageActivity.getInstance();
                            if (messageActivity != null) {
                                Log.d("TypingEvent", "Updating typing indicator in MessageActivity.");
                                messageActivity.showTypingIndicator(participant.getIdentity());
                            } else {
                                Log.e("TypingEvent", "MessageActivity instance is NULL!");
                            }
                        });
                    }

                    @Override
                    public void onTypingEnded(Conversation conversation, Participant participant) {
                        Log.d("onTypingEnded", "Typing ended by: " + participant.getIdentity());

                        for (int i = 0; i < chatList.size(); i++) {
                            Chat chatModel = chatList.get(i);

                            if (chatModel.getChatMemberName().equals(participant.getIdentity())) {
                                chatList.get(i).setTyping(false); // If Chat has a setter
                                Log.d("Typing Updated", "Set typing false for: " + chatModel.getChatMemberName());
                                final int index = i;

                                new Handler(Looper.getMainLooper()).post(() -> {
                                    chatListAdapter.notifyItemChanged(index);
                                    Log.d("notifyItemChanged", "Updated UI at position: " + index);
                                });

                                break;
                            }
                        }

                        // Now update typing indicator in MessageActivity
                        new Handler(Looper.getMainLooper()).post(() -> {
                            MessageActivity messageActivity = MessageActivity.getInstance();
                            if (messageActivity != null) {
                                Log.d("TypingEvent", "Updating typing indicator in MessageActivity.");
                                messageActivity.hideTypingIndicator();
                            } else {
                                Log.e("TypingEvent", "MessageActivity instance is NULL!");
                            }
                        });
                    }

                    @Override
                    public void onSynchronizationChanged(Conversation conversation) {
                        if (conversation.getSynchronizationStatus() == Conversation.SynchronizationStatus.ALL) {
                            Log.d("TwilioSync", "Conversation fully synchronized!");
                            ConversationsClientManager.getInstance().setConvSynced(true);

                            // Fetch the last message for this conversation
                            conversation.getLastMessages(1, new CallbackListener<List<Message>>() {
                                @Override
                                public void onSuccess(List<Message> messages) {
                                    if (!messages.isEmpty()) {
                                        Message lastMessage = messages.get(0);
                                        Log.d("LastMessage", "Last message: " + lastMessage.getBody());
                                        // Update the UI with the last message
                                        updateLastMessageUI(conversation, lastMessage);
                                    }
                                }

                                @Override
                                public void onError(ErrorInfo errorInfo) {
                                    Log.e("TwilioSync", "Error fetching last message: " + errorInfo.toString());
                                }
                            });

                        } else {
                            Log.d("TwilioSync", "Conversation still synchronizing...");
                        }
                    }
                });


                conversation.getLastMessages(1, new CallbackListener<List<Message>>() {
                    @Override
                    public void onSuccess(List<Message> messages) {
                        if (!messages.isEmpty()) {
                            Message lastMessage = messages.get(0);
                            Log.d("chatListSIZEVAL", "chatListSIZEVAL " + chatList.size());
                            Log.d("getLastMessages", "Initial last message: " + lastMessage.getBody());
                            updateLastMessageUI(conversation, lastMessage); // Update the last message text immediately
                        }
                    }

                    @Override
                    public void onError(ErrorInfo errorInfo) {
                        Log.e("getLastMessages", "Error fetching initial last message: " + errorInfo.toString());
                    }
                });


            }
        } catch (Exception e) {
            Log.e("Conversation Error", e.getMessage());
        }
    }

    private void updateLastMessageUI(Conversation conversation, Message lastMessage) {
        Log.d("updateLastMessageUIMETHOD", "updateLastMessageUIMETHOD" + lastMessage.getBody());
        Log.d("chatListSIZE", "chatListSIZE" + chatList.size());
        for (int i = 0; i < chatList.size(); i++) {
            Chat chatModel = chatList.get(i);
            Log.d("TwilioSync", "Member Name: " + chatModel.getChatMemberName() + ", Friendly Name: " + conversation.getFriendlyName());
            Log.d("TwilioSync", "chatModel.getSID() " + chatModel.getSID() + ", conversation.getSid(): " + conversation.getSid());

            if (chatModel.getSID().equals(conversation.getSid())) {
                // Log the last message body
                Log.d("updateLastMessageUI", "Last message body: " + lastMessage.getBody());
                chatModel.setLastMsgText(lastMessage.getBody()); // Assuming `Chat` has a method to set last message
                final int index = i;
                new Handler(Looper.getMainLooper()).post(() -> {
                    chatListAdapter.notifyItemChanged(index); // Update RecyclerView
                    Log.d("UIUpdate", "Updated last message for conversation: " + conversation.getSid());
                });

                break;
            }
        }
    }

    private void handleInitialUnreadMessages(Conversation conversation, Long unreadMsgCount, Context context, RecyclerView chatRs) {
        if (conversation == null) {
            Log.d("handleInitialUnreadMessages", "Conversation is null");
            return;
        }

        if (chatList == null) {
            chatList = new ArrayList<>();
        }

        if (chatListAdapter == null) {
            chatListAdapter = new ChatListAdapter(chatList, context, conversation.getSid());
            chatRs.setAdapter(chatListAdapter);
        }

        String[] nameParts = conversation.getFriendlyName().split("-");
        String friendlyUserName = nameParts[0];
        String friendlyName = getMemberNameFromList(friendlyUserName, context);

        Date lastMessageDate = conversation.getLastMessageDate();
        long timestamp = (lastMessageDate != null) ? lastMessageDate.getTime() : System.currentTimeMillis();
        String formattedTime = (lastMessageDate != null)
                ? TimeFormatter.formatTimestampFromLog(lastMessageDate.toString())
                : "Just Now";

        String convSid = conversation.getSid();
        int unreadMessages = (unreadMsgCount != null) ? unreadMsgCount.intValue() : 0;

        conversation.getLastMessages(1, new CallbackListener<List<Message>>() {
            @Override
            public void onSuccess(List<Message> messages) {
                String lastMessageText = (!messages.isEmpty()) ? messages.get(0).getBody() : "";

                updateChatList(friendlyUserName, friendlyName, formattedTime, unreadMessages, timestamp, context, convSid, lastMessageText);
            }

            @Override
            public void onError(ErrorInfo errorInfo) {
                Log.e("LastMessageError", "Failed to fetch last message: " + errorInfo);
                updateChatList(friendlyUserName, friendlyName, formattedTime, unreadMessages, timestamp, context, convSid, "");
            }
        });
    }

    private void updateChatList(String friendlyUserName,String friendlyName, String formattedTime, int unreadMessages, long timestamp, Context context, String sid, String lastMessageText) {
        boolean chatUpdated = false;
        Log.d("updateChatList1","updateChatList1");
        for (Chat chat : chatList) {
            if (chat.getChatMemberName().equals(friendlyName)) {
                chat.setUnreadCount(unreadMessages); // Update unread count
                chat.setTimeStamp(formattedTime); // Update time
                chat.setSID(sid);
                chat.setTimeStampMillis(timestamp);
                chat.setFriendlyUsername(friendlyUserName);
                chat.setLastMsgText(lastMessageText);
                chatUpdated = true;
                break;
            }
        }
        Log.d("chatListONMAIN", String.valueOf(chatList.size()));
        Log.d("chatListONMAIN", String.valueOf(chatList));
        if (!chatUpdated) {
            // Add new chat entry if not found
            chatList.add(new Chat(friendlyUserName,friendlyName, formattedTime, unreadMessages, timestamp,false, lastMessageText, sid));
            Log.d("AddedtoChatModel", friendlyName + formattedTime + unreadMessages + timestamp+ lastMessageText );
        }

        // **Sort the list by timestamp in descending order (latest first)**
        // Sort the list by timestampMillis in descending order (latest chat first)
        Collections.sort(chatList, (chat1, chat2) -> Long.compare(chat2.getTimestampMillis(), chat1.getTimestampMillis()));
        Log.d("UI sort", "Updating chat list in UI");

        ((Activity) context).runOnUiThread(() -> {
            Log.d("UI Update", "Updating chat list in UI");
            Log.d("updateChatListCaller1", "Calling updateChatList from handleUnreadMessages with " + chatList.size());

            chatListAdapter.updateChatList(chatList);
            // progressBarChat.setVisibility(View.GONE);
        });
    }

    private void handleUnreadMessages(Conversation conversation, Long unreadMsgCount, Context context, RecyclerView chatRs, String sender) {
        Log.d("handleUnreadMessagesONMSGADD", "Conversation is null");
        if (conversation == null) {
            Log.d("handleUnreadMessagesONMSGADD", "Conversation is null");
            return;
        }

        if (chatList == null) {
            Log.d("chatListNULL", "chatList is null");
            chatList = new ArrayList<>(); // Initialize if null
        }

        if (chatListAdapter == null) {
            Log.e("chatListAdapter", "chatListAdapter is null");
            chatListAdapter = new ChatListAdapter(chatList, context, conversation.getSid());
            chatRs.setAdapter(chatListAdapter);
        }

        String friendlyName = conversation.getFriendlyName().split("-")[0];
        String friendlyUserName = conversation.getFriendlyName().split("-")[0];
        // Replace friendlyName with MemberName if found in the list
        friendlyName = getMemberNameFromList(friendlyName,context);
        Log.d("friendlyUserName:-", friendlyUserName);
        Log.d("friendlyName2new:-", friendlyName);
        Date lastMessageDate = conversation.getLastMessageDate();
        long timestamp = (lastMessageDate != null) ? lastMessageDate.getTime() : System.currentTimeMillis();
        String formattedTime = (lastMessageDate != null) ? TimeFormatter.formatTimestampFromLog(lastMessageDate.toString()) : "No Messages";
        int unreadMessages = (unreadMsgCount != null) ? unreadMsgCount.intValue() : 0;
        Log.d("ConversationInfo", "Friendly Name: " + friendlyName + ", Last Message Time: " + formattedTime + ", Unread Messages: " + unreadMessages);
        String convSid = conversation.getSid();
        boolean chatUpdated = false;
        ConversationsClient conversationsClient = ConversationsClientManager.getInstance().getConvClient();

        String loggedInUser = conversationsClient.getMyIdentity();
        for (Chat chat : chatList) {
            if (chat.getChatMemberName().equals(friendlyName)) {
                Log.d("sender",sender);
                Log.d("loggedInUser",loggedInUser);
                chat.setUnreadCount(unreadMessages); // Update unread count
                chat.setTimeStamp(formattedTime); // Update time
                chat.setTimeStampMillis(timestamp);
                chat.setFriendlyUsername(friendlyUserName);
                chat.setLastMsgText(chat.getLastMsgText());
                chat.setSID(convSid);
                chatUpdated = true;
                break;
            }
        }

        if (!chatUpdated) {
            Log.d("chatUpdated", String.valueOf(chatUpdated));
            // Add new chat entry if not found
            chatList.add(new Chat(friendlyUserName,friendlyName, formattedTime, unreadMessages, timestamp, false, lastMessageText, convSid));
            Log.d("AddedtoChatModel:--",friendlyName+formattedTime+unreadMessages+timestamp+ lastMessageText);
        }

// Print sorted chat names to confirm sorting
        Log.d("SortedChatList", "Chats in sorted order:");
        for (Chat chat : chatList) {
            Log.d("SortedChat", "Chat Name: " + chat.getChatMemberName() + ", Timestamp: " + chat.getTimestampMillis());
        }

        Collections.sort(chatList, (chat1, chat2) -> Long.compare(chat2.getTimestampMillis(), chat1.getTimestampMillis()));

        ((Activity) context).runOnUiThread(() -> {
            Log.d("GETTINGINSIDETHREAD ", String.valueOf(chatListAdapter));
            Log.d("updateChatListCaller2", "Calling updateChatList from handleUnreadMessages with " + chatList.size());

            chatListAdapter.updateChatList(chatList);
            progressBarChat.setVisibility(View.GONE);
        });
    }

    private String getMemberNameFromList(String friendlyName, Context context) {
        SharedPreferences sharedPreferences = context.getSharedPreferences("RPMUserApp", Context.MODE_PRIVATE);
        String savedResponse = sharedPreferences.getString("members_list", "[]");

        try {
            JSONArray jsonArray = new JSONArray(savedResponse);
            for (int i = 0; i < jsonArray.length(); i++) {
                JSONObject obj = jsonArray.getJSONObject(i);
                String memberUserName = obj.getString("MemberUserName").trim(); // Trim spaces
                String memberName = obj.getString("MemberName").trim(); // Trim spaces
                Log.d("hamemberUserName:memberName", memberUserName + " : " + friendlyName);

                if (memberUserName.equalsIgnoreCase(friendlyName.trim())) {  // Trim and Ignore Case
                    return memberName; // Found matching user, return their MemberName
                }
            }
        } catch (JSONException e) {
            e.printStackTrace();
        }

        return friendlyName; // If no match found, return original friendlyName
    }


    public void createAndJoinConversation(String TO_USER, ConversationsClient convClient, Context context,
                                          String convToken, MessageActivity.MessagesAdapter messagesAdapter,
                                          String messageBody, String token, String userId) {

        Log.d("TO_USER", TO_USER);
        Log.d("createConvClient", String.valueOf(convClient));
        Log.d("getMyUser", String.valueOf(convClient.getMyUser()));

        String createFriendlyName = TO_USER + "-" + userId;
        Log.d("FRIENDLY_NAME", createFriendlyName);

        convClient.createConversation(createFriendlyName, new CallbackListener<Conversation>() {
            @Override
            public void onSuccess(Conversation conversation) {
                if (conversation == null) {
                    Log.e("Conversation", "Created conversation is null");
                    return;
                }

                Log.d("createConversation success", conversation.getSid());
                FileLogger.d("createConversation success", conversation.getSid());

                // Assuming `MessageActivity` implements a method to handle the SID
                if (context instanceof MessageActivity) {
                    MessageActivity activity = (MessageActivity) context;
                    activity.updateConversationSid(conversation.getSid());  // Pass SID back to MessageActivity
                }

                if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.Q) {
                    try {
                        chatUpdate(conversation.getSid(), TO_USER, context, convToken);
                    } catch (IOException e) {
                        e.printStackTrace();
                    }
                }

                joinConversationAndSendMessage(conversation, TO_USER, messagesAdapter,
                        messageBody, context, conversation.getSid(), token, userId);
            }

            @Override
            public void onError(ErrorInfo errorInfo) {
                Log.e("Error creating conversation", errorInfo.getMessage());
            }
        });
    }

    private void joinConversation(final Conversation conversation, String TO_USER, MessageActivity.MessagesAdapter messagesAdapter) {
        FileLogger.d(DashboardFragment.TAG, "joinConversation: " + conversation.getUniqueName());
        Log.d(DashboardFragment.TAG, "joinConversation: " + conversation.getUniqueName());
        if (conversation.getStatus() == Conversation.ConversationStatus.JOINED) {
            FileLogger.d("joinConversatiogetStatus", String.valueOf(conversation.getStatus()));
            Log.d("joinConversatiogetStatus", String.valueOf(conversation.getStatus()));
            QuickstartConversationsManager.this.conversation = conversation;
            FileLogger.d(DashboardFragment.TAG, "Already joined default joinconversation");
            Log.d(DashboardFragment.TAG, "Already joined default joinconversation");
            QuickstartConversationsManager.this.conversation.addListener(mDefaultConversationListener);
            return;
        }
        conversation.addParticipantByIdentity(TO_USER,null, new StatusListener() {
            @Override
            public void onSuccess() {
                FileLogger.d("successaddParticipantByIdentityDEF", TO_USER);
                Log.d("successaddParticipantByIdentityDEF", TO_USER);
            }
        });

        conversation.join(new StatusListener() {
            @Override
            public void onSuccess() {
                QuickstartConversationsManager.this.conversation = conversation;
                FileLogger.d(DashboardFragment.TAG, "Joined default conversation joinDEF");
                Log.d(DashboardFragment.TAG, "Joined default conversation joinDEF");
                QuickstartConversationsManager.this.conversation.addListener(mDefaultConversationListener);
                QuickstartConversationsManager.this.loadPreviousMessages(conversation,messagesAdapter);
            }

            @Override
            public void onError(ErrorInfo errorInfo) {
                Log.e(DashboardFragment.TAG, "Error joining conversation: " + errorInfo.getMessage());
                FileLogger.d(DashboardFragment.TAG, "Error joining conversation: " + errorInfo.getMessage());
            }
        });
    }


    private void joinConversationAndSendMessage(final Conversation conversation,
                                                String TO_USER,
                                                MessageActivity.MessagesAdapter messagesAdapter,
                                                String messageBody,
                                                Context context,
                                                String conversationSid,
                                                String token,
                                                String userId) {

        FileLogger.d(DashboardFragment.TAG, "joinConversationAndSendMessage " + conversation.getUniqueName());
        Log.d(DashboardFragment.TAG, "joinConversationAndSendMessage " + conversation.getUniqueName());

        if (conversation.getStatus() == Conversation.ConversationStatus.JOINED) {
            Log.d("ConversationStatus", "Already joined");
            FileLogger.d("ConversationStatus", "Already joined");

            QuickstartConversationsManager.this.conversation = conversation;
            conversation.addListener(mDefaultConversationListener);
            loadPreviousMessages(conversation, messagesAdapter);

            addParticipantAndSendMessage(conversation, TO_USER, messageBody, context, conversationSid, token, userId, messagesAdapter);
            return;
        }

        conversation.join(new StatusListener() {
            @Override
            public void onSuccess() {
                QuickstartConversationsManager.this.conversation = conversation;
                Log.d(DashboardFragment.TAG, "Joined conversation");
                FileLogger.d(DashboardFragment.TAG, "Joined conversation");

                conversation.addListener(mDefaultConversationListener);
                loadPreviousMessages(conversation, messagesAdapter);

                addParticipantAndSendMessage(conversation, TO_USER, messageBody, context, conversationSid, token, userId, messagesAdapter);
            }

            @Override
            public void onError(ErrorInfo errorInfo) {
                Log.e(DashboardFragment.TAG, "Error joining conversation: " + errorInfo.getMessage());
            }
        });
    }


    private void addParticipantAndSendMessage(Conversation conversation,
                                              String TO_USER,
                                              String messageBody,
                                              Context context,
                                              String conversationSid,
                                              String token,
                                              String userId,
                                              MessageActivity.MessagesAdapter messagesAdapter) {

        conversation.addParticipantByIdentity(TO_USER, null, new StatusListener() {
            @Override
            public void onSuccess() {
                Log.d("Participant Added", TO_USER);
                FileLogger.d("Participant Added", TO_USER);

                sendMessage(messageBody, context, conversationSid, token, userId, TO_USER, messagesAdapter);
            }

            @Override
            public void onError(ErrorInfo errorInfo) {
                Log.e("Add Participant Error", errorInfo.getMessage());
            }
        });
    }

    private void loadPreviousMessages(final Conversation conversation, MessageActivity.MessagesAdapter messagesAdapter) {
        FileLogger.d("loadPreviousMessagesconversation", conversation.toString());
        Log.d("loadPreviousMessagesconversation", conversation.toString());
        if (conversation.getSynchronizationStatus() != Conversation.SynchronizationStatus.ALL) {
            Log.e("TwilioChat", "Conversation is not yet synchronized. Waiting...");
            return;  // Don't proceed until it's fully synchronized
        }
        conversation.getLastMessages(100,
                new CallbackListener<List<Message>>() {
                    @Override
                    public void onSuccess(List<Message> result) {

                        Log.d("loadPreviousMessages", result.toString());
                        Log.d("messagesonSuccess", messages.toString());
                        messages.clear();
                        messages.addAll(result);
                        Log.d("messages", messages.toString());
                        Log.d("messages result", messages.toString());
                        Log.d("messagesize", String.valueOf(messages.size()));
                        for(int i=0; i< messages.size() ; i++ )
                        {
                            messages.get(i);
                            Log.d("messagesgeti " + i , String.valueOf(messages.get(i)));
                        }

                        Log.d("messageAdap111", String.valueOf(messagesAdapter));
                        // Set the messages to the adapter using the setMessages method
                        if (messagesAdapter != null) {
                            Log.d("messagesQQQ", messages.toString());
                            Log.d("messageAdap", String.valueOf(messagesAdapter));
                            messagesAdapter.setMessages(messages);
                            Log.d("messagesWWW", messages.toString());
                        }
                        if (conversationsManagerListener != null) {
                            Log.d("conversationsManagerListener not null", "conversationsManagerListener not null");
                            conversationsManagerListener.reloadMessages();

                        }

                        // Get the count of unread messages in the channel
                        conversation.getUnreadMessagesCount(new CallbackListener<Long>() {
                            @Override
                            public void onSuccess(Long unreadMessageCount) {
                                Log.d("convunrdmsgCount " , String.valueOf(unreadMessageCount));
                            }

                            @Override
                            public void onError(ErrorInfo errorInfo) {
                            }
                        });
                    }
                });
    }

    private final ConversationsClientListener mConversationsClientListener =
            new ConversationsClientListener() {

                @Override
                public void onConversationAdded(Conversation conversation) {
                    Log.d("onConversationAdded", conversation.toString());
                    Log.d("onConversation getSid", conversation.getSid());
                    Log.d("onConversationgetsttus", String.valueOf(conversation.getStatus()));
                    Log.d("onConversationgetstate", String.valueOf(conversation.getState()));
                }

                @Override
                public void onConversationUpdated(Conversation conversation, Conversation.UpdateReason updateReason) {
                    Log.d("onConversationUpdatedrsn", updateReason.toString());
                    Log.d("onConversupdtgetsttus", String.valueOf(conversation.getStatus()));
                    Log.d("onConversupdtgetstate", String.valueOf(conversation.getState()));
                    System.out.println("quickactnew");
                    System.out.println("cont");
                    System.out.println("contextValue");
                    System.out.println(contextValue);
                    System.out.println("myApplication");
                    pref = contextValue.getSharedPreferences("RPMUserApp", MODE_PRIVATE);
                    editor = pref.edit();
                    savedUserName = pref.getString("UserName", null);
                    String convupFriendlyName = conversation.getFriendlyName().split("-")[0];
                    Log.d("equal1", "equal");
                    ((MyApplication) getApplication()).getLatestActivity();
                    System.out.println("chatctvty");
                    System.out.println(((MyApplication) getApplication()).getLatestActivity());

                    try {
                        List<Conversation> conversationList = conversationsClient.getMyConversations();
                        Log.d("ConvUpconversationL", String.valueOf(conversationList));
                        Log.d("ConvUpconversationLists", String.valueOf(conversationList.size()));
                        int size = conversationList.size();
                    } catch (Exception e) {
                        e.printStackTrace();
                    }

                    Log.d("savedUserName2", String.valueOf(savedUserName));
                    Log.d("savedUserName3", String.valueOf(updateReason.toString().equals("LAST_MESSAGE")));
                    Log.d("savedUserName4", String.valueOf(!convupFriendlyName.equals(savedUserName)));
                    Log.d("savedUserName5", String.valueOf(convupFriendlyName.equals(savedUserName)));
                    System.out.println((convupFriendlyName.equals(savedUserName) == false));
                    System.out.println((convupFriendlyName.equals(pref.getString("UserName", null))));
                    Log.d("createdby", conversation.getCreatedBy());

                    if (updateReason == Conversation.UpdateReason.LAST_MESSAGE) {
                    }
                }

                @Override
                public void onConversationDeleted(Conversation conversation) {
                    Log.d("onConversationDeleted", conversation.toString());
                }

                @Override
                public void onConversationSynchronizationChange(Conversation conversation) {
                    Log.d("onConversationSynchronizationChange", conversation.toString());

                }

                @Override
                public void onError(ErrorInfo errorInfo) {
                    Log.d("ErrorInfo", errorInfo.toString());
                }

                @Override
                public void onUserUpdated(User user, User.UpdateReason updateReason) {
                    Log.d("onUserUpdated", updateReason.toString());
                }

                @Override
                public void onUserSubscribed(User user) {
                    Log.d("onUserSubscribed", user.toString());
                    Log.d("onUserSubscribed", user.getFriendlyName());
                    Log.d("onUserSubscribed", user.getIdentity());
                    Log.d("onUserSubscribed", String.valueOf(user.getAttributes()));
                }

                @Override
                public void onUserUnsubscribed(User user) {
                    Log.d("onUserUnsubscribed", user.toString());
                }

                @RequiresApi(api = Build.VERSION_CODES.Q)
                @Override
                public void onClientSynchronization(ConversationsClient.SynchronizationStatus synchronizationStatus) {

                    Log.d("onClientSynchronization", synchronizationStatus.toString());

                    if (synchronizationStatus == ConversationsClient.SynchronizationStatus.COMPLETED) {
                        Log.d("SynchronizationStatus.COMPLETED", synchronizationStatus.toString());
                        ConversationsClient twilioClient = ConversationsClientManager.getInstance().getConvClient();
                        Log.d("twilioClient",String.valueOf(twilioClient));
                        if(twilioClient!= null) {
                            Log.d("twilioClientNOTNULL",String.valueOf(twilioClient));
                            ConversationsClientManager.getInstance().setClientSynced(true);
                            Log.d("isSyncedSYNC",String.valueOf(isSynced));
                            registerFCMTokenForTwilioChat(twilioClient);
                        }

                        hideLoading();  // Hide the progress bar here
                    }
                }

                @Override
                public void onNewMessageNotification(String s, String s1, long l) {
                    Log.d("onNewMessageNotification", s.toString());
                }

                @Override
                public void onAddedToConversationNotification(String s) {
                    Log.d("onAddedToConversationNotification", s.toString());
                }

                @Override
                public void onRemovedFromConversationNotification(String s) {
                    Log.d("onRemovedFromConversationNotification", s.toString());
                }

                @Override
                public void onNotificationSubscribed() {
                    Log.d("onNotificationSubscribed", "onNotificationSubscribed");
                }

                @Override
                public void onNotificationFailed(ErrorInfo errorInfo) {
                    Log.d("onNotificationFailed", "onNotificationFailed");
                }

                @Override
                public void onConnectionStateChange(ConversationsClient.ConnectionState connectionState) {
                    Log.d("onConnectionStateChange", connectionState.toString());

                    switch (connectionState) {
                        case CONNECTING:
                            // The client is attempting to establish a connection
                            break;
                        case CONNECTED:
                            // The client is connected and ready to use
                            break;
                        case DISCONNECTED:
                            // The client lost its connection (network issue, etc.)
                            // Automatic reconnect attempts will be made by the SDK
                            break;
                        case DENIED:
                            // The client connection was denied due to invalid token or other reasons
                            break;
                        case ERROR:
                            // An error occurred during connection (failed reconnect attempts)
                            break;
                    }
                }

                @Override
                public void onTokenExpired() {
                    Log.d("onTokenExpired", "onTokenExpired");

                    regenerateToken(appAccessToken , contxt , new AccessTokenListener() {
                        @Override
                        public void receivedAccessToken(@Nullable String tokenNew, @Nullable Exception exception) {
                            if (tokenNew != null) {
                                ConversationsClient regClient = ConversationsClientManager.getInstance().getConvClient();

                                Log.d("regClient ", String.valueOf(regClient));
                                Log.d("regconversationsClient", String.valueOf(conversationsClient));
                                regClient.updateToken(tokenNew, new StatusListener() {
                                    @Override
                                    public void onSuccess() {
                                        Log.d(DashboardFragment.TAG, "Refreshed access token.");
                                    }
                                });
                            }
                        }
                    });
                }

                @Override
                public void onTokenAboutToExpire() {
                    Log.d("onTokenAboutToExpire", "onTokenAboutToExpire");
                    regenerateToken(appAccessToken , contxt , new AccessTokenListener() {
                        @Override
                        public void receivedAccessToken(@Nullable String tokenNew, @Nullable Exception exception) {
                            if (tokenNew != null) {

                                Log.d("aboutotexpireregenerateToken", "aboutotexpireregenerateToken");
                                ConversationsClient regClient = ConversationsClientManager.getInstance().getConvClient();

                                Log.d("regClient ", String.valueOf(regClient));
                                Log.d("regconversationsClient", String.valueOf(conversationsClient));
                                regClient.updateToken(tokenNew, new StatusListener() {
                                    @Override
                                    public void onSuccess() {
                                        Log.d(DashboardFragment.TAG, "Refreshed access token.");
                                    }
                                });
                            }
                        }
                    });
                }


            };

    private void hideLoading() {
        if (progressBarChat != null) {
            progressBarChat.setVisibility(View.GONE);
        }
    }


    private final CallbackListener<ConversationsClient> mConversationsClientCallback =
            new CallbackListener<ConversationsClient>() {
                @Override
                public void onSuccess(ConversationsClient conversationsClient) {
                    Log.d("callbacksClient", String.valueOf(conversationsClient));

                    QuickstartConversationsManager.this.conversationsClient = conversationsClient;
                    ConversationsClientManager.getInstance().setConvClient(conversationsClient);
                    conversationsClient.addListener(QuickstartConversationsManager.this.mConversationsClientListener);
                    Log.d(DashboardFragment.TAG, "Success creating Twilio Conversations Client");

                    // Ensure the token response is delivered
                    new Handler(Looper.getMainLooper()).post(new Runnable() {
                        @Override
                        public void run() {
                            if (tokenResponseListener != null) {
                                Log.d("TokenResponse", "Notifying success to listener.");
                                tokenResponseListener.receivedTokenResponse(true, null);
                            }
                        }
                    });
                }

                @Override
                public void onError(ErrorInfo errorInfo) {
                    Log.e(DashboardFragment.TAG, "Error creating Twilio Conversations Client: " + errorInfo.getMessage());
                    Log.d("convonError", "convonError");

                    retryWithNewToken();
                }
            };

    private void registerFCMTokenForTwilioChat(ConversationsClient twilioClient) {
        FirebaseMessaging.getInstance().getToken()
                .addOnCompleteListener(task -> {
                    if (task.isSuccessful() && task.getResult() != null) {
                        String fcmToken = task.getResult();
                        twilioClient.registerFCMToken(new ConversationsClient.FCMToken(fcmToken), new StatusListener() {
                            @Override
                            public void onSuccess() {
                                Log.d("fcmToken",fcmToken);
                                Log.d("Twilio", "FCM Token registered successfully with Twilio");
                            }
                            @Override
                            public void onError(ErrorInfo errorInfo) {
                                Log.e("Twilio", "Error registering FCM token: " + errorInfo.getMessage());
                            }
                        });
                    } else {
                        Log.e("Twilio", "Fetching FCM token failed");
                    }
                });
    }


    private boolean tokenRetryHandled = false; // Add this to your class as a field

    private void retryWithNewToken() {
        // Prevent multiple re-entries
        if (tokenRetryHandled) {
            Log.d("retryWithNewToken", "Already handled, skipping...");
            return;
        }

        retrieveAccessTokenFromServerNew(contxt, appAccessToken, new TokenResponseListener() {
            @Override
            public void receivedTokenResponse(boolean success, @Nullable Exception exception) {
                if (success) {
                    Log.d("retrywithnewtokensuccess", String.valueOf(success));

                    // Ensure the success is only handled once
                    tokenRetryHandled = true;

                    new Handler(Looper.getMainLooper()).post(() -> {
                        if (tokenResponseListener != null) {
                            Log.d("retryWithNewToken", "Notifying success to listener.");
                            tokenResponseListener.receivedTokenResponse(true, null);
                        }
                    });
                } else {
                    Log.e("TwilioChat", "Failed to retrieve new token");
                }
            }
        });
    }

    private final ConversationListener mDefaultConversationListener = new ConversationListener() {

        @Override
        public void onMessageAdded(final Message message) {
            Log.d("Messageadded", String.valueOf(message));
            Log.d("MessageaddgetAuthor", String.valueOf(message.getAuthor()));

            pref = contextValue.getSharedPreferences("RPMUserApp", MODE_PRIVATE);
            editor = pref.edit();
            savedUserName = pref.getString("UserName", null);
            String capitalizedUsername = savedUserName.toUpperCase();

            if (!message.getAuthor().equals(capitalizedUsername)) {  // Message from someone else (Received)
                Log.d("New Incoming Message", "Count should be updated!");
                // Update unread message count
                unreadMessageCount++;  // Increment your unread count variable
            }

            messages.add(message);
            if (conversationsManagerListener != null) {
                conversationsManagerListener.receivedNewMessage(message.getAuthor().equals(capitalizedUsername));
            }
        }

        @Override
        public void onMessageUpdated(Message message, Message.UpdateReason updateReason) {
            Log.d(DashboardFragment.TAG, "Message updated: " + message.getBody());
            Log.d("Messageupdated", String.valueOf(message));
        }

        @Override
        public void onMessageDeleted(Message message) {
            Log.d(DashboardFragment.TAG, "Message deleted");
        }

        @Override
        public void onParticipantAdded(Participant participant) {
            Log.d(DashboardFragment.TAG, "Participant added: " + participant.getIdentity());
        }

        @Override
        public void onParticipantUpdated(Participant participant, Participant.UpdateReason updateReason) {
            Log.d(DashboardFragment.TAG, "Participant updated: " + participant.getIdentity() + " " + updateReason.toString());
        }

        @Override
        public void onParticipantDeleted(Participant participant) {
            Log.d(DashboardFragment.TAG, "Participant deleted: " + participant.getIdentity());
        }

        @Override
        public void onTypingStarted(Conversation conversation, Participant participant) {
            Log.d(DashboardFragment.TAG, "Started Typing: " + participant.getIdentity());
            Log.d("Started Typing:", "Started Typing:");
        }

        @Override
        public void onTypingEnded(Conversation conversation, Participant participant) {
            Log.d(DashboardFragment.TAG, "Ended Typing: " + participant.getIdentity());
            Log.d("Ended Typing:", "Ended Typing:");
        }

        @Override
        public void onSynchronizationChanged(Conversation conversation) {
            Log.d("onSynchronizationChanged", String.valueOf(conversation));
        }
    };

    @RequiresApi(api = Build.VERSION_CODES.Q)
    private void chatUpdate(String conversationSID, String toUser, Context context, String convToken) throws IOException {

        Log.d("chatUpdate", "chatUpdate");
        String CHAT_UPDATE_URL = BASE_URL + CHAT_UPDATE;
        Log.d("CHAT_UPDATE_URL", CHAT_UPDATE_URL);
        Log.d("toUser", toUser);
        Log.d("conversationSID", conversationSID);
        String chatToken = ConversationsClientManager.getInstance().getChatToken();
        Log.d("chatToken", chatToken);
        String ToUser = toUser;
        String ConversationSid = conversationSID;
        String ChatToken = chatToken;

        JSONObject postData = new JSONObject();
        try {
            postData.put("ToUser", ToUser);
            postData.put("ConversationSid", ConversationSid);
            postData.put("ChatToken", ChatToken);

        } catch (JSONException e) {
            Log.d("JSONException", e.toString());
            e.printStackTrace();
            Log.d("e", e.toString());
        }

        StatusCodeRequest statusCodeRequest = new StatusCodeRequest(Request.Method.POST, CHAT_UPDATE_URL,
                new Response.Listener<Integer>() {
                    @Override
                    public void onResponse(Integer statusCode) {
                        Log.d("chatUpdatestatusCode", statusCode.toString());
                        FileLogger.d("chatUpdatestatusCode", statusCode.toString());
                        // Handle the status code
                        // You can check if the status code is 200 (OK) or handle different codes separately
                    }
                },
                new Response.ErrorListener() {
                    @Override
                    public void onErrorResponse(VolleyError error) {
                        // Handle the error
                    }
                })

        {
            @Override
            public String getBodyContentType() {
                return "application/json";
            }

            @Override
            public byte[] getBody() throws AuthFailureError {
                return postData.toString().getBytes();
            }
            @Override
            public Map<String, String> getHeaders() throws AuthFailureError {
                Map<String, String> headers = new HashMap<>();
                headers.put("Bearer",convToken);
                Log.d("chattokup",headers.toString());
                return headers;
            }
        };

        //creating a request queue
        RequestQueue requestQ = Volley.newRequestQueue(context);
        //adding the string request to request queue
        statusCodeRequest.setRetryPolicy(new DefaultRetryPolicy(0,-1,DefaultRetryPolicy.DEFAULT_BACKOFF_MULT));
        requestQ.add(statusCodeRequest);

    }

    public ArrayList<Message> getMessages() {
        Log.d("getMessages list", messages.toString());
        return messages;
    }

    public void setListener(QuickstartConversationsManagerListener listener)  {
        Log.d("getMessages setListener", "setListener");
        this.conversationsManagerListener = listener;
    }
}