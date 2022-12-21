using System;

namespace StardewOpenWorld
{
    public interface IStardewOpenWorldAPI
    {
        public void RegisterBiome(string id, Func<int, int, int, WorldTile> func);
    }
    public class StardewOpenWorldAPI : IStardewOpenWorldAPI
    {
        public void RegisterBiome(string id, Func<int, int, int, WorldTile> func)
        {
            ModEntry.biomes[id] = func;
        }
    }
}