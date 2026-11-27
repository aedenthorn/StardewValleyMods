using Microsoft.Xna.Framework;

namespace FurnitureDisplayFramework
{
    public class FurnitureDisplayData
    {
        public string name;
        public FurnitureDisplaySlot[] slots;
    }

    public class FurnitureDisplaySlot
    {
        public Rectangle slotRect;
        public Rectangle itemRect;
    }
}