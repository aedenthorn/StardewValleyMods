using System.Collections.Generic;

namespace StardewVN
{
    public class VNScene
    {
        public List<VNOption> Actions { get; set; }
        public List<VNOption> Objects { get; set; }
        public List<VNOptionUnique> Background { get; set; }
        public List<VNOptionUnique> Music { get; set; }
    }
}