package com.rpm.clynx.activity;

import android.app.Activity;
import android.content.Intent;
import com.rpm.clynx.adapter.MyInfoVPadapter;
import com.rpm.clynx.R;
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
import com.rpm.clynx.fragments.Communicn;
import com.rpm.clynx.fragments.Login;
import com.rpm.clynx.fragments.PersonalFragment;
import com.rpm.clynx.fragments.Preference;
import com.rpm.clynx.utility.DataBaseHelper;
import com.rpm.clynx.utility.MyApplication;

public class MyInfoActivity extends AppCompatActivity {
    SharedPreferences pref;
    SharedPreferences.Editor editor;
    DataBaseHelper db;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);

        setContentView(R.layout.activity_my_info);
        setSystemBarColor(this, R.color.background2);
        TabLayout tabLayout = findViewById(R.id.myinfo_tablayout);
        ViewPager viewPager = findViewById(R.id.viewPager);
        tabLayout.setupWithViewPager(viewPager);
        db = new DataBaseHelper(this);
        pref = this.getSharedPreferences("RPMUserApp",this.MODE_PRIVATE);
        editor = pref.edit();
        initToolbar();
        MyInfoVPadapter myInfoVPadapter = new MyInfoVPadapter(getSupportFragmentManager(), FragmentPagerAdapter.BEHAVIOR_RESUME_ONLY_CURRENT_FRAGMENT);
        myInfoVPadapter.adFragment(new PersonalFragment(), "Personal");
        myInfoVPadapter.adFragment(new Communicn(), "Communication");
        myInfoVPadapter.adFragment(new Preference(), "Preference");
        viewPager.setAdapter(myInfoVPadapter);
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