package com.rpm.clynx.model;

public class ActivitySchedulesItemModel {
    private String ScheduleType;
    private String ScheduleTime;
    private String AssignedByName;

    public ActivitySchedulesItemModel(String scheduleType, String scheduleTime, String assignedByName) {
        ScheduleType = scheduleType;
        ScheduleTime = scheduleTime;
        AssignedByName = assignedByName;
    }

    public String getScheduleType() {
        return ScheduleType;
    }

    public String getScheduleTime() {
        return ScheduleTime;
    }

    public String getAssignedByName() {
        return AssignedByName;
    }

}
