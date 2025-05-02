package com.rpm.clynx.fragments;

import static com.rpm.clynx.utility.Links.BASE_URL;
import static com.rpm.clynx.utility.Links.CHAT_SID;
import static com.rpm.clynx.utility.Links.chatheartbeat_URL;
import android.annotation.SuppressLint;
import android.content.Context;
import android.content.SharedPreferences;
import android.os.Build;
import android.os.Bundle;
import android.os.Handler;
import android.text.Editable;
import android.text.TextWatcher;
import android.text.method.ScrollingMovementMethod;
import android.util.Log;
import android.view.LayoutInflater;
import android.view.MotionEvent;
import android.view.View;
import android.view.ViewGroup;
import android.widget.EditText;
import android.widget.ImageButton;
import android.widget.ImageView;
import android.widget.LinearLayout;
import android.widget.ProgressBar;
import android.widget.RelativeLayout;
import android.widget.TextView;
import android.widget.Toast;
import android.widget.Toolbar;
import androidx.annotation.NonNull;
import androidx.annotation.RequiresApi;
import androidx.appcompat.app.AlertDialog;
import androidx.appcompat.app.AppCompatActivity;
import androidx.recyclerview.widget.LinearLayoutManager;
import androidx.recyclerview.widget.RecyclerView;
import com.android.volley.AuthFailureError;
import com.android.volley.DefaultRetryPolicy;
import com.android.volley.Request;
import com.android.volley.RequestQueue;
import com.android.volley.Response;
import com.android.volley.VolleyError;
import com.android.volley.toolbox.JsonObjectRequest;
import com.android.volley.toolbox.Volley;
import com.rpm.clynx.R;
import com.rpm.clynx.utility.ConversationsClientManager;
import com.rpm.clynx.utility.FileLogger;
import com.twilio.conversations.Attributes;
import com.twilio.conversations.CallbackListener;
import com.twilio.conversations.Conversation;
import com.twilio.conversations.ConversationsClient;
import com.twilio.util.ErrorInfo;
import com.twilio.conversations.Message;
import com.twilio.conversations.StatusListener;
import org.json.JSONException;
import org.json.JSONObject;
import java.time.Instant;
import java.time.LocalDateTime;
import java.time.ZoneId;
import java.time.ZonedDateTime;
import java.time.format.DateTimeFormatter;
import java.util.ArrayList;
import java.util.HashMap;
import java.util.HashSet;
import java.util.List;
import java.util.Map;
import java.util.Set;

public class MessageActivity extends AppCompatActivity implements QuickstartConversationsManagerListener {
    public final static String TAG = "TwilioConversations";
    private String memberusername = "";
    private MessagesAdapter messagesAdapter;
    private EditText writeMessageEditText;
    private final QuickstartConversationsManager quickstartConversationsManager = new QuickstartConversationsManager(this);
    SharedPreferences pref;
    SharedPreferences.Editor editor;
    private String Token, Username, UserId, ToUser, FriendlyUsername ;
    ProgressBar progressBarMsg;
    TextView chatMemberName;
    String CONVERSATION_SID = "";
    RecyclerView recyclerView;
    ImageButton scrollToLatestButton;
    int latestMessagePosition = -1; // Initialize it to an invalid value
    boolean isScrollingUp = false;
    TextView newMessageNotification;
    ImageView sendChatMessageButton;
    String conversationSid;
    private TextView typingIndicator;
    private static MessageActivity instance;
    private Handler activityStatusHandler = new Handler();
    private Runnable activityStatusRunnable;

    @SuppressLint({"MissingInflatedId", "SuspiciousIndentation"})
    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_chat);

        initPerformBackClick();
        pref = this.getSharedPreferences("RPMUserApp", this.MODE_PRIVATE);
        editor = pref.edit();
        Token = pref.getString("Token", null);
        quickstartConversationsManager.setListener(this);
        typingIndicator = findViewById(R.id.msgTypingIndicator);
        instance = this;
        Username = pref.getString("UserName", null);
        UserId = pref.getString("Patent_id", null);

        ToUser = getIntent().getStringExtra("memberUsername") != null ? getIntent().getStringExtra("memberUsername") : "";
        FriendlyUsername = getIntent().getStringExtra("FriendlyUsername") != null ? getIntent().getStringExtra("FriendlyUsername") : "";

        if (FriendlyUsername.contains("-")) {
            FriendlyUsername = FriendlyUsername.split("-")[0];  // Take the part before "-"
        }
        // Get the conversation SID from the intent
        conversationSid = getIntent().getStringExtra("conversationSid");
        memberusername = ToUser;
        messagesAdapter = new MessagesAdapter();

        Log.d("memberusername", memberusername);
        chatMemberName = findViewById(R.id.chatMemName);
        chatMemberName.setText(memberusername);

        Log.d("ToUservalue:--", ToUser);
        Log.d("FriendlyUsernamevalue:-", FriendlyUsername);

        FileLogger.d("ToUservalue:--", ToUser);
        FileLogger.d("FriendlyUsernamevalue:-", FriendlyUsername);

        getChatSid(FriendlyUsername, Token);

        recyclerView = findViewById(R.id.messagesRecyclerView);
        progressBarMsg = findViewById(R.id.progressMessage); // Replace with your progress indicator view ID
        LinearLayoutManager layoutManager = new LinearLayoutManager(this);

        // for a chat app, show latest messages at the bottom
        layoutManager.setStackFromEnd(true);
        recyclerView.setLayoutManager(layoutManager);
        recyclerView.setAdapter(messagesAdapter);
        writeMessageEditText = findViewById(R.id.writeMessageEditText);

        writeMessageEditText.setMovementMethod(new ScrollingMovementMethod());

        writeMessageEditText.addTextChangedListener(new TextWatcher() {
            @Override
            public void onTextChanged(CharSequence s, int start, int before, int count) {
                // Nothing
            }

            @Override
            public void beforeTextChanged(CharSequence s, int start, int count, int after) {
                // Nothing
            }

            @Override
            public void afterTextChanged(Editable s) {
                // Enable scrolling after 5 lines
                if (writeMessageEditText.getLineCount() > 4) {
                    writeMessageEditText.setVerticalScrollBarEnabled(true);
                    writeMessageEditText.setMovementMethod(new ScrollingMovementMethod());
                } else {
                    writeMessageEditText.setVerticalScrollBarEnabled(false);
                    writeMessageEditText.setMovementMethod(null);
                }
            }
        });


        sendChatMessageButton = findViewById(R.id.sendChatMessageButton);

        sendChatMessageButton.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View view) {
                String messageBody = writeMessageEditText.getText().toString();
                // Check if the message is empty
                if (messageBody.isEmpty()) {
                    // Show a Toast message if the input is empty
                    Toast.makeText(MessageActivity.this, "Message is empty. Please enter a message.", Toast.LENGTH_SHORT).show();
                } else {
                    Log.d("messageBody", messageBody);
                    FileLogger.d("messageBody", messageBody);


                    if (messageBody.length() > 0) {
                        // Case 1: When CONVERSATION_SID is null or empty, create and join a new conversation.
                        if (CONVERSATION_SID == null || CONVERSATION_SID.isEmpty()) {
                            Log.d("CONVERSATION_SID is null or empty", CONVERSATION_SID);
                            FileLogger.d("CONVERSATION_SID is null or empty", CONVERSATION_SID);
                            ConversationsClient convClientVal = ConversationsClientManager.getInstance().getConvClient();

                            // Create and join a new conversation, then send the message
                            quickstartConversationsManager.createAndJoinConversation(ToUser, convClientVal, MessageActivity.this, Token, messagesAdapter, messageBody, Token, UserId);

                        }
                        // Case 2: If CONVERSATION_SID is not null, check if we are already part of the conversation.
                        else {
                            Log.d("CONVERSATION_SID not null", CONVERSATION_SID);
                            FileLogger.d("CONVERSATION_SID not null", CONVERSATION_SID);

                            // Check if the conversation is already joined or the user is already participating.
                            if (quickstartConversationsManager.isConversationParticipating(CONVERSATION_SID)) {
                                Log.d("Already Participating", "Sending message to existing conversation.");
                                FileLogger.d("Already Participating", "Sending message to existing conversation.");
                                Log.d("ChatDebug", "Sending message. FromUser: " + UserId + ", ToUser: " + ToUser);
                                FileLogger.d("ChatDebug", "Sending message. FromUser: " + UserId + ", ToUser: " + ToUser);


                                // Send message to the existing conversation
                                quickstartConversationsManager.sendMessage(messageBody, MessageActivity.this, CONVERSATION_SID, Token, UserId, FriendlyUsername, messagesAdapter);
                            } else {
                                Log.d("Not Participating", "Joining conversation and sending message.");
                                FileLogger.d("Not Participating", "Joining conversation and sending message.");

                                // If not yet participating, create, join, and send the message.
                                ConversationsClient convClientVal = ConversationsClientManager.getInstance().getConvClient();
                                quickstartConversationsManager.createAndJoinConversation(ToUser, convClientVal, MessageActivity.this, Token, messagesAdapter, messageBody, Token, UserId);
                            }
                        }
                    }
                }
            }
        });

        newMessageNotification = findViewById(R.id.newMessageNotification);
        // Find the RecyclerView and the scroll-to-latest button
        scrollToLatestButton = findViewById(R.id.scrollToLatestButton);
        newMessageNotification.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View view) {
                // Handle the click event here, for example, scroll to the latest message.
                // You can replace the following line with your specific action.
                // Scroll the RecyclerView to the latest message
                Log.d("msgonClick","msgonClick");
                int latestMessagePosition = messagesAdapter.getItemCount() - 1;
                Log.d("msgonClicklatestPosition", String.valueOf(latestMessagePosition));
                recyclerView.scrollToPosition(latestMessagePosition);
                newMessageNotification.setVisibility(View.GONE);
            }
        });

        recyclerView.addOnScrollListener(new RecyclerView.OnScrollListener() {
            @Override
            public void onScrolled(@NonNull RecyclerView recyclerView, int dx, int dy) {
                super.onScrolled(recyclerView, dx, dy);

                LinearLayoutManager layoutManager = (LinearLayoutManager) recyclerView.getLayoutManager();
                if (layoutManager != null) {
                    int lastVisibleItemPosition = layoutManager.findLastVisibleItemPosition();
                    int totalItemCount = recyclerView.getAdapter().getItemCount();
                    Log.d("RecyclerView", "Last visible item: " + lastVisibleItemPosition + ", Total items: " + totalItemCount);
                    // Check if at the last item
                    if (lastVisibleItemPosition == totalItemCount - 1) {
                        newMessageNotification.setVisibility(View.GONE);
                        scrollToLatestButton.setVisibility(View.GONE);
                        isScrollingUp = false;
                        Log.d("RecyclerView", "At the bottom, hiding button");
                    } else {
                        // Detect scrolling direction
                        if (dy < 0) {  // Scrolling up
                            if (!isScrollingUp) {
                                isScrollingUp = true;
                                scrollToLatestButton.setVisibility(View.VISIBLE);
                                Log.d("RecyclerView", "Scrolling up, showing button");
                            }
                        } else if (dy > 0) { // Scrolling down
                            if (isScrollingUp) {
                                isScrollingUp = false;
                                scrollToLatestButton.setVisibility(View.GONE);
                                Log.d("RecyclerView", "Scrolling down, hiding button");
                            }
                        }
                    }
                }
            }
        });

        scrollToLatestButton.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                Log.d("ScrollButton", "Scroll button clicked");
                int latestMessagePosition = recyclerView.getAdapter().getItemCount() - 1;
                Log.d("ScrollButton", "Latest message position: " + latestMessagePosition);

                recyclerView.post(new Runnable() {
                    @Override
                    public void run() {
                        Log.d("ScrollButton", "Scrolling to position: " + latestMessagePosition);
                        recyclerView.smoothScrollToPosition(latestMessagePosition);
                    }
                });

                isScrollingUp = false;
                scrollToLatestButton.setVisibility(View.GONE);
                Log.d("ScrollButton", "Button hidden after scrolling");
            }
        });

        Handler handler = new Handler();
        handler.postDelayed(new Runnable() {
            @Override
            public void run() {
                newMessageNotification.setVisibility(View.GONE);
            }
        }, 3000); // 3000 milliseconds (3 seconds)
    }

    @Override
    protected void onPause() {
        super.onPause();
        if (activityStatusHandler != null && activityStatusRunnable != null) {
            activityStatusHandler.removeCallbacks(activityStatusRunnable);
        }
    }
    // Method to update CONVERSATION_SID
    public void updateConversationSid(String conversationSid) {
        this.CONVERSATION_SID = conversationSid;
        Log.d("Updated CONVERSATION_SID", conversationSid);

        // Optionally, update the UI or perform actions based on the new SID
        // For example, you can enable sending messages now that SID is available
    }
    public static MessageActivity getInstance() {
        return instance;
    }

    public void deleteMessageFromList(String messageSid) {
        if (messagesAdapter != null) {
            messagesAdapter.removeMessage(messageSid);
        }
    }

    public void refreshMessageInAdapter(Message message) {
        if (messagesAdapter != null) {
            messagesAdapter.updateMessage(message);
        }
    }
    public void showTypingIndicator(String participantName) {
        runOnUiThread(() -> {
            Log.d("typingIndicator", "typingIndicator" );
            typingIndicator.setVisibility(View.VISIBLE);
            typingIndicator.setText( " is typing...");
        });
    }

    public void hideTypingIndicator() {
        runOnUiThread(() -> typingIndicator.setVisibility(View.GONE));
    }

    private void getChatSid(String ToUser, String tkn) {
        Log.d("ToUser", ToUser);
        Log.d("getChatSid api call", "getChatSid api call");
        String CHAT_SID_URL = BASE_URL + CHAT_SID + ToUser;
        Log.d("CHAT_SID_URL", CHAT_SID_URL);

        JsonObjectRequest jsonObjectRequest = new JsonObjectRequest(Request.Method.GET, CHAT_SID_URL,
                null,  // No parameters
                new Response.Listener<JSONObject>() {
                    @Override
                    public void onResponse(JSONObject response) {
                        Log.d("CHAT_SID_URL inner", CHAT_SID_URL);
                        try {
                            String getchatsid = response.getString("message");
                            String stringWithoutQuotes = getchatsid.replace("\"", "");
                            Log.d("CONNECT stringWithoutQuotes chat sid", stringWithoutQuotes);
                            FileLogger.d("CONNECT stringWithoutQuotes chat sid", stringWithoutQuotes);
                            CONVERSATION_SID = stringWithoutQuotes;
                            // Call to fetch the conversation using the new CONVERSATION_SID
                            if (CONVERSATION_SID != null && !CONVERSATION_SID.isEmpty()) {

                                quickstartConversationsManager.getConv(messagesAdapter, CONVERSATION_SID, ToUser, MessageActivity.this, Token, progressBarMsg);
                                // Start periodic user activity update
                                startUserActivityUpdate(); // Add this
                            } else {
                                Log.e("getChatSid", "Invalid conversation SID returned.");
                            }

                        } catch (JSONException e) {
                            throw new RuntimeException(e);
                        }
                    }
                },
                new Response.ErrorListener() {
                    @RequiresApi(api = Build.VERSION_CODES.Q)
                    @Override
                    public void onErrorResponse(VolleyError error) {
                        Log.e("getChatSid error", error.toString());
                        FileLogger.d("getChatSid error", error.toString());
                        // You may want to show a Toast or Snackbar to inform the user about the error
                        progressBarMsg.setVisibility(View.GONE);
                        Toast.makeText(MessageActivity.this, "Send Message to start Conversation.", Toast.LENGTH_SHORT).show();
                    }
                }) {
            @Override
            public String getBodyContentType() {
                return "application/json";
            }

            @Override
            public Map<String, String> getHeaders() throws AuthFailureError {
                Map<String, String> headers = new HashMap<>();
                headers.put("Bearer", tkn);
                Log.d("Token_chat", tkn);
                return headers;
            }
        };
        // Initialize the request queue and add the jsonObjectRequest to it
        RequestQueue requestQueue = Volley.newRequestQueue(MessageActivity.this);
        // Set the retry policy
        jsonObjectRequest.setRetryPolicy(new DefaultRetryPolicy(0, -1, DefaultRetryPolicy.DEFAULT_BACKOFF_MULT));
        // Add the request to the queue
        requestQueue.add(jsonObjectRequest);
    }

    private void startUserActivityUpdate() {
        activityStatusRunnable = new Runnable() {
            @Override
            public void run() {
                String utcTime = null;
                if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.O) {
                    utcTime = getCurrentLocalTimeWithOffset(); // Or use your local time formatting method
                }
                sendUserActivityStatus(CONVERSATION_SID, UserId, utcTime, Token, MessageActivity.this);
                // Repeat every 15 seconds
                activityStatusHandler.postDelayed(this, 10000);
            }
        };
        // Start the first call
        activityStatusHandler.post(activityStatusRunnable);
    }

    @RequiresApi(api = Build.VERSION_CODES.O)
    public String getCurrentLocalTimeWithOffset() {
        ZonedDateTime now = ZonedDateTime.now(); // Gets the current local time with zone
        DateTimeFormatter formatter = DateTimeFormatter.ofPattern("M/d/yyyy, h:mm:ss a XXX");
        return now.format(formatter);
    }

    private void sendUserActivityStatus(String conversationSid, String userName, String lastActiveAt, String token, Context context) {
        String API_URL = BASE_URL + chatheartbeat_URL;  // Replace with your actual endpoint

        JSONObject jsonBody = new JSONObject();
        try {
            jsonBody.put("ConversationSid", conversationSid);
            jsonBody.put("UserName", userName);
            jsonBody.put("LastActiveAt", lastActiveAt);
            Log.d("jsonBodychatheartbeat",  jsonBody.toString());
        } catch (JSONException e) {
            e.printStackTrace();
            return;
        }

        JsonObjectRequest jsonObjectRequest = new JsonObjectRequest(
                Request.Method.POST,
                API_URL,
                jsonBody,
                response -> {
                    Log.d("sendUserActivityStatus", "Response: " + response.toString());
                    // Handle success
                },
                error -> {
                    Log.e("sendUserActivityStatus", "Error: " + error.toString());
                    // Toast.makeText(MessageActivity.this, "Failed to send activity status.", Toast.LENGTH_SHORT).show();
                }
        ) {
            @Override
            public Map<String, String> getHeaders() throws AuthFailureError {
                Map<String, String> headers = new HashMap<>();
                headers.put("Bearer", token);
                return headers;
            }
        };

        // Add request to queue
        RequestQueue requestQueue = Volley.newRequestQueue(context);
        jsonObjectRequest.setRetryPolicy(new DefaultRetryPolicy(0, -1, DefaultRetryPolicy.DEFAULT_BACKOFF_MULT));
        requestQueue.add(jsonObjectRequest);
    }

    private void initPerformBackClick() {
        Toolbar toolbar = findViewById(R.id.toolbar);
        toolbar.setNavigationIcon(R.drawable.ic_baseline_west_24);
        toolbar.setNavigationOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                quickstartConversationsManager.setLastReadIndex(CONVERSATION_SID, MessageActivity.this);

                finish();
            }
        });
    }

    @Override
    public void receivedNewMessage( boolean equals) {
        runOnUiThread(new Runnable() {
            @Override
            public void run() {
                Log.d("receivedNewMessage", "receivedNewMessage");
                Log.d("messagesAdapterCount", String.valueOf(messagesAdapter.getItemCount()));
                Log.d("equals", String.valueOf(equals));

                if (equals) {
                    newMessageNotification.setVisibility(View.GONE);
                    // Update the latest message position
                    latestMessagePosition = messagesAdapter.getItemCount() - 1;
                    Log.d("latestMessagePosition", String.valueOf(latestMessagePosition));
                    // Scroll the RecyclerView to the latest message
                    int latestMessagePosition = messagesAdapter.getItemCount() - 1;
                    recyclerView.scrollToPosition(latestMessagePosition);
                    scrollToLatestButton.setVisibility(View.GONE);
                    messagesAdapter.notifyDataSetChanged();
                }
                else
                {
                    Log.d("latestMsgposition1", String.valueOf(latestMessagePosition));
                    if (messagesAdapter.getItemCount() > 0) {
                        latestMessagePosition = messagesAdapter.getItemCount() - 1;
                        Log.d("latestMsgposition2", String.valueOf(latestMessagePosition));
                        newMessageNotification.setVisibility(View.VISIBLE);
                    } else {
                        Log.d("Fix", "No messages in adapter yet, skipping notification.");
                    }
                    messagesAdapter.notifyDataSetChanged();
                }
            }
        });
    }

    @Override
    public void reloadMessages() {
        runOnUiThread(new Runnable() {
            @Override
            public void run() {
                // need to modify user interface elements on the UI thread
                messagesAdapter.notifyDataSetChanged();
                Log.d("reloadMessages chat acti", "reloadMessages");
                progressBarMsg.setVisibility(View.GONE);
            }
        });
    }

    @Override
    public void messageSentCallback() {
        Log.d("messageSentCallback", "messageSentCallback");
        MessageActivity.this.runOnUiThread(new Runnable() {
            @Override
            public void run() {
                // need to modify user interface elements on the UI thread
                writeMessageEditText.setText("");
            }
        });
    }

    class MessagesAdapter extends RecyclerView.Adapter<MessagesAdapter.ViewHolder> {

        private static final int VIEW_TYPE_INCOMING_MESSAGE = 1;
        private static final int VIEW_TYPE_OUTGOING_MESSAGE = 2;
        private static final int VIEW_TYPE_DATE_HEADER = 3;
        private static final int VIEW_TYPE_DATE_HEADER_WITH_OUTGOING_MESSAGE = 4;
        private static final int VIEW_TYPE_DATE_HEADER_WITH_INCOMING_MESSAGE = 5;
        private Set<String> editedMessageIds = new HashSet<>();

        private List<Message> messagesS;
        private List<String> dates;

        private int expandedPosition = -1;

        MessagesAdapter() {
            messagesS = new ArrayList<>();
            // dates = new ArrayList<>();
            // Check if the message was edited (via attributes)
            loadEditedMessageIds();
        }

        // Add a new method to update the adapter data
        public void setMessages(List<Message> messages) {
            Log.d("SETmessages111", String.valueOf(messages));
            dates = new ArrayList<>();
            this.messagesS = messages;
        }

        @Override
        public ViewHolder onCreateViewHolder(ViewGroup parent, int viewType) {
            LayoutInflater inflater = LayoutInflater.from(parent.getContext());
            View itemView;

            if (viewType == VIEW_TYPE_DATE_HEADER_WITH_OUTGOING_MESSAGE) {
                itemView = inflater.inflate(R.layout.item_date_header_with_outgoing_msg, parent, false);
            } else if (viewType == VIEW_TYPE_DATE_HEADER_WITH_INCOMING_MESSAGE) {
                itemView = inflater.inflate(R.layout.item_date_header_with_incoming_msg, parent, false);
            } else if (viewType == VIEW_TYPE_DATE_HEADER) {
                itemView = inflater.inflate(R.layout.item_date_header, parent, false);
            } else if (viewType == VIEW_TYPE_OUTGOING_MESSAGE) {
                itemView = inflater.inflate(R.layout.item_outgoing_msg, parent, false);
            } else if (viewType == VIEW_TYPE_INCOMING_MESSAGE) {
                itemView = inflater.inflate(R.layout.item_incoming_msg, parent, false);
            } else {
                throw new IllegalArgumentException("Invalid view type: " + viewType);
            }

            return new ViewHolder(itemView, viewType);
        }

        @Override
        public void onBindViewHolder(ViewHolder holder, int position) {
            int viewType = getItemViewType(position);

            if (viewType == VIEW_TYPE_DATE_HEADER) {
                // Handle Date Header binding
                String date = null;
                if (android.os.Build.VERSION.SDK_INT >= android.os.Build.VERSION_CODES.O) {
                    date = convertUTCToLocalDate(quickstartConversationsManager.getMessages().get(position).getDateUpdated());
                }
                holder.bindDateHeader(date);
            } else if (viewType == VIEW_TYPE_INCOMING_MESSAGE) {

                // Handle incoming message binding
                Message message = quickstartConversationsManager.getMessages().get(position);
                // String messageText = String.format("%s: %s", message.getAuthor(), message.getMessageBody());
                String messageText = String.format("%s", message.getBody());
                String localTime = null;
                if (android.os.Build.VERSION.SDK_INT >= android.os.Build.VERSION_CODES.O) {
                    localTime = getTimeFromUTC(message.getDateUpdated());
                }
                holder.incomingMessageText.setText(messageText);
                holder.incomingTime.setText(localTime);
            } else if (viewType == VIEW_TYPE_OUTGOING_MESSAGE) {
                // Handle outgoing message binding
                Message message = quickstartConversationsManager.getMessages().get(position);
                String messageText = String.format("%s", message.getBody());
                String localTime = null;
                if (android.os.Build.VERSION.SDK_INT >= android.os.Build.VERSION_CODES.O) {
                    localTime = getTimeFromUTC(message.getDateUpdated());
                }
                holder.outgoingMessageText.setText(messageText);
                holder.outgoingTime.setText(localTime);

                // Handle the "Edited" label
                try {
                    JSONObject attributesJson = new JSONObject(message.getAttributes().toString());
                    boolean isEdited = attributesJson.optBoolean("edited", false);
                    if (isEdited) {
                        Log.d("isEdited", String.valueOf(isEdited));
                        holder.editedLabel.setVisibility(View.VISIBLE); // Show "Edited"
                    } else {
                        Log.d("isEdited", String.valueOf(isEdited));
                        holder.editedLabel.setVisibility(View.GONE); // Hide "Edited"
                    }
                } catch (JSONException e) {
                    Log.e("MessageAttributes", "Error parsing attributes: " + e.getMessage());
                    holder.editedLabel.setVisibility(View.GONE); // Default to hide "Edited" on error
                }

                // ADD THIS BLOCK HERE to control visibility
                if (holder.getAdapterPosition() == expandedPosition) {
                    holder.editDeleteContainer.setVisibility(View.VISIBLE);
                } else {
                    holder.editDeleteContainer.setVisibility(View.GONE);
                }


                holder.outgoingMessageLayout.setOnClickListener(new View.OnClickListener() {
                    @Override
                    public void onClick(View v) {
                        int adapterPosition = holder.getAdapterPosition();
                        if (adapterPosition == RecyclerView.NO_POSITION) return;

                        if (expandedPosition == adapterPosition) {
                            expandedPosition = -1;
                        } else {
                            expandedPosition = adapterPosition;
                        }
                        notifyDataSetChanged();

//                        if (holder.editDeleteContainer.getVisibility() == View.VISIBLE) {
//                            holder.editDeleteContainer.setVisibility(View.GONE);
//                        } else {
//                            holder.editDeleteContainer.setVisibility(View.VISIBLE);
//                        }
                    }
                });

                holder.editMessageButton.setOnClickListener(v -> editMessage(position, message));
                holder.deleteMessageButton.setOnClickListener(v -> deleteMessageFromConversation(position, message));

            } else if (viewType == VIEW_TYPE_DATE_HEADER_WITH_OUTGOING_MESSAGE) {
                // Handle Date Header with outgoing message binding
                String date = null;
                if (android.os.Build.VERSION.SDK_INT >= android.os.Build.VERSION_CODES.O) {
                    date = convertUTCToLocalDate(quickstartConversationsManager.getMessages().get(position).getDateUpdated());
                }
                holder.bindDateHeader(date);

                Message outgoingMessage = quickstartConversationsManager.getMessages().get(position);
                String messageText = String.format("%s", outgoingMessage.getBody());
                String localTime = null;
                if (android.os.Build.VERSION.SDK_INT >= android.os.Build.VERSION_CODES.O) {
                    localTime = getTimeFromUTC(outgoingMessage.getDateUpdated());
                }
                holder.outgoingMessageText.setText(messageText);
                holder.outgoingTime.setText(localTime);

                // ADD THIS BLOCK HERE to control visibility
                if (holder.getAdapterPosition() == expandedPosition) {
                    holder.editDeleteContainer.setVisibility(View.VISIBLE);
                } else {
                    holder.editDeleteContainer.setVisibility(View.GONE);
                }


                holder.outgoingMessageLayout.setOnClickListener(new View.OnClickListener() {
                    @Override
                    public void onClick(View v) {
                        int adapterPosition = holder.getAdapterPosition();
                        if (adapterPosition == RecyclerView.NO_POSITION) return;

                        if (expandedPosition == adapterPosition) {
                            expandedPosition = -1;
                        } else {
                            expandedPosition = adapterPosition;
                        }
                        notifyDataSetChanged();

//                        if (holder.editDeleteContainer.getVisibility() == View.VISIBLE) {
//                            holder.editDeleteContainer.setVisibility(View.GONE);
//                        } else {
//                            holder.editDeleteContainer.setVisibility(View.VISIBLE);
//                        }
                    }
                });

                holder.editMessageDateButton.setOnClickListener(v -> editMessage(position, outgoingMessage));
                holder.deleteMessageDateButton.setOnClickListener(v -> deleteMessageFromConversation(position, outgoingMessage));

            } else if (viewType == VIEW_TYPE_DATE_HEADER_WITH_INCOMING_MESSAGE) {
                // Handle Date Header with incoming message binding
                String date = null;
                if (android.os.Build.VERSION.SDK_INT >= android.os.Build.VERSION_CODES.O) {
                    date = convertUTCToLocalDate(quickstartConversationsManager.getMessages().get(position).getDateUpdated());
                }
                holder.bindDateHeader(date);

                Message incomingMessage = quickstartConversationsManager.getMessages().get(position);
                String messageText = String.format("%s", incomingMessage.getBody());
                String localTime = null;
                if (android.os.Build.VERSION.SDK_INT >= android.os.Build.VERSION_CODES.O) {
                    localTime = getTimeFromUTC(incomingMessage.getDateUpdated());
                }
                holder.incomingMessageText.setText(messageText);
                holder.incomingTime.setText(localTime);
            }
        }

        private void loadEditedMessageIds() {
            SharedPreferences prefs =  MessageActivity.this.getSharedPreferences("EditedMessages", Context.MODE_PRIVATE);
            editedMessageIds = prefs.getStringSet("edited_ids", new HashSet<>());
        }
        @Override
        public int getItemCount() {
            Log.d("quickstartConversationsManager.getMessages().size()", String.valueOf(quickstartConversationsManager.getMessages().size()));
            return quickstartConversationsManager.getMessages().size();
        }

        @Override
        public int getItemViewType(int position) {
            if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.O) {
                if (position == 0) {
                    Message firstMessage = messagesS.get(position);
                    String capitalizedUsername = Username.toUpperCase();

                    if (firstMessage.getAuthor().equals(capitalizedUsername)) {
                        return VIEW_TYPE_DATE_HEADER_WITH_OUTGOING_MESSAGE; // Display date header with outgoing message
                    } else {
                        return VIEW_TYPE_DATE_HEADER_WITH_INCOMING_MESSAGE; // Display date header with incoming message
                    }
                }

                Message currentMessage = messagesS.get(position);
                Message prevMessage = messagesS.get(position - 1);

                String currentDate = convertUTCToLocalDate(currentMessage.getDateUpdated());
                String prevDate = convertUTCToLocalDate(prevMessage.getDateUpdated());

                String capitalizedUsername = Username.toUpperCase();

                if (!currentDate.equals(prevDate)) {
                    if (currentMessage.getAuthor().equals(capitalizedUsername)) {
                        return VIEW_TYPE_DATE_HEADER_WITH_OUTGOING_MESSAGE; // Display date header with outgoing message
                    } else {
                        return VIEW_TYPE_DATE_HEADER_WITH_INCOMING_MESSAGE; // Display date header with incoming message
                    }
                } else if (currentMessage.getAuthor().equals(capitalizedUsername)) {
                    return VIEW_TYPE_OUTGOING_MESSAGE; // Display outgoing message
                } else {
                    return VIEW_TYPE_INCOMING_MESSAGE; // Display incoming message
                }
            }

            return VIEW_TYPE_INCOMING_MESSAGE; // Default type
        }

        class ViewHolder extends RecyclerView.ViewHolder {

            TextView incomingMessageText;
            TextView outgoingMessageText;
            TextView incomingTime;
            TextView outgoingTime;
            TextView dateTextView;
            TextView editedLabel;
            ImageButton editMessageButton, deleteMessageButton, editMessageDateButton, deleteMessageDateButton;
            RelativeLayout outgoingMessageLayout;
            LinearLayout editDeleteContainer;
            ViewHolder(View itemView, int viewType) {
                super(itemView);

                if (viewType == VIEW_TYPE_INCOMING_MESSAGE) {
                    incomingMessageText = itemView.findViewById(R.id.incomingMessageText);
                    incomingTime = itemView.findViewById(R.id.incomingTime);
                    outgoingMessageText = null;
                    outgoingTime = null;
                    dateTextView = null;
                } else if (viewType == VIEW_TYPE_OUTGOING_MESSAGE) {
                    outgoingMessageText = itemView.findViewById(R.id.outgoingMessageText);
                    outgoingTime = itemView.findViewById(R.id.outgoingTime);
                    incomingMessageText = null;
                    incomingTime = null;
                    dateTextView = null;
                    editedLabel = itemView.findViewById(R.id.editedLabel);
                    editMessageButton = itemView.findViewById(R.id.editMessageButton);
                    deleteMessageButton = itemView.findViewById(R.id.deleteMessageButton);
                    outgoingMessageLayout = itemView.findViewById(R.id.outgoingLayout);
                    editDeleteContainer = itemView.findViewById(R.id.buttonContainer);
                } else if (viewType == VIEW_TYPE_DATE_HEADER) {
                    dateTextView = itemView.findViewById(R.id.dateTextView);
                    incomingTime = null;
                    outgoingTime = null;
                    incomingMessageText = null;
                    outgoingMessageText = null;
                } else if (viewType == VIEW_TYPE_DATE_HEADER_WITH_OUTGOING_MESSAGE) {
                    dateTextView = itemView.findViewById(R.id.dateTextView);
                    incomingTime = itemView.findViewById(R.id.incomingTime);
                    outgoingTime = itemView.findViewById(R.id.outgoingTime);
                    incomingMessageText = itemView.findViewById(R.id.incomingMessageText);
                    outgoingMessageText = itemView.findViewById(R.id.outgoingMessageText);
                    editMessageDateButton = itemView.findViewById(R.id.editMessageDateButton);
                    deleteMessageDateButton = itemView.findViewById(R.id.deleteMessageDateButton);
                    outgoingMessageLayout = itemView.findViewById(R.id.outgoingLayout);
                    editDeleteContainer = itemView.findViewById(R.id.buttonContainer);
                }
                else if (
                        viewType == VIEW_TYPE_DATE_HEADER_WITH_INCOMING_MESSAGE) {
                    dateTextView = itemView.findViewById(R.id.dateTextView);
                    incomingTime = itemView.findViewById(R.id.incomingTime);
                    outgoingTime = itemView.findViewById(R.id.outgoingTime);
                    incomingMessageText = itemView.findViewById(R.id.incomingMessageText);
                    outgoingMessageText = itemView.findViewById(R.id.outgoingMessageText);
                }
                Log.d("ViewHolder", "ViewHolder");
            }

            void bindDateHeader(String dateHeader) {
                if (dateTextView != null) {
                    dateTextView.setText(dateHeader);
                }
            }
        }

        private void editMessage(int position, Message message) {
            AlertDialog.Builder builder = new AlertDialog.Builder(MessageActivity.this);
            builder.setTitle("Edit Message");

            final EditText input = new EditText(MessageActivity.this);
            input.setText(message.getBody());

            builder.setView(input);

            builder.setPositiveButton("Save", (dialog, which) -> {
                String editedText = input.getText().toString().trim();

                // Check if the text is empty or just whitespace
                if (editedText.isEmpty()) {
                    input.setError("Message cannot be empty or just spaces");  // Show error
                    return;  // Don't proceed with saving, just show the error
                }
              //  if (!editedText.isEmpty()) {

                    message.updateBody(editedText, new StatusListener() {
                        @Override
                        public void onSuccess() {
                            Log.d("Twilio", "Message updated successfully");
                            markMessageAsEdited(message);
                            // Fetch the latest message object from the conversation
                            runOnUiThread(() -> {
                                messagesS.set(position, message); // This ensures the list has the updated message
                                notifyItemChanged(position); // Refresh UI for the edited message
                                Toast.makeText(MessageActivity.this, "Message edited", Toast.LENGTH_SHORT).show();
                            });
                        }

                        @Override
                        public void onError(ErrorInfo errorInfo) {
                            Log.e("Twilio", "Failed to update message: " + errorInfo.getMessage());
                            runOnUiThread(() ->
                                    Toast.makeText(MessageActivity.this, "Failed to edit message", Toast.LENGTH_SHORT).show()
                            );
                        }
                    });
              //  }
            });

            builder.setNegativeButton("Cancel", (dialog, which) -> dialog.dismiss());
            builder.show();
        }

        private void markMessageAsEdited(Message message) {
            if (message != null) {
                try {
                    Attributes attributes = new Attributes("{\"edited\": true}"); // Provide JSON as a string
                    message.setAttributes(attributes, new StatusListener() {
                        @Override
                        public void onSuccess() {
                            Log.d("Twilio", "Attributes updated successfully");
                        }

                        @Override
                        public void onError(ErrorInfo errorInfo) {
                            Log.e("Twilio", "Failed to update attributes: " + errorInfo.getMessage());
                        }
                    });
                } catch (Exception e) {
                    Log.e("AttributesError", "Error creating Attributes object: " + e.getMessage());
                }
            }
        }

        public void updateMessage(Message updatedMessage) {
            int position = -1;
            for (int i = 0; i < messagesS.size(); i++) {
                if (messagesS.get(i).getSid().equals(updatedMessage.getSid())) {
                    position = i;
                    break;
                }
            }

            if (position != -1) {
                messagesS.set(position, updatedMessage); // Replace the old message with the updated one
                messagesAdapter.notifyItemChanged(position); // Notify RecyclerView to update UI
            }

        }

        public void removeMessage(String messageSid) {
            int position = -1;
            for (int i = 0; i < messagesS.size(); i++) {
                if (messagesS.get(i).getSid().equals(messageSid)) {
                    position = i;
                    break;
                }
            }

            if (position != -1) {
                messagesS.remove(position);
                notifyItemRemoved(position);
            }
        }


        private void deleteMessageFromConversation(int position, Message message) {
            AlertDialog.Builder builder = new AlertDialog.Builder(MessageActivity.this);
            builder.setTitle("Delete Message");
            builder.setMessage("Are you sure you want to delete this message?");

            builder.setPositiveButton("Yes", (dialog, which) -> {
                ConversationsClient convClient = ConversationsClientManager.getInstance().getConvClient();
                convClient.getConversation(CONVERSATION_SID, new CallbackListener<Conversation>() {
                    @Override
                    public void onSuccess(Conversation conversation) {
                        conversation.getMessageByIndex(message.getMessageIndex(), new CallbackListener<Message>() {
                            @Override
                            public void onSuccess(Message message) {
                                conversation.removeMessage(message, new StatusListener() {
                                    @Override
                                    public void onSuccess() {
                                        Log.d("Twilio", "Message deleted successfully");

                                        runOnUiThread(() -> {
                                            int actualPosition = messagesS.indexOf(message);
                                            if (actualPosition != -1) {
                                                messagesS.remove(actualPosition);
                                                notifyItemRemoved(actualPosition);
                                                Log.d("Twilio", "Message removed at corrected position: " + actualPosition);
                                            } else {
                                                Log.e("Twilio", "Message not found in messagesS, skipping removal.");
                                            }
                                        });
                                    }

                                    @Override
                                    public void onError(ErrorInfo errorInfo) {
                                        Log.e("Twilio", "Error deleting message: " + errorInfo.getMessage());
                                    }
                                });
                            }

                            @Override
                            public void onError(ErrorInfo errorInfo) {
                                Log.e("Twilio", "Error fetching message: " + errorInfo.getMessage());
                            }
                        });
                    }

                    @Override
                    public void onError(ErrorInfo errorInfo) {
                        Log.e("Twilio", "Error fetching conversation: " + errorInfo.getMessage());
                    }
                });
            });

            builder.setNegativeButton("No", (dialog, which) -> dialog.dismiss());
            builder.show();
        }

        @RequiresApi(api = Build.VERSION_CODES.O)
        public String convertUTCToLocalDate(String utcDateString) {
            Instant instant = Instant.parse(utcDateString);
            LocalDateTime localDateTime = instant.atZone(ZoneId.systemDefault()).toLocalDateTime();
            DateTimeFormatter formatter = DateTimeFormatter.ofPattern("MMM dd, yyyy");
            return localDateTime.format(formatter);
        }

        @RequiresApi(api = Build.VERSION_CODES.O)
        public String getTimeFromUTC(String utcDateString) {
            Instant instant = Instant.parse(utcDateString);
            LocalDateTime localDateTime = instant.atZone(ZoneId.systemDefault()).toLocalDateTime();
            DateTimeFormatter formatter = DateTimeFormatter.ofPattern("hh:mm a");
            return localDateTime.format(formatter);
        }
    }
}