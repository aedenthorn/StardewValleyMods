
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace FuzzyTime
{
    public class ModConfig
    {
        public bool ModEnabled { get; set; } = true;
        public int ScrollInterval { get; set; } = 500;
        public string FontType { get; set; } = "small";
        public Dictionary<int, string> TimeNames { get; set; } = new()
        {
            { 600, "early" },
            { 830, "morning" },
            { 1100, "midday" },
            { 1300, "afternoon" },
            { 1630, "evening" },
            { 1930, "night" },
            { 2200, "midnight" },
            { 2400, "late" }
        };
    }
}
