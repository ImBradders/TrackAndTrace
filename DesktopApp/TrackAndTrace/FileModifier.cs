using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace TrackAndTrace
{
    public class FileModifier
    {
        private string _textMessagesPath;
        private string _outputFilesPath;
        private bool _silent;
        
        public FileModifier(bool silent, string textMessagesLocalSource, string outputFileDestination)
        {
            _silent = silent;
            _textMessagesPath = textMessagesLocalSource;
            _outputFilesPath = outputFileDestination;
        }

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

        private TextMessage GetMessageFromFile(string filePath)
        {
            string fileContents = Utils.ReadTextMessageFile(filePath);
            if (fileContents == null)
                return null;

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