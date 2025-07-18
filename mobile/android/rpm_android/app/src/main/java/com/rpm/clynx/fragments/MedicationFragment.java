package com.rpm.clynx.fragments;

import android.content.SharedPreferences;
import android.os.Bundle;
import android.util.Log;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.TextView;
import androidx.fragment.app.Fragment;
import com.rpm.clynx.adapter.MedicationListAdapter;
import com.rpm.clynx.model.MedicationItemModel;
import com.rpm.clynx.model.MedicationModel;
import com.rpm.clynx.utility.DataBaseHelper;
import com.rpm.clynx.utility.DateUtils;
import com.rpm.clynx.utility.Links;
import com.rpm.clynx.utility.Loader;
import com.rpm.clynx.R;
import org.json.JSONArray;
import org.json.JSONException;
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
import java.util.HashMap;
import java.util.Map;

public class MedicationFragment extends Fragment {
    RecyclerView medR;
    TextView emptyView;
    DataBaseHelper db;
    SharedPreferences pref;
    SharedPreferences.Editor editor;
    String Token;
    private MedicationListAdapter adapter;
    private List<MedicationModel> medicationModels;
    RecyclerView.LayoutManager layoutManager;
    View view;
    @Override
    public View onCreateView(LayoutInflater inflater, ViewGroup container,
                             Bundle savedInstanceState) {
        // Inflate the layout for this fragment
        view = inflater.inflate(R.layout.fragment_medications, container, false);
        db = new DataBaseHelper(getContext());
        pref = this.getActivity().getSharedPreferences("RPMUserApp",getContext().MODE_PRIVATE);
        editor = pref.edit();
        Token = pref.getString("Token", null);
        medR =  view.findViewById(R.id.fragmentMedications);
        emptyView = (TextView) view.findViewById(R.id.empty_viewMed);
        layoutManager = new LinearLayoutManager(getContext());
        medicationModels = new ArrayList<>();
        adapter = new MedicationListAdapter(medicationModels,getContext());
        medR.setLayoutManager(layoutManager);
        medR.setAdapter(adapter);

        checkmeddet();

        Log.d("medfrg","medfrg");
        return  view;
    }

    @Override
    public void onResume() {
        super.onResume();
        refreshLayout();
    }

    public void checkmeddet() {
        String url = Links.BASE_URL+  Links.GET_MEDICATIONS;
        final Loader l1 = new Loader(getActivity());
        l1.show("Please wait...");
        StringRequest stringRequest = new StringRequest(Request.Method.GET, url, new Response.Listener<String>() {
            @Override
            public void onResponse(String response) {
                Log.d("responseGET_MEDICATIONS",response.toString());
                JSONArray jsonArrayData=null;
                l1.dismiss();
                    try {
                        jsonArrayData = new JSONArray(response);
                        Log.d("log_data_medi",jsonArrayData.toString());
                        ArrayList<MedicationItemModel> nim = new ArrayList<MedicationItemModel>();
                        for (int i = 0; i < jsonArrayData.length(); i++){
                            medR.setVisibility(View.VISIBLE);
                            emptyView.setVisibility(View.GONE);

                            String endDateStr = jsonArrayData.getJSONObject(i).getString("EndDate");
                            boolean isExpired = DateUtils.isPastDate(endDateStr); // Implement this if not already


                            nim.add(new MedicationItemModel(jsonArrayData.getJSONObject(i).getString("Medicinename"),
                                    jsonArrayData.getJSONObject(i).getString("MedicineSchedule") + " / " + jsonArrayData.getJSONObject(i).getString("BeforeOrAfterMeal") ,
                                    jsonArrayData.getJSONObject(i).getString("Morning"),
                                    jsonArrayData.getJSONObject(i).getString("AfterNoon"),
                                    jsonArrayData.getJSONObject(i).getString("Evening"),
                                    jsonArrayData.getJSONObject(i).getString("Night"),
                                    DateUtils.formatDate(jsonArrayData.getJSONObject(i).getString("StartDate"),"dd, MMM yyyy"),
                                    DateUtils.formatDate(jsonArrayData.getJSONObject(i).getString("EndDate"),"dd, MMM yyyy"),
                                    isExpired
                            ));
                            Log.d("nimGoals", nim.toString());
                        }
                        medicationModels.add(new MedicationModel(nim));
                        adapter.notifyDataSetChanged();
                    }
                    catch (JSONException | ParseException e) {
                        e.printStackTrace();
                    }  finally {
                        adapter.notifyDataSetChanged();
                    }
                if (jsonArrayData.length()<=0){
                    medR.setVisibility(View.GONE);
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
                Log.d("headers_MEDI",headers.toString());
                Log.d("Token_MEDI", Token);
                return headers;
            }
        };
        RequestQueue requestQueue = Volley.newRequestQueue(getActivity());
        stringRequest.setRetryPolicy(new DefaultRetryPolicy(60000, DefaultRetryPolicy.DEFAULT_MAX_RETRIES, DefaultRetryPolicy.DEFAULT_BACKOFF_MULT));
        requestQueue.add(stringRequest);
    }

    public void refreshLayout() {
        Log.d("refreshLayout", "refreshLayout");
        // Clear existing data
        medicationModels.clear();
        adapter.notifyDataSetChanged();
        // Fetch and add new data
        checkmeddet();
    }
}