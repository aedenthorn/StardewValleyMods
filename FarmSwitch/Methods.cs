using StardewValley;
using System.Collections.Generic;
using System.Linq;

namespace FarmSwitch
{
    public partial class ModEntry
    {
        public static List<string> GetFarms()
        {
            List<string> farms = new List<string>()
                {
                    "Farm", "Farm_Fishing", "Farm_Foraging", "Farm_Mining", "Farm_Combat", "Farm_FourCorners", "Farm_Island"
                };
            farms.AddRange(DataLoader.AdditionalFarms(Game1.content).Select(f => f.MapName));
            return farms;
        }
    }
}