using Microsoft.Xna.Framework;

namespace FlowerColors
{
    public interface IPrismaticFlowersAPI
    {
        public bool MakePrismatic(object obj);
        public Color GetPrismaticColorForCropHarvest(string id, int offset, int x, int y);
        public Color GetPrismaticColorForItemId(string id, int offset);
        public Color GetPrismaticColorForObject(object obj, Color fallback);
    }
}