package com.rpm.clynx.model;

import android.util.Log;
import java.util.ArrayList;

public class InsuranceModel {

    private ArrayList<InsuranceItemModel> insuranceItemModels;

    public InsuranceModel() {
    }

    public InsuranceModel(ArrayList<InsuranceItemModel> insuranceItemModels) {

        this.insuranceItemModels = insuranceItemModels;
        Log.d("insuranceItemModels", insuranceItemModels.toString());
    }

    public ArrayList<InsuranceItemModel> getInsuranceItemModels() {
        return insuranceItemModels;
    }

    public void setInsuranceItemModels(ArrayList<InsuranceItemModel> insuranceItemModels) {
        this.insuranceItemModels = insuranceItemModels;
    }
}
