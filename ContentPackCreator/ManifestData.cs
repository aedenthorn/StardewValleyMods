using System.Collections.Generic;

namespace ContentPackCreator
{
    internal class ManifestData
    {
        public string Name = "";
        public string Author = "";
        public string Version = "";
        public string Description = "";
        public string UniqueID = "";
        public string MinimumApiVersion;
        public ContentPackForData ContentPackFor = new ContentPackForData();
        public List<DependencyData> Dependencies = new List<DependencyData>();
    }

    public class DependencyData
    {
        public string UniqueID = "";
        public bool IsRequired = true;
        public string MinimumVersion;
    }

    public class ContentPackForData
    {
        public string UniqueID = "";
        public string MinimumVersion;
    }
}