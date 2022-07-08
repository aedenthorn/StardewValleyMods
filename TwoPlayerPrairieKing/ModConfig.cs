using System.Collections.Generic;

namespace TwoPlayerPrairieKing
{
    public class ModConfig
    {
        public bool ModEnabled { get; set; } = true;
        public int MinHearts { get; set; } = 4;
        public int NamesPerPage { get; set; } = 6;
        public bool SameLocation { get; set; } = false;
    }
}
