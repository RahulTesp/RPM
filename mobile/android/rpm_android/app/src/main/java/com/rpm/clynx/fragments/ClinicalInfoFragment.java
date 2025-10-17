package com.rpm.clynx.fragments;

import android.app.Activity;
import android.content.Context;
import android.content.Intent;
import android.content.SharedPreferences;
import android.os.Bundle;
import android.util.Log;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.ImageView;

import androidx.activity.result.ActivityResultLauncher;
import androidx.activity.result.contract.ActivityResultContracts;
import androidx.fragment.app.Fragment;
import androidx.fragment.app.FragmentPagerAdapter;
import androidx.viewpager.widget.ViewPager;

import com.google.android.material.tabs.TabLayout;
import com.rpm.clynx.R;
import com.rpm.clynx.activity.AddMedication;
import com.rpm.clynx.adapter.ClinicalVPadapter;
import com.rpm.clynx.utility.DataBaseHelper;
import com.rpm.clynx.utility.MyApplication;

public class ClinicalInfoFragment extends Fragment {

    DataBaseHelper db;
    SharedPreferences pref;
    SharedPreferences.Editor editor;
    String openedFrom;
    ImageView addM;
    private String ProgramName;
    private TabLayout tabLayout;
    private ViewPager viewPager;
    ClinicalVPadapter clinicalVPadapter;

    private int[] tabIcons = {
            R.drawable.icons_vitals,
            R.drawable.icons_medication,
            R.drawable.icons_symptoms,
    };

    private ActivityResultLauncher<Intent> addMedicationLauncher;

    public ClinicalInfoFragment() {
        // Required empty public constructor
    }

    @Override
    public View onCreateView(LayoutInflater inflater, ViewGroup container,
                             Bundle savedInstanceState) {

        View view = inflater.inflate(R.layout.fragment_clinical_info, container, false);

        addM = view.findViewById(R.id.addmed);

        // Get arguments if passed (for openedFrom)
        if (getArguments() != null) {
            openedFrom = getArguments().getString("opened_from");
        }

        tabLayout = view.findViewById(R.id.clinicalinfo_tablayout);
        viewPager = view.findViewById(R.id.viewClinicalPager);
        tabLayout.setupWithViewPager(viewPager);

        db = new DataBaseHelper(requireContext());
        pref = requireActivity().getSharedPreferences("RPMUserApp", Context.MODE_PRIVATE);
        editor = pref.edit();

        ProgramName = pref.getString("ProgramName", null);

        addMedicationLauncher = registerForActivityResult(
                new ActivityResultContracts.StartActivityForResult(),
                result -> {
                    if (result.getResultCode() == Activity.RESULT_OK) {
                        // Medication was added successfully, refresh your fragment UI
                        refreshMedicationFragment();
                    }
                });


        initToolbar(view);
        setupViewPager();

        if ("CCM".equals(ProgramName)) {
            addM.setVisibility(View.VISIBLE);
        } else {
            addM.setVisibility(View.GONE);
        }


        addM.setOnClickListener(v -> {
            Intent medclinical = new Intent(requireContext(), AddMedication.class);
            addMedicationLauncher.launch(medclinical);
        });


        tabLayout.addOnTabSelectedListener(new TabLayout.OnTabSelectedListener() {
            @Override
            public void onTabSelected(TabLayout.Tab tab) {
                int selectedTabIndex = tab.getPosition();

                if ("RPM".equals(ProgramName)) {
                    addM.setVisibility(selectedTabIndex == 1 ? View.VISIBLE : View.INVISIBLE);
                } else {
                    addM.setVisibility(selectedTabIndex == 0 ? View.VISIBLE : View.INVISIBLE);
                }
            }

            @Override
            public void onTabUnselected(TabLayout.Tab tab) {
                // No action needed
            }

            @Override
            public void onTabReselected(TabLayout.Tab tab) {
                // No action needed
            }
        });

        return view;
    }

    private void setupViewPager() {
        clinicalVPadapter = new ClinicalVPadapter(getChildFragmentManager(),
                FragmentPagerAdapter.BEHAVIOR_RESUME_ONLY_CURRENT_FRAGMENT);

        if ("RPM".equals(ProgramName)) {
            clinicalVPadapter.adFragment(new VitalReadingsFragment(), "VitalReadingsFragmentTag");
            clinicalVPadapter.adFragment(new MedicationFragment(), "medicationFragmentTag");
            clinicalVPadapter.adFragment(new SymptomsFragment(), "SymptomsFragmentTag");
        } else {
            clinicalVPadapter.adFragment(new MedicationFragment(), "medicationFragmentTag");
            clinicalVPadapter.adFragment(new SymptomsFragment(), "SymptomsFragmentTag");
        }
        viewPager.setAdapter(clinicalVPadapter);

        setupTabIcons();
    }

    private void setupTabIcons() {
        tabLayout.removeAllTabs();

        if ("RPM".equals(ProgramName)) {
            tabLayout.addTab(tabLayout.newTab().setIcon(tabIcons[0]));
            tabLayout.addTab(tabLayout.newTab().setIcon(tabIcons[1]));
            tabLayout.addTab(tabLayout.newTab().setIcon(tabIcons[2]));
        } else {
            tabLayout.addTab(tabLayout.newTab().setIcon(tabIcons[1]));
            tabLayout.addTab(tabLayout.newTab().setIcon(tabIcons[2]));
        }
    }

    private void refreshMedicationFragment() {
        if ("RPM".equals(ProgramName)) {
            Fragment fragment = clinicalVPadapter.getItem(1);
            if (fragment instanceof MedicationFragment) {
                ((MedicationFragment) fragment).refreshLayout();
            }
        } else {
            Fragment fragment = clinicalVPadapter.getItem(0);
            if (fragment instanceof MedicationFragment) {
                ((MedicationFragment) fragment).refreshLayout();
            }
        }
    }

    private void initToolbar(View view) {
        androidx.appcompat.widget.Toolbar toolbar = view.findViewById(R.id.toolbar);

        // Set back icon
        toolbar.setNavigationIcon(R.drawable.ic_baseline_west_24);

        toolbar.setNavigationOnClickListener(v -> {
            if ("VITALS".equals(openedFrom) || "MORE".equals(openedFrom)) {
                if (getActivity() != null) {
                    getActivity().getSupportFragmentManager().popBackStack();
                    // Optionally: update bottom nav selected item here if needed
                }
            } else {
                if (getActivity() != null) {
                    getActivity().onBackPressed();
                }
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
