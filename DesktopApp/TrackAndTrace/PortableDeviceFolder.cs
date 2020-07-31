using System.Collections.Generic;

namespace TrackAndTrace
{
    /// <summary>
    /// Class for storing a PortableDeviceFolder.
    /// Credit for this code goes to https://github.com/geersch/WPD
    /// </summary>
    public class PortableDeviceFolder : PortableDeviceObject
    {
        public PortableDeviceFolder(string id, string name) : base(id, name)
        {
            Files = new List<PortableDeviceObject>();
        }
        public IList<PortableDeviceObject> Files { get; set; }
    }
}