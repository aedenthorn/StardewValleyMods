using StardewValley;
using StardewValley.Locations;
using System.Collections.Generic;

namespace WeddingTweaks
{
    public interface IFreeLoveAPI
    {
        public void PlaceSpousesInFarmhouse(FarmHouse farmHouse);
        public Dictionary<string, NPC> GetSpouses(Farmer farmer, bool all = true);

    }
}