package com.rpm.clynx.model;

public class InsuranceItemModel {
    private String InsuranceVendorName;
    private String IsPrimary;

    public InsuranceItemModel(String insuranceVendorName , String isPrimary) {
        InsuranceVendorName = insuranceVendorName;
        IsPrimary = isPrimary;
    }

    public InsuranceItemModel() {
    }

    public String getInsuranceVendorName() {
        return InsuranceVendorName;
    }

    public void setInsuranceVendorName(String insuranceVendorName) {
        InsuranceVendorName = insuranceVendorName;
    }
    public String getIsPrimary() {
        return IsPrimary;
    }

    public void setIsPrimary(String isPrimary) {
        IsPrimary = isPrimary;
    }
}
