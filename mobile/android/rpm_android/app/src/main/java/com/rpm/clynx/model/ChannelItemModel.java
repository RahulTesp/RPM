package com.rpm.clynx.model;
import android.util.Log;

public class ChannelItemModel {

    private String MemberUsrnm;
    private String MemberName;

    public ChannelItemModel(String memberusrnm, String membername) {
        MemberUsrnm = memberusrnm;
        MemberName = membername;
        Log.d("MemberUsrnm--", MemberUsrnm.toString());
    }

    public ChannelItemModel() {
    }

    public String getMemberUsrnm() {
        return MemberUsrnm;
    }

    public String getMemberName() {
        return MemberName;
    }
}
