using System.Collections.Generic;

namespace ProceduralDialogue
{
    public class ProceduralDialogue
    {
        public string language = "en";
        public string baseString;
        public List<string> characters = null;
        public List<List<string>> options = new List<List<string>>();
    }
}