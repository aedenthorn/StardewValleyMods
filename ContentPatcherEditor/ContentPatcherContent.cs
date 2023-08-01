using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace ContentPatcherEditor
{
    public class ContentPatcherContent
    {
        public string Format;
        public List<JObject> Changes;
        public Dictionary<string, JObject> ConfigSchema;
    }
}