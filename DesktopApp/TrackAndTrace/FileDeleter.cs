using System;
using System.Globalization;
using System.IO;

namespace TrackAndTrace
{
    /// <summary>
    /// Class to delete files after they go out of date.
    /// This class is written to comply with the Track and Trace law in the UK at the time of writing.
    /// Therefore, all files containing data over 21 days old are deleted.
    /// </summary>
    public class FileDeleter
    {
        private bool _silent;
        private readonly string[] _directories;

        public FileDeleter(bool silent, string[] directories)
        {
            _silent = silent;
            _directories = directories;
        }

        /// <summary>
        /// Method to run the file File Deleter.
        /// </summary>
        public void Run()
        {
            foreach (string directory in _directories)
            {
                if (Directory.Exists(directory))
                {
                    string[] files = Directory.GetFiles(directory);
                    CleanseFiles(files);
                }
            }
            
            if(!_silent)
                Console.WriteLine();
        }

        /// <summary>
        /// Method which determines which files need to be deleted.
        /// </summary>
        /// <param name="files">The files to check for deletion.</param>
        private void CleanseFiles(string[] files)
        {
            string[] pathSplit;
            string fileName;
            DateTime fileDate;
            DateTime twentyOneDaysAgo = DateTime.Now.AddDays(-21);

            foreach (string file in files)
            {
                pathSplit = file.Split('\\');
                fileName = pathSplit[pathSplit.Length - 1];
                //trim file extension
                fileName = fileName.Substring(0, fileName.Length - 4);
                
                //check to see if the file name is a DateTime. If it is, we need to use this to know when to delete it.
                if (!DateTime.TryParseExact(fileName, "yyyy.MM.dd", CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out fileDate))
                    fileDate = GetDateFromText(file);

                if (fileDate == DateTime.MinValue)
                {
                    Utils.PrintErrorMessage(null, new string[] {"Error getting date from file " + file}, true);
                }
                else if (fileDate.CompareTo(twentyOneDaysAgo) < 0)
                    DeleteFile(file);
            }
        }

        /// <summary>
        /// Method to get the date from inside of a text message file.
        /// </summary>
        /// <param name="filePath">The path of the text file to get the date from.</param>
        /// <returns>The date-stamp of the text message.</returns>
        private DateTime GetDateFromText(string filePath)
        {
            string fileContents = Utils.ReadTextMessageFile(filePath);
            string[] splitContents = fileContents.Split(',');

            if (splitContents.Length != 4)
                return DateTime.MinValue;
            
            for (int i = 0; i < splitContents.Length; i++)
            {
                splitContents[i] = splitContents[i].Trim();
            }
            
            DateTime messageTime;
            if (!DateTime.TryParseExact(splitContents[3] + " " + splitContents[2], "dd/MM/yyyy HH:mm",
                CultureInfo.InvariantCulture, DateTimeStyles.None, out messageTime))
                return DateTime.MinValue;

            return messageTime;
        }

        /// <summary>
        /// Method to delete a specific file.
        /// </summary>
        /// <param name="filePath">The path of the file to delete.</param>
        private void DeleteFile(string filePath)
        {
            bool deleted = true;
            try
            {
                File.Delete(filePath);
            }
            catch (Exception exception)
            {
                Utils.PrintErrorMessage(exception, new string[] {"Error deleting file " + filePath}, true);
                deleted = false;
            }
            
            if(!_silent)
                PrintDetails(filePath, deleted);
        }

        /// <summary>
        /// Method to print details if the class is not running in silent mode.
        /// </summary>
        /// <param name="filePath">The file path of the file that was deleted.</param>
        /// <param name="deleted">Whether or not the file was deleted.</param>
        private void PrintDetails(string filePath, bool deleted)
        {
            if (deleted)
                Console.Write("Deleted file ");
            else
                Console.Write("NOT DELETED file ");
            Console.WriteLine(filePath);
        }
    }
}