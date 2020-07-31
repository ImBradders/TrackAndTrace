using System;
using System.IO;
using System.Runtime.InteropServices;
using PortableDeviceApiLib;
using PortableDeviceTypesLib;

namespace TrackAndTrace
{
    /// <summary>
    /// Class representing a specific portable device.
    /// Credit for WindowsPortableDevices code goes to https://github.com/geersch/WPD
    /// </summary>
    public class PortableDevice
    {
        private bool _isConnected;
        private PortableDeviceClass _device;
        public string DeviceId { get; set; }
        
        /// <summary>
        /// Constructor for a PortableDevice
        /// </summary>
        /// <param name="deviceId">The ID of the device given by the system.</param>
        public PortableDevice(string deviceId)
        {
            DeviceId = deviceId;
            _device = new PortableDeviceClass();
            _isConnected = false;
        }

        /// <summary>
        /// Method to connect to this device.
        /// </summary>
        public void Connect()
        {
            if (_isConnected) 
                return;

            var clientInfo = (PortableDeviceApiLib.IPortableDeviceValues) new PortableDeviceValuesClass();
            _device.Open(DeviceId, clientInfo);
            _isConnected = true;
        }
        
        /// <summary>
        /// Method to disconnect from this device.
        /// </summary>
        public void Disconnect()
        {
            if (!_isConnected) 
                return;
            
            _device.Close();
            _isConnected = false;
        }
        
        /// <summary>
        /// Method to get the drive letter of this device.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if this is attempted before Connect is called.</exception>
        public string DriveLetter
        {
            get
            {
                if (!_isConnected)
                    throw new InvalidOperationException("Not connected to device.");

                // Retrieve the properties of the device
                IPortableDeviceContent content;
                IPortableDeviceProperties properties;
                _device.Content(out content);
                content.Properties(out properties);
                
                // Retrieve the values for the properties
                PortableDeviceApiLib.IPortableDeviceValues propertyValues;
                properties.GetValues("DEVICE", null, out propertyValues);
                
                // Identify the property to retrieve
                var property = new PortableDeviceApiLib._tagpropertykey();
                property.fmtid = new Guid(0x26D4979A, 0xE643, 0x4626, 0x9E, 0x2B,
                    0x73, 0x6D, 0xC0, 0xC9, 0x2F, 0xDC);
                property.pid = 12;
                
                // Retrieve the friendly name
                string propertyValue;
                propertyValues.GetStringValue(ref property, out propertyValue);
                return propertyValue;
            }
        }
        
        /// <summary>
        /// Get the entire content of the device.
        /// </summary>
        /// <returns>A PortableDeviceFolder containing the entire contents of the device.
        /// This is effectively the root dir of the device.</returns>
        public PortableDeviceFolder GetContents()
        {
            var root = new PortableDeviceFolder("DEVICE", "DEVICE");
            PortableDeviceApiLib.IPortableDeviceContent content;
            _device.Content(out content);
            EnumerateContents(ref content, root);
            return root;
        }
        
        /// <summary>
        /// Turns the content of the device into PortableDeviceFolders and PortableDeviceFiles by running through the folder structure.
        /// Credit to https://github.com/geersch/WPD for this code.
        /// </summary>
        /// <param name="content">The content of the device.</param>
        /// <param name="parent">The root folder of the device.</param>
        private static void EnumerateContents(ref IPortableDeviceContent content, PortableDeviceFolder parent)
        {
            // Get the properties of the object
            IPortableDeviceProperties properties;
            content.Properties(out properties);
            
            // Enumerate the items contained by the current object
            IEnumPortableDeviceObjectIDs objectIds;
            content.EnumObjects(0, parent.Id, null, out objectIds);
            uint fetched = 0;
            
            do
            {
                string objectId;
                objectIds.Next(1, out objectId, ref fetched);
                if (fetched > 0)
                {
                    var currentObject = WrapObject(properties, objectId);
                    parent.Files.Add(currentObject);
                    if (currentObject is PortableDeviceFolder)
                    {
                        EnumerateContents(ref content, (PortableDeviceFolder) currentObject);
                    }
                }
            } while (fetched > 0);
        }
        
        /// <summary>
        /// Turns an object on the device into a PortableDeviceFolder or PortableDeviceFile.
        /// I have no idea how this works or where the GUID numbers come from.
        /// Credit to https://github.com/geersch/WPD for this code.
        /// </summary>
        /// <param name="properties">The properties of the portable device.</param>
        /// <param name="objectId">The id of the object to be wrapped.</param>
        /// <returns></returns>
        private static PortableDeviceObject WrapObject(IPortableDeviceProperties properties, string objectId)
        {
            PortableDeviceApiLib.IPortableDeviceKeyCollection keys;
            properties.GetSupportedProperties(objectId, out keys);
            PortableDeviceApiLib.IPortableDeviceValues values;
            properties.GetValues(objectId, keys, out values);
            
            // Get the name of the object
            string name;
            var property = new PortableDeviceApiLib._tagpropertykey();
            property.fmtid = new Guid(0xEF6B490D, 0x5CD8, 0x437A, 0xAF, 0xFC,
                0xDA, 0x8B, 0x60, 0xEE, 0x4A, 0x3C);
            property.pid = 4;
            values.GetStringValue(property, out name);
            
            // Get the type of the object
            Guid contentType;
            property = new PortableDeviceApiLib._tagpropertykey();
            property.fmtid = new Guid(0xEF6B490D, 0x5CD8, 0x437A, 0xAF, 0xFC,
                0xDA, 0x8B, 0x60, 0xEE, 0x4A, 0x3C);
            property.pid = 7;
            values.GetGuidValue(property, out contentType);
            
            var folderType = new Guid(0x27E2E392, 0xA111, 0x48E0, 0xAB, 0x0C,
                0xE1, 0x77, 0x05, 0xA0, 0x5F, 0x85);
            var functionalType = new Guid(0x99ED0160, 0x17FF, 0x4C44, 0x9D, 0x98,
                0x1D, 0x7A, 0x6F, 0x94, 0x19, 0x21);
            
            if (contentType == folderType || contentType == functionalType)
            {
                return new PortableDeviceFolder(objectId, name);
            }
            
            return new PortableDeviceFile(objectId, name);
        }

        /// <summary>
        /// Download a specific file from this device.
        /// Credit to https://github.com/geersch/WPD for majority of this code.
        /// </summary>
        /// <param name="file">The file to be downloaded.</param>
        /// <param name="saveToPath">The local path to save this to.</param>
        /// <returns>Whether or no the transfer was successful.</returns>
        public bool DownloadFile(PortableDeviceFile file, string saveToPath)
        {
            try
            {
                //create the relevant things for pulling the data off the device.
                IPortableDeviceContent content;
                _device.Content(out content);
            
                IPortableDeviceResources resources;
                content.Transfer(out resources);
            
                //create the IStream to do the pulling.
                PortableDeviceApiLib.IStream wpdStream;
                uint optimalTransferSize = 0;
                var property = new PortableDeviceApiLib._tagpropertykey();
                property.fmtid = new Guid(0xE81E79BE, 0x34F0, 0x41BF, 0xB5, 0x3F,
                    0xF1, 0xA0, 0x6A, 0xE8, 0x78, 0x42);
                property.pid = 0;
                resources.GetStream(file.Id, ref property, 0, ref optimalTransferSize,
                    out wpdStream);
                System.Runtime.InteropServices.ComTypes.IStream sourceStream =
                    (System.Runtime.InteropServices.ComTypes.IStream) wpdStream;
            
                string filename = Path.GetFileName(file.Name) + ".txt";
                FileStream targetStream = new FileStream(Path.Combine(saveToPath, filename),
                    FileMode.Create, FileAccess.Write);
                unsafe
                {
                    byte[] buffer = new byte[1024];
                    int bytesRead;
                    do
                    {
                        sourceStream.Read(buffer, 1024, new IntPtr(&bytesRead));
                        targetStream.Write(buffer, 0, bytesRead);
                    } while (bytesRead > 0);
                    //for future Brad - wpdSteam may also need to be released
                    Marshal.ReleaseComObject(sourceStream);
                    targetStream.Close();
                }

                return true;
            }
            catch (Exception error)
            {
                Utils.PrintErrorMessage(error, new string[] 
                    {"Unable to access " + file.Name + " from remote device."}, true);
                return false;
            }
        }
    }
}