using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Locations;

namespace FreeLove
{
    public interface ISpouseSpotAPI
    {
        public bool SetRandomSpot(NPC npc, FarmHouse location, bool dialogue = true, bool animation = true);
        public (Point, int) GetRandomSpotFacing(string npc, FarmHouse location);
        public float ChanceAtNewDay();
        public float ChancePerTick();
    }
}