namespace TrackAndTrace
{
    /// <summary>
    /// Class for storing a PortableDeviceObject
    /// Credit for this code goes to https://github.com/geersch/WPD
    /// </summary>
    public abstract class PortableDeviceObject
    {
        protected PortableDeviceObject(string id, string name)
        {
            Id = id;
            Name = name;
        }
        public string Id { get; private set; }
        public string Name { get; private set; }
    }

}