package com.rpm.clynx.model;

public class DeviceItemModel {
    private  String Ival;
    private String VitalName;
    private String DeviceNumber;
    private String DeviceCommunicationType;
    private String DeviceName;
    private String DeviceStatus;

    public DeviceItemModel(String ival, String vitalName, String deviceNumber, String deviceCommunicationType,
                           String deviceName, String deviceStatus) {
        Ival = ival;
        VitalName = vitalName;
        DeviceNumber = deviceNumber;
        DeviceCommunicationType = deviceCommunicationType;
        DeviceName = deviceName;
        DeviceStatus = deviceStatus;

    }

    public DeviceItemModel() {
    }

    public String getIval() {
        return Ival;
    }

    public String getVitalName() {
        return VitalName;
    }

    public String getDeviceNumber() {
        return DeviceNumber;
    }

    public void setDeviceNumber(String deviceNumber) {
        DeviceNumber = deviceNumber;
    }

    public String getDeviceCommunicationType() {
        return DeviceCommunicationType;
    }

    public String getDeviceName() {
        return DeviceName;
    }

    public String getDeviceStatus() {
        return DeviceStatus;
    }

}
