using System.Collections.Generic;

namespace ChestFullnessTextures
{
    internal class TempJson
    {
        public string Format = "1.28.4";
        public List<Temp2Json> Changes = new();
    }

    public class Temp2Json
    {
        public string Action;
        public string Target;
        public string FromFile;
        public Dictionary<string, ChestTextureDataShell> Entries;
    }
}