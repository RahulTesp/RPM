package com.rpm.clynx.fragments;

        import android.content.SharedPreferences;
        import android.os.Bundle;
        import android.util.Log;
        import android.view.LayoutInflater;
        import android.view.View;
        import android.view.ViewGroup;
        import android.widget.TextView;
        import androidx.fragment.app.Fragment;
        import com.rpm.clynx.adapter.VitalSchedulesListAdapter;
        import com.rpm.clynx.model.VitalSchedulesItemModel;
        import com.rpm.clynx.model.VitalSchedulesModel;
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
        import com.rpm.clynx.R;
        import com.android.volley.AuthFailureError;
        import com.android.volley.DefaultRetryPolicy;
        import com.android.volley.Request;
        import com.android.volley.RequestQueue;
        import com.android.volley.Response;
        import com.android.volley.VolleyError;
        import com.android.volley.toolbox.StringRequest;
        import com.android.volley.toolbox.Volley;
        import java.util.HashMap;
        import java.util.Map;

public class VitalSchedulesFragment extends Fragment {
    RecyclerView vitalsR;
    TextView emptyView;
    DataBaseHelper db;
    SharedPreferences pref;
    SharedPreferences.Editor editor;
    String Token;
    private VitalSchedulesListAdapter adapter;
    private List<VitalSchedulesModel> vitalSchedulesModels;
    RecyclerView.LayoutManager layoutManager;
    TextView presdate, consultdate, clinic, branch, physician;
    View view;
    @Override
    public View onCreateView(LayoutInflater inflater, ViewGroup container,
                             Bundle savedInstanceState) {
        // Inflate the layout for this fragment
        view = inflater.inflate(R.layout.fragment_vital_schedules, container, false);
        emptyView = (TextView) view.findViewById(R.id.empty_viewVS);
        db = new DataBaseHelper(getContext());
        pref = this.getActivity().getSharedPreferences("RPMUserApp",getContext().MODE_PRIVATE);
        editor = pref.edit();
        Token = pref.getString("Token", null);
        vitalsR =  view.findViewById(R.id.fragmentVitalSchedules);
        layoutManager = new LinearLayoutManager(getContext());
        vitalSchedulesModels = new ArrayList<>();
        adapter = new VitalSchedulesListAdapter(vitalSchedulesModels,getContext());
        vitalsR.setLayoutManager(layoutManager);
        vitalsR.setAdapter(adapter);

        checkVitalSchedules();
        return  view;
    }

    private void checkVitalSchedules() {
        String url = Links.BASE_URL+  Links.GET_PATIENT;
        final Loader l1 = new Loader(getActivity());

        l1.show("Please wait...");
        StringRequest stringRequest = new StringRequest(Request.Method.GET, url, new Response.Listener<String>() {
            @Override
            public void onResponse(String response) {
                Log.d("response checkVitalSchedules",response.toString());
                JSONArray jsonArrayData=null;
                l1.dismiss();

                try {
                    JSONObject jsonObject = new JSONObject(response);
                    Log.d("jsonObject",jsonObject.toString());
                    JSONObject jsonObjPatientVitDet = jsonObject.getJSONObject("PatientVitalDetails");
                    Log.d("jsonObjPatientVitDet",jsonObjPatientVitDet.toString());
                    jsonArrayData = new JSONArray(jsonObjPatientVitDet.getString("PatientVitalInfos"));
                    Log.d("jsonArrayData",jsonArrayData.toString());


                    for (int i = 0; i < jsonArrayData.length(); i++) {
                        try {
                            int ival = i + 1;
                            String combinedSchedule = "Schedule " + ival;  // this goes into SchedVal

                            JSONObject jsonNotificationList = jsonArrayData.getJSONObject(i);
                            JSONArray jsonArrayNL = new JSONArray(jsonNotificationList.getString("VitalMeasureInfos"));

                            ArrayList<VitalSchedulesItemModel> nim = new ArrayList<>();

                            for (int j = 0; j < jsonArrayNL.length(); j++) {
                                vitalsR.setVisibility(View.VISIBLE);
                                emptyView.setVisibility(View.GONE);

                                nim.add(new VitalSchedulesItemModel(
                                        jsonArrayNL.getJSONObject(j).getString("NormalMinimum"),
                                        jsonArrayNL.getJSONObject(j).getString("NormalMaximum"),
                                        jsonArrayNL.getJSONObject(j).getString("MeasureName"),
                                        jsonArrayNL.getJSONObject(j).getString("UnitName")
                                ));
                            }

                            //  Pass combinedSchedule as first argument
                            vitalSchedulesModels.add(new VitalSchedulesModel(
                                    combinedSchedule,   // "Schedule 1", "Schedule 2", etc.
                                    jsonArrayData.getJSONObject(i).getString("VitalName"),
                                    jsonArrayData.getJSONObject(i).getString("ScheduleName"),
                                    jsonArrayData.getJSONObject(i).getString("VitalScheduleName"),
                                    jsonArrayData.getJSONObject(i).getString("Morning"),
                                    jsonArrayData.getJSONObject(i).getString("Afternoon"),
                                    jsonArrayData.getJSONObject(i).getString("Evening"),
                                    jsonArrayData.getJSONObject(i).getString("Night"),
                                    nim
                            ));

                            adapter.notifyDataSetChanged();
                        }
                        catch (JSONException e) {
                            e.printStackTrace();
                        }
                        finally {
                            adapter.notifyDataSetChanged();
                        }
                    }

                }
                catch (JSONException e) {
                    e.printStackTrace();
                }
                if (jsonArrayData.length()<=0){
                    vitalsR.setVisibility(View.GONE);
                    emptyView.setVisibility(View.VISIBLE);
                }
            }
        } ,new Response.ErrorListener() {
            @Override
            public void onErrorResponse(VolleyError error) {
                Toast.makeText(getContext(), "No data!", Toast.LENGTH_SHORT).show();
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