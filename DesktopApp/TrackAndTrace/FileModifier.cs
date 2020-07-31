using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace TrackAndTrace
{
    /// <summary>
    /// Class to modify the files that have been pulled off the portable device.
    /// This ensures that they are in a more readable format for the user.
    /// </summary>
    public class FileModifier
    {
        private string _textMessagesPath;
        private string _outputFilesPath;
        private bool _silent;
        
        /// <summary>
        /// Constructor for the file modifier.
        /// </summary>
        /// <param name="silent">Whether the file modifier should run in silent mode.</param>
        /// <param name="textMessagesLocalSource">The source of the text files on the system.</param>
        /// <param name="outputFileDestination">The destination that the files should be output to.</param>
        public FileModifier(bool silent, string textMessagesLocalSource, string outputFileDestination)
        {
            _silent = silent;
            _textMessagesPath = textMessagesLocalSource;
            _outputFilesPath = outputFileDestination;
        }

        /// <summary>
        /// Method to run the file modifier.
        /// </summary>
        public void Run()
        {
            DateTime currentFileDate = new DateTime(1971, 1, 1);
            string currentFilePath = "";
            StreamWriter fileWriter = null;
            string newFileLine = "Name,Number,Hour,Minute";
            string[] textMessages = Directory.GetFiles(_textMessagesPath);
            textMessages = SortTextMessages(textMessages);

            foreach (string textMessage in textMessages)
            {
                TextMessage currentMessage = GetMessageFromFile(textMessage);

                //if we were unable to get a given message, skip it
                if (currentMessage == null)
                    continue;

                if (currentMessage.Time.Year > currentFileDate.Year ||
                    currentMessage.Time.DayOfYear > currentFileDate.DayOfYear)
                {
                    //attempt to close the old file writer if there was one.
                    fileWriter?.Close();
                    
                    //new file required
                    currentFileDate = currentMessage.Time;
                    currentFilePath = _outputFilesPath + "\\" + 
                                      currentMessage.Time.ToString("yyyy.MM.dd") + ".csv";
                    FileStream fs = File.Create(currentFilePath);
                    fs.Close();

                    fileWriter = new StreamWriter(currentFilePath, true);
                    WriteToFile(ref fileWriter, newFileLine);
                }

                bool written = WriteToFile(ref fileWriter, currentMessage.GetOutputFileLine());
                if (!_silent)
                    PrintMessageDetails(currentMessage, written);
            }
            
            //ensure that we close the stream writer once we are done with it.
            fileWriter?.Close();
            if(!_silent)
                Console.WriteLine();
        }

        /// <summary>
        /// Method to sort the text messages into the order in which they were received.
        /// </summary>
        /// <param name="textMessages">The messages received.</param>
        /// <returns>An array containing the text messages in id order.</returns>
        private string[] SortTextMessages(string[] textMessages)
        {
            int[] fileNames = new int[textMessages.Length];
            string[] splitPath = textMessages[0].Split('\\');
            string pathStart = "";
            const string fileExt = ".txt";

            for (int i = 0; i < splitPath.Length - 1; i++)
            {
                pathStart += splitPath[i] + "\\";
            }

            string fileName;
            for (int i = 0; i < fileNames.Length; i++)
            {
                splitPath = textMessages[i].Split('\\');
                fileName = splitPath[splitPath.Length - 1];
                fileName = fileName.Substring(0, fileName.Length - fileExt.Length);
                if (!int.TryParse(fileName, out fileNames[i]))
                {
                    Utils.PrintErrorMessage(null, new string[] 
                                            {"Unable to parse file for sorting " + textMessages[i]}, true);
                    fileNames[i] = int.MinValue;
                }
            }

            Array.Sort(fileNames);
            if (fileNames[0] > fileNames[1])
                Array.Reverse(fileNames);

            List<string> orderedFileNames = new List<string>();
            for (int i = 0; i < fileNames.Length; i++)
            {
                if (fileNames[i] != int.MinValue)
                    orderedFileNames.Add(pathStart + fileNames[i] + fileExt);
            }

            return orderedFileNames.ToArray();
        }

        /// <summary>
        /// Method to write a specific line to a given stream.
        /// </summary>
        /// <param name="writer">The StreamWriter to use.</param>
        /// <param name="line">The line to write.</param>
        /// <returns>Whether or not the line was written.</returns>
        public bool WriteToFile(ref StreamWriter writer, string line)
        {
            bool written = true;
            try
            {
                writer.WriteLine(line);
            }
            catch (Exception exception)
            {
                Utils.PrintErrorMessage(exception, new string[] 
                    {"Error writing to file " + ((FileStream)writer.BaseStream).Name}, true);
                written = false;
            }

            return written;
        }

        /// <summary>
        /// Method to retrieve a TextMessage from a given file.
        /// </summary>
        /// <param name="filePath">The file to retrieve the contents of.</param>
        /// <returns>A TextMessage instance containing the data from the file.</returns>
        private TextMessage GetMessageFromFile(string filePath)
        {
            //read the text message file
            string fileContents = Utils.ReadTextMessageFile(filePath);
            if (fileContents == null)
                return null;

            //format the contents.
            string[] textMessageDetails = fileContents.Split(',');
            if (textMessageDetails.Length != 4)
                return null;
            for (int i = 0; i < textMessageDetails.Length; i++)
            {
                textMessageDetails[i] = textMessageDetails[i].Trim();
            }
            
            string[] filePathSplit = filePath.Split('\\');
            string fileName = filePathSplit[filePathSplit.Length - 1];
            string messageId = fileName.Substring(0, fileName.Length - 4);
            DateTime messageTime;
            if (!DateTime.TryParseExact(textMessageDetails[3] + " " + textMessageDetails[2],
                "dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out messageTime))
                return null;

            //file should be body,number,time,date
            return new TextMessage(messageId, textMessageDetails[0], textMessageDetails[1], messageTime);
        }

        /// <summary>
        /// Method to print the details of the messages as they are read from one file and written to another.
        /// </summary>
        /// <param name="message">The message details.</param>
        /// <param name="written">Whether or not this has been written.</param>
        private void PrintMessageDetails(TextMessage message, bool written)
        {
            if (written) 
                Console.Write("Saved message ");
            else
                Console.Write("NOT SAVED ");
            Console.WriteLine(message.Id + "  " + message.From);
        }
    }
}