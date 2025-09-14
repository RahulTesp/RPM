package com.rpm.clynx.utility;

import android.util.Log;

import java.text.ParseException;
import java.text.SimpleDateFormat;
import java.util.Calendar;
import java.util.Date;
import java.util.Locale;
import java.util.TimeZone;

public class DateUtils {

    public static String convertToUTC(String localDateStr, String inputFormatStr, String outputFormatStr) {
        SimpleDateFormat inputFormat = new SimpleDateFormat(inputFormatStr);
        try {
            Date localDate = inputFormat.parse(localDateStr);

            TimeZone utcTimeZone = TimeZone.getTimeZone("UTC");
            SimpleDateFormat outputFormat = new SimpleDateFormat(outputFormatStr);
            outputFormat.setTimeZone(utcTimeZone);

            return outputFormat.format(localDate);
        } catch (ParseException e) {
            e.printStackTrace();
            return null;
        }
    }

    public static String getStartDateOnly(String dateTimeString) {
        if (dateTimeString == null || !dateTimeString.contains("T")) {
            return dateTimeString; // return as-is if format is unexpected
        }
        return dateTimeString.split("T")[0]; // splits at 'T' and returns the date part
    }

    public static String convertUtcToLocalFormatted(String utcDateStr, String outputFormatStr) {
        SimpleDateFormat inputFormat = new SimpleDateFormat("yyyy-MM-dd'T'HH:mm:ss");
        inputFormat.setTimeZone(TimeZone.getTimeZone("UTC"));

        try {
            Date utcDate = inputFormat.parse(utcDateStr);

            SimpleDateFormat outputFormat = new SimpleDateFormat(outputFormatStr, Locale.getDefault());
            outputFormat.setTimeZone(TimeZone.getDefault()); // Set to local time zone

            return outputFormat.format(utcDate);
        } catch (ParseException e) {
            e.printStackTrace();
            return null;
        }
    }
    public static String convertUtcToLocalTimeDiff(String utcDateStr, String outputFormatStr) {
        SimpleDateFormat inputFormat = new SimpleDateFormat("yyyy-MM-dd'T'HH:mm:ss", Locale.getDefault());
        inputFormat.setTimeZone(TimeZone.getTimeZone("UTC"));

        try {
            Date utcDate = inputFormat.parse(utcDateStr);

            SimpleDateFormat outputFormat = new SimpleDateFormat(outputFormatStr, Locale.getDefault());
            outputFormat.setTimeZone(TimeZone.getDefault()); // Set to local time zone

            return outputFormat.format(utcDate);
        } catch (ParseException e) {
            e.printStackTrace();
            return null;
        }
    }

    public static String formatDate(String inputDateStr, String outputFormat) throws ParseException {
        // Create a SimpleDateFormat object for the input format
        SimpleDateFormat inputDateFormat = new SimpleDateFormat("yyyy-MM-dd'T'HH:mm:ss");

        // Create a SimpleDateFormat object for the desired output format
        SimpleDateFormat outputDateFormat = new SimpleDateFormat(outputFormat);

        // Parse the input date string
        Date date = inputDateFormat.parse(inputDateStr);

        // Format the date in the desired output format
        return outputDateFormat.format(date);
    }
    public static boolean isPastDate(String dateStr) throws ParseException {
        Log.d("dateStr", dateStr);
        SimpleDateFormat sdf = new SimpleDateFormat("yyyy-MM-dd'T'HH:mm:ss", Locale.getDefault());
        sdf.setLenient(false);
        Date endDate = sdf.parse(dateStr);

        // Strip time from current date for accurate comparison
        Calendar today = Calendar.getInstance();
        today.set(Calendar.HOUR_OF_DAY, 0);
        today.set(Calendar.MINUTE, 0);
        today.set(Calendar.SECOND, 0);
        today.set(Calendar.MILLISECOND, 0);

        return endDate.before(today.getTime());
    }


}
