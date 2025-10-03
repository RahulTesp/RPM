package com.rpm.clynx.fragments;

        import android.content.SharedPreferences;
        import android.icu.text.SimpleDateFormat;
        import android.os.Bundle;
        import android.util.Log;
        import android.view.LayoutInflater;
        import android.view.View;
        import android.view.ViewGroup;
        import android.widget.TextView;
        import androidx.fragment.app.Fragment;
        import com.rpm.clynx.adapter.SymptomListAdapter;
        import com.rpm.clynx.model.SymptomItemModel;
        import com.rpm.clynx.model.SymptomModel;
        import com.rpm.clynx.utility.DataBaseHelper;
        import com.rpm.clynx.utility.DateUtils;
        import com.rpm.clynx.utility.Links;
        import com.rpm.clynx.utility.Loader;
        import org.json.JSONArray;
        import org.json.JSONException;
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

public class SymptomsFragment extends Fragment {
    RecyclerView sympR;
    TextView emptyView;
    DataBaseHelper db;
    SharedPreferences pref;
    SharedPreferences.Editor editor;
    String Token;
    private SymptomListAdapter adapter;
    private List<SymptomModel> symptomModels;
    RecyclerView.LayoutManager layoutManager;
    View view;
    @Override
    public View onCreateView(LayoutInflater inflater, ViewGroup container,
                             Bundle savedInstanceState) {
        // Inflate the layout for this fragment
        view = inflater.inflate(R.layout.fragment_symptoms, container, false);

            db = new DataBaseHelper(getContext());
            pref = this.getActivity().getSharedPreferences("RPMUserApp",getContext().MODE_PRIVATE);
            editor = pref.edit();
            Token = pref.getString("Token", null);
            sympR =  view.findViewById(R.id.fragmentSymptoms);
            emptyView = (TextView) view.findViewById(R.id.empty_viewSympR);
            layoutManager = new LinearLayoutManager(getContext());
            symptomModels = new ArrayList<>();
            adapter = new SymptomListAdapter(symptomModels,getContext());
            sympR.setLayoutManager(layoutManager);
            sympR.setAdapter(adapter);

        checksympdet();
        return  view;
    }

    @Override
    public void onResume() {
        super.onResume();
        refreshSympLayout();
    }

    private void checksympdet() {
        String url = Links.BASE_URL+  Links.GET_SYMPTOMS;
        final Loader l1 = new Loader(getActivity());
        l1.show("Please wait...");
        StringRequest stringRequest = new StringRequest(Request.Method.GET, url, new Response.Listener<String>() {
            @Override
            public void onResponse(String response) {
                Log.d("responseGETsymp",response);
                JSONArray jsonArrayData = null;
                l1.dismiss();
                try {
                    jsonArrayData = new JSONArray(response);
                    SimpleDateFormat sdf = new SimpleDateFormat("yyyy-MM-dd'T'HH:mm:ss");

                    Log.d("log_data_array",jsonArrayData.toString());

                    // 🔹 Clear old data before adding new
                    symptomModels.clear();

                    try {
                        ArrayList<SymptomItemModel> nim = new ArrayList<>();
                        for (int i = 0; i < jsonArrayData.length(); i++) {
                            sympR.setVisibility(View.VISIBLE);
                            emptyView.setVisibility(View.GONE);

                            String localDateFormatted = DateUtils.convertUtcToLocalFormatted(
                                    jsonArrayData.getJSONObject(i).getString("SymptomStartDateTime"),
                                    "MMM d, yyyy, h:mm a"
                            );

                            nim.add(new SymptomItemModel(
                                    jsonArrayData.getJSONObject(i).getString("Symptom"),
                                    localDateFormatted,
                                    jsonArrayData.getJSONObject(i).getString("Description")
                            ));
                        }
                        symptomModels.add(new SymptomModel(nim));
                    }
                    catch (JSONException e) {
                        e.printStackTrace();
                    }
                    finally {
                        adapter.notifyDataSetChanged();
                    }
                } catch (JSONException e) {
                    e.printStackTrace();
                }

                if (jsonArrayData != null && jsonArrayData.length() <= 0) {
                    sympR.setVisibility(View.GONE);
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
                return headers;
            }
        };

        RequestQueue requestQueue = Volley.newRequestQueue(getActivity());
        stringRequest.setRetryPolicy(new DefaultRetryPolicy(60000,
                DefaultRetryPolicy.DEFAULT_MAX_RETRIES,
                DefaultRetryPolicy.DEFAULT_BACKOFF_MULT));
        requestQueue.add(stringRequest);
    }

    public void refreshSympLayout() {
        Log.d("refreshLayout", "refreshLayout");
        // Clear existing data
        symptomModels.clear();
        adapter.notifyDataSetChanged();
        // Fetch and add new data
        checksympdet();
    }
}