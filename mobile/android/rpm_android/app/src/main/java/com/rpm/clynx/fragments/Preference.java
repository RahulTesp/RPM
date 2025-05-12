package com.rpm.clynx.fragments;

import android.content.SharedPreferences;
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

public class Preference extends Fragment {
    DataBaseHelper db;
    TextView callTime,language,preference1,preference2,preference3,notes;
    View view;
    @Override
    public View onCreateView(LayoutInflater inflater, ViewGroup container,
                             Bundle savedInstanceState) {
        // Inflate the layout for this fragment
         view=inflater.inflate(R.layout.fragment_preference, container, false);

        callTime = (TextView) view.findViewById(R.id.frag_Preference_time);
        language = (TextView) view.findViewById(R.id.frag_Preference_language);
        preference1 = (TextView) view.findViewById(R.id.frag_Preference_preference1);
        preference2 = (TextView) view.findViewById(R.id.frag_Preference_preference2);
        preference3 = (TextView) view.findViewById(R.id.frag_Preference_preference3);
        notes = (TextView) view.findViewById(R.id.frag_Preference_additional_note);
        db = new DataBaseHelper(getContext());

        getDbData();

        return view;
    }

    private void getDbData() {
        JSONObject object = null;
        StringBuilder PersonalData = new StringBuilder();
        Cursor dbData =  db.getdata();
        Log.d("Cursor_dbData",dbData.toString());
        if (dbData != null){
            dbData.moveToFirst();
        }do {
            String dataDB = dbData.getString(0);
            PersonalData.append(dataDB);
        }while (dbData.moveToNext());
        Log.d("PersonalData",PersonalData.toString());
        try {
            JSONArray jsonArry = new JSONArray(PersonalData.toString());
            Log.d("JSONObject",jsonArry.toString());

            for(int n = 0; n < jsonArry.length(); n++)
            {
                object = jsonArry.getJSONObject(n);
                // do some stuff....
            }

            String CallTime = object.getString("CallTime");
            callTime.setText(CallTime);

            String Language = object.getString("Language");
            language.setText(Language);

            String Preference1 = object.getString("Preference1");
            preference1.setText(Preference1);

            String Preference2 = object.getString("Preference2");
            preference2.setText(Preference2);

            String Preference3 = object.getString("Preference3");
            preference3.setText(Preference3);

            String Notes = object.getString("Notes");
            notes.setText(Notes);

        } catch (JSONException e) {
            e.printStackTrace();
        }
    }
}