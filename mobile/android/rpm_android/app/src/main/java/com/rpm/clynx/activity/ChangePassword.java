package com.rpm.clynx.activity;

import android.content.Intent;
import android.content.SharedPreferences;
import android.os.Bundle;
import android.text.Editable;
import android.text.TextWatcher;
import android.text.method.PasswordTransformationMethod;
import android.util.Log;
import android.view.ActionMode;
import android.view.GestureDetector;
import android.view.Menu;
import android.view.MenuItem;
import android.view.MotionEvent;
import android.view.View;
import android.widget.EditText;
import android.widget.ImageView;
import android.widget.TextView;
import android.widget.Toast;
import android.widget.Toolbar;
import androidx.appcompat.app.AppCompatActivity;
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
import com.rpm.clynx.fragments.Login;
import com.rpm.clynx.utility.DataBaseHelper;
import com.rpm.clynx.utility.Links;
import com.rpm.clynx.utility.Loader;
import com.rpm.clynx.R;
import com.rpm.clynx.utility.MyApplication;
import org.json.JSONException;
import org.json.JSONObject;
import java.util.HashMap;
import java.util.Map;
import java.util.regex.Matcher;
import java.util.regex.Pattern;

public class ChangePassword extends AppCompatActivity {

    EditText curpw, newpw, cnnepw;
    String scupw, snewpw;
    TextView tvact_tvcp_cancle;
    SharedPreferences pref;
    SharedPreferences.Editor editor;
    String Token,username,oldPassword;
    DataBaseHelper db;
    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_change_password);
        db = new DataBaseHelper(this);
        curpw = findViewById(R.id.act_cp_cuur_pw);
        newpw = findViewById(R.id.act_cp_cuur_newpw);
        cnnepw = findViewById(R.id.act_cp_cuur_cnf_newPw);
        tvact_tvcp_cancle = findViewById(R.id.act_tvcp_cancle);
        initPerformBackClick();
        pref = this.getSharedPreferences("RPMUserApp",this.MODE_PRIVATE);
        editor = pref.edit();
        Token = pref.getString("Token", null);
        username = pref.getString("UserName", null);
        oldPassword = pref.getString("Password", null);

        ImageView showCurrPassword = findViewById(R.id.showcurrPassword);
        ImageView showNewPswrd = findViewById(R.id.shownewPswrd);
        ImageView showConfirmNewPwd = findViewById(R.id.showConfirmnewPassword);

        showCurrPassword.setOnClickListener(new View.OnClickListener() {
            boolean passwordVisible = false;
            @Override
            public void onClick(View v) {
                passwordVisible = !passwordVisible;
                if (passwordVisible) {
                    curpw.setTransformationMethod(null);
                    showCurrPassword.setImageResource(R.drawable.icons_view);
                } else {
                    curpw.setTransformationMethod(new PasswordTransformationMethod());
                    showCurrPassword.setImageResource(R.drawable.icons_view_a);
                }
                curpw.setSelection(curpw.getText().length());
            }
        });

        showNewPswrd.setOnClickListener(new View.OnClickListener() {
            boolean passwordVisible = false;
            @Override
            public void onClick(View v) {
                passwordVisible = !passwordVisible;
                if (passwordVisible) {
                    newpw.setTransformationMethod(null);
                    showNewPswrd.setImageResource(R.drawable.icons_view);
                } else {
                    newpw.setTransformationMethod(new PasswordTransformationMethod());
                    showNewPswrd.setImageResource(R.drawable.icons_view_a);
                }
                newpw.setSelection(newpw.getText().length());

            }
        });
        showConfirmNewPwd.setOnClickListener(new View.OnClickListener() {
            boolean passwordVisible = false;
            @Override
            public void onClick(View v) {
                passwordVisible = !passwordVisible;
                if (passwordVisible) {
                    cnnepw.setTransformationMethod(null);
                    showConfirmNewPwd.setImageResource(R.drawable.icons_view);
                } else {
                    cnnepw.setTransformationMethod(new PasswordTransformationMethod());
                    showConfirmNewPwd.setImageResource(R.drawable.icons_view_a);
                }
                cnnepw.setSelection(cnnepw.getText().length());
            }
        });

        // disable cut, copy, and paste options on long-press
        curpw.setCustomInsertionActionModeCallback(new ActionMode.Callback() {
            @Override
            public boolean onCreateActionMode(ActionMode mode, Menu menu) {
                // remove the paste option from the context menu
                //   menu.removeItem(android.R.id.paste);
                return false;
            }

            @Override
            public boolean onPrepareActionMode(ActionMode mode, Menu menu) {
                return false;
            }

            @Override
            public boolean onActionItemClicked(ActionMode mode, MenuItem item) {
                return false;
            }

            @Override
            public void onDestroyActionMode(ActionMode mode) {

            }
        });

// disable cut, copy, and paste options on double-tap
        curpw.setCustomSelectionActionModeCallback(new ActionMode.Callback() {
            @Override
            public boolean onCreateActionMode(ActionMode mode, Menu menu) {
                // remove the paste option from the context menu
                menu.removeItem(android.R.id.paste);
                return true;
            }

            @Override
            public boolean onPrepareActionMode(ActionMode mode, Menu menu) {
                return false;
            }

            @Override
            public boolean onActionItemClicked(ActionMode mode, MenuItem item) {
                return false;
            }

            @Override
            public void onDestroyActionMode(ActionMode mode) {

            }
        });

        // disable cut, copy, and paste options on long-press
        newpw.setCustomInsertionActionModeCallback(new ActionMode.Callback() {
            @Override
            public boolean onCreateActionMode(ActionMode mode, Menu menu) {
                // remove the paste option from the context menu
                //   menu.removeItem(android.R.id.paste);
                return false;
            }

            @Override
            public boolean onPrepareActionMode(ActionMode mode, Menu menu) {
                return false;
            }

            @Override
            public boolean onActionItemClicked(ActionMode mode, MenuItem item) {
                return false;
            }

            @Override
            public void onDestroyActionMode(ActionMode mode) {

            }
        });

// disable cut, copy, and paste options on double-tap
        newpw.setCustomSelectionActionModeCallback(new ActionMode.Callback() {
            @Override
            public boolean onCreateActionMode(ActionMode mode, Menu menu) {
                // remove the paste option from the context menu
                menu.removeItem(android.R.id.paste);
                return true;
            }

            @Override
            public boolean onPrepareActionMode(ActionMode mode, Menu menu) {
                return false;
            }

            @Override
            public boolean onActionItemClicked(ActionMode mode, MenuItem item) {
                return false;
            }

            @Override
            public void onDestroyActionMode(ActionMode mode) {

            }
        });

        // disable cut, copy, and paste options on long-press
        cnnepw.setCustomInsertionActionModeCallback(new ActionMode.Callback() {
            @Override
            public boolean onCreateActionMode(ActionMode mode, Menu menu) {
                // remove the paste option from the context menu
                //   menu.removeItem(android.R.id.paste);
                return false;
            }

            @Override
            public boolean onPrepareActionMode(ActionMode mode, Menu menu) {
                return false;
            }

            @Override
            public boolean onActionItemClicked(ActionMode mode, MenuItem item) {
                return false;
            }

            @Override
            public void onDestroyActionMode(ActionMode mode) {

            }
        });

// disable cut, copy, and paste options on double-tap
        cnnepw.setCustomSelectionActionModeCallback(new ActionMode.Callback() {
            @Override
            public boolean onCreateActionMode(ActionMode mode, Menu menu) {
                // remove the paste option from the context menu
                menu.removeItem(android.R.id.paste);
                return true;
            }

            @Override
            public boolean onPrepareActionMode(ActionMode mode, Menu menu) {
                return false;
            }

            @Override
            public boolean onActionItemClicked(ActionMode mode, MenuItem item) {
                return false;
            }

            @Override
            public void onDestroyActionMode(ActionMode mode) {

            }
        });

        curpw.setOnTouchListener(new View.OnTouchListener() {

            private GestureDetector gestureDetector = new GestureDetector(ChangePassword.this, new GestureDetector.SimpleOnGestureListener() {
                @Override
                public boolean onDoubleTap(MotionEvent e) {
                    // Consume the double-tap event so it doesn't trigger the default behavior
                    return true;
                }
            });

            @Override
            public boolean onTouch(View v, MotionEvent event) {
                return gestureDetector.onTouchEvent(event);
                // return true;
            }
        });

        newpw.setOnTouchListener(new View.OnTouchListener() {

            private GestureDetector gestureDetector = new GestureDetector(ChangePassword.this, new GestureDetector.SimpleOnGestureListener() {
                @Override
                public boolean onDoubleTap(MotionEvent e) {
                    // Consume the double-tap event so it doesn't trigger the default behavior
                    return true;
                }
            });

            @Override
            public boolean onTouch(View v, MotionEvent event) {
                return gestureDetector.onTouchEvent(event);
                // return true;

            }
        });

        cnnepw.setOnTouchListener(new View.OnTouchListener() {

            private GestureDetector gestureDetector = new GestureDetector(ChangePassword.this, new GestureDetector.SimpleOnGestureListener() {
                @Override
                public boolean onDoubleTap(MotionEvent e) {
                    // Consume the double-tap event so it doesn't trigger the default behavior
                    return true;
                }
            });

            @Override
            public boolean onTouch(View v, MotionEvent event) {
                return gestureDetector.onTouchEvent(event);
                // return true;

            }
        });

        // when we start typing
        newpw.addTextChangedListener(new TextWatcher() {
            @Override
            public void beforeTextChanged(CharSequence s, int start, int count, int after) {
            }

            @Override
            public void onTextChanged(CharSequence s, int start, int before, int count) {
                // validate the password here
                String password = s.toString();

                if (password.isEmpty()) {
                    newpw.setError("Password cannot be empty ! ");
                }
                else
                if (password.contains(" ")) {
                    newpw.setError("Spaces are not allowed in password.");
                }
                else
                if (password.length() < 8) {
                    newpw.setError("Password must be at least 8 characters long.");
                }
                else if( password.matches("^(.*[A-Z]){2,}.*$") != true) {
                    newpw.setError("Password must contain at least 2 uppercase letters.");
                }
                else    if(!password.matches("^(?=.*[0-9].*[0-9])[a-zA-Z0-9!@#$%^&*()_+\\-=\\[\\]{};':\"\\\\|,.<>\\/?]{8,}$"))  {

                    newpw.setError("Password must contain at least 2 digits.");
                }
                else
                if (password.matches("^(?=.*[!@#$%^&*()_+\\-=\\[\\]{};':\"\\\\|,.<>\\/?]).{8,}$") != true) {
                    newpw.setError("Password must contain at least 1 special character.");
                }
                else {
                    newpw.setError(null);
                }
            }

            @Override
            public void afterTextChanged(Editable s) {

            }
        });

        // when we start typing
        cnnepw.addTextChangedListener(new TextWatcher() {
            @Override
            public void beforeTextChanged(CharSequence s, int start, int count, int after) {
            }

            @Override
            public void onTextChanged(CharSequence s, int start, int before, int count) {
                // validate the password here
                String password = s.toString();

                if (password.isEmpty()) {
                    cnnepw.setError("Password cannot be empty ! ");
                }
                else
                if (password.contains(" ")) {
                    cnnepw.setError("Spaces are not allowed in password.");
                }
                else
                if (password.length() < 8) {
                    cnnepw.setError("Password must be at least 8 characters long.");
                }
                else if( password.matches("^(.*[A-Z]){2,}.*$") != true) {
                    cnnepw.setError("Password must contain at least 2 uppercase letters.");
                }
                else    if(!password.matches("^(?=.*[0-9].*[0-9])[a-zA-Z0-9!@#$%^&*()_+\\-=\\[\\]{};':\"\\\\|,.<>\\/?]{8,}$"))  {
                    cnnepw.setError("Password must contain at least 2 digits.");
                }
                else
                if (password.matches("^(?=.*[!@#$%^&*()_+\\-=\\[\\]{};':\"\\\\|,.<>\\/?]).{8,}$") != true) {
                    cnnepw.setError("Password must contain at least 1 special character.");
                }
                else {
                    cnnepw.setError(null);
                }
            }

            @Override
            public void afterTextChanged(Editable s) {
            }

        });

        tvact_tvcp_cancle.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View view) {
                curpw.getText().clear();
                newpw.getText().clear();
                cnnepw.getText().clear();
            }
        });
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

    public boolean isValidPassword(final String password) {

        Pattern pattern;
        Matcher matcher;
        final String PASSWORD_PATTERN =
                "^(?=(?:.*\\d){2})(?=(?:.*[A-Z]){2})(?=.*[@#$%^&+=]).{8,}$";
        pattern = Pattern.compile(PASSWORD_PATTERN);
        matcher = pattern.matcher(password);
        return matcher.matches();
    }
    public void changepwonact(View view) {
        scupw = newpw.getText().toString();
        snewpw = cnnepw.getText().toString();
        Log.d("scupw",scupw);
        Log.d("snewpw",snewpw);
        Log.d("eq", String.valueOf(scupw.equals(snewpw)));

           if (!isValidPassword(scupw))
        {
            Toast.makeText(this, "Password not valid ! ", Toast.LENGTH_SHORT).show();
        }
        else if (scupw.equals(snewpw)) {
                changePwd();
            } else {
                Toast.makeText(this, "new and confirm password are not same ! ", Toast.LENGTH_SHORT).show();
            }
    }

    private void changePwd()  {
        final Loader l1 = new Loader(ChangePassword.this);
        l1.show("Please wait...");
        String CHANGE_PWD_URL = Links.BASE_URL+ Links.CHANGE_PASSWORD;
        String currentPwd =  curpw.getText().toString();
        String newPwd = newpw.getText().toString();
        JSONObject postData = new JSONObject();
        try{
            postData.put("UserName", username);
            postData.put("OldPassword", currentPwd);
            postData.put("NewPassword", newPwd);
        } catch (JSONException e) {
            e.printStackTrace();
        }

        JsonObjectRequest jsonObjReq = new JsonObjectRequest(Request.Method.POST, CHANGE_PWD_URL, postData,
                new Response.Listener<JSONObject>() {
                    @Override
                    public void onResponse(JSONObject response) {
                        l1.dismiss();
                        Log.d("Log_url", CHANGE_PWD_URL);
                        Log.d("chngpwdresponse", String.valueOf(response));
                        Toast.makeText(ChangePassword.this, "Password has been updated!", Toast.LENGTH_SHORT).show();
                        Log.d("CPSTTS", String.valueOf(pref.getBoolean("loginstatus", false)));
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
                },
                new Response.ErrorListener() {
                    @Override

                    public void onErrorResponse(VolleyError error) {
                        if (error instanceof TimeoutError || error instanceof NoConnectionError) {
                            Toast.makeText(getApplicationContext(), "Please check Internet Connection", Toast.LENGTH_SHORT).show();
                            l1.dismiss();
                        } else {
                            Toast.makeText(getApplicationContext(), "Current Password may be wrong!", Toast.LENGTH_LONG).show();
                            l1.dismiss();
                        }
                    }
                }){
            @Override
            public Map<String, String> getHeaders() throws AuthFailureError {
                Map<String, String> headers = new HashMap<>();
                headers.put("Bearer",Token);
                Log.d("Token_profile_personal", Token);
                return headers;
            }
        };
        //creating a request queue
        RequestQueue requestQueue = Volley.newRequestQueue(ChangePassword.this);
        jsonObjReq.setRetryPolicy(new DefaultRetryPolicy(60000, DefaultRetryPolicy.DEFAULT_MAX_RETRIES, DefaultRetryPolicy.DEFAULT_BACKOFF_MULT));
        requestQueue.add(jsonObjReq);
    }
}