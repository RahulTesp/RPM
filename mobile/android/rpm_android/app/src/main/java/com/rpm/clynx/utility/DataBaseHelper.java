package com.rpm.clynx.utility;

import android.content.ContentValues;
import android.content.Context;
import android.database.Cursor;
import android.database.sqlite.SQLiteDatabase;
import android.database.sqlite.SQLiteOpenHelper;
import android.util.Log;

public class DataBaseHelper extends SQLiteOpenHelper {
    public static final String DATABASE_NAME="RPMDb.db";
    public static final String TABLE_NAME="bloodPressure";
    public static final String TABLE_NAME2="myprofileandprogram";
    public static final String TABLE_NAME3="vitaltable";
    public static final String TABLE_NAME4="programDetails";
    public static final String Col_2="Name";
    public static final String Col_3="Name";
    public static final String Col_4="Name";
    public static final String Col_T2="Name";
    public DataBaseHelper(Context context) {
        super(context, DATABASE_NAME, null, 1);
    }

    @Override
    public void onCreate(SQLiteDatabase db) {
        db.execSQL("create table " + TABLE_NAME+"(ID INTEGER PRIMARY KEY AUTOINCREMENT,NAME TEXT)");
        db.execSQL("create table " + TABLE_NAME2+"(ID INTEGER PRIMARY KEY AUTOINCREMENT,NAME TEXT)");
        db.execSQL("create table " + TABLE_NAME3+"(ID INTEGER PRIMARY KEY AUTOINCREMENT,NAME TEXT)");
        db.execSQL("create table " + TABLE_NAME4+"(ID INTEGER PRIMARY KEY AUTOINCREMENT,NAME TEXT)");
    }

    @Override
    public void onUpgrade(SQLiteDatabase db, int i, int i1) {
        db.execSQL("DROP TABLE IF EXISTS "+TABLE_NAME);
        db.execSQL("DROP TABLE IF EXISTS "+TABLE_NAME2);
        db.execSQL("DROP TABLE IF EXISTS "+TABLE_NAME3);
        db.execSQL("DROP TABLE IF EXISTS "+TABLE_NAME4);
        onCreate(db);
    }

    public boolean insertVitalData(String Name){
        SQLiteDatabase db = this.getWritableDatabase();
        ContentValues contentValues = new ContentValues();
        contentValues.put(Col_2,Name);
        Long res = db.insert(TABLE_NAME3,null,contentValues);

        if (res==-1){
            return false;
        }else {
            return true;
        }

    }
    public boolean insertData(String Name){
        SQLiteDatabase db = this.getWritableDatabase();
        ContentValues contentValues = new ContentValues();
        contentValues.put(Col_2,Name);
        Long res = db.insert(TABLE_NAME,null,contentValues);

        if (res==-1){
            return false;
        }else {
            return true;
        }

    }
    public boolean insertProgDet(String Name){
        SQLiteDatabase db = this.getWritableDatabase();
        ContentValues contentValues = new ContentValues();
        Log.d("contentValues",contentValues.toString());
        contentValues.put(Col_3,Name);
        Long res = db.insert(TABLE_NAME4,null,contentValues);

        if (res==-1){
            return false;
        }else {
            return true;
        }

    }

    public boolean insertProfileData(String Name){
        SQLiteDatabase db = this.getWritableDatabase();
        ContentValues contentValues = new ContentValues();
        contentValues.put(Col_T2,Name);
        Long res = db.insert(TABLE_NAME2,null,contentValues);

        if (res==-1){
            return false;
        }else {
            return true;
        }

    }

    public void deleteData(){
        SQLiteDatabase db = this.getWritableDatabase();
        //db.execSQL("DROP TABLE IF EXISTS "+TABLE_NAME);
        db.delete(TABLE_NAME,null,null);
    }

    public void deleteVitalData(){
        SQLiteDatabase db = this.getWritableDatabase();
        db.execSQL("DROP TABLE IF EXISTS "+TABLE_NAME3);

    }

    public void deleteProfileData(String s){
        SQLiteDatabase db = this.getWritableDatabase();
        //db.execSQL("DROP TABLE IF EXISTS "+TABLE_NAME2);
        db.delete(TABLE_NAME2, null, null);

    }

    public Cursor getdata(){
        SQLiteDatabase db = this.getWritableDatabase();
        Cursor cursorCourses = db.rawQuery("SELECT NAME FROM " + TABLE_NAME, null);
        return cursorCourses;
    }
    public Cursor getPgmDet(){
        SQLiteDatabase db = this.getWritableDatabase();
        Cursor pgmDet = db.rawQuery("SELECT NAME FROM " + TABLE_NAME4, null);
        return pgmDet;
    }
    public Cursor getVitaldata(){
        SQLiteDatabase db = this.getWritableDatabase();
        Cursor vitaldata = db.rawQuery("SELECT NAME FROM " + TABLE_NAME3, null);
        return vitaldata;
    }

    public Cursor getProfileData(){
        SQLiteDatabase db = this.getWritableDatabase();
        Cursor ProfileData = db.rawQuery("SELECT NAME FROM " + TABLE_NAME2, null);
        return ProfileData;
    }
}
