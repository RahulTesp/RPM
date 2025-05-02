package com.rpm.clynx.activity;

import android.app.Activity;
import android.content.Intent;
import android.content.SharedPreferences;
import android.os.Build;
import android.os.Bundle;
import android.util.Log;
import android.view.View;
import android.view.Window;
import android.view.WindowManager;
import android.widget.ImageView;
import androidx.annotation.ColorRes;
import androidx.annotation.RequiresApi;
import androidx.appcompat.app.AppCompatActivity;
import androidx.fragment.app.Fragment;
import androidx.fragment.app.FragmentPagerAdapter;
import androidx.viewpager.widget.ViewPager;
import com.google.android.material.tabs.TabLayout;
import com.rpm.clynx.adapter.ClinicalVPadapter;
import com.rpm.clynx.fragments.Login;
import com.rpm.clynx.utility.DataBaseHelper;
import com.rpm.clynx.fragments.MedicationFragment;
import com.rpm.clynx.fragments.SymptomsFragment;
import com.rpm.clynx.fragments.VitalReadingsFragment;
import com.rpm.clynx.R;
import com.rpm.clynx.utility.MyApplication;

public class ClinicalInfoActivity extends AppCompatActivity {
    DataBaseHelper db;
    SharedPreferences pref;
    SharedPreferences.Editor editor;
    String  openedFrom;
    ImageView addM;
    private String ProgramName;
    private TabLayout tabLayout;
    private static final int ADD_MEDICATION_REQUEST_CODE = 1001;
    ViewPager viewPager;
    ClinicalVPadapter clinicalVPadapter;
    private int[] tabIcons = {
            R.drawable.icons_vitals,
            R.drawable.icons_medication,
            R.drawable.icons_symptoms,
    };

    @RequiresApi(api = Build.VERSION_CODES.O)
    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_clinical_info);
        setSystemBarColor(this, R.color.background2);
        addM = (ImageView) findViewById(R.id.addmed);

        addM.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View view) {
                Intent medclinical = new Intent(ClinicalInfoActivity.this, AddMedication.class);
                startActivityForResult(medclinical, ADD_MEDICATION_REQUEST_CODE);
            }
        });

         openedFrom = getIntent().getStringExtra("opened_from");
         tabLayout = (TabLayout) findViewById(R.id.clinicalinfo_tablayout);
         viewPager = findViewById(R.id.viewClinicalPager);
         tabLayout.setupWithViewPager(viewPager);
        db = new DataBaseHelper(this);
        pref = getApplicationContext().getSharedPreferences("RPMUserApp", MODE_PRIVATE);
        editor = pref.edit();
        initToolbar();
        ProgramName = pref.getString("ProgramName", null);
        Log.d("ProgramNamecLI", ProgramName);
        clinicalVPadapter = new ClinicalVPadapter(getSupportFragmentManager(), FragmentPagerAdapter.BEHAVIOR_RESUME_ONLY_CURRENT_FRAGMENT);

        if ("RPM".equals(ProgramName)) {
            clinicalVPadapter.adFragment(new VitalReadingsFragment(), "VitalReadingsFragmentTag");
            clinicalVPadapter.adFragment(new MedicationFragment(), "medicationFragmentTag");
            clinicalVPadapter.adFragment(new SymptomsFragment(), "SymptomsFragmentTag");
            viewPager.setAdapter(clinicalVPadapter);
        }
        else
        {
            clinicalVPadapter.adFragment(new MedicationFragment(), "medicationFragmentTag");
            clinicalVPadapter.adFragment(new SymptomsFragment(), "SymptomsFragmentTag");
            viewPager.setAdapter(clinicalVPadapter);
        }

        setupTabIcons();
        initToolbar();

        if ("CCM".equals(ProgramName)) {
            addM.setVisibility(View.VISIBLE);
        }

        tabLayout.addOnTabSelectedListener(new TabLayout.OnTabSelectedListener() {
            @Override
            public void onTabSelected(TabLayout.Tab tab) {

                // Get the position of the selected tab
                int selectedTabIndex = tab.getPosition();

                // Check if the selected tab is the second tab (index 1)
                if ("RPM".equals(ProgramName)) {
                    if (selectedTabIndex == 1) {
                        Log.d("tab2Q", String.valueOf(selectedTabIndex));
                        addM.setVisibility(View.VISIBLE);
                    } else {
                        Log.d("tab2W", String.valueOf(selectedTabIndex));
                        addM.setVisibility(View.INVISIBLE);
                    }
                }
                else
                {
                    if (selectedTabIndex == 0) {
                        Log.d("tab2Q", String.valueOf(selectedTabIndex));
                        addM.setVisibility(View.VISIBLE);
                    } else {
                        Log.d("tab2W", String.valueOf(selectedTabIndex));
                        addM.setVisibility(View.INVISIBLE);
                    }
                }
            }

            @Override
            public void onTabUnselected(TabLayout.Tab tab) {
                // Not needed for this scenario, but you can add logic if required.
            }

            @Override
            public void onTabReselected(TabLayout.Tab tab) {
                // Not needed for this scenario, but you can add logic if required.
            }
        });
    }

    @Override
    protected void onActivityResult(int requestCode, int resultCode, Intent data) {
        super.onActivityResult(requestCode, resultCode, data);

        if (requestCode == ADD_MEDICATION_REQUEST_CODE) {
            if (resultCode == RESULT_OK) {
                // The AddMedication activity has finished successfully.
                // Refresh the Medication Fragment (Tab 2).
                refreshMedicationFragment();
            }
        }
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

    private void setupTabIcons() {
        // Clear all tabs first
        tabLayout.removeAllTabs();

        // Check the condition and add tabs accordingly
        if ("RPM".equals(ProgramName)) {
            tabLayout.addTab(tabLayout.newTab().setIcon(tabIcons[0])); // Add tab 1
            tabLayout.addTab(tabLayout.newTab().setIcon(tabIcons[1])); // Add tab 2
            tabLayout.addTab(tabLayout.newTab().setIcon(tabIcons[2])); // Add tab 3
        } else {
            // Only add tabs 2 and 3 if ProgramName is not "RPM"
            tabLayout.addTab(tabLayout.newTab().setIcon(tabIcons[1])); // Add tab 2
            tabLayout.addTab(tabLayout.newTab().setIcon(tabIcons[2])); // Add tab 3
        }
    }

    private void refreshMedicationFragment() {
        Log.d("refreshMedicationFragment","refreshMedicationFragment");
        // Check if the fragment is of type MedicationFragment

        if ("RPM".equals(ProgramName)) {
            // Get the fragment at the specified position (e.g., 1 for the second fragment)
            Fragment fragment = clinicalVPadapter.getItem(1);
            Log.d("CURRENTfragment", String.valueOf(fragment));
            if (fragment instanceof MedicationFragment) {
                MedicationFragment medicationFragment = (MedicationFragment) fragment;
                // Now, you can call methods or perform actions on the MedicationFragment
                medicationFragment.refreshLayout();
            }
            }
        else
        {
            // Get the fragment at the specified position (e.g., 1 for the second fragment)
            Fragment fragment1 = clinicalVPadapter.getItem(0);
            Log.d("CURRENTfragment1", String.valueOf(fragment1));
            if (fragment1 instanceof MedicationFragment) {
                MedicationFragment medicationFragment1 = (MedicationFragment) fragment1;
                // Now, you can call methods or perform actions on the MedicationFragment
                medicationFragment1.refreshLayout();
            }
        }
    }

    private void initToolbar() {
        androidx.appcompat.widget.Toolbar toolbar = (androidx.appcompat.widget.Toolbar) findViewById(R.id.toolbar);
        setSupportActionBar(toolbar);
        getSupportActionBar();

        // Set the navigation icon
        toolbar.setNavigationIcon(R.drawable.ic_baseline_west_24);

// Set an OnClickListener for the navigation icon
        toolbar.setNavigationOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                if ("VITALS".equals(openedFrom)) {
                    // Navigate back to VITALS activity
                    onBackPressed();
                } else if ("MORE".equals(openedFrom)) {
                    // Navigate back to More activity
                    onBackPressed();
                }
                else
                {
                    finish();
                }
            }
        });
    }

    public static void setSystemBarColor(Activity act, @ColorRes int color) {
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.LOLLIPOP) {
            Window window = act.getWindow();
            window.addFlags(WindowManager.LayoutParams.FLAG_DRAWS_SYSTEM_BAR_BACKGROUNDS);
            window.clearFlags(WindowManager.LayoutParams.FLAG_TRANSLUCENT_STATUS);
            window.setStatusBarColor(act.getResources().getColor(color));
        }
    }
}