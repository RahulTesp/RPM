package com.rpm.clynx.model;

public class NewVitalsItemModel {
    private String Value;
    private String Unit;
    private String Schedule;
    private String Time;

    public NewVitalsItemModel(String value, String unit,String schedule, String time) {
        Value = value;
        Unit = unit;
        Time = time;
        Schedule = schedule;
    }

    public NewVitalsItemModel() {
    }

    public String getValue() {
        return Value;
    }

    public void setValue(String value) {
        Value = value;
    }

    public String getUnit() {
        return Unit;
    }

    public void setUnit(String unit) {
        Unit = unit;
    }
    public String getSchedule() {
        return Schedule;
    }

    public void setSchedule(String schedule) {
        Schedule = schedule;
    }
    public String getTime() {
        return Time;
    }

    public void setTime(String time) {
        Time = time;
    }
}
