package com.rpm.clynx.model;

import java.util.ArrayList;

public class NewVitalsModel {
    private String vitalName;
    private ArrayList<NewVitalsItemModel> newVitalsItemModels;

    public NewVitalsModel() {
    }

    public NewVitalsModel(String vitalName, ArrayList<NewVitalsItemModel> newVitalsItemModels) {
        this.vitalName = vitalName;
        this.newVitalsItemModels = newVitalsItemModels;
    }
    public String getVitalName() {
        return vitalName;
    }

    public void setVitalName(String vitalName) {
        this.vitalName = vitalName;
    }

    public ArrayList<NewVitalsItemModel> getNewVitalsItemModels() {
        return newVitalsItemModels;
    }

    public void setNewVitalsItemModels(ArrayList<NewVitalsItemModel> newVitalsItemModels) {
        this.newVitalsItemModels = newVitalsItemModels;
    }
}
