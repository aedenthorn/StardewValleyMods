using StardewModdingAPI;
using System.Collections.Generic;

namespace ContentPatcherEditor
{
    public class ContentPatcherPack
    {
        public MyManifest manifest;
        public ContentPatcherContent content;
        public string directory;
        public List<KeyValuePair<string, ConfigVar>> config;
    }
}