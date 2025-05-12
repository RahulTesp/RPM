package com.rpm.clynx.service;

import android.Manifest;
import android.app.Activity;
import android.app.NotificationChannel;
import android.app.NotificationManager;
import android.content.Context;
import android.content.Intent;
import android.content.SharedPreferences;
import android.content.pm.PackageManager;
import android.os.Build;
import android.os.Bundle;
import android.util.Log;
import androidx.annotation.NonNull;
import androidx.annotation.RequiresApi;
import androidx.core.app.ActivityCompat;
import androidx.core.app.NotificationCompat;
import androidx.core.app.NotificationManagerCompat;
import androidx.localbroadcastmanager.content.LocalBroadcastManager;
import com.google.firebase.messaging.FirebaseMessagingService;
import com.google.firebase.messaging.RemoteMessage;
import com.rpm.clynx.R;
import com.rpm.clynx.fragments.DashboardFragment;
import com.rpm.clynx.fragments.NotificationFragment;
import com.rpm.clynx.utility.ConversationsClientManager;
import com.rpm.clynx.utility.FileLogger;
import com.rpm.clynx.utility.MyApplication;
import com.twilio.conversations.ConversationsClient;
import com.twilio.util.ErrorInfo;
import com.twilio.conversations.StatusListener;
import java.util.Map;

public class PushNotificationService extends FirebaseMessagingService  {
    Activity latestActivity;
    boolean onCallstatus;
    String title;

SharedPreferences pref;
    SharedPreferences.Editor editor;
    @Override
    public void onCreate() {
        super.onCreate();
        Log.d("heloonCreate","heloonCreate");
        pref = getApplicationContext().getSharedPreferences("RPMUserApp", MODE_PRIVATE);
        editor = pref.edit();

        MyApplication myApplication = (MyApplication) getApplicationContext();

           if   (!(myApplication.isAppInForeground())) {

               Log.d("misAppInForeground", String.valueOf(myApplication.isAppInForeground()));
           }
    }

    private static final String TAG = "PushNotificationService";
    //Override onNewToken to get new token

    @Override
    public void onNewToken(@NonNull String token)
    {
        Log.d(TAG, "Refreshed token: " + token);
        Intent intent = new Intent("com.example.NEW_TOKEN_RECEIVED");
        intent.putExtra("onNewToken", token);
        LocalBroadcastManager.getInstance(this).sendBroadcast(intent);
        Log.d("TwilioChat", "New FCM token: " + token);
        ConversationsClient convClientVal = ConversationsClientManager.getInstance().getConvClient();
        if (convClientVal != null) {
            ConversationsClient.FCMToken twilioFCMToken = new ConversationsClient.FCMToken(token);

            convClientVal
                    .registerFCMToken(twilioFCMToken, new StatusListener() {
                        @Override
                        public void onSuccess() {
                            Log.d("TwilioChat", "FCM token updated successfully with Twilio");
                        }

                        @Override
                        public void onError(ErrorInfo errorInfo) {
                            Log.e("TwilioChat", "Error updating FCM token: " + errorInfo.getMessage());
                        }
                    });
        }
    }

    @RequiresApi(api = Build.VERSION_CODES.O)
    @Override
    public void onMessageReceived(RemoteMessage remoteMessage) {
        Log.d("remoteMessageRCVD", remoteMessage.toString());
        FileLogger.d("remoteMessageRCVD", remoteMessage.toString());
        super.onMessageReceived(remoteMessage);

        if (DashboardFragment.DashboardFragmentIsVisible || NotificationFragment.NotificationFragmentIsVisible) {
            Intent intent = new Intent(NotificationReceiver.ACTION_NOTIFICATION_RECEIVED);
            intent.putExtra(NotificationReceiver.EXTRA_NOTIFICATION_DATA, remoteMessage.getData().toString());
            sendBroadcast(intent);
        } else {
            Log.d("remoteMessageRCVD", "Dashboard or Notification Fragment not visible, not sending broadcast");
        }

        //  Check if message contains a notification payload
        if (remoteMessage.getNotification() != null) {
            Log.d("Notification", "Notification Payload Received");
            String title = remoteMessage.getNotification().getTitle();
            String body = remoteMessage.getNotification().getBody();
            Log.d("FCM", "Title: " + title + ", Body: " + body);
        }

        //  Check if message contains a data payload
        if (remoteMessage.getData().size() > 0) {
            Log.d("Data", "Data Payload Received");
            Map<String, String> data = remoteMessage.getData();
            for (Map.Entry<String, String> entry : data.entrySet()) {
                Log.d("FCM", "Key: " + entry.getKey() + ", Value: " + entry.getValue());
            }
        }

             title = remoteMessage.getData().get("Title");  // Key is case-sensitive
            String videoCallbody = remoteMessage.getData().get("Body");
            Log.d("TwilioChatRECEIVED", "Push notification received!");
            Log.d("TwilioChatData", String.valueOf(remoteMessage.getData()));
            Log.d("TwilioChat", "Push received from: " + remoteMessage.getFrom());

        // Extract message type
        String messageType = remoteMessage.getData().get("twi_message_type");

        // Check if it's a new Twilio message
        if ("twilio.conversations.new_message".equals(messageType)) {
            // Extract conversation title and message body
            String conversationTitle = remoteMessage.getData().get("conversation_title"); // Chat name
            String rawMessageBody = remoteMessage.getData().get("twi_body"); // Full message with sender info
            // Extract only the actual message after ":"
            String messageBody = extractMessageContent(rawMessageBody);

            // Default values if null
            String title = (conversationTitle != null) ? conversationTitle : "New Chat Message";
            String body = (messageBody != null) ? messageBody : "You have a new message";

            // Show the notification
            showChatNotification(getApplicationContext(), title, body);
        }

        Log.d("From",remoteMessage.getFrom());
        Log.d("From", String.valueOf(remoteMessage));
        Log.d("remoteMessagetData", String.valueOf(remoteMessage.getData()));
        Log.d("remoteMessagetNotification", String.valueOf(remoteMessage.getNotification()));

        latestActivity = ((MyApplication) getApplication()).getLatestActivity();

        Log.d("pushservclatestActivity", String.valueOf(latestActivity));
        MyApplication appFG = (MyApplication) getApplicationContext();
        Log.d("appFG", String.valueOf(appFG.isAppInForeground()));

            System.out.println("remoteMessage");
            System.out.println(remoteMessage);

                if (appFG.isAppInForeground()) {
                    Log.d("myApplicationisAppInForeground","myApplicationisAppInForeground");
                        System.out.println("iaminforegnd");

                    // Create an intent for the notification data
                    Bundle notificationData = new Bundle();
                    notificationData.putString("title",remoteMessage.getData().get("Title"));
                    notificationData.putString("body", remoteMessage.getData().get("Body"));

                    Log.d("NotificationonCall", String.valueOf(pref.getBoolean("onCall", false)));

                    onCallstatus = pref.getBoolean("onCall", false);
                    FileLogger.d("onCallstatus1", String.valueOf(onCallstatus));
                    if (onCallstatus == false) {
                        Log.d("onCallstatus", String.valueOf(onCallstatus));
                        FileLogger.d("onCallstatus2", String.valueOf(onCallstatus));
                        Log.d("latestActivity", String.valueOf(latestActivity));
                        Log.d("notificationData", String.valueOf(notificationData));
                        FileLogger.d("notificationData", String.valueOf(notificationData));
                        Intent intentnew = new Intent(NotificationReceiver.ACTION_NOTIFICATION_RECEIVED);
                        intentnew.putExtra(NotificationReceiver.EXTRA_NOTIFICATION_DATA, notificationData);
                        Log.d("sendingvdbroadcast","sendingvdbroadcast");
                        LocalBroadcastManager.getInstance(latestActivity).sendBroadcast(intentnew);
                   Log.d("sendvidbroadcast","sendvidbroadcast");
                    }

                    else {
                    }
                }

                else if (!(appFG.isAppInForeground()))
                {
                    Log.d("myApplicationisAppInbg","myApplicationisAppInbg");
                    // App is in the background → Show notification in the notification panel
                    showMissedCallNoti(title);
                    // Handle the received notification here
                    System.out.println(appFG.isAppInForeground());
                }
        }

    private void showMissedCallNoti(String callerName) {
        Log.d("showMissedvideocl", "showMissedvideocl");

        // Create a notification for missed call
        NotificationCompat.Builder builder = new NotificationCompat.Builder(getApplicationContext(), "NOTIFICATION")
                .setSmallIcon(R.drawable.icons_notification)
                .setContentTitle("CLYNX CARE")
                .setContentText("You have a missed " + callerName + "!")  // Show only title in panel
                .setPriority(NotificationCompat.PRIORITY_HIGH)
                .setAutoCancel(true);

        builder.setDefaults(NotificationCompat.DEFAULT_SOUND); // Enable default sound

        // Optional: If you want to allow expansion, use BigTextStyle
        NotificationCompat.BigTextStyle bigTextStyle = new NotificationCompat.BigTextStyle();
        bigTextStyle.setBigContentTitle("CLYNX CARE");
        bigTextStyle.bigText("You have a missed " + callerName + "!");

        builder.setStyle(bigTextStyle);

        // Create a notification manager
        NotificationManagerCompat notificationManager = NotificationManagerCompat.from(this);

        // Show the notification
        if (ActivityCompat.checkSelfPermission(this, Manifest.permission.POST_NOTIFICATIONS) != PackageManager.PERMISSION_GRANTED) {
            // TODO: Consider calling
            //    ActivityCompat#requestPermissions
            // here to request the missing permissions, and then overriding
            //   public void onRequestPermissionsResult(int requestCode, String[] permissions,
            //                                          int[] grantResults)
            // to handle the case where the user grants the permission. See the documentation
            // for ActivityCompat#requestPermissions for more details.
            return;
        }
        notificationManager.notify(1, builder.build());
        Log.d("NOTIFICATION", "Notification should be displayed now!");
    }

    private String extractMessageContent(String twiBody) {
        if (twiBody != null && twiBody.contains(":")) {
            return twiBody.substring(twiBody.indexOf(":") + 1).trim();
        }
        return twiBody; // Return original if no colon is found
    }

    private void showChatNotification(Context context, String title, String message) {
        String channelId = "twilio_chat_channel";
        NotificationManager notificationManager = (NotificationManager) context.getSystemService(Context.NOTIFICATION_SERVICE);

        // Create notification channel for Android 8.0+
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.O) {
            NotificationChannel channel = new NotificationChannel(
                    channelId,
                    "Twilio Chat Notifications",
                    NotificationManager.IMPORTANCE_HIGH
            );
            notificationManager.createNotificationChannel(channel);
        }

        // Build the notification
        NotificationCompat.Builder builder = new NotificationCompat.Builder(context, channelId)
                .setSmallIcon(R.drawable.icons_notification)  // Change to your app's notification icon
                .setContentTitle(title)
                .setContentText(message)
                .setAutoCancel(true)
                .setPriority(NotificationCompat.PRIORITY_HIGH);

        // Show the notification
        notificationManager.notify(1, builder.build());
    }
}
