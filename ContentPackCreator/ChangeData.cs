using System.Collections.Generic;
using System.Drawing;

namespace ContentPackCreator
{
    public class ChangeData
    {
        public string Action = "";
        public string Target = "";
        public string LogName = "";
        public bool[] Update;
        
        public string FromFile;
        public Dictionary<string, string> When;
        
        public string PatchMode;
        public Rectangle FromArea;
        public Rectangle ToArea;

        public Dictionary<string, string> MapProperties;
        public List<string> AddWarps;
        public List<TextOperation> TextOperations;
    }

    public class TextOperation
    {
        public string Operation;
        public string Value;
        public string Delimiter;
        public List<string> Target;
    }
}