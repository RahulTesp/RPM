package com.rpm.clynx.model;

import android.util.Log;
import java.util.ArrayList;

public class ChannelModel {

    private ArrayList<ChannelItemModel> channelItemModels;
    String Token;
    public ChannelModel() {
    }

    public ChannelModel(ArrayList<ChannelItemModel> channelItemModels,String token) {
        this.channelItemModels = channelItemModels;
        this.Token = token;
        Log.d("channelItemModels", channelItemModels.toString());
    }

    public ArrayList<ChannelItemModel> getChannelList() {
        return channelItemModels;
    }
    public String getToken() {
        return Token;
    }
}
