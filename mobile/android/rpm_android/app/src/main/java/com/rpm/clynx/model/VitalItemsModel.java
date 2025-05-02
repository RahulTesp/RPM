package com.rpm.clynx.model;

public class VitalItemsModel {

    private String Value;
    private String unit;
    private String time;
    private String MeasureName;
    private String Date;

    public VitalItemsModel() {
    }

    public VitalItemsModel(String value, String unit, String time, String measureName, String date) {
        Value = value;
        this.unit = unit;
        this.time = time;
        MeasureName = measureName;
        Date = date;
    }

    public String getValue() {
        return Value;
    }

    public void setValue(String value) {
        Value = value;
    }

    public String getUnit() {
        return unit;
    }

    public void setUnit(String unit) {
        this.unit = unit;
    }

    public String getTime() {
        return time;
    }

    public void setTime(String time) {
        this.time = time;
    }

    public String getMeasureName() {
        return MeasureName;
    }

    public void setMeasureName(String measureName) {
        MeasureName = measureName;
    }

    public String getDate() {
        return Date;
    }

    public void setDate(String date) {
        Date = date;
    }
}
