using StardewValley;
using StardewValley.Locations;
using System.Collections.Generic;

namespace FreeLove
{
    public class FreeLoveAPI
    {
        public void PlaceSpousesInFarmhouse(FarmHouse farmHouse)
        {
            Misc.PlaceSpousesInFarmhouse(farmHouse);
        }
        public static Dictionary<string, NPC> GetSpouses(Farmer farmer, int all = -1)
        {
            return Misc.GetSpouses(farmer, all);
        }
    }
}