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
        FileLogger.d("remoteMessageRCVD", remoteMessage.toString());
        super.onMessageReceived(remoteMessage);

        // Log notification payload (if any)
        if (remoteMessage.getNotification() != null) {
            FileLogger.d("Notification", "Notification Payload Received");
            String notifTitle = remoteMessage.getNotification().getTitle();
            String notifBody = remoteMessage.getNotification().getBody();
            FileLogger.d("FCM", "Title: " + notifTitle + ", Body: " + notifBody);
        }

        // Log data payload
        if (remoteMessage.getData().size() > 0) {
            FileLogger.d("Data", "Data Payload Received");
            for (Map.Entry<String, String> entry : remoteMessage.getData().entrySet()) {
                FileLogger.d("FCM", "Key: " + entry.getKey() + ", Value: " + entry.getValue());
            }
        }

        Map<String, String> data = remoteMessage.getData();
        String title = data.get("Title");
        String body = data.get("Body");
        String messageType = data.get("twi_message_type");

        // Normalize helpers (guard nulls)
        String titleLower = title != null ? title.trim().toLowerCase() : null;

        // ---------- 1) Chat detection ----------
        boolean isTwilioNewMsg = "twilio.conversations.new_message".equals(messageType);
        boolean isTitleChat = titleLower != null && titleLower.startsWith("new message from");
        if (isTwilioNewMsg || isTitleChat) {
            // Prefer twi_body if present (Twilio standard), else fallback to Body/title
            String chatBody = null;
            if (data.get("twi_body") != null) {
                chatBody = extractMessageContent(data.get("twi_body"));
            } else if (body != null && !body.isEmpty()) {
                chatBody = body;
            } else if (data.get("Body") != null) {
                chatBody = data.get("Body");
            } else {
                chatBody = "You have a new message";
            }

            String conversationTitle = data.get("conversation_title");
            String chatTitle = conversationTitle != null ? conversationTitle : (title != null ? title : "New Chat Message");

            FileLogger.d("TwilioChatNotification", "Title: " + chatTitle + ", Message: " + chatBody);
            showChatNotification(getApplicationContext(), chatTitle, chatBody);

            // Broadcast for fragments / UI
            Intent chatIntent = new Intent("com.rpm.clynx.NOTIFICATION_RECEIVED");
            chatIntent.putExtra("type", "chat");
            chatIntent.putExtra("title", chatTitle);
            chatIntent.putExtra("body", chatBody);
            LocalBroadcastManager.getInstance(getApplicationContext()).sendBroadcast(chatIntent);

            return; // handled
        }

        // ---------- 2) Video call detection ----------
        boolean isVideoCall = titleLower != null && titleLower.startsWith("video call from");
        if (isVideoCall) {
            FileLogger.d("VideoCallNotification", "Incoming video call -> Title=" + title + ", Body=" + body);

            // get app & activity state
            latestActivity = ((MyApplication) getApplication()).getLatestActivity();
            MyApplication appFG = (MyApplication) getApplicationContext();
            FileLogger.d("appFG", String.valueOf(appFG.isAppInForeground()));

            if (appFG.isAppInForeground()) {
                FileLogger.d("myApplicationisAppInForeground", "myApplicationisAppInForeground");

                Bundle notificationData = new Bundle();
                notificationData.putString("title", title);
                notificationData.putString("body", body);

                boolean onCallstatus = pref.getBoolean("onCall", false);
                FileLogger.d("onCallstatus1", String.valueOf(onCallstatus));

                if (!onCallstatus) {
                    FileLogger.d("onCallstatus2", String.valueOf(onCallstatus));
                    FileLogger.d("latestActivity", String.valueOf(latestActivity));
                    FileLogger.d("notificationData", String.valueOf(notificationData));

                    Intent intent = new Intent("com.rpm.clynx.NOTIFICATION_RECEIVED");
                    intent.putExtra("notificationData", notificationData);
                    intent.putExtra("type", "video");

                    Context context = (latestActivity != null) ? latestActivity : getApplicationContext();
                    LocalBroadcastManager.getInstance(context).sendBroadcast(intent);

                    FileLogger.d("sendbroadcastdone", String.valueOf(latestActivity));
                } else {
                    FileLogger.d("onCallstatus", "Call already active. Skipping notification broadcast.");
                }
            } else {
                FileLogger.d("myApplicationisAppInbg", "myApplicationisAppInbg");
                showMissedCallNoti(title);
                FileLogger.d("MissedCallNotification", "Notification triggered in background.");
            }
            return; // handled
        }

        // ---------- 3) Other / fallback ----------
        FileLogger.d("OtherNotification", "Unhandled notification: " + data.toString());
        FileLogger.d("From", remoteMessage.getFrom());
        FileLogger.d("remoteMessagetData", String.valueOf(remoteMessage.getData()));
        FileLogger.d("remoteMessagetNotification", String.valueOf(remoteMessage.getNotification()));
        FileLogger.d("SystemOutRemoteMessage", remoteMessage.toString());
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
              //  .setContentText(message)
                .setAutoCancel(true)
                .setPriority(NotificationCompat.PRIORITY_HIGH);

        // Show the notification
        notificationManager.notify(1, builder.build());
    }
}
