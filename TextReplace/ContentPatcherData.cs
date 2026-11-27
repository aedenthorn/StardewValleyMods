using System.Collections.Generic;

namespace TextReplace
{
    public class ContentPatcherData
    {
        public string Format = "2.9.0";
        public List<ChangeData> Changes = new();
    }

    public class ChangeData
    {
        public string Action = "EditData";
        public string Target = "aedenthorn.TextReplace/dict";
        public Dictionary<string, ReplaceDict> Entries = new();
    }
}