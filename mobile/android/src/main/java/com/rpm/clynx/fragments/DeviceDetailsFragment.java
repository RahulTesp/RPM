package com.rpm.clynx.fragments;

        import android.content.SharedPreferences;
        import android.os.Bundle;
        import android.util.Log;
        import android.view.LayoutInflater;
        import android.view.View;
        import android.view.ViewGroup;
        import android.widget.TextView;
        import androidx.fragment.app.Fragment;
        import com.rpm.clynx.adapter.DeviceListAdapter;
        import com.rpm.clynx.model.DeviceItemModel;
        import com.rpm.clynx.model.DeviceModel;
        import com.rpm.clynx.utility.DataBaseHelper;
        import com.rpm.clynx.utility.Links;
        import com.rpm.clynx.utility.Loader;
        import org.json.JSONArray;
        import org.json.JSONException;
        import org.json.JSONObject;
        import java.util.ArrayList;
        import java.util.List;
        import android.widget.Toast;
        import androidx.recyclerview.widget.LinearLayoutManager;
        import androidx.recyclerview.widget.RecyclerView;
        import com.android.volley.AuthFailureError;
        import com.android.volley.DefaultRetryPolicy;
        import com.android.volley.Request;
        import com.android.volley.RequestQueue;
        import com.android.volley.Response;
        import com.android.volley.VolleyError;
        import com.android.volley.toolbox.StringRequest;
        import com.android.volley.toolbox.Volley;
        import com.rpm.clynx.R;
        import java.util.HashMap;
        import java.util.Map;

public class DeviceDetailsFragment extends Fragment {
    RecyclerView deviceR;
    TextView emptyView;
    DataBaseHelper db;
    SharedPreferences pref;
    SharedPreferences.Editor editor;
    String Token;
    private DeviceListAdapter adapter;
    private List<DeviceModel> deviceModels;
    RecyclerView.LayoutManager layoutManager;
    View view,view1;
    @Override
    public View onCreateView(LayoutInflater inflater, ViewGroup container,
                             Bundle savedInstanceState) {
        // Inflate the layout for this fragment
        view = inflater.inflate(R.layout.fragment_device_details, container, false);
        emptyView = (TextView) view.findViewById(R.id.empty_viewDD);
        view1 = inflater.inflate(R.layout.device_item, container, false);
        db = new DataBaseHelper(getContext());
        pref = this.getActivity().getSharedPreferences("RPMUserApp",getContext().MODE_PRIVATE);
        editor = pref.edit();
        Token = pref.getString("Token", null);
        deviceR =  view.findViewById(R.id.fragmentDevice);
        layoutManager = new LinearLayoutManager(getContext());
        deviceModels = new ArrayList<>();
        adapter = new DeviceListAdapter(deviceModels,getContext());
        deviceR.setLayoutManager(layoutManager);
        deviceR.setAdapter(adapter);
        checkdevdet();
        return  view;
    }

    private void checkdevdet() {
        String url = Links.BASE_URL+  Links.GET_PATIENT;
        final Loader l1 = new Loader(getActivity());
        l1.show("Please wait...");
        StringRequest stringRequest = new StringRequest(Request.Method.GET, url, new Response.Listener<String>() {
            @Override
            public void onResponse(String response) {
                Log.d("response checkpgmdet",response.toString());
                JSONArray jsonArrayData=null;
                l1.dismiss();
                try {
                    JSONObject jsonObject = new JSONObject(response);
                    JSONObject jsonObjectPres = jsonObject.getJSONObject("PatientDevicesDetails");
                    jsonArrayData = new JSONArray(jsonObjectPres.getString("PatientDeviceInfos"));
                    Log.d("log_data_array",jsonArrayData.toString());
                    try {
                        ArrayList<DeviceItemModel> nim = new ArrayList<DeviceItemModel>();
                        for (int i = 0; i < jsonArrayData.length(); i++){
                            deviceR.setVisibility(View.VISIBLE);
                            emptyView.setVisibility(View.GONE);
                            int ival = i +1;
                            String combinedString = "Device " + ival;
                            nim.add(new DeviceItemModel(combinedString,jsonArrayData.getJSONObject(i).getString("VitalName"), jsonArrayData.getJSONObject(i).getString("DeviceNumber"),jsonArrayData.getJSONObject(i).getString("DeviceCommunicationType"),jsonArrayData.getJSONObject(i).getString("DeviceName"),jsonArrayData.getJSONObject(i).getString("DeviceStatus")));
                            Log.d("nimGoals", nim.toString());
                        }
                        deviceModels.add(new DeviceModel(nim));
                        adapter.notifyDataSetChanged();
                    }
                    catch (JSONException e) {
                        e.printStackTrace();
                    }finally {
                        adapter.notifyDataSetChanged();
                    }
                } catch (JSONException e) {
                    e.printStackTrace();
                }
                if (jsonArrayData.length()<=0){
                    deviceR.setVisibility(View.GONE);
                    emptyView.setVisibility(View.VISIBLE);
                }
            }
        } ,new Response.ErrorListener() {
            @Override
            public void onErrorResponse(VolleyError error) {
                Toast.makeText(getContext(), "No data available!", Toast.LENGTH_SHORT).show();
                l1.dismiss();
            }
        }){

            @Override
            public Map<String, String> getHeaders() throws AuthFailureError {
                Map<String, String> headers = new HashMap<>();

                headers.put("Bearer",Token);
                Log.d("headers_myprofileandprogram",headers.toString());
                Log.d("Token_myprofileandprogram", Token);
                return headers;
            }
        };
        RequestQueue requestQueue = Volley.newRequestQueue(getActivity());
        stringRequest.setRetryPolicy(new DefaultRetryPolicy(60000, DefaultRetryPolicy.DEFAULT_MAX_RETRIES, DefaultRetryPolicy.DEFAULT_BACKOFF_MULT));
        requestQueue.add(stringRequest);
    }
}