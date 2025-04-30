package com.rpm.clynx;

import androidx.appcompat.app.AppCompatActivity;
import android.content.SharedPreferences;
import android.os.Bundle;
import android.os.Handler;

public class MainActivity extends AppCompatActivity {

    String Username, videocallToken, NotificationBody , NotificationTitle , CurrentTime;
    SharedPreferences pref;
    SharedPreferences.Editor editor;
    String Token;
    private Handler handler;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
      //  setContentView(R.layout.activity_main);
        setContentView(R.layout.dialog_notification);
      //  TextView messageTextView = findViewById(R.id.message_text_view);
        System.out.println("NotificationDialogmessage");

        pref = getApplicationContext().getSharedPreferences("RPMUserApp", MODE_PRIVATE);
        Token = pref.getString("Token", null);
        editor = pref.edit();
        Username = pref.getString("UserName", null);
        System.out.println("Username  Username");
        System.out.println(Username);

        // Initialize the handler
        handler = new Handler();
        // Start the timer when the activity is resumed

        // Check if the intent contains fragment information
        if (getIntent().getExtras() != null) {
            NotificationBody = getIntent().getStringExtra("alert");
            NotificationTitle = getIntent().getStringExtra("title");
            System.out.println("homeii  alert");
            System.out.println("alert body");
            System.out.println(NotificationBody);
        }
    }
}