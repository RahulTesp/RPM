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
            enrolldate, clinic,cliniccode, physician, readyAssigneeNameTxt,
            readyAssignedDateTxt, holdAssigneeNameTxt, holdAssignedDateTxt,
            inactiveAssigneeNameTxt, inactiveAssignedDateTxt, dischargeAssigneeNameTxt,
            dischargeAssignedDateTxt, activeAssigneeNameTxt, activeAssignedDateTxt;
    private String PrescribedDate;
    private String Physician;
    private String Clinic,ClinicCode;
    private String ConsultationDate;
    LinearLayout activeSectionLayout, readySectionLayout, holdSectionLayout,
            inactiveSectionLayout, dischargedSectionLayout, enrolledSectionLayout;
    private String EnrollmentPersonal;
    private String  EnrollAssignedDate;

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
        activeSectionLayout = view.findViewById(R.id.activeSectionLayout);

        // Section layouts
        enrolledSectionLayout = view.findViewById(R.id.enrollSectionLayout);
         readySectionLayout = view.findViewById(R.id.readyForDischargeSectionLayout);
         holdSectionLayout = view.findViewById(R.id.onHoldSectionLayout);
         inactiveSectionLayout = view.findViewById(R.id.inactiveSectionLayout);
         dischargedSectionLayout = view.findViewById(R.id.dischargedSectionLayout);

// TextViews for ReadyForDischarge
         readyAssigneeNameTxt = view.findViewById(R.id.frag_ready_assignee);
         readyAssignedDateTxt = view.findViewById(R.id.frag_ready_dt);

        activeAssigneeNameTxt = view.findViewById(R.id.frag_active_assignee);
        activeAssignedDateTxt = view.findViewById(R.id.frag_active_dt);


// TextViews for OnHold
         holdAssigneeNameTxt = view.findViewById(R.id.frag_hold_assignee);
         holdAssignedDateTxt = view.findViewById(R.id.frag_hold_dt);

// TextViews for InActive
         inactiveAssigneeNameTxt = view.findViewById(R.id.frag_inactive_assignee);
         inactiveAssignedDateTxt = view.findViewById(R.id.frag_inactive_dt);

// TextViews for Discharged
         dischargeAssigneeNameTxt = view.findViewById(R.id.frag_discharged_assignee);
         dischargeAssignedDateTxt = view.findViewById(R.id.frag_discharged_dt);

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

                    // Ready For Discharge
                    JSONObject jsonObjectReadyForDischargePatientDet = jsonObject.getJSONObject("ReadyForDischargePatientDetails");
                    String readyStatus = jsonObjectReadyForDischargePatientDet.optString("Status", null);
                    if (readyStatus != null && !"null".equalsIgnoreCase(readyStatus)) {
                        readyAssigneeNameTxt.setText(jsonObjectReadyForDischargePatientDet.optString("AssigneeName", ""));
                        String readyAssigneeNametolocal = DateUtils.convertUtcToLocalFormatted(jsonObjectReadyForDischargePatientDet.optString("AssignedDate", ""), "MMM d, yyyy");
                        Log.d("enrollassgndtolocal", String.valueOf(readyAssigneeNametolocal));
                        readyAssignedDateTxt.setText(readyAssigneeNametolocal);
                        readySectionLayout.setVisibility(View.VISIBLE);
                    } else {
                        readySectionLayout.setVisibility(View.GONE);
                    }

// On Hold
                    JSONObject jsonObjectOnHoldPatientDet = jsonObject.getJSONObject("OnHoldPatientDetais");
                    String holdStatus = jsonObjectOnHoldPatientDet.optString("Status", null);
                    if (holdStatus != null && !"null".equalsIgnoreCase(holdStatus)) {

                        holdAssigneeNameTxt.setText(jsonObjectOnHoldPatientDet.optString("AssigneeName", ""));
                        String holdAssignedtolocal = DateUtils.convertUtcToLocalFormatted(jsonObjectOnHoldPatientDet.optString("AssignedDate", ""), "MMM d, yyyy");
                        Log.d("enrollassgndtolocal", String.valueOf(holdAssignedtolocal));
                        holdAssignedDateTxt.setText(holdAssignedtolocal);
                        holdSectionLayout.setVisibility(View.VISIBLE);
                    } else {
                        holdSectionLayout.setVisibility(View.GONE);
                    }

// InActive
                    JSONObject jsonObjectInActivePatientDetaisDet = jsonObject.getJSONObject("InActivePatientDetais");
                    String inactiveStatus = jsonObjectInActivePatientDetaisDet.optString("Status", null);
                    if (inactiveStatus != null && !"null".equalsIgnoreCase(inactiveStatus)) {
                        inactiveAssigneeNameTxt.setText(jsonObjectInActivePatientDetaisDet.optString("AssigneeName", ""));

                        String inactiveAssignedtolocal = DateUtils.convertUtcToLocalFormatted(jsonObjectInActivePatientDetaisDet.optString("AssignedDate", ""), "MMM d, yyyy");
                        Log.d("enrollassgndtolocal", String.valueOf(inactiveAssignedtolocal));
                        inactiveAssignedDateTxt.setText(inactiveAssignedtolocal);
                        inactiveSectionLayout.setVisibility(View.VISIBLE);
                    } else {
                        inactiveSectionLayout.setVisibility(View.GONE);
                    }

// Discharged
                    JSONObject jsonObjectDischargedPatientDet = jsonObject.getJSONObject("DischargedPatientDetails");
                    String dischargeStatus = jsonObjectDischargedPatientDet.optString("Status", null);
                    if (dischargeStatus != null && !"null".equalsIgnoreCase(dischargeStatus)) {
                        dischargeAssigneeNameTxt.setText(jsonObjectDischargedPatientDet.optString("AssigneeName", ""));

                        String dischargeAssignedtolocal = DateUtils.convertUtcToLocalFormatted(jsonObjectDischargedPatientDet.optString("AssignedDate", ""), "MMM d, yyyy");
                        Log.d("enrollassgndtolocal", String.valueOf(dischargeAssignedtolocal));
                        dischargeAssignedDateTxt.setText(dischargeAssignedtolocal);
                        dischargedSectionLayout.setVisibility(View.VISIBLE);
                    } else {
                        dischargedSectionLayout.setVisibility(View.GONE);
                    }

                    JSONObject jsonObjectEnrolled = jsonObject.getJSONObject("PatientEnrolledDetails");
                    String enrollStatus = jsonObjectEnrolled.optString("Status", null);
                    if (enrollStatus != null && !"null".equalsIgnoreCase(enrollStatus)) {

                        enrollpers.setText(jsonObjectEnrolled.optString("EnrolledPersonal", ""));
                        enrolldate.setText(jsonObjectEnrolled.optString("EnrolledDate", ""));
                        enrolledSectionLayout.setVisibility(View.VISIBLE);
                    } else {
                        enrolledSectionLayout.setVisibility(View.GONE);
                    }

                    JSONObject jsonObjectActive = jsonObject.getJSONObject("ActivePatientDetails");
                    String activeStatus = jsonObjectActive.optString("Status", null);
                    if (activeStatus != null && !"null".equalsIgnoreCase(activeStatus)) {
                        activeAssigneeNameTxt.setText(jsonObjectActive.optString("AssigneeName", ""));
                        String activeAssignedDatetolocal = DateUtils.convertUtcToLocalFormatted(jsonObjectActive.optString("AssignedDate", ""), "MMM d, yyyy");
                        Log.d("enrollassgndtolocal", String.valueOf(activeAssignedDatetolocal));
                        activeAssignedDateTxt.setText(activeAssignedDatetolocal);
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