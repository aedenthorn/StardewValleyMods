using System.Collections.Generic;

namespace StardewVN
{
    public class VNOption
    {
        public string Which { get; set; }
        public float Probability { get; set; } = 1;
        public VNRequirement Requirement { get; set; }
    }
}