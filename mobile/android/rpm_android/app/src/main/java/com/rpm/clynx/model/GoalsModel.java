package com.rpm.clynx.model;

import android.util.Log;
import java.util.ArrayList;

public class GoalsModel {

    private ArrayList<GoalsItemModel> goalsItemModels;

    public GoalsModel() {
    }

    public GoalsModel(ArrayList<GoalsItemModel> goalsItemModels) {

        this.goalsItemModels = goalsItemModels;
        Log.d("goalsItemModels", goalsItemModels.toString());
    }

    public ArrayList<GoalsItemModel> getGoalsList() {
        return goalsItemModels;
    }

    public void setGoalsList(ArrayList<GoalsItemModel> goalsItemModels) {
        this.goalsItemModels = goalsItemModels;
    }
}
