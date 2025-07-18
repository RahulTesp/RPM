// NetworkErrorHandler.java
package com.rpm.clynx.utility;

import android.content.Context;
import android.content.Intent;
import android.util.Log;
import android.widget.Toast;

import com.android.volley.VolleyError;
import com.rpm.clynx.fragments.Login;

import android.content.SharedPreferences;


public class NetworkErrorHandler {

    public static void handleError(Context context, VolleyError error, SharedPreferences.Editor editor, DataBaseHelper db) {
        if (error == null || error.networkResponse == null) {
            if (context != null) {
                NetworkAlert.showNetworkDialog(context);
            }
            return;
        }

        int statusCode = error.networkResponse.statusCode;
        if (statusCode == 401) {
            editor.putBoolean("loginstatus", false);
            editor.apply();
            db.deleteProfileData("myprofileandprogram");
            db.deleteData();
            editor.clear();
            editor.commit();
            try {
                Intent intentlogout = new Intent(context, Login.class);
                intentlogout.addFlags(Intent.FLAG_ACTIVITY_NEW_TASK | Intent.FLAG_ACTIVITY_CLEAR_TASK);
                context.startActivity(intentlogout);
            } catch (Exception e) {
                Log.e("onLogOff Clear", e.toString());
            }
        } else if (statusCode == 400) {
            Log.e("API_ERROR", "Bad Request: " + new String(error.networkResponse.data));
        } else {
            Toast.makeText(context, "Something went wrong!", Toast.LENGTH_SHORT).show();
        }
    }
}
