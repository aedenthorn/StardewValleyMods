using Microsoft.Xna.Framework;
using StardewValley;

namespace CropStacking
{
    public class ItemData
    {
        public string id;
        public int stack;
        public int quality;
        public Object.PreserveType preserveType;
        public string preservedParentSheetIndex;
        public Color? color;
    }
}