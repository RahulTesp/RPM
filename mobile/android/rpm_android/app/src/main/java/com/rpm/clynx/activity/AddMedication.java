package com.rpm.clynx.activity;

import android.annotation.SuppressLint;
import android.app.DatePickerDialog;
import android.content.Intent;
import android.content.SharedPreferences;
import android.icu.text.SimpleDateFormat;
import android.os.Bundle;
import android.util.Log;
import android.view.View;
import android.widget.AdapterView;
import android.widget.ArrayAdapter;
import android.widget.CheckBox;
import android.widget.CompoundButton;
import android.widget.DatePicker;
import android.widget.EditText;
import android.widget.ImageButton;
import android.widget.ImageView;
import android.widget.Spinner;
import android.widget.TextView;
import android.widget.Toast;
import androidx.appcompat.app.AppCompatActivity;
import com.android.volley.AuthFailureError;
import com.android.volley.DefaultRetryPolicy;
import com.android.volley.Request;
import com.android.volley.RequestQueue;
import com.android.volley.Response;
import com.android.volley.VolleyError;
import com.android.volley.toolbox.StringRequest;
import com.android.volley.toolbox.Volley;
import com.rpm.clynx.fragments.Login;
import com.rpm.clynx.utility.DataBaseHelper;
import com.rpm.clynx.utility.Links;
import com.rpm.clynx.utility.Loader;
import com.rpm.clynx.R;
import com.rpm.clynx.utility.MyApplication;
import org.json.JSONException;
import org.json.JSONObject;
import java.text.ParseException;
import java.util.Calendar;
import java.util.Date;
import java.util.HashMap;
import java.util.Locale;
import java.util.Map;

public class AddMedication extends AppCompatActivity {

    EditText comments;
    String dropdown1val, dropdown2val ;
    TextView RightTextView;
    SharedPreferences pref;
    SharedPreferences.Editor editor;
    String Token,SelectedStartDate, SelectedEndtDate, Comments,  EndDateValue;
    DataBaseHelper db;
    private ImageButton pickStartDateBtn;
    TextView medname,spinnererror,intervalerror,dateerror,selectedStartDate,selectedEndDate;
    CheckBox mng,aftr,eve,nyt ;
    SimpleDateFormat sdf = new SimpleDateFormat("MMM dd, yyyy", Locale.getDefault());
    Spinner dropdown1,dropdown2;
    boolean valid1;
    private int count = 1;
    private TextView countValue;
    ImageView minusIcon,plusIcon;
    @SuppressLint("MissingInflatedId")
    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.add_medication);
        medname = findViewById(R.id.itemMedcnnm);
        spinnererror = findViewById(R.id.spinerror);
        intervalerror = findViewById(R.id.intervalerror);
        dateerror = findViewById(R.id.dateerror);
        mng = findViewById(R.id.checkBoxMorning);
        aftr = findViewById(R.id.checkBoxAfternoon);
        eve = findViewById(R.id.checkBoxEvening);
        nyt = findViewById(R.id.checkBoxNight);
        pickStartDateBtn = findViewById(R.id.idBtnStartDate);
        selectedStartDate = findViewById(R.id.idSelectedStartDate);
        comments =  findViewById(R.id.editcomments);
        RightTextView =  findViewById(R.id.rightTextView);
        initToolbar();
        pref = this.getSharedPreferences("RPMUserApp",this.MODE_PRIVATE);
        editor = pref.edit();
        Token = pref.getString("Token", null);
        dropdown1 = findViewById(R.id.spinner1);
        String[] items1 = new String[]{"","Monthly","Weekly", "Daily", "Alternative"};
        ArrayAdapter<String> adapter1 = new ArrayAdapter<>(this, android.R.layout.simple_spinner_dropdown_item, items1);
        dropdown1.setAdapter(adapter1);
        Log.d("dropdown1", items1.toString());

        dropdown1.setOnItemSelectedListener(new AdapterView.OnItemSelectedListener() {
                                              @Override
                                              public void onItemSelected(AdapterView<?> parentView, View selectedItemView, int position, long id) {
                                                  // your code here
                                                  dropdown1val = dropdown1.getSelectedItem().toString();
                                                  Log.d("dropdown1val", dropdown1val.toString());

                                                  if(!dropdown1val.isEmpty() &&  !dropdown2val.isEmpty()  )
                                                  {
                                                      spinnererror.setVisibility(View.INVISIBLE);
                                                      Log.d("111noitemslectd","noitemslectd");
                                                  }
                                                  if(dropdown1val == "Monthly")
                                                  {
                                                      RightTextView.setText("Month");
                                                  }
                                                  else if(dropdown1val == "Weekly")
                                                  {
                                                      RightTextView.setText("Week");
                                                  }
                                                  else if(dropdown1val == "Daily")
                                                  {
                                                      RightTextView.setText("Day");
                                                  }
                                                  else if(dropdown1val == "Alternative")
                                                  {
                                                      RightTextView.setText("Day");
                                                  }
                                              }

                                                @Override
                                                public void onNothingSelected(AdapterView<?> adapterView) {
                                                    Log.d("drop1onNothingSelected", "onNothingSelected");
                                                }
        });

        dropdown2 = findViewById(R.id.spinner2);
        String[] items2 = new String[]{"","Before Meal", "After Meal"};
        ArrayAdapter<String> adapter2 = new ArrayAdapter<>(this, android.R.layout.simple_spinner_dropdown_item, items2);
        dropdown2.setAdapter(adapter2);
        Log.d("dropdown2", dropdown2.toString());

        dropdown2.setOnItemSelectedListener(new AdapterView.OnItemSelectedListener() {
            @Override
            public void onItemSelected(AdapterView<?> parentView, View selectedItemView, int position, long id) {
                // your code here
               dropdown2val = dropdown2.getSelectedItem().toString();
                Log.d("dropdown2val", dropdown2val.toString());
                if(!dropdown1val.isEmpty() &&  !dropdown2val.isEmpty() )
                {
                    spinnererror.setVisibility(View.INVISIBLE);
                    Log.d("111noitemslectd","noitemslectd");
                }
            }

            @Override
            public void onNothingSelected(AdapterView<?> adapterView) {

            }
        });

        CompoundButton.OnCheckedChangeListener checkBoxListener = new CompoundButton.OnCheckedChangeListener() {
            @Override
            public void onCheckedChanged(CompoundButton buttonView, boolean isChecked) {
                // If any of the checkboxes is checked, make intervalerror invisible
                if (mng.isChecked() || aftr.isChecked() || eve.isChecked() || nyt.isChecked()) {
                    intervalerror.setVisibility(View.INVISIBLE);
                } else {
                }
            }
        };

        mng.setOnCheckedChangeListener(checkBoxListener);
        aftr.setOnCheckedChangeListener(checkBoxListener);
        eve.setOnCheckedChangeListener(checkBoxListener);
        nyt.setOnCheckedChangeListener(checkBoxListener);

        // on below line we are adding click listener for our pick date button
        pickStartDateBtn.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                // on below line we are getting
                // the instance of our calendar.
                final Calendar c = Calendar.getInstance();
                // on below line we are getting
                // our day, month and year.
                int year = c.get(Calendar.YEAR);
                int month = c.get(Calendar.MONTH);
                int day = c.get(Calendar.DAY_OF_MONTH);
                // on below line we are creating a variable for date picker dialog.
                DatePickerDialog datePickerDialog = new DatePickerDialog(
                        // on below line we are passing context.
                        AddMedication.this,
                        new DatePickerDialog.OnDateSetListener() {
                            @Override
                            public void onDateSet(DatePicker view, int year,
                                                  int monthOfYear, int dayOfMonth) {
                                // on below line we are setting date to our text view.
                                selectedStartDate.setText(year + "-" + (monthOfYear + 1) + "-" + dayOfMonth);
                                dateerror.setVisibility(View.INVISIBLE);
                            }
                        },
                        // on below line we are passing year,
                        // month and day for selected date in our date picker.
                        year, month, day);
                // at last we are calling show to
                // display our date picker dialog.
                datePickerDialog.show();
            }
        });

         countValue = findViewById(R.id.countValue);
         minusIcon = findViewById(R.id.minusIcon);
         plusIcon = findViewById(R.id.plusIcon);

        // Set a click listener for the minus icon
        minusIcon.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                if (count > 1) {
                    count--;
                    countValue.setText(String.valueOf(count));
                }
            }
        });

        // Set a click listener for the plus icon
        plusIcon.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                count++;
                countValue.setText(String.valueOf(count));
            }
        });
    }
    @Override
    protected void onResume() {
        super.onResume();
        Log.d("actResume","homeonResume");
        if (pref.getBoolean("loginstatus", false) == false){
            Log.d("loginstsfrmhome2", String.valueOf(pref.getBoolean("loginstatus", false)));
            editor.clear();
            editor.commit();
            db.deleteProfileData("myprofileandprogram");
            db.deleteData();

            try {
                Log.d("loginlatestActivity", String.valueOf(((MyApplication) getApplication()).getLatestActivity()));
                Intent intentlogout = new Intent((((MyApplication) getApplication()).getLatestActivity()), Login.class);
                intentlogout.addFlags(Intent.FLAG_ACTIVITY_NEW_TASK | Intent.FLAG_ACTIVITY_CLEAR_TASK);
                startActivity(intentlogout);
                finish();
            }catch (Exception e)
            {
                Log.e("onLogOff Clear", e.toString());
            }
        }
    }

    private void initToolbar() {
        androidx.appcompat.widget.Toolbar toolbar = findViewById(R.id.toolbar);
        setSupportActionBar(toolbar);

        if (getSupportActionBar() != null) {
            getSupportActionBar().setDisplayHomeAsUpEnabled(true);  // shows the arrow, though your XML already sets it
        }

        // Handle the back arrow click
        toolbar.setNavigationOnClickListener(v -> {
            // This will finish your activity and go back
            finish();
        });
    }

    public void addmedication(View view) throws ParseException {
        if (ValidateData(valid1 == false)) {
            Log.d("validfalse","validfalse");
           Toast.makeText(AddMedication.this, "Please Enter Valid Data", Toast.LENGTH_SHORT).show();
        } else {
            Log.d("validtrue","validtrue");
            addmedic();
        }
    }
    public boolean ValidateData(boolean valid) {

        if (!validateMedname() | !validateSpinner1() |  !validateSpinner2() | !validateCheckboxes() | !validateDates1()  | !validateComments()) {
            return true;
        } else {
            return false;
        }

    }
    private boolean validateSpinner1() {
        Log.d("validateSpinner1","validateSpinner1");
          // An item is selected
            String selectedItem = dropdown1.getSelectedItem().toString();

        if(selectedItem.isEmpty())
        {
            Log.d("111noitemslectd","noitemslectd");
            spinnererror.setVisibility(View.VISIBLE);
            return false;
        }
        else {
            spinnererror.setVisibility(View.INVISIBLE);
            return true;
        }
    }

    private boolean validateSpinner2() {
        Log.d("validateSpinner2","validateSpinner2");
           String selectedItem = dropdown2.getSelectedItem().toString();

        if(selectedItem.isEmpty())
        {
            Log.d("noitemslectd","noitemslectd");
            spinnererror.setVisibility(View.VISIBLE);
            return false;
        }
        else {
            spinnererror.setVisibility(View.INVISIBLE);
            return true;
        }

    }
    private boolean validateMedname() {
        String medName =  medname.getText().toString();

        if (medName.isEmpty()) {
            medname.setError("Medicine name is required");
            return false;
        } else {
            medname.setError(null);
            return true;
        }
    }
    private  boolean validateCheckboxes() {
        StringBuilder selectedTimes = new StringBuilder();

        if (mng.isChecked()) {
            selectedTimes.append("Morning, ");
        }
        if (aftr.isChecked()) {
            selectedTimes.append("Afternoon, ");
        }
        if (eve.isChecked()) {
            selectedTimes.append("Evening, ");
        }
        if (nyt.isChecked()) {
            selectedTimes.append("Night");
        }
        if (selectedTimes.length() > 0) {
            intervalerror.setVisibility(View.INVISIBLE);
            return  true;
        } else {
            intervalerror.setVisibility(View.VISIBLE);
      return false;
        }
    }
    private  boolean validateDates1() {
         SelectedStartDate =  selectedStartDate.getText().toString();
         Log.d("SelectedStartDate",SelectedStartDate);
        if (SelectedStartDate.isEmpty()) {
            dateerror.setVisibility(View.VISIBLE);
            return false;
        } else {
            dateerror.setVisibility(View.INVISIBLE);
            return true;
        }
    }

    private  boolean validateComments() {
        Comments =  comments.getText().toString();

        if (Comments.isEmpty()) {
            comments.setError("Comments are required");
            return false;
        } else {
            comments.setError(null);
            return true;
        }
    }
    private void addmedic() throws ParseException {
        final Loader l1 = new Loader(AddMedication.this);
        l1.show("Please wait...");
        Log.d("addmedic", "addmedic");
        String ADD_MEDICATION_URL = Links.BASE_URL+ Links.ADD_MEDICATION;
        String medName =  medname.getText().toString();
        String sched1 =  dropdown1val;
        String sched2 =  dropdown2val;
        Comments = comments.getText().toString();
        JSONObject postData = new JSONObject();

        try{
            Log.d("medName", medName);
            Log.d("sched1", sched1);
            Log.d("sched2", sched2);
            Log.d("mng", String.valueOf(mng.isChecked() ? 1 : 0));
            Log.d("aftr", String.valueOf(aftr.isChecked() ? 1 : 0));
            Log.d("eve", String.valueOf( eve.isChecked() ? 1 : 0));
            Log.d("nyt", String.valueOf(nyt.isChecked() ? 1 : 0));
            Log.d("SelectedStartDate", SelectedStartDate);
            Log.d("Comments", Comments);
            Log.d("countvalue", String.valueOf(count));
            try {
                SimpleDateFormat sdf = new SimpleDateFormat("yyyy-MM-dd", Locale.US);
                // Parse the selectedStartDate to a Date object
                Date startDate = sdf.parse(SelectedStartDate);
                // Create a Calendar instance and set it to the parsed start date
                Calendar calendar = Calendar.getInstance();
                calendar.setTime(startDate);

                if (dropdown1val.equals("Monthly")) {
                    // If dropdown1val is "Monthly," increment the end date by one month
                    calendar.add(Calendar.MONTH, count-1);
                }
                else if (dropdown1val.equals("Weekly")) {
                    // If dropdown1val is "Weekly," set the end date to 7 days (1 week) ahead
                    if (count > 1) {
                        calendar.add(Calendar.DAY_OF_MONTH, 7 * (count - 1));
                    }
                }
                else if (dropdown1val.equals("Alternative")) {
                    // If dropdown1val is "Monthly," increment the end date by one month
                    calendar.add(Calendar.DAY_OF_MONTH, 2 * (count - 1));
                }
                else if (dropdown1val.equals("Daily")) {
                    // Otherwise, add count days to the start date
                    calendar.add(Calendar.DAY_OF_MONTH, count - 1);
                }
                // Format the modified date back to "yyyy-MM-dd" format
                EndDateValue = sdf.format(calendar.getTime());
                // Now, EndDateValue contains the calculated end date
                Log.d("EndDateValue", EndDateValue);

            } catch (ParseException e) {
                e.printStackTrace();
            }

            Log.d("MedicineSchedule", (sched1 != null ? sched1 : "") + " / " + (sched2 != null ? sched2 : ""));
            postData.put("Medicinename", medName!= null ? medName : "");
            postData.put("MedicineSchedule", (sched1 != null ? sched1 : ""));
            postData.put("BeforeOrAfterMeal", (sched2 != null ? sched2 : ""));
            postData.put("Morning", mng.isChecked() ? 1 : 0);
            postData.put("AfterNoon",  aftr.isChecked() ? 1 : 0);
            postData.put("Evening",   eve.isChecked() ? 1 : 0);
            postData.put("Night",  nyt.isChecked() ? 1 : 0);
            postData.put("StartDate", SelectedStartDate);
            postData.put("EndDate",  EndDateValue);
            postData.put("Description", Comments);
            System.out.println("Formed String is-->"+postData);

        } catch (JSONException e) {
            e.printStackTrace();
        }


        StringRequest stringRequest = new StringRequest(Request.Method.POST, ADD_MEDICATION_URL, new Response.Listener<String>() {
            @Override
            public void onResponse(String response) {
                // Handle the integer response
                try {
                    int intValue = Integer.parseInt(response);
                    Log.d("intValue", String.valueOf(intValue));
                    l1.dismiss();
                                Log.d("Log_url", ADD_MEDICATION_URL);
                                Toast.makeText(AddMedication.this, "Medication has been added!", Toast.LENGTH_SHORT).show();
                    medname.setText("");
                    selectedStartDate.setText("");
                    comments.setText("");
                    dropdown1.setSelection(0); // Assuming the first item is at index 0
                    dropdown2.setSelection(0);

                    // Clear the checked state of the CheckBoxes
                    mng.setChecked(false);
                    aftr.setChecked(false);
                    eve.setChecked(false);
                    nyt.setChecked(false);
                    countValue.setText("1");
                    count = 1;

// When you're done adding medication and want to go back to ClinicalInfoActivity
                    Intent resultIntent = new Intent();
                    setResult(RESULT_OK, resultIntent);
                    finish();

                    // Or perform some calculations or logic based on the value
                    if (intValue > 0) {
                        // Do something if the value is positive
                    } else {
                        // Do something else if the value is non-positive
                    }
                    // The rest of your code goes here
                }
                catch (NumberFormatException e) {
                    // Handle any errors related to parsing the integer value
                    // Display an error message to the user if necessary
                }
            }
                },
                new Response.ErrorListener() {
                    @Override
                    public void onErrorResponse(VolleyError error) {
                        l1.dismiss();
                        Log.e("onErrorResponse", error.toString());
                        Toast.makeText(AddMedication.this, "Something went wrong!", Toast.LENGTH_SHORT).show();
                    }
                }
                )
        {

            @Override
            public String getBodyContentType() {
                return "application/json";
            }

            @Override
            public byte[] getBody() throws AuthFailureError {
                return postData.toString().getBytes();
            }

            @Override
            public Map<String, String> getHeaders() throws AuthFailureError {
                Map<String, String> headers = new HashMap<>();
                headers.put("Bearer",Token);
                Log.d("Token_med", Token);
                return headers;
            }
        };
        //creating a request queue
        RequestQueue requestQueue = Volley.newRequestQueue(AddMedication.this);
        stringRequest.setRetryPolicy(new DefaultRetryPolicy(60000, DefaultRetryPolicy.DEFAULT_MAX_RETRIES, DefaultRetryPolicy.DEFAULT_BACKOFF_MULT));
        requestQueue.add(stringRequest);

    }
}