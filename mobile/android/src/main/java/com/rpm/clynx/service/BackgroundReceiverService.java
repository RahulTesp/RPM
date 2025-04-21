package com.rpm.clynx.service;

import android.app.Service;
import android.content.BroadcastReceiver;
import android.content.Context;
import android.content.Intent;
import android.content.IntentFilter;
import android.os.Bundle;
import android.os.IBinder;
import androidx.appcompat.app.AlertDialog;
import javax.annotation.Nullable;

public class BackgroundReceiverService extends Service {

    private String title,body;
    public static final String EXTRA_NOTIFICATION_DATA = "notificationData";
    private BroadcastReceiver notificationReceiver = new BroadcastReceiver() {
        @Override
        public void onReceive(Context context, Intent intent) {
            System.out.println("BackgroundReceiverService onReceive");
            if (intent.getAction().equals("com.rpm.clynx.NOTIFICATION_RECEIVED")) {
                // Handle the received notification data
                Bundle notificationData = intent.getBundleExtra(EXTRA_NOTIFICATION_DATA);
                 title = notificationData.getString("title");
                 body = notificationData.getString("body");
                System.out.println(" BackgroundReceiverService Notification");
                System.out.println("Notificationbody title ---");
                System.out.println("Notificationbody body--- ");
                System.out.println(title);
                System.out.println(body);
                if (body != null) {
                    System.out.println("alert success");
                    // Show an alert dialog
                    showNotificationAlert();
                }
            }
        }
    };
    private void showNotificationAlert() {
        AlertDialog.Builder builder = new AlertDialog.Builder(this);
        builder.setTitle("Notification Received")
                .setMessage("You have received a new notification.")
                .setPositiveButton("OK", null)
                .show();
    }
    @Override
    public void onCreate() {
        super.onCreate();
        // Register the notification receiver
        IntentFilter intentFilter = new IntentFilter("com.rpm.clynx.NOTIFICATION_RECEIVED");
        registerReceiver(notificationReceiver, intentFilter);
    }

    @Override
    public int onStartCommand(Intent intent, int flags, int startId) {
        // Perform any initialization or setup here
        // ...

        // Return START_STICKY to ensure the service restarts if killed by the system
        return START_STICKY;
    }

    @Override
    public void onDestroy() {
        super.onDestroy();
        // Unregister the notification receiver
        unregisterReceiver(notificationReceiver);
    }

    @Nullable
    @Override
    public IBinder onBind(Intent intent) {
        return null;
    }
}
