package com.rpm.clynx.auth;

import com.android.volley.NetworkResponse;
import com.android.volley.Request;
import com.android.volley.Response;

public class StatusCodeRequest extends Request<Void> {
    private final Response.Listener<Integer> listener;

    public StatusCodeRequest(int method, String url, Response.Listener<Integer> listener, Response.ErrorListener errorListener) {
        super(method, url, errorListener);
        this.listener = listener;
    }

    @Override
    protected Response<Void> parseNetworkResponse(NetworkResponse response) {
        int statusCode = response.statusCode;
        // Pass the status code to the listener
        listener.onResponse(statusCode);
        // Return null since we don't require a response body
        return null;
    }

    @Override
    protected void deliverResponse(Void response) {
        // No response body to deliver
    }
}