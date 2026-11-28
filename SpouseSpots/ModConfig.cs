using System.Collections.Generic;

namespace SpouseSpots
{
    public class ModConfig
    {
        public bool ModEnabled { get; set; } = true;
        public bool Debug { get; set; } = false;
        public float ChancePerTick { get; set; } = 0.0006f;
        public float ChanceAtNewDay { get; set; } = 0.25f;
        public Dictionary<string, Dictionary<string, SpotData>> Spots { get; set; } = new();
        public Dictionary<string, string> Animations { get; set; } = new();
        public Dictionary<string, string> Dialogues { get; set; } = new();
    }
}
