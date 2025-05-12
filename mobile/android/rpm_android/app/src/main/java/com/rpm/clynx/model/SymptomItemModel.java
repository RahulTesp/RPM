package com.rpm.clynx.model;

    public class SymptomItemModel {
    private String Symptom;
    private String SymptomStartDateTime;
    private String Description;

    public SymptomItemModel(String symptom, String symptomStartDateTime, String description
                               ) {
        Symptom = symptom;
        SymptomStartDateTime = symptomStartDateTime;
        Description = description;
    }

    public SymptomItemModel() {
    }

    public String getSymptom() {
        return Symptom;
    }

    public void setSymptom(String symptom) {
        Symptom = symptom;
    }

    public String getSymptomStartDateTime() {
        return SymptomStartDateTime;
    }

    public void setSymptomStartDateTime(String symptomStartDateTime) {
        SymptomStartDateTime = symptomStartDateTime;
    }

    public String getDescription() {
            return Description;
        }

    public void setDescription(String description) {
            Description = description;
        }
}
