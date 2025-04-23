package com.rpm.clynx.auth;

import static com.rpm.clynx.utility.Links.BASE_URL;
import static com.rpm.clynx.utility.Links.GENERATE_OTP;
import static com.rpm.clynx.utility.Links.RESET_PASSWORD;
import androidx.appcompat.app.AlertDialog;
import androidx.appcompat.app.AppCompatActivity;
import android.content.DialogInterface;
import android.content.Intent;
import android.content.SharedPreferences;
import android.os.Bundle;
import android.os.CountDownTimer;
import android.os.Handler;
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
import android.widget.Button;
import android.widget.EditText;
import android.widget.ImageView;
import android.widget.LinearLayout;
import android.widget.ProgressBar;
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
import com.android.volley.toolbox.StringRequest;
import com.android.volley.toolbox.Volley;
import com.poovam.pinedittextfield.PinField;
import com.poovam.pinedittextfield.SquarePinField;
import com.rpm.clynx.R;
import com.rpm.clynx.fragments.Login;
import com.rpm.clynx.utility.DataBaseHelper;
import com.rpm.clynx.utility.Loader;
import com.rpm.clynx.utility.SystemBarColor;
import org.jetbrains.annotations.NotNull;
import org.json.JSONException;
import org.json.JSONObject;
import java.util.HashMap;
import java.util.Map;
import java.util.regex.Matcher;
import java.util.regex.Pattern;

public class ResetPassword extends AppCompatActivity {
    DataBaseHelper db;
    SharedPreferences pref;
    SharedPreferences.Editor editor;
    EditText newPassword, confirmPassword;
    Button fp_reset_btn, fp_resend_btn;
    TextView verifCodeRP;
    String mobileNumberRP, TokenFP, UsernameFP;
    int TimeLimitRP;
    String fpOtp, newpwd, confpwd;
    int i;
    SquarePinField squarePinFieldFP;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_reset_pw);
        SystemBarColor.setSystemBarColor(this, R.color.profile_card_body_bg);

        fp_reset_btn = findViewById(R.id.fp_reset_btn);
        fp_resend_btn = findViewById(R.id.fp_resend_btn);
        verifCodeRP = (TextView) findViewById(R.id.FPtxtvwVerif);
        newPassword = findViewById(R.id.resetpw1);
        confirmPassword = findViewById(R.id.resetpw2);
        ImageView showPasswordNew = findViewById(R.id.showNewPassword);
        ImageView showPasswordConfirm = findViewById(R.id.showConfirmPassword);

        showPasswordNew.setOnClickListener(new View.OnClickListener() {
            boolean passwordVisible = false;
            @Override
            public void onClick(View v) {
                passwordVisible = !passwordVisible;
                if (passwordVisible) {
                    newPassword.setTransformationMethod(null);
                    showPasswordNew.setImageResource(R.drawable.icons_view);
                } else {
                    newPassword.setTransformationMethod(new PasswordTransformationMethod());
                    showPasswordNew.setImageResource(R.drawable.icons_view_a);
                }
                newPassword.setSelection(newPassword.getText().length());
            }
        });

        showPasswordConfirm.setOnClickListener(new View.OnClickListener() {
            boolean passwordVisible = false;
            @Override
            public void onClick(View v) {
                passwordVisible = !passwordVisible;
                if (passwordVisible) {
                    confirmPassword.setTransformationMethod(null);
                    showPasswordConfirm.setImageResource(R.drawable.icons_view);
                } else {
                    confirmPassword.setTransformationMethod(new PasswordTransformationMethod());
                    showPasswordConfirm.setImageResource(R.drawable.icons_view_a);
                }
                confirmPassword.setSelection(confirmPassword.getText().length());
            }
        });

        squarePinFieldFP = findViewById(R.id.squareFieldFP);

        LinearLayout.LayoutParams params = new LinearLayout.LayoutParams(1000, 1000);
        squarePinFieldFP.setLayoutParams(params);
        pref = getApplicationContext().getSharedPreferences("RPMUserApp", MODE_PRIVATE);
        editor = pref.edit();
        UsernameFP = pref.getString("UserNameFP", null);
        TokenFP = pref.getString("TokenFP", null);
        TimeLimitRP = pref.getInt("TimeLimitRP", 0);
        i = TimeLimitRP * 60;
        Log.d("i", String.valueOf(i));
        mobileNumberRP = pref.getString("MobileNumberRP", null);
        verifCodeRP.setText("Enter the Code received on your Mobile number " + mobileNumberRP);

        squarePinFieldFP.setOnTextCompleteListener(new PinField.OnTextCompleteListener() {
            @Override
            public boolean onTextComplete(@NotNull String enteredText) {
                Log.d("enteredText", enteredText);
                Log.d("squarePinField.getText().toString()", squarePinFieldFP.getText().toString());
                Toast.makeText(ResetPassword.this, enteredText, Toast.LENGTH_SHORT).show();
                fpOtp = squarePinFieldFP.getText().toString();
                Log.d("fpOtp", fpOtp);
                return true; // Return false to keep the keyboard open else return true to close the keyboard
               // return false;
            }
        });

        db = new DataBaseHelper(this);

        ProgressBar progressBar = (ProgressBar) findViewById(R.id.progressBarRP);
        TextView progressTxt = (TextView) findViewById(R.id.txtprogressRP);

        final CountDownTimer remainingTimeCounter = new CountDownTimer(310000, 1000) {
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

// disable cut, copy, and paste options on long-press
        newPassword.setCustomInsertionActionModeCallback(new ActionMode.Callback() {
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
        newPassword.setCustomSelectionActionModeCallback(new ActionMode.Callback() {
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
        confirmPassword.setCustomInsertionActionModeCallback(new ActionMode.Callback() {
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
        confirmPassword.setCustomSelectionActionModeCallback(new ActionMode.Callback() {
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

        newPassword.setOnTouchListener(new View.OnTouchListener() {

            private GestureDetector gestureDetector = new GestureDetector(ResetPassword.this, new GestureDetector.SimpleOnGestureListener() {
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

        confirmPassword.setOnTouchListener(new View.OnTouchListener() {
            private GestureDetector gestureDetector = new GestureDetector(ResetPassword.this, new GestureDetector.SimpleOnGestureListener() {
                @Override
                public boolean onDoubleTap(MotionEvent e) {
                    // Consume the double-tap event so it doesn't trigger the default behavior
                    return true;
                }
            });

            @Override
            public boolean onTouch(View v, MotionEvent event) {
                // Let the GestureDetector handle the touch event
                return gestureDetector.onTouchEvent(event);
            }
        });

        // when we start typing
        newPassword.addTextChangedListener(new TextWatcher() {
            @Override
            public void beforeTextChanged(CharSequence s, int start, int count, int after) {
            }

            @Override
            public void onTextChanged(CharSequence s, int start, int before, int count) {
                // validate the password here
                String password = s.toString();

                if (password.isEmpty()) {
                    newPassword.setError("Password cannot be empty ! ");
                }
                else
                if (password.contains(" ")) {
                    newPassword.setError("Spaces are not allowed in password.");
                }
                else if (password.length() < 8) {
                    newPassword.setError("Password must be at least 8 characters long.");
                } else if (password.matches("^(.*[A-Z]){2,}.*$") != true) {
                    newPassword.setError("Password must contain at least 2 uppercase letters.");
                } else if (!password.matches("^(?=.*[0-9].*[0-9])[a-zA-Z0-9!@#$%^&*()_+\\-=\\[\\]{};':\"\\\\|,.<>\\/?]{8,}$")) {
                    newPassword.setError("Password must contain at least 2 digits.");
                } else if (password.matches("^(?=.*[!@#$%^&*()_+\\-=\\[\\]{};':\"\\\\|,.<>\\/?]).{8,}$") != true) {
                    newPassword.setError("Password must contain at least 1 special character.");
                } else {
                    newPassword.setError(null);
                }
            }

            @Override
            public void afterTextChanged(Editable s) {
            }
        });

        // when we start typing
        confirmPassword.addTextChangedListener(new TextWatcher() {
            @Override
            public void beforeTextChanged(CharSequence s, int start, int count, int after) {
            }

            @Override
            public void onTextChanged(CharSequence s, int start, int before, int count) {
                // validate the password here
                String password = s.toString();

                if (password.isEmpty()) {
                    confirmPassword.setError("Password cannot be empty ! ");
                }
                else
                if (password.contains(" ")) {
                    confirmPassword.setError("Spaces are not allowed in password.");
                }
                else if (password.length() < 8) {
                    confirmPassword.setError("Password must be at least 8 characters long.");
                } else if (password.matches("^(.*[A-Z]){2,}.*$") != true) {
                    confirmPassword.setError("Password must contain at least 2 uppercase letters.");
                } else if (!password.matches("^(?=.*[0-9].*[0-9])[a-zA-Z0-9!@#$%^&*()_+\\-=\\[\\]{};':\"\\\\|,.<>\\/?]{8,}$")) {
                    confirmPassword.setError("Password must contain at least 2 digits.");
                } else if (password.matches("^(?=.*[!@#$%^&*()_+\\-=\\[\\]{};':\"\\\\|,.<>\\/?]).{8,}$") != true) {
                    confirmPassword.setError("Password must contain at least 1 special character.");
                } else {
                    confirmPassword.setError(null);
                }
            }

            @Override
            public void afterTextChanged(Editable s) {
            }
        });


        fp_resend_btn.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                System.out.println("resend");
                remainingTimeCounter.cancel();
                remainingTimeCounter.start();

                i = TimeLimitRP * 60;
                RPuserResendOTP();
                fp_resend_btn.setVisibility(View.INVISIBLE);
                fp_reset_btn.setVisibility(View.VISIBLE);
                squarePinFieldFP.getText().clear();
            }
        });
    }

    @Override
    public void onBackPressed() {
        super.onBackPressed();
        showExitConfirmationDialog();
    }
    private void showExitConfirmationDialog() {
        AlertDialog.Builder builder = new AlertDialog.Builder(this);
        builder.setTitle("Exit")
                .setMessage("Do you want to go back?")
                .setPositiveButton("Yes", new DialogInterface.OnClickListener() {
                    @Override
                    public void onClick(DialogInterface dialog, int which) {
                        // Perform necessary cleanup and exit the app
                        //   finish(); // Close the activity
                        // Start the login activity and clear the back stack
                        Intent intent = new Intent(ResetPassword.this, Login.class);
                        intent.addFlags(Intent.FLAG_ACTIVITY_CLEAR_TOP | Intent.FLAG_ACTIVITY_NEW_TASK);
                        startActivity(intent);
                    }
                })
                .setNegativeButton("No", new DialogInterface.OnClickListener() {
                    @Override
                    public void onClick(DialogInterface dialog, int which) {
                        // Dismiss the dialog and continue with the app
                        dialog.dismiss();
                    }
                })
                .show();
    }

    public void check() {
        fp_resend_btn.setVisibility(View.VISIBLE);
        fp_reset_btn.setVisibility(View.INVISIBLE);
    }

    private void RPuserResendOTP() {
        final Loader l1 = new Loader(ResetPassword.this);
        l1.show("Please wait...");
        String valUserID = UsernameFP;
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
                        Log.d("GENERATE_OTP_URL", GENERATE_OTP_URL);
                        Log.d("response", response.toString());
                        try {
                            Log.d("responses RESEND", response.toString());
                            Log.d("response RESEND", response.toString());

                            if (response.getString("tkn") != null) {
                                TokenFP = response.getString("tkn");
                                TimeLimitRP = response.getInt("TimeLimit");
                                mobileNumberRP = response.getString("Mobilenumber");
                                Log.d("TokenFP", TokenFP.toString());
                                editor.putString("UserNameFP", valUserID);
                                editor.putString("TokenFP", TokenFP);
                                editor.putInt("TimeLimitRP", TimeLimitRP);
                                editor.putString("MobileNumberRP", mobileNumberRP);
                                Log.d("timeLimit", String.valueOf(TimeLimitRP));
                                editor.apply();
                            }
                        } catch (Exception e) {
                            Toast.makeText(ResetPassword.this, "Invalid", Toast.LENGTH_SHORT).show();
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
        RequestQueue requestQueue = Volley.newRequestQueue(ResetPassword.this);
        //adding the string request to request queue
        jsonObjReq.setRetryPolicy(new DefaultRetryPolicy(60000, DefaultRetryPolicy.DEFAULT_MAX_RETRIES, DefaultRetryPolicy.DEFAULT_BACKOFF_MULT));
        requestQueue.add(jsonObjReq);
    }

    public void resetpwdact(View view) {
        newpwd = newPassword.getText().toString();
        confpwd = confirmPassword.getText().toString();
        System.out.println("newpwd");
        System.out.println(newpwd);
        System.out.println("confpwd");
        System.out.println(confpwd);
        System.out.println("OTP fpOtp");
        System.out.println(fpOtp);

        if (squarePinFieldFP.getText().toString().isEmpty()) {
            Toast.makeText(this, "Please enter valid OTP ! ", Toast.LENGTH_SHORT).show();
        }

        if (newpwd.isEmpty()) {
            Toast.makeText(this, "Please enter New Password ! ", Toast.LENGTH_SHORT).show();
        }
        if (confpwd.isEmpty()) {
            Toast.makeText(this, "Please enter Confirm Password ! ", Toast.LENGTH_SHORT).show();
        }
        // Check for spaces in either new or confirm password
        if (newpwd.contains(" ") || confpwd.contains(" ")) {
            Toast.makeText(this, "Password should not contain spaces!", Toast.LENGTH_SHORT).show();
        }
        else {
            if (newpwd.equals(confpwd)) {
                System.out.println("resetpwdact");
                isValidPassword(newpwd);
                Log.d(" isValidPasswordnewpwd", String.valueOf(isValidPassword(newpwd)));
                if( isValidPassword(newpwd) == true)
                {
                    resetPassword();
                }
                else
                {
                    Toast.makeText(this, "Please enter valid Password ! ", Toast.LENGTH_SHORT).show();
                }
            } else {
                Toast.makeText(this, "New and Confirm Password are not same ! ", Toast.LENGTH_SHORT).show();
            }
        }
    }

    private void resetPassword() {
        final Loader l1 = new Loader(ResetPassword.this);
        l1.show("Please wait...");
        String RESET_PASSWORD_URL = BASE_URL + RESET_PASSWORD.toString();
        String newPwd = newPassword.getText().toString();
        String confPwd = confirmPassword.getText().toString();
        JSONObject postData = new JSONObject();
        try {
            postData.put("UserName", pref.getString("UserNameFP", null));
            postData.put("OTP", fpOtp);
            postData.put("Password", confPwd);
        } catch (JSONException e) {
            Log.d("JSONException", e.toString());
            e.printStackTrace();
            Log.d("e", e.toString());
        }

        StringRequest stringRequest = new StringRequest(Request.Method.POST, RESET_PASSWORD_URL,
                new Response.Listener<String>() {
                    @Override
                    public void onResponse(String response) {
                        l1.dismiss();
                        Log.d("RESET_PASSWORD_URL", RESET_PASSWORD_URL);
                        try {
                            Log.d("responses RESET", response.toString());
                            Toast.makeText(getApplicationContext(), "Reset Password Successfully !", Toast.LENGTH_LONG).show();
                            Intent intent = new Intent(ResetPassword.this, Login.class);
                            startActivity(intent);
                            finish();
                        } catch (Exception e) {
                            Toast.makeText(ResetPassword.this, "Invalid", Toast.LENGTH_SHORT).show();
                            e.printStackTrace();
                        }
                    }
                },
                new Response.ErrorListener() {
                    @Override
                    public void onErrorResponse(VolleyError error) {
                        l1.dismiss();
                        if (error instanceof TimeoutError || error instanceof NoConnectionError) {
                            // progressBar.setVisibility(View.GONE);
                            Toast.makeText(getApplicationContext(), "Please check Internet Connection", Toast.LENGTH_SHORT).show();
                        } else {
                            if (error.networkResponse.statusCode == 401) {
                                fp_resend_btn.setVisibility(View.VISIBLE);
                                fp_reset_btn.setVisibility(View.INVISIBLE);
                                Log.d("JSONE statusCode 401", String.valueOf(error.networkResponse.statusCode));
                                error.printStackTrace();
                                squarePinFieldFP.getText().clear();
                                AlertDialog.Builder builder = new AlertDialog.Builder(ResetPassword.this);
                                builder.setTitle("OTP may be wrong")
                                        .setPositiveButton("OK", new DialogInterface.OnClickListener() {
                                            public void onClick(DialogInterface dialog, int id) {
                                                // Do something when OK button is clicked
                                            }
                                        });
                                AlertDialog alert = builder.create();
                                alert.show();
                            } else if (error.networkResponse.statusCode == 403) {
                                Log.d("Attempts exceeded", error.toString());
                                error.printStackTrace();
                                System.out.println("Maximum number of attempts exceeded");
                                AlertDialog.Builder builder = new AlertDialog.Builder(ResetPassword.this);
                                builder.setTitle("The user is locked !")
                                        .setMessage("Maximum number of attempts exceeded.")
                                        .setPositiveButton("OK", new DialogInterface.OnClickListener() {
                                            public void onClick(DialogInterface dialog, int id) {
                                                // Do something when OK button is clicked
                                                Intent intent = new Intent(ResetPassword.this, Login.class);
                                                startActivity(intent);
                                                finish();
                                            }
                                        });
                                AlertDialog alert = builder.create();
                                alert.show();
                            }
                        }
                    }
                }) {
            @Override
            public String getBodyContentType() {
                return "application/json";
            }

            @Override
            public byte[] getBody() throws AuthFailureError {
                return postData.toString().getBytes();
            }

            @Override
            public Map<String, String> getHeaders() throws AuthFailureError {
                Map<String, String> headers = new HashMap<>();
                headers.put("Bearer", TokenFP);
                Log.d("headers_TokenFP", headers.toString());
                Log.d("Token_TokenFP", TokenFP);
                return headers;
            }
        };

        //creating a request queue
        RequestQueue requestQueue = Volley.newRequestQueue(ResetPassword.this);
        //adding the string request to request queue
        stringRequest.setRetryPolicy(new DefaultRetryPolicy(0, -1, DefaultRetryPolicy.DEFAULT_BACKOFF_MULT));
        requestQueue.add(stringRequest);
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
}




