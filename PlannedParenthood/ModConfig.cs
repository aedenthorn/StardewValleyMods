using System.Collections.Generic;

namespace PlannedParenthood
{
    public class ModConfig
    {
        public bool ModEnabled { get; set; } = true;
        public int MinHearts { get; set; } = 10;
        public int NamesPerPage { get; set; } = 6;
        public bool InBed { get; set; } = false;
    }
}
