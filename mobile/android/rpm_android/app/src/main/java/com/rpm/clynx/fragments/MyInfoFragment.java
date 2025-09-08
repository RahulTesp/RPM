package com.rpm.clynx.fragments;

import android.content.Context;
import android.content.Intent;
import android.content.SharedPreferences;
import android.os.Bundle;
import android.util.Log;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;

import androidx.appcompat.widget.Toolbar;


import androidx.fragment.app.Fragment;
import androidx.fragment.app.FragmentPagerAdapter;
import androidx.viewpager.widget.ViewPager;

import com.google.android.material.bottomnavigation.BottomNavigationView;
import com.google.android.material.tabs.TabLayout;
import com.rpm.clynx.R;
import com.rpm.clynx.adapter.MyInfoVPadapter;
import com.rpm.clynx.utility.DataBaseHelper;
import com.rpm.clynx.utility.MyApplication;

public class MyInfoFragment extends Fragment {

    SharedPreferences pref;
    SharedPreferences.Editor editor;
    DataBaseHelper db;

    public MyInfoFragment() {
        // Required empty public constructor
    }

    @Override
    public View onCreateView(LayoutInflater inflater, ViewGroup container,
                             Bundle savedInstanceState) {
        // Inflate your existing activity_my_info.xml layout (you may want to rename it to fragment_my_info.xml)
        View view = inflater.inflate(R.layout.fragment_my_info, container, false);

        // Initialize your views and logic here using 'view.findViewById'

        pref = requireActivity().getSharedPreferences("RPMUserApp", Context.MODE_PRIVATE);
        editor = pref.edit();
        db = new DataBaseHelper(requireContext());

        // Setup toolbar inside fragment
        initToolbar(view);

        // Setup TabLayout and ViewPager
        TabLayout tabLayout = view.findViewById(R.id.myinfo_tablayout);
        ViewPager viewPager = view.findViewById(R.id.viewPager);

        MyInfoVPadapter myInfoVPadapter = new MyInfoVPadapter(getChildFragmentManager(), FragmentPagerAdapter.BEHAVIOR_RESUME_ONLY_CURRENT_FRAGMENT);
        myInfoVPadapter.adFragment(new PersonalFragment(), "Personal");
        myInfoVPadapter.adFragment(new Communicn(), "Communication");
        myInfoVPadapter.adFragment(new Preference(), "Preference");

        viewPager.setAdapter(myInfoVPadapter);
        tabLayout.setupWithViewPager(viewPager);

        return view;
    }

    private void initToolbar(View view) {
        Toolbar toolbar = view.findViewById(R.id.toolbar);
        toolbar.setNavigationIcon(R.drawable.ic_baseline_west_24);
        toolbar.setNavigationOnClickListener(v -> {
            BottomNavigationView bottomNav = requireActivity().findViewById(R.id.navigation);
            if (bottomNav != null) {
                bottomNav.setSelectedItemId(R.id.More);  // Switch to More tab
            }
        });
    }


    @Override
    public void onResume() {
        super.onResume();

        if (!pref.getBoolean("loginstatus", false)) {
            editor.clear();
            editor.commit();
            db.deleteProfileData("myprofileandprogram");
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
}
