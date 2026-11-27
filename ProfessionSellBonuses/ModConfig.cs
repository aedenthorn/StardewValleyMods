using System.Collections.Generic;

namespace ProfessionSellBonuses
{
    public class ModConfig
    {
        public bool ModEnabled { get; set; } = true;
        public bool Debug { get; set; } = false;
        public Dictionary<string, SellBonusData> Professions { get; set; } = new();
        public Dictionary<string, SellBonusData> Skills { get; set; } = new();
    }
}
