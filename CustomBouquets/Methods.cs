using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Objects;
using Object = StardewValley.Object;

namespace CustomBouquets
{
    public partial class ModEntry
    {
        public static Color GetColor(string color)
        {
            string[] bytes = color.Split(',');
            Color c = new Color(byte.Parse(bytes[0]), byte.Parse(bytes[1]), byte.Parse(bytes[2]));
            if(c == new Color(6, 6, 6))
            {
                c = Utility.GetPrismaticColor();
            }
            return c;
        }
        public static void CacheTextures()
        {
            if (flower1 == null)
            {
                flower1 = SHelper.GameContent.Load<Texture2D>(flowerPath1);
                flower2 = SHelper.GameContent.Load<Texture2D>(flowerPath2);
                flower3 = SHelper.GameContent.Load<Texture2D>(flowerPath3);
                bouquet = SHelper.GameContent.Load<Texture2D>(bouquetPath);
            }
        }
        public static bool CheckBouquet(Object obj, Furniture f, SpriteBatch spriteBatch, int x, int y, float alpha)
        {
            if (!Config.EnableMod || obj?.modData.TryGetValue(flowerPath1, out var f1) != true)
            {
                return obj != null;
            }
            obj.drawInMenu(spriteBatch, Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(f.boundingBox.Center.X - 32), (float)(f.boundingBox.Center.Y - (f.drawHeldObjectLow.Value ? 32 : 85)))), 1f, 1f, (float)(f.boundingBox.Bottom + 1) / 10000f, StackDrawType.Hide, Color.White * alpha, false);
            return false;
        }
    }
}