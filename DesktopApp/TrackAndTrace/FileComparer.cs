using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace TrackAndTrace
{
    /// <summary>
    /// Class which compares the intermediary files to the files for the user to view.
    /// This ensures that no data is lost during text message retrieval if one message cannot be pulled from the phone.
    /// </summary>
    public class FileComparer
    {
        private bool _silent;
        private string _sourceDirectory;
        private string _destinationDirectory;
        private readonly string _newFileLine = "Name,Number,Hour,Minute";

        public FileComparer(bool silent, string sourceDirectory, string destinationDirectory)
        {
            _silent = silent;
            _sourceDirectory = sourceDirectory;
            _destinationDirectory = destinationDirectory;
        }

        /// <summary>
        /// Method to run the File Comparer
        /// </summary>
        public void Run()
        {
            string[] sourceFilePaths = Directory.GetFiles(_sourceDirectory);
            string[] destinationFilePaths = Directory.GetFiles(_destinationDirectory);
            
            List<string> destinationFiles = new List<string>();
            foreach (string destinationFilePath in destinationFilePaths)
            {
                string[] destinationFilePathSplit = destinationFilePath.Split('\\');
                destinationFiles.Add(destinationFilePathSplit[destinationFilePathSplit.Length - 1]);
            }

            List<TextMessage> messages;
            int numMessagesWritten;
            string[] splitSourceFilePath;
            string sourceFile;

            foreach (string sourceFilePath in sourceFilePaths)
            {
                splitSourceFilePath = sourceFilePath.Split('\\');
                sourceFile = splitSourceFilePath[splitSourceFilePath.Length - 1];
                
                if (!destinationFiles.Contains(sourceFile))
                {
                    //file is not yet present in the destination
                    //if we are unable to create the file, skip it. We will attempt again in 5 mins.
                    if (!CreateFile(_destinationDirectory + "\\" + sourceFile))
                        continue;

                    messages = ReadCSVFile(_sourceDirectory + "\\" + sourceFile);
                    numMessagesWritten = WriteToFile(_destinationDirectory + "\\" + sourceFile, messages);
                    if (!_silent)
                        PrintDetails(numMessagesWritten, messages.Count, sourceFile);
                }
                else
                {
                    messages = CompareFiles(sourceFile);
                    numMessagesWritten = WriteToFile(_destinationDirectory + "\\" + sourceFile, messages);
                    if (!_silent)
                        PrintDetails(numMessagesWritten, messages.Count, sourceFile);
                }
            }
            
            if(!_silent)
                Console.WriteLine();
        }

        /// <summary>
        /// Method to compare the contents of two CSV files - one from the destination directory and one from the source directory.
        /// </summary>
        /// <param name="fileName">The name of the files</param>
        /// <returns>A list of text messages where are in the source file but not the destination file.</returns>
        private List<TextMessage> CompareFiles(string fileName)
        {
            List<TextMessage> srcMessages = ReadCSVFile(_sourceDirectory + "\\" + fileName);
            List<TextMessage> destMessages = ReadCSVFile(_destinationDirectory + "\\" + fileName);
            List<string> destMessagesBodies = new List<string>();

            foreach (TextMessage message in destMessages)
            {
                destMessagesBodies.Add(message.Body);
            }
            
            //move all the messages to the destination if they aren't already there
            foreach (TextMessage message in srcMessages)
            {
                if (!destMessagesBodies.Contains(message.Body))
                    destMessages.Add(message);
            }
            
            //sort and return the destination list
            destMessages.Sort((x, y) => x.Time.CompareTo(y.Time));
            return destMessages;
        }

        /// <summary>
        /// Method to create a given file.
        /// </summary>
        /// <param name="file">The path of the file to create.</param>
        /// <returns>Whether or not the file was created.</returns>
        private bool CreateFile(string file)
        {
            if (!File.Exists(file))
            {
                FileStream fs = File.Create(file);
                fs.Close();
            }

            return File.Exists(file);
        }

        /// <summary>
        /// Method to write a list of text messages to a file.
        /// </summary>
        /// <param name="filePath">The file path of the file to write to.</param>
        /// <param name="messages">The list of text messages to write to.</param>
        /// <returns>The number of messages that were written to the file.</returns>
        private int WriteToFile(string filePath, List<TextMessage> messages)
        {
            int numMessagesWritten = 0;
            StreamWriter writer = null;

            try
            {
                writer = new StreamWriter(filePath, false);
                //write the header
                writer.WriteLine(_newFileLine);

                foreach (TextMessage message in messages)
                {
                    writer.WriteLine(message.GetOutputFileLine());
                    numMessagesWritten++;
                }
            }
            catch (Exception exception)
            {
                if (exception.HResult == -2147024864)
                {
                    //this is an error where two processes are attempting to access the same file.
                    //this is an error that I want to ignore because we will attempt the read again in 4-6 mins
                    //and the file will be updated the next time around.
                    Utils.PrintErrorMessage(null, new string[] {"Please close the file " + filePath}, false);
                }
                else
                {
                    Utils.PrintErrorMessage(exception, new string[]{"Error writing to file " + filePath}, true);
                }
            }
            finally
            {
                writer?.Close();
            }

            return numMessagesWritten;
        }
        
        /// <summary>
        /// Method to read a CSV file.
        /// </summary>
        /// <param name="filePath">The path of the file to read.</param>
        /// <returns>A list of text messages from the file.</returns>
        private List<TextMessage> ReadCSVFile(string filePath)
        {
            List<TextMessage> messages = new List<TextMessage>();
            StreamReader reader = null;
            string line = "";
            string[] splitLine;
            string[] splitFilePath = filePath.Split('\\');
            string date = splitFilePath[splitFilePath.Length - 1];
            date = date.Substring(0, date.Length - 4);
            string body;
            string from;
            string hour;
            string minute;

            try
            {
                reader = new StreamReader(filePath);
                reader.ReadLine(); //read off the file header.
                while ((line = reader.ReadLine()) != null)
                {
                    splitLine = line.Split(',');
                    //if the size of the array is not what was expected, ignore this line.
                    if (splitLine.Length != 4)
                        continue;

                    body = splitLine[0].Trim();
                    from = splitLine[1].Trim();
                    hour = splitLine[2].Trim();
                    minute = splitLine[3].Trim();

                    //if we are unable to parse the date & time of this message, ignore it
                    DateTime messageTime;
                    if (!DateTime.TryParseExact(date + " " + hour + minute, "yyyy.MM.dd HHmm",
                        CultureInfo.InvariantCulture, DateTimeStyles.None, out messageTime))
                        continue;
                    
                    messages.Add(new TextMessage("none", body, from, messageTime));
                }
            }
            catch (Exception exception)
            {
                if (exception.HResult == -2147024864)
                {
                    //this is an error where two processes are attempting to access the same file.
                    //this is an error that I want to ignore because we will attempt the read again in 4-6 mins
                    //and the file will be updated the next time around.
                    Utils.PrintErrorMessage(null, new string[] {"Please close the file " + filePath}, false);
                }
                else
                {
                    Utils.PrintErrorMessage(exception, new string[] {"Error reading from file " + filePath}, false);
                }
            }
            finally
            {
                reader?.Close();
            }

            return messages;
        }
        
        /// <summary>
        /// Method to print the details of what is happening in the file comparison.
        /// </summary>
        /// <param name="numMessagesWritten">The number of messages written to the Viewing files.</param>
        /// <param name="numMessagesToWrite">The number of messages that we expected to write.</param>
        /// <param name="date">The date (file name) of both the Output and Viewing files.</param>
        private void PrintDetails(int numMessagesWritten, int numMessagesToWrite, string date)
        {
            string srcPath = _sourceDirectory + "\\" + date;
            string destPath = _destinationDirectory + "\\" + date;
            if (numMessagesWritten == numMessagesToWrite)
                Console.Write("All new messages written from ");
            else
                Console.Write(numMessagesWritten + " messages written from ");

            Console.Write(srcPath + " to " + destPath + ". ");
            Console.WriteLine(numMessagesWritten + " total messages written.");
        }
    }
}