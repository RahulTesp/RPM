package com.rpm.clynx.utility;

import android.os.Build;
import androidx.annotation.RequiresApi;
import java.text.ParseException;
import java.text.SimpleDateFormat;
import java.util.Calendar;
import java.util.Date;
import java.util.Locale;
import java.util.concurrent.TimeUnit;

public class TimeAgoUtils {
    @RequiresApi(api = Build.VERSION_CODES.O)
    public static String calculateTimeAgo(String localDateString) {
        try {
            SimpleDateFormat sdf = new SimpleDateFormat("yyyy-MMM-dd,h:mm a", Locale.getDefault());
            Date localDate = sdf.parse(localDateString);

            // Get the current local time
            long currentTimeMillis = System.currentTimeMillis();
            Date currentDate = new Date(currentTimeMillis);

            Calendar calendar1 = Calendar.getInstance();
            calendar1.setTime(localDate);

            Calendar calendar2 = Calendar.getInstance();
            calendar2.setTime(currentDate);

            // Calculate the time difference in milliseconds
            long timeDifferenceMillis = calendar2.getTimeInMillis() - calendar1.getTimeInMillis();
            long secondsDifference = TimeUnit.MILLISECONDS.toSeconds(timeDifferenceMillis);
            long minutesDifference = TimeUnit.MILLISECONDS.toMinutes(timeDifferenceMillis);
            long hoursDifference = TimeUnit.MILLISECONDS.toHours(timeDifferenceMillis);

            if (secondsDifference < 60) {
                if (secondsDifference <= 5) {
                    return "Just now";
                } else {
                    return secondsDifference + " seconds ago";
                }
            } else if (minutesDifference < 60) {
                // Round the minutes to the nearest unit
                minutesDifference = Math.round((float) timeDifferenceMillis / TimeUnit.MINUTES.toMillis(1));
                return minutesDifference + " minutes ago";
            } else if (hoursDifference < 24) {
                // Round the hours to the nearest unit
                hoursDifference = Math.round((float) timeDifferenceMillis / TimeUnit.HOURS.toMillis(1));
                return hoursDifference + " hours ago";
            }

            // Calculate days difference, considering the time
            long daysDifference = TimeUnit.MILLISECONDS.toDays(timeDifferenceMillis);

            if (daysDifference == 0 && hoursDifference > 24) {
                daysDifference = 1;
            }

            if (daysDifference < 7) {
                return calendar1.getDisplayName(Calendar.DAY_OF_WEEK, Calendar.LONG, Locale.getDefault());
            } else if (daysDifference >= 7 && daysDifference <= 14) {
                return "1 week ago";
            } else if (daysDifference > 14 && daysDifference <= 21) {
                return "2 weeks ago";
            } else if (daysDifference > 21 && daysDifference <= 28) {
                return "3 weeks ago";
            } else {
                int yearDiff = calendar2.get(Calendar.YEAR) - calendar1.get(Calendar.YEAR);
                int monthDiff = calendar2.get(Calendar.MONTH) - calendar1.get(Calendar.MONTH);

                if (yearDiff > 0 || monthDiff > 1) {
                    return monthDiff + " months ago";
                } else if (yearDiff == 0 && monthDiff == 1) {
                    return "1 month ago";
                }
            }

            // Return a default value if necessary
            return "Some default text";
        } catch (ParseException e) {
            e.printStackTrace();
            return null;
        }
    }
}
