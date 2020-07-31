using System;

namespace TrackAndTrace
{
    /// <summary>
    /// Class which retrieves files off the mobile phone running the Android application.
    /// Credit for WindowsPortableDevices code goes to https://github.com/geersch/WPD
    /// </summary>
    public class FileRetriever
    {
        private readonly bool _silent;
        private int _depth;
        private PortableDevice _currentDevice;
        private readonly string _destinationPath;
        
        /// <summary>
        /// Constructor for the class.
        /// </summary>
        /// <param name="silent">Whether or not the file retriever is to run silently.</param>
        /// <param name="destinationPath">The path that the files are to be written to.</param>
        public FileRetriever(bool silent, string destinationPath)
        {
            _silent = silent;
            _destinationPath = destinationPath;
        }
        
        /// <summary>
        /// Method to run the file retriever.
        /// </summary>
        /// <returns>Whether or not the files were successfully retrieved.</returns>
        public bool Run()
        {
            PortableDeviceList deviceList = new PortableDeviceList();
            if (!deviceList.Refresh())
                return false;
            
            foreach(PortableDevice device in deviceList)
            {
                _currentDevice = device;
                if (!_silent)
                    Console.WriteLine(_currentDevice.DeviceId);
                _currentDevice.Connect();
                PortableDeviceFolder root = _currentDevice.GetContents();
                RetrieveFromDevice(root, "TrackAndTrace");
                _currentDevice.Disconnect();
                Console.WriteLine();
            }

            return true;
        }

        /// <summary>
        /// Method to retrieve a specific folder's contents from the device.
        /// </summary>
        /// <param name="rootDirectory">The root dir of the device.</param>
        /// <param name="folderName">The folder name to find.</param>
        private void RetrieveFromDevice(PortableDeviceFolder rootDirectory, string folderName)
        {
            foreach (PortableDeviceObject folder in rootDirectory.Files)
            {
                RetrieveFromObject(folder, folderName);
            }
        }
        
        /// <summary>
        /// Method to search for and retrieve a specific folder's contents from an internal object on the device.
        /// If the object is not a folder and we are not in silent mode, print out its name in the directory tree.
        /// If the object is a folder, check its contents for the folder name.
        /// </summary>
        /// <param name="portableDeviceObject">The object to search for the folder in.</param>
        /// <param name="folderName">The folder name to find.</param>
        private void RetrieveFromObject(PortableDeviceObject portableDeviceObject, string folderName)
        {
            if (!_silent)
                Console.WriteLine(IndentString() + portableDeviceObject.Name);
            
            if (portableDeviceObject is PortableDeviceFolder)
            {
                RetrieveFromFolder((PortableDeviceFolder) portableDeviceObject, folderName);
            }
        }
        
        /// <summary>
        /// Method to search for and retrieve a specific folder's content from an internal folder on the device.
        /// </summary>
        /// <param name="folder">The folder to search in.</param>
        /// <param name="folderName">The folder to find.</param>
        private void RetrieveFromFolder(PortableDeviceFolder folder, string folderName)
        {
            _depth++;
            //get all dirs from the folder.
            foreach (PortableDeviceObject item in folder.Files)
            {
                //if the folder contains another folder, search in there also.
                if (item is PortableDeviceFolder)
                {
                    if (!_silent)
                        Console.WriteLine(IndentString() + item.Name);
                    RetrieveFromFolder((PortableDeviceFolder) item, folderName);
                }
                //if the folder's name that the items were retrieved from is the same as the folder we are searching for, download the items in the folder.
                else if (folder.Name.Equals(folderName))
                {
                    bool saved = _currentDevice.DownloadFile((PortableDeviceFile) item, _destinationPath);
                    if (!_silent && saved)
                        Console.WriteLine(IndentString() + item.Name + " - Saved to Computer");
                    else if (!_silent && !saved)
                        Console.WriteLine(IndentString() + item.Name + " - NOT saved");
                }
            }
            _depth--;
        }

        /// <summary>
        /// Small method to get the number of spaces before a specific folder or file.
        /// This makes the directory tree that is output look good.
        /// </summary>
        /// <returns>A number of spaces based on the depth that we are in the devices file system.</returns>
        private string IndentString()
        {
            string indentString = "";
            for (int i = 0; i < _depth; i++)
            {
                indentString += "  ";
            }
            return indentString;
        }
    }
}