package com.rpm.clynx.model;

import android.util.Log;

public class VitalsMonitoredItemModel {
    private String VitalsMonitoredName;

    public VitalsMonitoredItemModel(String vitalsMonitoredName) {
        VitalsMonitoredName = vitalsMonitoredName;
        Log.d("VitalsMonitoredName--l", VitalsMonitoredName.toString());
    }

    public VitalsMonitoredItemModel() {
    }

    public String getVitalsMonitoredName() {
        return VitalsMonitoredName;
    }

    public void setVitalsMonitoredName(String vitalsMonitoredName) {
        VitalsMonitoredName = vitalsMonitoredName;
    }
}
