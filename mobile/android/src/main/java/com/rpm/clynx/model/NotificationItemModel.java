package com.rpm.clynx.model;

public class NotificationItemModel {
    private String Description;
    private String CreatedTime;
    private String NotiId;

    public NotificationItemModel(String description,String notiId, String createdTime) {
        Description = description;
        NotiId = notiId;
        CreatedTime = createdTime;
    }

    public NotificationItemModel() {
    }

    public String getDescription() {
        return Description;
    }

    public void setDescription(String description) {
        Description = description;
    }
    public String getNotiId() {
        return NotiId;
    }

    public void setNotiId(String notiId) {
        NotiId = notiId;
    }

    public String getCreatedTime() {
        return CreatedTime;
    }

    public void setCreatedTime(String createdTime) {
        CreatedTime = createdTime;
    }
}
