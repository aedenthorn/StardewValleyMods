using Microsoft.Xna.Framework;

namespace FillableVases
{
    public interface IPrismaticFlowersAPI
    {
        public Color GetPrismaticColorForCropHarvest(string id, int offset, int x, int y);
        public Color GetPrismaticColorForItemId(string id, int offset);
        public Color GetPrismaticColorForObject(object obj, Color fallback);
    }
}