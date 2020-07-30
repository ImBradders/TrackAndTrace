package online.bradleydavis.trackandtrace;

import android.content.BroadcastReceiver;
import android.content.ContentValues;
import android.content.Context;
import android.content.Intent;
import android.net.Uri;
import android.os.Bundle;
import android.provider.Telephony;
import android.telephony.SmsMessage;

import androidx.localbroadcastmanager.content.LocalBroadcastManager;

/**
 * Class which listens for system broadcasts of text messages being received.
 *
 * @author Bradley Davis
 */
public class SmsBroadcastReceiver extends BroadcastReceiver {
    private static final String ACTION_SMS_NEW = "android.provider.Telephony.SMS_DELIVER";

    @Override
    public void onReceive(Context context, Intent intent) {
        final String action = intent.getAction();

        //if the message is a new message, we need to save it.
        if (action.equals(ACTION_SMS_NEW)) {
            Bundle bundle = intent.getExtras();
            if (bundle != null) {

                String sender = "", text = "";
                //retrieve the messages from the intent.
                SmsMessage[] smsMessages = Telephony.Sms.Intents.getMessagesFromIntent(intent);
                for (SmsMessage message : smsMessages) {
                    // Do whatever you want to do with SMS.
                    sender = message.getOriginatingAddress();
                    text += message.getMessageBody(); //I have no idea why this concat is here but it was there on SO.
                }

                //add the message to the database.
                ContentValues values = new ContentValues();
                values.put("address", sender);
                values.put("body", text);
                context.getContentResolver().insert(
                        Uri.parse("content://sms/inbox"), values);
            }
        }

        //Send a local broadcast to inform the UI that a message has been received.
        LocalBroadcastManager localBroadcastManager =
                LocalBroadcastManager.getInstance(context.getApplicationContext());
        localBroadcastManager.sendBroadcast(new Intent("online.bradleydavis.TrackAndTrace"));
    }
}
