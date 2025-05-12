package com.rpm.clynx.fragments;

import android.annotation.SuppressLint;
import android.content.Intent;
import android.content.SharedPreferences;
import android.graphics.Color;
import android.os.Build;
import android.os.Bundle;
import android.util.Log;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.TextView;
import androidx.annotation.RequiresApi;
import androidx.fragment.app.Fragment;
import com.rpm.clynx.adapter.GoalsListAdapter;
import com.rpm.clynx.adapter.VitalsMonitoredListAdapter;
import com.rpm.clynx.model.GoalsItemModel;
import com.rpm.clynx.model.GoalsModel;
import com.rpm.clynx.model.VitalsMonitoredItemModel;
import com.rpm.clynx.model.VitalsMonitoredModel;
import com.rpm.clynx.utility.DataBaseHelper;
import com.rpm.clynx.utility.DateUtils;
import com.rpm.clynx.utility.Links;
import com.rpm.clynx.utility.Loader;
import org.json.JSONArray;
import org.json.JSONException;
import org.json.JSONObject;
import java.text.ParseException;
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
import java.util.StringTokenizer;

public class ProgramDetailsFragment extends Fragment {
    RecyclerView goalsR;
    RecyclerView vitalsMR;
    DataBaseHelper db;
    SharedPreferences pref;
    SharedPreferences.Editor editor;
    String Token,ProgramName,ProgramTypeName;
    private String StartDate;
    private String EndDate;
    private String Personal;
    private String Duration;
    private String Physician;
    private GoalsListAdapter adapterG;
    private VitalsMonitoredListAdapter adapterVM;
    private List<GoalsModel> goalsModels;
    private List<VitalsMonitoredModel> vitalsMonitoredModels;
    RecyclerView.LayoutManager layoutManager;
    TextView vitalsmonitored,duration, startdate, enddate,personal,physician, tv_prog_name, tv_status, tv_firstmonth, tv_secondmonth;
    View view;
    @SuppressLint("ResourceType")
    @RequiresApi(api = Build.VERSION_CODES.O)
    @Override
    public View onCreateView(LayoutInflater inflater, ViewGroup container,
                             Bundle savedInstanceState) {
        // Inflate the layout for this fragment
        view = inflater.inflate(R.layout.fragment_program_details, container, false);
        db = new DataBaseHelper(getContext());
        pref = this.getActivity().getSharedPreferences("RPMUserApp",getContext().MODE_PRIVATE);
        editor = pref.edit();
        tv_prog_name = (TextView) view.findViewById(R.id.frag_dash_prog_name);
        tv_status = (TextView) view.findViewById(R.id.frag_dash_status);
        duration = (TextView) view.findViewById(R.id.frag_pgmdetails_duration);
        startdate = (TextView) view.findViewById(R.id.frag_pgmdetails_startdate);
        enddate = (TextView) view.findViewById(R.id.frag_pgmdetails_enddate);
        personal = (TextView) view.findViewById(R.id.frag_pgmdetails_careteampersonal);
        physician = (TextView) view.findViewById(R.id.frag_pgmdetails_physician);
        layoutManager = new LinearLayoutManager(getContext());
        goalsR =  view.findViewById(R.id.fragmentGoals);
        goalsModels = new ArrayList<>();
        adapterG = new GoalsListAdapter(goalsModels,getContext());
        goalsR.setLayoutManager(layoutManager);
        goalsR.setAdapter(adapterG);
        vitalsMR =  view.findViewById(R.id.fragmentVitalsMonitored);
        vitalsMonitoredModels = new ArrayList<>();
        adapterVM = new VitalsMonitoredListAdapter(vitalsMonitoredModels,getContext());
        vitalsMR.setAdapter(adapterVM);
        vitalsmonitored = (TextView) view.findViewById(R.id.vtlsmoni);
        ProgramName = pref.getString("ProgramName", null);
        Log.d("ProgramNameHm", ProgramName);
        if ( ProgramName.equals("RPM")) {
            vitalsmonitored.setText("Vitals Monitored");
        }
        else
        {
            vitalsmonitored.setText("Conditions Monitored");
        }
        Token = pref.getString("Token", null);

        // Define a mapping of status text to background colors
        HashMap<String, Integer> statusColorMap = new HashMap<>();
        HashMap<String, Integer> statusTextColorMap = new HashMap<>();

        statusColorMap.put("Active", R.drawable.active_background_radious);
        statusColorMap.put("Enrolled", R.drawable.enrollpres_background_radious);
        statusColorMap.put("Prescribed", R.drawable.enrollpres_background_radious);
        statusColorMap.put("InActive", R.drawable.inactive_background_radious);
        statusColorMap.put("OnHold", R.drawable.inactive_background_radious);
        statusColorMap.put("Discharged", R.drawable.discharg_background_radious);
        statusColorMap.put("ReadyToDischarge", R.drawable.discharg_background_radious);

        statusTextColorMap.put("Active", R.color.white);
        statusTextColorMap.put("Enrolled", R.color.white);
        statusTextColorMap.put("Prescribed", R.color.white);
        statusTextColorMap.put("InActive", R.color.grey_60);
        statusTextColorMap.put("OnHold", R.color.black);
        statusTextColorMap.put("Discharged", R.color.white);
        statusTextColorMap.put("ReadyToDischarge", R.color.white);

        String CompletedDuration = pref.getString("CompletedDuration", null);
        StringTokenizer tokens = new StringTokenizer(CompletedDuration, "/");
        String first = tokens.nextToken();// this will contain "Fruit"
        String second = tokens.nextToken();// this will contain " they taste good"

        String Status = pref.getString("Status", null);
        ProgramTypeName = pref.getString("ProgramTypeName", null);

        tv_prog_name.setText(ProgramTypeName);
        tv_status.setText(Status);

        // Check if the status text is in your mapping
        if (Status != null && statusColorMap.containsKey(Status) && statusTextColorMap.containsKey(Status)) {
            int backgroundColor = statusColorMap.get(Status);
            int textColorResource = statusTextColorMap.get(Status);
            // Set the background color of the TextView
            tv_status.setBackgroundResource(backgroundColor);
            // Set the text color of the TextView
            tv_status.setTextColor(getResources().getColor(textColorResource));
        } else {
            tv_status.setBackgroundResource(Color.GREEN);
            tv_status.setTextColor(getResources().getColor(Color.WHITE));
        }

        checkpgmdet();
        return  view;
    }

    private void checkpgmdet() {
        String url = Links.BASE_URL+  Links.GET_PATIENT;
        final Loader l1 = new Loader(getActivity());
        l1.show("Please wait...");
        StringRequest stringRequest = new StringRequest(Request.Method.GET, url, new Response.Listener<String>() {
            @Override
            public void onResponse(String response) {
                Log.d("response checkpgmdet",response.toString());
                JSONArray jsonArrayData = null;
                JSONArray jsonArrayVM  = null;
                l1.dismiss();
                try {
                    JSONObject jsonObject = new JSONObject(response);
                    JSONObject jsonObjectPgmDet = jsonObject.getJSONObject("PatientProgramdetails");
                    JSONObject jsonObjectLVM = jsonObject.getJSONObject("PatientProgramGoals");

                    JSONObject jsonObjectPres = jsonObject.getJSONObject("PatientPrescribtionDetails");
                    jsonArrayData = new JSONArray(jsonObjectLVM.getString("goalDetails"));
                    jsonArrayVM = new JSONArray(jsonObjectPgmDet.getString("PatientVitalInfos"));
                    Duration = jsonObjectPgmDet.getString("Duration");
                    duration.setText(Duration);
                    StartDate = DateUtils.convertUtcToLocalFormatted(jsonObjectPgmDet.getString("StartDate"),"MMM dd, yyyy");
                    Log.d("StartDate",StartDate.toString());
                    startdate.setText(StartDate);
                    EndDate = DateUtils.formatDate(jsonObjectPgmDet.getString("EndDate"),"MMM dd, yyyy");
                    Log.d("EndDate",EndDate.toString());
                    enddate.setText(EndDate);
                    Personal = jsonObjectPgmDet.getString("AssignedMember");
                    personal.setText(Personal);
                    Physician = jsonObjectPres.getString("Physician");
                    physician.setText(Physician);
                    Log.d("log_data_array",jsonArrayData.toString());
                            try {
                                ArrayList<GoalsItemModel> nim = new ArrayList<GoalsItemModel>();
                                for (int i = 0; i < jsonArrayData.length(); i++){
                                    nim.add(new GoalsItemModel(jsonArrayData.getJSONObject(i).getString("Goal"), jsonArrayData.getJSONObject(i).getString("Description")));
                                    Log.d("nimGoals", nim.toString());
                                }
                                goalsModels.add(new GoalsModel(nim));
                                adapterG.notifyDataSetChanged();
                            }
                        catch (JSONException e) {
                            e.printStackTrace();
                        }finally {
                            adapterG.notifyDataSetChanged();
                        }
                    try {
                        ArrayList<VitalsMonitoredItemModel> nim = new ArrayList<VitalsMonitoredItemModel>();
                        for (int i = 0; i < jsonArrayVM.length(); i++){
                            JSONObject vitalObject = jsonArrayVM.getJSONObject(i);
                            boolean isSelected = vitalObject.getBoolean("Selected");

                            if (isSelected) {
                                nim.add(new VitalsMonitoredItemModel(vitalObject.getString("Vital")));
                            }
                            Log.d("nimvm", nim.toString());
                        }
                        vitalsMonitoredModels.add(new VitalsMonitoredModel(nim));
                        adapterVM.notifyDataSetChanged();
                    }
                    catch (JSONException e) {
                        e.printStackTrace();
                    }finally {
                        adapterVM.notifyDataSetChanged();
                    }
                } catch (JSONException | ParseException e) {
                    e.printStackTrace();
                }
                if (jsonArrayData.length()<=0){
                    Toast.makeText(getContext(), "No Data Available", Toast.LENGTH_SHORT).show();
                }
                if (jsonArrayVM.length()<=0){
                    Toast.makeText(getContext(), "No Data Available", Toast.LENGTH_SHORT).show();
                }
            }
        } ,new Response.ErrorListener() {
            @Override
            public void onErrorResponse(VolleyError error) {
                Toast.makeText(getContext(), "No data available!", Toast.LENGTH_SHORT).show();
                l1.dismiss();
                if ( error.networkResponse.statusCode == 401) {
                    Log.d("homecode", String.valueOf(error.networkResponse.statusCode));
                    error.printStackTrace();
                    Log.d("e", error.toString());
                    editor.putBoolean("loginstatus", false);
                    editor.apply();
                    db.deleteProfileData("myprofileandprogram");
                    db.deleteData();
                    editor.clear();
                    editor.commit();
                    try {
                        Log.d("loginlatestActfrmdashboard", String.valueOf(requireContext()));
                        Intent intentlogout = new Intent(requireContext(), Login.class);
                        intentlogout.addFlags(Intent.FLAG_ACTIVITY_NEW_TASK | Intent.FLAG_ACTIVITY_CLEAR_TASK);
                        startActivity(intentlogout);
                    }catch (Exception e)
                    {
                        Log.e("onLogOff Clear", e.toString());
                    }
                }
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