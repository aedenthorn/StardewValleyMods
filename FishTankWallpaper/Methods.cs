using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Objects;

namespace FishTankWallpaper
{
    public partial class ModEntry
    {
        private static Texture2D CheckTexture(Texture2D texture, Furniture f)
        {
            if (!Config.ModEnabled || !f.modData.ContainsKey(modKey) || !TankDict.TryGetValue(f.ItemId, out var data))
            {
                return texture;
            }
            return SHelper.GameContent.Load<Texture2D>(data.Texture);
        }

    }
}