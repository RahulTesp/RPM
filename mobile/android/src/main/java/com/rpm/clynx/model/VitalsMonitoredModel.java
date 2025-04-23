package com.rpm.clynx.model;

import android.util.Log;
import java.util.ArrayList;

public class VitalsMonitoredModel {

    private ArrayList<VitalsMonitoredItemModel> vitalsMonitoredItemModels;

    public VitalsMonitoredModel() {
    }

    public VitalsMonitoredModel(ArrayList<VitalsMonitoredItemModel> vitalsMonitoredItemModels) {

        this.vitalsMonitoredItemModels = vitalsMonitoredItemModels;
        Log.d("vitalsMonitoredItemModels", vitalsMonitoredItemModels.toString());
    }

    public ArrayList<VitalsMonitoredItemModel> getVitalsMonitoredItemModels() {
        return vitalsMonitoredItemModels;
    }

    public void setVitalsMonitoredItemModels(ArrayList<VitalsMonitoredItemModel> vitalsMonitoredItemModels) {
        this.vitalsMonitoredItemModels = vitalsMonitoredItemModels;
    }
}
