using System;
using System.Threading;

namespace TrackAndTrace
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            string textMessagesLocalStorage = @"C:\TrackAndTrace\TextMessages";
            string outputFiles = @"C:\TrackAndTrace\OutputFiles";
            string viewingFiles = @"C:\TrackAndTrace\ViewingFiles";

            if (!Startup.PerformStartup(textMessagesLocalStorage, outputFiles, viewingFiles))
            {
                Console.WriteLine("Program was unable to create the necessary folders to start.");
                Console.WriteLine("Please restart the program.");
                Console.WriteLine("If this issue persists, please contact the developer.");
                return;
            }

            bool programmingRunning = true;
            Console.WriteLine("Please press the escape key if the program needs to be stopped.");
            Console.WriteLine("This ensures that the program is not stopped during file transmission.");
            Console.WriteLine("It could take up to 5 seconds for your key press to be registered, be patient.");

            FileRetriever fileRetriever = new FileRetriever(false, textMessagesLocalStorage);
            FileModifier fileModifier = new FileModifier(false, textMessagesLocalStorage, outputFiles);
            FileComparer fileComparer = new FileComparer(false, outputFiles, viewingFiles);
            Random random = new Random();
            
            while (programmingRunning)
            {
                //kill the program if we cannot retrieve the files.
                if (fileRetriever.Run())
                    break;
                fileModifier.Run();
                fileComparer.Run();

                //see if the user needs to stop the program while
                //waiting a random time between 4 and 6 minutes to copy the files again.
                int i = 0;
                while (programmingRunning && i < 100)
                {
                    Thread.Sleep(random.Next(2800, 3200));
                    if (Console.KeyAvailable)
                    {
                        ConsoleKey key = Console.ReadKey().Key;
                        programmingRunning = key != ConsoleKey.Escape;
                    }

                    i++;
                }
            }
            Console.WriteLine();
            Console.WriteLine("Program closing, press any key to continue.");
            Console.ReadKey();
        }
    }
}