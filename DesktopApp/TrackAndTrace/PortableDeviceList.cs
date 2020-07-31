using System;
using System.Collections.Generic;
using PortableDeviceApiLib;

namespace TrackAndTrace
{
    /// <summary>
    /// A list of portable devices which are currently connected to the system.
    /// It should be noted that some of the calls used here are part of a c++ library
    /// and using this in c# gets a little funky in places.
    /// Credit for WindowsPortableDevices code goes to https://github.com/geersch/WPD
    /// </summary>
    public class PortableDeviceList : List<PortableDevice>
    {
        private PortableDeviceManager _deviceManager;

        public PortableDeviceList()
        {
            _deviceManager = new PortableDeviceManager();
        }

        /// <summary>
        /// Method to refresh this class with the portable devices that are connected to the system.
        /// </summary>
        /// <returns>Whether the refresh was successful or not.</returns>
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

            //add each device found to the list.
            foreach(string deviceId in deviceIds)
            {
                Add(new PortableDevice(deviceId));
            }

            return true;
        }
    }
}