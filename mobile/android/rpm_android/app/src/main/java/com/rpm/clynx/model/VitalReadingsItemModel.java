package com.rpm.clynx.model;

import android.util.Log;

public class VitalReadingsItemModel {
    private String Systolic;
    private String Diastolic;
    private String Pulse;
    private String Oxygen;
    private String PulSE;
    private String Weight;
    private String Glucose;
    private String Schedule;
    private String ReadingTime;
    private String SystolicStatus;
    private String DiastolicStatus;
    private String PulseStatus;
    private String Status;
    private String OxygenStatus;
    private String PulseStatuS;

    public VitalReadingsItemModel(String systolic,String diastolic, String pulse,String oxygen, String pulSe,String weight,String glucose,String schedule,String readingTime,String systolicstatus, String diastolicstatus,String pulsestatus, String status, String oxygenstatus,String pulseStatuS) {
        Systolic = systolic;
        Diastolic = diastolic;
        Pulse = pulse;
        Oxygen = oxygen;
        PulSE = pulSe;
        Weight = weight;
        Glucose = glucose;
        Schedule = schedule;
        ReadingTime = readingTime;
        SystolicStatus = systolicstatus;
        DiastolicStatus = diastolicstatus;
        PulseStatus = pulsestatus;
        Status = status;
        OxygenStatus = oxygenstatus;
        PulseStatuS = pulseStatuS;
        Log.d("BGmgdl--l", Systolic.toString());
        Log.d("ReadingTime--", ReadingTime.toString());
    }

    public VitalReadingsItemModel() {
    }

    public String getSystolic() {
        return Systolic;
    }

    public void setSystolic(String systolic) {
        Systolic = systolic;
    }

    public String getDiastolic() {
        return Diastolic;
    }

    public void setDiastolic(String diastolic) {
        Diastolic = diastolic;
    }

    public String getPulse() {
        return Pulse;
    }

    public void setPulse(String pulse) {
        Pulse = pulse;
    }
    public String getOxygen() {
        return Oxygen;
    }

    public void setOxygen(String oxygen) {
        Oxygen = oxygen;
    }
    public String getPulSE() {
        return PulSE;
    }

    public void setPulSE(String pulSe) {
        PulSE = pulSe;
    }
    public String getWeight() {
        return Weight;
    }

    public void setWeight(String weight) {
        Weight = weight;
    }
    public String getGlucose() {
        return Glucose;
    }

    public void setGlucose(String glucose) {
        Glucose = glucose;
    }
    public String getSchedule() {
        return Schedule;
    }

    public void setSchedule(String schedule) {
        Schedule = schedule;
    }

    public String getReadingTime() {
        return ReadingTime;
    }

    public void setReadingTime(String readingTime) {
        ReadingTime = readingTime;
    }

    public String getSystolicStatus() {
        return SystolicStatus;
    }

    public void setSystolicStatus(String systolicStatus) {
        SystolicStatus = systolicStatus;
    }

    public String getDiastolicStatus() {
        return DiastolicStatus;
    }

    public void setDiastolicStatus(String diastolicStatus) {
        DiastolicStatus = diastolicStatus;
    }
    public String getPulseStatus() {
        return PulseStatus;
    }

    public void setPulseStatus(String pulseStatus) {
        PulseStatus = pulseStatus;
    }
    public String getStatus() {
        return Status;
    }

    public void setStatus(String status) {
        Status = status;
    }

    public String getOxygenStatus() {
        return OxygenStatus;
    }

    public void setOxygenStatus(String oxygenStatus) {
        OxygenStatus = oxygenStatus;
    }
    public String getPulseStatuS() {
        return PulseStatuS;
    }

    public void setPulseStatuS(String pulseStatuS) {
        PulseStatuS = pulseStatuS;
    }
}
