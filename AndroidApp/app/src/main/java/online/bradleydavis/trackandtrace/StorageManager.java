package online.bradleydavis.trackandtrace;

import android.content.ContentResolver;
import android.content.Context;
import android.database.Cursor;
import android.media.MediaScannerConnection;
import android.net.Uri;
import android.os.Environment;
import android.util.Log;
import android.widget.Toast;

import java.io.File;
import java.io.FileWriter;
import java.io.IOException;
import java.text.SimpleDateFormat;
import java.util.Date;
import java.util.Locale;

public class StorageManager {
    private Context context;
    private final String filePathExt = File.separator + "TrackAndTrace";
    SimpleDateFormat timeConverter = new SimpleDateFormat("HH:mm", Locale.ENGLISH);
    SimpleDateFormat dateConverter = new SimpleDateFormat("dd/MM/yyyy", Locale.ENGLISH);

    public StorageManager(Context context) {
        this.context = context;
        if (!CreateDir(Environment.getExternalStoragePublicDirectory(Environment.DIRECTORY_DOCUMENTS) + filePathExt)) {
            Toast.makeText(context, "Error creating file System - contact developer", Toast.LENGTH_LONG).show();
        }
    }

    public void WriteToFile() {
        Thread thread = new Thread(WriteData());
        thread.start();
    }

    private Runnable WriteData() {
        return new Runnable() {
            @Override
            public void run() {
                //one file per text message will be written. These can then be more easily managed off device.
                ContentResolver contentResolver = context.getContentResolver();
                Cursor smsInboxCursor = contentResolver.query(Uri.parse("content://sms/inbox"), null, null, null, null);
                int indexID = smsInboxCursor.getColumnIndex("_id");
                int indexBody = smsInboxCursor.getColumnIndex("body");
                int indexAddress = smsInboxCursor.getColumnIndex("address");
                int indexTimeStamp = smsInboxCursor.getColumnIndex("date");

                if (indexBody < 0 || !smsInboxCursor.moveToFirst()) return;

                do {
                    //get message details
                    long id = smsInboxCursor.getLong(indexID);
                    String phoneNumber = smsInboxCursor.getString(indexAddress);
                    String timeStamp = smsInboxCursor.getString(indexTimeStamp);
                    Date dateStamp = new Date(Long.parseLong(timeStamp));
                    String time = timeConverter.format(dateStamp);
                    String date = dateConverter.format(dateStamp);
                    String messageBody = smsInboxCursor.getString(indexBody);
                    messageBody = messageBody.replaceAll(",", "");
                    messageBody = messageBody.replaceAll("\n", " ");

                    File newFile = new File(Environment.getExternalStoragePublicDirectory(Environment.DIRECTORY_DOCUMENTS) + filePathExt +
                            File.separator + id + ".txt");
                    String formattedEntry = messageBody + ", " + phoneNumber + ", " +
                            time + ", " + date;

                    int returnValue = WriteSpecific(newFile, formattedEntry);
                    switch (returnValue) {
                        case -1:
                            //error in writing to file
                            break;
                        case 0:
                            //everything was fine
                            break;
                        case 1:
                            //file already exists
                            break;
                    }

                } while (smsInboxCursor.moveToNext());
            }
        };
    }

    private int WriteSpecific(File file, String data) {
        FileWriter fileWriter = null;
        try {
            if (!file.exists()) {
                if (!file.createNewFile()) {
                    return -1;
                }
            }
            else {
                //file already exists - presume that it has been dealt with
                return 1;
            }

            fileWriter = new FileWriter(file);
            fileWriter.write(data);
            //force the media scanner to update the file system.
            MediaScannerConnection.scanFile(context, new String[] {file.getAbsolutePath()},
                    null, null);
        }
        catch (IOException e) {
            Log.d("TrackAndTrace File Writing", e.getMessage());
            return -1;
        }
        finally {
            if (fileWriter != null) {
                try {
                    fileWriter.close();
                }
                catch (IOException e) {
                    Log.d("TrackAndTrace File Writing", e.getMessage());
                    return -1;
                }
            }
        }

        return 0;
    }

    private boolean CreateDir(String filePath) {
        File folder = null;
        try {
            folder = new File(filePath);
            if (!folder.exists()) {
                if (!folder.mkdirs()) {
                    return false;
                }
            }
        }
        catch (Exception e) {
            Log.d("TrackAndTrace Dir Creation", e.getMessage());
            return false;
        }

        return true;
    }
}
