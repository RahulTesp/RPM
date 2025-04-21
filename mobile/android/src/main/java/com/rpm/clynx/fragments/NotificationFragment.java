package com.rpm.clynx.fragments;

import static android.content.Context.MODE_PRIVATE;
import static com.rpm.clynx.utility.Links.BASE_URL;
import static com.rpm.clynx.utility.Links.NOTI_DELETE;
import android.content.SharedPreferences;
import android.os.Bundle;
import androidx.fragment.app.Fragment;
import androidx.recyclerview.widget.LinearLayoutManager;
import androidx.recyclerview.widget.RecyclerView;
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
import com.android.volley.toolbox.StringRequest;
import com.android.volley.toolbox.Volley;
import com.google.android.material.bottomnavigation.BottomNavigationItemView;
import com.rpm.clynx.adapter.NotificationListAdapter;
import com.rpm.clynx.model.NotificationItemModel;
import com.rpm.clynx.model.NotificationsModel;
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
import java.util.HashMap;
import java.util.List;
import java.util.Locale;
import java.util.Map;

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

    @Override
    public void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);

        if (getArguments() != null) {
            mColumnCount = getArguments().getInt(ARG_COLUMN_COUNT);
        }
    }
//    @Override
//    public void onDeleteClick(String notiId) {
//        // Implement your API call here using the notiId
//        // For example, you can make a network request to delete the notification
//        notiDelete(notiId);
//
//
//    }
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

        checkNotifications();
        return view;
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
        String url = Links.BASE_URL+  Links.NOTIFICATION;
        final Loader l1 = new Loader(getActivity());

        l1.show("Please wait...");
        StringRequest stringRequest = new StringRequest(Request.Method.GET, url, new Response.Listener<String>() {
            @Override
            public void onResponse(String response) {
                Log.d("response notifications",response.toString());
                JSONArray jsonArrayData=null;
                DateFormat df = DateFormat.getDateInstance(DateFormat.SHORT, Locale.getDefault());
                DateFormat dft = DateFormat.getTimeInstance(DateFormat.SHORT);
                SimpleDateFormat sdf = new SimpleDateFormat("yyyy-MM-dd'T'HH:mm:ss");
                l1.dismiss();
                try {
                    JSONObject jsonObject = new JSONObject(response);
                    jsonArrayData = new JSONArray(jsonObject.getString("Data"));
                    unreadCount = jsonObject.getString("TotalUnRead");
                    totalCount = jsonObject.getString("TotalNotifications");
                    Log.d("log_data_array",jsonArrayData.toString());
                    TextView tv =  (TextView) view.findViewById(R.id.notification_unread);
                    tv.setText(unreadCount + " Unread");

                    for (int i = 0; i < jsonArrayData.length(); i++){
                        try {
                            try {
                                JSONObject jsonNotificationList = jsonArrayData.getJSONObject(i);
                                JSONArray jsonArrayNL=new JSONArray(jsonNotificationList.getString("NotificationList"));
                                ArrayList<NotificationItemModel> nim = new ArrayList<NotificationItemModel>();
                                for (int j = 0; j < jsonArrayNL.length(); j++){
                                    nim.add(new NotificationItemModel(jsonArrayNL.getJSONObject(j).getString("Description"),jsonArrayNL.getJSONObject(j).getString("NotificationId"),
                                            dft.format(sdf.parse(jsonArrayNL.getJSONObject(j).getString("CreatedOn")))));
                                    Log.d("nimNoti", nim.toString());
                                }
                                notifications.add(new NotificationsModel( df.format(sdf.parse(jsonNotificationList.getString("NotificationDate"))),nim));
                                adapter.notifyDataSetChanged();
                            } catch (ParseException e) {
                                e.printStackTrace();
                            }
                        }
                        catch (JSONException e) {
                            e.printStackTrace();
                        }finally {
                            adapter.notifyDataSetChanged();
                        }
                    }
                } catch (JSONException e) {
                    e.printStackTrace();
                }
                if (jsonArrayData.length()<=0){
                    Toast.makeText(getContext(), "No Data Available", Toast.LENGTH_SHORT).show();
                }
            }
        } ,new Response.ErrorListener() {
            @Override
            public void onErrorResponse(VolleyError error) {
                Toast.makeText(getContext(), "No data available!", Toast.LENGTH_SHORT).show();
                l1.dismiss();
            }
        }){

            @Override
            public Map<String, String> getHeaders() throws AuthFailureError {
                Map<String, String> headers = new HashMap<>();
                headers.put("Bearer",Token);
                Log.d("headers_myprofileandprogram",headers.toString());
                Log.d("Token_myprofileandprogram", Token);
                return headers;
            }
        };
        RequestQueue requestQueue = Volley.newRequestQueue(getActivity());
        stringRequest.setRetryPolicy(new DefaultRetryPolicy(60000, DefaultRetryPolicy.DEFAULT_MAX_RETRIES, DefaultRetryPolicy.DEFAULT_BACKOFF_MULT));
        requestQueue.add(stringRequest);
    }

    private void notiDelete(String notiID)  {
        Log.d("notiID", notiID);
        String NOTI_DELETE_URL = BASE_URL+ NOTI_DELETE +
                notiID;
        System.out.println("NOTI_DELETE_URL");
        System.out.println(NOTI_DELETE_URL);

        JSONObject parameters = new JSONObject();
        BooleanRequest booleanRequest = new BooleanRequest(Request.Method.POST, NOTI_DELETE,
                new Response.Listener<Boolean>() {
                    @Override
                    public void onResponse(Boolean response) {
                        Log.d("notidelresponse", String.valueOf(response));
                        // Handle the boolean response here
                        if (response) {
                            // Request was successful (true)
                        } else {
                            // Request was not successful (false)
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
        requestQueue.add(booleanRequest);
    }
}