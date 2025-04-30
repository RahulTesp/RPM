package com.rpm.clynx.fragments;

        import android.content.SharedPreferences;
        import android.os.Bundle;
        import android.util.Log;
        import android.view.LayoutInflater;
        import android.view.View;
        import android.view.ViewGroup;
        import android.widget.TextView;
        import androidx.fragment.app.Fragment;
        import com.rpm.clynx.adapter.InsuranceListAdapter;
        import com.rpm.clynx.model.InsuranceItemModel;
        import com.rpm.clynx.model.InsuranceModel;
        import com.rpm.clynx.utility.DataBaseHelper;
        import com.rpm.clynx.utility.Links;
        import com.rpm.clynx.utility.Loader;
        import com.rpm.clynx.R;
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
        import java.util.HashMap;
        import java.util.Map;

public class InsuranceDetailsFragment extends Fragment {
    RecyclerView insuranceR;
    DataBaseHelper db;
    TextView emptyView;
    SharedPreferences pref;
    SharedPreferences.Editor editor;
    String Token;
    private InsuranceListAdapter adapter;
    private List<InsuranceModel> insuranceModels;
    RecyclerView.LayoutManager layoutManager;
    View view;

    @Override
    public View onCreateView(LayoutInflater inflater, ViewGroup container,
                             Bundle savedInstanceState) {
        // Inflate the layout for this fragment
        view = inflater.inflate(R.layout.fragment_insurance_details, container, false);
        emptyView = (TextView) view.findViewById(R.id.empty_viewInsur);
        db = new DataBaseHelper(getContext());
        pref = this.getActivity().getSharedPreferences("RPMUserApp",getContext().MODE_PRIVATE);
        editor = pref.edit();
        Token = pref.getString("Token", null);
        insuranceR =  view.findViewById(R.id.fragmentInsurance);
        layoutManager = new LinearLayoutManager(getContext());
        insuranceModels = new ArrayList<>();
        adapter = new InsuranceListAdapter(insuranceModels,getContext());
        insuranceR.setLayoutManager(layoutManager);
        insuranceR.setAdapter(adapter);

        checkinsurancedet();
        return  view;
    }

    private void checkinsurancedet() {
        String url = Links.BASE_URL+  Links.GET_PATIENT;
        final Loader l1 = new Loader(getActivity());
        l1.show("Please wait...");
        StringRequest stringRequest = new StringRequest(Request.Method.GET, url, new Response.Listener<String>() {
            @Override
            public void onResponse(String response) {
                Log.d("responseinsurance",response.toString());
                JSONArray jsonArrayData=null;
                l1.dismiss();
                try {
                    JSONObject jsonObject = new JSONObject(response);
                    JSONObject jsonObjectPres = jsonObject.getJSONObject("PatientInsurenceDetails");
                    jsonArrayData = new JSONArray(jsonObjectPres.getString("PatientInsurenceInfos"));
                    Log.d("log_data_array",jsonArrayData.toString());
                    try {
                        ArrayList<InsuranceItemModel> nim = new ArrayList<InsuranceItemModel>();
                        for (int i = 0; i < jsonArrayData.length(); i++){
                            insuranceR.setVisibility(View.VISIBLE);
                            emptyView.setVisibility(View.GONE);
                            nim.add(new InsuranceItemModel(jsonArrayData.getJSONObject(i).getString("InsuranceVendorName"),
                                    jsonArrayData.getJSONObject(i).getString("IsPrimary")));
                            Log.d("nimGoals", nim.toString());
                        }
                        insuranceModels.add(new InsuranceModel(nim));
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
                    insuranceR.setVisibility(View.GONE);
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