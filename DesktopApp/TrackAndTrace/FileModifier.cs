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
                Console.WriteLine("Please report the following error to the developer:");
                Console.WriteLine("Error writing to file " + ((FileStream)writer.BaseStream).Name);
                Console.WriteLine(exception.Message);
                Console.WriteLine(exception.StackTrace);
                written = false;
            }

            return written;
        }

        private TextMessage GetMessageFromFile(string filePath)
        {
            string fileContents = ReadTextMessageFile(filePath);
            if (fileContents == null)
                return null;
            
            string[] textMessageDetails = fileContents.Split(',');
            string[] filePathSplit = filePath.Split('\\');
            string fileName = filePathSplit[filePathSplit.Length - 1];
            string messageId = fileName.Substring(0, fileName.Length - 4);
            DateTime messageTime = DateTime.ParseExact(textMessageDetails[3] + " " + textMessageDetails[2],
                "dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture);
            
            //file should be body,number,time,date
            return new TextMessage(messageId, textMessageDetails[0], textMessageDetails[1], messageTime);
        }

        private string ReadTextMessageFile(string filePath)
        {
            StreamReader reader = null;
            string fileContents = "";
            try
            {
                reader = new StreamReader(filePath);
                string line = "";

                while ((line = reader.ReadLine()) != null)
                {
                    fileContents += line;
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine("Please report the following error to the developer:");
                Console.WriteLine("Error reading from file " + filePath);
                Console.WriteLine(exception.Message);
                Console.WriteLine(exception.StackTrace);
            }
            finally
            {
                //close the reader if it is not null
                reader?.Close();
            }

            return fileContents;
        }
        
        private void PrintMessageDetails(TextMessage message, bool written)
        {
            if (written) 
                Console.Write("Saved message ");
            else
                Console.Write("NOT SAVED ");
            Console.WriteLine(message.Id + "  " + message.From);
            Console.WriteLine();
        }
    }
}