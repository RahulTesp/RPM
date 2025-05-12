package com.rpm.clynx.utility;

import android.app.Activity;
import android.app.Application;
import android.content.Context;
import android.content.SharedPreferences;
import android.os.Bundle;
import android.util.Log;
import java.util.HashSet;
import java.util.Set;

public class MyApplication extends Application implements Application.ActivityLifecycleCallbacks {

    private boolean appInForeground;
    private static Activity latestActivity;
    @Override
    public void onCreate() {
        super.onCreate();
        registerActivityLifecycleCallbacks(this);
        // Retrieve stored Conversation SID
        SharedPreferences pref = getApplicationContext().getSharedPreferences("RPMUserApp", Context.MODE_PRIVATE);
        Set<String> conversationSIDs = pref.getStringSet("ConversationSIDs", new HashSet<>());
    }

    @Override
    public void onActivityCreated(Activity activity, Bundle savedInstanceState) {
        Log.d("onActivityCreated", String.valueOf(latestActivity));
        latestActivity = activity;
    }

    @Override
    public void onActivityStarted(Activity activity) {
        Log.d("onActivityStarted", String.valueOf(latestActivity));
        latestActivity = activity;
    }

    @Override
    public void onActivityResumed(Activity activity) {
        appInForeground = true;
        // Set the latest activity when it's resumed
        latestActivity = activity;
        Log.d("onActivityResumed", String.valueOf(latestActivity));
    }

    @Override
    public void onActivityPaused(Activity activity) {
        appInForeground = false;
    }

    @Override
    public void onActivityStopped(Activity activity) {
    }

    @Override
    public void onActivitySaveInstanceState(Activity activity, Bundle outState) {
    }
    @Override
    public void onActivityDestroyed(Activity activity) {
    }

    // Getter method to retrieve the latest activity
    public static Activity getLatestActivity() {
        return latestActivity;
    }

    public boolean isAppInForeground() {
        return appInForeground;
    }
}
