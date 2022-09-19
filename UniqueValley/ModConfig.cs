using StardewModdingAPI;
using System.Collections.Generic;

namespace UniqueValley
{
    public class ModConfig
    {
        public bool ModEnabled { get; set; } = true;
        public string ForbiddenList { get; set; } = "George";
        public bool MaintainGender { get; set; } = true;
        public bool MaintainAge { get; set; } = true;
        public bool MaintainDatable { get; set; } = true;
        public bool RandomizeGiftTastes { get; set; } = false;
    }
}
