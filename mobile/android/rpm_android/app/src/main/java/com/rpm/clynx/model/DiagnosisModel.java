package com.rpm.clynx.model;

import android.util.Log;
import java.util.ArrayList;

public class DiagnosisModel {

    private ArrayList<DiagnosisItemModel> diagnosisItemModels;

    public DiagnosisModel() {
    }

    public DiagnosisModel(ArrayList<DiagnosisItemModel> diagnosisItemModels) {

        this.diagnosisItemModels = diagnosisItemModels;
        Log.d("diagnosisItemModels", diagnosisItemModels.toString());
    }

    public ArrayList<DiagnosisItemModel> getDiagnosisItemModels() {
        return diagnosisItemModels;
    }

    public void setDiagnosisItemModels(ArrayList<DiagnosisItemModel> goalsItemModels) {
        this.diagnosisItemModels = diagnosisItemModels;
    }
}
