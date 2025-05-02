package com.rpm.clynx.adapter;

import androidx.annotation.NonNull;
import androidx.annotation.Nullable;
import androidx.fragment.app.Fragment;
import androidx.fragment.app.FragmentManager;
import androidx.fragment.app.FragmentPagerAdapter;
import java.util.ArrayList;

public class ClinicalVPadapter extends FragmentPagerAdapter {

    private  final ArrayList<Fragment> fragmentArrayList = new ArrayList<>();
    private  final ArrayList<String> fragmentTitelList = new ArrayList<>();
    private int icon;

    public ClinicalVPadapter(@NonNull FragmentManager fm, int behavior) {
        super(fm, behavior);
    }

    @NonNull
    @Override
    public Fragment getItem(int position) {

        return fragmentArrayList.get(position);
    }

    @Override
    public int getCount() {
        return fragmentArrayList.size();
    }

    public  void adFragment(Fragment fragment, String title){

        fragmentArrayList.add(fragment);
        fragmentTitelList.add(title);
    }

    @Nullable
    @Override
    public CharSequence getPageTitle(int position) {
        // Generate title based on item position
        return fragmentTitelList.get(position);
    }
}
