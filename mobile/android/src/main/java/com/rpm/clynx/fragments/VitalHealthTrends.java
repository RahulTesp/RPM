package com.rpm.clynx.fragments;

import static android.content.Context.MODE_PRIVATE;
import android.annotation.SuppressLint;
import android.content.SharedPreferences;
import android.graphics.Color;
import android.graphics.PorterDuff;
import android.os.Build;
import android.os.Bundle;
import android.util.Log;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.FrameLayout;
import android.widget.ImageButton;
import android.widget.ImageView;
import android.widget.LinearLayout;
import android.widget.TextView;
import android.widget.Toast;
import androidx.annotation.RequiresApi;
import androidx.cardview.widget.CardView;
import androidx.fragment.app.Fragment;
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
import com.github.mikephil.charting.charts.LineChart;
import com.github.mikephil.charting.components.Legend;
import com.github.mikephil.charting.components.XAxis;
import com.github.mikephil.charting.components.YAxis;
import com.github.mikephil.charting.data.Entry;
import com.github.mikephil.charting.data.LineData;
import com.github.mikephil.charting.data.LineDataSet;
import com.github.mikephil.charting.formatter.IndexAxisValueFormatter;
import com.github.mikephil.charting.interfaces.datasets.ILineDataSet;
import com.google.android.material.tabs.TabItem;
import com.google.android.material.tabs.TabLayout;
import com.rpm.clynx.activity.ClinicalInfoActivity;
import com.rpm.clynx.adapter.NewVitalListAdapter;
import com.rpm.clynx.model.NewVitalsItemModel;
import com.rpm.clynx.model.NewVitalsModel;
import com.rpm.clynx.model.VitalItemsModel;
import com.rpm.clynx.service.TimeFormatter;
import com.rpm.clynx.utility.CustomMarkerView;
import com.rpm.clynx.utility.DataBaseHelper;
import com.rpm.clynx.utility.DateUtils;
import com.rpm.clynx.utility.Links;
import com.rpm.clynx.utility.Loader;
import com.rpm.clynx.R;
import com.rpm.clynx.utility.MyMarkerView;
import com.rpm.clynx.utility.NetworkAlert;
import org.joda.time.DateTime;
import org.json.JSONArray;
import org.json.JSONException;
import org.json.JSONObject;
import java.text.ParseException;
import java.text.SimpleDateFormat;
import java.time.LocalDate;
import java.util.ArrayList;
import java.util.Calendar;
import java.util.Date;
import java.util.HashMap;
import java.util.List;
import java.util.Map;
import java.util.TimeZone;
import android.content.Intent;
import java.time.format.DateTimeFormatter;

public class VitalHealthTrends extends Fragment {

    View view;
    TextView emptyView, emptyChart,textHighlight7,textHighlight30,headername,viewMoreTextView,VitalNameLabel7,VitalNameLabel30 ;
    RecyclerView recyclerView_newvitals;
    private List<NewVitalsModel> newVitalsModels;
    LineChart mpLinechart7, mpLinechart30;
    private NewVitalListAdapter adapter;
    RecyclerView.LayoutManager layoutManager;
    DataBaseHelper db;
    SharedPreferences pref;
    SharedPreferences.Editor editor;
    private static final String ARG_PARAM1 = "param1";
    private static final String ARG_PARAM2 = "param2";
    String ProgramTypeName;
    ArrayList<VitalItemsModel> aList = new ArrayList<VitalItemsModel>();
    TextView  curDay ;
    ImageView list, trends;
    TabItem t7,t30;
    private String mParam1;
    private String mParam2;
    TabLayout guiTabs;
    private String Token;
    int curIndex;
    Date dt = new Date();
    ImageButton btnnext, btnprev;
    DateTime curdate = new DateTime();
    SimpleDateFormat spdf= new SimpleDateFormat("EEEE- MMM d, yyyy");
    SimpleDateFormat spdff= new SimpleDateFormat("yyyy-MM-dd'T'HH:mm:ss");
    private TextView tooltip;
    FrameLayout frame30days,rootFrame;
    LinearLayout linearLayout7D30D,headearLayout;
    CardView c1;
    CardView cardViewLineChart7, cardViewLineChart30;
    private LinearLayout chartHealthTrends,chartHealthTrends30;
    MyMarkerView markerView77,markerView3030;

    public VitalHealthTrends() {
        // Required empty public constructor
    }

    @Override
    public void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        if (getArguments() != null) {
            mParam1 = getArguments().getString(ARG_PARAM1);
            mParam2 = getArguments().getString(ARG_PARAM2);
        }
    }

    @Override
    public View onCreateView(LayoutInflater inflater, ViewGroup container,
                             Bundle savedInstanceState) {

        // Inflate the layout for this fragment
        view =  inflater.inflate(R.layout.new_fragment_vitals, container, false);
        viewMoreTextView = view.findViewById(R.id.viewMore);
        viewMoreTextView.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                // Handle the "View More..." click here
                openClinicalInfoActivityFromVitals();
            }
        });

        VitalNameLabel30 = (TextView) view.findViewById(R.id.vitalname30);
        final LinearLayout linearLayoutDateView = (LinearLayout) view.findViewById(R.id.headerdateview);
        linearLayout7D30D = (LinearLayout) view.findViewById(R.id.header7D30D);
        headearLayout = (LinearLayout) view.findViewById(R.id.headers);
        final FrameLayout frameVitalSummary = (FrameLayout) view.findViewById(R.id.fl_main);
        final FrameLayout frame7days = (FrameLayout) view.findViewById(R.id.fl_sub7);
        frame30days = (FrameLayout) view.findViewById(R.id.fl_sub30);
        rootFrame = (FrameLayout) view.findViewById(R.id.rootframe);
        c1 = (CardView) view.findViewById(R.id.card_viewvitals);
        t7 = (TabItem) view.findViewById(R.id.tabitem7);
        t30 = (TabItem) view.findViewById(R.id.tabitem30);
        guiTabs = view.findViewById(R.id.tablayout7D30D);

        linearLayout7D30D.setVisibility(View.INVISIBLE);
        frame7days.setVisibility(View.INVISIBLE);
        frame30days.setVisibility(View.INVISIBLE);
        mpLinechart7 = (LineChart) view.findViewById(R.id.linechart7);
        mpLinechart30 = (LineChart) view.findViewById(R.id.linechart30);

        chartHealthTrends = view.findViewById(R.id.chartHealthTrends);
        chartHealthTrends30 = view.findViewById(R.id.chartHealthTrends30);

        markerView77 = new MyMarkerView(getContext(), R.layout.custom_marker_layout7);
        markerView3030 = new MyMarkerView(getContext(), R.layout.custom_marker_layout30);

        textHighlight7 = (TextView) markerView77.findViewById(R.id.marker_text7);
        textHighlight30 = (TextView) markerView3030.findViewById(R.id.marker_text30);

        db = new DataBaseHelper(getContext());
        pref = getContext().getSharedPreferences("RPMUserApp", MODE_PRIVATE);
        editor = pref.edit();
        Token = pref.getString("Token", null);

        ProgramTypeName = pref.getString("ProgramTypeName", null);

        recyclerView_newvitals = view.findViewById(R.id.recyclerview_newvitals);
        emptyView = (TextView) view.findViewById(R.id.empty_viewnew);
        emptyChart = (TextView) view.findViewById(R.id.empty_viewnew);

        headername = (TextView) view.findViewById(R.id.headerval);
        btnnext = view.findViewById(R.id.nextdateonvitalnew);
        btnprev = view.findViewById(R.id.prevdateonvitalnew);

        list = (ImageView) view.findViewById(R.id.iconList);
        list.setOnClickListener(new View.OnClickListener() {
            @SuppressLint("ResourceAsColor")
            @Override
            public void onClick(View view) {
                trends.setBackgroundColor(getResources().getColor(R.color.Primary_bg));
                list.setBackgroundColor(getResources().getColor(R.color.Primary_dark));
                list.setColorFilter(list.getContext().getResources().getColor(R.color.white), PorterDuff.Mode.SRC_ATOP);
                trends.setColorFilter(list.getContext().getResources().getColor(R.color.Primary_dark), PorterDuff.Mode.SRC_ATOP);
                headername.setText("Vitals");
                c1.setVisibility(View.INVISIBLE);

                linearLayoutDateView.setVisibility(View.VISIBLE);
                frameVitalSummary.setVisibility(View.VISIBLE);

                linearLayout7D30D.setVisibility(View.INVISIBLE);
                frame7days.setVisibility(View.INVISIBLE);
                frame30days.setVisibility(View.INVISIBLE);
            }
        });

        trends = (ImageView) view.findViewById(R.id.iconTrends);
        trends.setOnClickListener(new View.OnClickListener() {
            @SuppressLint("ResourceAsColor")
            @Override
            public void onClick(View view) {
                list.setBackgroundColor(getResources().getColor(R.color.Primary_bg));
                trends.setBackgroundColor(getResources().getColor(R.color.Primary_dark));
                trends.setColorFilter(list.getContext().getResources().getColor(R.color.white), PorterDuff.Mode.SRC_ATOP);
                list.setColorFilter(list.getContext().getResources().getColor(R.color.Primary_dark), PorterDuff.Mode.SRC_ATOP);
                headername.setText("Health Trends");
                linearLayoutDateView.setVisibility(View.INVISIBLE);
                c1.setVisibility(View.VISIBLE);
                linearLayout7D30D.setVisibility(View.VISIBLE);
                frameVitalSummary.setVisibility(View.INVISIBLE);
                frame7days.setVisibility(View.VISIBLE);
            }
        });

        guiTabs.addOnTabSelectedListener(new TabLayout.OnTabSelectedListener() {
            @Override
            public void onTabSelected(TabLayout.Tab tab) {
                switch (tab.getPosition()) {
                    case 0:
//                        codes related to the first tab
                        linearLayoutDateView.setVisibility(View.INVISIBLE);
                        linearLayout7D30D.setVisibility(View.VISIBLE);
                        frameVitalSummary.setVisibility(View.INVISIBLE);
                        frame7days.setVisibility(View.VISIBLE);
                        frame30days.setVisibility(View.INVISIBLE);
                        break;
                    case 1:
//                        codes related to the second tab
                        linearLayoutDateView.setVisibility(View.INVISIBLE);
                        linearLayout7D30D.setVisibility(View.VISIBLE);
                        frameVitalSummary.setVisibility(View.INVISIBLE);
                        frame7days.setVisibility(View.INVISIBLE);
                        frame30days.setVisibility(View.VISIBLE);
                        break;
                }
            }

            @Override
            public void onTabUnselected(TabLayout.Tab tab) {

            }

            @Override
            public void onTabReselected(TabLayout.Tab tab) {

            }
        });

        // Get the system's default time zone
        TimeZone systemTimeZone = TimeZone.getDefault();

        // Get the current date and time in the system's time zone
        Calendar currentCalendar = Calendar.getInstance();

        // Set the time zone of currentCalendar to the system's time zone
        currentCalendar.setTimeZone(systemTimeZone);

        // Now, currentCalendar holds the current date and time in the system's time zone
        System.out.println("Currentdateandtimeinsystemtimezone" + currentCalendar.getTime());

        // Calculate the end date (current date and time)
        Calendar endDateCalendar = (Calendar) currentCalendar.clone();

        // Calculate the start date (7 days back)
        Calendar startDate7Calendar = (Calendar) currentCalendar.clone();
        startDate7Calendar.add(Calendar.DAY_OF_MONTH, -6);

        // Calculate the start date (30 days back)
        Calendar startDate30Calendar = (Calendar) currentCalendar.clone();
        startDate30Calendar.add(Calendar.DAY_OF_MONTH, -29);

        // Format the dates in the desired format
        SimpleDateFormat sdf = new SimpleDateFormat("yyyy-MM-dd'T'HH:mm:ss.SSS'Z'");
        sdf.setTimeZone(systemTimeZone); // Use the system's time zone

        String formattedCurrentDate = sdf.format(currentCalendar.getTime());
        String formattedStart7Date = sdf.format(startDate7Calendar.getTime());
        String formattedStart30Date = sdf.format(startDate30Calendar.getTime());
        String formattedEndDate = sdf.format(endDateCalendar.getTime());

        // Print the formatted dates
        Log.d("Current Date and Time (UTC):",formattedCurrentDate);
        Log.d("Start Date (7 days back, UTC):",formattedStart7Date);
        Log.d("Start Date (30 days back, UTC):",formattedStart30Date);
        Log.d("End Date (UTC): ",formattedEndDate);

        checkVitalData7(formattedStart7Date,formattedEndDate);
        checkVitalData30(formattedStart30Date,formattedEndDate );

        btnnext.setOnClickListener(new View.OnClickListener() {
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

        layoutManager = new LinearLayoutManager(getContext());
        newVitalsModels = new ArrayList<>();
        adapter = new NewVitalListAdapter( newVitalsModels,getContext());
        recyclerView_newvitals.setLayoutManager(new LinearLayoutManager(getContext(), LinearLayoutManager.HORIZONTAL, false));
        recyclerView_newvitals.setAdapter(adapter);
        curDay = view.findViewById(R.id.VitalFragment_today_dt);

        DateTime curdt = new DateTime(dt);
        getLocalToUTCDate(dt);
        Log.d("current date",getLocalToUTCDate(dt));

        checkvitels(curdt,curdt);
        curDay.setText(spdf.format(curdt.toDate()));
        return  view;
    }

    private void openClinicalInfoActivityFromVitals() {
        Intent intent = new Intent(getActivity(), ClinicalInfoActivity.class);
        intent.putExtra("opened_from", "VITALS");
        startActivity(intent);
    }
    public String getLocalToUTCDate(Date date) {
        Calendar calendar = Calendar.getInstance();
        calendar.setTime(date);
        calendar.setTimeZone(TimeZone.getTimeZone("UTC"));
        Date time = calendar.getTime();
        @SuppressLint("SimpleDateFormat") SimpleDateFormat outputFmt = new SimpleDateFormat("yyyy-MM-dd'T'HH:mm:ss.SSS'Z'");
        outputFmt.setTimeZone(TimeZone.getTimeZone("UTC"));
        return outputFmt.format(time);
    }

    private void prevdateVital() throws ParseException {
        Date dt = new Date();
        DateTime curdt = new DateTime(dt);
        DateTime dtPlusOne = curdate.minusDays(1);
        DateTime dtToday = curdate;
        curDay.setText(spdf.format(dtPlusOne.toDate()));
        curIndex = curIndex-1;
        curdate = dtPlusOne;
        Log.d("dtPlusOne",dtPlusOne.toString());
        Log.d("dtToday",dtToday.toString());
        Log.d("curdt",curdt.toString());
        Log.d("day1",spdff.format(dtPlusOne.toDate()));
        Log.d("daycurdt1",spdff.format(curdt.toDate()));

        checkvitels(dtPlusOne,dtPlusOne);
    }

    private void nextDateVital() throws ParseException {
        Date dt = new Date();
        DateTime curdt = new DateTime(dt);
        DateTime dtPlusOne = curdate.plusDays(1);
        curDay.setText(spdf.format(dtPlusOne.toDate()));
        curIndex = curIndex+1;
        curdate = dtPlusOne;
        Log.d("nextDateaddsOne",dtPlusOne.toString());
        Log.d("nextDatecurdt",curdt.toString());

        checkvitels(dtPlusOne,dtPlusOne);
        Log.d("day1",spdf.format(dtPlusOne.toDate()));
    }

    private void checkvitels(DateTime startDate, DateTime endDate) {
        Log.d("startDate",startDate.toString());
        Log.d("endDate",endDate.toString());
        Log.d("day1startDate",spdff.format(startDate.toDate()));
        Log.d("daycurdt1endDate",spdff.format(endDate.toDate()));
        String asubstring1 =  spdff.format(startDate.toDate()).substring(0, 10);
        Log.d("substr1", asubstring1);
        String result1 = asubstring1+"T00:00:00";
        Log.d("result1", result1);
        String utcDateStrstart = DateUtils.convertToUTC(result1, "yyyy-MM-dd'T'HH:mm:ss", "yyyy-MM-dd'T'HH:mm:ss");
        Log.d("utcDateStrstart", utcDateStrstart);
        String asubstring2 =  spdff.format(endDate.toDate()).substring(0, 10);
        Log.d("substr2", asubstring2);
        String result2 = asubstring2+"T23:59:59";
        Log.d("result2", result2);
        String utcDateStrend = DateUtils.convertToUTC(result2, "yyyy-MM-dd'T'HH:mm:ss", "yyyy-MM-dd'T'HH:mm:ss");
        Log.d("utcDateStrend", utcDateStrend);

        String url = Links.BASE_URL+  Links.VITALS + "StartDate=" +
                utcDateStrstart
                +
                "&EndDate=" +
                utcDateStrend
                ;
        final Loader l1 = new Loader(getActivity());
        Log.d("url",url.toString());
        l1.show("Please wait...");

        newVitalsModels.clear();
        aList.clear();

        StringRequest stringRequest = new StringRequest(Request.Method.GET, url, new Response.Listener<String>() {
            @RequiresApi(api = Build.VERSION_CODES.O)
            @Override
            public void onResponse(String response) {
                Log.d("VITALresponse",response.toString());

                JSONArray jsonArray=null;
                JSONArray jsonArray1=null;
                JSONArray jsonArray3=null;

                l1.dismiss();

                try {
                    JSONObject jsonObject = new JSONObject(response);
                    jsonArray = new JSONArray(jsonObject.getString("vitals"));
                    Log.d("vhjsonArray",jsonArray.toString());

                    for (int i = 0; i < jsonArray.length(); i++){
                        recyclerView_newvitals.setVisibility(View.VISIBLE);
                        emptyView.setVisibility(View.GONE);
                        try {
                            JSONObject jsonObject1 = jsonArray.getJSONObject(i);
                            NewVitalsModel newVitalsModel = new NewVitalsModel();
                            newVitalsModel.setVitalName(jsonObject1.getString("VitalName"));
                            String vitalname = jsonObject1.getString("VitalName");
                            jsonArray1 = new JSONArray(jsonObject1.getString("VitalDetails"));
                            Log.d("vitaldetailsaray", jsonArray1.toString());

                            for (int j = 0; j < jsonArray1.length(); j++) {
                                JSONObject jsonObject2 = jsonArray1.getJSONObject(j);
                                Log.i("jsonobject2index", jsonObject2.toString());
                                jsonArray3 = new JSONArray(jsonObject2.getString("Vitaldata"));
                                Log.d("vitaldetailsaray3", jsonArray3.toString());
                                Log.i("jsonarray3", jsonArray3.toString());

                                ArrayList<NewVitalsItemModel> nim = new ArrayList<NewVitalsItemModel>();
                                for (int k = 0; k < jsonArray3.length(); k++) {
                                    JSONObject jsonObject3 = jsonArray3.getJSONObject(k);
                                    Log.i("value", jsonObject3.getString("Value"));
                                    Log.i("value", jsonObject3.getString("unit"));
                                    Log.i("length", String.valueOf(k));

                                    String utcTime = uTCToLocal("yyyy/MM/dd'T'HH:mm:ss","h:mm a" , jsonObject3.getString("time"));
                                    if(vitalname.equals("Blood Glucose")) {
                                        nim.add(new NewVitalsItemModel(jsonObject3.getString("Value"), jsonObject3.getString("unit"), jsonObject3.getString("MeasureName"), utcTime));
                                    }
                                    else
                                    {
                                        nim.add(new NewVitalsItemModel(jsonObject3.getString("Value"), jsonObject3.getString("unit"),"", utcTime));
                                    }
                                    Log.d("nimNoti", nim.toString());
                                }
                                newVitalsModels.add(new NewVitalsModel( jsonObject1.getString("VitalName"),nim));
                                adapter.notifyDataSetChanged();
                            }
                        } catch(JSONException e){
                            e.printStackTrace();
                        } finally{
                            adapter.notifyDataSetChanged();
                        }
                    }

                } catch (JSONException e) {
                    e.printStackTrace();
                }
                if (jsonArray.length()<=0){
                    recyclerView_newvitals.setVisibility(View.GONE);
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

    private void checkVitalData7(String formattedStartDate,String formattedEndDate ) {
        DateTimeFormatter inputFormatter = null;
        if (android.os.Build.VERSION.SDK_INT >= android.os.Build.VERSION_CODES.O) {
            inputFormatter = DateTimeFormatter.ofPattern("yyyy-MM-dd'T'HH:mm:ss.SSS'Z'");
        }
        LocalDate extractedstDate = null;
        LocalDate extractedendDate = null;

        if (android.os.Build.VERSION.SDK_INT >= android.os.Build.VERSION_CODES.O) {
            extractedstDate = LocalDate.parse(formattedStartDate, inputFormatter);
            extractedendDate = LocalDate.parse(formattedEndDate, inputFormatter);
            Log.d("extractedstDate7", String.valueOf(extractedstDate));
            Log.d("extractedendDate7", String.valueOf(extractedendDate));

        }
        String formattedsDate = extractedstDate.toString() + "T00:00:00";
        String formattedendDate = extractedendDate.toString() + "T23:59:59";
        System.out.println("FormasDate7:" + formattedsDate);
        System.out.println("FormaendDate7:" + formattedendDate);

        String sdUTC = DateUtils.convertToUTC(formattedsDate, "yyyy-MM-dd'T'HH:mm:ss","yyyy-MM-dd'T'HH:mm:ss");
        String endUTC = DateUtils.convertToUTC(formattedendDate, "yyyy-MM-dd'T'HH:mm:ss","yyyy-MM-dd'T'HH:mm:ss");

        Log.d("sdUTC7",sdUTC);
        Log.d("endUTC7",endUTC);

        String url = Links.BASE_URL+  Links.VITALHEALTHTRENDS7 + "StartDate="+ sdUTC+ "&EndDate="+endUTC;
        final Loader l1 = new Loader(getActivity());

        l1.show("Please wait...");
        StringRequest stringRequest = new StringRequest(Request.Method.GET, url, new Response.Listener<String>() {
            @Override
            public void onResponse(String response) {
                l1.dismiss();
                try {
                    Log.d("chartresponse", response.toString());
                    if (response.trim().startsWith("{")) {
                        // Single Object Case
                        JSONObject jsonObject = new JSONObject(response);
                        Log.d("jsonObjectdev7", jsonObject.toString());
                        processVitalData(jsonObject);
                    } else if (response.trim().startsWith("[")) {
                        // Array of Objects Case
                        JSONArray jsonArray = new JSONArray(response);
                        for (int i = 0; i < jsonArray.length(); i++) {
                            Log.d("jsonArraypre7", jsonArray.toString());
                            JSONObject jsonObject = jsonArray.getJSONObject(i);
                            Log.d("jsonObjectpre7", jsonArray.toString());
                            processVitalData(jsonObject);
                        }
                    } else {
                        Log.e("ResponseError", "Unexpected response format");
                    }
                } catch (JSONException e) {
                    Log.e("JSONError", "Error parsing JSON: " + e.getMessage());
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

    private void processVitalData(JSONObject jsonObject) {
        try {
            LayoutInflater inflater = LayoutInflater.from(requireContext());
            // Inflate a new instance of the card layout for each item
            View cardViewLayout = inflater.inflate(R.layout.card_view_health_trends, null);

            // Get the CardView from the inflated layout
            cardViewLineChart7 = cardViewLayout.findViewById(R.id.cardVitalHealthTrends);
            cardViewLineChart7.setVisibility(View.VISIBLE); // Ensure it’s visible

            // Find views inside CardView and set data
            TextView vitalNameTV = cardViewLayout.findViewById(R.id.vitalname7);
            String vitalName = jsonObject.isNull("VitalName") ? "No Readings" : jsonObject.optString("VitalName", "").trim();
            Log.d("VitalNameCheck", "VitalName: " + vitalName);
            vitalNameTV.setText(vitalName);

                LinearLayout.LayoutParams layoutParams = new LinearLayout.LayoutParams(
                        ViewGroup.LayoutParams.WRAP_CONTENT,
                        ViewGroup.LayoutParams.WRAP_CONTENT
                );
                layoutParams.setMargins(5, 0, 5, 0); // Left, Top, Right, Bottom margins
                cardViewLineChart7.setLayoutParams(layoutParams);

                // Find the LineChart inside the CardView
                LineChart lineChart = cardViewLayout.findViewById(R.id.linechart7);

                // Extract Time and Values dynamically
                JSONArray timeArray = jsonObject.optJSONArray("Time");
                JSONArray valuesArray = jsonObject.optJSONArray("Values");

                Log.d("valuesArray", String.valueOf(valuesArray));

                if (timeArray != null && valuesArray != null && valuesArray.length() > 0) {
                    List<String> xAxisLabels = new ArrayList<>();
                    for (int t = 0; t < timeArray.length(); t++) {
                        Log.d("timeArray", String.valueOf(timeArray));
                        String utcTimestamp = timeArray.optString(t, "");  // Get the timestamp at index 't'
                        Log.d("utcTimestamp", utcTimestamp); // Log the extracted timestamp
                        String localTime = TimeFormatter.formatChartTimestampFromUTC(utcTimestamp);
                        Log.d("localTime", localTime);
                        xAxisLabels.add(localTime);
                    }

                    List<ILineDataSet> dataSets = new ArrayList<>();
                    for (int v = 0; v < valuesArray.length(); v++) {
                        JSONObject valueObject = valuesArray.getJSONObject(v);
                        String label = valueObject.optString("label", "Unknown");
                        JSONArray dataArray = valueObject.optJSONArray("data");

                        List<Entry> entries = new ArrayList<>();
                        for (int d = 0; d < dataArray.length(); d++) {
                            float yValue = dataArray.isNull(d) ? 0 : Float.parseFloat(dataArray.optString(d, "0"));
                            entries.add(new Entry(d, yValue));
                        }

                        if (!entries.isEmpty()) {
                            LineDataSet dataSet = new LineDataSet(entries, label);
                            dataSet.setColor(getRandomColor(v)); // Assign unique colors dynamically
                            dataSet.setCircleColor(getRandomColor(v));
                            dataSet.setLineWidth(2f);
                            dataSet.setCircleRadius(3f);
                            dataSet.setDrawValues(true);
                            dataSet.setDrawHighlightIndicators(true); // Enables highlight indicators
                            dataSet.setHighLightColor(Color.RED); // Set highlight line color
                            dataSets.add(dataSet);
                        }
                    }
                    // Configure LineChart
                    LineData lineData = new LineData(dataSets);
                    configureLineChart(lineChart, xAxisLabels, lineData);
                }
                // Add the dynamically created CardView to the container
                chartHealthTrends.addView(cardViewLayout);
        }
            catch (JSONException e) {
                Log.e("JSONError", "Error processing JSON object", e);
            }
    }

    private void processVitalData30(JSONObject jsonObject) {
        try {
            LayoutInflater inflater = LayoutInflater.from(requireContext());
            // Inflate a new instance of the card layout for each item
            View cardViewLayout = inflater.inflate(R.layout.card_view_health_trends30, null);

            // Get the CardView from the inflated layout
            cardViewLineChart30 = cardViewLayout.findViewById(R.id.cardVitalHealthTrends30);
            cardViewLineChart30.setVisibility(View.VISIBLE); // Ensure it’s visible

            // Find views inside CardView and set data
            TextView vitalNameTV = cardViewLayout.findViewById(R.id.vitalname30);
            String vitalName = jsonObject.isNull("VitalName") ? "No Readings" : jsonObject.optString("VitalName", "").trim();
            Log.d("VitalNameCheck", "VitalName: " + vitalName);
            vitalNameTV.setText(vitalName);

                LinearLayout.LayoutParams layoutParams = new LinearLayout.LayoutParams(
                        ViewGroup.LayoutParams.WRAP_CONTENT,
                        ViewGroup.LayoutParams.WRAP_CONTENT
                );
                layoutParams.setMargins(5, 0, 5, 0); // Left, Top, Right, Bottom margins
                cardViewLineChart30.setLayoutParams(layoutParams);

                // Find the LineChart inside the CardView
                LineChart lineChart = cardViewLayout.findViewById(R.id.linechart30);

                // Extract Time and Values dynamically
                JSONArray timeArray = jsonObject.optJSONArray("Time");
                JSONArray valuesArray = jsonObject.optJSONArray("Values");
                Log.d("valuesArray", String.valueOf(valuesArray));

                if (timeArray != null && valuesArray != null && valuesArray.length() > 0) {
                    List<String> xAxisLabels = new ArrayList<>();
                    for (int t = 0; t < timeArray.length(); t++) {
                        Log.d("timeArray", String.valueOf(timeArray));
                        String utcTimestamp = timeArray.optString(t, "");  // Get the timestamp at index 't'
                        Log.d("utcTimestamp", utcTimestamp); // Log the extracted timestamp
                        String localTime = TimeFormatter.formatChartTimestampFromUTC(utcTimestamp);
                        Log.d("localTime",localTime);
                        xAxisLabels.add(localTime);
                    }

                    List<ILineDataSet> dataSets = new ArrayList<>();
                    for (int v = 0; v < valuesArray.length(); v++) {
                        JSONObject valueObject = valuesArray.getJSONObject(v);
                        String label = valueObject.optString("label", "Unknown");
                        JSONArray dataArray = valueObject.optJSONArray("data");
                        Log.d("dataArraysize", String.valueOf(dataArray.length()));

                        List<Entry> entries = new ArrayList<>();
                        for (int d = 0; d < dataArray.length(); d++) {
                            float yValue = dataArray.isNull(d) ? 0 : Float.parseFloat(dataArray.optString(d, "0"));
                            entries.add(new Entry(d, yValue));
                        }

                        if (!entries.isEmpty()) {
                            LineDataSet dataSet = new LineDataSet(entries, label);
                            dataSet.setColor(getRandomColor(v)); // Assign unique colors dynamically
                            dataSet.setCircleColor(getRandomColor(v));
                            dataSet.setLineWidth(2f);
                            dataSet.setCircleRadius(3f);
                            dataSet.setDrawValues(true);
                            dataSet.setDrawHighlightIndicators(true); // Enables highlight indicators
                            dataSet.setHighLightColor(Color.RED); // Set highlight line color
                            dataSets.add(dataSet);
                        }
                    }

                    // Configure LineChart
                    LineData lineData = new LineData(dataSets);
                    configureLineChart(lineChart, xAxisLabels, lineData);
                }
                // Add the dynamically created CardView to the container
                chartHealthTrends30.addView(cardViewLayout);
        }
        catch (JSONException e) {
            Log.e("JSONError", "Error processing JSON object", e);
        }
    }
    private int getRandomColor(int index) {
        int[] colors = {Color.RED, Color.BLUE, Color.GREEN, Color.CYAN, Color.MAGENTA, Color.YELLOW};
        return colors[index % colors.length];
    }

    private void configureLineChart(LineChart lineChart, List<String> xAxisLabels, LineData lineData) {
        lineChart.setData(lineData);
        lineChart.getDescription().setEnabled(false);
        lineChart.setTouchEnabled(true);
        lineChart.setPinchZoom(true);

        // Show only 30 data points at a time
        lineChart.setVisibleXRangeMaximum(15);
        lineChart.setDragEnabled(true);  // Enable scrolling
        lineChart.setScaleXEnabled(true);  // Allow horizontal scaling

        XAxis xAxis = lineChart.getXAxis();
        xAxis.setValueFormatter(new IndexAxisValueFormatter(xAxisLabels));
        xAxis.setPosition(XAxis.XAxisPosition.BOTTOM);
        xAxis.setGranularityEnabled(true);
        xAxis.setGranularity(1f);
        xAxis.setDrawGridLines(false);
        xAxis.setLabelRotationAngle(-85f);
        // Ensure labels are visible
        xAxis.setLabelCount(15);  // Display labels for 30 points at a time

        YAxis leftAxis = lineChart.getAxisLeft();
        leftAxis.setDrawGridLines(true);
        YAxis rightAxis = lineChart.getAxisRight();
        rightAxis.setEnabled(false);

        Legend legend = lineChart.getLegend();
        legend.setTextSize(12f);
        legend.setForm(Legend.LegendForm.LINE);

        CustomMarkerView markerView = new CustomMarkerView(lineChart.getContext(), R.layout.custom_chart_marker_view, xAxisLabels, lineChart);
        markerView.setChartView(lineChart);
        lineChart.setMarker(markerView);

        lineChart.invalidate(); // Refresh the chart
    }

    private void checkVitalData30(String formattedStartDate, String formattedEndDate) {
        DateTimeFormatter inputFormatter = null;
        if (android.os.Build.VERSION.SDK_INT >= android.os.Build.VERSION_CODES.O) {
            inputFormatter = DateTimeFormatter.ofPattern("yyyy-MM-dd'T'HH:mm:ss.SSS'Z'");
        }
        LocalDate extractedstDate = null;
        LocalDate extractedendDate = null;

        if (android.os.Build.VERSION.SDK_INT >= android.os.Build.VERSION_CODES.O) {
            extractedstDate = LocalDate.parse(formattedStartDate, inputFormatter);
            extractedendDate = LocalDate.parse(formattedEndDate, inputFormatter);
        }

        String formattedsDate = extractedstDate.toString() + "T00:00:00";
        String formattedendDate = extractedendDate.toString() + "T23:59:59";
        System.out.println("FormasDate30" + formattedsDate);
        System.out.println("FormaendDate30" + formattedendDate);

        String sdUTC = DateUtils.convertToUTC(formattedsDate, "yyyy-MM-dd'T'HH:mm:ss","yyyy-MM-dd'T'HH:mm:ss");
        String endUTC = DateUtils.convertToUTC(formattedendDate, "yyyy-MM-dd'T'HH:mm:ss","yyyy-MM-dd'T'HH:mm:ss");
        Log.d("sdUTC30",sdUTC);
        Log.d("endUTC30",endUTC);

        String url = Links.BASE_URL+  Links.VITALHEALTHTRENDS30 + "StartDate="+ sdUTC+ "&EndDate="+endUTC;
        final Loader l1 = new Loader(getActivity());
        l1.show("Please wait...");
        StringRequest stringRequest = new StringRequest(Request.Method.GET, url, new Response.Listener<String>() {
            @Override
            public void onResponse(String response) {
                l1.dismiss();
                try {
                    Log.d("chartresponse", response.toString());
                    if (response.trim().startsWith("{")) {
                        // Single Object Case
                        JSONObject jsonObject = new JSONObject(response);
                        Log.d("jsonObjectdev", jsonObject.toString());
                        processVitalData30(jsonObject);
                    } else if (response.trim().startsWith("[")) {
                        // Array of Objects Case
                        JSONArray jsonArray = new JSONArray(response);
                        for (int i = 0; i < jsonArray.length(); i++) {
                            Log.d("jsonArraypre", jsonArray.toString());
                            JSONObject jsonObject = jsonArray.getJSONObject(i);
                            Log.d("jsonObjectpre", jsonArray.toString());
                            processVitalData30(jsonObject);
                        }
                    } else {
                        Log.e("ResponseError", "Unexpected response format");
                    }
                } catch (JSONException e) {
                    Log.e("JSONError", "Error parsing JSON: " + e.getMessage());
                }
            }
        } ,new Response.ErrorListener() {
            @Override
            public void onErrorResponse(VolleyError error) {
                Log.d("error30", String.valueOf(error));
                Toast.makeText(getContext(), "No data available!", Toast.LENGTH_SHORT).show();
                l1.dismiss();
                if (error == null || error.networkResponse == null) {
                    // No internet, show network dialog
                    if (getContext() != null) {  // Ensure Fragment is attached
                        NetworkAlert.showNetworkDialog(requireContext());
                    }
                    return;
                }
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

    public static String uTCToLocal(String dateFormatInPut, String dateFomratOutPut, String datesToConvert) {
        String dateToReturn = datesToConvert;
        SimpleDateFormat sdf = new SimpleDateFormat(dateFormatInPut);
        sdf.setTimeZone(TimeZone.getTimeZone("UTC"));

        Date gmt = null;
        SimpleDateFormat sdfOutPutToSend = new SimpleDateFormat(dateFomratOutPut);
        sdfOutPutToSend.setTimeZone(TimeZone.getDefault());

        try {
            gmt = sdf.parse(datesToConvert);
            dateToReturn = sdfOutPutToSend.format(gmt);
        } catch (ParseException e) {
            e.printStackTrace();
        }
        return dateToReturn; }
}