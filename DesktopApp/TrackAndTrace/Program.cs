using System;
using System.Threading;

namespace TrackAndTrace
{
    internal class Program
    {
        /// <summary>
        /// Main method of the program providing an entry point for the system.
        /// </summary>
        /// <param name="args">Program arguments</param>
        public static void Main(string[] args)
        {
            string textMessagesLocalStorage = @"C:\TrackAndTrace\TextMessages";
            string outputFiles = @"C:\TrackAndTrace\OutputFiles";
            string viewingFiles = @"C:\TrackAndTrace\ViewingFiles";

            //ensure that the program can start.
            if (!Startup.PerformStartup(textMessagesLocalStorage, outputFiles, viewingFiles))
            {
                Console.WriteLine("Program was unable to create the necessary folders to start.");
                Console.WriteLine("Please restart the program.");
                Console.WriteLine("If this issue persists, please contact the developer.");
                return;
            }

            //request input from the user as to whether they would like the application to run in silent mode or not
            bool silent = false;
            bool gotSilent = false;
            while (!gotSilent)
            {
                Console.WriteLine("Would you like the application to run in silent mode? y/n");
                ConsoleKey key = Console.ReadKey().Key;
                switch (key)
                {
                    case ConsoleKey.Y:
                        silent = true;
                        gotSilent = true;
                        break;
                    case ConsoleKey.N:
                        silent = false;
                        gotSilent = true;
                        break;
                    default:
                        Console.WriteLine("Key entered was invalid. Please use y or n.");
                        break;
                }
            }
            Console.WriteLine();
            
            //give the user some important information that they will probably ignore.
            bool programmingRunning = true;
            Console.WriteLine("Please press the escape key if the program needs to be stopped.");
            Console.WriteLine("This ensures that the program is not stopped during file transmission.");
            Console.WriteLine("It could take up to 5 seconds for your key press to be registered, be patient.");
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
            Console.WriteLine();

            //create classes to run the program.
            FileRetriever fileRetriever = new FileRetriever(silent, textMessagesLocalStorage);
            FileModifier fileModifier = new FileModifier(silent, textMessagesLocalStorage, outputFiles);
            FileComparer fileComparer = new FileComparer(silent, outputFiles, viewingFiles);
            FileDeleter fileDeleter = new FileDeleter(silent, new string[]{textMessagesLocalStorage, outputFiles, viewingFiles});
            Random random = new Random();
            
            //main program loop
            while (programmingRunning)
            {
                //kill the program if we cannot retrieve the files.
                if (!fileRetriever.Run())
                    break;
                fileModifier.Run();
                fileComparer.Run();
                fileDeleter.Run();

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