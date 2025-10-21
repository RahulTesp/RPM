package com.rpm.clynx.fragments;

import static com.rpm.clynx.utility.Links.BASE_URL;
import static com.rpm.clynx.utility.Links.FB_TOK_DELETE;
import static com.rpm.clynx.utility.Links.FB_TOK_SAVE;
import static com.rpm.clynx.utility.Links.LOGIN;
import static com.rpm.clynx.utility.Links.LOGOUT;
import static com.rpm.clynx.utility.Links.MEMBERS_LIST;
import android.annotation.SuppressLint;
import android.app.Activity;
import android.app.NotificationChannel;
import android.app.NotificationManager;
import android.content.BroadcastReceiver;
import android.content.Context;
import android.content.Intent;
import android.content.IntentFilter;
import android.content.SharedPreferences;
import android.graphics.Color;
import android.net.ConnectivityManager;
import android.net.NetworkInfo;
import android.os.Build;
import android.os.Bundle;
import android.os.Handler;
import android.os.Looper;
import android.text.Editable;
import android.text.TextWatcher;
import android.util.Log;
import android.view.LayoutInflater;
import android.view.View;
import android.widget.Button;
import android.widget.TextView;
import android.widget.Toast;
import androidx.annotation.NonNull;
import androidx.annotation.RequiresApi;
import androidx.appcompat.app.AlertDialog;
import androidx.appcompat.app.AppCompatActivity;
import androidx.core.content.ContextCompat;
import androidx.localbroadcastmanager.content.LocalBroadcastManager;
import com.android.volley.AuthFailureError;
import com.android.volley.DefaultRetryPolicy;
import com.android.volley.NoConnectionError;
import com.android.volley.Request;
import com.android.volley.RequestQueue;
import com.android.volley.Response;
import com.android.volley.TimeoutError;
import com.android.volley.VolleyError;
import com.android.volley.toolbox.JsonObjectRequest;
import com.android.volley.toolbox.StringRequest;
import com.android.volley.toolbox.Volley;
import com.google.android.gms.tasks.OnCompleteListener;
import com.google.android.gms.tasks.Task;
import com.google.android.material.textfield.TextInputEditText;
import com.google.android.material.textfield.TextInputLayout;
import com.google.firebase.messaging.FirebaseMessaging;
import com.rpm.clynx.R;
import com.rpm.clynx.auth.GenerateOTP;
import com.rpm.clynx.auth.OTP;
import com.rpm.clynx.home.Home;
import com.rpm.clynx.utility.ConversationsClientManager;
import com.rpm.clynx.utility.DataBaseHelper;
import com.rpm.clynx.utility.FileLogger;
import com.rpm.clynx.utility.Loader;
import com.rpm.clynx.utility.MyApplication;
import com.rpm.clynx.utility.SystemBarColor;
import com.twilio.conversations.ConversationsClient;
import org.jetbrains.annotations.Nullable;
import org.json.JSONArray;
import org.json.JSONException;
import org.json.JSONObject;
import java.util.HashMap;
import java.util.Map;

public class Login extends AppCompatActivity  implements QuickstartConversationsManagerListener {
    DataBaseHelper db;
    SharedPreferences pref;
    SharedPreferences.Editor editor;
    TextInputLayout userName,passWord;
    TextInputEditText login_UsernameTV,login_PasswordTV;
    Button login_btn;
    boolean valid1;
    String tokenOld,token, mobileNumber;
    String role,programname;
    int timeLimit;
    String valUserID,valPassWord;
    private static final String TAG = "Login";
    String firebasetoken;
    private Handler handler;
    Activity latestActivity;
    private final QuickstartConversationsManager quickstartConversationsManager = new QuickstartConversationsManager(this);
    private static Login logininstance;

    private BroadcastReceiver logoutReceiver = new BroadcastReceiver() {
        @Override
        public void onReceive(Context context, Intent intent) {
            Log.d("logoutintentfrmlogn", String.valueOf(intent));
            if (intent != null && "com.rpm.tespcare.ACTION_LOGOUT_RESULT".equals(intent.getAction())) {
                boolean logoutResult = intent.getBooleanExtra("logoutResult", false);
                String logToken = intent.getStringExtra("LOGINTOKEN");
                // Handle the logout result here
                if (logoutResult) {
                    Log.d("logoutResults", String.valueOf(logoutResult));
                    Log.d("pref.getStringtokn)", String.valueOf(pref.getString("Token", null)));
                    logoutOnExpiry(logToken != null ? logToken : pref.getString("Token", null));
                    // Perform actions related to successful logout
                } else {
                    Log.d("logoutResultf", String.valueOf(logoutResult));
                    // Perform actions related to failed logout
                }
            }
        }
    };

    @RequiresApi(api = Build.VERSION_CODES.Q)
    // private Timer timer;
    @SuppressLint("SuspiciousIndentation")
    @Override

    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_login);
        SystemBarColor.setSystemBarColor(this, R.color.profile_card_body_bg);
        handler = new Handler(Looper.getMainLooper());
        login_btn = findViewById(R.id.login_login_btn);
        userName = findViewById(R.id. login_user_name_input_layout);
        passWord = findViewById(R.id.login_password_input_layout);
        login_UsernameTV = findViewById(R.id.login_user_name_edit_text);
        login_PasswordTV = findViewById(R.id.login_password_edit_text);
        TextView forgotPassword = (TextView)findViewById(R.id.fp);
        quickstartConversationsManager.setListener(this);
        pref = getApplicationContext().getSharedPreferences("RPMUserApp", MODE_PRIVATE);
        editor = pref.edit();
        boolean loginstatus = pref.getBoolean("loginstatus", false);
        boolean otpstatus = pref.getBoolean("otpstatus", false);
        handler = new Handler();
        latestActivity = ((MyApplication) getApplication()).getLatestActivity();

        // Register the logout receiver
        IntentFilter intentFilterlogout = new IntentFilter("com.rpm.tespcare.ACTION_LOGOUT_RESULT");
        LocalBroadcastManager.getInstance(this).registerReceiver(logoutReceiver, intentFilterlogout);

        createNotificationChannel(); // Ensure channel exists

        FileLogger.init(getApplicationContext());

        // Check if the activity was started from a notification tap
        if (getIntent().getExtras() != null) {
            // Handle the data here
            Bundle extras = getIntent().getExtras();
            if (extras != null) {
                Log.d("extras", String.valueOf(extras));
                // Retrieve data from the extras bundle
                String dataValue = extras.getString("Title");
                // Handle the data as needed
            }
        }

        //get notification data info
        Bundle bundle = getIntent().getExtras();
        if (bundle != null) {
            Log.d("bundlenotnull", String.valueOf(bundle));
            System.out.println("bundlenotnull");
            //bundle must contain all info sent in "data" field of the notification
        }

// Disable button initially
        login_btn.setEnabled(false);
        login_btn.setTextColor(ContextCompat.getColor(Login.this, R.color.accent3)); // inactive color

        TextWatcher textWatcher = new TextWatcher() {
            @Override
            public void beforeTextChanged(CharSequence s, int start, int count, int after) {}

            @Override
            public void onTextChanged(CharSequence s, int start, int before, int count) {
                checkFields();
            }

            @Override
            public void afterTextChanged(Editable s) {}
        };

        login_UsernameTV.addTextChangedListener(textWatcher);
        login_PasswordTV.addTextChangedListener(textWatcher);


        forgotPassword.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                Intent intent = new Intent(Login.this, GenerateOTP.class);
                startActivity(intent);

            }
        });

        db = new DataBaseHelper(this);
        Log.d("loginstate", String.valueOf(loginstatus));

        if (loginstatus ){
            Log.d("loginstatsTorF", String.valueOf(loginstatus));

            ConversationsClient ccc =   ConversationsClientManager.getInstance().getConvClient();
            Log.d("cccLOGIN", String.valueOf(ccc));
            if(pref.getString("Token", null) != null) {
                Log.d("chatclientrecreate","chatclientrecreate");
            }
            Log.d("ostatus", String.valueOf(otpstatus));
            Log.d("loginstatus", String.valueOf(loginstatus));

            Intent more = new Intent(Login.this, Home.class);
            more.addFlags(Intent.FLAG_ACTIVITY_CLEAR_TOP );
            startActivity(more);
            finish();
        }
        else
        {
            Log.d("lognstsfalsecase", String.valueOf(loginstatus));
        }

        login_btn.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                if (ValidateData(valid1 == false)) {
                    Toast.makeText(Login.this, "Please Enter Valid Data", Toast.LENGTH_SHORT).show();

                } else {
                    userLogin();
                }
            }
        });
    }


    // Add this method inside your Login class
    private void checkFields() {
        String username = login_UsernameTV.getText().toString().trim();
        String password = login_PasswordTV.getText().toString().trim();

        if (!username.isEmpty() && !password.isEmpty()) {
            login_btn.setEnabled(true);
            login_btn.setTextColor(ContextCompat.getColor(Login.this, R.color.white));
            login_btn.setBackgroundResource(R.drawable.button_inactive); // optional active bg
        } else {
            login_btn.setEnabled(false);
            login_btn.setTextColor(ContextCompat.getColor(Login.this, R.color.accent3)); // inactive color
            login_btn.setBackgroundResource(R.drawable.button_inactive);
        }
    }
    private void createNotificationChannel() {
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.O) {
            String channelId = "NOTIFICATION";
            CharSequence name = "Missed Call Notifications";
            String description = "Notifications for missed calls";
            int importance = NotificationManager.IMPORTANCE_HIGH;

            NotificationChannel channel = new NotificationChannel(channelId, name, importance);
            channel.setDescription(description);
            channel.enableLights(true);
            channel.enableVibration(true);

            NotificationManager notificationManager = getSystemService(NotificationManager.class);
            if (notificationManager != null) {
                notificationManager.createNotificationChannel(channel);
            }
        }
    }

    private BroadcastReceiver newTokenReceiver = new BroadcastReceiver() {
        @Override
        public void onReceive(Context context, Intent intent) {
            // Extract the token from the intent
            String refreshedtoken = intent.getStringExtra("onNewToken");
            // Call the method in the activity to save the new token to the server
            // firebasetokenInsertion(refreshedtoken);
            LoginHelper.firebasetokenInsertion(Login.this, token, refreshedtoken);
        }
    };

    @Override
    protected void onResume() {
        super.onResume();
        IntentFilter intentFilterTok = new IntentFilter("com.example.NEW_TOKEN_RECEIVED");
        LocalBroadcastManager.getInstance(this).registerReceiver(newTokenReceiver, intentFilterTok);
        Log.d("loginonResststus", String.valueOf(pref.getBoolean("loginstatus", false) ));
    }

    @Override
    protected void onPause() {
        super.onPause();
    }

    @Override
    public void onBackPressed() {
        //do nothing
        super.onBackPressed();
        finishAffinity();
    }

    private void userLogin() {
        final Loader l1 = new Loader(Login.this);
        l1.show("Please wait...");
        Log.d("userLogin", "userLogin");
        String LOGIN_URL = BASE_URL + LOGIN.toString();
        valUserID = userName.getEditText().getText().toString();
        valPassWord = passWord.getEditText().getText().toString();
        JSONObject postData = new JSONObject();
        Log.d("valUserID", valUserID);
        Log.d("valPassWord", valPassWord);
        try {
            postData.put("UserName", valUserID);
            postData.put("Password", valPassWord);
        } catch (JSONException e) {
            Log.d("JSONException", e.toString());
            e.printStackTrace();
            Log.d("e", e.toString());
        }
        Log.d("LOGIN_URL", LOGIN_URL);
        JsonObjectRequest jsonObjReq = new JsonObjectRequest(Request.Method.POST, LOGIN_URL, postData,
                new Response.Listener<JSONObject>() {
                    @SuppressLint("SuspiciousIndentation")
                    @Override
                    public void onResponse(JSONObject response) {
                        l1.dismiss();
                        Log.d("Log_url", LOGIN_URL);
                        Log.d("loginresponse", response.toString());
                        try {
                            Log.d("responses", response.toString());
                            JSONArray jsonArrayPS = null;
                            if (response.getString("tkn") != null) {
                                JSONObject jsonObject = new JSONObject(response.toString());
                                jsonArrayPS = new JSONArray(jsonObject.getString("Roles"));
                                for (int i = 0; i < jsonArrayPS.length(); i++) {
                                    JSONObject jsonObjectPS = jsonArrayPS.getJSONObject(i);
                                    role = jsonObjectPS.getString("Id");
                                    programname = jsonObjectPS.getString("ProgramName");
                                    editor.putString("ProgramName", programname);
                                    Log.d("programnamelogin", programname);
                                    System.out.println("My Role : - " + role);
                                    editor.apply();
                                }
                                if ( role.equals("7")) {
                                    if (response.getBoolean("MFA") == true) {
                                        tokenOld = response.getString("tkn");
                                        mobileNumber = response.getString("Mobilenumber");
                                        timeLimit = response.getInt("TimeLimit");
                                        Log.d("Token_login", tokenOld.toString());
                                        editor.putString("UserName", valUserID);
                                        editor.putString("Password", valPassWord);
                                        editor.putString("TokenOld", tokenOld);
                                        editor.putString("MobileNumber", mobileNumber);
                                        editor.putInt("TimeLimit", timeLimit);
                                        editor.putBoolean("loginstatus", true);
                                        editor.apply();
                                        Log.d("MFA1", String.valueOf(response.getBoolean("MFA")));
                                        Intent intent = new Intent(Login.this, OTP.class);
                                        startActivity(intent);
                                    } else {
                                        token = response.getString("tkn");
                                        Log.d("Token_login", token.toString());
                                        editor.putString("UserName", valUserID);
                                        editor.putString("Password", valPassWord);
                                        editor.putString("Token", token);
                                        editor.putBoolean("loginstatus", true);
                                        editor.apply();

// Inside Login after successful login
                                        TokenManager.getInstance(Login.this).retrieveTokenFromServer(token);


                                        Log.d("loginretrvchattoken", "loginretrvchattoken");
                                        Log.d("logintoknapply",pref.getString("Token", null)) ;

                                        FirebaseMessaging.getInstance().subscribeToTopic("videocall");
                                        FirebaseMessaging.getInstance().getToken()
                                                .addOnCompleteListener(new OnCompleteListener<String>() {
                                                    @Override
                                                    public void onComplete(@NonNull Task<String> task) {
                                                        if (!task.isSuccessful()) {
                                                            Log.w(TAG, "Fetching FCM registration token failed", task.getException());
                                                            return;
                                                        }
                                                        // Get new FCM registration token
                                                        firebasetoken = task.getResult();
                                                        Log.d("Firebase get token" , firebasetoken);
                                                        editor.putString("FirebaseToken", firebasetoken);
                                                        editor.apply();
                                                        // Log and toast
                                                        @SuppressLint({"StringFormatInvalid", "LocalSuppress"}) String msg = getString(R.string.msgtokenfmt, token);
                                                        Log.d(TAG, msg);
                                                        Toast.makeText(Login.this, msg, Toast.LENGTH_SHORT).show();

                                                        LoginHelper.firebasetokenInsertion(Login.this, token, firebasetoken);
                                                        LoginHelper.membersList(Login.this, token);

                                                    }
                                                });

                                        Log.d("MFA2", String.valueOf(response.getBoolean("MFA")));


                                        Intent intent = new Intent(Login.this, Home.class);
                                        intent.addFlags(Intent.FLAG_ACTIVITY_NEW_TASK | Intent.FLAG_ACTIVITY_CLEAR_TASK);
                                        startActivity(intent);
                                        finish(); // Login killed


                                        // Inside your login success callback
                                        Log.d("starttimerService", "starttimerService");
                                    }
                                }
                                else
                                {
                                    // Refresh the current page
                                    finish();
                                    startActivity(getIntent());

                                    // Show a toast message
                                    showToastWithCustomColor("Invalid User!");
                                }
                            }
                            else {
                                showToastWithCustomColor("Unauthorized !");
                            }
                        } catch (JSONException jsonException) {
                            jsonException.printStackTrace();
                        }
                    }
                },
                new Response.ErrorListener() {
                    @Override
                    public void onErrorResponse(VolleyError error) {
                        l1.dismiss();
                        Log.d("logine", error.toString());
                        if (error instanceof TimeoutError || error instanceof NoConnectionError) {
                            showToastWithCustomColor("Please check Internet Connection");
                        } else if ( error.networkResponse.statusCode == 503) {
                            Log.d("errorstatusCde", String.valueOf(error.networkResponse.statusCode));
                            showToastWithCustomColor("Number may be invalid! Please contact your careteam.");
                        } else if ( error.networkResponse.statusCode == 401) {
                            Log.d("JSONE statusCoden", String.valueOf(error.networkResponse.statusCode));
                            error.printStackTrace();
                            Log.d("e", error.toString());
                            showToastWithCustomColor("Username or Password may be wrong.");
                        }
                        else if( error.networkResponse.statusCode == 403)
                        {
                            Log.d("Attempts exceeded", error.toString());
                            showToastWithCustomColor("The user is locked! Maximum number of attempts exceeded.");
                            System.out.println("Maximum number of attempts exceeded");
                        }
                        else if( error.networkResponse.statusCode == 404)
                        {
                            Log.d("logine404", error.toString());
                            // Refresh the current page
                            showToastWithCustomColor("Invalid User!");
                            finish();
                            startActivity(getIntent());
                        }
                        else {
                            Log.d("logineelse", error.toString());
                            showToastWithCustomColor("Invalid User!");
                        }
                    }
                });

        //creating a request queue
        RequestQueue requestQueue = Volley.newRequestQueue(Login.this);
        //adding the string request to request queue
        jsonObjReq.setRetryPolicy(new DefaultRetryPolicy(0,-1,DefaultRetryPolicy.DEFAULT_BACKOFF_MULT));
        requestQueue.add(jsonObjReq);
    }

    // Assuming you are in an Activity or a Context instance
    public void showToastWithCustomColor(String msg) {
        LayoutInflater inflater = getLayoutInflater();
        View layout = inflater.inflate(R.layout.custom_toast_layout, null);
        TextView textViewToast = layout.findViewById(R.id.textViewToast);
        textViewToast.setText(msg);
        textViewToast.setTextColor(Color.RED); // Set your desired font color here
        Toast toast = new Toast(getApplicationContext());
        toast.setDuration(Toast.LENGTH_SHORT);
        toast.setView(layout);
        toast.show();
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

    private boolean isNetworkAvailable() {
        ConnectivityManager connectivityManager = (ConnectivityManager) getSystemService(Context.CONNECTIVITY_SERVICE);
        if (connectivityManager != null) {
            NetworkInfo activeNetworkInfo = connectivityManager.getActiveNetworkInfo();
            return activeNetworkInfo != null && activeNetworkInfo.isConnected();
        }
        return false;
    }

    public void logoutOnExpiry(String tokenVALUE) {
        Log.d("logouttimerends", "logouttimerends");
        Context context = ((MyApplication) getApplication()).getLatestActivity();
        SharedPreferences sharedPreferences = context.getSharedPreferences("RPMUserApp", Context.MODE_PRIVATE);
        SharedPreferences.Editor editor = sharedPreferences.edit();
        DataBaseHelper db = new DataBaseHelper(context);

        // Check network connectivity
        if (!isNetworkAvailable()) {
            Log.d("isNetworkAvailable", "no network → do local cleanup");
            unsubscribeFromTopicsOnLogout(context);
            editor.putBoolean("loginstatus", false);
            editor.apply();
            return; // stop here
        }

        // 1⃣ Get Firebase token from SharedPreferences
        String fbtoken = sharedPreferences.getString("FirebaseToken", null);

        if (fbtoken != null) {
            // Delete token from backend first
            firebasetokenDeletion(fbtoken, tokenVALUE, editor, db, context);
        } else {
            // If no Firebase token → directly call logout API
            callLogoutApi(tokenVALUE, editor, db, context);
        }
    }

    private void firebasetokenDeletion(String fbtoken, String tokenVALUE,
                                       SharedPreferences.Editor editor,
                                       DataBaseHelper db,
                                       Context context) {
        String FB_token_delete_URL = BASE_URL + FB_TOK_DELETE; // backend delete endpoint
        RequestQueue requestQueue = Volley.newRequestQueue(context);

        StringRequest stringRequest = new StringRequest(Request.Method.POST, FB_token_delete_URL + fbtoken,
                response -> {
                    Log.e("fbDeletionSuccess", response);
                    // after deletion → call logout API
                    callLogoutApi(tokenVALUE, editor, db, context);
                },
                error -> {
                    Log.e("fbDeletionFailed", error.toString());
                    // even if delete fails → still call logout API
                    callLogoutApi(tokenVALUE, editor, db, context);
                }
        ) {
            @Override
            public Map<String, String> getHeaders() throws AuthFailureError {
                Map<String, String> headers = new HashMap<>();
                headers.put("Bearer", tokenVALUE);
                return headers;
            }
        };

        requestQueue.add(stringRequest);
    }

    private void callLogoutApi(String tokenVALUE,
                               SharedPreferences.Editor editor,
                               DataBaseHelper db,
                               Context context) {
        String LOGOUT_URL = BASE_URL + LOGOUT.toString();

        StringRequest request = new StringRequest(Request.Method.POST, LOGOUT_URL,
                response -> {
                    Log.i("LogoutAPI", "Success: " + response);

                    FirebaseMessaging.getInstance().deleteToken()
                            .addOnCompleteListener(task -> {
                                if (task.isSuccessful()) {
                                    Log.d("LogoutFIREBASE", "FCM token deleted locally");
                                }
                            });

                    editor.putBoolean("loginstatus", false);
                    editor.apply();
                    db.deleteData();
                    db.deleteProfileData("myprofileandprogram");
                },
                error -> {
                    Log.e("LogoutAPI", "Error: " + error.toString());
                    if (error.networkResponse != null && error.networkResponse.statusCode == 401) {
                        editor.putBoolean("loginstatus", false);
                        editor.apply();
                    }
                }) {
            @Override
            public Map<String, String> getHeaders() throws AuthFailureError {
                Map<String, String> headers = new HashMap<>();
                headers.put("Bearer", tokenVALUE);
                headers.put("Content-Type", "application/json");
                return headers;
            }
        };

        RequestQueue requestQueue = Volley.newRequestQueue(context);
        request.setRetryPolicy(new DefaultRetryPolicy(
                60000,
                DefaultRetryPolicy.DEFAULT_MAX_RETRIES,
                DefaultRetryPolicy.DEFAULT_BACKOFF_MULT
        ));
        requestQueue.add(request);
    }

    public static synchronized Login getInstance() {
        if (logininstance == null) {
            logininstance = new Login();
            Log.d("logininstancenew", String.valueOf(logininstance));
        }
        return logininstance;
    }

    private boolean validateUserName() {
        String val_name = userName.getEditText().getText().toString();

        if (val_name.isEmpty()) {
            userName.setError("Username is required");
            return false;
        } else {
            userName.setErrorEnabled(false);
            return true;
        }
    }

    private boolean validateUserPassword() {
        String val_pass = passWord.getEditText().getText().toString();

        if (val_pass.isEmpty()) {
            passWord.setError("Password is required");
            return false;
        } else {
            passWord.setErrorEnabled(false);
            return true;
        }
    }

    public boolean ValidateData(boolean valid) {

        if (!validateUserName() | !validateUserPassword()) {
            return true;
        } else {
            return false;
        }

    }

    @Override
    public void receivedNewMessage(boolean equals) {

    }

    @Override
    public void messageSentCallback() {

    }

    @Override
    public void reloadMessages() {

    }
}