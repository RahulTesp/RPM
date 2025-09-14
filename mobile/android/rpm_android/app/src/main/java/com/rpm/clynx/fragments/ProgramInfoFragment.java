package com.rpm.clynx.fragments;

import android.app.Activity;
import android.content.Context;
import android.content.Intent;
import android.content.SharedPreferences;
import android.os.Build;
import android.os.Bundle;
import android.util.Log;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.view.Window;
import android.view.WindowManager;

import androidx.annotation.ColorRes;
import androidx.fragment.app.Fragment;
import androidx.fragment.app.FragmentPagerAdapter;
import androidx.viewpager.widget.ViewPager;

import com.google.android.material.tabs.TabLayout;
import com.rpm.clynx.R;
import com.rpm.clynx.adapter.PgmVPadapter;
import com.rpm.clynx.fragments.Login;
import com.rpm.clynx.utility.DataBaseHelper;
import com.rpm.clynx.utility.MyApplication;

public class ProgramInfoFragment extends Fragment {
    DataBaseHelper db;
    SharedPreferences pref;
    SharedPreferences.Editor editor;
    private String ProgramName;
    private TabLayout tabLayout;
    private ViewPager viewPager;

    private int[] tabIcons = {
            R.drawable.icons_program,
            R.drawable.icons_devices,
            R.drawable.icons_vitals,
            R.drawable.icons_insurance,
            R.drawable.icons_timeline,
    };

    public ProgramInfoFragment() {
        // Required empty public constructor
    }

    @Override
    public View onCreateView(LayoutInflater inflater, ViewGroup container,
                             Bundle savedInstanceState) {
        // Inflate your layout for the fragment
        View view = inflater.inflate(R.layout.fragment_pgm_info, container, false);

        // Initialize views
        tabLayout = view.findViewById(R.id.pgminfo_tablayout);
        viewPager = view.findViewById(R.id.viewPgmPager);

        db = new DataBaseHelper(requireContext());
        pref = requireActivity().getSharedPreferences("RPMUserApp", Context.MODE_PRIVATE);
        editor = pref.edit();
        ProgramName = pref.getString("ProgramName", null);

        initToolbar(view);
        setupViewPagerAndTabs();

        return view;
    }

    private void setupViewPagerAndTabs() {
        PgmVPadapter pgmVPadapter = new PgmVPadapter(getChildFragmentManager(),
                FragmentPagerAdapter.BEHAVIOR_RESUME_ONLY_CURRENT_FRAGMENT);

        if ("RPM".equals(ProgramName)) {
            pgmVPadapter.adFragment(new ProgramDetailsFragment(), "");
            pgmVPadapter.adFragment(new DeviceDetailsFragment(), "");
            pgmVPadapter.adFragment(new VitalSchedulesFragment(), "");
            pgmVPadapter.adFragment(new InsuranceDetailsFragment(), "");
            pgmVPadapter.adFragment(new ProgramTimelineFragment(), "");
        } else {
            pgmVPadapter.adFragment(new ProgramDetailsFragment(), "");
            pgmVPadapter.adFragment(new InsuranceDetailsFragment(), "");
            pgmVPadapter.adFragment(new ProgramTimelineFragment(), "");
        }

        viewPager.setAdapter(pgmVPadapter);
        tabLayout.setupWithViewPager(viewPager);
        setupTabIcons();
    }

    private void setupTabIcons() {
        tabLayout.removeAllTabs();

        if ("RPM".equals(ProgramName)) {
            tabLayout.addTab(tabLayout.newTab().setIcon(tabIcons[0])); // Program
            tabLayout.addTab(tabLayout.newTab().setIcon(tabIcons[1])); // Devices
            tabLayout.addTab(tabLayout.newTab().setIcon(tabIcons[2])); // Vitals
            tabLayout.addTab(tabLayout.newTab().setIcon(tabIcons[3])); // Insurance
            tabLayout.addTab(tabLayout.newTab().setIcon(tabIcons[4])); // Timeline
        } else {
            tabLayout.addTab(tabLayout.newTab().setIcon(tabIcons[0])); // Program
            tabLayout.addTab(tabLayout.newTab().setIcon(tabIcons[3])); // Insurance
            tabLayout.addTab(tabLayout.newTab().setIcon(tabIcons[4])); // Timeline
        }
    }

    private void initToolbar(View view) {
        androidx.appcompat.widget.Toolbar toolbar = view.findViewById(R.id.toolbar);

        // Set back button icon
        toolbar.setNavigationIcon(R.drawable.ic_baseline_west_24);

        // Back button click listener
        toolbar.setNavigationOnClickListener(v -> {
            // You can either pop back stack here:
            if (getActivity() != null) {
                getActivity().getSupportFragmentManager().popBackStack();
            }

        });
    }

    @Override
    public void onResume() {
        super.onResume();

        if (!pref.getBoolean("loginstatus", false)) {
            editor.clear();
            editor.commit();
            db.deleteData();
            try {
                Intent intentlogout = new Intent(((MyApplication) requireActivity().getApplication()).getLatestActivity(), Login.class);
                intentlogout.addFlags(Intent.FLAG_ACTIVITY_NEW_TASK | Intent.FLAG_ACTIVITY_CLEAR_TASK);
                startActivity(intentlogout);
                requireActivity().finish();
            } catch (Exception e) {
                Log.e("onLogOff Clear", e.toString());
            }
        }
    }

    // Optional: If you want to set system bar color from fragment
    public static void setSystemBarColor(Activity act, @ColorRes int color) {
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.LOLLIPOP) {
            Window window = act.getWindow();
            window.addFlags(WindowManager.LayoutParams.FLAG_DRAWS_SYSTEM_BAR_BACKGROUNDS);
            window.clearFlags(WindowManager.LayoutParams.FLAG_TRANSLUCENT_STATUS);
            window.setStatusBarColor(act.getResources().getColor(color));
        }
    }
}
