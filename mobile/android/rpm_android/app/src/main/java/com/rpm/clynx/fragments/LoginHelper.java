package com.rpm.clynx.fragments;

import static com.rpm.clynx.utility.Links.BASE_URL;
import static com.rpm.clynx.utility.Links.FB_TOK_SAVE;
import static com.rpm.clynx.utility.Links.MEMBERS_LIST;

import android.content.Context;
import android.content.SharedPreferences;
import android.util.Log;
import android.widget.Toast;

import com.android.volley.AuthFailureError;
import com.android.volley.DefaultRetryPolicy;
import com.android.volley.Request;
import com.android.volley.RequestQueue;
import com.android.volley.toolbox.StringRequest;
import com.android.volley.toolbox.Volley;

import java.util.HashMap;
import java.util.Map;

public class LoginHelper {

    public static void firebasetokenInsertion(Context context, String token, String fbtoken) {
        String FB_token_save_URL = BASE_URL + FB_TOK_SAVE;
        RequestQueue requestQueue = Volley.newRequestQueue(context);
        StringRequest stringRequest = new StringRequest(Request.Method.POST, FB_token_save_URL + fbtoken,
                response -> Log.e("firebasetokenInsertionSuccess", response),
                error -> Log.e("firebasetokenInsertionFailed", error.toString())) {
            @Override
            public Map<String, String> getHeaders() throws AuthFailureError {
                Map<String, String> headers = new HashMap<>();
                headers.put("Bearer", token);
                return headers;
            }
        };
        requestQueue.add(stringRequest);
    }

    public static void membersList(Context context, String token) {
        String MEMBERS_LIST_URL = BASE_URL + MEMBERS_LIST;
        StringRequest stringRequest = new StringRequest(Request.Method.GET, MEMBERS_LIST_URL,
                response -> saveMembersList(context, response),
                error -> Toast.makeText(context, "error", Toast.LENGTH_LONG).show()) {
            @Override
            public Map<String, String> getHeaders() throws AuthFailureError {
                Map<String, String> headers = new HashMap<>();
                headers.put("Bearer", token);
                return headers;
            }
        };
        RequestQueue requestQueue = Volley.newRequestQueue(context);
        stringRequest.setRetryPolicy(new DefaultRetryPolicy(0, -1, DefaultRetryPolicy.DEFAULT_BACKOFF_MULT));
        requestQueue.add(stringRequest);
    }

    private static void saveMembersList(Context context, String response) {
        SharedPreferences sharedPreferences = context.getSharedPreferences("RPMUserApp", Context.MODE_PRIVATE);
        SharedPreferences.Editor editor = sharedPreferences.edit();
        editor.putString("members_list", response);
        editor.apply();
        Log.d("SharedPreferences", "Members list saved successfully");
    }
}
