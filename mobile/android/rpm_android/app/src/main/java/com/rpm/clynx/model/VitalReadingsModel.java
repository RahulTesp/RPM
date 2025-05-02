package com.rpm.clynx.model;

import android.util.Log;
import java.util.ArrayList;

public class VitalReadingsModel {
    private String vitalType;
    private ArrayList<VitalReadingsItemModel> vitalReadingsItemModels;

    public VitalReadingsModel() {
    }

    public VitalReadingsModel(String vitalType,ArrayList<VitalReadingsItemModel> vitalReadingsItemModels) {
        this.vitalType = vitalType;
        this.vitalReadingsItemModels = vitalReadingsItemModels;
        Log.d("vitalReadingsItemModels", vitalReadingsItemModels.toString());
    }

    public String getVitalType() {
        return vitalType;
    }

    public void setVitalType(String vitalType) {
        this.vitalType = vitalType;
    }
    public ArrayList<VitalReadingsItemModel> getVitalReadingsItemModels() {
        return vitalReadingsItemModels;
    }

    public void setVitalReadingsItemModels(ArrayList<VitalReadingsItemModel> goalsItemModels) {
        this.vitalReadingsItemModels = vitalReadingsItemModels;
    }
}
