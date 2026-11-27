using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Objects;

namespace PrismaticFlowers
{
    public interface IPrismaticFlowersAPI
    {
        public bool MakePrismatic(object obj);
        public Color GetPrismaticColorForCropHarvest(string id, int offset, int x, int y);
        public Color GetPrismaticColorForItemId(string id, int offset);
        public Color GetPrismaticColorForObject(object obj, Color fallback);
    }
    public class PrismaticFlowersAPI : IPrismaticFlowersAPI
    {
        bool IPrismaticFlowersAPI.MakePrismatic(object obj)
        {
            return ModEntry.MakePrismatic(obj);
        }
        Color IPrismaticFlowersAPI.GetPrismaticColorForCropHarvest(string id, int offset,int x, int y)
        {
            return ModEntry.GetPrismaticColorForID(id, offset, true, x, y);
        }
        Color IPrismaticFlowersAPI.GetPrismaticColorForItemId(string id, int offset)
        {
            return ModEntry.GetPrismaticColorForID(id, offset, false, 0, 0);
        }
        Color IPrismaticFlowersAPI.GetPrismaticColorForObject(object obj, Color fallback)
        {
            return ModEntry.GetPrismaticColorForObject(fallback, obj);
        }
    }
}