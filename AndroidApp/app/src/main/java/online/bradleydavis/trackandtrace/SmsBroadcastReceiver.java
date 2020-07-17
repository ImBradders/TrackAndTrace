package online.bradleydavis.trackandtrace;

import android.content.BroadcastReceiver;
import android.content.Context;
import android.content.Intent;

import androidx.localbroadcastmanager.content.LocalBroadcastManager;

public class SmsBroadcastReceiver extends BroadcastReceiver {
    @Override
    public void onReceive(Context context, Intent intent) {
        LocalBroadcastManager localBroadcastManager =
                LocalBroadcastManager.getInstance(context.getApplicationContext());
        localBroadcastManager.sendBroadcast(new Intent("online.bradleydavis.TrackAndTrace"));
    }
}
