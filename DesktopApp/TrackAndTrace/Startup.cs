using System.IO;

namespace TrackAndTrace
{
    /// <summary>
    /// Class to perform the startup of the system to ensure the folder structure is in place.
    /// </summary>
    public class Startup
    {
        /// <summary>
        /// Method to perform startup.
        /// </summary>
        /// <param name="textMessages">File path for local text message storage.</param>
        /// <param name="outputFiles">File path for intermediary file storage.</param>
        /// <param name="viewingFiles">File path for files that user can view.</param>
        /// <returns></returns>
        public static bool PerformStartup(string textMessages, string outputFiles, string viewingFiles)
        {
            return CreateFolder(textMessages, true) && 
                   CreateFolder(viewingFiles) && 
                   CreateFolder(outputFiles, true);
        }

        /// <summary>
        /// Method to create a folder.
        /// </summary>
        /// <param name="path">The path of the folder to be created.</param>
        /// <returns>Whether or not the folder was created.</returns>
        private static bool CreateFolder(string path)
        {
            return CreateFolder(path, false);
        }

        /// <summary>
        /// Method to create a folder.
        /// </summary>
        /// <param name="path">The path of the folder to be created.</param>
        /// <param name="hidden">Whether or not the folder should have the "hidden" property.</param>
        /// <returns>Whether or not the folder was created.</returns>
        private static bool CreateFolder(string path, bool hidden)
        {
            //if the directory does not exist, attempt to create it.
            if (!Directory.Exists(path))
            {
                DirectoryInfo info = Directory.CreateDirectory(path);
                if (hidden)
                    info.Attributes = FileAttributes.Directory | FileAttributes.Hidden;
            }

            //return whether the directory now exists.
            return Directory.Exists(path);
        }
    }
}