package com.rpm.clynx.service;

import android.content.BroadcastReceiver;
import android.content.Context;
import android.content.Intent;
import android.util.Log;
import com.rpm.clynx.fragments.Login;

public class MyLogoutReceiver extends BroadcastReceiver {
    @Override
    public void onReceive(Context context, Intent intent) {
        Log.d("contextINTENT", String.valueOf(context));
        Log.d("intentGET", String.valueOf(intent.getAction()));
        if (intent != null && "com.rpm.clynx.ACTION_LOGOUT_RESULT".equals(intent.getAction())) {
            // Handle the broadcast here
            Log.d("intentMYBROAD", String.valueOf(intent));

            boolean logoutResult = intent.getBooleanExtra("logoutResult", false);
            String logToken = intent.getStringExtra("LOGINTOKEN");
            // Handle the logout result based on the received data
            Log.d("logoutResultsSS", String.valueOf(logoutResult));

            Log.d("logTokenVAL", String.valueOf(logToken));
            // Handle the logout result here
            if (logoutResult) {
                Log.d("logoutRes", String.valueOf(logoutResult));
                Log.d("logininstan", String.valueOf(Login.getInstance()));
                // Perform actions related to successful logout
            } else {
                Log.d("logoutResultf", String.valueOf(logoutResult));
                // Perform actions related to failed logout
            }
        }
    }
}
