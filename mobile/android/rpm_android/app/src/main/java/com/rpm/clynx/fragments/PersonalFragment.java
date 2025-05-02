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
    private void changepw() {
        Intent more = new Intent(getContext(), ChangePassword.class);
        startActivity(more);
    }
    private void getDbData() {
        JSONObject object = null;
        StringBuilder PersonalData = new StringBuilder();
       Cursor dbData =  db.getdata();
       Log.d("Cursor_dbPersonalFragmentData",dbData.toString());
       if (dbData != null){
           dbData.moveToFirst();
       }do {
           String dataDB = dbData.getString(0);

            PersonalData.append(dataDB);

       }while (dbData.moveToNext());
        Log.d("PersonalFragmentData",PersonalData.toString());
        try {
            JSONArray jsonArry = new JSONArray(PersonalData.toString());
            Log.d("JSONObject",jsonArry.toString());

            for(int n = 0; n < jsonArry.length(); n++)
            {
                object = jsonArry.getJSONObject(n);
                // do some stuff....
            }
            String First_name = object.getString("FirstName");
            first_name.setText(First_name);

            String Middle_name = object.getString("MiddleName");
            middle_name.setText(Middle_name);

            String Last_name = object.getString("LastName");
            last_name.setText(Last_name);

            String DOb = object.getString("DOB");
            SimpleDateFormat inputFormat = new SimpleDateFormat("M/d/yyyy h:mm:ss a", Locale.US);
            SimpleDateFormat outputFormat = new SimpleDateFormat("MMM d, yyyy", Locale.US);

            if (DOb!=null && DOb.length()>0)
            try {
                Date inputDate = inputFormat.parse(DOb);
                String outputDateStr = outputFormat.format(inputDate);
                System.out.println(outputDateStr); // Output: Apr 17, 1951
                Dob.setText(outputDateStr);
            } catch (ParseException e) {
                e.printStackTrace();
            }

            String Gender = object.getString("Gender");
            if(Gender.toUpperCase().equals("M"))
                gender.setText("Male");
            else if (Gender.toUpperCase().equals("F"))
                gender.setText("Female");

            String Height = object.getString("Height");
            height.setText(Height + " Feet");

            String Weight = object.getString("Weight");
            weight.setText(Weight + " Pounds");

            String email = object.getString("Email");
            Email.setText(email);

            String primary_Mobile_no = object.getString("PhoneNo");
            Primary_Mobile_no.setText(primary_Mobile_no);

            String Alternate_Mobile_no = object.getString("AlternateMobNo");
            alternate_Mobile_no.setText(Alternate_Mobile_no);

        } catch (JSONException e) {
            e.printStackTrace();
        }
    }
}