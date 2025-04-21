package com.rpm.clynx.fragments;

import static com.rpm.clynx.utility.Links.BASE_URL;
import static com.rpm.clynx.utility.Links.CALL_REJECT;
import static com.rpm.clynx.utility.Links.FB_TOK_SAVE;
import static com.rpm.clynx.utility.Links.LOGIN;
import static com.rpm.clynx.utility.Links.LOGOUT;
import static com.rpm.clynx.utility.Links.MEMBERS_LIST;
import static com.rpm.clynx.utility.Links.PATIENT_PROFILE;
import static com.rpm.clynx.utility.Links.VIDEOCALL;
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
import com.rpm.clynx.activity.VideoCallTwilio;
import com.rpm.clynx.auth.GenerateOTP;
import com.rpm.clynx.auth.OTP;
import com.rpm.clynx.home.Home;
import com.rpm.clynx.service.NotificationReceiver;
import com.rpm.clynx.utility.ConversationsClientManager;
import com.rpm.clynx.utility.DataBaseHelper;
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
import java.util.Timer;

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
    String valUserID,valPassWord,  toUsername;
    private static final String TAG = "Login";
    String firebasetoken;
    private String title,body;
    AlertDialog alertDialog;
    private Handler handler;
    Activity latestActivity;
    private String videocallToken;
    private final QuickstartConversationsManager quickstartConversationsManager = new QuickstartConversationsManager(this);
    private static Login logininstance;
    String roomnameVal,tokenid, callStatus;
    private boolean isCallJoined = false; // Track whether the user has joined the call
    private BroadcastReceiver notificationReceiver = new BroadcastReceiver() {
        @Override
        public void onReceive(Context context, Intent intent) {
Log.d("loginnotialert","loginnotialert");
            if (intent.getAction().equals(NotificationReceiver.ACTION_NOTIFICATION_RECEIVED)) {
                // Handle the received notification data
                Bundle notificationData = intent.getBundleExtra(NotificationReceiver.EXTRA_NOTIFICATION_DATA);
                title = notificationData.getString("title");
                body = notificationData.getString("body");
                if (body != null) {
                    System.out.println("alert success");
                    // Find the indices of the underscores and "@" symbol
                    int atSymbolIndex = body.indexOf("@");
                    int hashIndex = body.indexOf("#");
                    // Extract the username, room name, and token ID
                    roomnameVal = body.substring(0, atSymbolIndex);
                    tokenid = body.substring(atSymbolIndex + 1, hashIndex);
                    // Extract the value after '#'
                     callStatus = body.substring(hashIndex + 1);
                    Log.d("callbody", body); // It will be either "True" or "False"
                    Log.d("CallStatusfrombody", callStatus); // It will be either "True" or "False"
                    Log.d("roomnameVal",roomnameVal);
                    Log.d("TokenID",tokenid);

                    int firstUnderscoreIndex = body.indexOf('_');
                    if (firstUnderscoreIndex != -1) {
                        int secondUnderscoreIndex = body.indexOf('_', firstUnderscoreIndex + 1);
                        if (secondUnderscoreIndex != -1) {
                            toUsername = body.substring(0, secondUnderscoreIndex);
                            // Use the toUsername as desired
                            Log.d("toUsername", toUsername);
                        } else {
                            // Only one underscore found in the string
                            Log.d("CutString", "Only one underscore found");
                        }
                    } else {
                        // No underscore found in the string
                        Log.d("CutString", "No underscore found");
                    }
                   startAlertTimer(callStatus);
                }
            }
        }
    };

    private BroadcastReceiver logoutReceiver = new BroadcastReceiver() {
        @Override
        public void onReceive(Context context, Intent intent) {
            Log.d("logoutintentfrmlogn", String.valueOf(intent));
            if (intent != null && "com.rpm.clynx.ACTION_LOGOUT_RESULT".equals(intent.getAction())) {
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
    private Timer timer;
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

        // Register the notification receiver
        IntentFilter intentFilter = new IntentFilter(NotificationReceiver.ACTION_NOTIFICATION_RECEIVED);
        LocalBroadcastManager.getInstance(this).registerReceiver(notificationReceiver, intentFilter);

        // Register the logout receiver
        IntentFilter intentFilterlogout = new IntentFilter("com.rpm.clynx.ACTION_LOGOUT_RESULT");
        LocalBroadcastManager.getInstance(this).registerReceiver(logoutReceiver, intentFilterlogout);

        createNotificationChannel(); // Ensure channel exists

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

    public void retrieveTokenFromServer(String appAccessToken) {
        Log.d("appAccessToken", String.valueOf(appAccessToken));
        quickstartConversationsManager.retrieveAccessTokenFromServer(getApplicationContext(), appAccessToken, new TokenResponseListener() {
            @Override
            public void receivedTokenResponse(boolean success, @Nullable Exception exception) {
                if (success) {
                    runOnUiThread(new Runnable() {
                        @Override
                        public void run() {
                            // need to modify user interface elements on the UI thread
                            Log.d("success", String.valueOf(success));
                        }
                    });
                }
                else {
                    String errorMessage = getString(R.string.error_retrieving_access_token);
                    if (exception != null) {
                        errorMessage = errorMessage + " " + exception.getLocalizedMessage();
                    }
                    Toast.makeText(Login.this,
                                    errorMessage,
                                    Toast.LENGTH_LONG)
                            .show();
                }
            }
        });
    }

    private BroadcastReceiver newTokenReceiver = new BroadcastReceiver() {
        @Override
        public void onReceive(Context context, Intent intent) {
            // Extract the token from the intent
            String refreshedtoken = intent.getStringExtra("onNewToken");
            // Call the method in the activity to save the new token to the server
            firebasetokenInsertion(refreshedtoken);
        }
    };

    private void startAlertTimer(String callStatus) {
        Log.d("callStatus",callStatus);
        latestActivity = ((MyApplication) getApplication()).getLatestActivity();
        System.out.println("latestActivity: " + latestActivity);

        // If callStatus is "False" and alert is displayed, dismiss it
        if ("False".equalsIgnoreCase(callStatus)) {
            if (alertDialog != null && alertDialog.isShowing()) {
                alertDialog.dismiss();
                System.out.println("Call status is False, alert dismissed.");
            }

            return; // Exit the function early
        }

        // If callStatus is "True", show the alert
        AlertDialog.Builder alertDialogBuilder = new AlertDialog.Builder(latestActivity);
        alertDialogBuilder.setTitle(title);
        alertDialogBuilder.setMessage("Do you want to join?");
        alertDialogBuilder.setCancelable(false);

        // Set the positive button
        alertDialogBuilder.setPositiveButton("Yes", (dialog, which) -> {
            System.out.println("VIDEO activity: " + latestActivity);
            isCallJoined = true; // Mark as joined
            videoCall(roomnameVal, title, latestActivity);
            dialog.dismiss();
        });

        // Set the negative button
        alertDialogBuilder.setNegativeButton("No", (dialog, which) -> {
            Toast.makeText(latestActivity, "No clicked", Toast.LENGTH_SHORT).show();
            callReject(latestActivity);
            dialog.dismiss();
        });

        // Create and show the AlertDialog
        alertDialog = alertDialogBuilder.create();
        alertDialog.show();

        // Auto-dismiss after 2 minutes
        new Handler().postDelayed(() -> {
            if (alertDialog != null && alertDialog.isShowing()) {
                alertDialog.dismiss();
            }
        }, 120000); // 2 minutes (120,000 milliseconds)
    }

    private void videoCall(String roomname, String caller, Activity latstActvty)  {
        String VIDEOCALL_URL = BASE_URL+ VIDEOCALL ;
        System.out.println("roomname");
        System.out.println(roomname);
        System.out.println("VIDEOCALL_URL");
        System.out.println(VIDEOCALL_URL);
        System.out.println("VIDEO lateactity");
        System.out.println(latstActvty);

        StringRequest stringRequest = new StringRequest(Request.Method.GET, VIDEOCALL_URL + roomname ,
                new Response.Listener<String>() {
                    @Override
                    public void onResponse(String response) {
                        // Handle the API response
                        System.out.println("  VIDEOCALL response");
                        System.out.println(response);
                        System.out.println("  VIDEOCALL latestActivity");
                        System.out.println(latstActvty);
                        videocallToken = response;
                        Intent intent = new Intent(latstActvty, VideoCallTwilio.class);
                        intent.putExtra("videocallToken", videocallToken);
                        intent.putExtra("videocallRoomname", roomname);
                        intent.putExtra("videocallCallername", caller);
                        startActivity(intent);
                    }
                },
                new Response.ErrorListener() {
                    @Override
                    public void onErrorResponse(VolleyError error) {
                        // Handle network or API errors
                        System.out.println("VolleyError");
                        Log.e("BooleanonErrorResponse", error.toString());
                    }
                }
        )

        {
            @Override
            public Map<String, String> getHeaders() throws AuthFailureError {
                Map<String, String> headers = new HashMap<>();
                headers.put("Bearer",pref.getString("Token", null));
                Log.d("Token_videocall", pref.getString("Token", null));
                //  headers.put("Content-Type", "application/json");
                //  headers.put("Authorization", "Bearer" + Token);
                return headers;
            }
        };

// Add the request to the request queue
        RequestQueue requestQueue = Volley.newRequestQueue(getApplicationContext());
        stringRequest.setRetryPolicy(new DefaultRetryPolicy(60000, DefaultRetryPolicy.DEFAULT_MAX_RETRIES, DefaultRetryPolicy.DEFAULT_BACKOFF_MULT));
        requestQueue.add(stringRequest);
    }

    private void callReject(Activity latstactvty)  {
        Log.d("toUsername", toUsername);
        Log.d("tokenid", tokenid);
        String CALL_REJECT_URL = BASE_URL+ CALL_REJECT +
                toUsername + "&tokenid="+ tokenid;
        System.out.println("CALL_REJECT_URL");
        System.out.println(CALL_REJECT_URL);
        String body =  "Rejected your Call";
        JSONObject parameters = new JSONObject();
        try{
            parameters.put("Title", "");
            parameters.put("Body", body);
            System.out.println("Formed String is-->"+parameters);
            System.out.println("Formed CALL_REJECT_URL is-->"+CALL_REJECT_URL);
        } catch (JSONException e) {
            e.printStackTrace();
        }

        StringRequest stringRequest = new StringRequest(Request.Method.POST, CALL_REJECT_URL,
                new Response.Listener<String>() {
                    @Override
                    public void onResponse(String response) {
                        Log.d("rejectcallResponse", response.toString());
                        Log.d("Log_url", CALL_REJECT_URL);
                        Toast.makeText(latstactvty, "Call Rejected , Notification Sent Successfully!", Toast.LENGTH_LONG).show();
                    }
                },
                new Response.ErrorListener() {
                    @Override
                    public void onErrorResponse(VolleyError error) {
                        Log.e("onErrorResponse", error.toString());
                        Toast.makeText(latstactvty, "Something went wrong!", Toast.LENGTH_SHORT).show();
                    }
                }
        )
        {

            @Override
            public String getBodyContentType() {
                return "application/json";
            }

            @Override
            public byte[] getBody() throws AuthFailureError {
                return parameters.toString().getBytes();
            }
            @Override
            public Map<String, String> getHeaders() throws AuthFailureError {
                Map<String, String> headers = new HashMap<>();
                headers.put("Bearer",pref.getString("Token", null));
                Log.d("Token_callReject", pref.getString("Token", null));
                return headers;
            }
        };

        RequestQueue requestQueue = Volley.newRequestQueue(latstactvty);
        //adding the string request to request queue
        stringRequest.setRetryPolicy(new DefaultRetryPolicy(0, -1, DefaultRetryPolicy.DEFAULT_BACKOFF_MULT));
        requestQueue.add(stringRequest);
    }


    @Override
    protected void onDestroy() {
        super.onDestroy();
        // Unregister the notification receiver
        stopTimer();
    }
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

    private void stopTimer() {
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.Q) {
            if (timer != null) {
                timer.cancel();
                timer = null;
            }
        }
    }

    private void userDetails() {
        Runnable objRunnable = new Runnable() {
            @Override
            public void run() {
                //userLogin();
                getpatientprofile();
                try {
                    Thread.sleep(60000);
                }catch (Exception e){
                    e.printStackTrace();
                }
            }
        };
        Thread objBgThread = new Thread(objRunnable);
        objBgThread.start();
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

                                        // Inside your login success callback
                                        retrieveTokenFromServer(token);

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

                                                        firebasetokenInsertion(firebasetoken);

                                                        membersList();
                                                    }
                                                });

                                        Log.d("MFA2", String.valueOf(response.getBoolean("MFA")));
                                        Intent intent = new Intent(Login.this, Home.class);
                                        startActivity(intent);

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
                            // Refresh the current page
                            showToastWithCustomColor("Invalid User!");
                            finish();
                            startActivity(getIntent());
                        }
                        else {
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
        Log.d("context", String.valueOf((((MyApplication) getApplication()).getLatestActivity())));
        SharedPreferences sharedPreferences = (((MyApplication) getApplication()).getLatestActivity()).getSharedPreferences("RPMUserApp", Context.MODE_PRIVATE);
        SharedPreferences.Editor editor = sharedPreferences.edit();
        DataBaseHelper db = new DataBaseHelper((((MyApplication) getApplication()).getLatestActivity()));

        // Check network connectivity
        if (!isNetworkAvailable()) {
            Log.d("isNetworkAvailable", "isNetworkAvailable");
            unsubscribeFromTopicsOnLogout((((MyApplication) getApplication()).getLatestActivity()));
            editor.putBoolean("loginstatus", false);
            editor.apply();
            // Display a toast message indicating poor or no network connectivity
            unsubscribeFromTopicsOnLogout((((MyApplication) getApplication()).getLatestActivity()));
        }
else
        {
            Log.d("notisNetworkAvailable", "notisNetworkAvailable");
            String LOGOUT_URL = BASE_URL + LOGOUT.toString();
            JSONObject parameters = new JSONObject();
        try {
            parameters.put("Bearer", tokenVALUE);
        } catch (Exception e) {
        }
        StringRequest request = new StringRequest(Request.Method.POST, LOGOUT_URL, new Response.Listener<String>() {
            @Override
            public void onResponse(String response) {
                Log.d("loginstsfrmlogn1", String.valueOf(pref.getBoolean("loginstatus", false)));
                Log.i("onResponse", response.toString());
                editor.putBoolean("loginstatus", false);
                editor.apply();
                Log.d("loginstsfrmlogn2", String.valueOf(pref.getBoolean("loginstatus", false)));
            }
        }, new Response.ErrorListener() {
            @Override
            public void onErrorResponse(VolleyError error) {
                Log.e("onErrorResponse", error.toString());
                if ( error.networkResponse.statusCode == 401) {
                    Log.d("JSONE statusCoden", String.valueOf(error.networkResponse.statusCode));
                    error.printStackTrace();
                    Log.d("e", error.toString());
                    editor.putBoolean("loginstatus", false);
                    editor.apply();
                }
            }
        }) {
            @Override
            public Map<String, String> getHeaders() throws AuthFailureError {
                Map<String, String> headers = new HashMap<>();
                headers.put("Bearer", tokenVALUE);
                headers.put("Content-Type", "application/json");
                Log.d("headers", headers.toString());
                return headers;
            }
        };
        Log.d("contextgetApplicationContext", String.valueOf(((MyApplication) getApplication()).getLatestActivity()));
        RequestQueue requestQueue = Volley.newRequestQueue(((MyApplication) getApplication()).getLatestActivity());
        request.setRetryPolicy(new DefaultRetryPolicy(60000, DefaultRetryPolicy.DEFAULT_MAX_RETRIES, DefaultRetryPolicy.DEFAULT_BACKOFF_MULT));
        requestQueue.add(request);
    }
    }
    public static synchronized Login getInstance() {
        if (logininstance == null) {
            logininstance = new Login();
            Log.d("logininstancenew", String.valueOf(logininstance));
        }
        return logininstance;
    }

    public static void unsubscribeFirebaseOnLogout(Context context) {
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

    private void membersList() {
        String MEMBERS_LIST_URL = BASE_URL + MEMBERS_LIST;
        StringRequest stringRequest = new StringRequest(Request.Method.GET, MEMBERS_LIST_URL,
                new Response.Listener<String>() {
                    @Override
                    public void onResponse(String response) {
                        Log.d("MEMBERS_LIST_URL", MEMBERS_LIST_URL);
                        Log.d("membersListresponse", response.toString());

                        saveMembersList(response);
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
                headers.put("Bearer",token);
                Log.d("headers_myprofileandprogram",headers.toString());
                Log.d("Token_myprofileandprogram", token);
                return headers;
            }
        };

        //creating a request queue
        RequestQueue requestQueue = Volley.newRequestQueue(Login.this);
        //adding the string request to request queue
        stringRequest.setRetryPolicy(new DefaultRetryPolicy(0,-1,DefaultRetryPolicy.DEFAULT_BACKOFF_MULT));
        requestQueue.add(stringRequest);
    }

    private void saveMembersList(String response) {
        SharedPreferences sharedPreferences = getSharedPreferences("RPMUserApp", MODE_PRIVATE);
        SharedPreferences.Editor editor = sharedPreferences.edit();
        editor.putString("members_list", response);
        editor.apply(); // Saves asynchronously
        Log.d("SharedPreferences", "Members list saved successfully");
    }
    private void firebasetokenInsertion(String fbtoken)  {
        String FB_token_save_URL = BASE_URL+ FB_TOK_SAVE ;
        RequestQueue requestQueue = Volley.newRequestQueue(getApplicationContext());
        StringRequest stringRequest = new StringRequest(Request.Method.POST, FB_token_save_URL + fbtoken,
                new Response.Listener<String>() {
                    @Override
                    public void onResponse(String response) {
                        // Handle the API response
                        boolean success = Boolean.parseBoolean(response);
                        // Process the boolean value as needed
                        System.out.println("fbtokinsrtsuccess");
                        System.out.println(success);
                    }
                },
                new Response.ErrorListener() {
                    @Override
                    public void onErrorResponse(VolleyError error) {
                        // Handle network or API errors
                        System.out.println("VolleyError");
                        Log.e("BooleanonErrorResponse", error.toString());
                    }
                }
        )
        {
            @Override
            public Map<String, String> getHeaders() throws AuthFailureError {
                Map<String, String> headers = new HashMap<>();
                headers.put("Bearer",token);
                return headers;
            }
        };

// Add the request to the request queue
        requestQueue.add(stringRequest);
    }

    private void getpatientprofile() {
        String Token = pref.getString("Token", null);
        Log.d("Token_profile", Token);
        String PROFILE_URL = BASE_URL + PATIENT_PROFILE.toString();
        JSONObject parameters = new JSONObject();
        try {
            parameters.put("Bearer", token);
        } catch (Exception e) {
        }
        Log.d("Bearer", parameters.toString());
        StringRequest request = new StringRequest(Request.Method.POST, PROFILE_URL,new Response.Listener<String>() {
            @Override
            public void onResponse(String response) {
                Log.i("onResponse", response.toString());
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
                headers.put("Bearer",token);
                Log.d("headers",headers.toString());
                Log.d("Token_profile", token);
                return headers;
            }
        };
        RequestQueue requestQueue = Volley.newRequestQueue(Login.this);
        request.setRetryPolicy(new DefaultRetryPolicy(60000, DefaultRetryPolicy.DEFAULT_MAX_RETRIES, DefaultRetryPolicy.DEFAULT_BACKOFF_MULT));
        requestQueue.add(request);
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