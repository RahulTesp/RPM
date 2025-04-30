package com.rpm.clynx.model;

import android.util.Log;
import java.util.ArrayList;

public class SymptomModel {

    private ArrayList<SymptomItemModel> symptomItemModels;

    public SymptomModel() {
    }

    public SymptomModel(ArrayList<SymptomItemModel> symptomItemModels) {

        this.symptomItemModels = symptomItemModels;
        Log.d("symptomItemModels", symptomItemModels.toString());
    }

    public ArrayList<SymptomItemModel> getSymptomItemModels() {
        return symptomItemModels;
    }

    public void setSymptomItemModels(ArrayList<SymptomItemModel> symptomItemModels) {
        this.symptomItemModels = symptomItemModels;
    }
}
