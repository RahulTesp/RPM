package com.rpm.clynx.model;

public class HTMeasurementEntry implements Comparable<HTMeasurementEntry> {
    private String time;
    private String systolicValue;
    private String diastolicValue;
    private String pulseValue;
    public String getTime() { return time; }
    public void setTime(String time) { this.time = time; }

    public String getSystolicValue() { return systolicValue; }
    public void setSystolicValue(String systolicValue) { this.systolicValue = systolicValue; }

    public String getDiastolicValue() { return diastolicValue; }
    public void setDiastolicValue(String diastolicValue) { this.diastolicValue = diastolicValue; }

    public String getPulseValue() { return pulseValue; }
    public void setPulseValue(String pulseValue) { this.pulseValue = pulseValue; }

    @Override
    public int compareTo(HTMeasurementEntry other) {
        return this.time.compareTo(other.time);
    }
}
