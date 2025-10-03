package com.rpm.clynx.fragments;

import android.content.SharedPreferences;
import android.os.Build;
import android.os.Bundle;
import android.util.Log;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.ImageButton;
import android.widget.TextView;
import androidx.annotation.RequiresApi;
import androidx.fragment.app.Fragment;
import com.rpm.clynx.adapter.VitalReadingsListAdapter;
import com.rpm.clynx.model.VitalReadingsItemModel;
import com.rpm.clynx.model.VitalReadingsModel;
import com.rpm.clynx.utility.DataBaseHelper;
import com.rpm.clynx.utility.DateUtils;
import com.rpm.clynx.utility.Links;
import com.rpm.clynx.utility.Loader;
import org.joda.time.LocalDate;
import java.time.YearMonth;
import org.json.JSONArray;
import org.json.JSONException;
import org.json.JSONObject;
import java.text.ParseException;
import java.time.format.DateTimeFormatter;
import java.util.ArrayList;
import java.util.Calendar;
import java.util.List;
import android.widget.Toast;
import androidx.recyclerview.widget.LinearLayoutManager;
import androidx.recyclerview.widget.RecyclerView;
import com.android.volley.AuthFailureError;
import com.android.volley.DefaultRetryPolicy;
import com.android.volley.Request;
import com.android.volley.RequestQueue;
import com.android.volley.toolbox.StringRequest;
import com.android.volley.toolbox.Volley;
import org.joda.time.DateTime;
import com.rpm.clynx.R;
import java.util.Date;
import java.util.HashMap;
import java.util.Map;

@RequiresApi(api = Build.VERSION_CODES.O)
public class VitalReadingsFragment extends Fragment  {
    RecyclerView recyclerView_vitalreadings;
    TextView emptyView;
    DataBaseHelper db;
    TextView  curDay ;
    ImageButton btnnext, btnprev;
    SharedPreferences pref;
    SharedPreferences.Editor editor;
    String Token;
    private VitalReadingsListAdapter adapter;
    private List<VitalReadingsModel> vitalReadingsModels;
    RecyclerView.LayoutManager layoutManager;
    DateTime curdate = new DateTime();
    View view;
    int curIndex;
    YearMonth thisMonth    = YearMonth.now();
    java.text.SimpleDateFormat spdf= new java.text.SimpleDateFormat("MMMM, yyyy");

    @Override
    public View onCreateView(LayoutInflater inflater, ViewGroup container,
                             Bundle savedInstanceState) {
        // Inflate the layout for this fragment
        view = inflater.inflate(R.layout.fragment_vital_readings, container, false);

        emptyView = (TextView) view.findViewById(R.id.empty_viewvtlR);
        db = new DataBaseHelper(getContext());
        pref = this.getActivity().getSharedPreferences("RPMUserApp",getContext().MODE_PRIVATE);
        editor = pref.edit();
        Token = pref.getString("Token", null);

        recyclerView_vitalreadings =  view.findViewById(R.id.fragmentVitalReadings);
        layoutManager = new LinearLayoutManager(getContext(), LinearLayoutManager.HORIZONTAL, false);
        vitalReadingsModels = new ArrayList<>();

        adapter = new VitalReadingsListAdapter(vitalReadingsModels, getContext(),
                new VitalReadingsListAdapter.OnViewMoreClickListener() {
                    @Override
                    public void onViewMoreClicked(VitalReadingsModel vital) {
                        // Open the clinical info fragment when "View More" is clicked
                        openClinicalInfoFragmentFromVitals(vital);
                    }
                },
                false);
        recyclerView_vitalreadings.setLayoutManager(layoutManager);
        recyclerView_vitalreadings.setAdapter(adapter);
        curDay = view.findViewById(R.id.VtlRdngsFragment_today_date);
        Date dt = new Date();
        DateTime curdt = new DateTime(dt);
        Log.d("curdate.toDate()",curdate.toDate().toString());
        curDay.setText(spdf.format(curdate.toDate()));
        btnnext = view.findViewById(R.id.nextdateonavtlR);
        btnprev = view.findViewById(R.id.prevdateonvtlR);
        java.time.LocalDate firstMonth = thisMonth.atDay( 1 );     // 2015-01-01
        java.time.LocalDate lastMonth = thisMonth.atEndOfMonth();

        checkvtlRead(firstMonth.toString(),lastMonth.toString());

        btnnext.setOnClickListener(new View.OnClickListener() {
            @RequiresApi(api = Build.VERSION_CODES.O)
            @Override
            public void onClick(View view) {
                Log.d("day1","entered");
                try {
                    nextDateVital();
                } catch (ParseException e) {
                    e.printStackTrace();
                }
            }
        });

        btnprev.setOnClickListener(new View.OnClickListener() {
            @RequiresApi(api = Build.VERSION_CODES.O)
            @Override
            public void onClick(View view) {
                Log.d("day1","entered");
                try {
                    prevdateVital();
                } catch (ParseException e) {
                    e.printStackTrace();
                }
            }
        });

        return  view;
    }

    private void openClinicalInfoFragmentFromVitals(VitalReadingsModel vital) {
        ClinicalInfoFragment clinicalInfoFragment = new ClinicalInfoFragment();

        // Pass arguments via Bundle
        Bundle args = new Bundle();
        args.putString("opened_from", "VITALS");
        args.putString("vital_type", vital.getVitalType());
        clinicalInfoFragment.setArguments(args);


        requireActivity().getSupportFragmentManager()
                .beginTransaction()
                .replace(R.id.fl_main, clinicalInfoFragment)
                .addToBackStack(null)  // so back button works
                .commit();
    }


    @RequiresApi(api = Build.VERSION_CODES.O)
    private void prevdateVital() throws ParseException {
        DateTimeFormatter monthYearFormatter = DateTimeFormatter.ofPattern("MMMM yyyy");
        Calendar cal = Calendar.getInstance();
        cal.add(Calendar.MONTH, -1);
        Date result = cal.getTime();
        Log.d("resultprevdateVital",result.toString());
        Log.d("thisMonthprevdateVital",thisMonth.toString());
        YearMonth prevMonth    = thisMonth.minusMonths(1);
        curDay.setText(prevMonth.format(monthYearFormatter));
        curIndex = curIndex-1;
        thisMonth = prevMonth ;

        Log.d("thisMonthC",thisMonth.toString());
        Log.d("prevMonth",prevMonth.toString());
        Log.d("thisMonth",thisMonth.toString());
        java.time.LocalDate firstOfMonth = thisMonth.atDay( 1 );     // 2015-01-01
        java.time.LocalDate lastOfMonth = thisMonth.atEndOfMonth();
        Log.d("prevdateVitalfirstOfMonth",firstOfMonth.toString());
        Log.d("prevdateVitallastOfMonth",lastOfMonth.toString());

// Check the length of the month
        int daysInMonth = thisMonth.lengthOfMonth();
        Log.d("daysInMonth", String.valueOf(daysInMonth));
        if (daysInMonth == 30) {
            Log.d("30 DAYS", "30");
            // Current month has 30 days
            // Add your logic here
        } else if (daysInMonth == 31) {
            Log.d("31 DAYS", "31");
            // Current month has 31 days
            // Add your logic here
        } else {
            // Handle February separately
            if (thisMonth.getMonthValue() == 2) {
                Log.d("28 OR 29 DAYS", "2829");
                // February has 28 or 29 days
                // Add your logic here
            }
        }

        checkvtlRead(firstOfMonth.toString(),lastOfMonth.toString());
    }

    @RequiresApi(api = Build.VERSION_CODES.O)
    private void nextDateVital() throws ParseException {
        DateTimeFormatter monthYearFormatter = DateTimeFormatter.ofPattern("MMMM yyyy");

        Calendar cal = Calendar.getInstance();
        cal.add(Calendar.MONTH, +1);
        Date result = cal.getTime();
        Log.d("result",result.toString());

        LocalDate mydate = LocalDate.now(); // Or whatever you want
        mydate = mydate.plusMonths(1);
        Log.d("mydate",mydate.toString());

        Log.d("thisMonth",thisMonth.toString());
        YearMonth currentMonth = YearMonth.now();
        YearMonth nextMonth = thisMonth.plusMonths(1);

        if (nextMonth.isAfter(currentMonth)) {
            Toast.makeText(getContext(), "You cannot view future months.", Toast.LENGTH_SHORT).show();
            return;
        }

        curDay.setText(nextMonth.format(monthYearFormatter));
        curIndex = curIndex+1;
        thisMonth = nextMonth ;

        Log.d("thisMonth",thisMonth.toString());
        Log.d("prevMonth",nextMonth.toString());
        Log.d("thisMonth",thisMonth.toString());
        Log.d("nextMonth.format(monthYearFormatter)",nextMonth.format(monthYearFormatter));

        Calendar.getInstance().getActualMaximum(Calendar.DAY_OF_MONTH);
        java.time.LocalDate firstOfMonth = thisMonth.atDay( 1 );     // 2015-01-01
        java.time.LocalDate lastOfMonth = thisMonth.atEndOfMonth();
        Log.d("firstOfMonth",firstOfMonth.toString());
        Log.d("lastOfMonth",lastOfMonth.toString());

        checkvtlRead(firstOfMonth.toString(),lastOfMonth.toString());
    }

    private void checkvtlRead(String startDate, String endDate) {
        Log.d("startDateschedules", startDate);
        String vrstdt = startDate + "T00:00:00";
        String vrUtcStDate = DateUtils.convertToUTC(vrstdt, "yyyy-MM-dd'T'HH:mm:ss", "yyyy-MM-dd'T'HH:mm:ss");
        Log.d("vrUtcStDate", vrUtcStDate);
        Log.d("endDate schedules", endDate);
        String vrenddt = endDate + "T23:59:59";
        String vrUtcEndDate = DateUtils.convertToUTC(vrenddt, "yyyy-MM-dd'T'HH:mm:ss", "yyyy-MM-dd'T'HH:mm:ss");
        Log.d("vrUtcEndDate", vrUtcEndDate);

        String url = Links.BASE_URL + Links.GET_VITALREADINGS +
                "StartDate=" + vrUtcStDate + "&EndDate=" + vrUtcEndDate;

        final Loader loader = new Loader(getActivity());
        loader.show("Please wait...");

        vitalReadingsModels.clear();
        Log.d("response url", url);

        StringRequest stringRequest = new StringRequest(Request.Method.GET, url,
                response -> {
                    loader.dismiss();
                    try {
                        JSONObject jsonObject = new JSONObject(response);
                        parseVitalReadings(jsonObject);
                        adapter.notifyDataSetChanged();
                    } catch (JSONException e) {
                        e.printStackTrace();
                    }
                },
                error -> {
                    Toast.makeText(getContext(), "No data available!", Toast.LENGTH_SHORT).show();
                    loader.dismiss();
                }) {
            @Override
            public Map<String, String> getHeaders() throws AuthFailureError {
                Map<String, String> headers = new HashMap<>();
                headers.put("Bearer", Token);
                Log.d("headers", headers.toString());
                return headers;
            }
        };

        RequestQueue requestQueue = Volley.newRequestQueue(getActivity());
        stringRequest.setRetryPolicy(new DefaultRetryPolicy(60000, DefaultRetryPolicy.DEFAULT_MAX_RETRIES, DefaultRetryPolicy.DEFAULT_BACKOFF_MULT));
        requestQueue.add(stringRequest);
    }


    private void parseVitalReadings(JSONObject jsonObject) {
        Map<String, String> vitalTypeMapping = new HashMap<>();
        vitalTypeMapping.put("BloodPressure", "Blood Pressure");
        vitalTypeMapping.put("BloodGlucose", "Blood Glucose");
        vitalTypeMapping.put("Weight", "Weight");
        vitalTypeMapping.put("BloodOxygen", "Blood Oxygen");

        for (String key : vitalTypeMapping.keySet()) {
            if (!jsonObject.has(key) || jsonObject.isNull(key)) {
                // Skip null vitals (e.g., BloodGlucose is null)
                continue;
            }

            JSONArray jsonArray = jsonObject.optJSONArray(key);
            ArrayList<VitalReadingsItemModel> items = parseVitalData(jsonArray, key);

            if (jsonArray != null && jsonArray.length() == 0) {
                // Add "No Readings" if the array is empty ([])
                items.add(createNoReadingsItem(key));
            }

            //  Always add each vital type with data (or "No Readings" if empty)
            vitalReadingsModels.add(new VitalReadingsModel(vitalTypeMapping.get(key), items));
        }
    }

    private ArrayList<VitalReadingsItemModel> parseVitalData(JSONArray jsonArray, String type) {
        ArrayList<VitalReadingsItemModel> items = new ArrayList<>();

        for (int i = 0; i < jsonArray.length(); i++) {
            try {
                JSONObject obj = jsonArray.getJSONObject(i);
                String VRutcTime = DateUtils.convertUtcToLocalFormatted(obj.getString("ReadingTime"), "MMM dd, hh:mm a");

                switch (type) {
                    case "BloodPressure":
                        items.add(new VitalReadingsItemModel(
                                obj.getString("Systolic"),
                                obj.getString("Diastolic"),
                                obj.getString("pulse"),
                                "", "", "", "", "",
                                VRutcTime,
                                obj.getString("SystolicStatus"),
                                obj.getString("DiastolicStatus"),
                                obj.getString("pulseStatus"),
                                obj.getString("Status"),
                                "", ""
                        ));
                        break;

                    case "BloodGlucose":
                        items.add(new VitalReadingsItemModel(
                                "", "", "", "", "", "",
                                obj.optString("BGmgdl", "No Readings"),
                                obj.optString("Schedule", "No Readings"),
                                VRutcTime, "", "", "",
                                obj.optString("Status", "No Readings"),
                                "", ""
                        ));
                        break;

                    case "BloodOxygen":
                        items.add(new VitalReadingsItemModel(
                                "", "", "",
                                obj.getString("Oxygen"),
                                obj.getString("Pulse"),
                                "", "", "",
                                VRutcTime, "", "", "",
                                obj.getString("Status"),
                                obj.getString("OxygenStatus"),
                                obj.getString("PulseStatus")
                        ));
                        break;

                    case "Weight":
                        items.add(new VitalReadingsItemModel(
                                "", "", "", "", "",
                                obj.optString("BWlbs", "No Readings"),
                                "", "",
                                VRutcTime, "", "", "",
                                obj.optString("Status", "No Readings"),
                                "", ""
                        ));
                        break;
                }
            } catch (JSONException e) {
                e.printStackTrace();
            }
        }

        return items;
    }

    private VitalReadingsItemModel createNoReadingsItem(String type) {
        return new VitalReadingsItemModel(
                "No Readings", "", "", "", "",
                "", "", "",
                "", "", "", "",
                "", "", ""
        );
    }
}