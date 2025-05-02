package com.rpm.clynx.model;

import android.util.Log;

public class VitalSchedulesItemModel {
    private String NormalMinimum;
    private String NormalMaximum;
    private String MeasureName;
    private String UnitName;

    public VitalSchedulesItemModel (String normalMinimum, String normalMaximum
           , String measureName, String unitName
    ) {
        NormalMinimum = normalMinimum;
        NormalMaximum = normalMaximum;
        MeasureName = measureName;
        UnitName = unitName;
        Log.d("NormalMinimum--l", String.valueOf(NormalMinimum));
        Log.d("NormalMaximum--", String.valueOf(NormalMaximum));
    }

    public VitalSchedulesItemModel() {
    }

    public String getNormalMinimum() {
        return NormalMinimum;
    }

    public void setNormalMinimum(String normalMinimum) {
        NormalMinimum = normalMinimum;
    }

    public String getNormalMaximum() {
        return NormalMaximum;
    }

    public void setNormalMaximum(String normalMaximum) {
        NormalMaximum = normalMaximum;
    }

    public String getMeasureName() {
        return MeasureName;
    }

    public void setMeasureName(String measureName) {
        MeasureName = measureName;
    }
    public String getUnitName() {
        return UnitName;
    }

    public void setUnitName(String unitName) {
        UnitName = unitName;
    }
}
