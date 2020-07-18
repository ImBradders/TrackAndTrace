using System.IO;

namespace TrackAndTrace
{
    public class Startup
    {
        public static bool PerformStartup(string textMessages, string outputFiles, string viewingFiles)
        {
            return CreateFolder(textMessages) && 
                   CreateFolder(viewingFiles) && 
                   CreateFolder(outputFiles, true);
        }

        private static bool CreateFolder(string path)
        {
            return CreateFolder(path, false);
        }

        private static bool CreateFolder(string path, bool hidden)
        {
            if (!Directory.Exists(path))
            {
                DirectoryInfo info = Directory.CreateDirectory(path);
                if (hidden)
                    info.Attributes = FileAttributes.Directory | FileAttributes.Hidden;
            }

            return Directory.Exists(path);
        }
    }
}