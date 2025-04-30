package com.rpm.clynx.service;

import java.text.SimpleDateFormat;
import java.util.Date;
import java.util.TimeZone;

public class TimeFormatter {

    // Method to format timestamp received in the log format (Mon Feb 17 14:56:28 GMT+05:30 2025)
    public static String formatTimestampFromLog(String dateString) {
        try {
            // Define the pattern for the input timestamp (e.g., "Mon Feb 17 14:56:28 GMT+05:30 2025")
            SimpleDateFormat inputFormat = new SimpleDateFormat("EEE MMM dd HH:mm:ss z yyyy");

            // Parse the input date string into a Date object
            Date date = inputFormat.parse(dateString);

            // Convert to local time and format as HH:mm
            SimpleDateFormat localFormat = new SimpleDateFormat("hh:mm a");
            localFormat.setTimeZone(TimeZone.getDefault()); // Use the device's local timezone
            return localFormat.format(date);
        } catch (Exception e) {
            e.printStackTrace();
            return ""; // Return empty string on error
        }
    }

    public static String formatChartTimestampFromUTC(String utcTimestamp) {
        try {
            // Define the pattern for the input timestamp (e.g., "2025-02-17T11:00:32.487Z")
            SimpleDateFormat utcFormat = new SimpleDateFormat("yyyy-MM-dd'T'HH:mm:ss");
            utcFormat.setTimeZone(TimeZone.getTimeZone("UTC")); // Ensure it's parsed as UTC
            Date date = utcFormat.parse(utcTimestamp);

            // Convert to local time and format as HH:mm
            SimpleDateFormat localFormat = new SimpleDateFormat("MMM-dd - h:mm a");
            localFormat.setTimeZone(TimeZone.getDefault()); // Use the device's local timezone
            return localFormat.format(date);
        } catch (Exception e) {
            e.printStackTrace();
            return ""; // Return empty string on error
        }
    }
}
