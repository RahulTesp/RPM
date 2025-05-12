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
import com.rpm.clynx.adapter.ActivitySchedulesListAdapter;
import com.rpm.clynx.model.ActivitySchedulesItemModel;
import com.rpm.clynx.model.ActivitySchedulesModel;
import com.rpm.clynx.utility.DataBaseHelper;
import com.rpm.clynx.utility.Links;
import com.rpm.clynx.utility.Loader;
import com.rpm.clynx.R;
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
import com.android.volley.Response;
import com.android.volley.VolleyError;
import com.android.volley.toolbox.StringRequest;
import com.android.volley.toolbox.Volley;
import org.joda.time.DateTime;
import java.util.Date;
import java.util.HashMap;
import java.util.Map;
import java.util.TimeZone;

@RequiresApi(api = Build.VERSION_CODES.O)
public class ActivitySchedulesFragment extends Fragment  {
    RecyclerView actSchedR;
    RecyclerView recyclerView_activity;
    TextView emptyView;
    DataBaseHelper db;
    TextView  curDay ;
    ImageButton btnnext, btnprev;
    SharedPreferences pref;
    SharedPreferences.Editor editor;
    String Token;
    private ActivitySchedulesListAdapter adapter;
    private List<ActivitySchedulesModel> activitySchedulesModels;
    RecyclerView.LayoutManager layoutManager;
    DateTime curdate = new DateTime();
    View view;
    int curIndex;
    YearMonth thisMonth = YearMonth.now();
    java.text.SimpleDateFormat spdf= new java.text.SimpleDateFormat("MMMM, yyyy");
    @Override
    public View onCreateView(LayoutInflater inflater, ViewGroup container,
                             Bundle savedInstanceState) {
        // Inflate the layout for this fragment
        view = inflater.inflate(R.layout.fragment_activity_schedules, container, false);
        recyclerView_activity = view.findViewById(R.id.fragmentActivitySchedules);
        emptyView = (TextView) view.findViewById(R.id.empty_viewactivity);
        db = new DataBaseHelper(getContext());
        pref = this.getActivity().getSharedPreferences("RPMUserApp",getContext().MODE_PRIVATE);
        editor = pref.edit();
        Token = pref.getString("Token", null);
        actSchedR =  view.findViewById(R.id.fragmentActivitySchedules);
        layoutManager = new LinearLayoutManager(getContext());
        activitySchedulesModels = new ArrayList<>();
        adapter = new ActivitySchedulesListAdapter(activitySchedulesModels,getContext());
        actSchedR.setLayoutManager(layoutManager);
        actSchedR.setAdapter(adapter);
        curDay = view.findViewById(R.id.ActSchedFragment_today_date);

        Date dt = new Date();
        Log.d("curdate.toDate()",curdate.toDate().toString());
        curDay.setText(spdf.format(curdate.toDate()));
        btnnext = view.findViewById(R.id.nextdateonactivity);
        btnprev = view.findViewById(R.id.prevdateonactivity);
        java.time.LocalDate firstMonth = thisMonth.atDay( 1 );     // 2015-01-01
        java.time.LocalDate lastMonth = thisMonth.atEndOfMonth();

        checkactsched(firstMonth.toString(),lastMonth.toString());

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

    @RequiresApi(api = Build.VERSION_CODES.O)
    private void prevdateVital() throws ParseException {
        DateTimeFormatter monthYearFormatter = DateTimeFormatter.ofPattern("MMMM yyyy");
        Calendar cal = Calendar.getInstance();
        cal.add(Calendar.MONTH, -1);
        Date result = cal.getTime();
        Log.d("result",result.toString());
        Log.d("thisMonth",thisMonth.toString());
        YearMonth prevMonth    = thisMonth.minusMonths(1);
        curDay.setText(prevMonth.format(monthYearFormatter));
        curIndex = curIndex-1;
        thisMonth = prevMonth ;
        Log.d("thisMonth",thisMonth.toString());
        Log.d("prevMonth",prevMonth.toString());
        Log.d("thisMonth",thisMonth.toString());
        java.time.LocalDate firstOfMonth = thisMonth.atDay( 1 );     // 2015-01-01
        java.time.LocalDate lastOfMonth = thisMonth.atEndOfMonth();
        Log.d("firstOfMonth",firstOfMonth.toString());
        Log.d("lastOfMonth",lastOfMonth.toString());

        checkactsched(firstOfMonth.toString(),lastOfMonth.toString());
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
            YearMonth nextMonth    = thisMonth.plusMonths(1);
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

            checkactsched(firstOfMonth.toString(),lastOfMonth.toString());
    }
    private void checkactsched(String startDate, String endDate) {
        Log.d("startDate schedules",startDate.toString());
        Log.d("endDate schedules",endDate.toString());
        String url = Links.BASE_URL+  Links.GET_ACTIVITYSCHEDULES +
                "StartDate=" + startDate.toString()
                + "&EndDate=" + endDate.toString();
        final Loader l1 = new Loader(getActivity());
        l1.show("Please wait...");
        activitySchedulesModels.clear();
        Log.d("response url",url.toString());
        StringRequest stringRequest = new StringRequest(Request.Method.GET, url, new Response.Listener<String>() {
            @Override
            public void onResponse(String response) {
                Log.d("response GET_ACTIVITYSCHEDULES",response.toString());
                JSONArray jsonArrayData = null;
                l1.dismiss();
                try {
                    jsonArrayData = new JSONArray(response);
                    Log.d("log_data_array",jsonArrayData.toString());
                    if (jsonArrayData.length()<=0){
                        recyclerView_activity.setVisibility(View.GONE);
                        emptyView.setVisibility(View.VISIBLE);
                    }

                    for (int i = 0; i < jsonArrayData.length(); i++){
                        try {
                            JSONObject jsonDateList = jsonArrayData.getJSONObject(i);
                            JSONArray jsonArrayNL=new JSONArray(jsonDateList.getString("SchedueInfos"));
                            ArrayList<ActivitySchedulesItemModel> nim = new ArrayList<ActivitySchedulesItemModel>();

                            if (jsonArrayNL.length()<=0){
                                recyclerView_activity.setVisibility(View.GONE);
                                emptyView.setVisibility(View.VISIBLE);
                            }

                            for (int j = 0; j < jsonArrayNL.length(); j++){
                                recyclerView_activity.setVisibility(View.VISIBLE);
                                emptyView.setVisibility(View.GONE);
                                Log.d(" jsonArrayNL", jsonArrayNL.toString());
                                nim.add(new ActivitySchedulesItemModel(jsonArrayNL.getJSONObject(j).getString("ScheduleType"),
                                        jsonArrayNL.getJSONObject(j).getString("ScheduleTime") ,
                                        jsonArrayNL.getJSONObject(j).getString("AssignedByName")
                                ));
                                Log.d("nimNoti", nim.toString());
                            }
                            activitySchedulesModels.add(new ActivitySchedulesModel( uTCToLocal("yyyy-MM-dd'T'HH:mm:ss","MMM dd, yyyy" ,jsonDateList.getString("ScheduleDate")),nim));
                            adapter.notifyDataSetChanged();
                        }
                        catch (JSONException e) {
                            e.printStackTrace();
                        }finally {
                            adapter.notifyDataSetChanged();
                        }
                    }
                } catch (JSONException e) {
                    e.printStackTrace();
                }
            }
        } ,new Response.ErrorListener() {
            @Override
            public void onErrorResponse(VolleyError error) {
                //loader.dismiss();
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

    public static String uTCToLocal(String dateFormatInPut, String dateFomratOutPut, String datesToConvert) {
        String dateToReturn = datesToConvert;
        java.text.SimpleDateFormat sdf = new java.text.SimpleDateFormat(dateFormatInPut);
        sdf.setTimeZone(TimeZone.getTimeZone("UTC"));
        Date gmt = null;
        java.text.SimpleDateFormat sdfOutPutToSend = new java.text.SimpleDateFormat(dateFomratOutPut);
        sdfOutPutToSend.setTimeZone(TimeZone.getDefault());

        try {
            gmt = sdf.parse(datesToConvert);
            dateToReturn = sdfOutPutToSend.format(gmt);

        } catch (ParseException e) {
            e.printStackTrace();
        }
        return dateToReturn; }
}