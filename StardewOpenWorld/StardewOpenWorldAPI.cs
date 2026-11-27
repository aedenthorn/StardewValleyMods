using System;

namespace StardewOpenWorld
{
    public interface IStardewOpenWorldAPI
    {
        public void RegisterBiome(string id, Func<ulong, int, int, WorldChunk> func);
        public int GetChunkSize();
        public int GetWorldSize();
    }
    public class StardewOpenWorldAPI : IStardewOpenWorldAPI
    {
        public void RegisterBiome(string id, Func<ulong, int, int, WorldChunk> func)
        {
            ModEntry.biomeCodeDict[id] = func;
        }
        public int GetChunkSize()
        {
            return ModEntry.openWorldChunkSize;
        }
        public int GetWorldSize()
        {
            return ModEntry.Config.OpenWorldSize;
        }
    }
}