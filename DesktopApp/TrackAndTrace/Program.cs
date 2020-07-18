using System;

namespace TrackAndTrace
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            string textMessagesLocalStorage = @"C:\TrackAndTrace\TextMessages";
            string outputFiles = @"C:\TrackAndTrace\OutputFiles";
            string viewingFiles = @"C:\TrackAndTrace\ViewingFiles";

            Startup.PerformStartup(textMessagesLocalStorage, outputFiles, viewingFiles);

            FileRetriever fileRetriever = new FileRetriever(false, textMessagesLocalStorage);
            fileRetriever.Run();
        }
    }
}