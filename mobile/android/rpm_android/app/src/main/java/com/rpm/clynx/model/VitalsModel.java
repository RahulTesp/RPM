package com.rpm.clynx.model;

import java.util.ArrayList;

public class VitalsModel {
    private String VitalName;
    private ArrayList<VitalItemsModel> vitalItemsModelList;

    public VitalsModel(String vitalName, String vitalValue, String vitalUnit, String vitalDate, String vitalTime) {
        VitalName = vitalName;
    }

    public VitalsModel() {
    }

    public VitalsModel(String vitalName, ArrayList<VitalItemsModel> vitalItemsModelList) {
        VitalName = vitalName;
        this.vitalItemsModelList = vitalItemsModelList;
    }

    public String getVitalName() {
        return VitalName;
    }

    public void setVitalName(String vitalName) {
        VitalName = vitalName;
    }

    public ArrayList<VitalItemsModel> getVitalItemsModelList() {
        return vitalItemsModelList;
    }

    public void setVitalItemsModelList(ArrayList<VitalItemsModel> vitalItemsModelList) {
        this.vitalItemsModelList = vitalItemsModelList;
    }
}
