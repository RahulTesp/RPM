package com.rpm.clynx.fragments;

import static android.content.Context.MODE_PRIVATE;
import android.content.Intent;
import android.content.SharedPreferences;
import android.graphics.Typeface;
import android.os.Build;
import android.os.Bundle;
import android.text.Spannable;
import android.text.SpannableString;
import android.text.style.StyleSpan;
import android.util.Log;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.ImageButton;
import android.widget.TextView;
import android.widget.Toast;
import android.widget.Toolbar;
import androidx.annotation.RequiresApi;
import androidx.core.content.ContextCompat;
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
import com.google.android.material.bottomnavigation.BottomNavigationItemView;
import com.google.android.material.bottomnavigation.BottomNavigationView;
import com.rpm.clynx.adapter.ToDoListAdapter;
import com.rpm.clynx.model.ToDoListModel;
import com.rpm.clynx.utility.DataBaseHelper;
import com.rpm.clynx.utility.DateUtils;
import com.rpm.clynx.utility.Links;
import com.rpm.clynx.utility.Loader;
import com.rpm.clynx.R;
import com.rpm.clynx.utility.NetworkAlert;
import org.joda.time.DateTime;
import org.joda.time.format.DateTimeFormat;
import org.joda.time.format.DateTimeFormatter;
import org.json.JSONArray;
import org.json.JSONException;
import org.json.JSONObject;
import java.text.DateFormat;
import java.text.ParseException;
import java.text.SimpleDateFormat;
import java.time.LocalDate;
import java.util.ArrayList;
import java.util.Calendar;
import java.util.Date;
import java.util.HashMap;
import java.util.List;
import java.util.Locale;
import java.util.Map;
import java.util.TimeZone;

/**
 * A simple {@link Fragment} subclass.
 * Use the {@link ToDoListFragment#newInstance} factory method to
 * create an instance of this fragment.
 */
public class ToDoListFragment extends Fragment  {
    View view;
    RecyclerView recyclerView_todolist;
    private List<ToDoListModel> toDoListModels;
    private ToDoListAdapter adapter;
    RecyclerView.LayoutManager layoutManager;
    DataBaseHelper db;
    SharedPreferences pref;
    SharedPreferences.Editor editor;
    TextView todo_nofoactivites, emptyView;
    ArrayList<String> dateList = new ArrayList<String>();
    ArrayList<Date> dayCompleteList = new ArrayList<Date>();
    ArrayList<TextView> listOfTextView = new ArrayList<TextView>();
    TextView d1,d2,d3,d4,d5,d6,d7, curDay ;
    ImageButton tvnextday,btnprevdate;
    String formattedDate;
    int curIndex ;
    // TODO: Rename parameter arguments, choose names that match
    // the fragment initialization parameters, e.g. ARG_ITEM_NUMBER
    private static final String ARG_PARAM1 = "param1";
    private static final String ARG_PARAM2 = "param2";
    // TODO: Rename and change types of parameters
    private String mParam1;
    private String mParam2;
    private String Token;

    public ToDoListFragment() {
        // Required empty public constructor
    }

    public static ToDoListFragment newInstance(String param1, String param2) {
        ToDoListFragment fragment = new ToDoListFragment();
        Bundle args = new Bundle();
        args.putString(ARG_PARAM1, param1);
        args.putString(ARG_PARAM2, param2);
        fragment.setArguments(args);
        return fragment;
    }

    @Override
    public void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        if (getArguments() != null) {
            mParam1 = getArguments().getString(ARG_PARAM1);
            mParam2 = getArguments().getString(ARG_PARAM2);
        }
    }

    @RequiresApi(api = Build.VERSION_CODES.O)
    @Override
    public View onCreateView(LayoutInflater inflater, ViewGroup container,
                             Bundle savedInstanceState) {
       view =  inflater.inflate(R.layout.fragment_to_do_list, container, false);
       initPerformBackClick();

       d1 = view.findViewById(R.id.ToDoListFragment_current_date1);
       d2 = view.findViewById(R.id.ToDoListFragment_current_date2);
       d3 = view.findViewById(R.id.ToDoListFragment_current_date3);
       d4 = view.findViewById(R.id.ToDoListFragment_current_date4);
       d5 = view.findViewById(R.id.ToDoListFragment_current_date5);
       d6 = view.findViewById(R.id.ToDoListFragment_current_date6);
       d7 = view.findViewById(R.id.ToDoListFragment_current_date7);

       tvnextday = view.findViewById(R.id.todo_nextdate);
       btnprevdate = view.findViewById(R.id.todo_prevdate);

       d1.setOnClickListener(new View.OnClickListener() {
           @Override
           public void onClick(View view) {
               try {
                   changedateoftextviewanddate(1);
               } catch (ParseException e) {
                   e.printStackTrace();
               }
           }
       });
        d2.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View view) {
                try {
                    changedateoftextviewanddate(2);
                } catch (ParseException e) {
                    e.printStackTrace();
                }
            }
        });
        d3.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View view) {
                try {
                    changedateoftextviewanddate(3);
                } catch (ParseException e) {
                    e.printStackTrace();
                }
            }
        });
        d4.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View view) {
                try {
                    changedateoftextviewanddate(4);
                } catch (ParseException e) {
                    e.printStackTrace();
                }
            }
        });
        d5.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View view) {
                try {
                    changedateoftextviewanddate(5);
                } catch (ParseException e) {
                    e.printStackTrace();
                }
            }
        });
        d6.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View view) {
                try {
                    changedateoftextviewanddate(6);
                } catch (ParseException e) {
                    e.printStackTrace();
                }
            }
        });
        d7.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View view) {
                try {
                    changedateoftextviewanddate(7);
                } catch (ParseException e) {
                    e.printStackTrace();
                }
            }
        });

        tvnextday.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View view) {
                nextDate();
            }
        });

        btnprevdate.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View view) {
                prevDate();
            }
        });

       curDay = view.findViewById(R.id.ToDoListFragment_today_date);

        db = new DataBaseHelper(getContext());
        pref = getContext().getSharedPreferences("RPMUserApp", MODE_PRIVATE);
        editor = pref.edit();
        Token = pref.getString("Token", null);

        recyclerView_todolist = view.findViewById(R.id.recyclerview_todolist_vertical);
        emptyView = (TextView) view.findViewById(R.id.empty_viewtodo);
        layoutManager = new LinearLayoutManager(getContext());
        toDoListModels = new ArrayList<>();
        adapter = new ToDoListAdapter(getContext(), toDoListModels);
        recyclerView_todolist.setLayoutManager(layoutManager);
        recyclerView_todolist.setAdapter(adapter);

        LocalDate currentDate = LocalDate.now();

        checkToDoListItems(String.valueOf(currentDate));

        try {
            getweekdata( );
        } catch (ParseException e) {
            e.printStackTrace();
        }
        todo_nofoactivites = view.findViewById(R.id.todo_nofoactivites);

       return  view;
    }

    private void initPerformBackClick() {
        Toolbar toolbar = view.findViewById(R.id.toolbar);
        toolbar.setNavigationIcon(R.drawable.ic_baseline_west_24);
        toolbar.setNavigationOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                // Get the BottomNavigationView and set the selected item
                BottomNavigationView bottomNav = getActivity().findViewById(R.id.navigation);
                if (bottomNav != null) {
                    bottomNav.setSelectedItemId(R.id.Home); // Navigate to Home tab
                }
            }
        });
    }

    @RequiresApi(api = Build.VERSION_CODES.O)
    public void changedateoftextviewanddate(int i) throws ParseException {
        removeColour(i);
        changeColour(i);
        changeData(i);
    }

    @RequiresApi(api = Build.VERSION_CODES.O)
    public void nextDate() {
        if (curIndex < 6){
            int thisindex = curIndex;
            int rmcolorindex = thisindex;
            int chcolorindex = thisindex+2;
            int chdatecollorindex = thisindex+2;

            removeColour(rmcolorindex);
            changeColour(chcolorindex);
            try {
                changeData(chdatecollorindex);
            } catch (ParseException e) {
                e.printStackTrace();
            }
            Log.d("currentindex",String.valueOf(curIndex) +
                    String.valueOf(rmcolorindex) +
                    String.valueOf(chcolorindex)
                    + String.valueOf(chdatecollorindex)
                    + String.valueOf(curIndex));
        }
    }

    @RequiresApi(api = Build.VERSION_CODES.O)
    public void prevDate() {

        if (curIndex>0){
            int thisindex = curIndex;
            Log.d("prevdate",String.valueOf(curIndex)
            );
            int rmcolorindex = thisindex;
            int chcolorindex = thisindex;
            int chdatecollorindex = thisindex;

            removeColour(rmcolorindex);
            changeColour(chcolorindex);
            try {
                changeData(chdatecollorindex);
            } catch (ParseException e) {
                e.printStackTrace();
            }
            Log.d("prevdate",String.valueOf(curIndex) +
                    String.valueOf(rmcolorindex) +
                    String.valueOf(chcolorindex)
                    + String.valueOf(chdatecollorindex)
                    + String.valueOf(curIndex));
        }
    }
   void  getweekdata() throws ParseException {
       Calendar calendar = Calendar.getInstance();
       int day = calendar.get(Calendar.DAY_OF_WEEK);
       Date dt = new Date();
       int l = 7-day;
       int k = l+1;

       for (int i = day-1; i>0; i--){
           DateTime dtOrg = new DateTime(dt);
           DateTime dtPlusOne = dtOrg.minusDays(i);
           DateTimeFormatter fmt = DateTimeFormat.shortDate();
           DateFormat df = DateFormat.getDateInstance(DateFormat.SHORT, Locale.getDefault());
           Log.d("day907",fmt.print(dtPlusOne));
           Log.d("day908",fmt.print(dtPlusOne));

           dateList.add((String) android.text.format.DateFormat.format("dd", dtPlusOne.toDate()));
           dayCompleteList.add(dtPlusOne.toDate());
       }
       for (int i = 0; i <k; i++){
           DateTime dtOrg = new DateTime(dt);
           DateTime dtPlusOne = dtOrg.plusDays(i);
           DateTimeFormatter fmt = DateTimeFormat.shortDate();
           dateList.add((String) android.text.format.DateFormat.format("dd", dtPlusOne.toDate()));
           dayCompleteList.add(dtPlusOne.toDate());
       }
       Log.d("day56",dateList.toString());
       listOfTextView.add(d1);
       listOfTextView.add(d2);
       listOfTextView.add(d3);
       listOfTextView.add(d4);
       listOfTextView.add(d5);
       listOfTextView.add(d6);
       listOfTextView.add(d7);

       d1.setText(dateList.get(0));
       d2.setText(dateList.get(1));
       d3.setText(dateList.get(2));
       d4.setText(dateList.get(3));
       d5.setText(dateList.get(4));
       d6.setText(dateList.get(5));
       d7.setText(dateList.get(6));

       curIndex =day;
       changeColour(day);

       Date todayDate = dayCompleteList.get(day - 1);
       String formattedText = getFormattedText(todayDate);

// Check if this date is today
       Calendar todayCal = Calendar.getInstance();
       Calendar selectedCal = Calendar.getInstance();
       selectedCal.setTime(todayDate);

       if (todayCal.get(Calendar.YEAR) == selectedCal.get(Calendar.YEAR)
               && todayCal.get(Calendar.DAY_OF_YEAR) == selectedCal.get(Calendar.DAY_OF_YEAR)) {

           // Today → make only "TODAY -" bold
           String todayPrefix = "TODAY - ";
           String displayText = todayPrefix + formattedText;
           SpannableString spannable = new SpannableString(displayText);
           spannable.setSpan(new StyleSpan(Typeface.BOLD), 0, todayPrefix.length(),
                   Spannable.SPAN_EXCLUSIVE_EXCLUSIVE);
           curDay.setText(spannable);

       } else {
           curDay.setText(formattedText);

       }


       Log.d("day56",dayCompleteList.toString());
   }

    @RequiresApi(api = Build.VERSION_CODES.O)

    private void changeData(int index) throws ParseException {
        Date selectedDate = dayCompleteList.get(index - 1);
        String formattedText = getFormattedText(selectedDate);

        // Check if selectedDate is today
        Calendar todayCal = Calendar.getInstance();
        Calendar selectedCal = Calendar.getInstance();
        selectedCal.setTime(selectedDate);

        if (todayCal.get(Calendar.YEAR) == selectedCal.get(Calendar.YEAR)
                && todayCal.get(Calendar.DAY_OF_YEAR) == selectedCal.get(Calendar.DAY_OF_YEAR)) {

            // Today → make only "TODAY -" bold
            String todayPrefix = "TODAY - ";
            String displayText = todayPrefix + formattedText;
            SpannableString spannable = new SpannableString(displayText);
            spannable.setSpan(new StyleSpan(Typeface.BOLD), 0, todayPrefix.length(),
                    Spannable.SPAN_EXCLUSIVE_EXCLUSIVE);
            curDay.setText(spannable);
        } else {
            curDay.setText(formattedText);

        }

        SimpleDateFormat inputFormat = new SimpleDateFormat("EEEE, MMM d, yyyy", Locale.US);
        SimpleDateFormat outputFormat = new SimpleDateFormat("yyyy-MM-dd");

        try {
            Date date = inputFormat.parse(formattedText);
            formattedDate = outputFormat.format(date);
            System.out.println("FormattedDate:" + formattedDate);
        } catch (ParseException e) {
            e.printStackTrace();
        }

        checkToDoListItems(formattedDate);
    }

    private void removeColour(int index) {
        listOfTextView.get(curIndex).setBackground(null);
        listOfTextView.get(curIndex).setTextColor(ContextCompat.getColor(getActivity(), R.color.todolist_color1));
    }
    private void changeColour(int index) {
        int ncurIndex = index-1;
        listOfTextView.get(ncurIndex).setBackground(ContextCompat.getDrawable(getActivity(), R.drawable.circel_background));
        listOfTextView.get(ncurIndex).setTextColor(ContextCompat.getColor(getActivity(), R.color.white));
        this.curIndex = ncurIndex;
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
        Log.d("dateToReturn",dateToReturn);
        return dateToReturn; }

    String getFormattedText(Date date) throws ParseException {
        Log.d("formatteddate1",date.toString() );
        SimpleDateFormat spf = new SimpleDateFormat("EEEE, MMM d, yyyy", Locale.getDefault());
        Log.d("formatteddate2",date.toString() );
        return spf.format(date);
    }
      String getFornattedDay(Date date) throws ParseException {
        Log.d("date2",date.toString() );
        SimpleDateFormat spf = new SimpleDateFormat("yyyy-MM-dd HH:mm:ss", Locale.getDefault());
        Log.d("date2",date.toString() );
        return spf.format(date);
    }

    public static String convertToUTC(String inputTimestamp) {
        String inputFormat = "yyyy-MM-dd HH:mm:ss";
        String outputFormat = "yyyy-MM-dd'T'HH:mm:ss";
        SimpleDateFormat sdfInput = new SimpleDateFormat(inputFormat);
        sdfInput.setTimeZone(TimeZone.getTimeZone("UTC"));
        SimpleDateFormat sdfOutput = new SimpleDateFormat(outputFormat);
        sdfOutput.setTimeZone(TimeZone.getTimeZone("UTC"));

        try {
            Date inputDate = sdfInput.parse(inputTimestamp);
            return sdfOutput.format(inputDate);
        } catch (ParseException e) {
            e.printStackTrace();
            return null;
        }
    }

    @RequiresApi(api = Build.VERSION_CODES.O)
    private void checkToDoListItems(String currentDate) {
        System.out.println("Current Local Date: " + currentDate);
        String cursdt = currentDate+"T00:00:00";
        String todoUtcStDate = DateUtils.convertToUTC(cursdt, "yyyy-MM-dd'T'HH:mm:ss", "yyyy-MM-dd'T'HH:mm:ss");
        Log.d("todoUtcStDate", todoUtcStDate);
        String curedt = currentDate+"T23:59:59";
        String todoUtcEdDate = DateUtils.convertToUTC(curedt, "yyyy-MM-dd'T'HH:mm:ss", "yyyy-MM-dd'T'HH:mm:ss");
        Log.d("todoUtcEdDate", todoUtcEdDate);
        Log.d("todliststrDate",todoUtcStDate);
        Log.d("todlistendDate",todoUtcEdDate);

        String url = Links.BASE_URL+  Links.TODOLIST +
                "StartDate="+cursdt +"&EndDate="+curedt;
        final Loader l1 = new Loader(getActivity());
        l1.show("Please wait...");
        toDoListModels.clear();
        StringRequest stringRequest = new StringRequest(Request.Method.GET, url, new Response.Listener<String>() {
            @Override
            public void onResponse(String response) {
                Log.d("todo2response",response.toString());
                JSONArray jsonArray=null;
                l1.dismiss();
                try {
                    jsonArray = new JSONArray(response);
                    Log.d("jsontodoleng", String.valueOf(jsonArray.length()));
                    Log.d("log_data_array",jsonArray.toString());
                } catch (JSONException e) {
                    e.printStackTrace();
                }

                todo_nofoactivites.setText(jsonArray.length()+" Activities");
                for (int i = 0; i < jsonArray.length(); i++){
                    recyclerView_todolist.setVisibility(View.VISIBLE);
                    emptyView.setVisibility(View.GONE);
                    try {
                        JSONObject jsonObject1 = jsonArray.getJSONObject(i);
                        ToDoListModel toDoListModel   = new ToDoListModel();
                        toDoListModel.setScheduleType(jsonObject1.getString("ActivityName"));
                        toDoListModel.setDecription(jsonObject1.getString("Description"));
                        String todoutcTime = DateUtils.formatDate( jsonObject1.getString("Date"),"MMM dd");
                        Log.d("todoutcTime",todoutcTime);
                        toDoListModel.setTime( todoutcTime);
                        toDoListModels.add(toDoListModel);
                        adapter.notifyDataSetChanged();
                    } catch (JSONException e) {
                        e.printStackTrace();
                    } catch (ParseException e) {
                        e.printStackTrace();
                    } finally {
                        adapter.notifyDataSetChanged();
                    }
                }

                if (jsonArray.length()<=0){
                    recyclerView_todolist.setVisibility(View.GONE);
                    emptyView.setVisibility(View.VISIBLE);
                }
            }
        } ,new Response.ErrorListener() {
            @Override
            public void onErrorResponse(VolleyError error) {
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
}