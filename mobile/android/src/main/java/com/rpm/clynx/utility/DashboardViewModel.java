package com.rpm.clynx.utility;

import android.util.Log;
import androidx.lifecycle.MutableLiveData;
import androidx.lifecycle.ViewModel;

public class DashboardViewModel extends ViewModel {
    private MutableLiveData<Long> messageCountLiveData = new MutableLiveData<>();
    private static DashboardViewModel instance;

    public static DashboardViewModel getInstance() {
        if (instance == null) {
            instance = new DashboardViewModel();
        }
        Log.d("dashinstance", String.valueOf(instance));
        return instance;
    }
}



