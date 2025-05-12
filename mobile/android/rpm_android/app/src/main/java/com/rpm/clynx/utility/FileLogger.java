package com.rpm.clynx.utility;

import android.content.Context;

import android.util.Log;

import java.io.File;

import java.io.FileWriter;

import java.io.IOException;

public class FileLogger {

    private static final String LOG_FILE_NAME = "app_log.txt";

    private static File logFile;

    public static void init(Context context) {

        File dir = context.getExternalFilesDir(null); // or use getFilesDir() for internal

        if (dir != null) {

            logFile = new File(dir, LOG_FILE_NAME);

        }

    }

    public static void d(String tag, String message) {

        Log.d(tag, message);

        writeToFile("DEBUG", tag, message);

    }

    public static void e(String tag, String message) {

        Log.e(tag, message);

        writeToFile("ERROR", tag, message);

    }

    private static void writeToFile(String level, String tag, String message) {

        if (logFile == null) return;

        try (FileWriter writer = new FileWriter(logFile, true)) {

            writer.append(String.format("%s/%s: %s\n", level, tag, message));

        } catch (IOException e) {

            e.printStackTrace();

        }

    }

    public static File getLogFile() {

        return logFile;

    }

}

