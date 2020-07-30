using System;
using System.Globalization;
using System.IO;

namespace TrackAndTrace
{
    public class FileDeleter
    {
        private bool _silent;
        private readonly string[] _directories;

        public FileDeleter(bool silent, string[] directories)
        {
            _silent = silent;
            _directories = directories;
        }

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
                //trim file extention
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