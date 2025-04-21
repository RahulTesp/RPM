package com.rpm.clynx.model;

import android.util.Log;
import java.util.ArrayList;

public class DeviceModel {

    private ArrayList<DeviceItemModel> deviceItemModels;

    public DeviceModel() {
    }

    public DeviceModel(ArrayList<DeviceItemModel> deviceItemModels) {

        this.deviceItemModels = deviceItemModels;
        Log.d("deviceItemModels", deviceItemModels.toString());
    }

    public ArrayList<DeviceItemModel> getDeviceItemModels() {
        return deviceItemModels;
    }
}
