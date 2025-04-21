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
import androidx.annotation.ColorRes;
import androidx.appcompat.app.AppCompatActivity;
import androidx.fragment.app.FragmentPagerAdapter;
import androidx.viewpager.widget.ViewPager;
import com.google.android.material.tabs.TabLayout;
import com.rpm.clynx.adapter.PgmVPadapter;
import com.rpm.clynx.fragments.Login;
import com.rpm.clynx.utility.DataBaseHelper;
import com.rpm.clynx.fragments.DeviceDetailsFragment;
import com.rpm.clynx.fragments.InsuranceDetailsFragment;
import com.rpm.clynx.fragments.ProgramDetailsFragment;
import com.rpm.clynx.fragments.ProgramTimelineFragment;
import com.rpm.clynx.fragments.VitalSchedulesFragment;
import com.rpm.clynx.R;
import com.rpm.clynx.utility.MyApplication;

public class ProgramInfoActivity extends AppCompatActivity {
    DataBaseHelper db;
    SharedPreferences pref;
    SharedPreferences.Editor editor;
    private String ProgramName;
    private TabLayout tabLayout;
    private int[] tabIcons = {
            R.drawable.icons_program,
            R.drawable.icons_devices,
            R.drawable.icons_vitals,
            R.drawable.icons_insurance,
            R.drawable.icons_timeline,
    };

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_pgm_info);
        setSystemBarColor(this, R.color.background2);
        tabLayout = (TabLayout) findViewById(R.id.pgminfo_tablayout);
        ViewPager viewPager = findViewById(R.id.viewPgmPager);
        tabLayout.setupWithViewPager(viewPager);
        db = new DataBaseHelper(this);
        pref = this.getSharedPreferences("RPMUserApp",this.MODE_PRIVATE);
        editor = pref.edit();
        ProgramName = pref.getString("ProgramName", null);
        Log.d("ProgramNameHm", ProgramName);
        initToolbar();

        PgmVPadapter pgmVPadapter = new PgmVPadapter(getSupportFragmentManager(), FragmentPagerAdapter.BEHAVIOR_RESUME_ONLY_CURRENT_FRAGMENT);
        if ("RPM".equals(ProgramName)) {
            pgmVPadapter.adFragment(new ProgramDetailsFragment(), "");
            pgmVPadapter.adFragment(new DeviceDetailsFragment(), "");
            pgmVPadapter.adFragment(new VitalSchedulesFragment(), "");
            pgmVPadapter.adFragment(new InsuranceDetailsFragment(), "");
            pgmVPadapter.adFragment(new ProgramTimelineFragment(), "");
        }
        else
        {
            pgmVPadapter.adFragment(new ProgramDetailsFragment(), "");
            pgmVPadapter.adFragment(new InsuranceDetailsFragment(), "");
            pgmVPadapter.adFragment(new ProgramTimelineFragment(), "");
        }
        viewPager.setAdapter(pgmVPadapter);
        setupTabIcons();
    }

    @Override
    protected void onResume() {
        super.onResume();
        Log.d("actResume","homeonResume");
        if (pref.getBoolean("loginstatus", false) == false){
            Log.d("loginstsfrmhome2", String.valueOf(pref.getBoolean("loginstatus", false)));
            editor.clear();
            editor.commit();
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
            tabLayout.addTab(tabLayout.newTab().setIcon(tabIcons[3])); // Add tab 4
            tabLayout.addTab(tabLayout.newTab().setIcon(tabIcons[4])); // Add tab 5
        } else {
            // Only add tabs 2 and 3 if ProgramName is not "RPM"
            tabLayout.addTab(tabLayout.newTab().setIcon(tabIcons[0])); // Add tab 1
            tabLayout.addTab(tabLayout.newTab().setIcon(tabIcons[3])); // Add tab 4
            tabLayout.addTab(tabLayout.newTab().setIcon(tabIcons[4])); // Add tab 5
        }
    }

    private void initToolbar() {
        System.out.println("pgminfo back");
        androidx.appcompat.widget.Toolbar toolbar = (androidx.appcompat.widget.Toolbar) findViewById(R.id.toolbar);
        setSupportActionBar(toolbar);
        getSupportActionBar();

        // Set the navigation icon
        toolbar.setNavigationIcon(R.drawable.ic_baseline_west_24);

        // Set an OnClickListener for the navigation icon
        toolbar.setNavigationOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                onBackPressed();
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