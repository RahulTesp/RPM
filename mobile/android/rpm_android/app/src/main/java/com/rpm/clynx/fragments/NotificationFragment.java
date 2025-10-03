package com.rpm.clynx.fragments;

import static android.content.Context.MODE_PRIVATE;
import static com.rpm.clynx.utility.Links.BASE_URL;
import static com.rpm.clynx.utility.Links.NOTI_DELETE;
import static com.rpm.clynx.utility.Links.NOTI_DELETE_ALL;

import android.app.Activity;
import android.content.BroadcastReceiver;
import android.content.Context;
import android.content.DialogInterface;
import android.content.Intent;
import android.content.IntentFilter;
import android.content.SharedPreferences;
import android.os.Bundle;

import androidx.appcompat.app.AlertDialog;
import androidx.core.content.ContextCompat;
import androidx.fragment.app.Fragment;
import androidx.localbroadcastmanager.content.LocalBroadcastManager;
import androidx.recyclerview.widget.LinearLayoutManager;
import androidx.recyclerview.widget.RecyclerView;

import android.os.Handler;
import android.os.Looper;
import android.util.Log;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.TextView;
import android.widget.Toast;
import android.widget.Toolbar;
import com.android.volley.AuthFailureError;
import com.android.volley.DefaultRetryPolicy;
import com.android.volley.Request;
import com.android.volley.RequestQueue;
import com.android.volley.Response;
import com.android.volley.VolleyError;
import com.android.volley.toolbox.JsonObjectRequest;
import com.android.volley.toolbox.StringRequest;
import com.android.volley.toolbox.Volley;
import com.google.android.material.bottomnavigation.BottomNavigationItemView;
import com.rpm.clynx.adapter.NotificationListAdapter;
import com.rpm.clynx.model.NotificationItemModel;
import com.rpm.clynx.model.NotificationsModel;
//import com.rpm.clynx.service.NotificationReceiver;
import com.rpm.clynx.utility.BooleanRequest;
import com.rpm.clynx.utility.DataBaseHelper;
import com.rpm.clynx.utility.Links;
import com.rpm.clynx.utility.Loader;
import com.rpm.clynx.R;
import org.json.JSONArray;
import org.json.JSONException;
import org.json.JSONObject;
import java.text.DateFormat;
import java.text.ParseException;
import java.text.SimpleDateFormat;
import java.util.ArrayList;
import java.util.Date;
import java.util.HashMap;
import java.util.List;
import java.util.Locale;
import java.util.Map;
import java.util.TimeZone;

/**
 * A fragment representing a list of Items.
 */
public class NotificationFragment extends Fragment{

    // TODO: Customize parameter argument names
    private static final String ARG_COLUMN_COUNT = "column-count";
    // TODO: Customize parameters
    RecyclerView frgNotifications;
    RecyclerView.LayoutManager layoutManager;
    private int mColumnCount = 1;
    private View view;
    private String Token;
    private DataBaseHelper db;
    private SharedPreferences pref;
    private SharedPreferences.Editor editor;
    private List<NotificationsModel> notifications;
    private NotificationListAdapter adapter;
    private String unreadCount = "0";
    private String totalCount = "0";

    private TextView tvHistory;
    private JSONArray jsonArrayData; // Store the full response to filter on toggle

    public static boolean NotificationFragmentIsVisible = false;
    private int lastNotificationCount = -1; // keep track of last value

    /**
     * Mandatory empty constructor for the fragment manager to instantiate the
     * fragment (e.g. upon screen orientation changes).
     */
    public NotificationFragment() {
    }

    // TODO: Customize parameter initialization
    @SuppressWarnings("unused")
    public static NotificationFragment newInstance(int columnCount) {
        NotificationFragment fragment = new NotificationFragment();
        Bundle args = new Bundle();
        args.putInt(ARG_COLUMN_COUNT, columnCount);
        fragment.setArguments(args);
        return fragment;
    }

    private BroadcastReceiver notificationReceiver = new BroadcastReceiver() {
        @Override
        public void onReceive(Context context, Intent intent) {
            // This method is called when notification/broadcast is received
            //  Call your API here
            Log.d("getNotificationFragsCount", "getNotificationFragsCount");
            String type = intent.getStringExtra("type");
            if ("chat".equals(type)) {
                Log.d("getNotificationFragstype", "getNotificationFragstype");

                checkNotifications(); // (your API method)
            }
        }
    };



    @Override
    public void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);

        if (getArguments() != null) {
            mColumnCount = getArguments().getInt(ARG_COLUMN_COUNT);
        }
    }

    @Override
    public View onCreateView(LayoutInflater inflater, ViewGroup container,
                             Bundle savedInstanceState) {

        view = inflater.inflate(R.layout.fragment_notification, container, false);
        initPerformBackClick();

        db = new DataBaseHelper(getContext());
        pref = getContext().getSharedPreferences("RPMUserApp", MODE_PRIVATE);
        editor = pref.edit();
        Token = pref.getString("Token", null);

        // set the adapter
        frgNotifications = view.findViewById(R.id.fragmentNotifications);
        layoutManager = new LinearLayoutManager(getContext());
        notifications = new ArrayList<>();
        adapter = new NotificationListAdapter(notifications,getContext());
        frgNotifications.setLayoutManager(layoutManager);
        frgNotifications.setAdapter(adapter);

        tvHistory = view.findViewById(R.id.tv_history);
        tvHistory.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {

                new AlertDialog.Builder(v.getContext())
                        .setTitle("Confirm Delete")
                        .setMessage("Do you really want to delete all?")
                        .setPositiveButton("Yes", new DialogInterface.OnClickListener() {
                            @Override
                            public void onClick(DialogInterface dialog, int which) {
                                notiDelete(""); // Only called when "Yes" is clicked
                            }
                        })
                        .setNegativeButton("No", null) // Do nothing on "No"
                        .show();

            }
        });


        checkNotifications();
        return view;
    }

    @Override
    public void onResume() {
        super.onResume();
        NotificationFragmentIsVisible = true;
        checkNotifications();
    }

    @Override
    public void onPause() {
        super.onPause();
        NotificationFragmentIsVisible = false;
    }

    @Override
    public void onStart() {
        super.onStart();

        IntentFilter filter = new IntentFilter("com.rpm.clynx.NOTIFICATION_RECEIVED");
        LocalBroadcastManager.getInstance(requireContext()).registerReceiver(notificationReceiver, filter);
    }

    @Override
    public void onStop() {
        super.onStop();
        LocalBroadcastManager.getInstance(requireContext()).unregisterReceiver(notificationReceiver);
    }
    private void initPerformBackClick() {
        Toolbar toolbar = view.findViewById(R.id.toolbar);
        toolbar.setNavigationIcon(R.drawable.ic_baseline_west_24);
        toolbar.setNavigationOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                BottomNavigationItemView navigationView = (BottomNavigationItemView) getActivity().findViewById(R.id.Home);
                navigationView.performClick();
            }
        });
    }

    private void checkNotifications() {
        fetchNotifications(false);
    }

    private void fetchNotifications(boolean isRetry) {
        // Ensure fragment is attached before doing anything
        if (!isAdded()) {
            Log.w("NotificationFragment", "Fragment not attached, skipping fetchNotifications()");
            return;
        }

        Activity activity = getActivity();
        if (activity == null || activity.isFinishing()) {
            Log.w("NotificationFragment", "Activity is null or finishing, skipping fetchNotifications()");
            return;
        }

        // Safe loader
        final Loader l1 = new Loader(activity);
        try {
            l1.show("Please wait...");
        } catch (Exception e) {
            Log.w("NotificationFragment", "Failed to show loader: " + e.getMessage());
        }

        String url = Links.BASE_URL + Links.NOTIFICATION;

        StringRequest stringRequest = new StringRequest(Request.Method.GET, url,
                response -> {
                    // Check fragment still attached before updating UI
                    if (!isAdded()) {
                        try { l1.dismiss(); } catch (Exception ignored) {}
                        return;
                    }

                    try {
                        l1.dismiss();
                    } catch (Exception ignored) {}

                    try {
                        JSONObject jsonObject = new JSONObject(response);
                        jsonArrayData = new JSONArray(jsonObject.getString("Data"));
                        unreadCount = jsonObject.getString("TotalUnRead");
                        totalCount = jsonObject.getString("TotalNotifications");

                        int currentCount = Integer.parseInt(unreadCount);

                        // Update UI safely
                        if (getView() != null) {
                            TextView tv = getView().findViewById(R.id.notification_unread);
                            if (tv != null) {
                                tv.setText(unreadCount + " Unread");
                            }
                        }

                        filterAndDisplayNotifications();

                        // Retry logic if needed
                        if (!isRetry && lastNotificationCount != -1 && currentCount <= lastNotificationCount) {
                            new Handler(Looper.getMainLooper())
                                    .postDelayed(() -> fetchNotifications(true), 1500);
                        }

                        lastNotificationCount = currentCount;

                    } catch (JSONException e) {
                        e.printStackTrace();
                    }

                    // Toast if no data
                    if ((jsonArrayData == null || jsonArrayData.length() <= 0) && getContext() != null) {
                        Toast.makeText(getContext(), "No Data Available", Toast.LENGTH_SHORT).show();
                    }

                },
                error -> {
                    // Only update UI if fragment is attached
                    if (isAdded() && getContext() != null) {
                        try { l1.dismiss(); } catch (Exception ignored) {}
                        Toast.makeText(getContext(), "No data available!", Toast.LENGTH_SHORT).show();
                    }
                }
        ) {
            @Override
            public Map<String, String> getHeaders() throws AuthFailureError {
                Map<String, String> headers = new HashMap<>();
                headers.put("Bearer", Token);
                return headers;
            }
        };

        // Use Activity's application context for RequestQueue
        RequestQueue requestQueue = Volley.newRequestQueue(activity.getApplicationContext());
        stringRequest.setRetryPolicy(new DefaultRetryPolicy(
                60000,
                DefaultRetryPolicy.DEFAULT_MAX_RETRIES,
                DefaultRetryPolicy.DEFAULT_BACKOFF_MULT
        ));
        requestQueue.add(stringRequest);
    }

    private void filterAndDisplayNotifications() {
        if (jsonArrayData == null) return;

        notifications.clear();
        adapter.notifyDataSetChanged();

        DateFormat df = DateFormat.getDateInstance(DateFormat.SHORT, Locale.getDefault());
        DateFormat dft = DateFormat.getTimeInstance(DateFormat.SHORT);
        SimpleDateFormat sdf = new SimpleDateFormat("yyyy-MM-dd'T'HH:mm:ss");

        for (int i = 0; i < jsonArrayData.length(); i++) {
            try {
                JSONObject jsonNotificationList = jsonArrayData.getJSONObject(i);
                JSONArray jsonArrayNL = new JSONArray(jsonNotificationList.getString("NotificationList"));
                ArrayList<NotificationItemModel> nim = new ArrayList<>();

                for (int j = 0; j < jsonArrayNL.length(); j++) {
                    JSONObject notifObj = jsonArrayNL.getJSONObject(j);
                    String description = notifObj.getString("Description");
                    String notificationId = notifObj.getString("NotificationId");

                    String utcTime = notifObj.getString("CreatedOn"); // e.g. "2025-04-24T17:44:00"

// Step 1: Define the input format (UTC)
                    SimpleDateFormat utcFormat = new SimpleDateFormat("yyyy-MM-dd'T'HH:mm:ss", Locale.getDefault());
                    utcFormat.setTimeZone(TimeZone.getTimeZone("UTC"));

// Step 2: Parse the UTC date
                    Date date = utcFormat.parse(utcTime);

// Step 3: Define the output format (local timezone)
                    SimpleDateFormat localFormat = new SimpleDateFormat("hh:mm a", Locale.getDefault()); // e.g. 24 Apr 2025, 11:15 PM
                    localFormat.setTimeZone(TimeZone.getDefault());

// Step 4: Format for display
                    String createdOn = localFormat.format(date);

// Now `createdOn` holds the local time string

                    boolean isRead = notifObj.getBoolean("IsRead");

                    if (!isRead) {
                        nim.add(new NotificationItemModel(description, notificationId, createdOn, isRead));
                    }
                }

                if (!nim.isEmpty()) {
                    notifications.add(new NotificationsModel(df.format(sdf.parse(jsonNotificationList.getString("NotificationDate"))), nim));
                }

            } catch (JSONException | ParseException e) {
                e.printStackTrace();
            }
        }

        adapter.notifyDataSetChanged();
    }

    private void notiDelete(String notiID)  {
        Log.d("notiID", notiID);
        String NOTI_DELETE_URL = BASE_URL+ NOTI_DELETE_ALL;
        System.out.println("NOTI_DELETE_URL");
        System.out.println(NOTI_DELETE_URL);


        JSONObject parameters = new JSONObject(); // Optional: add body params if needed

        JsonObjectRequest jsonObjectRequest = new JsonObjectRequest(
                Request.Method.POST,
                NOTI_DELETE_URL,
                parameters,
                new Response.Listener<JSONObject>() {
                    @Override
                    public void onResponse(JSONObject response) {
                        try {
                            boolean message = response.getBoolean("message");
                            Log.d("notidelresponse", String.valueOf(message));

                            if (message) {
                                // Successfully deleted, now refresh
                                checkNotifications();
                            } else {
                                // Deletion failed
                            }
                        } catch (JSONException e) {
                            e.printStackTrace();
                        }
                    }
                },
                new Response.ErrorListener() {
                    @Override
                    public void onErrorResponse(VolleyError error) {
                        // Handle error
                    }
                }
        ) {
            @Override
            public Map<String, String> getHeaders() throws AuthFailureError {
                Map<String, String> headers = new HashMap<>();
                headers.put("Bearer", Token);
                return headers;
            }
        };

        RequestQueue requestQueue = Volley.newRequestQueue(getActivity());
        requestQueue.add(jsonObjectRequest);
    }
}