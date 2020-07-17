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

        public void Refresh()
        {
            _deviceManager.RefreshDeviceList();
            // Determine how many WPDs are connected
            string[] deviceIds = new string[1];
            uint count = 1;
            _deviceManager.GetDevices(ref deviceIds[0], ref count);
            
            // Retrieve the device id for each connected device
            deviceIds = new string[count];
            _deviceManager.GetDevices(ref deviceIds[0], ref count);
            
            foreach(string deviceId in deviceIds)
            {
                Add(new PortableDevice(deviceId));
            }
        }
    }
}