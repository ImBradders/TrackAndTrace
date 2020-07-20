using System;
using System.IO;

namespace TrackAndTrace
{
    public class Utils
    {
        public static void PrintErrorMessage(Exception exception, string[] messages, bool tellDev)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.BackgroundColor = ConsoleColor.White;
            if (tellDev)
                Console.WriteLine("Please report the following error to the developer:");
            foreach (string message in messages)
            {
                Console.WriteLine(message);
            }
            if (exception != null)
            {
                Console.WriteLine(exception.Message);
                Console.WriteLine(exception.StackTrace);
            }
            Console.ResetColor();
        }
        
        public static string ReadTextMessageFile(string filePath) {
            StreamReader reader = null;
            string fileContents = "";
            try
            {
                reader = new StreamReader(filePath);
                string line;

                while ((line = reader.ReadLine()) != null)
                {
                    fileContents += line;
                }
            }
            catch (Exception exception)
            {
                if (exception.HResult == -2147024864)
                {
                    //this is an error where two processes are attempting to access the same file.
                    //this is an error that I want to ignore because we will attempt the read again in 4-6 mins
                    //and the file will be updated the next time around.
                    PrintErrorMessage(null, new string[] {"Please close the file " + filePath}, false);
                }
                else
                {
                    PrintErrorMessage(exception, new string[]{"Error reading from file " + filePath}, true);
                }
            }
            finally
            {
                //close the reader if it is not null
                reader?.Close();
            }

            return fileContents;
        }
    }
}