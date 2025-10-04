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

import com.rpm.clynx.home.Home;
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

    private void getDbData() {
        Cursor dbData = db.getdata();

        if (dbData == null || dbData.getCount() == 0) {
            Log.d("getDbData", "No data in DB");
            return;
        }

        StringBuilder PersonalData = new StringBuilder();
        if (dbData.moveToFirst()) {
            do {
                PersonalData.append(dbData.getString(0));
            } while (dbData.moveToNext());
        }

        Log.d("PersonalData", PersonalData.toString());

        try {
            JSONArray jsonArry = new JSONArray(PersonalData.toString());
            JSONObject object = jsonArry.getJSONObject(jsonArry.length() - 1); // get latest entry

            // safely update UI
            callTime.setText(object.optString("CallTime", ""));
            language.setText(object.optString("Language", ""));
            preference1.setText(object.optString("Preference1", ""));
            preference2.setText(object.optString("Preference2", ""));
            preference3.setText(object.optString("Preference3", ""));
            notes.setText(object.optString("Notes", ""));

        } catch (JSONException e) {
            e.printStackTrace();
        }
    }
}