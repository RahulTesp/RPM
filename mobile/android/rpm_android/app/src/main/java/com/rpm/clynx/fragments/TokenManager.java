package com.rpm.clynx.fragments;

import android.content.Context;
import android.util.Log;
import android.widget.Toast;

import androidx.annotation.Nullable;

public class TokenManager {
    private static TokenManager instance;
    private Context context;
    private QuickstartConversationsManager quickstartConversationsManager;

    private TokenManager(Context context) {
        this.context = context.getApplicationContext(); // safe context
        quickstartConversationsManager = new QuickstartConversationsManager(context);
    }

    public static synchronized TokenManager getInstance(Context context) {
        if (instance == null) {
            instance = new TokenManager(context);
        }
        return instance;
    }

    public void retrieveTokenFromServer(String appAccessToken) {
        Log.d("TokenManager", "AppAccessToken: " + appAccessToken);
        quickstartConversationsManager.retrieveAccessTokenFromServer(context, appAccessToken, new TokenResponseListener() {
            @Override
            public void receivedTokenResponse(boolean success, @Nullable Exception exception) {
                if (success) {
                    Log.d("TokenManager", "Token retrieved successfully");
                } else {
                    String errorMessage = "Error retrieving token";
                    if (exception != null) {
                        errorMessage += " " + exception.getLocalizedMessage();
                    }
                    Toast.makeText(context, errorMessage, Toast.LENGTH_LONG).show();
                }
            }
        });
    }
}
