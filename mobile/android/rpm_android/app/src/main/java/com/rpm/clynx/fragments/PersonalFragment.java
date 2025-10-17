package com.rpm.clynx.fragments;

import android.content.Intent;
import android.database.Cursor;
import android.icu.text.SimpleDateFormat;
import android.os.Bundle;
import android.util.Log;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.Button;
import android.widget.ImageView;
import android.widget.TextView;
import androidx.fragment.app.Fragment;
import com.rpm.clynx.activity.ChangePassword;
import com.rpm.clynx.home.Home;
import com.rpm.clynx.utility.DataBaseHelper;
import com.rpm.clynx.R;
import org.json.JSONArray;
import org.json.JSONException;
import org.json.JSONObject;
import java.text.ParseException;
import java.util.Date;
import java.util.Locale;

public class PersonalFragment extends Fragment {

    DataBaseHelper db;
    ImageView imageView;
    Button Logout_btn,changepw;
    TextView first_name,middle_name,last_name,Dob,gender,height,weight,Email,Primary_Mobile_no,alternate_Mobile_no;
    View view;
    @Override
    public View onCreateView(LayoutInflater inflater, ViewGroup container,
                             Bundle savedInstanceState) {
        // Inflate the layout for this fragment
        view = inflater.inflate(R.layout.fragment_personal, container, false);
        imageView = (ImageView) view.findViewById(R.id.frag_personal_img);
        Logout_btn = (Button) view.findViewById(R.id.frag_personal_logout_btn);
        first_name = (TextView) view.findViewById(R.id.frag_personal_first_name);
        middle_name = (TextView) view.findViewById(R.id.frag_personal_Middle_name);
        last_name = (TextView) view.findViewById(R.id.frag_personal_last_name);
        changepw = (Button) view.findViewById(R.id.frag_personal_changepassword);
        Dob = (TextView) view.findViewById(R.id.frag_personal_Dob);
        gender = (TextView) view.findViewById(R.id.frag_personal_gender);
        height = (TextView) view.findViewById(R.id.frag_personal_height);
        weight = (TextView) view.findViewById(R.id.frag_personal_weight);
        Email = (TextView) view.findViewById(R.id.frag_personal_Email);
        Primary_Mobile_no = (TextView) view.findViewById(R.id.frag_personal_primery_mobile_no);
        alternate_Mobile_no = (TextView) view.findViewById(R.id.frag_personal_Alternate_Mobile_no);
        db = new DataBaseHelper(getContext());

        getDbData();

        changepw.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View view) {
                changepw();
            }
        });

        return  view;
    }

    @Override
    public void onResume() {
        super.onResume();
        Log.d("getDbData","ONRESUME");
        Home activity = (Home) getActivity();
        if (activity != null) {
            // Fetch profile and update UI after DB is updated
            activity.getpatientprofile(() -> {
                if (isAdded()) { // make sure fragment is attached
                    getActivity().runOnUiThread(this::getDbData);
                }
            });
        }
    }
    private void changepw() {
        Intent more = new Intent(getContext(), ChangePassword.class);
        startActivity(more);
    }
    private void getDbData() {
       // JSONObject object = null;
       // StringBuilder PersonalData = new StringBuilder();

        Cursor dbData = db.getdata();
        Log.d("Cursor_dbPersonalFragmentData", dbData.toString());

        if (dbData != null) {
            if (dbData.moveToFirst()) {  // Only proceed if there is at least one row
                StringBuilder PersonalData = new StringBuilder();
                do {
                    String dataDB = dbData.getString(0);
                    PersonalData.append(dataDB);
                } while (dbData.moveToNext());

                Log.d("PersonalFragmentData", PersonalData.toString());

                try {
                    JSONArray jsonArry = new JSONArray(PersonalData.toString());
                    JSONObject object = jsonArry.getJSONObject(0);  // Assuming first object is needed

                    first_name.setText(object.optString("FirstName", ""));
                    middle_name.setText(object.optString("MiddleName", ""));
                    last_name.setText(object.optString("LastName", ""));

                    String DOb = object.optString("DOB", "");
                    if (!DOb.isEmpty()) {
                        SimpleDateFormat inputFormat = new SimpleDateFormat("M/d/yyyy h:mm:ss a", Locale.US);
                        SimpleDateFormat outputFormat = new SimpleDateFormat("MMM d, yyyy", Locale.US);
                        try {
                            Date inputDate = inputFormat.parse(DOb);
                            Dob.setText(outputFormat.format(inputDate));
                        } catch (ParseException e) {
                            e.printStackTrace();
                        }
                    }

                    String Gender = object.optString("Gender", "");
                    if (Gender.equalsIgnoreCase("M")) gender.setText("Male");
                    else if (Gender.equalsIgnoreCase("F")) gender.setText("Female");

                    height.setText(object.optString("Height", "") + " Feet");
                    weight.setText(object.optString("Weight", "") + " Pounds");
                    Email.setText(object.optString("Email", ""));
                    Primary_Mobile_no.setText(object.optString("PhoneNo", ""));
                    alternate_Mobile_no.setText(object.optString("AlternateMobNo", ""));

                } catch (JSONException e) {
                    e.printStackTrace();
                }
            } else {
                Log.d("PersonalFragment", "No data found in DB");
            }
            dbData.close(); // Always close cursor
        } else {
            Log.d("PersonalFragment", "Cursor is null");
        }
    }
}