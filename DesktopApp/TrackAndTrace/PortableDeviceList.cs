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
                Utils.PrintErrorMessage(indexOutOfRangeException, new string[]
                {
                    "Unable to access mobile device.",
                    "Please unplug and replug the device and ensure that the device is set to \"USB for file transfer\".",
                    "Once this is done, restart this program."
                }, true);
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