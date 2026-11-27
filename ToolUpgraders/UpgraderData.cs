using System.Collections.Generic;

namespace ToolUpgraders
{
    public class UpgraderData
    {
        public string ShopName { get; set; }
        public string TileAction { get; set; }
        public IEnumerable<string> Tools { get; set; }
        public int UpgradeDays { get; set; }
        public string ReadyText { get; set; }
        public string BeginText { get; set; }
        public string NoSpaceText { get; set; }
        public string BeginSound { get; set; }
        public string BeginNPC { get; set; }
    }
}