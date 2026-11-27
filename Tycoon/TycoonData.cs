using Microsoft.Xna.Framework;
using StardewValley.GameData.Locations;
using StardewValley.GameData.Minecarts;

namespace Tycoon
{
    public class TycoonData
    {
        public int Price;
        public string Name;
        public string Description;
        public string DeedPath;
        public string Shop = "Carpenter";
        public string Network = "Default";
        public MinecartDestinationData MinecartData;
    }
}