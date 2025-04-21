package com.rpm.clynx.activity;

import android.Manifest;
import android.annotation.SuppressLint;
import android.app.Activity;
import android.content.Context;
import android.content.DialogInterface;
import android.content.Intent;
import android.content.SharedPreferences;
import android.content.pm.PackageManager;
import android.media.AudioManager;
import android.os.Bundle;
import android.os.Handler;
import android.preference.PreferenceManager;
import android.util.Log;
import android.view.Display;
import android.view.Menu;
import android.view.MenuInflater;
import android.view.MenuItem;
import android.view.View;
import android.widget.EditText;
import android.widget.ProgressBar;
import android.widget.Toast;
import androidx.annotation.NonNull;
import androidx.appcompat.app.AlertDialog;
import androidx.appcompat.app.AppCompatActivity;
import androidx.core.app.ActivityCompat;
import androidx.core.content.ContextCompat;
import com.google.android.material.floatingactionbutton.FloatingActionButton;
import com.google.android.material.snackbar.Snackbar;
import com.rpm.clynx.fragments.Login;
import com.rpm.clynx.utility.DataBaseHelper;
import com.rpm.clynx.utility.MyApplication;
import com.twilio.audioswitch.AudioDevice;
import com.twilio.audioswitch.AudioDevice.BluetoothHeadset;
import com.twilio.audioswitch.AudioDevice.Earpiece;
import com.twilio.audioswitch.AudioDevice.Speakerphone;
import com.twilio.audioswitch.AudioDevice.WiredHeadset;
import com.twilio.video.AudioTrackPublication;
import com.twilio.video.LocalParticipant;
import com.twilio.audioswitch.AudioSwitch;
import com.twilio.video.AudioCodec;
import com.twilio.video.ConnectOptions;
import com.twilio.video.EncodingParameters;
import com.twilio.video.G722Codec;
import com.twilio.video.H264Codec;
import com.twilio.video.LocalAudioTrack;
import com.twilio.video.LocalVideoTrack;
import com.twilio.video.OpusCodec;
import com.twilio.video.PcmaCodec;
import com.twilio.video.PcmuCodec;
import com.twilio.video.RemoteAudioTrack;
import com.twilio.video.RemoteAudioTrackPublication;
import com.twilio.video.RemoteDataTrack;
import com.twilio.video.RemoteDataTrackPublication;
import com.twilio.video.RemoteParticipant;
import com.twilio.video.RemoteVideoTrack;
import com.twilio.video.RemoteVideoTrackPublication;
import com.twilio.video.Room;
import com.twilio.video.TwilioException;
import com.twilio.video.Video;
import com.twilio.video.VideoCodec;
import com.twilio.video.VideoTrack;
import com.twilio.video.VideoView;
import com.twilio.video.Vp8Codec;
import com.twilio.video.Vp9Codec;
import  com.rpm.clynx.R;
import com.rpm.clynx.utility.CameraCapturerCompat;
import java.util.ArrayList;
import java.util.Collections;
import java.util.List;
import kotlin.Unit;
import tvi.webrtc.VideoSink;

public class VideoCallTwilio extends AppCompatActivity {
    private static final int CAMERA_MIC_PERMISSION_REQUEST_CODE = 1;
    private static final int CAMERA_PERMISSION_INDEX = 0;
    private static final int MIC_PERMISSION_INDEX = 1;
    private static final String TAG = "VideoActivity";
    DataBaseHelper db;
    RemoteParticipant remoteParticipantvalue;
    List<RemoteParticipant> remoteParticipantval;

    /*
     * Audio and video tracks can be created with names. This feature is useful for categorizing
     * tracks of participants. For example, if one participant publishes a video track with
     * ScreenCapturer and CameraCapturer with the names "screen" and "camera" respectively then
     * other participants can use RemoteVideoTrack#getName to determine which video track is
     * produced from the other participant's screen or camera.
     */
    private static final String LOCAL_AUDIO_TRACK_NAME = "mic";
    private static final String LOCAL_VIDEO_TRACK_NAME = "camera";

    /*
     * You must provide a Twilio Access Token to connect to the Video service
     */
    //  private static final String TWILIO_ACCESS_TOKEN = BuildConfig.TWILIO_ACCESS_TOKEN;
    // private static final String ACCESS_TOKEN_SERVER = BuildConfig.TWILIO_ACCESS_TOKEN_SERVER;

    /*
     * Access token used to connect. This field will be set either from the console generated token
     * or the request to the token server.
     */
    private String accessToken;

    /*
     * A Room represents communication between a local participant and one or more participants.
     */
    private Room room;
    private LocalParticipant localParticipant;

    /*
     * AudioCodec and VideoCodec represent the preferred codec for encoding and decoding audio and
     * video.
     */
    private AudioCodec audioCodec;
    private VideoCodec videoCodec;

    /*
     * Encoding parameters represent the sender side bandwidth constraints.
     */
    private EncodingParameters encodingParameters;

    /*
     * A VideoView receives frames from a local or remote video track and renders them
     * to an associated view.
     */
    private VideoView primaryVideoView;
    private VideoView thumbnailVideoView;

    View remotePlaceHolderImage, localPlaceHolderImage;

    /*
     * Android shared preferences used for settings
     */
    private SharedPreferences preferences;

    /*
     * Android application UI elements
     */
    private CameraCapturerCompat cameraCapturerCompat;
    private LocalAudioTrack localAudioTrack;
    private LocalVideoTrack localVideoTrack;
    private FloatingActionButton connectActionFab;
    private FloatingActionButton switchCameraActionFab;
    private FloatingActionButton localVideoActionFab;
    private FloatingActionButton muteActionFab;
    private ProgressBar reconnectingProgressBar;
    private AlertDialog connectDialog;
    private String remoteParticipantIdentity;

    /*
     * Audio management
     */
    private AudioSwitch audioSwitch;
    private int savedVolumeControlStream;
    private MenuItem audioDeviceMenuItem;
    private VideoSink localVideoView;
    private boolean disconnectedFromOnDestroy;
    private boolean enableAutomaticSubscription;
    String roomName,roomToken;
    Activity latestActivity;
    private Handler handler;
    SharedPreferences pref;
    SharedPreferences.Editor editor;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_video_call);

        primaryVideoView = findViewById(R.id.primary_video_view);
        thumbnailVideoView = findViewById(R.id.thumbnail_video_view);
        reconnectingProgressBar = findViewById(R.id.reconnecting_progress_bar);
        connectActionFab = findViewById(R.id.connect_action_fab);
        switchCameraActionFab = findViewById(R.id.switch_camera_action_fab);
        localVideoActionFab = findViewById(R.id.local_video_action_fab);
        muteActionFab = findViewById(R.id.mute_action_fab);
        remotePlaceHolderImage = findViewById(R.id.remote_placeholder);
        localPlaceHolderImage = findViewById(R.id.local_placeholder);
        handler = new Handler();
        latestActivity = ((MyApplication) getApplication()).getLatestActivity();
        pref = getApplicationContext().getSharedPreferences("RPMUserApp", MODE_PRIVATE);
        editor = pref.edit();
        Display display = getWindowManager().getDefaultDisplay();
        // Get the screen width and height
        int screenWidth = display.getWidth();
        int screenHeight = display.getHeight();
        System.out.println("screenWidth");
        System.out.println(screenWidth);
        System.out.println("screenHeight");
        System.out.println(screenHeight);
        AudioManager audioManager = (AudioManager) VideoCallTwilio.this .getSystemService(Context.AUDIO_SERVICE);
        // Retrieve the values from the Intent
        roomName = getIntent().getStringExtra("videocallRoomname");
        roomToken = getIntent().getStringExtra("videocallToken");
        System.out.println("roomName");
        System.out.println(roomName);
        System.out.println("roomToken");
        System.out.println(roomToken);
        boolean isWiredHeadsetOn = audioManager.isWiredHeadsetOn();
        System.out.println("isWiredHeadsetOn");
        System.out.println(isWiredHeadsetOn);
        audioManager.setMode( AudioManager.MODE_IN_COMMUNICATION);
        audioManager.setSpeakerphoneOn(!isWiredHeadsetOn);
        audioManager.setSpeakerphoneOn(isWiredHeadsetOn ? false : true);

        /*
         * Get shared preferences to read settings
         */
        preferences = PreferenceManager.getDefaultSharedPreferences(this);

        /*
         * Setup audio management and set the volume control stream
         */
        audioSwitch = new AudioSwitch(getApplicationContext());
        savedVolumeControlStream = getVolumeControlStream();
        setVolumeControlStream(AudioManager.STREAM_VOICE_CALL);

        /*
         * Check camera and microphone permissions. Needed in Android M. Also, request for bluetooth
         * permissions for enablement of bluetooth audio routing.
         */
        if (!checkPermissionForCameraAndMicrophone()) {
            requestPermissionForCameraMicrophoneAndBluetooth();
        } else {
            audioSwitch.start((audioDevices, audioDevice) -> {
                Toast.makeText(this, audioDevice.getName() , Toast.LENGTH_SHORT).show();
                Earpiece earpiece = null;
                Speakerphone speakerphone = null;
                WiredHeadset wiredheadset = null;
                List<AudioDevice> availableAudioDevices = audioSwitch.getAvailableAudioDevices();
                System.out.println("audioDevice availableAudioDevices");
                System.out.println(availableAudioDevices);
                audioDevice = availableAudioDevices.get(0);
                if(audioDevice.getName().equals("WiredHeadset"))
                {
                    System.out.println("audioDevice WiredHeadset he");
                    System.out.println(availableAudioDevices);
                    audioSwitch.selectDevice(audioDevice);
                }
                if(audioDevice instanceof WiredHeadset) {
                    System.out.println("hai 111");
                    wiredheadset = (WiredHeadset) audioDevice;
                    audioDevice = wiredheadset;
                    System.out.println("audioDevice 1");
                    System.out.println(audioDevice);
                    audioSwitch.selectDevice(audioDevice);
                }
                if(audioDevice instanceof Speakerphone) {
                    System.out.println("hai Speakerphone");
                    speakerphone = (Speakerphone) audioDevice;
                    audioDevice = speakerphone;
                    System.out.println("audioDevice Speakerphone");
                    System.out.println(audioDevice);
                    audioSwitch.selectDevice(audioDevice);
                }
                if (audioDevice instanceof Earpiece ) {
                    List<AudioDevice> availableAudioDevicess = audioSwitch.getAvailableAudioDevices();
                    System.out.println("audioDevice availableAudioDevicess");
                    System.out.println(availableAudioDevicess);
                    int selectedDeviceIndex = availableAudioDevicess.indexOf(new Speakerphone());
                    audioSwitch.selectDevice(availableAudioDevicess.get(selectedDeviceIndex));
                    System.out.println("audioDevice selectedDeviceIndex");
                }
                return Unit.INSTANCE;
            });

            createAudioAndVideoTracks();
        }

        /*
         * Set the initial state of the UI
         */

        System.out.println("initial===11111");
        System.out.println("encodingParameters");
        System.out.println(encodingParameters);
        System.out.println("localVideoTrack");
        System.out.println(localVideoTrack);
        System.out.println("localParticipant");
        System.out.println(localParticipant);

        if( encodingParameters!=null && localVideoTrack!=null && localParticipant!=null)
        {
            System.out.println("initial===");
            System.out.println("encodingParameters");
            System.out.println(encodingParameters);
            System.out.println("localVideoTrack");
            System.out.println(localVideoTrack);
            System.out.println("localParticipant");
            System.out.println(localParticipant);
        }
    }

    @SuppressLint("SetTextI18n")
    @Override
    protected void
    onResume() {
        super.onResume();
        Log.d("VideoCallTwilio", "======== onResume called ========");
        if (!pref.getBoolean("loginstatus", false)) {
            Log.d("loginstsfrmhome2", "User not logged in, redirecting to login");
            editor.clear();
            editor.commit();
            db.deleteProfileData("myprofileandprogram");
            db.deleteData();

            try {
                Intent intentlogout = new Intent(((MyApplication) getApplication()).getLatestActivity(), Login.class);
                intentlogout.addFlags(Intent.FLAG_ACTIVITY_NEW_TASK | Intent.FLAG_ACTIVITY_CLEAR_TASK);
                startActivity(intentlogout);
                finish();
            } catch (Exception e) {
                Log.e("onLogOff Clear", e.toString());
            }
            return;
        }

       //  Update preferences and encoding
        audioCodec = getAudioCodecPreference(SettingsActivity.PREF_AUDIO_CODEC, SettingsActivity.PREF_AUDIO_CODEC_DEFAULT);
        videoCodec = getVideoCodecPreference(SettingsActivity.PREF_VIDEO_CODEC, SettingsActivity.PREF_VIDEO_CODEC_DEFAULT);
        enableAutomaticSubscription = getAutomaticSubscriptionPreference(SettingsActivity.PREF_ENABLE_AUTOMATIC_SUBSCRIPTION, SettingsActivity.PREF_ENABLE_AUTOMATIC_SUBSCRIPTION_DEFAULT);

        final EncodingParameters newEncodingParameters = getEncodingParameters();

        // Handle room connection
        // If room isn't connected, reconnect
        if (room == null && roomName != null && !roomName.isEmpty()) {
            connectToRoom(roomName);
            intializeUI();
        } else if (room != null) {
            localParticipant = room.getLocalParticipant();
        }

        if (localParticipant == null) {
            Log.e("VideoCallTwilio", "localParticipant is null in onResume");
        } else {
            Log.d("VideoCallTwilio", "localParticipant identity: " + localParticipant.getIdentity());
        }

        // Unmute video (just enable the track)
        if (localVideoTrack != null) {
            localVideoTrack.enable(true);
            Log.d("VideoCallTwilio", "localVideoTrack enabled (unmuted)");
        }

        // Set encoding parameters
        if (!newEncodingParameters.equals(encodingParameters) && localParticipant != null) {
            localParticipant.setEncodingParameters(newEncodingParameters);
        }
        encodingParameters = newEncodingParameters;

        // Update UI
        if (room != null) {
            reconnectingProgressBar.setVisibility(
                    (room.getState() != Room.State.RECONNECTING) ? View.GONE : View.VISIBLE);
        }

        // Debug log to confirm final track count
        if (localParticipant != null) {
            Log.d("TrackCheck", "Published video tracks count: " + localParticipant.getVideoTracks().size());
        }

        Log.d("VideoCallTwilio", "======== onResume execution completed ========");
    }

    @Override
    public boolean onCreateOptionsMenu(Menu menu) {
        MenuInflater inflater = getMenuInflater();
        inflater.inflate(R.menu.menu_video_activity, menu);
        audioDeviceMenuItem = menu.findItem(R.id.menu_audio_device);
        // AudioSwitch has already started and thus notified of the initial selected device
        // so we need to updates the UI
        updateAudioDeviceIcon(audioSwitch.getSelectedAudioDevice());
        return true;
    }
    @Override
    public void onBackPressed() {
        super.onBackPressed();
        AlertDialog.Builder builder = new AlertDialog.Builder(this);
        builder.setMessage("Do you want to exit?")
                .setPositiveButton("Yes", new DialogInterface.OnClickListener() {
                    @Override
                    public void onClick(DialogInterface dialog, int which) {
                        // Exit the app or perform any other desired action
                        room.disconnect();
                        finish();
                    }
                })
                .setNegativeButton("No", new DialogInterface.OnClickListener() {
                    @Override
                    public void onClick(DialogInterface dialog, int which) {
                        // Continue with the current activity
                        dialog.dismiss();
                    }
                })
                .show();
    }

    @Override
    public boolean onOptionsItemSelected(MenuItem item) {
        switch (item.getItemId()) {
            case R.id.menu_settings:
                startActivity(new Intent(this, SettingsActivity.class));
                return true;
            case R.id.menu_audio_device:
                showAudioDevices();
                return true;
            default:
                return false;
        }
    }

    @Override
    public void onRequestPermissionsResult(
            int requestCode, @NonNull String[] permissions, @NonNull int[] grantResults) {
        super.onRequestPermissionsResult(requestCode, permissions, grantResults);
        if (requestCode == CAMERA_MIC_PERMISSION_REQUEST_CODE) {
            /*
             * The first two permissions are Camera & Microphone, bluetooth isn't required but
             * enabling it enables bluetooth audio routing functionality.
             */
            boolean cameraAndMicPermissionGranted =
                    (PackageManager.PERMISSION_GRANTED == grantResults[CAMERA_PERMISSION_INDEX])
                            & (PackageManager.PERMISSION_GRANTED
                            == grantResults[MIC_PERMISSION_INDEX]);

            /*
             * Due to bluetooth permissions being requested at the same time as camera and mic
             * permissions, AudioSwitch should be started after providing the user the option
             * to grant the necessary permissions for bluetooth.
             */
            audioSwitch.start(
                    (audioDevices, audioDevice) -> {
                        Earpiece earpiece = null;
                        Speakerphone speakerphone = null;
                        WiredHeadset wiredheadset = null;
                        if(audioDevice instanceof Speakerphone) {
                            System.out.println("hai");
                            speakerphone = (Speakerphone) audioDevice;
                            audioDevice = speakerphone;
                        }
                        else if(audioDevice instanceof WiredHeadset) {
                            System.out.println("hello");
                            wiredheadset = (WiredHeadset) audioDevice;
                            audioDevice = wiredheadset;
                        }
                        else if(audioDevice instanceof Earpiece) {
                            System.out.println("yay");
                            speakerphone = (Speakerphone) audioDevice;
                            audioDevice = speakerphone;
                        }

                        System.out.println("audioDevice000");
                        System.out.println(audioDevice);

                        updateAudioDeviceIcon(audioDevice);

                        System.out.println("audioDevice999");
                        System.out.println(audioDevice);
                        return Unit.INSTANCE;
                    });

            List<AudioDevice> availableAudioDevices = audioSwitch.getAvailableAudioDevices();
            Speakerphone speakerphone = null;
            WiredHeadset wiredheadset = null;
            for (AudioDevice audioDevice : availableAudioDevices) {
                Log.d(TAG, "Available Audio devices newww- " + audioDevice.getName());
                if(audioDevice instanceof Speakerphone) {
                    speakerphone = (Speakerphone) audioDevice;
                    System.out.println("audioDevice Speakerphone newww");
                    System.out.println(audioDevice);
                }
                if(audioDevice instanceof WiredHeadset) {
                    wiredheadset = (WiredHeadset) audioDevice;
                    System.out.println("audioDevice WiredHeadset newww");
                    System.out.println(audioDevice);
                }
            }

            if (cameraAndMicPermissionGranted) {
                createAudioAndVideoTracks();
            } else {
                Toast.makeText(this, R.string.permissions_needed, Toast.LENGTH_LONG).show();
            }
        }
    }

    @Override
    protected void onPause() {
        System.out.println("onPause fg to bg");

        if (localVideoTrack != null) {
            if (localVideoTrack != null) {
                localVideoTrack.enable(false); // Mute video (stops sending video frames)
            }
        }

        super.onPause();
    }

    @Override
    protected void onDestroy() {
        System.out.println("onDestroy");
               System.out.println(room);

        /*
         * Tear down audio management and restore previous volume stream
         */
        audioSwitch.stop();
        setVolumeControlStream(savedVolumeControlStream);

        /*
         * Always disconnect from the room before leaving the Activity to
         * ensure any memory allocated to the Room resource is freed.
         */
        if (room != null && room.getState() != Room.State.DISCONNECTED) {
            room.disconnect();
            finish();
            disconnectedFromOnDestroy = true;
        }

        /*
         * Release the local audio and video tracks ensuring any memory allocated to audio
         * or video is freed.
         */
        if (localAudioTrack != null) {
            localAudioTrack.release();
            localAudioTrack = null;
        }
        if (localVideoTrack != null) {
            localVideoTrack.release();
            localVideoTrack = null;
        }

        super.onDestroy();
    }

    private boolean checkPermissions(String[] permissions) {
        boolean shouldCheck = true;
        for (String permission : permissions) {
            shouldCheck &=
                    (PackageManager.PERMISSION_GRANTED
                            == ContextCompat.checkSelfPermission(this, permission));
        }
        return shouldCheck;
    }

    private void requestPermissions(String[] permissions) {
        boolean displayRational = false;
        for (String permission : permissions) {
            displayRational |=
                    ActivityCompat.shouldShowRequestPermissionRationale(this, permission);
        }
        if (displayRational) {
            Toast.makeText(this, R.string.permissions_needed, Toast.LENGTH_LONG).show();
        } else {
            ActivityCompat.requestPermissions(
                    this, permissions, CAMERA_MIC_PERMISSION_REQUEST_CODE);
        }
    }

    private boolean checkPermissionForCameraAndMicrophone() {
        return checkPermissions(
                new String[] {Manifest.permission.CAMERA, Manifest.permission.RECORD_AUDIO});
    }

    private void requestPermissionForCameraMicrophoneAndBluetooth() {
        String[] permissionsList;
        if (android.os.Build.VERSION.SDK_INT >= android.os.Build.VERSION_CODES.S) {
            permissionsList =
                    new String[] {
                            Manifest.permission.CAMERA,
                            Manifest.permission.RECORD_AUDIO,
                            Manifest.permission.BLUETOOTH_CONNECT
                    };
        } else {
            permissionsList =
                    new String[] {Manifest.permission.CAMERA, Manifest.permission.RECORD_AUDIO};
        }
        requestPermissions(permissionsList);
    }

    private void createAudioAndVideoTracks() {
        // Share your microphone
        localAudioTrack = LocalAudioTrack.create(this, true, LOCAL_AUDIO_TRACK_NAME);

        // Share your camera
        cameraCapturerCompat =
                new CameraCapturerCompat(this, CameraCapturerCompat.Source.FRONT_CAMERA);
        localVideoTrack =
                LocalVideoTrack.create(this, true, cameraCapturerCompat, LOCAL_VIDEO_TRACK_NAME);
        primaryVideoView.setMirror(true);
        localVideoTrack.addSink(primaryVideoView);
        localVideoView = primaryVideoView;
    }

    private void connectToRoom(String RoomName) {
        Log.d("CONNECTRoomName",RoomName);
        if (roomName == null) {
            Log.d("VideoCallTwilio", "Room name is null, cannot connect.");
            return;
        }
        System.out.println("CONNECT roomToken");
        System.out.println(roomToken);
        String stringWithoutQuotes = roomToken.replace("\"", "");
        System.out.println("CONNECT stringWithoutQuotes");
        System.out.println(stringWithoutQuotes);
        System.out.println("toString");
        System.out.println(RoomName.toString());

        audioSwitch.activate();
        ConnectOptions.Builder connectOptionsBuilder =
                new ConnectOptions.Builder(stringWithoutQuotes).roomName(RoomName);
        /*
         * Add local audio track to connect options to share with participants.
         */
        if (localAudioTrack != null) {
            connectOptionsBuilder.audioTracks(Collections.singletonList(localAudioTrack));
        }

        /*
         * Add local video track to connect options to share with participants.
         */
        if (localVideoTrack != null) {
            System.out.println("localVideoTrack connection======");
            connectOptionsBuilder.videoTracks(Collections.singletonList(localVideoTrack));
        }

        /*
         * Set the preferred audio and video codec for media.
         */
        connectOptionsBuilder.preferAudioCodecs(Collections.singletonList(audioCodec));
        connectOptionsBuilder.preferVideoCodecs(Collections.singletonList(videoCodec));

        /*
         * Set the sender side encoding parameters.
         */

        Log.d("encodingParameters", String.valueOf(encodingParameters));
        if (encodingParameters != null) {
            connectOptionsBuilder.encodingParameters(encodingParameters);
        } else {
            Log.e("VideoCallTwilio", "Encoding parameters are null.");
        }

        /*
         * Toggles automatic track subscription. If set to false, the LocalParticipant will receive
         * notifications of track publish events, but will not automatically subscribe to them. If
         * set to true, the LocalParticipant will automatically subscribe to tracks as they are
         * published. If unset, the default is true. Note: This feature is only available for Group
         * Rooms. Toggling the flag in a P2P room does not modify subscription behavior.
         */
        connectOptionsBuilder.enableAutomaticSubscription(enableAutomaticSubscription);

        room = Video.connect(this, connectOptionsBuilder.build(), roomListener());
        setDisconnectAction();
    }

    /*
     * The initial state when there is no active room.
     */
    private void intializeUI() {
        connectActionFab.setImageDrawable(
                ContextCompat.getDrawable(this, R.drawable.ic_call_end_white_24px));
        connectActionFab.show();
        connectActionFab.setOnClickListener(disconnectClickListener());
        switchCameraActionFab.show();
        switchCameraActionFab.setOnClickListener(switchCameraClickListener());
        localVideoActionFab.show();
        localVideoActionFab.setOnClickListener(localVideoClickListener());
        muteActionFab.show();
        muteActionFab.setOnClickListener(muteClickListener());
    }

    /*
     * Show the current available audio devices.
     */
    private void showAudioDevices() {
        AudioDevice selectedDevice = audioSwitch.getSelectedAudioDevice();
        List<AudioDevice> availableAudioDevices = audioSwitch.getAvailableAudioDevices();

        if (selectedDevice != null) {
            int selectedDeviceIndex = availableAudioDevices.indexOf(selectedDevice);

            ArrayList<String> audioDeviceNames = new ArrayList<>();
            for (AudioDevice a : availableAudioDevices) {
                audioDeviceNames.add(a.getName());
                System.out.println("a.getName()");
                System.out.println(a.getName());
            }

            new AlertDialog.Builder(this)
                    .setTitle(R.string.room_screen_select_device)
                    .setSingleChoiceItems(
                            audioDeviceNames.toArray(new CharSequence[0]),
                            selectedDeviceIndex,
                            (dialog, index) -> {
                                dialog.dismiss();
                                AudioDevice selectedAudioDevice = availableAudioDevices.get(index);
                                updateAudioDeviceIcon(selectedAudioDevice);
                                System.out.println("selectedAudioDevice");
                                System.out.println(selectedAudioDevice);
                                audioSwitch.selectDevice(selectedAudioDevice);
                            })
                    .create()
                    .show();
        }
    }

    /*
     * Update the menu icon based on the currently selected audio device.
     */
    private void updateAudioDeviceIcon(AudioDevice selectedAudioDevice) {
        if (null != audioDeviceMenuItem) {
            int audioDeviceMenuIcon = R.drawable.ic_phonelink_ring_white_24dp;

            if (selectedAudioDevice instanceof Speakerphone) {
                audioDeviceMenuIcon = R.drawable.ic_volume_up_white_24dp;
                audioSwitch.selectDevice(selectedAudioDevice);
            }
            else if (selectedAudioDevice instanceof WiredHeadset) {
                audioDeviceMenuIcon = R.drawable.ic_headset_mic_white_24dp;
                audioSwitch.selectDevice(selectedAudioDevice);
            }
            else if (selectedAudioDevice instanceof BluetoothHeadset) {
                audioDeviceMenuIcon = R.drawable.ic_bluetooth_white_24dp;
                audioSwitch.selectDevice(selectedAudioDevice);
            }

          /*  else if (selectedAudioDevice instanceof Earpiece) {
                audioDeviceMenuIcon = R.drawable.ic_phonelink_ring_white_24dp;
            }*/

            audioDeviceMenuItem.setIcon(audioDeviceMenuIcon);
        }
    }

    /*
     * Get the preferred audio codec from shared preferences
     */
    private AudioCodec getAudioCodecPreference(String key, String defaultValue) {
        final String audioCodecName = preferences.getString(key, defaultValue);

        switch (audioCodecName) {
            case OpusCodec.NAME:
                return new OpusCodec();
            case PcmaCodec.NAME:
                return new PcmaCodec();
            case PcmuCodec.NAME:
                return new PcmuCodec();
            case G722Codec.NAME:
                return new G722Codec();
            default:
                return new OpusCodec();
        }
    }

    /*
     * Get the preferred video codec from shared preferences
     */
    private VideoCodec getVideoCodecPreference(String key, String defaultValue) {
        final String videoCodecName = preferences.getString(key, defaultValue);

        switch (videoCodecName) {
            case Vp8Codec.NAME:
                boolean simulcast =
                        preferences.getBoolean(
                                SettingsActivity.PREF_VP8_SIMULCAST,
                                SettingsActivity.PREF_VP8_SIMULCAST_DEFAULT);
                return new Vp8Codec(simulcast);
            case H264Codec.NAME:
                return new H264Codec();
            case Vp9Codec.NAME:
                return new Vp9Codec();
            default:
                return new Vp8Codec();
        }
    }

    private boolean getAutomaticSubscriptionPreference(String key, boolean defaultValue) {
        return preferences.getBoolean(key, defaultValue);
    }

    private EncodingParameters getEncodingParameters() {
        final int maxAudioBitrate =
                Integer.parseInt(
                        preferences.getString(
                                SettingsActivity.PREF_SENDER_MAX_AUDIO_BITRATE,
                                SettingsActivity.PREF_SENDER_MAX_AUDIO_BITRATE_DEFAULT));
        final int maxVideoBitrate =
                Integer.parseInt(
                        preferences.getString(
                                SettingsActivity.PREF_SENDER_MAX_VIDEO_BITRATE,
                                SettingsActivity.PREF_SENDER_MAX_VIDEO_BITRATE_DEFAULT));

        return new EncodingParameters(maxAudioBitrate, maxVideoBitrate);
    }

    /*
     * The actions performed during disconnect.
     */
    private void setDisconnectAction() {
        connectActionFab.setImageDrawable(
                ContextCompat.getDrawable(this, R.drawable.ic_call_end_white_24px));
        connectActionFab.show();
        connectActionFab.setOnClickListener(disconnectClickListener());
    }

    /*
     * Called when remote participant joins the room
     */
    @SuppressLint("SetTextI18n")
    private void addRemoteParticipant(RemoteParticipant remoteParticipant) {
        /*
         * This app only displays video for one additional participant per Room
         */
        Log.d("addRemoteParticipant", String.valueOf(remoteParticipant));

        if (thumbnailVideoView.getVisibility() == View.VISIBLE) {
            Snackbar.make(
                            connectActionFab,
                            "Multiple participants are not currently support in this UI",
                            Snackbar.LENGTH_LONG)
                    .setAction("Action", null)
                    .show();
            return;
        }
        remoteParticipantIdentity = remoteParticipant.getIdentity();

        /*
         * Add remote participant renderer
         */
        if (remoteParticipant.getRemoteVideoTracks().size() > 0) {
            RemoteVideoTrackPublication remoteVideoTrackPublication =
                    remoteParticipant.getRemoteVideoTracks().get(0);

            /*
             * Only render video tracks that are subscribed to
             */
            if (remoteVideoTrackPublication.isTrackSubscribed()) {
                addRemoteParticipantVideo(remoteVideoTrackPublication.getRemoteVideoTrack());
            }
        }

        /*
         * Start listening for participant events
         */
        remoteParticipant.setListener(remoteParticipantListener());
    }

    /*
     * Set primary view as renderer for participant video track
     */
    private void addRemoteParticipantVideo(VideoTrack videoTrack) {
        moveLocalVideoToThumbnailView();
        primaryVideoView.setMirror(false);
        videoTrack.addSink(primaryVideoView);
    }

    private void moveLocalVideoToThumbnailView() {
        if (thumbnailVideoView.getVisibility() == View.GONE) {
            thumbnailVideoView.setVisibility(View.VISIBLE);
            localVideoTrack.removeSink(primaryVideoView);
            localVideoTrack.addSink(thumbnailVideoView);
            localVideoView = thumbnailVideoView;
            thumbnailVideoView.setMirror(
                    cameraCapturerCompat.getCameraSource()
                            == CameraCapturerCompat.Source.FRONT_CAMERA);
        }
    }

    /*
     * Called when remote participant leaves the room
     */
    @SuppressLint("SetTextI18n")
    private void removeRemoteParticipant(RemoteParticipant remoteParticipant) {
        if (!remoteParticipant.getIdentity().equals(remoteParticipantIdentity)) {
            return;
        }

        /*
         * Remove remote participant renderer
         */
        if (!remoteParticipant.getRemoteVideoTracks().isEmpty()) {
            RemoteVideoTrackPublication remoteVideoTrackPublication =
                    remoteParticipant.getRemoteVideoTracks().get(0);

            /*
             * Remove video only if subscribed to participant track
             */
            if (remoteVideoTrackPublication.isTrackSubscribed()) {
                removeParticipantVideo(remoteVideoTrackPublication.getRemoteVideoTrack());
            }
        }
        moveLocalVideoToPrimaryView();
    }

    private void removeParticipantVideo(VideoTrack videoTrack) {
        videoTrack.removeSink(primaryVideoView);
    }

    private void moveLocalVideoToPrimaryView() {
        if (thumbnailVideoView.getVisibility() == View.VISIBLE) {
            thumbnailVideoView.setVisibility(View.GONE);
            if (localVideoTrack != null) {
                localVideoTrack.removeSink(thumbnailVideoView);
                localVideoTrack.addSink(primaryVideoView);
            }
            localVideoView = primaryVideoView;
            primaryVideoView.setMirror(
                    cameraCapturerCompat.getCameraSource()
                            == CameraCapturerCompat.Source.FRONT_CAMERA);
        }
    }

    /*
     * Room events listener
     */
    @SuppressLint("SetTextI18n")
    private Room.Listener roomListener() {
        return new Room.Listener() {
            @Override
            public void onConnected(Room room) {
                Log.d("roomListeneronConnected", String.valueOf(room));
                localParticipant = room.getLocalParticipant();
                remoteParticipantval =  room.getRemoteParticipants();
                Log.d("remotepartys", String.valueOf(remoteParticipantval));

                if(remoteParticipantval.size() == 0)
                {
                    Log.d("size", String.valueOf(remoteParticipantval.size()));
                    room.disconnect();
                    finish();
                    Toast.makeText(((MyApplication) getApplication()).getLatestActivity(), "Video Call Expired", Toast.LENGTH_LONG).show();
                }
                else {
                    System.out.println(localParticipant);
                    setTitle(room.getName());

                for (RemoteParticipant remoteParticipant : room.getRemoteParticipants()) {
                    addRemoteParticipant(remoteParticipant);
                    System.out.println("roomListenerremoteParticipant");
                    System.out.println(remoteParticipant);
                    break;
                }
                    editor.putBoolean("onCall", true);
                    editor.apply();
                }
            }

            @Override
            public void onReconnecting(
                    @NonNull Room room, @NonNull TwilioException twilioException) {
                System.out.println("onReconnecting");
                System.out.println(twilioException);
                reconnectingProgressBar.setVisibility(View.VISIBLE);
            }

            @Override
            public void onReconnected(@NonNull Room room) {
                System.out.println("onReconnected");
                System.out.println(room);
                reconnectingProgressBar.setVisibility(View.GONE);
            }

            @Override
            public void onConnectFailure(Room room, TwilioException e) {
                System.out.println("onConnectFailure Exception");
                System.out.println(e);
                audioSwitch.deactivate();
                intializeUI();
            }

            @Override
            public void onDisconnected(Room room, TwilioException e) {
                System.out.println("onDisconnectedException");
                System.out.println(e);
                System.out.println("onDisconnectedroom");
                System.out.println(room);
                System.out.println("oncall5555");
                editor.putBoolean("onCall",false );
                editor.apply();
                localParticipant = null;
                reconnectingProgressBar.setVisibility(View.GONE);
                VideoCallTwilio.this.room = null;
                finish();

                // Only reinitialize the UI if disconnect was not called from onDestroy()
                if (!disconnectedFromOnDestroy) {
                    audioSwitch.deactivate();
                    intializeUI();
                    moveLocalVideoToPrimaryView();
                }
            }

            @Override
            public void onParticipantConnected(Room room, RemoteParticipant remoteParticipant) {

                System.out.println("remoteParticipantonParticipantConnected");
                System.out.println(remoteParticipant);
                addRemoteParticipant(remoteParticipant);
                remoteParticipantvalue= remoteParticipant;
                Log.d("remoteParticipantvaluecn", String.valueOf(remoteParticipantvalue));

            }

            @Override
            public void onParticipantDisconnected(Room room, RemoteParticipant remoteParticipant) {
                removeRemoteParticipant(remoteParticipant);
                System.out.println("remoteParticipantonParticipantDisconnected");
                System.out.println(remoteParticipant.getIdentity());
                Log.d("localPartici", String.valueOf(localParticipant));
                remoteParticipantvalue= remoteParticipant;
                Log.d("remoteParticipantvaluedc", String.valueOf(remoteParticipantvalue));
                    room.disconnect();
                    System.out.println("oncall66666");
                editor.putBoolean("onCall",false );
                editor.apply();
                   finish();

                if(remoteParticipant == null)
                {
                    System.out.println("oncallremoteParticipant");
                    System.out.println(remoteParticipant);
                    room.disconnect();
                    finish();
                }
            }

            @Override
            public void onRecordingStarted(Room room) {
                /*
                 * Indicates when media shared to a Room is being recorded. Note that
                 * recording is only available in our Group Rooms developer preview.
                 */
                Log.d(TAG, "onRecordingStarted");
            }

            @Override
            public void onRecordingStopped(Room room) {
                /*
                 * Indicates when media shared to a Room is no longer being recorded. Note that
                 * recording is only available in our Group Rooms developer preview.
                 */
                Log.d(TAG, "onRecordingStopped");
            }
        };
    }

    @SuppressLint("SetTextI18n")
    private RemoteParticipant.Listener remoteParticipantListener() {
        return new RemoteParticipant.Listener() {
            @Override
            public void onAudioTrackPublished(
                    RemoteParticipant remoteParticipant,
                    RemoteAudioTrackPublication remoteAudioTrackPublication) {
                Log.d("RemoteDebug", "Audio track published by " + remoteParticipant.getIdentity());
                Log.i(
                        TAG,
                        String.format(
                                "onAudioTrackPublished: "
                                        + "[RemoteParticipant: identity=%s], "
                                        + "[RemoteAudioTrackPublication: sid=%s, enabled=%b, "
                                        + "subscribed=%b, name=%s]",
                                remoteParticipant.getIdentity(),
                                remoteAudioTrackPublication.getTrackSid(),
                                remoteAudioTrackPublication.isTrackEnabled(),
                                remoteAudioTrackPublication.isTrackSubscribed(),
                                remoteAudioTrackPublication.getTrackName()));
            }

            @Override
            public void onAudioTrackUnpublished(
                    RemoteParticipant remoteParticipant,
                    RemoteAudioTrackPublication remoteAudioTrackPublication) {
                Log.i(
                        TAG,
                        String.format(
                                "onAudioTrackUnpublished: "
                                        + "[RemoteParticipant: identity=%s], "
                                        + "[RemoteAudioTrackPublication: sid=%s, enabled=%b, "
                                        + "subscribed=%b, name=%s]",
                                remoteParticipant.getIdentity(),
                                remoteAudioTrackPublication.getTrackSid(),
                                remoteAudioTrackPublication.isTrackEnabled(),
                                remoteAudioTrackPublication.isTrackSubscribed(),
                                remoteAudioTrackPublication.getTrackName()));
            }

            @Override
            public void onDataTrackPublished(
                    RemoteParticipant remoteParticipant,
                    RemoteDataTrackPublication remoteDataTrackPublication) {
                Log.i(
                        TAG,
                        String.format(
                                "onDataTrackPublished: "
                                        + "[RemoteParticipant: identity=%s], "
                                        + "[RemoteDataTrackPublication: sid=%s, enabled=%b, "
                                        + "subscribed=%b, name=%s]",
                                remoteParticipant.getIdentity(),
                                remoteDataTrackPublication.getTrackSid(),
                                remoteDataTrackPublication.isTrackEnabled(),
                                remoteDataTrackPublication.isTrackSubscribed(),
                                remoteDataTrackPublication.getTrackName()));
            }

            @Override
            public void onDataTrackUnpublished(
                    RemoteParticipant remoteParticipant,
                    RemoteDataTrackPublication remoteDataTrackPublication) {
                Log.i(
                        TAG,
                        String.format(
                                "onDataTrackUnpublished: "
                                        + "[RemoteParticipant: identity=%s], "
                                        + "[RemoteDataTrackPublication: sid=%s, enabled=%b, "
                                        + "subscribed=%b, name=%s]",
                                remoteParticipant.getIdentity(),
                                remoteDataTrackPublication.getTrackSid(),
                                remoteDataTrackPublication.isTrackEnabled(),
                                remoteDataTrackPublication.isTrackSubscribed(),
                                remoteDataTrackPublication.getTrackName()));
            }

            @Override
            public void onVideoTrackPublished(
                    RemoteParticipant remoteParticipant,
                    RemoteVideoTrackPublication remoteVideoTrackPublication) {
                System.out.println("onVideoTrackPublished--------");

                Log.i(
                        TAG,
                        String.format(
                                "onVideoTrackPublished: "
                                        + "[RemoteParticipant: identity=%s], "
                                        + "[RemoteVideoTrackPublication: sid=%s, enabled=%b, "
                                        + "subscribed=%b, name=%s]",
                                remoteParticipant.getIdentity(),
                                remoteVideoTrackPublication.getTrackSid(),
                                remoteVideoTrackPublication.isTrackEnabled(),
                                remoteVideoTrackPublication.isTrackSubscribed(),
                                remoteVideoTrackPublication.getTrackName()));
            }

            @Override
            public void onVideoTrackUnpublished(
                    RemoteParticipant remoteParticipant,
                    RemoteVideoTrackPublication remoteVideoTrackPublication) {


                System.out.println("onVideoTrackUnpublished-------");
                Log.i(
                        TAG,
                        String.format(
                                "c: "
                                        + "[RemoteParticipant: identity=%s], "
                                        + "[RemoteVideoTrackPublication: sid=%s, enabled=%b, "
                                        + "subscribed=%b, name=%s]",
                                remoteParticipant.getIdentity(),
                                remoteVideoTrackPublication.getTrackSid(),
                                remoteVideoTrackPublication.isTrackEnabled(),
                                remoteVideoTrackPublication.isTrackSubscribed(),
                                remoteVideoTrackPublication.getTrackName()));
            }

            @Override
            public void onAudioTrackSubscribed(
                    RemoteParticipant remoteParticipant,
                    RemoteAudioTrackPublication remoteAudioTrackPublication,
                    RemoteAudioTrack remoteAudioTrack) {
                Log.d("RemoteDebug", "Subscribed to audio from: " + remoteParticipant.getIdentity());
                Log.i(
                        TAG,
                        String.format(
                                "onAudioTrackSubscribed: "
                                        + "[RemoteParticipant: identity=%s], "
                                        + "[RemoteAudioTrack: enabled=%b, playbackEnabled=%b, name=%s]",
                                remoteParticipant.getIdentity(),
                                remoteAudioTrack.isEnabled(),
                                remoteAudioTrack.isPlaybackEnabled(),
                                remoteAudioTrack.getName()));
            }

            @Override
            public void onAudioTrackUnsubscribed(
                    RemoteParticipant remoteParticipant,
                    RemoteAudioTrackPublication remoteAudioTrackPublication,
                    RemoteAudioTrack remoteAudioTrack) {
                Log.i(
                        TAG,
                        String.format(
                                "onAudioTrackUnsubscribed: "
                                        + "[RemoteParticipant: identity=%s], "
                                        + "[RemoteAudioTrack: enabled=%b, playbackEnabled=%b, name=%s]",
                                remoteParticipant.getIdentity(),
                                remoteAudioTrack.isEnabled(),
                                remoteAudioTrack.isPlaybackEnabled(),
                                remoteAudioTrack.getName()));
            }

            @Override
            public void onAudioTrackSubscriptionFailed(
                    RemoteParticipant remoteParticipant,
                    RemoteAudioTrackPublication remoteAudioTrackPublication,
                    TwilioException twilioException) {
                Log.i(
                        TAG,
                        String.format(
                                "onAudioTrackSubscriptionFailed: "
                                        + "[RemoteParticipant: identity=%s], "
                                        + "[RemoteAudioTrackPublication: sid=%b, name=%s]"
                                        + "[TwilioException: code=%d, message=%s]",
                                remoteParticipant.getIdentity(),
                                remoteAudioTrackPublication.getTrackSid(),
                                remoteAudioTrackPublication.getTrackName(),
                                twilioException.getCode(),
                                twilioException.getMessage()));
            }

            @Override
            public void onDataTrackSubscribed(
                    RemoteParticipant remoteParticipant,
                    RemoteDataTrackPublication remoteDataTrackPublication,
                    RemoteDataTrack remoteDataTrack) {
                Log.i(
                        TAG,
                        String.format(
                                "onDataTrackSubscribed: "
                                        + "[RemoteParticipant: identity=%s], "
                                        + "[RemoteDataTrack: enabled=%b, name=%s]",
                                remoteParticipant.getIdentity(),
                                remoteDataTrack.isEnabled(),
                                remoteDataTrack.getName()));
            }

            @Override
            public void onDataTrackUnsubscribed(
                    RemoteParticipant remoteParticipant,
                    RemoteDataTrackPublication remoteDataTrackPublication,
                    RemoteDataTrack remoteDataTrack) {
                Log.i(
                        TAG,
                        String.format(
                                "onDataTrackUnsubscribed: "
                                        + "[RemoteParticipant: identity=%s], "
                                        + "[RemoteDataTrack: enabled=%b, name=%s]",
                                remoteParticipant.getIdentity(),
                                remoteDataTrack.isEnabled(),
                                remoteDataTrack.getName()));
            }

            @Override
            public void onDataTrackSubscriptionFailed(
                    RemoteParticipant remoteParticipant,
                    RemoteDataTrackPublication remoteDataTrackPublication,
                    TwilioException twilioException) {
                Log.i(
                        TAG,
                        String.format(
                                "onDataTrackSubscriptionFailed: "
                                        + "[RemoteParticipant: identity=%s], "
                                        + "[RemoteDataTrackPublication: sid=%b, name=%s]"
                                        + "[TwilioException: code=%d, message=%s]",
                                remoteParticipant.getIdentity(),
                                remoteDataTrackPublication.getTrackSid(),
                                remoteDataTrackPublication.getTrackName(),
                                twilioException.getCode(),
                                twilioException.getMessage()));
            }

            @Override
            public void onVideoTrackSubscribed(
                    RemoteParticipant remoteParticipant,
                    RemoteVideoTrackPublication remoteVideoTrackPublication,
                    RemoteVideoTrack remoteVideoTrack) {
                Log.i(
                        TAG,
                        String.format(
                                "onVideoTrackSubscribed: "
                                        + "[RemoteParticipant: identity=%s], "
                                        + "[RemoteVideoTrack: enabled=%b, name=%s]",
                                remoteParticipant.getIdentity(),
                                remoteVideoTrack.isEnabled(),
                                remoteVideoTrack.getName()));
                addRemoteParticipantVideo(remoteVideoTrack);
            }

            @Override
            public void onVideoTrackUnsubscribed(
                    RemoteParticipant remoteParticipant,
                    RemoteVideoTrackPublication remoteVideoTrackPublication,
                    RemoteVideoTrack remoteVideoTrack) {
                Log.i(
                        TAG,
                        String.format(
                                "onVideoTrackUnsubscribed: "
                                        + "[RemoteParticipant: identity=%s], "
                                        + "[RemoteVideoTrack: enabled=%b, name=%s]",
                                remoteParticipant.getIdentity(),
                                remoteVideoTrack.isEnabled(),
                                remoteVideoTrack.getName()));
                removeParticipantVideo(remoteVideoTrack);

            }

            @Override
            public void onVideoTrackSubscriptionFailed(
                    RemoteParticipant remoteParticipant,
                    RemoteVideoTrackPublication remoteVideoTrackPublication,
                    TwilioException twilioException) {
                Log.i(
                        TAG,
                        String.format(
                                "onVideoTrackSubscriptionFailed: "
                                        + "[RemoteParticipant: identity=%s], "
                                        + "[RemoteVideoTrackPublication: sid=%b, name=%s]"
                                        + "[TwilioException: code=%d, message=%s]",
                                remoteParticipant.getIdentity(),
                                remoteVideoTrackPublication.getTrackSid(),
                                remoteVideoTrackPublication.getTrackName(),
                                twilioException.getCode(),
                                twilioException.getMessage()));
                Snackbar.make(
                                connectActionFab,
                                String.format(
                                        "Failed to subscribe to %s video track",
                                        remoteParticipant.getIdentity()),
                                Snackbar.LENGTH_LONG)
                        .show();
            }

            @Override
            public void onAudioTrackEnabled(
                    RemoteParticipant remoteParticipant,
                    RemoteAudioTrackPublication remoteAudioTrackPublication) {
                Log.d("onAudioTrackEnabled","onAudioTrackEnabled");
            }

            @Override
            public void onAudioTrackDisabled(
                    RemoteParticipant remoteParticipant,

                    RemoteAudioTrackPublication remoteAudioTrackPublication) {
                Log.d("onAudioTrackDisabled","onAudioTrackDisabled");
            }

            @Override
            public void onVideoTrackEnabled(
                    RemoteParticipant remoteParticipant,
                    RemoteVideoTrackPublication remoteVideoTrackPublication) {

                Log.d("onVideoTrackEnabled","onVideoTrackEnabled");
                runOnUiThread(() -> {
                    // Show video view again
                    primaryVideoView.setVisibility(View.VISIBLE);
                    // Hide placeholder image
                    remotePlaceHolderImage.setVisibility(View.GONE);
                });
            }

            @Override
            public void onVideoTrackDisabled(
                    RemoteParticipant remoteParticipant,
                    RemoteVideoTrackPublication remoteVideoTrackPublication) {

                Log.d("onVideoTrackDisabled", "onVideoTrackDisabled");
                runOnUiThread(() -> {
                    // Hide the primary video (remote video)
                    primaryVideoView.setVisibility(View.INVISIBLE);
                    // Ensure placeholder is only covering primary video, NOT the thumbnail
                    remotePlaceHolderImage.getLayoutParams().width = primaryVideoView.getWidth();
                    remotePlaceHolderImage.getLayoutParams().height = primaryVideoView.getHeight();
                    // Align placeholder with the primary video view's position
                    remotePlaceHolderImage.setX(primaryVideoView.getX());
                    remotePlaceHolderImage.setY(primaryVideoView.getY());
                    // Show placeholder only for primary video
                    remotePlaceHolderImage.setVisibility(View.VISIBLE);
                    remotePlaceHolderImage.requestLayout();
                    //  Ensure the thumbnail video stays visible
                    thumbnailVideoView.setVisibility(View.VISIBLE);
                    thumbnailVideoView.bringToFront();
                });
            }
        };
    }

    private DialogInterface.OnClickListener connectClickListener(final EditText roomEditText) {
        return (dialog, which) -> {
            /*
             * Connect to room
             */

            System.out.println(roomEditText.getText().toString());
        };
    }

    private View.OnClickListener disconnectClickListener() {
        return v -> {
            /*
             * Disconnect from room
             */
            if (room != null) {
                room.disconnect();
                Log.d("roomdisconnct","roomdisconnct");
                finish();
            }
        };
    }

    private DialogInterface.OnClickListener cancelConnectDialogClickListener() {
        return (dialog, which) -> {
            intializeUI();
            connectDialog.dismiss();
        };
    }

    private View.OnClickListener switchCameraClickListener() {
        return v -> {
            if (cameraCapturerCompat != null) {
                CameraCapturerCompat.Source cameraSource = cameraCapturerCompat.getCameraSource();
                cameraCapturerCompat.switchCamera();
                if (thumbnailVideoView.getVisibility() == View.VISIBLE) {
                    thumbnailVideoView.setMirror(
                            cameraSource == CameraCapturerCompat.Source.BACK_CAMERA);
                } else {
                    primaryVideoView.setMirror(
                            cameraSource == CameraCapturerCompat.Source.BACK_CAMERA);
                }
            }
        };
    }

    private View.OnClickListener localVideoClickListener() {
        return v -> {
            if (localVideoTrack != null) {
                boolean enable = !localVideoTrack.isEnabled();
                localVideoTrack.enable(enable);
                Log.d("AudioCase", "Video track toggled. Now enabled: " + enable);
                // Handle video UI state changes
                int icon;
                if (enable) {
                    icon = R.drawable.ic_videocam_white_24dp;
                    switchCameraActionFab.show();
                    runOnUiThread(() -> {
                        localPlaceHolderImage.setVisibility(View.GONE);
                        thumbnailVideoView.setVisibility(View.VISIBLE);
                    });
                } else {
                    icon = R.drawable.ic_videocam_off_black_24dp;
                    switchCameraActionFab.hide();
                    runOnUiThread(() -> {
                        thumbnailVideoView.setVisibility(View.GONE);
                        localPlaceHolderImage.setVisibility(View.VISIBLE);
                        localPlaceHolderImage.bringToFront();
                    });
                }

                // Only update the video mute icon
                localVideoActionFab.setImageDrawable(
                        ContextCompat.getDrawable(VideoCallTwilio.this, icon));

                // KEY FIX: Ensure audio stays published when video is muted
                if (localAudioTrack != null && localAudioTrack.isEnabled()) {
                    Log.d("AudioCase", "Audio track isEnabled: true");
                    // Declare and initialize the audioTrackPublished flag
                    boolean audioTrackPublished = false;
                    Log.d("AudioCase", "localParticipant audioTracks size: " + localParticipant.getAudioTracks().size());
                    // Check if the audio track is already published
                    if (localParticipant != null) {
                        List<AudioTrackPublication> tracks = localParticipant.getAudioTracks();
                        for (AudioTrackPublication publication : tracks) {
                            Log.d("AudioCase", "Checking audio track: " + publication.getAudioTrack());
                            if (publication.getAudioTrack() == localAudioTrack) {
                                audioTrackPublished = true;
                                Log.d("AudioCase", "Matched localAudioTrack with published track.");
                                break;
                            }
                        }
                        Log.d("AudioCase", "localAudioTrack: " + localAudioTrack);
                    }

                    // If the audio track is not published, publish it
                    if (!audioTrackPublished) {
                        localParticipant.publishTrack(localAudioTrack);
                        Log.d("AudioCase", "Audio was unpublished. Re-publishing audio track.");
                    }
                }

                for (AudioTrackPublication publication : localParticipant.getAudioTracks()) {
                    if (publication.isTrackEnabled() && publication.getAudioTrack() != null) {
                        Log.d("AudioCase", "Confirmed local audio track is still active.");
                    }
                }

            }
        };
    }

    private View.OnClickListener muteClickListener() {
        return v -> {
            if (localAudioTrack != null) {
                boolean newEnabledState = !localAudioTrack.isEnabled();
                localAudioTrack.enable(newEnabledState);
                // Update audio mute icon
                int icon = newEnabledState
                        ? R.drawable.ic_mic_white_24dp
                        : R.drawable.ic_mic_off_black_24dp;
                muteActionFab.setImageDrawable(ContextCompat.getDrawable(VideoCallTwilio.this, icon));
                Log.d("AudioFix", newEnabledState ? "Unmuted audio track." : "Muted audio track.");
                // Re-publish audio if it's unmuted
                if (newEnabledState && localParticipant != null) {
                    localParticipant.publishTrack(localAudioTrack);
                    Log.d("AudioFix", "Re-published audio track after unmuting.");
                }
            } else {
                // If localAudioTrack is null, create and publish the track
                localAudioTrack = LocalAudioTrack.create(VideoCallTwilio.this, true);
                if (localAudioTrack != null && localParticipant != null) {
                    localParticipant.publishTrack(localAudioTrack);
                    muteActionFab.setImageDrawable(ContextCompat.getDrawable(VideoCallTwilio.this, R.drawable.ic_mic_white_24dp));
                    Log.d("AudioFix", "Created and published audio track.");
                } else {
                    Log.e("AudioFix", "Failed to recreate audio track.");
                }
            }
        };
    }
}
