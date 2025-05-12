package com.rpm.clynx.service;

import android.content.BroadcastReceiver;
import android.content.Context;
import android.content.Intent;
import android.os.Bundle;
import android.util.Log;
import androidx.localbroadcastmanager.content.LocalBroadcastManager;

public class NotificationReceiver extends BroadcastReceiver {

    public static final String ACTION_NOTIFICATION_RECEIVED = "com.rpm.clynx.NOTIFICATION_RECEIVED";
    public static final String EXTRA_NOTIFICATION_DATA = "notificationData";
    private String title,body;
    @Override
    public void onReceive(Context context, Intent intent) {
        Log.d("Broadcastcontext", String.valueOf(context));
        Log.d("BroadcastNotification", String.valueOf(intent));
        System.out.println("BroadcastNotification");
        if (intent.getAction().equals(ACTION_NOTIFICATION_RECEIVED)) {
            // Retrieve the notification data from the intent
            Bundle notificationData = intent.getBundleExtra(EXTRA_NOTIFICATION_DATA);
                System.out.println("Notiftitle ---");
                System.out.println("Notifbody--- ");
                System.out.println(title);
                System.out.println(body);
            // Broadcast the notification data to all activities in the app
            Intent broadcastIntent = new Intent(ACTION_NOTIFICATION_RECEIVED);
            broadcastIntent.putExtra(EXTRA_NOTIFICATION_DATA, notificationData);
            LocalBroadcastManager.getInstance(context).sendBroadcast(broadcastIntent);
        }
    }
}
