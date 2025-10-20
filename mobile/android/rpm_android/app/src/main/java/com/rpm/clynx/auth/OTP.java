package com.rpm.clynx.auth;

import static com.rpm.clynx.utility.Links.BASE_URL;
import static com.rpm.clynx.utility.Links.LOGIN;
import static com.rpm.clynx.utility.Links.OTP_VERIFY;
import androidx.appcompat.app.AppCompatActivity;
import android.content.Intent;
import android.content.SharedPreferences;
import android.os.Bundle;
import android.os.CountDownTimer;
import android.os.Handler;
import android.util.Log;
import android.view.View;
import android.widget.Button;
import android.widget.ProgressBar;
import android.widget.RelativeLayout;
import android.widget.TextView;
import android.widget.Toast;
import com.android.volley.AuthFailureError;
import com.android.volley.DefaultRetryPolicy;
import com.android.volley.NoConnectionError;
import com.android.volley.Request;
import com.android.volley.RequestQueue;
import com.android.volley.Response;
import com.android.volley.TimeoutError;
import com.android.volley.VolleyError;
import com.android.volley.toolbox.JsonObjectRequest;
import com.android.volley.toolbox.Volley;
import com.google.firebase.messaging.FirebaseMessaging;
import com.poovam.pinedittextfield.PinField;
import com.poovam.pinedittextfield.SquarePinField;
import com.rpm.clynx.fragments.Login;
import com.rpm.clynx.fragments.LoginHelper;
import com.rpm.clynx.fragments.TokenManager;
import com.rpm.clynx.home.Home;
import com.rpm.clynx.R;
import com.rpm.clynx.utility.DataBaseHelper;
import com.rpm.clynx.utility.Loader;
import com.rpm.clynx.utility.SystemBarColor;
import org.jetbrains.annotations.NotNull;
import org.json.JSONArray;
import org.json.JSONException;
import org.json.JSONObject;
import java.util.HashMap;
import java.util.Map;

public class OTP extends AppCompatActivity {

    DataBaseHelper db;
    SharedPreferences pref;
    SharedPreferences.Editor editor;
    String tokenOld, Usrnm, Pwd , mobileNumber;
    TextView verifCode;
    Button resendotp_btn;
    String token;
    String role;
    RelativeLayout reltv;
    int i ;
    int TimeLimit;
    Boolean resnd = false;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_otp);
        SystemBarColor.setSystemBarColor(this, R.color.profile_card_body_bg);
        resendotp_btn = findViewById(R.id.login_otp_resend);
        reltv = (RelativeLayout) findViewById(R.id.relativeverif);
        verifCode = (TextView) findViewById(R.id.txtvwVerif);
        final SquarePinField squarePinField = findViewById(R.id.squareField);
        pref = getApplicationContext().getSharedPreferences("RPMUserApp", MODE_PRIVATE);
        editor = pref.edit();
        boolean otpstatus = pref.getBoolean("otpstatus", false);
        tokenOld = pref.getString("TokenOld", null);
        Usrnm = pref.getString("UserName", null);
        Pwd = pref.getString("Password", null);
        TimeLimit = pref.getInt("TimeLimit", 0);
        i = TimeLimit * 60 ;
        Log.d("i", String.valueOf(i));
        mobileNumber = pref.getString("MobileNumber", null);
        verifCode.setText("Enter the Code received on your Mobile number "+mobileNumber);

        if (otpstatus) {
            Log.d("otpstatus", String.valueOf(otpstatus));
            Intent more = new Intent(OTP.this, Home.class);
            more.addFlags(Intent.FLAG_ACTIVITY_CLEAR_TOP);
            startActivity(more);
            finish();
        }

        ProgressBar progressBar = (ProgressBar) findViewById(R.id.progressBarToday);
        TextView progressTxt = (TextView) findViewById(R.id.txtprogress);

        final CountDownTimer remainingTimeCounter = new CountDownTimer(130000, 1000) {
            public void onTick(long millisUntilFinished) {
                if (i >= 0) {
                    progressTxt.setText("" + i + " Sec");
                    progressBar.setProgress(i);
                    i--;

                    if (i == 0) {
                        new Handler().postDelayed(() -> check(), 1000);
                    }
                } else {
                }
            }

            public void onFinish() {
                Log.d("Counter", "Finished....");
            }
        }.start();

        squarePinField.setOnTextCompleteListener(new PinField.OnTextCompleteListener() {
            @Override
            public boolean onTextComplete(@NotNull String enteredText) {
                Log.d("enteredText", enteredText);
                Log.d("squarePinField.getText().toString()", squarePinField.getText().toString());
                Toast.makeText(OTP.this, enteredText, Toast.LENGTH_SHORT).show();

                verifycode(squarePinField.getText().toString(), squarePinField);
                return true; // Return false to keep the keyboard open else return true to close the keyboard
            }
        });


        db = new DataBaseHelper(this);

        resendotp_btn.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                System.out.println("resend");
                System.out.println(resnd);
                remainingTimeCounter.cancel();
                remainingTimeCounter.start();
                i =   TimeLimit * 60;
                userResendOTP();
                reltv.setVisibility(View.INVISIBLE);
                squarePinField.getText().clear();
            }
        });
    }

    public void check() {
        reltv.setVisibility(View.VISIBLE);
    }

    private void userResendOTP() {

        final Loader l1 = new Loader(OTP.this);
        l1.show("Please wait...");
        String LOGIN_URL = BASE_URL + LOGIN.toString();
        String valUserID = Usrnm;
        String valPassWord = Pwd;
        JSONObject postData = new JSONObject();
        try {
            postData.put("UserName", valUserID);
            postData.put("Password", valPassWord);
        } catch (JSONException e) {
            Log.d("JSONException", e.toString());
            e.printStackTrace();
            Log.d("e", e.toString());
        }

        JsonObjectRequest jsonObjReq = new JsonObjectRequest(Request.Method.POST, LOGIN_URL, postData,
                new Response.Listener<JSONObject>() {
                    @Override
                    public void onResponse(JSONObject response) {
                        l1.dismiss();
                        Log.d("Log_url", LOGIN_URL);
                        Log.d("response", response.toString());
                        try {
                            Log.d("responses", response.toString());
                            tokenOld = response.getString("tkn");
                            //String message = response.getString("message");
                            Log.d("Token_login", tokenOld.toString());
                            Log.d("response", response.toString());

                            if (response.getString("tkn") != null) {
                                editor.putString("UserName", valUserID);
                                editor.putString("Password", valPassWord);
                                editor.putString("TokenOld", tokenOld);
                                editor.apply();
                            }
                        } catch (Exception e) {
                            Toast.makeText(OTP.this, "Invalid", Toast.LENGTH_SHORT).show();
                            e.printStackTrace();
                        }
                    }
                },
                new Response.ErrorListener() {
                    @Override
                    public void onErrorResponse(VolleyError error) {
                        l1.dismiss();
                    }
                });

        //creating a request queue
        RequestQueue requestQueue = Volley.newRequestQueue(OTP.this);
        //adding the string request to request queue
        jsonObjReq.setRetryPolicy(new DefaultRetryPolicy(60000, DefaultRetryPolicy.DEFAULT_MAX_RETRIES, DefaultRetryPolicy.DEFAULT_BACKOFF_MULT));
        requestQueue.add(jsonObjReq);
    }

    private void verifycode(String otp, SquarePinField squarePinField) {
        Log.d("VERIFYCODE_LOG", "verifycode() called with OTP: " + otp);

        final Loader l1 = new Loader(OTP.this);
        l1.show("Please wait...");
        Log.d("VERIFYCODE_LOG", "Loader shown");

        String OTP_URL = BASE_URL + OTP_VERIFY.toString();
        Log.d("VERIFYCODE_LOG", "OTP URL: " + OTP_URL);

        JSONObject postData = new JSONObject();
        try {
            postData.put("UserName", Usrnm);
            postData.put("OTP", otp);
            Log.d("VERIFYCODE_LOG", "Post data prepared: " + postData.toString());
        } catch (JSONException e) {
            e.printStackTrace();
            Log.e("VERIFYCODE_LOG", "JSONException while preparing post data: " + e.getMessage());
        }

        JsonObjectRequest jsonObjReq = new JsonObjectRequest(Request.Method.POST, OTP_URL, postData,
                new Response.Listener<JSONObject>() {
                    @Override
                    public void onResponse(JSONObject response) {
                        l1.dismiss();
                        Log.d("VERIFYCODE_LOG", "Response received: " + response.toString());
                        try {
                            // Get token
                            token = response.optString("tkn");
                            Log.d("VERIFYCODE_LOG", "Token received: " + token);
                            if (token == null || token.isEmpty()) {
                                Log.e("VERIFYCODE_LOG", "Token is null or empty");
                                Toast.makeText(OTP.this, "Invalid token received", Toast.LENGTH_SHORT).show();
                                return;
                            }

                            // Parse Roles
                            JSONArray rolesArray = response.optJSONArray("Roles");
                            Log.d("VERIFYCODE_LOG", "Roles array: " + rolesArray);
                            boolean hasRole7 = false;

                            if (rolesArray != null) {
                                for (int i = 0; i < rolesArray.length(); i++) {
                                    JSONObject roleObj = rolesArray.getJSONObject(i);
                                    int roleId = roleObj.optInt("Id", -1);
                                    Log.d("VERIFYCODE_LOG", "RoleId at index " + i + ": " + roleId);
                                    if (roleId == 7) {
                                        hasRole7 = true;
                                        break;
                                    }
                                }
                            } else {
                                Log.e("VERIFYCODE_LOG", "Roles array is null");
                            }

                            // Check for role and token

                            if (hasRole7) {
                                editor.putString("Token", token);

                                editor.putBoolean("otpstatus", true);
                                editor.putBoolean("loginstatus", true);
                                editor.apply();

                                // Retrieve Chat / Twilio token
                                TokenManager.getInstance(getApplicationContext()).retrieveTokenFromServer(token);

                                // Firebase
                                FirebaseMessaging.getInstance().subscribeToTopic("videocall");
                                FirebaseMessaging.getInstance().getToken().addOnCompleteListener(task -> {
                                    if (!task.isSuccessful()) return;
                                    String firebasetoken = task.getResult();
                                    editor.putString("FirebaseToken", firebasetoken);
                                    editor.apply();

                                    LoginHelper.firebasetokenInsertion(OTP.this, token, firebasetoken);
                                    LoginHelper.membersList(OTP.this, token);

                                });

                                // Navigate to Home
                                Intent intent = new Intent(OTP.this, Home.class);
                                startActivity(intent);
                                finish();
                            }

                            else {
                                Log.e("VERIFYCODE_LOG", "User does not have role 7");
                                Toast.makeText(OTP.this, "Invalid User or Unauthorized!", Toast.LENGTH_SHORT).show();
                            }

                        } catch (JSONException e) {
                            e.printStackTrace();
                            Log.e("VERIFYCODE_LOG", "JSONException in onResponse: " + e.getMessage());
                            Toast.makeText(OTP.this, "Invalid response format", Toast.LENGTH_SHORT).show();
                        } catch (Exception e) {
                            e.printStackTrace();
                            Log.e("VERIFYCODE_LOG", "Unexpected exception in onResponse: " + e.getMessage());
                            Toast.makeText(OTP.this, "Unexpected error occurred", Toast.LENGTH_SHORT).show();
                        }
                    }

                },
                new Response.ErrorListener() {
                    @Override
                    public void onErrorResponse(VolleyError error) {
                        l1.dismiss();
                        Log.e("VERIFYCODE_LOG", "VolleyError received");
                        if (error instanceof TimeoutError || error instanceof NoConnectionError) {
                            Log.e("VERIFYCODE_LOG", "Timeout or No Connection");
                            Toast.makeText(getApplicationContext(), "Poor Network Connection", Toast.LENGTH_SHORT).show();
                        } else if (error.networkResponse != null) {
                            int statusCode = error.networkResponse.statusCode;
                            Log.e("VERIFYCODE_LOG", "Server error code: " + statusCode);
                            if (statusCode == 401) {
                                Toast.makeText(getApplicationContext(), "OTP may be wrong!", Toast.LENGTH_LONG).show();
                                squarePinField.getText().clear();
                            } else if (statusCode == 403) {
                                Toast.makeText(getApplicationContext(), "The user is locked! Maximum number of attempts exceeded.", Toast.LENGTH_LONG).show();
                                finish();
                            } else {
                                Toast.makeText(getApplicationContext(), "Server error: " + statusCode, Toast.LENGTH_SHORT).show();
                            }
                        } else {
                            Log.e("VERIFYCODE_LOG", "Unexpected Volley error: " + error.toString());
                            Toast.makeText(getApplicationContext(), "Unexpected error occurred", Toast.LENGTH_SHORT).show();
                        }
                        error.printStackTrace();
                    }
                }) {
            @Override
            public Map<String, String> getHeaders() throws AuthFailureError {
                Map<String, String> headers = new HashMap<>();
                headers.put("Bearer", tokenOld != null ? tokenOld : "");
                Log.d("VERIFYCODE_LOG", "Header tokenOld: " + tokenOld);
                return headers;
            }
        };

        RequestQueue requestQueue = Volley.newRequestQueue(OTP.this);
        jsonObjReq.setRetryPolicy(new DefaultRetryPolicy(0, -1, DefaultRetryPolicy.DEFAULT_BACKOFF_MULT));
        requestQueue.add(jsonObjReq);

        Log.d("VERIFYCODE_LOG", "Request added to queue");
    }
}