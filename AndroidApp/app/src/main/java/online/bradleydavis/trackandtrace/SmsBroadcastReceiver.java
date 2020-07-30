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

public class SmsBroadcastReceiver extends BroadcastReceiver {
    private static final String ACTION_SMS_NEW = "android.provider.Telephony.SMS_DELIVER";

    @Override
    public void onReceive(Context context, Intent intent) {
        final String action = intent.getAction();

        if (action.equals(ACTION_SMS_NEW)) {
            Bundle bundle = intent.getExtras();
            if (bundle != null) {

                String sender = "", text = "";
                SmsMessage[] smsMessages = Telephony.Sms.Intents.getMessagesFromIntent(intent);
                for (SmsMessage message : smsMessages) {
                    // Do whatever you want to do with SMS.
                    sender = message.getOriginatingAddress();
                    text += message.getMessageBody();
                }

                ContentValues values = new ContentValues();
                values.put("address", sender);
                values.put("body", text);
                context.getContentResolver().insert(
                        Uri.parse("content://sms/inbox"), values);
            }
        }

        LocalBroadcastManager localBroadcastManager =
                LocalBroadcastManager.getInstance(context.getApplicationContext());
        localBroadcastManager.sendBroadcast(new Intent("online.bradleydavis.TrackAndTrace"));
    }
}
