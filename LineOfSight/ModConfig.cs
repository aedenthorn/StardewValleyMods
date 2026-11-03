using System.Collections.Generic;

namespace LineOfSight
{
    public class ModConfig
    {
        public bool ModEnabled { get; set; } = true;
        public bool Debug { get; set; } = false;
        public int MaxDistance { get; set; } = 10;
        public HashSet<string> ExcludedLocations { get; set; } = new HashSet<string>();
        public HashSet<string> ExcludedLocationTypes { get; set; } = new HashSet<string>();
    }
}
