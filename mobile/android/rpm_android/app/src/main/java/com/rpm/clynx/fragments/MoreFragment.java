package com.rpm.clynx.fragments;

import android.annotation.SuppressLint;
import android.content.Context;
import android.content.Intent;
import android.content.SharedPreferences;
import android.os.Bundle;
import android.util.Log;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.Button;
import android.widget.LinearLayout;
import android.widget.TextView;
import android.widget.Toast;

import androidx.annotation.NonNull;
import androidx.annotation.Nullable;
import androidx.appcompat.widget.Toolbar;
import androidx.fragment.app.Fragment;
import androidx.fragment.app.FragmentManager;

import com.android.volley.AuthFailureError;
import com.android.volley.Request;
import com.android.volley.RequestQueue;
import com.android.volley.toolbox.StringRequest;
import com.android.volley.toolbox.Volley;
import com.google.android.material.bottomnavigation.BottomNavigationView;
import com.google.firebase.messaging.FirebaseMessaging;
import com.rpm.clynx.R;
import com.rpm.clynx.service.TwilioChatManager;
import com.rpm.clynx.utility.ConversationsClientManager;
import com.rpm.clynx.utility.DataBaseHelper;
import com.rpm.clynx.utility.MyApplication;
import com.rpm.clynx.utility.SystemBarColor;
import static com.rpm.clynx.utility.Links.BASE_URL;
import static com.rpm.clynx.utility.Links.FB_TOK_DELETE;
import static com.rpm.clynx.utility.Links.LOGOUT;

import java.util.HashMap;
import java.util.Map;

public class MoreFragment extends Fragment {

    SharedPreferences pref;
    SharedPreferences.Editor editor;
    TextView tvusername, tvuserid;
    DataBaseHelper db;
    String Token;
    Button Logout_btn;
    View view;
    Toolbar toolbar;
    @SuppressLint("MissingInflatedId")
    @Nullable
    @Override
    public View onCreateView(@NonNull LayoutInflater inflater,
                             @Nullable ViewGroup container,
                             @Nullable Bundle savedInstanceState) {

        // Inflate the layout for the fragment
        view = inflater.inflate(R.layout.fragment_more, container, false);
        LinearLayout myInfoLayout = view.findViewById(R.id.my_info_layout);
        LinearLayout pgmInfoLayout = view.findViewById(R.id.pgm_info_layout);
        LinearLayout clinicalInfoLayout = view.findViewById(R.id.clinical_info_layout);

        myInfoLayout.setOnClickListener(v -> {
            requireActivity().getSupportFragmentManager()
                    .beginTransaction()
                    .replace(R.id.fl_main, new MyInfoFragment())
                    .addToBackStack(null)  // IMPORTANT: add to back stack!
                    .commit();
        });


        pgmInfoLayout.setOnClickListener(v -> {
            // Replace current fragment with MyInfoFragment
            FragmentManager fragmentManager = requireActivity().getSupportFragmentManager();
            fragmentManager.beginTransaction()
                    .replace(R.id.fl_main, new ProgramInfoFragment())  // Use fl_main here
                    .addToBackStack(null)
                    .commit();

        });

        clinicalInfoLayout.setOnClickListener(v -> {
            ClinicalInfoFragment clinicalInfoFragment = new ClinicalInfoFragment();

            // Create a Bundle to pass arguments
            Bundle args = new Bundle();
            args.putString("opened_from", "MORE");  // set your argument here
            clinicalInfoFragment.setArguments(args);

            // Replace fragment with back stack
            requireActivity().getSupportFragmentManager()
                    .beginTransaction()
                    .replace(R.id.fl_main, clinicalInfoFragment)
                    .addToBackStack(null)
                    .commit();
        });


        // If you had set system bar color for Activity, you can call it in onViewCreated()
        SystemBarColor.setSystemBarColor(requireActivity(), R.color.Primary_bg);

        db = new DataBaseHelper(requireContext());
        pref = requireContext().getSharedPreferences("RPMUserApp", Context.MODE_PRIVATE);
        editor = pref.edit();

        String User_full_Name = pref.getString("User_full_name", null);
        String Patent_Id = pref.getString("Patent_id", null);
        Token = pref.getString("Token", null);

        // Init toolbar if needed
         toolbar = view.findViewById(R.id.toolbar);


        toolbar.setNavigationIcon(R.drawable.ic_baseline_west_24);
        toolbar.setNavigationOnClickListener(v -> {
            // Example: Switch bottom nav to dashboard/home fragment
            BottomNavigationView bottomNav = requireActivity().findViewById(R.id.navigation);
            if (bottomNav != null) {
                bottomNav.setSelectedItemId(R.id.Home);
            }
        });
        // For fragments, toolbar setup usually happens in parent Activity

        tvusername = view.findViewById(R.id.act_more_username);
        tvuserid = view.findViewById(R.id.act_more_userid);
        Logout_btn = view.findViewById(R.id.act_more_logoff);

        tvusername.setText("Hi, " + User_full_Name);
        tvuserid.setText(Patent_Id);

        LinearLayout linearLayout = view.findViewById(R.id.linmore);
        linearLayout.setOnClickListener(v ->
                Toast.makeText(requireContext(), "Coming Soon!", Toast.LENGTH_SHORT).show()
        );

        Logout_btn.setOnClickListener(v -> logOff(Token));

        return view;
    }


    @Override
    public void onResume() {
        super.onResume();
        if (!pref.getBoolean("loginstatus", false)) {
            editor.clear();
            editor.commit();
            db.deleteProfileData("myprofileandprogram");
            db.deleteData();

            try {
                Intent intentlogout = new Intent(requireContext(), Login.class);
                intentlogout.addFlags(Intent.FLAG_ACTIVITY_NEW_TASK | Intent.FLAG_ACTIVITY_CLEAR_TASK);
                startActivity(intentlogout);
                requireActivity().finish();
            } catch (Exception e) {
                Log.e("onLogOff Clear", e.toString());
            }
        }
    }

    private void logOff(String tokenVALUE) {
        // 1️ Get Firebase token from SharedPreferences
        String fbtoken = pref.getString("FirebaseToken", null);

        if (fbtoken != null) {
            // Call delete API first
            firebasetokenDeletion(fbtoken, tokenVALUE);
        } else {
            // No FCM token saved, skip directly to logout API
            callLogoutApi(tokenVALUE);
        }
    }

    /**
     * Delete Firebase token from backend before logout.
     */
    private void firebasetokenDeletion(String fbtoken, String tokenVALUE) {
        String FB_token_delete_URL = BASE_URL + FB_TOK_DELETE; // backend delete endpoint
        RequestQueue requestQueue = Volley.newRequestQueue(requireContext());

        StringRequest stringRequest = new StringRequest(Request.Method.POST, FB_token_delete_URL + fbtoken,
                response -> {
                    Log.e("fbDeletionSuccess", response);
                    // after deletion → call logout API
                    callLogoutApi(tokenVALUE);
                },
                error -> {
                    Log.e("firebasetokenDeletionFailed", error.toString());
                    // even if delete fails → continue logout
                    callLogoutApi(tokenVALUE);
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

    /**
     *  Logout API call.
     */
    private void callLogoutApi(String tokenVALUE) {
        String LOGOUT_URL = BASE_URL + LOGOUT.toString();

        StringRequest request = new StringRequest(Request.Method.POST, LOGOUT_URL,
                response -> {
                    Log.i("LogoutAPI", "Success: " + response);
                    doLocalLogoutCleanup();
                },
                error -> {
                    Log.e("LogoutAPI", "Error: " + error.toString());
                    doLocalLogoutCleanup(); // force local cleanup anyway
                }) {
            @Override
            public Map<String, String> getHeaders() throws AuthFailureError {
                Map<String, String> headers = new HashMap<>();
                headers.put("Bearer", tokenVALUE);
                headers.put("Content-Type", "application/json");
                return headers;
            }
        };

        RequestQueue queue = Volley.newRequestQueue(requireContext());
        queue.add(request);
    }

    /**
     * Local cleanup (Twilio, DB, SharedPreferences, navigation).
     */
    private void doLocalLogoutCleanup() {
        try {
            TwilioChatManager.getInstance().clearChatList();
            unsubscribeFromTopicsOnLogout(requireContext());

            FirebaseMessaging.getInstance().deleteToken()
                    .addOnCompleteListener(task -> {
                        if (task.isSuccessful()) {
                            Log.d("LogoutFIREBASE", "FCM token deleted locally");
                        }
                    });

            editor.clear();
            editor.apply();
            db.deleteData();
            db.deleteProfileData("myprofileandprogram");

            ConversationsClientManager.getInstance().clearConversationsClient();

            Intent more = new Intent(requireContext(), Login.class);
            more.addFlags(Intent.FLAG_ACTIVITY_CLEAR_TOP |
                    Intent.FLAG_ACTIVITY_NEW_TASK |
                    Intent.FLAG_ACTIVITY_CLEAR_TASK);
            startActivity(more);
            requireActivity().finish();

        } catch (Exception e) {
            Log.e("onLogOff Cleanup Error", e.toString());
        }
    }

    public static void unsubscribeFromTopicsOnLogout(Context context) {
        SharedPreferences prefs = context.getSharedPreferences("RPMUserApp", Context.MODE_PRIVATE);
        String token = prefs.getString("FirebaseToken", null);

        if (token != null) {
            FirebaseMessaging.getInstance().unsubscribeFromTopic("videocall")
                    .addOnCompleteListener(task -> {
                        if (task.isSuccessful()) {
                            Log.d("FCM", "Successfully unsubscribed from topic: videocall");
                        } else {
                            Log.e("FCM", "Failed to unsubscribe from topic", task.getException());
                        }
                    });


            SharedPreferences.Editor editor = prefs.edit();
            editor.remove("FirebaseToken");
            editor.apply();
        }
    }
}
