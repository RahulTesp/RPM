package com.rpm.clynx.fragments;

import static com.rpm.clynx.utility.Links.BASE_URL;
import static com.rpm.clynx.utility.Links.MEMBERS_LIST;
import android.annotation.SuppressLint;
import android.content.SharedPreferences;
import android.os.Bundle;
import android.util.Log;
import android.view.View;
import android.widget.ProgressBar;
import android.widget.Toast;
import android.widget.Toolbar;
import androidx.appcompat.app.AppCompatActivity;
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
import com.rpm.clynx.R;
import com.rpm.clynx.adapter.ChannelListAdapter;
import com.rpm.clynx.model.ChannelItemModel;
import com.rpm.clynx.model.ChannelModel;
import org.json.JSONArray;
import org.json.JSONException;
import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;
import java.util.Map;

public class ChatAddActivity extends AppCompatActivity {
    public final static String TAG = "TwilioConversations";
    // Update this identity for each individual user, for instance after they login
    SharedPreferences pref;
    SharedPreferences.Editor editor;
    private String Token;
    ProgressBar progressBar;
    RecyclerView channelR;
    private  List<ChannelModel> channelModels;
    private  ChannelListAdapter channelListAdapter;
    RecyclerView.LayoutManager layoutManager;

    @SuppressLint("MissingInflatedId")
    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_add_chat);
        initPerformBackClick();
        pref = this.getSharedPreferences("RPMUserApp",this.MODE_PRIVATE);
        editor = pref.edit();
        Token = pref.getString("Token", null);
        progressBar = findViewById(R.id.progressChatAdd); // Replace with your progress indicator view ID
        LinearLayoutManager layoutManager = new LinearLayoutManager(this);
        // for a chat app, show latest messages at the bottom
        layoutManager.setStackFromEnd(true);
        channelR = findViewById(R.id.fragmentChatAdd);
        layoutManager = new LinearLayoutManager(this);
        channelModels = new ArrayList<>();
        channelListAdapter = new ChannelListAdapter(channelModels,this);
        channelR.setLayoutManager(layoutManager);
        channelR.setAdapter(channelListAdapter);

        membersList();
    }
    private void initPerformBackClick() {
        Toolbar toolbar = findViewById(R.id.toolbar);
        toolbar.setNavigationIcon(R.drawable.ic_baseline_west_24);
        toolbar.setNavigationOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                finish();
            }
        });
    }
    private void membersList() {
        String MEMBERS_LIST_URL = BASE_URL + MEMBERS_LIST;
        StringRequest stringRequest = new StringRequest(Request.Method.GET, MEMBERS_LIST_URL,
                new Response.Listener<String>() {
                    @Override
                    public void onResponse(String response) {
                        //  l1.dismiss();
                        Log.d("MEMBERS_LIST_URL", MEMBERS_LIST_URL);
                        Log.d("membersListresponse", response.toString());
                        JSONArray jsonArray = null;
                        try {
                            jsonArray = new JSONArray(response);
                            ArrayList<ChannelItemModel> nim = new ArrayList<ChannelItemModel>();
                            for (int i = 0; i < jsonArray.length(); i++) {
                                nim.add(new ChannelItemModel(jsonArray.getJSONObject(i).getString("MemberUserName"), jsonArray.getJSONObject(i).getString("MemberName")));
                                Log.d("Channels", nim.toString());
                            }
                            channelModels.add(new ChannelModel(nim,Token));
                            channelListAdapter.notifyDataSetChanged();
                            progressBar.setVisibility(View.GONE);
                        } catch (JSONException e) {
                            e.printStackTrace();
                        } finally {
                            channelListAdapter.notifyDataSetChanged();
                        }
                    }
                },
                new Response.ErrorListener() {
                    @Override
                    public void onErrorResponse(VolleyError error) {
                            Toast.makeText(getApplicationContext(), "error", Toast.LENGTH_LONG).show();
                    }
                })
        {
            @Override
            public Map<String, String> getHeaders() throws AuthFailureError {
                Map<String, String> headers = new HashMap<>();
                headers.put("Bearer",Token);
                Log.d("headers_myprofileandprogram",headers.toString());
                Log.d("Token_myprofileandprogram", Token);
                return headers;
            }
        };

        //creating a request queue
        RequestQueue requestQueue = Volley.newRequestQueue(ChatAddActivity.this);
        //adding the string request to request queue
        stringRequest.setRetryPolicy(new DefaultRetryPolicy(0,-1,DefaultRetryPolicy.DEFAULT_BACKOFF_MULT));
        requestQueue.add(stringRequest);
    }
}
