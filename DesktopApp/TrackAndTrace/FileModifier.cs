using System;
using System.Globalization;
using System.IO;

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
                    {"Error writing to file " + ((FileStream)writer.BaseStream).Name});
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