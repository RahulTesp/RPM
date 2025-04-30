package com.rpm.clynx.fragments;

import android.database.Cursor;
import android.os.Bundle;
import android.util.Log;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.TextView;
import androidx.fragment.app.Fragment;
import com.rpm.clynx.utility.DataBaseHelper;
import com.rpm.clynx.R;
import org.json.JSONArray;
import org.json.JSONException;
import org.json.JSONObject;

public class Communicn extends Fragment {
    DataBaseHelper db;
    TextView address1,address2,city,state,timeZone,zipCode,emergencyContact1,
            emergencyContactNumber1,emergencyContact1Relation,emergencyContact2,
            emergencyContactNumber2,emergencyContact2Relation;
    View view;
    @Override
    public View onCreateView(LayoutInflater inflater, ViewGroup container,
                             Bundle savedInstanceState) {
        // Inflate the layout for this fragment
         view = inflater.inflate(R.layout.fragment_communicn, container, false);

        address1 = (TextView) view.findViewById(R.id.frag_Communicn_address_line1);
        address2 = (TextView) view.findViewById(R.id.frag_Communicn_address_line2);
        city = (TextView) view.findViewById(R.id.frag_Communicn_city);
        state = (TextView) view.findViewById(R.id.frag_Communicn_state);
        timeZone = (TextView) view.findViewById(R.id.frag_Communicn_time_zone);
        zipCode = (TextView) view.findViewById(R.id.frag_Communicn_zip_code);
        emergencyContact1 = (TextView) view.findViewById(R.id.frag_Communicn_emergency_contact1);
        emergencyContact1Relation = (TextView) view.findViewById(R.id.frag_Communicn_relation1);
        emergencyContact2 = (TextView) view.findViewById(R.id.frag_Communicn_emergency_contact2);
        emergencyContactNumber1 = (TextView) view.findViewById(R.id.frag_Communicn_emergency_mobile1);
        emergencyContactNumber2 = (TextView) view.findViewById(R.id.frag_Communicn_emergency_mobile2);
        emergencyContact2Relation = (TextView) view.findViewById(R.id.frag_Communicn_relation2);

        db = new DataBaseHelper(getContext());
        getDbData();

        return view;
    }

    private void getDbData() {
        JSONObject object = null;
        StringBuilder PersonalData = new StringBuilder();
        Cursor dbData =  db.getdata();
        Log.d("Cursor_db PersonalData Data",dbData.toString());
        if (dbData != null){
            dbData.moveToFirst();
        }do {
            String dataDB = dbData.getString(0);

            PersonalData.append(dataDB);

        }while (dbData.moveToNext());
        Log.d("Personal Comm Data",PersonalData.toString());
        try {
            JSONArray jsonArry = new JSONArray(PersonalData.toString());
            Log.d("JSONObject",jsonArry.toString());

            for(int n = 0; n < jsonArry.length(); n++)
            {
                object = jsonArry.getJSONObject(n);
                // do some stuff....
            }

            String Address1 = object.getString("Address1");
            address1.setText(Address1);

            String Address2 = object.getString("Address2");
            address2.setText(Address2);

            String City = object.getString("City");
            city.setText(City);

            String State = object.getString("State");
            state.setText(State);

            String TimeZone = object.getString("TimeZone");
            timeZone.setText(TimeZone);

            String ZipCode = object.getString("ZipCode");
            zipCode.setText(ZipCode);

            String EmergencyContact1 = object.getString("EmergencyContact1");
            emergencyContact1.setText(EmergencyContact1);

            String EmergencyContactNumber1 = object.getString("EmergencyContactNumber1");
            emergencyContactNumber1.setText(EmergencyContactNumber1);

            String EmergencyContact1Relation = object.getString("EmergencyContact1Relation");
            emergencyContact1Relation.setText(EmergencyContact1Relation);

            String EmergencyContact2 = object.getString("EmergencyContact2");
            emergencyContact2.setText(EmergencyContact2);

            String EmergencyContactNumber2 = object.getString("EmergencyContactNumber2");
            emergencyContactNumber2.setText(EmergencyContactNumber2);

            String EmergencyContact2Relation = object.getString("EmergencyContact2Relation");
            emergencyContact2Relation.setText(EmergencyContact2Relation);

        } catch (JSONException e) {
            e.printStackTrace();
        }
    }
}