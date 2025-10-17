package com.rpm.clynx.fragments;

import static android.content.Context.MODE_PRIVATE;
import android.content.Intent;
import android.content.SharedPreferences;
import android.graphics.Typeface;
import android.os.Build;
import android.os.Bundle;
import android.text.Spannable;
import android.text.SpannableString;
import android.text.Spanned;
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
import com.rpm.clynx.adapter.ToDoDateAdapter;
import com.rpm.clynx.adapter.ToDoListAdapter;
import com.rpm.clynx.model.ToDoListModel;
import com.rpm.clynx.utility.DataBaseHelper;
import com.rpm.clynx.utility.DateUtils;
import com.rpm.clynx.utility.Links;
import com.rpm.clynx.utility.Loader;
import com.rpm.clynx.R;
import com.rpm.clynx.utility.NetworkAlert;
import org.joda.time.DateTime;

import java. time. format. DateTimeFormatter;

import org.joda.time.format.DateTimeFormat;
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
    ToDoDateAdapter toDoDateAdapter;
    private LocalDate selectedDate = LocalDate.now();  // start with today

    RecyclerView recyclerView;
    List<LocalDate> dates;
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
        view = inflater.inflate(R.layout.fragment_to_do_list, container, false);
        initPerformBackClick();

        recyclerView = view.findViewById(R.id.recyclerview_dates);

        LinearLayoutManager layoutManager = new LinearLayoutManager(getContext(), LinearLayoutManager.HORIZONTAL, false) {
            @Override
            public boolean canScrollHorizontally() {
                return false; // stop horizontal scroll
            }
        };
        recyclerView.setLayoutManager(layoutManager);

        // --- Build a list of many dates (back and forward) ---
       dates = new ArrayList<>();
        LocalDate start = LocalDate.now().minusDays(100); // allow scrolling backwards
        for (int i = 0; i < 200; i++) {
            dates.add(start.plusDays(i));
        }

        curDay = view.findViewById(R.id.ToDoListFragment_today_date);

        // --- Adapter ---
        toDoDateAdapter = new ToDoDateAdapter(dates, date -> {
            selectedDate = date;

            DateTimeFormatter labelFormatter = DateTimeFormatter.ofPattern("EEEE, MMM d, yyyy");
            curDay.setText(date.format(labelFormatter));

            DateTimeFormatter apiFormatter = DateTimeFormatter.ofPattern("yyyy-MM-dd");
            checkToDoListItems(date.format(apiFormatter));

            toDoDateAdapter.setSelectedDate(date);
        });

        recyclerView.setAdapter(toDoDateAdapter);

        // --- Scroll so current week (Sun–Sat) is visible ---
        LocalDate today = LocalDate.now();

        //  Get Sunday of the *current* week (not next Sunday!)
        LocalDate sunday = today.with(java.time.temporal.TemporalAdjusters.previousOrSame(java.time.DayOfWeek.SUNDAY));
        int sundayIndex = dates.indexOf(sunday);

        LinearLayoutManager lm = (LinearLayoutManager) recyclerView.getLayoutManager();
        if (lm != null && sundayIndex >= 0) {
            lm.scrollToPositionWithOffset(sundayIndex, 0);
        }

        // Highlight today
        toDoDateAdapter.setSelectedDate(today);

        // Set label + call API for today
        curDay.setText(today.format(DateTimeFormatter.ofPattern("EEEE, MMM d, yyyy")));
        checkToDoListItems(today.format(DateTimeFormatter.ofPattern("yyyy-MM-dd")));

       tvnextday = view.findViewById(R.id.todo_nextdate);
       btnprevdate = view.findViewById(R.id.todo_prevdate);

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

        todo_nofoactivites = view.findViewById(R.id.todo_nofoactivites);

       return  view;
    }

    private void nextDate() {
        selectedDate = selectedDate.plusDays(1);   // move one day forward
        updateDateAndFetchData();
    }

    private void prevDate() {
        selectedDate = selectedDate.minusDays(1);  // move one day backward
        updateDateAndFetchData();
    }

    private void updateDateAndFetchData() {
        DateTimeFormatter displayFormatter = DateTimeFormatter.ofPattern("EEEE, MMM d, yyyy");

        // If the selected date is today → show "TODAY - ..."
        if (selectedDate.equals(LocalDate.now())) {
            String todayText = "TODAY - " + selectedDate.format(displayFormatter);

            // Make "TODAY -" bold
            SpannableString spannable = new SpannableString(todayText);
            spannable.setSpan(new StyleSpan(Typeface.BOLD), 0, 6, Spanned.SPAN_EXCLUSIVE_EXCLUSIVE);
            curDay.setText(spannable);
        } else {
            // Normal date
            curDay.setText(selectedDate.format(displayFormatter));
        }

        // Call API with yyyy-MM-dd format
        String formattedDate = selectedDate.format(DateTimeFormatter.ofPattern("yyyy-MM-dd"));
        checkToDoListItems(formattedDate);

        // Update RecyclerView highlight
        toDoDateAdapter.setSelectedDate(selectedDate);

        //  Scroll RecyclerView back to start of this week (Sun–Sat)
        LocalDate sunday = selectedDate.with(java.time.temporal.TemporalAdjusters.previousOrSame(java.time.DayOfWeek.SUNDAY));
        int sundayIndex = dates.indexOf(sunday);  // make sure `dates` is accessible here (move it to class level!)

        LinearLayoutManager lm = (LinearLayoutManager) recyclerView.getLayoutManager();
        if (lm != null && sundayIndex >= 0) {
            lm.scrollToPositionWithOffset(sundayIndex, 0);
        }
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

    String getFormattedText(Date date) throws ParseException {
        Log.d("formatteddate1",date.toString() );
        SimpleDateFormat spf = new SimpleDateFormat("EEEE, MMM d, yyyy", Locale.getDefault());
        Log.d("formatteddate2",date.toString() );
        return spf.format(date);
    }

    @RequiresApi(api = Build.VERSION_CODES.O)
    private void checkToDoListItems(String currentDate) {
        System.out.println("Current Local Date: " + currentDate);
        String cursdt = currentDate+"T00:00:00";
        String todoUtcStDate = DateUtils.convertToUTC(cursdt, "yyyy-MM-dd'T'HH:mm:ss", "yyyy-MM-dd'T'HH:mm:ss");
        String curedt = currentDate+"T23:59:59";
        String todoUtcEdDate = DateUtils.convertToUTC(curedt, "yyyy-MM-dd'T'HH:mm:ss", "yyyy-MM-dd'T'HH:mm:ss");

        String url = Links.BASE_URL+  Links.TODOLIST +
                "StartDate="+cursdt +"&EndDate="+curedt;
        final Loader l1 = new Loader(getActivity());
        l1.show("Please wait...");
        Log.d("urltodo",url.toString());
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

                    DateTimeFormatter inputFormatter = DateTimeFormatter.ofPattern("yyyy-MM-dd"); // match your incoming currentDate format
                    LocalDate selectedDate = LocalDate.parse(currentDate, inputFormatter);
                    LocalDate today = LocalDate.now();

                    DateTimeFormatter outputFormatter = DateTimeFormatter.ofPattern("EEEE, MMM d, yyyy");
                    String formattedDate = selectedDate.format(outputFormatter);

                    if (selectedDate.isEqual(today)) {
                        String prefix = "TODAY - ";
                        String fullText = prefix + formattedDate;

                        SpannableString spannable = new SpannableString(fullText);
                        spannable.setSpan(new StyleSpan(Typeface.BOLD), 0, prefix.length(), Spanned.SPAN_EXCLUSIVE_EXCLUSIVE);

                        curDay.setText(spannable);
                    } else {
                        curDay.setText(formattedDate);
                    }

                } catch (JSONException e) {
                    e.printStackTrace();
                }

                //  Clear old data before adding new
                toDoListModels.clear();
                adapter.notifyDataSetChanged();

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