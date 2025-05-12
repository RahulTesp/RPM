package com.rpm.clynx.model;
import android.util.Log;

public class DiagnosisItemModel {
    private String DiagnosisName;
    private String DiagnosisCode;

    public DiagnosisItemModel(String diagnosisName, String diagnosisCode) {
        DiagnosisName = diagnosisName;
        DiagnosisCode = diagnosisCode;
        Log.d("DiagnosisName--l", DiagnosisName.toString());
        Log.d("DiagnosisCode--", DiagnosisCode.toString());
    }

    public DiagnosisItemModel() {
    }

    public String getDiagnosisName() {
        return DiagnosisName;
    }

    public void setDiagnosisName(String diagnosisName) {
        DiagnosisName = diagnosisName;
    }

    public String getDiagnosisCode() {
        return DiagnosisCode;
    }

    public void setDiagnosisCode(String diagnosisCode) {
        DiagnosisCode = diagnosisCode;
    }
}
