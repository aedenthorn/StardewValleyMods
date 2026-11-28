using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Locations;

namespace SpouseSpots
{
    public interface ISpouseSpotAPI
    {
        public bool SetRandomSpot(NPC npc, FarmHouse location, bool dialogue = true, bool animation = true);
        public (Point, int) GetRandomSpotFacing(string npc, FarmHouse location);
        public float ChanceAtNewDay();
        public float ChancePerTick();
    }
    public class SpouseSpotAPI: ISpouseSpotAPI
    {
        public bool SetRandomSpot(NPC npc, FarmHouse location, bool dialogue = true, bool animation = true)
        {
            return ModEntry.SetRandomSpot(npc, location, dialogue, animation);
        }
        public (Point, int) GetRandomSpotFacing(string name, FarmHouse location)
        {
            var kvp = ModEntry.GetRandomSpot(name, location);
            if (kvp.Key == null)
                return (Point.Zero, -1);
            return (kvp.Value.Tile, kvp.Value.Facing);
        }
        public float ChanceAtNewDay()
        {
            return ModEntry.Config.ChanceAtNewDay;
        }
        public float ChancePerTick()
        {
            return ModEntry.Config.ChancePerTick;
        }
    }
}