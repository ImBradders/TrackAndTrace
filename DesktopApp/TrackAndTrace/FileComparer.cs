using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace TrackAndTrace
{
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

        public void Run()
        {
            string[] sourceFilePaths = Directory.GetFiles(_sourceDirectory);
            string[] destinationFilePaths = Directory.GetFiles(_destinationDirectory);

            List<TextMessage> messages;
            int numMessagesWritten;
            string[] splitSourceFilePath;
            string sourceFile;

            foreach (string sourceFilePath in sourceFilePaths)
            {
                splitSourceFilePath = sourceFilePath.Split('\\');
                sourceFile = splitSourceFilePath[splitSourceFilePath.Length - 1];
                
                if (!destinationFilePaths.Contains(sourceFilePath))
                {
                    //file is not yet present in the destination
                    //if we are unable to create the file, skip it. We will attempt again in 5 mins.
                    if (!CreateFile(_destinationDirectory + "\\" + sourceFile))
                        continue;

                    messages = ReadFile(_sourceDirectory + "\\" + sourceFile);
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
        }

        private List<TextMessage> CompareFiles(string filePath)
        {
            List<TextMessage> srcMessages = ReadFile(_sourceDirectory + "\\" + filePath);
            List<TextMessage> destMessages = ReadFile(_destinationDirectory + "\\" + filePath);

            //move all the messages to the destination if they aren't already there
            foreach (TextMessage message in srcMessages)
            {
                if (!destMessages.Contains(message))
                    destMessages.Add(message);
            }
            
            //sort and return the destination list
            destMessages.Sort((x, y) => x.Time.CompareTo(y.Time));
            return destMessages;
        }

        private bool CreateFile(string file)
        {
            if (!File.Exists(file))
            {
                FileStream fs = File.Create(file);
                fs.Close();
            }

            return File.Exists(file);
        }

        private int WriteToFile(string filePath, List<TextMessage> messages)
        {
            int numMessagesWritten = 0;
            StreamWriter writer = new StreamWriter(filePath, false);

            try
            {
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
                }
                else
                {
                    Console.WriteLine("Please report the following error to the developer:");
                    Console.WriteLine("Error writing to file " + filePath);
                    Console.WriteLine(exception.Message);
                    Console.WriteLine(exception.StackTrace);
                }
            }
            finally
            {
                writer.Close();
            }

            return numMessagesWritten;
        }
        
        private List<TextMessage> ReadFile(string filePath)
        {
            List<TextMessage> messages = new List<TextMessage>();
            StreamReader reader = new StreamReader(filePath);
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
                reader.ReadLine(); //read off the file header.
                while ((line = reader.ReadLine()) != null)
                {
                    splitLine = line.Split(',');
                    //if the size of the array is not what was expected, ignore this line.
                    if (splitLine.Length != 4)
                        continue;

                    body = splitLine[0];
                    from = splitLine[1];
                    hour = splitLine[2];
                    minute = splitLine[3];
                    
                    messages.Add(new TextMessage("none", body, from, 
                        DateTime.ParseExact(date + " " + hour + minute, "yyyy.MM.dd HHmm", CultureInfo.InvariantCulture)));
                }
            }
            catch (Exception exception)
            {
                if (exception.HResult == -2147024864)
                {
                    //this is an error where two processes are attempting to access the same file.
                    //this is an error that I want to ignore because we will attempt the read again in 4-6 mins
                    //and the file will be updated the next time around.
                }
                else
                {
                    Console.WriteLine("Please report the following error to the developer:");
                    Console.WriteLine("Error reading from file " + filePath);
                    Console.WriteLine(exception.Message);
                    Console.WriteLine(exception.StackTrace);
                }
            }
            finally
            {
                reader.Close();
            }

            return messages;
        }

        private void PrintDetails(int numMessagesWritten, int numMessagesToWrite, string date)
        {
            string srcPath = _sourceDirectory + "\\" + date;
            string destPath = _destinationDirectory + "\\" + date;
            if (numMessagesWritten == numMessagesToWrite)
                Console.Write("All messages written from ");
            else
                Console.Write(numMessagesWritten + " messages written from ");

            Console.WriteLine(srcPath + " to " + destPath + ".");
        }
    }
}