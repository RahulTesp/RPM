package com.rpm.clynx.model;

import android.util.Log;

public class GoalsItemModel {
    private String Goal;
    private String Description;

    public GoalsItemModel(String goal, String description) {
        Goal = goal;
        Description = description;
        Log.d("Goa--l", Goal.toString());
        Log.d("Description--", Description.toString());
    }

    public GoalsItemModel() {
    }

    public String getGoal() {
        return Goal;
    }

    public void setGoal(String goal) {
        Goal = goal;
    }

    public String getDescription() {
        return Description;
    }

    public void setDescription(String description) {
        Description = description;
    }
}
