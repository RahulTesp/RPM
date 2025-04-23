package com.rpm.clynx.home;

import static com.rpm.clynx.utility.Links.BASE_URL;
import static com.rpm.clynx.utility.Links.GET_MY_PROFILE;
import static com.rpm.clynx.utility.Links.PATIENT_PROFILE;
import com.rpm.clynx.R;
import android.content.Intent;
import android.content.SharedPreferences;
import android.net.ConnectivityManager;
import android.net.NetworkInfo;
import android.os.Bundle;
import android.util.Log;
import android.view.MenuItem;
import androidx.annotation.NonNull;
import androidx.appcompat.app.AppCompatActivity;
import androidx.fragment.app.Fragment;
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
import com.rpm.clynx.activity.MoreActivity;
import com.rpm.clynx.fragments.DashboardFragment;
import com.rpm.clynx.fragments.Login;
import com.rpm.clynx.fragments.ToDoListFragment;
import com.rpm.clynx.fragments.VitalHealthTrends;
import com.rpm.clynx.utility.DataBaseHelper;
import com.rpm.clynx.utility.MyApplication;
import com.rpm.clynx.utility.SystemBarColor;
import org.json.JSONException;
import org.json.JSONObject;
import java.util.HashMap;
import java.util.Map;
import android.Manifest;
import android.content.Context;
import android.content.pm.PackageManager;
import android.os.Handler;
import android.widget.Toast;
import androidx.core.app.ActivityCompat;
import androidx.core.content.ContextCompat;

public class Home extends AppCompatActivity {

    DataBaseHelper db;
    SharedPreferences pref;
    SharedPreferences.Editor editor;
    String Token;
    String user_full_name;
    String patent_id;
    private BottomNavigationView navigation;
    private String ProgramName,ProgramTypeName;
    private String CompletedDuration;
    private String Status;
    private String pgmdet;
    private static final int CAMERA_MIC_PERMISSION_REQUEST_CODE = 1;
    private Handler handler;
    boolean loginstatusHome;

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
            getmyprofileandprogram();
            getpatientprofile();
        }
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
        super.onBackPressed();
        moveTaskToBack(true);
    }

    private void getmyprofileandprogram() {
        String PROFILE_URL = BASE_URL + GET_MY_PROFILE.toString();
        StringRequest request = new StringRequest(Request.Method.GET, PROFILE_URL, new Response.Listener<String>() {
            @Override
            public void onResponse(String response) {
                Log.i("onResponse_myprofileandprogram", response.toString());
                try {
                    JSONObject obj = new JSONObject(response);
                    user_full_name = obj.getString("Name");
                    patent_id = obj.getString("UserName");
                    ProgramTypeName = obj.getString("ProgramName");
                    Log.d("hProgramTypeName", ProgramTypeName);
                    CompletedDuration = obj.getString("CompletedDuration");
                    Status = obj.getString("Status");
                    editor.putString("CompletedDuration", CompletedDuration);
                    editor.putString("Status", Status);
                    editor.putString("ProgramTypeName", ProgramTypeName);
                    editor.putString("User_full_name", user_full_name);
                    editor.putString("Patent_id", patent_id);
                    editor.apply();
                    replaceFragment(new DashboardFragment());
                } catch (JSONException e) {
                    e.printStackTrace();
                }
                db.insertProfileData(response.toString());
            }

        }, new Response.ErrorListener() {
            @Override
            public void onErrorResponse(VolleyError error) {
                Log.e("onErrorResponse", error.toString());
                if ( error.networkResponse != null && error.networkResponse.statusCode == 401) {
                    error.printStackTrace();
                    Log.d("e", error.toString());
                    editor.putBoolean("loginstatus", false);
                    editor.apply();
                    db.deleteProfileData("myprofileandprogram");
                    db.deleteData();
                    editor.clear();
                    editor.commit();

                    try {
                        Log.d("cpLatestActivity", String.valueOf(((MyApplication) getApplication()).getLatestActivity()));
                        Intent intentlogout = new Intent((((MyApplication) getApplication()).getLatestActivity()), Login.class);
                        intentlogout.addFlags(Intent.FLAG_ACTIVITY_NEW_TASK | Intent.FLAG_ACTIVITY_CLEAR_TASK);
                        startActivity(intentlogout);
                    }catch (Exception e)
                    {
                        Log.e("onLogOff Clear", e.toString());
                    }
                }
                else
                {
                    Log.d("statusCodenull","statusCodenull");
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
        request.setRetryPolicy(new DefaultRetryPolicy(60000, DefaultRetryPolicy.DEFAULT_MAX_RETRIES, DefaultRetryPolicy.DEFAULT_BACKOFF_MULT));
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
                switch (item.getItemId()) {
                    case R.id.Home:
                        replaceFragment(new DashboardFragment());
                        return true;
                    case R.id.TodoList:
                        replaceFragment(new ToDoListFragment());
                        return true;
                    case R.id.Vital:
                        replaceFragment(new VitalHealthTrends());
                        return true;
                    case R.id.More:
                        Intent more = new Intent(Home.this, MoreActivity.class);
                        startActivity(more);
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

    private void getpatientprofile() {
        String PROFILE_URL = BASE_URL + PATIENT_PROFILE.toString();
        StringRequest request = new StringRequest(Request.Method.GET, PROFILE_URL, new Response.Listener<String>() {
            @Override
            public void onResponse(String response) {
                Log.i("onResponse_patientprofile", response.toString());
                db.insertData(response.toString());
            }
        }, new Response.ErrorListener() {
            @Override
            public void onErrorResponse(VolleyError error) {
                Log.e("onErrorResponse", error.toString());
                if ( error.networkResponse != null && error.networkResponse.statusCode == 401) {
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
                        Log.d("cpLatestActivity", String.valueOf(((MyApplication) getApplication()).getLatestActivity()));
                        Intent intentlogout = new Intent((((MyApplication) getApplication()).getLatestActivity()), Login.class);
                        intentlogout.addFlags(Intent.FLAG_ACTIVITY_NEW_TASK | Intent.FLAG_ACTIVITY_CLEAR_TASK);
                        startActivity(intentlogout);
                    }catch (Exception e)
                    {
                        Log.e("onLogOff Clear", e.toString());
                    }
                }
                else
                {
                    Log.d("statusCodenull","statusCodenull");
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
        request.setRetryPolicy(new DefaultRetryPolicy(60000, DefaultRetryPolicy.DEFAULT_MAX_RETRIES, DefaultRetryPolicy.DEFAULT_BACKOFF_MULT));
        requestQueue.add(request);
    }
}