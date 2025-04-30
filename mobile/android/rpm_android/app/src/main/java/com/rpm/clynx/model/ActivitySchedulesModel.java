package com.rpm.clynx.model;

import java.util.ArrayList;

public class ActivitySchedulesModel {
    private String scheduleDate;
    private ArrayList<ActivitySchedulesItemModel> activitySchedulesItemModels;

    public ActivitySchedulesModel() {
    }

    public ActivitySchedulesModel(String scheduleDate, ArrayList<ActivitySchedulesItemModel> activitySchedulesItemModels) {
        this.scheduleDate = scheduleDate;
        this.activitySchedulesItemModels = activitySchedulesItemModels;
    }

    public String getScheduleDate() {
        return scheduleDate;
    }

    public ArrayList<ActivitySchedulesItemModel> getActivitySchedulesItemModels() {
        return activitySchedulesItemModels;
    }
}
