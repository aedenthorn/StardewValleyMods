using System.Collections.Generic;

namespace StardewVN
{
    public class VNDialogue
    {
        public Dictionary<string, VNOption> Text { get; set; }
        public Dictionary<string, VNOption> Action { get; set; }
    }
}