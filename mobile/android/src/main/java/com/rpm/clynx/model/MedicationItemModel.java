package com.rpm.clynx.model;

public class MedicationItemModel {
    private String Medicinename;
    private String MedicineSchedule;
    private CharSequence Morning;
    private CharSequence AfterNoon;
    private CharSequence Evening;
    private CharSequence Night;
    private String StartDate;
    private String EndDate;

    public MedicationItemModel(String medicinename, String medicineSchedule, CharSequence morning,
                               CharSequence afterNoon, CharSequence evening ,CharSequence night, String startDate, String endDate ) {
        Medicinename = medicinename;
        MedicineSchedule = medicineSchedule;
        Morning = morning;
        AfterNoon = afterNoon;
        Evening = evening;
        Night = night;
        StartDate = startDate;
        EndDate = endDate;
    }

    public MedicationItemModel() {
    }

    public String getMedicinename() {
        return Medicinename;
    }

    public void setMedicinename(String medicinename) {
        Medicinename = medicinename;
    }

    public String getMedicineSchedule() {
        return MedicineSchedule;
    }

    public void setMedicineSchedule(String medicineSchedule) {
        MedicineSchedule = medicineSchedule;
    }

    public CharSequence getMorning() {
        return Morning;
    }

    public void setMorning(CharSequence morning) {
        Morning = morning;
    }

    public CharSequence getAfterNoon() {
        return AfterNoon;
    }

    public void setAfterNoon(CharSequence afterNoon) {
        AfterNoon = afterNoon;
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

    public String getStartDate() {
        return StartDate;
    }

    public void setStartDate(String startDate) {
        StartDate = startDate;
    }
    public String getEndDate() {
        return EndDate;
    }

    public void setEndDate(String endDate) {
        EndDate = endDate;
    }
}
