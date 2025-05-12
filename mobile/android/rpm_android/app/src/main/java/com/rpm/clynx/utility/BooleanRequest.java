package com.rpm.clynx.utility;

import com.android.volley.NetworkResponse;
import com.android.volley.Request;
import com.android.volley.Response;

public class BooleanRequest extends Request<Boolean> {

    private final Response.Listener<Boolean> listener;

    public BooleanRequest(int method, String url, Response.Listener<Boolean> listener, Response.ErrorListener errorListener) {
        super(method, url, errorListener);
        this.listener = listener;
    }

    @Override
    protected Response<Boolean> parseNetworkResponse(NetworkResponse response) {
        // Parse the network response and return true or false based on your conditions
        // For example, check HTTP status codes, content, etc.
        // Modify the parsing logic based on your API response structure
        int statusCode = response.statusCode;

        // Check if the status code indicates success (e.g., 200 OK)
        boolean isSuccess = statusCode >= 200 && statusCode < 300;

        return Response.success(isSuccess, getCacheEntry());
    }

    @Override
    protected void deliverResponse(Boolean response) {
        if (listener != null) {
            listener.onResponse(response);
        }
    }
}
