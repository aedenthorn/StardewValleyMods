using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Objects;
using System;

namespace BedTweaks
{
    public partial class ModEntry
    {

        public static bool BedFurniture_draw_Prefix(BedFurniture __instance, SpriteBatch spriteBatch, int x, int y, float alpha, NetVector2 ___drawPosition)
        {
            if (__instance.isTemporarilyInvisible || __instance.bedType != BedFurniture.BedType.Double)
                return true;

            if (!Config.EnableMod)
            {
                __instance.boundingBox.Width = 3 * 64;
                return true;
            }

            Vector2 drawPosition = Furniture.isDrawingLocationFurniture ? ___drawPosition.Value : new Vector2(x * 64, y * 64 - (__instance.sourceRect.Height * 4 - __instance.boundingBox.Height));

            return drawBedAtLocation(__instance, spriteBatch, drawPosition, alpha);
        }
    }
}