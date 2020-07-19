using System;
using System.IO;

namespace TrackAndTrace
{
    public class Utils
    {
        public static void PrintErrorMessage(Exception exception, string[] messages)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.BackgroundColor = ConsoleColor.White;
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
                PrintErrorMessage(exception, new string[] {"Error reading from file " + filePath});
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