package online.bradleydavis.trackandtrace;

import android.app.Service;
import android.content.Intent;
import android.os.IBinder;

import androidx.annotation.Nullable;

/**
 * Stub class required for Manifest to be identified by the OS as an application which can be the default text messaging application.
 *
 * @author Bradley Davis
 */
public class HeadlessSmsSendService extends Service {
    @Nullable
    @Override
    public IBinder onBind(Intent intent) {
        //stub
        return null;
    }
}
