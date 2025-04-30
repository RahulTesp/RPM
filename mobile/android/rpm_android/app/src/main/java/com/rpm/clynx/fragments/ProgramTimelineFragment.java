package com.rpm.clynx.fragments;

        import android.content.SharedPreferences;
        import android.icu.text.SimpleDateFormat;
        import android.os.Bundle;
        import android.util.Log;
        import android.view.LayoutInflater;
        import android.view.View;
        import android.view.ViewGroup;
        import android.widget.LinearLayout;
        import android.widget.TextView;
        import androidx.fragment.app.Fragment;
        import com.rpm.clynx.adapter.DiagnosisListAdapter;
        import com.rpm.clynx.model.DiagnosisItemModel;
        import com.rpm.clynx.model.DiagnosisModel;
        import com.rpm.clynx.utility.DataBaseHelper;
        import com.rpm.clynx.utility.DateUtils;
        import com.rpm.clynx.utility.Links;
        import com.rpm.clynx.utility.Loader;
        import com.rpm.clynx.R;
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
        import java.util.Date;
        import java.util.HashMap;
        import java.util.Map;

public class ProgramTimelineFragment extends Fragment {
    RecyclerView diagnosisR;
    DataBaseHelper db;
    SharedPreferences pref;
    SharedPreferences.Editor editor;
    String Token,Status;
    private DiagnosisListAdapter adapter;
    private List<DiagnosisModel> diagnosisModels;
    RecyclerView.LayoutManager layoutManager;
    TextView presdate, consultdate, enrollpers , activedate,  carepersonal,caremanager,
            enrolldate, clinic,cliniccode, physician;
    private String PrescribedDate;
    private String Physician;
    private String Clinic,ClinicCode;
    private String ConsultationDate;
    LinearLayout activeSectionLayout;
    private String EnrollmentPersonal;
    private String ActiveAssignedDate, EnrollAssignedDate;
    private String CareTeamPersonalAssigneeName;
    private String ManagerName;
    View view;
    @Override
    public View onCreateView(LayoutInflater inflater, ViewGroup container,
                             Bundle savedInstanceState) {
        // Inflate the layout for this fragment
        view = inflater.inflate(R.layout.fragment_program_timeline, container, false);
        presdate = (TextView) view.findViewById(R.id.frag_timeline_date);
        physician = (TextView) view.findViewById(R.id.frag_timeline_physician);
        clinic = (TextView) view.findViewById(R.id.frag_timeline_clinic);
        cliniccode = (TextView) view.findViewById(R.id.frag_timeline_cliniccode);
        consultdate = (TextView) view.findViewById(R.id.frag_timeline_consulationdt);
        enrolldate = (TextView) view.findViewById(R.id.frag_timeline_enrolldt);
        enrollpers = (TextView) view.findViewById(R.id.enrollpersonal);
        activedate = (TextView) view.findViewById(R.id.frag_activedt);
        carepersonal = (TextView) view.findViewById(R.id.careteam_pers);
        caremanager = (TextView) view.findViewById(R.id.careteammngr);
        activeSectionLayout = view.findViewById(R.id.activeSectionLayout);
        db = new DataBaseHelper(getContext());
        pref = this.getActivity().getSharedPreferences("RPMUserApp",getContext().MODE_PRIVATE);
        editor = pref.edit();
        Token = pref.getString("Token", null);
        Status = pref.getString("Status", null);
        Log.d("Status", Status);
        diagnosisR =  view.findViewById(R.id.fragmentDiagnosis);
        layoutManager = new LinearLayoutManager(getContext());
        diagnosisModels = new ArrayList<>();
        adapter = new DiagnosisListAdapter(diagnosisModels,getContext());
        diagnosisR.setLayoutManager(layoutManager);
        diagnosisR.setAdapter(adapter);

        checkpgmtimeline();
        return  view;
    }

    private void checkpgmtimeline() {
        String url = Links.BASE_URL+  Links.GET_PATIENT;
        final Loader l1 = new Loader(getActivity());

        l1.show("Please wait...");
        StringRequest stringRequest = new StringRequest(Request.Method.GET, url, new Response.Listener<String>() {
            @Override
            public void onResponse(String response) {
                Log.d("response checkpgmdet",response.toString());
                JSONArray jsonArrayEnrollInfo=null;
                JSONArray jsonArrayDiag=null;
                l1.dismiss();
                try {
                    JSONObject jsonObject = new JSONObject(response);
                    JSONObject jsonObjectPresDet = jsonObject.getJSONObject("PatientPrescribtionDetails");
                    JSONObject jsonObjectEnrollDet = jsonObject.getJSONObject("PatientEnrolledDetails");
                    JSONObject jsonObjectActiveDet = jsonObject.getJSONObject("ActivePatientDetails");

                    if ("Active".equals(Status)) {
                        Log.d("StatusPT",Status);
                        activeSectionLayout.setVisibility(View.VISIBLE);
                    } else {
                        activeSectionLayout.setVisibility(View.GONE);
                    }

                    jsonArrayEnrollInfo = new JSONArray(jsonObjectEnrollDet.getString("patientEnrolledInfos"));
                    jsonArrayDiag = new JSONArray(jsonObjectPresDet.getString("PatientDiagnosisInfos"));

                    PrescribedDate = jsonObjectPresDet.getString("PrescribedDate");
                    Log.d("PrescribedDate",PrescribedDate);
                    String output1 = DateUtils.convertUtcToLocalFormatted(PrescribedDate,"MMM dd, yyyy");
                    presdate.setText(output1);
                    Physician = jsonObjectPresDet.getString("Physician");
                    physician.setText(Physician);
                    Clinic = jsonObjectPresDet.getString("Clinic");

                    if (Clinic == "null" || Clinic.isEmpty()) {
                        clinic.setText("");
                    } else {
                        clinic.setText(Clinic);
                    }
                       ClinicCode = jsonObjectPresDet.getString("ClinicCode");
                       cliniccode.setText(ClinicCode);

                    ConsultationDate = jsonObjectPresDet.getString("ConsultationDate");
                    EnrollAssignedDate = jsonObjectEnrollDet.getString("AssignedDate");
                    ActiveAssignedDate = jsonObjectActiveDet.getString("AssignedDate");
                    Log.d("ConsultationDate", String.valueOf(ConsultationDate));
                    Log.d("EnrollAssignedDate", String.valueOf(EnrollAssignedDate));
                    Log.d("ActiveAssignedDate", String.valueOf(ActiveAssignedDate));
                    Log.d("ConsultationDateisEmpty", String.valueOf(ConsultationDate.isEmpty()));

                    try {
                        if (!ConsultationDate.isEmpty()) {
                            // Create a SimpleDateFormat object for the input format
                            SimpleDateFormat inputDateFormat = new SimpleDateFormat("MM/dd/yyyy hh:mm:ss a");
                            // Create a SimpleDateFormat object for the desired output format
                            SimpleDateFormat outputDateFormat = new SimpleDateFormat("MMM d, yyyy");
                            try {
                                // Parse the input date string
                                Date date = inputDateFormat.parse(ConsultationDate);
                                // Format the date in the desired output format
                                String outputDateStr = outputDateFormat.format(date);
                                System.out.println("Input Date: " + ConsultationDate);
                                System.out.println("Formatted Date: " + outputDateStr);
                                consultdate.setText(outputDateStr);
                            } catch (ParseException e) {
                                e.printStackTrace();
                            }
                       }
                        else
                        {
                            consultdate.setText("");
                        }
                        if (!EnrollAssignedDate.isEmpty()) {
                            String enrollassgndtolocal = DateUtils.convertUtcToLocalFormatted(EnrollAssignedDate, "MMM d, yyyy");
                            Log.d("enrollassgndtolocal", String.valueOf(enrollassgndtolocal));
                            enrolldate.setText(enrollassgndtolocal);
                        }
                        else
                        {
                            enrolldate.setText("");
                        }
                        if (!ActiveAssignedDate.isEmpty()) {
                            String actvassgndtolocal = DateUtils.convertUtcToLocalFormatted(ActiveAssignedDate, "MMM d, yyyy");
                            Log.d("actvassgndtolocal", String.valueOf(actvassgndtolocal));
                            activedate.setText(actvassgndtolocal);
                        }
                        else
                        {
                            activedate.setText("");
                        }

                        CareTeamPersonalAssigneeName = jsonObjectActiveDet.getString("AssigneeName");
                        Log.d("CareTeamPersonalAssigneeName", CareTeamPersonalAssigneeName);

                        if (CareTeamPersonalAssigneeName == "null" || CareTeamPersonalAssigneeName.isEmpty()) {
                            carepersonal.setText("Not Assigned");
                            Log.d("Debug", "AssigneeNameisnullorempty");
                        } else {
                            Log.d("Debug", "AssigneeNameisnotnullornotempty");
                            carepersonal.setText(CareTeamPersonalAssigneeName);
                        }

                        ManagerName = jsonObjectActiveDet.getString("ManagerName");
                        Log.d("ManagerName", ManagerName);

                        if (ManagerName == "null" || ManagerName.isEmpty()) {
                            caremanager.setText("Not Assigned");
                            Log.d("Debug", "ManagerNameisnullorempty");
                        } else {
                            Log.d("Debug", "ManagerNameisnotnullornotempty");
                            caremanager.setText(ManagerName);
                        }
                        if( jsonArrayEnrollInfo.length() != 0) {
                            for (int i = 0; i < jsonArrayEnrollInfo.length(); i++) {
                                EnrollmentPersonal = jsonArrayEnrollInfo.getJSONObject(i).getString("EnrollmentPersonal");
                                System.out.println("EnrollmentPersonal");
                                System.out.println(EnrollmentPersonal);
                                enrollpers.setText(EnrollmentPersonal);
                            }
                        }
                        else
                        {
                            enrollpers.setText("Not Assigned");
                        }
                    } catch (Exception e) {
                        e.printStackTrace();
                    }
                    Log.d("log_data_array",jsonArrayDiag.toString());
                    try {
                        ArrayList<DiagnosisItemModel> nim = new ArrayList<DiagnosisItemModel>();
                        for (int i = 0; i < jsonArrayDiag.length(); i++){
                            nim.add(new DiagnosisItemModel(jsonArrayDiag.getJSONObject(i).getString("DiagnosisName"), jsonArrayDiag.getJSONObject(i).getString("DiagnosisCode")));
                            Log.d("nimGoals", nim.toString());
                        }
                        diagnosisModels.add(new DiagnosisModel(nim));
                        adapter.notifyDataSetChanged();
                    } catch (JSONException e) {
                        e.printStackTrace();
                    }
                } catch (JSONException e) {
                    e.printStackTrace();
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