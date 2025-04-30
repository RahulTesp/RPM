package com.rpm.clynx.model;

import android.util.Log;
import java.util.ArrayList;

public class MedicationModel {

    private ArrayList<MedicationItemModel> medicationItemModels;

    public MedicationModel() {
    }

    public MedicationModel(ArrayList<MedicationItemModel> medicationItemModels) {

        this.medicationItemModels = medicationItemModels;
        Log.d("medicationItemModels", medicationItemModels.toString());
    }

    public ArrayList<MedicationItemModel> getMedicationItemModels() {
        return medicationItemModels;
    }

    public void setMedicationItemModels(ArrayList<MedicationItemModel> medicationItemModels) {
        this.medicationItemModels = medicationItemModels;
    }
}
