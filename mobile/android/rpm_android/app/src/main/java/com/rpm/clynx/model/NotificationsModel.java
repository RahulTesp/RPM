package com.rpm.clynx.model;

import java.util.ArrayList;

public class NotificationsModel {
    private String notificationDate;
    private ArrayList<NotificationItemModel> notificationsList;

    public NotificationsModel() {
    }

    public NotificationsModel(String notificationDate, ArrayList<NotificationItemModel> notificationsList) {
        this.notificationDate = notificationDate;
        this.notificationsList = notificationsList;
    }

    public String getNotificationDate() {
        return notificationDate;
    }

    public void setNotificationDate(String notificationDate) {
        this.notificationDate = notificationDate;
    }

    public ArrayList<NotificationItemModel> getNotificationsList() {
        return notificationsList;
    }

    public void setNotificationsList(ArrayList<NotificationItemModel> notificationsList) {
        this.notificationsList = notificationsList;
    }
}
