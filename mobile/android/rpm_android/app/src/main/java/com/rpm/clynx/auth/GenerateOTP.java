package com.rpm.clynx.auth;

import static com.rpm.clynx.utility.Links.BASE_URL;
import static com.rpm.clynx.utility.Links.GENERATE_OTP;
import androidx.appcompat.app.AppCompatActivity;
import android.content.Intent;
import android.content.SharedPreferences;
import android.os.Bundle;
import android.util.Log;
import android.view.View;
import android.widget.Button;
import android.widget.Toast;
import com.android.volley.DefaultRetryPolicy;
import com.android.volley.NoConnectionError;
import com.android.volley.Request;
import com.android.volley.RequestQueue;
import com.android.volley.Response;
import com.android.volley.TimeoutError;
import com.android.volley.VolleyError;
import com.android.volley.toolbox.JsonObjectRequest;
import com.android.volley.toolbox.Volley;
import com.google.android.material.textfield.TextInputLayout;
import com.rpm.clynx.R;
import com.rpm.clynx.utility.DataBaseHelper;
import com.rpm.clynx.utility.Loader;
import com.rpm.clynx.utility.SystemBarColor;
import org.json.JSONException;
import org.json.JSONObject;

public class GenerateOTP extends AppCompatActivity {
    DataBaseHelper db;
    SharedPreferences pref;
    SharedPreferences.Editor editor;
    TextInputLayout userName;
    Button fp_btn;
    String token_FP, mobileNumberRP;
    int timeLimitRP;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_fp_otp);
        SystemBarColor.setSystemBarColor(this, R.color.profile_card_body_bg);

        fp_btn = findViewById(R.id.fp_btn);
        userName = findViewById(R.id. fp_user_name);
        pref = getApplicationContext().getSharedPreferences("RPMUserApp", MODE_PRIVATE);
        editor = pref.edit();
        db = new DataBaseHelper(this);

        fp_btn.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                if ( userName.getEditText().getText().toString().isEmpty()) {
                    Toast.makeText(GenerateOTP.this, "Please Enter Username", Toast.LENGTH_SHORT).show();
                } else {
                   generateOTP();
                }
            }
        });
    }

    private void generateOTP() {
        final Loader l1 = new Loader(GenerateOTP.this);
        l1.show("Please wait...");

        String valUserID = userName.getEditText().getText().toString();
        String GENERATE_OTP_URL = BASE_URL + GENERATE_OTP + "username=" + valUserID;
        JSONObject postData = new JSONObject();
        try {
            postData.put("UserName", valUserID);
        } catch (JSONException e) {
            Log.d("JSONException", e.toString());
            e.printStackTrace();
            Log.d("e", e.toString());
        }

        JsonObjectRequest jsonObjReq = new JsonObjectRequest(Request.Method.GET, GENERATE_OTP_URL, postData,
                new Response.Listener<JSONObject>() {
                    @Override
                    public void onResponse(JSONObject response) {
                        l1.dismiss();
                        Log.d("Log_url", GENERATE_OTP_URL);
                        Log.d("response", response.toString());
                        try {
                            Log.d("responses", response.toString());

                            if (response.getString("tkn") != null) {
                                token_FP = response.getString("tkn");
                                timeLimitRP = response.getInt("TimeLimit");
                                mobileNumberRP = response.getString("Mobilenumber");
                                Log.d("TokenFP", token_FP.toString());
                                editor.putString("UserNameFP", valUserID);
                                editor.putString("TokenFP", token_FP);
                                editor.putInt("TimeLimitRP", timeLimitRP);
                                editor.putString("MobileNumberRP", mobileNumberRP);
                                Log.d("timeLimit", String.valueOf(timeLimitRP));
                                editor.apply();

                                Intent intent = new Intent(GenerateOTP.this, ResetPassword.class);
                                startActivity(intent);
                            }
                            else {
                                Toast.makeText(GenerateOTP.this, "Unauthorized !", Toast.LENGTH_SHORT).show();
                            }
                        } catch (Exception e) {
                            Toast.makeText(GenerateOTP.this, "Invalid", Toast.LENGTH_SHORT).show();
                            e.printStackTrace();
                        }
                    }
                },
                new Response.ErrorListener() {
                    @Override
                    public void onErrorResponse(VolleyError error) {
                        l1.dismiss();
                        if (error instanceof TimeoutError || error instanceof NoConnectionError) {
                            Toast.makeText(getApplicationContext(), "Please check Internet Connection", Toast.LENGTH_SHORT).show();
                        } else if ( error.networkResponse.statusCode == 503) {
                            Log.d("errorstatusCde", String.valueOf(error.networkResponse.statusCode));
                            Toast.makeText(getApplicationContext(), "Number may be invalid! Please contact your careteam.", Toast.LENGTH_SHORT).show();
                        } else if ( error.networkResponse.statusCode == 401) {
                            Log.d("JSONE statusCoden", String.valueOf(error.networkResponse.statusCode));
                            error.printStackTrace();
                            Log.d("e", error.toString());
                            Toast.makeText(getApplicationContext(), "Username may be incorrect", Toast.LENGTH_LONG).show();
                        }
                        else if( error.networkResponse.statusCode == 403)
                        {
                            Log.d("Attempts exceeded", error.toString());
                            Toast.makeText(getApplicationContext(), "The user is locked! Maximum number of attempts exceeded.", Toast.LENGTH_LONG).show();
                            System.out.println("Maximum number of attempts exceeded");
                        }
                        else {
                            Toast.makeText(getApplicationContext(), "Username may be incorrect", Toast.LENGTH_LONG).show();
                        }
                    }
                });

        //creating a request queue
        RequestQueue requestQueue = Volley.newRequestQueue(GenerateOTP.this);
        //adding the string request to request queue
        jsonObjReq.setRetryPolicy(new DefaultRetryPolicy(0,-1,DefaultRetryPolicy.DEFAULT_BACKOFF_MULT));
        requestQueue.add(jsonObjReq);
    }
}