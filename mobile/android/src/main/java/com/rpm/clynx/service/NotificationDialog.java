package com.rpm.clynx.service;

import android.app.Dialog;
import android.content.Context;
import android.os.Bundle;
import android.view.Window;
import android.widget.TextView;
import com.rpm.clynx.R;
public class NotificationDialog extends Dialog {
    private String message;

    public NotificationDialog(Context context, String message) {
        super(context);
        this.message = message;
    }

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        requestWindowFeature(Window.FEATURE_NO_TITLE);
        setContentView(R.layout.dialog_notification);

        TextView messageTextView = findViewById(R.id.message_text_view);
        System.out.println("NotificationDialogmessage");
        System.out.println(message);
        messageTextView.setText(message);
    }
}
