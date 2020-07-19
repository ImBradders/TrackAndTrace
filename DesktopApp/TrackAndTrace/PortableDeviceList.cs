using System;
using System.Collections.Generic;
using PortableDeviceApiLib;

namespace TrackAndTrace
{
    public class PortableDeviceList : List<PortableDevice>
    {
        private PortableDeviceManager _deviceManager;

        public PortableDeviceList()
        {
            _deviceManager = new PortableDeviceManager();
        }

        public bool Refresh()
        {
            _deviceManager.RefreshDeviceList();
            // Determine how many WPDs are connected
            string[] deviceIds = new string[1];
            uint count = 1;
            _deviceManager.GetDevices(ref deviceIds[0], ref count);
            
            // Retrieve the device id for each connected device
            deviceIds = new string[count];
            try
            {
                _deviceManager.GetDevices(ref deviceIds[0], ref count);
            }
            catch (IndexOutOfRangeException indexOutOfRangeException)
            {
                Console.WriteLine("Error: Unable to access mobile device.");
                Console.WriteLine("Please unplug and replug the device and ensure that the device is set to \"USB for file transfer\".");
                Console.WriteLine("Once this is done, restart this program.");
                return false;
            }

            foreach(string deviceId in deviceIds)
            {
                Add(new PortableDevice(deviceId));
            }

            return true;
        }
    }
}