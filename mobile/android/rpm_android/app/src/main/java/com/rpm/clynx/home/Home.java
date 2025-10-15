package com.rpm.clynx.home;
import static com.rpm.clynx.utility.Links.BASE_URL;
import static com.rpm.clynx.utility.Links.CALL_REJECT;
import static com.rpm.clynx.utility.Links.GET_MY_PROFILE;
import static com.rpm.clynx.utility.Links.PATIENT_PROFILE;
import static com.rpm.clynx.utility.Links.VIDEOCALL;

import com.rpm.clynx.R;

import android.app.Activity;
import android.content.BroadcastReceiver;
import android.content.Intent;
import android.content.IntentFilter;
import android.content.SharedPreferences;
import android.net.ConnectivityManager;
import android.net.NetworkInfo;
import android.os.Build;
import android.os.Bundle;
import android.os.Looper;
import android.util.Log;
import android.view.MenuItem;
import androidx.annotation.NonNull;
import androidx.appcompat.app.AlertDialog;
import androidx.appcompat.app.AppCompatActivity;
import androidx.fragment.app.Fragment;
import androidx.fragment.app.FragmentManager;
import androidx.fragment.app.FragmentTransaction;
import com.android.volley.AuthFailureError;
import com.android.volley.DefaultRetryPolicy;
import com.android.volley.Request;
import com.android.volley.RequestQueue;
import com.android.volley.Response;
import com.android.volley.VolleyError;
import com.android.volley.toolbox.StringRequest;
import com.android.volley.toolbox.Volley;
import com.google.android.material.bottomnavigation.BottomNavigationView;
import com.rpm.clynx.activity.VideoCallTwilio;
import com.rpm.clynx.fragments.DashboardFragment;
import com.rpm.clynx.fragments.Login;
import com.rpm.clynx.fragments.MoreFragment;
import com.rpm.clynx.fragments.ToDoListFragment;
import com.rpm.clynx.fragments.VitalHealthTrends;
import com.rpm.clynx.utility.DataBaseHelper;
import com.rpm.clynx.utility.FileLogger;
import com.rpm.clynx.utility.MyApplication;
import com.rpm.clynx.utility.SystemBarColor;
import org.json.JSONException;
import org.json.JSONObject;
import java.util.HashMap;
import java.util.Map;
import java.util.Timer;

import android.Manifest;
import android.content.Context;
import android.content.pm.PackageManager;
import android.os.Handler;
import android.widget.Toast;
import androidx.core.app.ActivityCompat;
import androidx.core.content.ContextCompat;
import androidx.localbroadcastmanager.content.LocalBroadcastManager;

public class Home extends AppCompatActivity {

    DataBaseHelper db;
    SharedPreferences pref;
    SharedPreferences.Editor editor;
    String Token;
    private BottomNavigationView navigation;
    private String ProgramName;
    private static final int CAMERA_MIC_PERMISSION_REQUEST_CODE = 1;
    private Handler handler;
    boolean loginstatusHome;

    private String title,body;
    String roomnameVal,tokenid, callStatus, toUsername;
    private boolean dashboardLoaded = false;
    Activity latestActivity;
    AlertDialog alertDialog;
    private boolean isCallJoined = false;
    private Handler alertHandler;
    private Runnable alertTimeoutRunnable;
    private String videocallToken;
    private Timer timer;
    private BroadcastReceiver notificationReceiver = new BroadcastReceiver() {
        @Override
        public void onReceive(Context context, Intent intent) {
            Log.d("loginnotialert", "loginnotialert");
            FileLogger.d("videologinnotialert", "loginnotialert");

            String type = intent.getStringExtra("type");
            if ("video".equals(type)) {
                FileLogger.d("notificationReceiver1", "ACTION_NOTIFICATION_RECEIVED received");

                // Handle the received notification data
                // Get the notification data
                Bundle notificationData = intent.getBundleExtra("notificationData");
               // Bundle notificationData = intent.getBundleExtra(NotificationReceiver.EXTRA_NOTIFICATION_DATA);
                if (notificationData == null) {
                    Log.d("notificationReceiver", "notificationData is null");
                    FileLogger.d("notificationReceiver2", "notificationData is null");
                    return;
                }

                title = notificationData.getString("title");
                body = notificationData.getString("body");

                Log.d("notificationReceiver", "Notification title: " + title);
                Log.d("notificationReceiver", "Notification body: " + body);
                FileLogger.d("notificationReceiver3", "Notification title: " + title);
                FileLogger.d("notificationReceiver4", "Notification body: " + body);

                if (body != null) {
                    FileLogger.d("notificationReceiver5", "alert success");

                    int atSymbolIndex = body.indexOf("@");
                    int hashIndex = body.indexOf("#");

                    if (atSymbolIndex != -1 && hashIndex != -1 && atSymbolIndex < hashIndex) {
                        roomnameVal = body.substring(0, atSymbolIndex);
                        tokenid = body.substring(atSymbolIndex + 1, hashIndex);
                        callStatus = body.substring(hashIndex + 1);

                        Log.d("callbody", body);
                        Log.d("CallStatusfrombody", callStatus);
                        Log.d("roomnameVal", roomnameVal);
                        Log.d("TokenID", tokenid);

                        FileLogger.d("notificationReceiver6", "callbody: " + body);
                        FileLogger.d("notificationReceiver7", "CallStatusfrombody: " + callStatus);
                        FileLogger.d("notificationReceiver8", "roomnameVal: " + roomnameVal);
                        FileLogger.d("notificationReceiver9", "TokenID: " + tokenid);

                        int firstUnderscoreIndex = body.indexOf('_');
                        if (firstUnderscoreIndex != -1) {
                            int secondUnderscoreIndex = body.indexOf('_', firstUnderscoreIndex + 1);
                            if (secondUnderscoreIndex != -1) {
                                toUsername = body.substring(0, secondUnderscoreIndex);
                                Log.d("toUsername", toUsername);
                                FileLogger.d("notificationReceiver10", "toUsername: " + toUsername);
                            } else {
                                Log.d("CutString", "Only one underscore found");
                                FileLogger.d("notificationReceiver11", "Only one underscore found");
                            }
                        } else {
                            Log.d("CutString", "No underscore found");
                            FileLogger.d("notificationReceiver12", "No underscore found");
                        }

                        FileLogger.d("notificationReceiver13", "Calling startAlertTimer with callStatus: " + callStatus);
                        startAlertTimer(callStatus);
                    } else {
                        Log.d("notificationReceiver14", "Invalid body format: missing '@' or '#' or incorrect order");
                        FileLogger.d("notificationReceiver14", "Invalid body format: missing '@' or '#' or incorrect order");
                    }
                } else {
                    Log.d("notificationReceiver", "Notification body is null");
                    FileLogger.d("notificationReceiver15", "Notification body is null");
                }

        }
        }
    };


    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_home);
        SystemBarColor.setSystemBarColor(this, R.color.Primary_bg);
        navigation = findViewById(R.id.navigation);
        db = new DataBaseHelper(this);
        pref = getApplicationContext().getSharedPreferences("RPMUserApp", MODE_PRIVATE);
        editor = pref.edit();
        ProgramName = pref.getString("ProgramName", null);
        initComponent();

        Token = pref.getString("Token", null);
        loginstatusHome = pref.getBoolean("loginstatus", false);

        // Initialize the handler
        handler = new Handler();

        if (!checkPermissionForCameraAndMicrophone()) {
            requestPermissionForCameraMicrophoneAndBluetooth();
        }
        if (!isNetworkAvailable()) {
            // Display a toast message indicating poor or no network connectivity
            Toast.makeText(this, "Poor network connection or mobile data is turned off", Toast.LENGTH_SHORT).show();
        } else {

            //  Immediately load DashboardFragment with cached data
            replaceFragment(new DashboardFragment());

            getmyprofileandprogram();
            getpatientprofile(() -> {
                Log.d("Home", "Profile fetched, DB updated");
                // any UI updates or other actions
            });

        }
    }
    @Override
    protected void onStart() {
        super.onStart();
        IntentFilter filter = new IntentFilter("com.rpm.clynx.NOTIFICATION_RECEIVED");
        LocalBroadcastManager.getInstance(this).registerReceiver(notificationReceiver, filter);

    }

    @Override
    protected void onStop() {
        super.onStop();
        LocalBroadcastManager.getInstance(this).unregisterReceiver(notificationReceiver);
        stopTimer();
        cancelAlertTimer();
        safelyDismissAlertDialog();
    }
    @Override
    protected void onResume() {
        super.onResume();

        Log.d("homeonResume","homeonResume");
        // Check network connectivity
        if (!isNetworkAvailable() && (pref.getBoolean("loginstatus", false) == false)) {
            // Display a toast message indicating poor or no network connectivity
            Toast.makeText(this, "Poor network connection or mobile data is turned off", Toast.LENGTH_SHORT).show();
            Log.d("loginstsfrmhome1111", String.valueOf(pref.getBoolean("loginstatus", false)));
            Log.d("loginstsfrmhome2", String.valueOf(pref.getBoolean("loginstatus", false)));
            db.deleteProfileData("myprofileandprogram");
            db.deleteData();
            editor.clear();
            editor.commit();

            try {
                Log.d("loginlatestActivity", String.valueOf(((MyApplication) getApplication()).getLatestActivity()));
                Intent intentlogout = new Intent((((MyApplication) getApplication()).getLatestActivity()), Login.class);
                intentlogout.addFlags(Intent.FLAG_ACTIVITY_NEW_TASK | Intent.FLAG_ACTIVITY_CLEAR_TASK);
                startActivity(intentlogout);
            }catch (Exception e)
            {
                Log.e("onLogOff Clear", e.toString());
            }
        }
    }
    @Override
    protected void onDestroy() {
        super.onDestroy();
        // Unregister the notification receiver
        FileLogger.d("onDestroy", "unregisternotificationReceiver");
        stopTimer();
        cancelAlertTimer();
        safelyDismissAlertDialog();
    }

    private void stopTimer() {
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.Q) {
            if (timer != null) {
                timer.cancel();
                timer = null;
            }
        }
    }
    private void startAlertTimer(String callStatus) {
        //  Log.d("startAlertTimer", "Invoked with callStatus: " + callStatus);
        FileLogger.d("startAlertTimer", "Invoked with callStatus: " + callStatus);

        latestActivity = ((MyApplication) getApplication()).getLatestActivity();
        // Log.d("startAlertTimer", "latestActivity: " + latestActivity);
        FileLogger.d("startAlertTimer", "latestActivity: " + latestActivity);

        if (latestActivity == null || latestActivity.isFinishing() || latestActivity.isDestroyed()) {
            Log.e("startAlertTimer", "latestActivity is invalid. Cannot show dialog.");
            FileLogger.e("startAlertTimer", "latestActivity is invalid. Cannot show dialog.");
            return;
        }

        latestActivity.runOnUiThread(() -> {
            if ("False".equalsIgnoreCase(callStatus)) {
                // Log.d("startAlertTimer", "callStatus is False. Checking existing alertDialog...");
                FileLogger.d("startAlertTimer", "callStatus is False. Checking existing alertDialog...");

                if (alertDialog != null && alertDialog.isShowing()) {
                    FileLogger.d("startAlertTimer", "alertDialog is showing. Dismissing.");
                    safelyDismissAlertDialog();
                    FileLogger.d("startAlertTimer", "alertDialog dismissed.");
                }
                return;
            }

            if (alertDialog != null) {
                safelyDismissAlertDialog(); // Dismiss previous alert
            }

            FileLogger.d("startAlertTimer", "Preparing to show new alert dialog.");
            AlertDialog.Builder alertDialogBuilder = new AlertDialog.Builder(latestActivity);
            alertDialogBuilder.setTitle(title);
            alertDialogBuilder.setMessage("Do you want to join?");
            alertDialogBuilder.setCancelable(false);

            alertDialogBuilder.setPositiveButton("Yes", (dialog, which) -> {
                FileLogger.d("AlertDialog", "User clicked YES");

                cancelAlertTimer();
                safelyDismissAlertDialog();

                isCallJoined = true;

                videoCall(roomnameVal, title, latestActivity);
            });

            alertDialogBuilder.setNegativeButton("No", (dialog, which) -> {
                FileLogger.d("AlertDialog", "User clicked NO");

                cancelAlertTimer();
                safelyDismissAlertDialog();


                Toast.makeText(latestActivity, "No clicked", Toast.LENGTH_SHORT).show();
                callReject(latestActivity);
            });


            try {
                alertDialog = alertDialogBuilder.create();
                alertDialog.show();
                //  Log.d("startAlertTimer", "Alert dialog shown.");
                FileLogger.d("startAlertTimer", "Alert dialog shown.");
            } catch (Exception e) {
              //  Log.e("startAlertTimer", "Exception while showing alert dialog", e);
                FileLogger.e("startAlertTimer", "Exception while showing alert dialog: " + e.getMessage());
            }

            // Start 2-minute timeout only if no interaction
            startAlertAutoDismissTimer();
        });
    }

    private void startAlertAutoDismissTimer() {
        // Always cancel previous timer first
        cancelAlertTimer();

        alertTimeoutRunnable = () -> {
            FileLogger.d("AlertTimerCheck", "Runnable executed");
          //  Log.d("AlertTimer", "2-minute timeout reached. Auto-dismissing alert.");
            FileLogger.d("AlertTimer", "2-minute timeout reached. Auto-dismissing alert.");
            safelyDismissAlertDialog();
        };

        // Keep handler reference for cancel
        alertHandler = new Handler(Looper.getMainLooper());
        alertHandler.postDelayed(alertTimeoutRunnable, 120000); // 2 minutes
    }

    private void cancelAlertTimer() {
        if (alertHandler != null && alertTimeoutRunnable != null) {
            alertHandler.removeCallbacks(alertTimeoutRunnable);
        }
    }


    private void safelyDismissAlertDialog() {
        // Always run on UI thread
        if (Looper.myLooper() == Looper.getMainLooper()) {
            dismissDialogInternal();
        } else {
            new Handler(Looper.getMainLooper()).post(this::dismissDialogInternal);
        }
    }

    private void dismissDialogInternal() {
        if (alertDialog != null) {
            try {
                if (alertDialog.isShowing()) {
                    alertDialog.dismiss();
                    Log.d("AlertDialog", "Dialog dismissed successfully.");
                    FileLogger.d("AlertDialog", "Dialog dismissed successfully.");
                } else {
                    Log.d("AlertDialog", "Dialog was not showing.");
                    FileLogger.d("AlertDialog", "Dialog was not showing.");
                }
            } catch (Exception e) {
                Log.e("AlertDialog", "Exception during dismiss: " + e.getMessage());
                FileLogger.e("AlertDialog", "Exception during dismiss: " + e.getMessage());
            } finally {
                alertDialog = null; // very important
            }
        } else {
            Log.d("AlertDialog", "alertDialog is already null.");
            FileLogger.d("AlertDialog", "alertDialog is already null.");
        }
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
        String body =  "Rejected your Call";
        JSONObject parameters = new JSONObject();
        try{
            parameters.put("Title", "");
            parameters.put("Body", body);
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

    private boolean isNetworkAvailable() {
        ConnectivityManager connectivityManager = (ConnectivityManager) getSystemService(Context.CONNECTIVITY_SERVICE);
        if (connectivityManager != null) {
            NetworkInfo activeNetworkInfo = connectivityManager.getActiveNetworkInfo();
            return activeNetworkInfo != null && activeNetworkInfo.isConnected();
        }
        return false;
    }

    private boolean checkPermissionForCameraAndMicrophone() {
        return checkPermissions(
                new String[] {Manifest.permission.CAMERA, Manifest.permission.RECORD_AUDIO});
    }
    private boolean checkPermissions(String[] permissions) {
        boolean shouldCheck = true;
        for (String permission : permissions) {
            shouldCheck &=
                    (PackageManager.PERMISSION_GRANTED
                            == ContextCompat.checkSelfPermission(this, permission));
        }
        return shouldCheck;
    }
    private void requestPermissionForCameraMicrophoneAndBluetooth() {
        String[] permissionsList;
        if (android.os.Build.VERSION.SDK_INT >= android.os.Build.VERSION_CODES.S) {
            permissionsList =
                    new String[] {
                            Manifest.permission.CAMERA,
                            Manifest.permission.RECORD_AUDIO,
                            Manifest.permission.BLUETOOTH_CONNECT
                    };
            Log.d("permissionsList11", String.valueOf(permissionsList));
        } else {
            permissionsList =
                    new String[] {Manifest.permission.CAMERA, Manifest.permission.RECORD_AUDIO};
            Log.d("permissionsList22", String.valueOf(permissionsList));
            Log.d("permissionsList22", String.valueOf(permissionsList.toString()));
        }
        requestPermissions(permissionsList);
    }
    private void requestPermissions(String[] permissions) {
        boolean displayRational = false;
        for (String permission : permissions) {
            displayRational |=
                    ActivityCompat.shouldShowRequestPermissionRationale(this, permission);
        }
        if (displayRational) {
            Toast.makeText(this, R.string.permissions_needed, Toast.LENGTH_LONG).show();
        } else {
            ActivityCompat.requestPermissions(
                    this, permissions, CAMERA_MIC_PERMISSION_REQUEST_CODE);
        }
    }


    @Override
    public void onBackPressed() {
        FragmentManager fm = getSupportFragmentManager();

        // If there are fragments in the back stack, pop the last one
        if (fm.getBackStackEntryCount() > 0) {
            fm.popBackStack();
        } else {
            // No fragments in back stack, check if current is DashboardFragment
            Fragment currentFragment = fm.findFragmentById(R.id.fl_main);
            if (!(currentFragment instanceof DashboardFragment)) {
                // Navigate to DashboardFragment
                navigation.setSelectedItemId(R.id.Home);
            } else {
                // Already in DashboardFragment, exit app
                super.onBackPressed();
            }
        }
    }

    private void getmyprofileandprogram() {
        String PROFILE_URL = BASE_URL + GET_MY_PROFILE.toString();
        StringRequest request = new StringRequest(Request.Method.GET, PROFILE_URL,
                new Response.Listener<String>() {
                    @Override
                    public void onResponse(String response) {
                        Log.i("onResponse_myprofileandprogram", response);

                        try {
                            JSONObject obj = new JSONObject(response);

                            String newFullName = obj.getString("Name");
                            String newPatentId = obj.getString("UserName");
                            String newProgramTypeName = obj.getString("ProgramName");
                            String newCompletedDuration = obj.getString("CompletedDuration");
                            String newStatus = obj.getString("Status");

                            //  Get old values BEFORE saving new ones
                            String oldStatus = pref.getString("Status", "");

                            //  Save new values
                            editor.putString("User_full_name", newFullName);
                            editor.putString("Patent_id", newPatentId);
                            editor.putString("ProgramTypeName", newProgramTypeName);
                            editor.putString("CompletedDuration", newCompletedDuration);
                            editor.putString("Status", newStatus);
                            editor.apply();

                            db.insertProfileData(response);

                            //  Determine when to refresh Dashboard
                            if (!dashboardLoaded) {
                                // First time loading Home activity → always load Dashboard
                                replaceFragment(new DashboardFragment());
                                dashboardLoaded = true;
                                Log.d("DashboardRefresh", "Dashboard loaded first time");
                            } else if (!newStatus.equals(oldStatus)) {
                                // Subsequent calls → refresh only if Status changed
                                replaceFragment(new DashboardFragment());
                                Log.d("DashboardRefresh", "Dashboard refreshed due to Status change");
                            } else {
                                Log.d("DashboardRefresh", "No change in Status, skipping refresh");
                            }

                        } catch (JSONException e) {
                            e.printStackTrace();
                        }
                    }
                },
                new Response.ErrorListener() {
                    @Override
                    public void onErrorResponse(VolleyError error) {
                        Log.e("onErrorResponse", error.toString());

                        if (error.networkResponse != null && error.networkResponse.statusCode == 401) {
                            error.printStackTrace();

                            //  Clear SharedPreferences cleanly
                            editor.clear();
                            editor.apply();

                            db.deleteProfileData("myprofileandprogram");
                            db.deleteData();

                            try {
                                Intent intentLogout = new Intent(((MyApplication) getApplication()).getLatestActivity(), Login.class);
                                intentLogout.addFlags(Intent.FLAG_ACTIVITY_NEW_TASK | Intent.FLAG_ACTIVITY_CLEAR_TASK);
                                startActivity(intentLogout);
                            } catch (Exception e) {
                                Log.e("onLogOff Clear", e.toString());
                            }
                        } else {
                            Log.d("statusCodenull", "statusCodenull");
                        }
                    }
                }) {
            @Override
            public Map<String, String> getHeaders() throws AuthFailureError {
                Map<String, String> headers = new HashMap<>();
                headers.put("Bearer", Token);
                return headers;
            }
        };

        RequestQueue requestQueue = Volley.newRequestQueue(Home.this);
        request.setRetryPolicy(new DefaultRetryPolicy(
                60000,
                DefaultRetryPolicy.DEFAULT_MAX_RETRIES,
                DefaultRetryPolicy.DEFAULT_BACKOFF_MULT));
        requestQueue.add(request);
    }


    private void initComponent() {
        Log.d("initPgmName", ProgramName);
        if (ProgramName != null) {
            Log.d("initProgramName", ProgramName);
            Log.d("notnullpgm", "notnullpgm");
            if ( ProgramName.equals("RPM")) {
                // ProgramName is not null and equals "RPM," so the "Vitals" item should be visible
                navigation.getMenu().findItem(R.id.Vital).setVisible(true);
                Log.d("ProgramNameisnotnull", "ProgramNameisnotnull");
            } else {
                Log.d("ProgramNameisnull", "ProgramNameisnull");
                // ProgramName is null or not equal to "RPM," so hide the "Vitals" item
                navigation.getMenu().findItem(R.id.Vital).setVisible(false);
            }
        }

        navigation.setOnNavigationItemSelectedListener(new BottomNavigationView.OnNavigationItemSelectedListener() {
            @Override
            public boolean onNavigationItemSelected(@NonNull MenuItem item) {
                int itemId = item.getItemId();
                if (itemId == R.id.Home) {
                    getmyprofileandprogram();
                    replaceFragment(new DashboardFragment());
                    return true;
                } else if (itemId == R.id.TodoList) {
                    replaceFragment(new ToDoListFragment());
                    return true;
                } else if (itemId == R.id.Vital) {
                    replaceFragment(new VitalHealthTrends());
                    return true;
                } else if (itemId == R.id.More) {
                    replaceFragment(new MoreFragment());
                    return true;
                }
                return false;
            }
        });
    }

    private void replaceFragment(Fragment fragment) {
        FragmentTransaction fragmentManager = getSupportFragmentManager().beginTransaction();
        fragmentManager.replace(R.id.fl_main, fragment);
        fragmentManager.commitAllowingStateLoss();
    }

    public void getpatientprofile(Runnable onComplete) {
        String PROFILE_URL = BASE_URL + PATIENT_PROFILE.toString();
        StringRequest request = new StringRequest(Request.Method.GET, PROFILE_URL,
                new Response.Listener<String>() {
                    @Override
                    public void onResponse(String response) {
                        Log.i("onResponse_patientprofile", response);
                        db.deleteData();  // clear old profile data
                        db.insertData(response); // insert the latest


                        // Notify fragment after DB update
                        if (onComplete != null) {
                            onComplete.run();
                        }
                    }
                },
                new Response.ErrorListener() {
                    @Override
                    public void onErrorResponse(VolleyError error) {
                        Log.e("onErrorResponse", error.toString());
                    }
                }) {
            @Override
            public Map<String, String> getHeaders() throws AuthFailureError {
                Map<String, String> headers = new HashMap<>();
                headers.put("Bearer", Token);
                return headers;
            }
        };

        RequestQueue requestQueue = Volley.newRequestQueue(this);
        requestQueue.add(request);
    }
}