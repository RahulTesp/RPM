package com.rpm.clynx.fragments;

import android.annotation.SuppressLint;
import android.content.BroadcastReceiver;
import android.content.Context;
import android.content.Intent;
import android.content.IntentFilter;
import android.content.SharedPreferences;
import android.graphics.Color;
import android.net.Uri;
import android.os.Build;
import android.os.Bundle;
import android.os.Handler;
import android.util.DisplayMetrics;
import android.util.Log;
import android.util.TypedValue;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.ImageView;
import android.widget.LinearLayout;
import android.widget.TextView;
import android.widget.Toast;
import androidx.annotation.RequiresApi;
import androidx.cardview.widget.CardView;
import androidx.core.content.ContextCompat;
import androidx.fragment.app.Fragment;
import androidx.fragment.app.FragmentTransaction;
import androidx.lifecycle.ViewModelProvider;
import androidx.recyclerview.widget.LinearLayoutManager;
import androidx.recyclerview.widget.RecyclerView;
import com.android.volley.AuthFailureError;
import com.android.volley.DefaultRetryPolicy;
import com.android.volley.Request;
import com.android.volley.RequestQueue;
import com.android.volley.Response;
import com.android.volley.VolleyError;
import com.android.volley.toolbox.StringRequest;
import com.android.volley.toolbox.Volley;
import com.github.mikephil.charting.charts.LineChart;
import com.github.mikephil.charting.components.Legend;
import com.github.mikephil.charting.components.XAxis;
import com.github.mikephil.charting.components.YAxis;
import com.github.mikephil.charting.data.Entry;
import com.github.mikephil.charting.data.LineData;
import com.github.mikephil.charting.data.LineDataSet;
import com.github.mikephil.charting.formatter.IndexAxisValueFormatter;
import com.github.mikephil.charting.interfaces.datasets.ILineDataSet;
import com.rpm.clynx.adapter.ToDoListHomeAdapter;
import com.rpm.clynx.model.ToDoListModel;
import com.rpm.clynx.service.NotificationReceiver;
import com.rpm.clynx.service.TimeFormatter;
import com.rpm.clynx.service.UnreadMessageViewModel;
import com.rpm.clynx.utility.ConversationsClientManager;
import com.rpm.clynx.utility.CustomMarkerView;
import com.rpm.clynx.utility.DashboardViewModel;
import com.rpm.clynx.utility.DataBaseHelper;
import com.rpm.clynx.utility.DateUtils;
import com.rpm.clynx.utility.FileLogger;
import com.rpm.clynx.utility.Links;
import com.rpm.clynx.utility.Loader;
import com.rpm.clynx.R;
import org.java_websocket.client.WebSocketClient;
import org.java_websocket.handshake.ServerHandshake;
import org.json.JSONArray;
import org.json.JSONException;
import org.json.JSONObject;
import java.net.URI;
import java.net.URISyntaxException;
import java.text.ParseException;
import java.time.LocalDate;
import java.time.format.DateTimeFormatter;
import java.util.ArrayList;
import java.util.Calendar;
import java.util.Date;
import java.util.HashMap;
import java.util.List;
import java.util.Map;
import java.util.TimeZone;
import java.text.SimpleDateFormat;
import com.rpm.clynx.utility.MyMarkerView;
import com.rpm.clynx.utility.NetworkAlert;
import com.rpm.clynx.utility.TimeAgoUtils;
import com.twilio.conversations.CallbackListener;
import com.twilio.conversations.Conversation;
import com.twilio.conversations.ConversationListener;
import com.twilio.conversations.ConversationsClient;
import com.twilio.util.ErrorInfo;
import com.twilio.conversations.Message;
import com.twilio.conversations.Participant;

public class DashboardFragment extends Fragment {
    DataBaseHelper db;
    SharedPreferences pref;
    SharedPreferences.Editor editor;
    View view;
    TextView recentvtls,textHighlight7,emptyChart,user_full_name,patent_id, tv_prog_name, tv_status, tv_date,tv_notification_count;
    String ProgramName,ProgramTypeName;
    ImageView ivNotification, call ,chat;
    RecyclerView recyclerView_todolist;
    private List<ToDoListModel> toDoListModels;
    private ToDoListHomeAdapter adapter;
    RecyclerView.LayoutManager layoutManager;
    TextView todo_nofoactivites;
    private String Token;
    TextView tv_vital_header,tv_vital_header_label, tv_vital_value, tv_vital_unit, tv_vital_time, tv_pulse_value,tv_pulse_unit,
            chatMessageCount;
    private WebSocketClient mWebSocketClient;
    private URI uri;
    int textSizeInSP = 24; // Set your desired text size in sp
    LineChart mpLinechart7;
    private LinearLayout chartContainer;
    private String identity = "123401130";
    public final static String TAG = "TwilioConversations";
    private final Handler handler = new Handler();
    private final int RETRY_INTERVAL = 3000; // 3 seconds
    MyMarkerView markerView;
    private DashboardViewModel dashboardViewModel;
    private Map<String, Integer> unreadCounts = new HashMap<>();

    public static boolean DashboardFragmentIsVisible = false;

    private BroadcastReceiver notificationReceiver = new BroadcastReceiver() {
        @Override
        public void onReceive(Context context, Intent intent) {
            // This method is called when notification/broadcast is received
            //  Call your API here
            Log.d("getNotificationsCount", "getNotificationsCount");
            getNotificationsCount(); // (your API method)
        }
    };


    @RequiresApi(api = Build.VERSION_CODES.O)
    @Override
    public void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        dashboardViewModel = new ViewModelProvider(this).get(DashboardViewModel.class);
    }

    @Override
    public void onStart() {
        super.onStart();
        Log.d("onStart", "Setting up Twilio Listeners...");
        setupTwilioListeners();

        IntentFilter filter = new IntentFilter();
        filter.addAction(NotificationReceiver.ACTION_NOTIFICATION_RECEIVED); // Use your app's broadcast action
        ContextCompat.registerReceiver(requireContext(), notificationReceiver, filter, ContextCompat.RECEIVER_NOT_EXPORTED);
    }

    @Override
    public void onResume() {
        super.onResume();
        DashboardFragmentIsVisible = true;
        getNotificationsCount();
        if (pref.getBoolean("loginstatus", false) == false){
            Log.d("loginstsfrmdashboard", String.valueOf(pref.getBoolean("loginstatus", false)));
            editor.clear();
            editor.commit();
            db.deleteProfileData("myprofileandprogram");
            db.deleteData();

            try {
                Log.d("loginlatestActfrmdashboard", String.valueOf(requireContext()));
                Intent intentlogout = new Intent(requireContext(), Login.class);
                intentlogout.addFlags(Intent.FLAG_ACTIVITY_NEW_TASK | Intent.FLAG_ACTIVITY_CLEAR_TASK);
                startActivity(intentlogout);
            }catch (Exception e)
            {
                Log.e("onLogOff Clear", e.toString());
            }
        }
        else {
            Log.d("loginstsfrmdash", String.valueOf(pref.getBoolean("loginstatus", false)));
        }
    }

    @Override
    public void onPause() {
        super.onPause();
        DashboardFragmentIsVisible = false;
    }
    @Override
    public void onStop() {
        super.onStop();
        Log.d("onStop", "Removing Twilio Listeners...");
        removeTwilioListeners();  // Clean up listeners when leaving page

        requireContext().unregisterReceiver(notificationReceiver);
    }

    private void removeTwilioListeners() {
        ConversationsClient conversationsClient = ConversationsClientManager.getInstance().getConvClient();
        if (conversationsClient != null) {
            for (Conversation conversation : conversationsClient.getMyConversations()) {
                conversation.removeAllListeners(); // Remove all listeners
            }
        }
    }

    @SuppressLint("ResourceType")
    @Override
    public View onCreateView(LayoutInflater inflater, ViewGroup container,
                             Bundle savedInstanceState) {

        view=inflater.inflate(R.layout.fragment_dashboard, container, false);
        ivNotification = (ImageView) view.findViewById(R.id.dashboard_notification_bell);
        ivNotification.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View view) {
                FragmentTransaction fragmentManager = getActivity().getSupportFragmentManager().beginTransaction();
                fragmentManager.replace(R.id.fl_main,new NotificationFragment());
                fragmentManager.commit();
            }
        });

        UnreadMessageViewModel unreadViewModel = new ViewModelProvider(requireActivity()).get(UnreadMessageViewModel.class);

        unreadViewModel.getUnreadCounts().observe(getViewLifecycleOwner(), unreadCounts -> {
            int totalUnread = unreadViewModel.getTotalUnread();
            chatMessageCount.setText(String.valueOf(totalUnread));
            chatMessageCount.setVisibility(totalUnread > 0 ? View.VISIBLE : View.GONE);
        });

        chartContainer = view.findViewById(R.id.chartContainer);

        call = (ImageView) view.findViewById(R.id.dashboard_call);
        call.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View view) {
                Intent intent = new Intent(Intent.ACTION_DIAL);
                intent.setData(Uri.parse("tel:+16232676578"));
                startActivity(intent);
            }
        });

       chatMessageCount = view.findViewById(R.id.chatMessageCount);

        chat = (ImageView) view.findViewById(R.id.dashboard_chat);
        chat.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View view) {
                Log.d("chat", "chat");
                FragmentTransaction transaction = requireActivity().getSupportFragmentManager().beginTransaction();
                transaction.replace(R.id.fl_main, new ChatMainFragment());
                transaction.addToBackStack(null);  // Add this line to save the previous fragment in the back stack
                transaction.commit();
            }
        });

        user_full_name = (TextView) view.findViewById(R.id.dashboard_user_full_name);
        patent_id = (TextView) view.findViewById(R.id.dashboard_patent_id);
        tv_prog_name = (TextView) view.findViewById(R.id.frag_dash_prog_name);
        tv_status = (TextView) view.findViewById(R.id.frag_dash_status);
        tv_date = (TextView) view.findViewById(R.id.frag_dash_date);
        tv_notification_count = (TextView) view.findViewById(R.id.dashboard_notification_count);
        recentvtls = (TextView) view.findViewById(R.id.recentvt);
        db = new DataBaseHelper(getContext());
        pref = this.getActivity().getSharedPreferences("RPMUserApp",getContext().MODE_PRIVATE);
        editor = pref.edit();
        user_full_name.setTextSize(TypedValue.COMPLEX_UNIT_SP, textSizeInSP);

// Define a mapping of status text to background colors
        HashMap<String, Integer> statusColorMap = new HashMap<>();
        HashMap<String, Integer> statusTextColorMap = new HashMap<>();

        statusColorMap.put("Active", R.drawable.active_background_radious);
        statusColorMap.put("Enrolled", R.drawable.enrollpres_background_radious);
        statusColorMap.put("Prescribed", R.drawable.enrollpres_background_radious);
        statusColorMap.put("InActive", R.drawable.inactive_background_radious);
        statusColorMap.put("OnHold", R.drawable.inactive_background_radious);
        statusColorMap.put("Discharged", R.drawable.discharg_background_radious);
        statusColorMap.put("ReadyToDischarge", R.drawable.discharg_background_radious);

        statusTextColorMap.put("Active", R.color.white);
        statusTextColorMap.put("Enrolled", R.color.white);
        statusTextColorMap.put("Prescribed", R.color.white);
        statusTextColorMap.put("InActive", R.color.grey_60);
        statusTextColorMap.put("OnHold", R.color.black);
        statusTextColorMap.put("Discharged", R.color.white);
        statusTextColorMap.put("ReadyToDischarge", R.color.white);

        Token = pref.getString("Token", null);
        String User_full_Name = pref.getString("User_full_name", null);
        String Patent_Id = pref.getString("Patent_id", null);
        String CompletedDuration = pref.getString("CompletedDuration", null);
        String Status = pref.getString("Status", null);
        ProgramName = pref.getString("ProgramName", null);
        ProgramTypeName = pref.getString("ProgramTypeName", null);

        if (ProgramName != null && ProgramName.equals("RPM")) {
            Log.d("pgmnameRPM","pgmnameRPM");
            tv_vital_header = (TextView) view.findViewById(R.id.vital_card_header);
            tv_vital_header_label = (TextView) view.findViewById(R.id.vital_card_header_label);
            tv_vital_value = (TextView) view.findViewById(R.id.vital_card_value);
            tv_pulse_value = (TextView) view.findViewById(R.id.vital_pulse_value);
            tv_pulse_unit = (TextView) view.findViewById(R.id.vital_pulse_unit);
            tv_vital_unit = (TextView) view.findViewById(R.id.vital_card_unit);
            tv_vital_time = (TextView) view.findViewById(R.id.vital_card_time);
            emptyChart = (TextView) view.findViewById(R.id.empty_viewDB);
            recentvtls.setVisibility(View.VISIBLE);
           }

        SimpleDateFormat DateFor = new SimpleDateFormat("EEEE, MMM dd, yyyy");
        tv_date.setText(DateFor.format(new Date()));
        tv_prog_name.setText(ProgramTypeName);
        tv_status.setText(Status);

        // Check if the status text is in your mapping
        if (Status != null && statusColorMap.containsKey(Status) && statusTextColorMap.containsKey(Status)) {
            int backgroundColor = statusColorMap.get(Status);
            int textColorResource = statusTextColorMap.get(Status);
            // Set the background color of the TextView
            tv_status.setBackgroundResource(backgroundColor);
            // Set the text color of the TextView
            tv_status.setTextColor(getResources().getColor(textColorResource));
        } else {
            tv_status.setBackgroundColor(Color.GREEN); //  Correct

           // tv_status.setBackgroundResource(Color.GREEN);
            tv_status.setTextColor(Color.WHITE); // ✅ No need to call getResources().getColor()

          //  tv_status.setTextColor(getResources().getColor(Color.WHITE));
            // Handle the case where the status text is not in your mapping
            // You can set a default background color or handle it in some other way
        }

        user_full_name.setText("Hi, " + User_full_Name);
        patent_id.setText(Patent_Id);
        mpLinechart7 = (LineChart) view.findViewById(R.id.linechartDashboard);
        markerView = new MyMarkerView(getContext(), R.layout.custom_marker_layout7);
        textHighlight7 = (TextView) markerView.findViewById(R.id.marker_text7);
        recyclerView_todolist = view.findViewById(R.id.recyclerview_todolist_vertical_dash);
        layoutManager = new LinearLayoutManager(getContext());
        toDoListModels = new ArrayList<>();
        adapter = new ToDoListHomeAdapter(getContext(), toDoListModels);
        recyclerView_todolist.setLayoutManager(new LinearLayoutManager(getContext(), LinearLayoutManager.HORIZONTAL, false));
        recyclerView_todolist.setAdapter(adapter);

        // Get screen width
        DisplayMetrics displayMetrics = new DisplayMetrics();
        requireActivity().getWindowManager().getDefaultDisplay().getMetrics(displayMetrics);
        int screenWidth = displayMetrics.widthPixels; // Screen width in pixels

        // Set RecyclerView width dynamically
        int recyclerWidth;
        if (screenWidth >= 1200) { // Large tablet
            recyclerWidth = (int) (screenWidth * 0.7); // 70% of screen width
        } else if (screenWidth >= 600) { // Small tablet
            recyclerWidth = (int) (screenWidth * 0.6); // 60% of screen width
        } else { // Mobile
            recyclerWidth = (int) (screenWidth * 0.5); // 50% of screen width
        }

        // Apply new width to RecyclerView
        ViewGroup.LayoutParams params = recyclerView_todolist.getLayoutParams();
        params.width = recyclerWidth;
        recyclerView_todolist.setLayoutParams(params);

        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.O) {
            try {
                checkToDoListItems();
            } catch (ParseException e) {
                e.printStackTrace();
            }
        }
        getNotificationsCount();

        // Get the system's default time zone
        TimeZone systemTimeZone = TimeZone.getDefault();

        // Get the current date and time in the system's time zone
        Calendar currentCalendar = Calendar.getInstance();

        // Set the time zone of currentCalendar to the system's time zone
        currentCalendar.setTimeZone(systemTimeZone);

        // Now, currentCalendar holds the current date and time in the system's time zone
        System.out.println("Currentdateandtimeinsystemtimezone" + currentCalendar.getTime());

        // Calculate the end date (current date and time)
        Calendar endDateCalendar = (Calendar) currentCalendar.clone();

        // Calculate the start date (7 days back)
        Calendar startDate7Calendar = (Calendar) currentCalendar.clone();
        startDate7Calendar.add(Calendar.DAY_OF_MONTH, -6);

        // Format the dates in the desired format
        SimpleDateFormat sdf = new SimpleDateFormat("yyyy-MM-dd'T'HH:mm:ss.SSS'Z'");
        sdf.setTimeZone(systemTimeZone); // Use the system's time zone

        // Clone the endDateCalendar
        Calendar endDatePlusOneCalendar = (Calendar) endDateCalendar.clone();

        // Add one day to the cloned endDateCalendar
        endDatePlusOneCalendar.add(Calendar.DAY_OF_MONTH, 1);

        // Format the endDatePlusOneCalendar in the desired format
        String formattedEndDatePlusOne = sdf.format(endDatePlusOneCalendar.getTime());

        // Print the formatted endDatePlusOne
        Log.d("EndDate1day(UTC):", formattedEndDatePlusOne);

        String formattedCurrentDate = sdf.format(currentCalendar.getTime());
        String formattedStart7Date = sdf.format(startDate7Calendar.getTime());
        String formattedEndDate = sdf.format(endDateCalendar.getTime());

        // Print the formatted dates
        Log.d("CurrentDateandTime(UTC):",formattedCurrentDate);
        Log.d("StartDate(7daysback,UTC):",formattedStart7Date);
        Log.d("EndDate(UTC):",formattedEndDate);

        if (ProgramName != null && ProgramName.equals("RPM"))
       {
           Log.d("pgmnameisRPM","pgmnameisRPM");
           checkVitalData(formattedStart7Date,formattedEndDate);
         }
       else {
           Log.d("pgmnameisCCM","pgmnameisCCM");
       }
      //  connectWebSocket();
        todo_nofoactivites = view.findViewById(R.id.todo_nofoactivitesda_dash);
        return view;
    }

    private void setupTwilioListeners() {
        ConversationsClient conversationsClient = ConversationsClientManager.getInstance().getConvClient();

        if (conversationsClient == null) {
            Log.e("TwilioChat", "ConversationsClient is null. Retrying...");
            retrySetupLater();
            return;
        }

        if (conversationsClient != null) {
            if (!ConversationsClientManager.getInstance().isClientSynced()) {
                Log.e("TwilioChat", "Client not yet synced. Waiting...");
                waitForSyncAndRetry();
                return;
            }
        }

        if (conversationsClient != null) {
            Log.e("conversationsClientNOTNULL","conversationsClientNOTNULL");
            for (Conversation conversation : conversationsClient.getMyConversations()) {
                String conversationSid = conversation.getSid();
                unreadCounts.put(conversationSid, 0); // Initialize unread count

                // Fetch the initial unread count for each conversation
                conversation.getUnreadMessagesCount(new CallbackListener<Long>() {
                    @Override
                    public void onSuccess(Long unreadCountValue) {
                        unreadCounts.put(conversationSid, unreadCountValue != null ? unreadCountValue.intValue() : 0);
                        updateUI(); // Update UI with initial count
                    }

                    @Override
                    public void onError(ErrorInfo errorInfo) {
                        Log.e("TwilioChat", "Failed to fetch initial unread count: " + errorInfo.getMessage());
                    }
                });

                // Listen for new messages and conversation synchronization
                conversation.addListener(new ConversationListener() {
                    @Override
                    public void onMessageAdded(Message message) {
                        Log.d("TwilioChat", "New message received.");
                        updateUnreadCount(conversation);
                    }

                    @Override
                    public void onSynchronizationChanged(Conversation conversation) {
                        Log.d("TwilioChat", "Conversation sync changed.");
                        updateUnreadCount(conversation);
                    }

                    @Override
                    public void onMessageUpdated(Message message, Message.UpdateReason reason) {}

                    @Override
                    public void onMessageDeleted(Message message) {
                        Log.d("homeonMessageDeleted", "homeonMessageDeleted");
                        FileLogger.d("homeonMessageDeleted", "homeonMessageDeleted");
                        updateUnreadCount(conversation);
                    }

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
        }
    }

    private void retrySetupLater() {
        handler.postDelayed(new Runnable() {
            @Override
            public void run() {
                setupTwilioListeners();
            }
        }, RETRY_INTERVAL);
    }

    private void waitForSyncAndRetry() {
        handler.postDelayed(new Runnable() {
            @Override
            public void run() {
                if (ConversationsClientManager.getInstance().isClientSynced()) {
                    Log.e("TwilioChat", "Client is now synced. Setting up listeners.");
                    setupTwilioListeners();
                } else {
                    Log.e("TwilioChat", "Client still not synced. Retrying...");
                    waitForSyncAndRetry();
                }
            }
        }, RETRY_INTERVAL);
    }

    private void updateUnreadCount(Conversation conversation) {
        String conversationSid = conversation.getSid();
        conversation.getUnreadMessagesCount(new CallbackListener<Long>() {
            @Override
            public void onSuccess(Long unreadCountValue) {
                if (unreadCountValue != null) {
                    unreadCounts.put(conversationSid, unreadCountValue.intValue());
                    updateUI(); // UI update only when count changes
                }
            }

            @Override
            public void onError(ErrorInfo errorInfo) {
                Log.e("TwilioChat", "Failed to update unread count: " + errorInfo.getMessage());
            }
        });
    }

    private void updateUI() {
        int totalUnread = unreadCounts.values().stream().mapToInt(Integer::intValue).sum();

        if (!isAdded()) {
            Log.w("DashboardFragment", "updateUI() called but fragment is not attached.");
            return;
        }

        requireActivity().runOnUiThread(() -> {
            if (totalUnread > 0) {
                chatMessageCount.setText(String.valueOf(totalUnread));
                chatMessageCount.setVisibility(View.VISIBLE);
            } else {
                chatMessageCount.setVisibility(View.GONE);
            }
        });
    }

    //TO DO: Need to complete the Notifiction Hub integration post clarifications on return type and testing strategy
    //Also, need to discuss on returning the unread messages on push notification
    private void connectWebSocket() {
        String NOTIFICATION_HUB_URL = Links.BASE_URL + Links.NOTIFICATION_CONNECTHUB;
        StringRequest request = new StringRequest(Request.Method.GET, NOTIFICATION_HUB_URL,new Response.Listener<String>() {
            @Override
            public void onResponse(String response) {
                Log.i("onResponse_personal", response.toString());
                if(response.toString()!=null && response.toString().length()>0)
                    try {
                        uri = new URI(response.replace("\"",""));
                        mWebSocketClient = new WebSocketClient(uri) {
                            @Override
                            public void onOpen(ServerHandshake serverHandshake) {
                                Log.i("Websocket", "Opened");
                                //mWebSocketClient.send("Hello from " + Build.MANUFACTURER + " " + Build.MODEL);
                            }
                            @Override
                            public void onMessage(String s) {
                                String message = null;
                                JSONObject msg = null;
                                try {
                                    msg = new JSONObject(s);
                                    message  = msg.getString("User");
                                } catch (JSONException e) {
                                    e.printStackTrace();
                                }
                                String finalMessage = message;
                                getActivity().runOnUiThread(new Runnable() {
                                    @Override
                                    public void run() {
                                        getNotificationsCount();
                                        //tv_notification_count.setText(tv_notification_count.getText() + "\n" + finalMessage);
                                        //tv_notification_count.setVisibility(view.VISIBLE);
                                    }
                                });
                            }

                            @Override
                            public void onClose(int i, String s, boolean b) {
                                Log.i("Websocket", "Closed " + s);
                            }

                            @Override
                            public void onError(Exception e) {
                                Log.i("Websocket", "Error " + e.getMessage());
                            }
                        };
                        mWebSocketClient.connect();
                    } catch (URISyntaxException e) {
                        e.printStackTrace();
                    }
            }

        }, new Response.ErrorListener() {
            @Override
            public void onErrorResponse(VolleyError error) {
                Log.e("onErrorResponse", error.toString());
            }
        }) {
            @Override
            public Map<String, String> getHeaders() throws AuthFailureError {
                Map<String, String> headers = new HashMap<>();

                headers.put("Bearer",Token);
                Log.d("headers_personal",headers.toString());
                Log.d("Token_profile_personal", Token);
                return headers;
            }
        };

        RequestQueue requestQueue = Volley.newRequestQueue(getActivity());
        request.setRetryPolicy(new DefaultRetryPolicy(60000, DefaultRetryPolicy.DEFAULT_MAX_RETRIES, DefaultRetryPolicy.DEFAULT_BACKOFF_MULT));
        requestQueue.add(request);
    }

    public static String uTCToLocal(String dateFormatInPut, String dateFomratOutPut, String datesToConvert) {
        String dateToReturn = datesToConvert;
        java.text.SimpleDateFormat sdf = new java.text.SimpleDateFormat(dateFormatInPut);
        sdf.setTimeZone(TimeZone.getTimeZone("UTC"));
        Date gmt = null;
        java.text.SimpleDateFormat sdfOutPutToSend = new java.text.SimpleDateFormat(dateFomratOutPut);
        sdfOutPutToSend.setTimeZone(TimeZone.getDefault());
        try {
            gmt = sdf.parse(datesToConvert);
            dateToReturn = sdfOutPutToSend.format(gmt);
        } catch (ParseException e) {
            e.printStackTrace();
        }
        return dateToReturn; }

    private void checkVitalData(String formattedStartDate, String formattedEndDate) {
        DateTimeFormatter inputFormatter = null;
        if (android.os.Build.VERSION.SDK_INT >= android.os.Build.VERSION_CODES.O) {
            inputFormatter = DateTimeFormatter.ofPattern("yyyy-MM-dd'T'HH:mm:ss.SSS'Z'");
        }
        LocalDate extractedstDate = null;
        LocalDate extractedendDate = null;

        if (android.os.Build.VERSION.SDK_INT >= android.os.Build.VERSION_CODES.O) {
            extractedstDate = LocalDate.parse(formattedStartDate, inputFormatter);
            extractedendDate = LocalDate.parse(formattedEndDate, inputFormatter);
        }

        String formattedsDate = extractedstDate.toString() + "T00:00:00";
        String formattedendDate = extractedendDate.toString() + "T23:59:59";

        String sdUTC = DateUtils.convertToUTC(formattedsDate, "yyyy-MM-dd'T'HH:mm:ss", "yyyy-MM-dd'T'HH:mm:ss");
        String endUTC = DateUtils.convertToUTC(formattedendDate, "yyyy-MM-dd'T'HH:mm:ss", "yyyy-MM-dd'T'HH:mm:ss");

        String url = Links.BASE_URL + Links.VITAL_SUMMARY + "StartDate=" + sdUTC + "&EndDate=" + endUTC;
        final Loader l1 = new Loader(getActivity());
        l1.show("Please wait...");

        StringRequest stringRequest = new StringRequest(Request.Method.GET, url, new Response.Listener<String>() {
            @RequiresApi(api = Build.VERSION_CODES.O)
            @Override
            public void onResponse(String response) {
                l1.dismiss();
                try {
                    Log.d("chartresponse", response.toString());

                    if (response.trim().startsWith("{")) {
                        // Single Object Case
                        JSONObject jsonObject = new JSONObject(response);
                        processVitalData(jsonObject);
                    } else if (response.trim().startsWith("[")) {
                        // Array of Objects Case
                        JSONArray jsonArray = new JSONArray(response);
                        for (int i = 0; i < jsonArray.length(); i++) {
                            JSONObject jsonObject = jsonArray.getJSONObject(i);
                            processVitalData(jsonObject);
                        }
                    } else {
                        Log.e("ResponseError", "Unexpected response format");
                    }
                } catch (JSONException e) {
                    Log.e("JSONError", "Error parsing JSON: " + e.getMessage());
                }

            }
        }, new Response.ErrorListener() {
            @Override
            public void onErrorResponse(VolleyError error) {
                Toast.makeText(getContext(), "No data available!", Toast.LENGTH_SHORT).show();
                l1.dismiss();

                if (error == null || error.networkResponse == null) {
                    // No internet, show network dialog
                    if (getContext() != null) {  // Ensure Fragment is attached
                        NetworkAlert.showNetworkDialog(requireContext());
                    }
                    return;
                }

                if (error.networkResponse != null && error.networkResponse.statusCode == 401) {
                    Log.d("homecode", String.valueOf(error.networkResponse.statusCode));
                    error.printStackTrace();
                    Log.d("e", error.toString());
                    editor.putBoolean("loginstatus", false);
                    editor.apply();
                    db.deleteProfileData("myprofileandprogram");
                    db.deleteData();
                    editor.clear();
                    editor.commit();
                    try {
                        Intent intentlogout = new Intent(requireContext(), Login.class);
                        intentlogout.addFlags(Intent.FLAG_ACTIVITY_NEW_TASK | Intent.FLAG_ACTIVITY_CLEAR_TASK);
                        startActivity(intentlogout);
                    } catch (Exception e) {
                        Log.e("onLogOff Clear", e.toString());
                    }
                }
                else
                    if (error.networkResponse.statusCode == 400) {
                        Log.e("API_ERROR", "Bad Request: " + new String(error.networkResponse.data));
                    }
            }
        }) {
            @Override
            public Map<String, String> getHeaders() throws AuthFailureError {
                Map<String, String> headers = new HashMap<>();
                headers.put("Bearer", Token);
                Log.d("headers_healthtrends", headers.toString());
                Log.d("Token_healthtrends", Token);
                return headers;
            }
        };
        RequestQueue requestQueue = Volley.newRequestQueue(getActivity());
        stringRequest.setRetryPolicy(new DefaultRetryPolicy(60000, DefaultRetryPolicy.DEFAULT_MAX_RETRIES, DefaultRetryPolicy.DEFAULT_BACKOFF_MULT));
        requestQueue.add(stringRequest);
    }

    private void processVitalData(JSONObject jsonObject) {
        try {
            if (!isAdded()) {
                Log.w("DashboardFragment", "Fragment not attached. Skipping processVitalData.");
                return;
            }
            LayoutInflater inflater = LayoutInflater.from(requireContext());
            View cardViewLayout = inflater.inflate(R.layout.card_view_item, null);
            TextView vitalNameTV = cardViewLayout.findViewById(R.id.vital_card_header);
            TextView vitalValueTV = cardViewLayout.findViewById(R.id.vital_card_value);
            TextView vitalUnitTV = cardViewLayout.findViewById(R.id.vital_card_unit);
            TextView vitalPulseValueTV = cardViewLayout.findViewById(R.id.vital_pulse_value);
            TextView vitalPulseUnitTV = cardViewLayout.findViewById(R.id.vital_pulse_unit);
            TextView timeAgoTV = cardViewLayout.findViewById(R.id.vital_card_time);
            CardView cardview = cardViewLayout.findViewById(R.id.cardDB);
            cardview.setVisibility(View.VISIBLE);

            String vitalName = jsonObject.isNull("VitalName") ? "No Readings" : jsonObject.optString("VitalName", "").trim();

            JSONObject latestVitalMeasure = jsonObject.optJSONObject("LatestVitalMeasure");
            if (latestVitalMeasure == null || latestVitalMeasure.length() == 0) {
                Log.w("JSONWarning", "Skipping entry: LatestVitalMeasure is null or empty for " + vitalName);

                vitalNameTV.setText(vitalName);
                vitalUnitTV.setText("");
                vitalPulseValueTV.setText("");
                vitalPulseUnitTV.setText("");
                timeAgoTV.setText("");
            }
            else {
                String value = latestVitalMeasure.optString("Value", "N/A");
                String unit = latestVitalMeasure.optString("unit", "N/A");
                String pulsevalue = latestVitalMeasure.optString("PulseValue", "N/A");
                String pulseunit = latestVitalMeasure.optString("PulseUnit", "N/A");
                String date = latestVitalMeasure.optString("Date", "N/A");
                String localDateFormatted = DateUtils.convertUtcToLocalTimeDiff(date, "yyyy-MMM-dd, h:mm a");
                String timeAgo = null;
                if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.O) {
                    timeAgo = TimeAgoUtils.calculateTimeAgo(localDateFormatted);
                }
                vitalNameTV.setText(vitalName);
                vitalValueTV.setText(value);
                vitalUnitTV.setText(unit);
                vitalPulseValueTV.setText(pulsevalue);
                vitalPulseUnitTV.setText(pulseunit);
                timeAgoTV.setText(timeAgo);
            }
            JSONArray timeArray = jsonObject.optJSONArray("Time");
            JSONArray valuesArray = jsonObject.optJSONArray("Values");

            if (timeArray != null && valuesArray != null && valuesArray.length() > 0) {
                List<String> xAxisLabels = new ArrayList<>();
                for (int t = 0; t < timeArray.length(); t++) {
                    String utcTimestamp = timeArray.optString(t, "");
                    String localTime = TimeFormatter.formatChartTimestampFromUTC(utcTimestamp);
                    xAxisLabels.add(localTime);
                }

                List<ILineDataSet> dataSets = new ArrayList<>();
                for (int v = 0; v < valuesArray.length(); v++) {
                    JSONObject valueObject = valuesArray.getJSONObject(v);
                    String label = valueObject.optString("label", "Unknown");
                    JSONArray dataArray = valueObject.optJSONArray("data");

                    List<Entry> entries = new ArrayList<>();
                    for (int d = 0; d < dataArray.length(); d++) {
                        if (!dataArray.isNull(d)) {
                            float yValue = Float.parseFloat(dataArray.optString(d, "0"));
                            entries.add(new Entry(d, yValue));
                        }
                    }

                    if (!entries.isEmpty()) {
                        LineDataSet dataSet = new LineDataSet(entries, label);
                        dataSet.setColor(getRandomColor(v));
                        dataSet.setCircleColor(getRandomColor(v));
                        dataSet.setLineWidth(2f);
                        dataSet.setCircleRadius(3f);
                        dataSet.setDrawValues(true);
                        dataSet.setHighLightColor(Color.RED);
                        dataSets.add(dataSet);
                    }
                }

                LineChart lineChart = cardViewLayout.findViewById(R.id.linechartDashboard);
                LineData lineData = new LineData(dataSets);
                configureLineChart(lineChart, xAxisLabels, lineData);
            }
            chartContainer.addView(cardViewLayout);
        } catch (JSONException e) {
            Log.e("JSONError", "Error processing JSON object", e);
        }
    }

    private void configureLineChart(LineChart lineChart, List<String> xAxisLabels, LineData lineData) {
        lineChart.setData(lineData);
        lineChart.getDescription().setEnabled(false);
        lineChart.setTouchEnabled(true);
        lineChart.setPinchZoom(true);
        XAxis xAxis = lineChart.getXAxis();
        xAxis.setValueFormatter(new IndexAxisValueFormatter(xAxisLabels));
        xAxis.setPosition(XAxis.XAxisPosition.BOTTOM);
        xAxis.setGranularityEnabled(true);
        xAxis.setGranularity(1f);
        xAxis.setDrawGridLines(false);
        xAxis.setLabelRotationAngle(-85f);

        // **Ensure all labels are visible**
        xAxis.setLabelCount(xAxisLabels.size()); // Force showing all labels
        YAxis leftAxis = lineChart.getAxisLeft();
        leftAxis.setDrawGridLines(true);
        YAxis rightAxis = lineChart.getAxisRight();
        rightAxis.setEnabled(false);
        Legend legend = lineChart.getLegend();
        legend.setTextSize(12f);
        legend.setForm(Legend.LegendForm.LINE);
        CustomMarkerView markerView = new CustomMarkerView(lineChart.getContext(), R.layout.custom_chart_marker_view, xAxisLabels, lineChart);
        markerView.setChartView(lineChart);
        lineChart.setMarker(markerView);
        lineChart.invalidate(); // Refresh the chart
    }

    private int getRandomColor(int index) {
        int[] colors = {Color.RED, Color.BLUE, Color.GREEN, Color.CYAN, Color.MAGENTA, Color.YELLOW};
        return colors[index % colors.length];
    }

    private void showNoActivities()
    {
        ToDoListModel toDoListModel   = new ToDoListModel();
        toDoListModel.setDecription("No Activities");
        toDoListModels.add(toDoListModel);
    }

    @RequiresApi(api = Build.VERSION_CODES.O)
    private void checkToDoListItems() throws ParseException {
        LocalDate currentDate = LocalDate.now();
        System.out.println("Current Local Date: " + currentDate);
        String cursdt = currentDate + "T00:00:00";
        String todoUtcStDate = DateUtils.convertToUTC(cursdt, "yyyy-MM-dd'T'HH:mm:ss", "yyyy-MM-dd'T'HH:mm:ss");
        Log.d("todoUtcStDate", todoUtcStDate);
        String curedt = currentDate + "T23:59:59";
        String todoUtcEdDate = DateUtils.convertToUTC(curedt, "yyyy-MM-dd'T'HH:mm:ss", "yyyy-MM-dd'T'HH:mm:ss");
        Log.d("todoUtcEdDate", todoUtcEdDate);
        String localTimeZoneId = TimeZone.getDefault().getID();
        Log.d("localTimeZoneId", localTimeZoneId);
        // Print the results
        Log.d("cursdt:", cursdt);
        Log.d("curedt:", curedt);

        String url = Links.BASE_URL + Links.TODOLIST + "StartDate=" + cursdt + "&EndDate=" + curedt;
        final Loader l1 = new Loader(getActivity());
        l1.show("Please wait...");
        StringRequest stringRequest = new StringRequest(Request.Method.GET, url, new Response.Listener<String>() {
            @Override
            public void onResponse(String response) {
                Log.d("todoresponse", response.toString());
                JSONArray jsonArray = null;
                l1.dismiss();
                try {
                    jsonArray = new JSONArray(response);
                    if (jsonArray == null && jsonArray.length() == 0) {
                        Toast.makeText(getContext(), "No data available!", Toast.LENGTH_SHORT).show();
                        showNoActivities();
                    }
                    Log.d("log_data_array", jsonArray.toString());
                } catch (JSONException e) {
                    e.printStackTrace();
                    showNoActivities();
                }
                if (jsonArray != null) {
                    if (jsonArray.length() <= 0) {
                        Toast.makeText(getContext(), "No Data Available", Toast.LENGTH_SHORT).show();
                        showNoActivities();
                    }
                    todo_nofoactivites.setText(jsonArray.length() + " Activities");
                    for (int i = 0; i < jsonArray.length(); i++) {
                        try {
                            JSONObject jsonObject1 = jsonArray.getJSONObject(i);
                            ToDoListModel toDoListModel = new ToDoListModel();
                            toDoListModel.setScheduleType(jsonObject1.getString("ActivityName"));
                            toDoListModel.setDecription(jsonObject1.getString("Description"));
                            String todoutcTime = DateUtils.formatDate(jsonObject1.getString("Date"), "MMM dd");
                            Log.d("todoutcTime", todoutcTime);

                            toDoListModel.setTime(todoutcTime);
                            toDoListModels.add(toDoListModel);
                        } catch (JSONException e) {
                            e.printStackTrace();
                        } catch (ParseException e) {
                            e.printStackTrace();
                        } finally {
                            adapter.notifyDataSetChanged();
                        }
                    }
                }

            }
        }, new Response.ErrorListener() {
            @Override
            public void onErrorResponse(VolleyError error) {
                Toast.makeText(getContext(), "No data available!", Toast.LENGTH_SHORT).show();
                l1.dismiss();
                showNoActivities();
                if (error == null || error.networkResponse == null) {
                    // Handle network error (e.g., no internet connection)
                    Log.e("VolleyError", "No response from server. Possible network issue.");
                    return;
                }

                int statusCode = error.networkResponse.statusCode; // Now safe to access
                Log.e("VolleyError", "Error Code: " + statusCode);

                if ( statusCode == 401) {
                    Log.d("homecode", String.valueOf(statusCode));
                    error.printStackTrace();
                    Log.d("e", error.toString());
                    editor.putBoolean("loginstatus", false);
                    editor.apply();
                    db.deleteProfileData("myprofileandprogram");
                    db.deleteData();
                    editor.clear();
                    editor.commit();

                    try {
                        Log.d("loginlatestActfrmdashboard", String.valueOf(requireContext()));
                        Intent intentlogout = new Intent(requireContext(), Login.class);
                        intentlogout.addFlags(Intent.FLAG_ACTIVITY_NEW_TASK | Intent.FLAG_ACTIVITY_CLEAR_TASK);
                        startActivity(intentlogout);
                    }catch (Exception e)
                    {
                        Log.e("onLogOff Clear", e.toString());
                    }
                }
            }
        }) {
            @Override
            public Map<String, String> getHeaders() throws AuthFailureError {
                Map<String, String> headers = new HashMap<>();
                headers.put("Bearer", Token);
                Log.d("headers_myprofileandprogram", headers.toString());
                Log.d("Token_myprofileandprogram", Token);
                return headers;
            }
        };
        RequestQueue requestQueue = Volley.newRequestQueue(getActivity());
        stringRequest.setRetryPolicy(new DefaultRetryPolicy(60000, DefaultRetryPolicy.DEFAULT_MAX_RETRIES, DefaultRetryPolicy.DEFAULT_BACKOFF_MULT));
        requestQueue.add(stringRequest);
    }
    private void getNotificationsCount () {
            String url = Links.BASE_URL + Links.NOTIFICATION;
           // final Loader l1 = new Loader(getActivity());
          //  l1.show("Please wait...");
            StringRequest stringRequest = new StringRequest(Request.Method.GET, url, new Response.Listener<String>() {
                @Override
                public void onResponse(String response) {
                    Log.d("response", response.toString());
                 //   l1.dismiss();
                    try {
                        JSONObject jsonObject = new JSONObject(response);
                        try {
                            Integer value = Integer.parseInt(jsonObject.getString("TotalUnRead"));
                            if (value > 0) {
                                tv_notification_count.setText(value.toString());
                                tv_notification_count.setVisibility(view.VISIBLE);
                            } else
                                tv_notification_count.setVisibility(view.INVISIBLE);
                        } catch (NumberFormatException e) {
                            e.printStackTrace();
                            tv_notification_count.setVisibility(View.INVISIBLE);
                        }
                    } catch (JSONException e) {
                        e.printStackTrace();
                    }
                }
            }, new Response.ErrorListener() {
                @Override
                public void onErrorResponse(VolleyError error) {
                    Log.d("Error while fetching Notification count", error.toString());
                  //  l1.dismiss();
                }
            }) {
                @Override
                public Map<String, String> getHeaders() throws AuthFailureError {
                    Map<String, String> headers = new HashMap<>();
                    headers.put("Bearer", Token);
                    Log.d("headers_myprofileandprogram", headers.toString());
                    Log.d("Token_myprofileandprogram", Token);
                    return headers;
                }
            };
            RequestQueue requestQueue = Volley.newRequestQueue(getActivity());
            stringRequest.setRetryPolicy(new DefaultRetryPolicy(60000, DefaultRetryPolicy.DEFAULT_MAX_RETRIES, DefaultRetryPolicy.DEFAULT_BACKOFF_MULT));
            requestQueue.add(stringRequest);
        }
}