package online.bradleydavis.trackandtrace;

import androidx.annotation.NonNull;
import androidx.appcompat.app.AppCompatActivity;
import androidx.core.content.ContextCompat;
import androidx.localbroadcastmanager.content.LocalBroadcastManager;

import android.Manifest;
import android.content.BroadcastReceiver;
import android.content.ComponentName;
import android.content.ContentResolver;
import android.content.Context;
import android.content.Intent;
import android.content.IntentFilter;
import android.content.pm.PackageManager;
import android.database.Cursor;
import android.net.Uri;
import android.os.Bundle;
import android.os.Handler;
import android.provider.Telephony;
import android.util.Log;
import android.widget.ArrayAdapter;
import android.widget.ListView;
import android.widget.Toast;

import java.text.SimpleDateFormat;
import java.util.ArrayList;
import java.util.Calendar;
import java.util.Date;
import java.util.List;
import java.util.Locale;

/**
 * The activity class for the application.
 *
 * @author Bradley Davis
 */
public class MainActivity extends AppCompatActivity {

    ListView messages;
    ArrayAdapter arrayAdapter;
    BroadcastReceiver localBroadcastReceiver;
    StorageManager storageManager;
    private static final int DEFAULT_APPLICATION_REQUEST = 99;
    private static final int READ_SMS_PERMISSIONS_REQUEST = 1;
    private static final int ACCESS_EXTERNAL_STORAGE_REQUEST = 2;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        storageManager = new StorageManager(this);

        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_main);

        if (!isDefaultApp()){
            Intent setSmsAppIntent = new Intent(Telephony.Sms.Intents.ACTION_CHANGE_DEFAULT);
            setSmsAppIntent.putExtra(Telephony.Sms.Intents.EXTRA_PACKAGE_NAME, getPackageName());
            startActivityForResult(setSmsAppIntent, DEFAULT_APPLICATION_REQUEST);
        }

        //get the messages listview and add adapter.
        messages = (ListView) findViewById(R.id.messages);
        arrayAdapter = new MessagesArrayAdapter(this, new ArrayList<SingleMessage>());
        messages.setAdapter(arrayAdapter);

        //ensure that we have asked for the relevant permissions
        if (ContextCompat.checkSelfPermission(this, Manifest.permission.READ_SMS)
                != PackageManager.PERMISSION_GRANTED) {
            getPermissionToReadSMS();
        }
        if (ContextCompat.checkSelfPermission(this, Manifest.permission.READ_EXTERNAL_STORAGE)
                != PackageManager.PERMISSION_GRANTED) {
            getPermissionToAccessStorage();
        }

        //if we have all of the correct permissions, we can run
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

    /**
     * Method which refreshes the inbox. This is called when any text messages are received.
     */
    public void refreshSmsInbox() {
        //ensure that we also update the files on the device with the new messages.
        storageManager.UpdateFiles();

        Calendar calendar = Calendar.getInstance(); 
        calendar.add(Calendar.DATE, -21);
        Date twentyOneDaysAgo = calendar.getTime();
        SimpleDateFormat timeConverter = new SimpleDateFormat("HH:mm", Locale.ENGLISH);
        SimpleDateFormat dateConverter = new SimpleDateFormat("dd/MM/yyyy", Locale.ENGLISH);

        //retrieve messages from database.
        ContentResolver contentResolver = getContentResolver();
        Cursor smsInboxCursor = contentResolver.query(Uri.parse("content://sms/inbox"), null, null, null, null);
        int indexID = smsInboxCursor.getColumnIndex("_id");
        int indexBody = smsInboxCursor.getColumnIndex("body");
        int indexAddress = smsInboxCursor.getColumnIndex("address");
        int indexTimeStamp = smsInboxCursor.getColumnIndex("date");

        //start at the first message if there are any messages.
        if (indexBody < 0 || !smsInboxCursor.moveToFirst()) return;

        //fill the array adapter with the messages.
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

            //if the message is over 21 days old, don't show it in the list and attempt to delete it
            if (dateStamp.before(twentyOneDaysAgo)) {
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

        smsInboxCursor.close();
    }

    /**
     * Creates the local broadcast receiver which listens for the local broadcasts.
     * When a message is received, a local broadcast is sent out which this picks up.
     *
     * @return the broadcast receiver.
     */
    private BroadcastReceiver createReceiver() {
        return new BroadcastReceiver() {
            @Override
            public void onReceive(Context context, Intent intent) {
                refreshSmsInbox();
            }
        };
    }

    /**
     * Method to get permissions to Read the SMS messages
     */
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

    /**
     * Method to get permission to access local storage.
     */
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

    /**
     * Method to receive the outcome of the permissions requests and process them.
     *
     * @param requestCode the custom request code provided.
     * @param permissions the permissions requested
     * @param grantResults the results granted.
     */
    @Override
    public void onRequestPermissionsResult(int requestCode,
                                           @NonNull String permissions[],
                                           @NonNull int[] grantResults) {
        // Make sure it's our original READ_SMS request
        if (requestCode == READ_SMS_PERMISSIONS_REQUEST) {
            if (grantResults.length == 1 &&
                    grantResults[0] == PackageManager.PERMISSION_GRANTED) {
                Toast.makeText(this, "Read SMS permission granted", Toast.LENGTH_SHORT).show();
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

    /**
     * Method to check whether or not this application is the default messaging application.
     *
     * @return whether or not this application is the default messaging application.
     */
    private boolean isDefaultApp() {
        final IntentFilter filter = new IntentFilter(Intent.ACTION_MAIN);
        filter.addCategory(Intent.CATEGORY_HOME);

        List<IntentFilter> filters = new ArrayList<IntentFilter>();
        filters.add(filter);

        final String myPackageName = getPackageName();
        List<ComponentName> activities = new ArrayList<ComponentName>();
        final PackageManager packageManager = (PackageManager) getPackageManager();

        // You can use name of your package here as third argument
        packageManager.getPreferredActivities(filters, activities, null);

        for (ComponentName activity : activities) {
            if (myPackageName.equals(activity.getPackageName())) {
                return true;
            }
        }
        return false;
    }

    /**
     * Method which is called when the activity is destroyed.
     */
    @Override
    protected void onDestroy() {
        super.onDestroy();
        //tidy up the local broadcast manager.
        LocalBroadcastManager localBroadcastManager = LocalBroadcastManager.getInstance(this);
        localBroadcastManager.unregisterReceiver(localBroadcastReceiver);
    }
}