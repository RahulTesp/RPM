package com.rpm.clynx.model;

public class ToDoListModel {
    private String ScheduleType;
    private String Time;
    private String Decription;
    private String Interval;

    public ToDoListModel(String scheduleType, String time, String decription, String interval) {
        ScheduleType = scheduleType;
        Time = time;
        Decription = decription;
        Interval = interval;
    }

    public ToDoListModel() {

    }

    public String getScheduleType() {
        return ScheduleType;
    }

    public void setScheduleType(String scheduleType) {
        ScheduleType = scheduleType;
    }

    public String getTime() {
        return Time;
    }

    public void setTime(String time) {
        Time = time;
    }

    public String getDecription() {
        return Decription;
    }

    public void setDecription(String decription) {
        Decription = decription;
    }

    public String getInterval() {
        return Interval;
    }

    public void setInterval(String interval) {
        Interval = interval;
    }
}
