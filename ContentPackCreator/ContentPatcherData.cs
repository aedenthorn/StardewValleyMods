using System.Collections.Generic;

namespace ContentPackCreator
{
    public class ContentPatcherData
    {
        public string Format;
        public List<ChangeData> Changes = new List<ChangeData>();
        public Dictionary<string, ConfigData> ConfigSchema = new Dictionary<string, ConfigData>();

    }
}