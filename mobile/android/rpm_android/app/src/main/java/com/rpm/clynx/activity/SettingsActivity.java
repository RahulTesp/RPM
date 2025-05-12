package com.rpm.clynx.activity;

import android.content.SharedPreferences;
import android.os.Bundle;
import android.preference.PreferenceManager;
import android.view.MenuItem;
import androidx.appcompat.app.ActionBar;
import androidx.appcompat.app.AppCompatActivity;
import com.twilio.video.G722Codec;
import com.twilio.video.H264Codec;
import com.twilio.video.OpusCodec;
import com.twilio.video.PcmaCodec;
import com.twilio.video.PcmuCodec;
import com.twilio.video.Vp8Codec;
import com.twilio.video.Vp9Codec;

public class SettingsActivity extends AppCompatActivity {
    public static final String PREF_AUDIO_CODEC = "audio_codec";
    public static final String PREF_AUDIO_CODEC_DEFAULT = OpusCodec.NAME;
    public static final String PREF_VIDEO_CODEC = "video_codec";
    public static final String PREF_VIDEO_CODEC_DEFAULT = Vp8Codec.NAME;
    public static final String PREF_SENDER_MAX_AUDIO_BITRATE = "sender_max_audio_bitrate";
    public static final String PREF_SENDER_MAX_AUDIO_BITRATE_DEFAULT = "0";
    public static final String PREF_SENDER_MAX_VIDEO_BITRATE = "sender_max_video_bitrate";
    public static final String PREF_SENDER_MAX_VIDEO_BITRATE_DEFAULT = "0";
    public static final String PREF_VP8_SIMULCAST = "vp8_simulcast";
    public static final String PREF_ENABLE_AUTOMATIC_SUBSCRIPTION = "enable_automatic_subscription";
    public static final boolean PREF_ENABLE_AUTOMATIC_SUBSCRIPTION_DEFAULT = true;
    public static final boolean PREF_VP8_SIMULCAST_DEFAULT = false;

    private static final String[] VIDEO_CODEC_NAMES =
            new String[] {Vp8Codec.NAME, H264Codec.NAME, Vp9Codec.NAME};

    private static final String[] AUDIO_CODEC_NAMES =
            new String[] {
                    OpusCodec.NAME, PcmaCodec.NAME, PcmuCodec.NAME, G722Codec.NAME
            };

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setupActionBar();
        SharedPreferences sharedPreferences = PreferenceManager.getDefaultSharedPreferences(this);
        getSupportFragmentManager()
                .beginTransaction()
                .commit();
    }

    @Override
    public boolean onOptionsItemSelected(MenuItem item) {
        switch (item.getItemId()) {
            case android.R.id.home:
                finish();
                return true;
            default:
                return super.onOptionsItemSelected(item);
        }
    }

    private void setupActionBar() {
        ActionBar actionBar = getSupportActionBar();
        if (actionBar != null) {
            // Show the Up button in the action bar.
            actionBar.setDisplayHomeAsUpEnabled(true);
        }
    }
}
