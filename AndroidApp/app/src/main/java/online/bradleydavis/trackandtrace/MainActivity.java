package online.bradleydavis.trackandtrace;

import androidx.annotation.NonNull;
import androidx.appcompat.app.AppCompatActivity;
import androidx.core.content.ContextCompat;
import androidx.localbroadcastmanager.content.LocalBroadcastManager;

import android.Manifest;
import android.content.BroadcastReceiver;
import android.content.ContentResolver;
import android.content.Context;
import android.content.Intent;
import android.content.IntentFilter;
import android.content.pm.PackageManager;
import android.database.Cursor;
import android.net.Uri;
import android.os.Bundle;
import android.os.Handler;
import android.util.Log;
import android.widget.ArrayAdapter;
import android.widget.ListView;
import android.widget.Toast;

import java.text.SimpleDateFormat;
import java.util.ArrayList;
import java.util.Calendar;
import java.util.Date;
import java.util.Locale;

public class MainActivity extends AppCompatActivity {

    ListView messages;
    ArrayAdapter arrayAdapter;
    BroadcastReceiver localBroadcastReceiver;
    StorageManager storageManager;
    private static final int READ_SMS_PERMISSIONS_REQUEST = 1;
    private static final int ACCESS_EXTERNAL_STORAGE_REQUEST = 2;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        storageManager = new StorageManager(this);

        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_main);

        messages = (ListView) findViewById(R.id.messages);
        arrayAdapter = new MessagesArrayAdapter(this, new ArrayList<SingleMessage>());
        messages.setAdapter(arrayAdapter);

        if (ContextCompat.checkSelfPermission(this, Manifest.permission.READ_SMS)
                != PackageManager.PERMISSION_GRANTED) {
            getPermissionToReadSMS();
        }

        if (ContextCompat.checkSelfPermission(this, Manifest.permission.READ_EXTERNAL_STORAGE)
                != PackageManager.PERMISSION_GRANTED) {
            getPermissionToAccessStorage();
        }

        if (ContextCompat.checkSelfPermission(this, Manifest.permission.READ_SMS)
                == PackageManager.PERMISSION_GRANTED &&
                ContextCompat.checkSelfPermission(this, Manifest.permission.READ_EXTERNAL_STORAGE)
                        == PackageManager.PERMISSION_GRANTED) {
            refreshSmsInbox();

            //register for updates when messages arrive
            localBroadcastReceiver = createReceiver();
            LocalBroadcastManager localBroadcastManager = LocalBroadcastManager.getInstance(this);
            localBroadcastManager.registerReceiver(localBroadcastReceiver, new IntentFilter("online.bradleydavis.TrackAndTrace"));

            //restart the activity after 10 minutes
            final Handler handler = new Handler();
            handler.postDelayed(new Runnable() {
                @Override
                public void run() {
                    recreate();
                }
            }, 600000);
        }
    }

    public void refreshSmsInbox() {
        storageManager.WriteToFile();

        Calendar calendar = Calendar.getInstance();
        calendar.add(Calendar.DATE, -22);
        Date twentyTwoDaysAgo = calendar.getTime();

        SimpleDateFormat timeConverter = new SimpleDateFormat("HH:mm", Locale.ENGLISH);
        SimpleDateFormat dateConverter = new SimpleDateFormat("dd/MM/yyyy", Locale.ENGLISH);

        ContentResolver contentResolver = getContentResolver();
        Cursor smsInboxCursor = contentResolver.query(Uri.parse("content://sms/inbox"), null, null, null, null);
        int indexID = smsInboxCursor.getColumnIndex("_id");
        int indexBody = smsInboxCursor.getColumnIndex("body");
        int indexAddress = smsInboxCursor.getColumnIndex("address");
        int indexTimeStamp = smsInboxCursor.getColumnIndex("date");

        if (indexBody < 0 || !smsInboxCursor.moveToFirst()) return;

        arrayAdapter.clear();
        do {
            //get message details
            long id = smsInboxCursor.getLong(indexID);
            String phoneNumber = smsInboxCursor.getString(indexAddress);
            String timeStamp = smsInboxCursor.getString(indexTimeStamp);
            Date dateStamp = new Date(Long.parseLong(timeStamp));
            String time = timeConverter.format(dateStamp);
            String date = dateConverter.format(dateStamp);
            String messageBody = smsInboxCursor.getString(indexBody);

            //if the message is over 22 days old, attempt to delete it
            if (dateStamp.before(twentyTwoDaysAgo)) {
                try {
                    contentResolver.delete(Uri.parse("content://sms/" + id), null, null);
                }
                catch (Exception e) {
                    Log.d("TrackAndTrace", e.getMessage());
                }
            }
            else {
                arrayAdapter.add(new SingleMessage(phoneNumber, date, time, messageBody));
            }
        } while (smsInboxCursor.moveToNext());
    }

    private BroadcastReceiver createReceiver() {
        return new BroadcastReceiver() {
            @Override
            public void onReceive(Context context, Intent intent) {
                refreshSmsInbox();
            }
        };
    }

    public void getPermissionToReadSMS() {
        if (ContextCompat.checkSelfPermission(this, Manifest.permission.READ_SMS)
                != PackageManager.PERMISSION_GRANTED) {
            if (shouldShowRequestPermissionRationale(Manifest.permission.READ_SMS)) {
                Toast.makeText(this, "Please allow permission!", Toast.LENGTH_SHORT).show();
            }
            requestPermissions(new String[]{Manifest.permission.READ_SMS},
                    READ_SMS_PERMISSIONS_REQUEST);
        }
    }

    public void getPermissionToAccessStorage() {
        if (ContextCompat.checkSelfPermission(this, Manifest.permission.READ_EXTERNAL_STORAGE)
                != PackageManager.PERMISSION_GRANTED) {
            if (shouldShowRequestPermissionRationale(Manifest.permission.READ_EXTERNAL_STORAGE)) {
                Toast.makeText(this, "Please allow permission!", Toast.LENGTH_SHORT).show();
            }
            requestPermissions(new String[] {Manifest.permission.READ_EXTERNAL_STORAGE,
                    Manifest.permission.WRITE_EXTERNAL_STORAGE},
                    ACCESS_EXTERNAL_STORAGE_REQUEST);
        }
    }

    @Override
    public void onRequestPermissionsResult(int requestCode,
                                           @NonNull String permissions[],
                                           @NonNull int[] grantResults) {
        // Make sure it's our original READ_CONTACTS request
        if (requestCode == READ_SMS_PERMISSIONS_REQUEST) {
            if (grantResults.length == 1 &&
                    grantResults[0] == PackageManager.PERMISSION_GRANTED) {
                Toast.makeText(this, "Read SMS permission granted", Toast.LENGTH_SHORT).show();
                refreshSmsInbox();
            }
            else {
                Toast.makeText(this, "Read SMS permission denied", Toast.LENGTH_SHORT).show();
            }

        }
        else if (requestCode == ACCESS_EXTERNAL_STORAGE_REQUEST) {
            if (grantResults.length == 1 &&
                    grantResults[0] == PackageManager.PERMISSION_GRANTED) {
                Toast.makeText(this, "Access external storage granted", Toast.LENGTH_SHORT).show();
            }
            else {
                Toast.makeText(this, "Access external storage denied", Toast.LENGTH_SHORT).show();
            }
        }
        else {
            super.onRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }

    @Override
    protected void onDestroy() {
        super.onDestroy();
        LocalBroadcastManager localBroadcastManager = LocalBroadcastManager.getInstance(this);
        localBroadcastManager.unregisterReceiver(localBroadcastReceiver);
    }
}