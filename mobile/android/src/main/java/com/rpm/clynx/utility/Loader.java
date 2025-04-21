package com.rpm.clynx.utility;

import android.app.Activity;
import android.app.AlertDialog;
import android.view.LayoutInflater;
import android.widget.TextView;
import com.rpm.clynx.R;

public class Loader {

    Activity activity;
    AlertDialog alertDialog;
    String text="Please wait...";
    TextView loadingtext;
    String loadtext;

    public Loader(Activity mActivity){
        this.activity=mActivity;
    }

    public void show(String text){
        this.text=text;
        AlertDialog.Builder builder = new AlertDialog.Builder(activity);
        LayoutInflater inflater = activity.getLayoutInflater();
        builder.setView(inflater.inflate(R.layout.progressbar_loader,null));
        builder.setCancelable(false);
        alertDialog = builder.create();
        alertDialog.show();
    }
    public void  dismiss(){
        alertDialog.dismiss();
    }
}
