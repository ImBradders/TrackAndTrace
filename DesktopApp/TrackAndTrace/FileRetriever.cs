using System;

namespace TrackAndTrace
{
    public class FileRetriever
    {
        private readonly bool _silent;
        private int _depth;
        private PortableDevice _currentDevice;
        private readonly string _destinationPath;
        
        public FileRetriever(bool silent, string destinationPath)
        {
            _silent = silent;
            _destinationPath = destinationPath;
        }
        public bool Run()
        {
            PortableDeviceList deviceList = new PortableDeviceList();
            if (!deviceList.Refresh())
                return false;
            
            foreach(PortableDevice device in deviceList)
            {
                _currentDevice = device;
                Console.WriteLine(_currentDevice.DeviceId);
                _currentDevice.Connect();
                PortableDeviceFolder root = _currentDevice.GetContents();
                RetrieveFromDevice(root, "TrackAndTrace");
                _currentDevice.Disconnect();
                Console.WriteLine();
            }

            return true;
        }

        private void RetrieveFromDevice(PortableDeviceFolder rootDirectory, string folderName)
        {
            foreach (PortableDeviceObject folder in rootDirectory.Files)
            {
                RetrieveFromObject(folder, folderName);
            }
        }
        
        private void RetrieveFromObject(PortableDeviceObject portableDeviceObject, string folderName)
        {
            if (!_silent)
                Console.WriteLine(IndentString() + portableDeviceObject.Name);
            
            if (portableDeviceObject is PortableDeviceFolder)
            {
                RetrieveFromFolder((PortableDeviceFolder) portableDeviceObject, folderName);
            }
        }
        private void RetrieveFromFolder(PortableDeviceFolder folder, string folderName)
        {
            _depth++;
            foreach (PortableDeviceObject item in folder.Files)
            {
                if (item is PortableDeviceFolder)
                {
                    if (!_silent)
                        Console.WriteLine(IndentString() + item.Name);
                    RetrieveFromFolder((PortableDeviceFolder) item, folderName);
                }
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