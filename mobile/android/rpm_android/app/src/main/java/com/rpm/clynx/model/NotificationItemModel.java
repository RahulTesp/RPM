package com.rpm.clynx.model;

public class NotificationItemModel {
    private String Description;
    private String CreatedTime;
    private String NotiId;
    private  Boolean isRead;

    public NotificationItemModel(String description,String notiId, String createdTime, Boolean isRead) {
        Description = description;
        NotiId = notiId;
        CreatedTime = createdTime;
        isRead = isRead;
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
