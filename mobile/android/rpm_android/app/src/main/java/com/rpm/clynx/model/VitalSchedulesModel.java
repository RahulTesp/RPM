package com.rpm.clynx.model;

import android.util.Log;
import java.util.ArrayList;

public class VitalSchedulesModel {
    private String VitalName;
    private String ScheduleName;
    private String VitalScheduleName;
    private CharSequence Morning;
    private CharSequence Afternoon;
    private CharSequence Evening;
    private CharSequence Night;
    private  String SchedVal;
    private ArrayList<VitalSchedulesItemModel> vitalSchedulesItemModel;

    public VitalSchedulesModel() {
    }

    public VitalSchedulesModel(String ischedVal,String vitalName, String scheduleName, String vitalScheduleName
            ,CharSequence morning, CharSequence afternoon,CharSequence evening,CharSequence night,
                               ArrayList<VitalSchedulesItemModel> vitalSchedulesItemModels) {
        SchedVal = ischedVal;
        this.VitalName = vitalName;
        this.ScheduleName = scheduleName;
        this.VitalScheduleName = vitalScheduleName;
        this.Morning = morning;
        this.Afternoon = afternoon;
        this.Evening = evening;
        this.Night = night;
        this.vitalSchedulesItemModel = vitalSchedulesItemModels;
        Log.d("vitalSchedulesItemModels", vitalSchedulesItemModels.toString());
    }

    public String getVitalName() {
        return VitalName;
    }

    public String getSchedVal() {
        return SchedVal;
    }

    public void setVitalName(String vitalName) {
        VitalScheduleName = vitalName;
    }

    public String getScheduleName() {
        return ScheduleName;
    }

    public void setScheduleName(String scheduleName) {
        ScheduleName = scheduleName;
    }
        public String getVitalScheduleName() {
        return VitalScheduleName;
    }

    public void setVitalScheduleName(String vitalScheduleName) {
        VitalScheduleName = vitalScheduleName;
    }

    public CharSequence getMorning() {
        return Morning;
    }

    public void setMorning(CharSequence morning) {
        Morning = morning;
    }

    public CharSequence getAfternoon() {
        return Afternoon;
    }

    public void setAfternoon(CharSequence afternoon) {
        Afternoon = afternoon;
    }

    public CharSequence getEvening() {
        return Evening;
    }

    public void setEvening(CharSequence evening) {
        Evening = evening;
    }

    public CharSequence getNight() {
        return Night;
    }

    public void setNight(CharSequence night) {
        Night = night;
    }
    public ArrayList<VitalSchedulesItemModel> getVitalSchedulesItemModel() {
        return vitalSchedulesItemModel;
    }

    public void setVitalSchedulesItemModels(ArrayList<VitalSchedulesItemModel> vitalSchedulesItemModels) {
        this.vitalSchedulesItemModel = vitalSchedulesItemModels;
    }
}
