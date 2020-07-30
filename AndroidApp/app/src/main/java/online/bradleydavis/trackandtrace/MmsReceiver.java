package online.bradleydavis.trackandtrace;

import android.content.BroadcastReceiver;
import android.content.Context;
import android.content.Intent;

/**
 * Stub class required for Manifest to be identified by the OS as an application which can be the default text messaging application.
 *
 * @author Bradley Davis
 */
public class MmsReceiver extends BroadcastReceiver {
    @Override
    public void onReceive(Context context, Intent intent) {
        //stub
    }
}
