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
import java.util.ArrayList;
import java.util.Calendar;
import java.util.Date;
import java.util.List;
import java.util.Locale;

/**
 * Class to handle how the application uses the device storage which is necessary for this application.
 *
 * @author Bradley Davis
 */
public class StorageManager {
    private Context context;
    private final String baseFilePath = Environment.getExternalStoragePublicDirectory(Environment.DIRECTORY_DOCUMENTS).toString();
    private final String filePathExt = File.separator + "TrackAndTrace";
    SimpleDateFormat timeConverter = new SimpleDateFormat("HH:mm", Locale.ENGLISH);
    SimpleDateFormat dateConverter = new SimpleDateFormat("dd/MM/yyyy", Locale.ENGLISH);

    /**
     * Constructor to pass in the context of the application.
     *
     * @param context Application context.
     */
    public StorageManager(Context context) {
        this.context = context;
        if (!CreateDir(baseFilePath + filePathExt)) {
            Toast.makeText(context, "Error creating file System - contact developer", Toast.LENGTH_LONG).show();
        }
    }

    /**
     * This is the method called to ensure that all files are updated.
     * This runs another method on a separate thread to ensure that this can be done outside of the UI thread.
     */
    public void UpdateFiles() {
        Thread thread = new Thread(PerformUpdate());
        thread.start();
    }

    /**
     * Method creating the runnable which updates the various files on the device.
     *
     * @return A runnable which will write new files and delete old ones.
     */
    private Runnable PerformUpdate() {
        return new Runnable() {
            @Override
            public void run() {
                List<String> filesWritten = WriteFiles();
                //delete the old files
                if (filesWritten != null)
                    DeleteOld(filesWritten);
            }
        };
    }

    /**
     * Method to delete the files which are no longer necessary.
     * To comply with GDPR, the files on this device must be deleted after 21 days.
     *
     * @param filesToKeep The files which have just been written.
     */
    private void DeleteOld(List<String> filesToKeep) {
        //get the files currently on disk.
        File directory = new File(baseFilePath + filePathExt);
        String[] oldFiles = directory.list();
        //if there are no files on disk or if the same number of files are on disk as have just been written, we have nothing to delete.
        if (oldFiles == null || oldFiles.length == filesToKeep.size())
            return;

        // for each file, if it is not in the list of files to keep, delete it.
        for (String file : oldFiles) {
            if (!filesToKeep.contains(file)) {
                //if the file found is not listed to be kept, delete it.
                File toDelete = new File(baseFilePath + filePathExt + File.separator + file);
                toDelete.delete();
                MediaScannerConnection.scanFile(context, new String[] {toDelete.getAbsolutePath()},
                        null, null);
            }
        }
    }

    /**
     * Method to write files to disk.
     * This attempts to write all text messages which are younger than 21 days to disk.
     *
     * @return an array of the files that either have been written to disk or that the write was attempted for.
     */
    private List<String> WriteFiles() {
        Calendar calendar = Calendar.getInstance();
        calendar.add(Calendar.DATE, -21);
        Date twentyOneDaysAgo = calendar.getTime();

        //one file per text message will be written. These can then be more easily managed off device.
        //get messages from database.
        ContentResolver contentResolver = context.getContentResolver();
        Cursor smsInboxCursor = contentResolver.query(Uri.parse("content://sms/inbox"), null, null, null, null);
        int indexID = smsInboxCursor.getColumnIndex("_id");
        int indexBody = smsInboxCursor.getColumnIndex("body");
        int indexAddress = smsInboxCursor.getColumnIndex("address");
        int indexTimeStamp = smsInboxCursor.getColumnIndex("date");

        //create the list of files to return.
        List<String> filesWritten = new ArrayList<String>();

        //move to start or return if there are no text messages.
        if (indexBody < 0 || !smsInboxCursor.moveToFirst()) return null;

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

            //create the file and data to write to it.
            File newFile = new File(baseFilePath + filePathExt +
                    File.separator + id + ".txt");
            String formattedEntry = messageBody + "," + phoneNumber + "," +
                    time + "," + date;

            //if the message is younger than 21 days, save it to the folder.
            if (dateStamp.after(twentyOneDaysAgo)) {
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

                //in this case, we need a list of all files that we attempted to write as these files should not be deleted if they exist.
                filesWritten.add(id + ".txt");
            }
         } while (smsInboxCursor.moveToNext());

        smsInboxCursor.close();

        return filesWritten;
    }

    /**
     * Method to create a file and write specific data to it.
     *
     * @param file the file to be written to/created.
     * @param data the data to write to the file.
     * @return
     * 1 - File already exists
     * 0 - File created and written to without issue
     * -1 - There was an issue in writing the file
     */
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

    /**
     * Create a given directory.
     *
     * @param filePath the path of the directory to create.
     * @return whether or not the directory could be created.
     */
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
