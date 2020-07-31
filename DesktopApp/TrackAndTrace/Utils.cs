using System;
using System.IO;

namespace TrackAndTrace
{
    /// <summary>
    /// Some utility functions that are useful for the program.
    /// </summary>
    public class Utils
    {
        /// <summary>
        /// Prints a given exception to the console if this exception cannot be handled internally.
        /// </summary>
        /// <param name="exception">The exception itself.</param>
        /// <param name="messages">A list of messages that also should be printed with the exception.</param>
        /// <param name="tellDev">Whether or not the developer needs to be informed.</param>
        public static void PrintErrorMessage(Exception exception, string[] messages, bool tellDev)
        {
            //make sure the user sees this.
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
            //return the console colour to default.
            Console.ResetColor();
        }
        
        /// <summary>
        /// Read a given text message file.
        /// </summary>
        /// <param name="filePath">The path of the text message file.</param>
        /// <returns>A string containing the contents of the text message file.</returns>
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
                    /*this is an error that I want to ignore because we will attempt the read again in 4-6 mins
                      and the file will be updated the next time around.*/
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