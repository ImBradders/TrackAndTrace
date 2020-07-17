using System;

namespace TrackAndTrace
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            FileRetriever fileRetriever = new FileRetriever(false);
            fileRetriever.Run();
        }
    }
}