package com.rpm.clynx.activity;

import com.google.firebase.messaging.FirebaseMessaging;
import com.rpm.clynx.fragments.Login;
import com.rpm.clynx.utility.ConversationsClientManager;
import com.rpm.clynx.utility.DataBaseHelper;
import com.rpm.clynx.utility.Links;
import com.rpm.clynx.utility.MyApplication;
import com.rpm.clynx.utility.NetworkAlert;
import com.rpm.clynx.utility.SystemBarColor;
import com.rpm.clynx.R;
import android.annotation.SuppressLint;
import android.content.Context;
import android.content.Intent;
import android.content.SharedPreferences;
import android.graphics.PixelFormat;
import android.os.Bundle;
import android.util.Log;
import android.view.View;
import android.widget.Button;
import android.widget.LinearLayout;
import android.widget.TextView;
import android.widget.Toast;
import androidx.appcompat.app.AppCompatActivity;
import com.android.volley.AuthFailureError;
import com.android.volley.DefaultRetryPolicy;
import com.android.volley.Request;
import com.android.volley.RequestQueue;
import com.android.volley.Response;
import com.android.volley.VolleyError;
import com.android.volley.toolbox.StringRequest;
import com.android.volley.toolbox.Volley;
import org.json.JSONObject;
import java.util.HashMap;
import java.util.Map;
import com.rpm.clynx.service.TwilioChatManager;

public class MoreActivity extends AppCompatActivity {
    SharedPreferences pref;
    SharedPreferences.Editor editor;
    TextView tvusername, tvuserid;
    DataBaseHelper db;
    String Token;
    Button Logout_btn;

    @SuppressLint("MissingInflatedId")
    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        getWindow().setFormat(PixelFormat.RGBA_8888);
        setContentView(R.layout.activity_more);
        SystemBarColor.setSystemBarColor(this, R.color.Primary_bg);
        db = new DataBaseHelper(this);
        pref = this.getSharedPreferences("RPMUserApp",this.MODE_PRIVATE);
        editor = pref.edit();
        String User_full_Name = pref.getString("User_full_name", null);
        String Patent_Id = pref.getString("Patent_id", null);
        Token = pref.getString("Token", null);
        initToolbar();
        tvusername = findViewById(R.id.act_more_username);
        tvuserid = findViewById(R.id.act_more_userid);
        Logout_btn = findViewById(R.id.logout_btn);
        tvusername.setText("Hi, " + User_full_Name);
        tvuserid.setText(Patent_Id);
        LinearLayout linearLayout = findViewById(R.id.linmore);

        linearLayout.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                Toast.makeText(MoreActivity.this, "Coming Soon!", Toast.LENGTH_SHORT).show();
            }
        });
    }
    @Override
    protected void onResume() {
        super.onResume();
        Log.d("actResume","homeonResume");
        if (pref.getBoolean("loginstatus", false) == false){
            Log.d("loginstsfrmhome2", String.valueOf(pref.getBoolean("loginstatus", false)));
            editor.clear();
            editor.commit();
            db.deleteProfileData("myprofileandprogram");
            db.deleteData();

            try {
                Log.d("loginlatestActivity", String.valueOf(((MyApplication) getApplication()).getLatestActivity()));
                Intent intentlogout = new Intent((((MyApplication) getApplication()).getLatestActivity()), Login.class);
                intentlogout.addFlags(Intent.FLAG_ACTIVITY_NEW_TASK | Intent.FLAG_ACTIVITY_CLEAR_TASK);
                startActivity(intentlogout);
                finish();
            }catch (Exception e)
            {
                Log.e("onLogOff Clear", e.toString());
            }
        }
    }

    private void initToolbar() {
        androidx.appcompat.widget.Toolbar toolbar = (androidx.appcompat.widget.Toolbar) findViewById(R.id.toolbar);
        setSupportActionBar(toolbar);
        getSupportActionBar();
    }

    public void myinfo(View view) {
        Intent moremyinfo = new Intent(MoreActivity.this, MyInfoActivity.class);
        startActivity(moremyinfo);
    }
    public void pgminfo(View view) {
        Intent morepgminfo = new Intent(MoreActivity.this, ProgramInfoActivity.class);
        startActivity(morepgminfo);
    }

    public void clinicalinfo(View view) {
        // Inside the onClick method for opening Clinical Info from the More activity
        Intent moreclinicalinfo = new Intent(MoreActivity.this, ClinicalInfoActivity.class);
        moreclinicalinfo.putExtra("opened_from", "MORE");
        startActivity(moreclinicalinfo);
    }

    public void activityinfo(View view) {
        Intent moreactivityinfo = new Intent(MoreActivity.this, ActivityInfoActivity.class);
        startActivity(moreactivityinfo);
    }
    private void logOff(){
       try {
           Log.d("morelogoff","morelogoff");
           TwilioChatManager.getInstance().clearChatList();
           unsubscribeFromTopicsOnLogout(MoreActivity.this);
           editor.putBoolean("loginstatus", false);
           editor.clear();
           editor.apply();
           db.deleteData();
           db.deleteProfileData("myprofileandprogram");
           ConversationsClientManager.getInstance().clearConversationsClient();

           Intent more = new Intent(MoreActivity.this, Login.class);
           more.addFlags(Intent.FLAG_ACTIVITY_CLEAR_TOP);
           startActivity(more);
           finish();
       }catch (Exception e)
       {
           Log.e("onLogOff Client Clear", e.toString());
       }

    }
    // Method to unsubscribe from FCM topics on logout
    public static void unsubscribeFromTopicsOnLogout(Context context) {
        SharedPreferences prefs = context.getSharedPreferences("RPMUserApp", Context.MODE_PRIVATE);
        String token = prefs.getString("FirebaseToken", null);

        if (token != null) {
            // Unsubscribe from FCM topics using the stored token
            FirebaseMessaging.getInstance().unsubscribeFromTopic("videocall");
            // Clear the stored token after unsubscribing
            SharedPreferences.Editor editor = prefs.edit();
            editor.remove("FirebaseToken");
            editor.apply();
        }
    }
    public void logOff(View view) {
        String LOGOUT_URL = Links.BASE_URL + Links.LOGOUT.toString();
        JSONObject parameters = new JSONObject();
        try {
            parameters.put("Bearer", Token);
        } catch (Exception e) {
        }
        StringRequest request = new StringRequest(Request.Method.POST, LOGOUT_URL,new Response.Listener<String>() {
            @Override
            public void onResponse(String response) {
                Log.i("onResponse", response.toString());
                logOff();
            }
        }, new Response.ErrorListener() {
            @Override
            public void onErrorResponse(VolleyError error) {
                Log.e("onErrorResponse", error.toString());
                if (error == null || error.networkResponse == null) {
                    // No internet, show network dialog
                    NetworkAlert.showNetworkDialog(MoreActivity.this); // Replace 'YourActivity' with your actual Activity name
                    return;
                }
                logOff();
            }
        }) {
            @Override
            public Map<String, String> getHeaders() throws AuthFailureError {
                Map<String, String> headers = new HashMap<>();
                // Basic Authentication
                headers.put("Bearer",Token);
                Log.d("Bearer",Token.toString());
                return headers;
            }
        };
        RequestQueue requestQueue = Volley.newRequestQueue(MoreActivity.this);
        request.setRetryPolicy(new DefaultRetryPolicy(60000, DefaultRetryPolicy.DEFAULT_MAX_RETRIES, DefaultRetryPolicy.DEFAULT_BACKOFF_MULT));
        requestQueue.add(request);
    }
}