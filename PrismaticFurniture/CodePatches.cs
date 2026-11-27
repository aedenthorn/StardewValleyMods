using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Object = StardewValley.Object;

namespace PrismaticFurniture
{
    public partial class ModEntry
    {

        [HarmonyPatch(typeof(Furniture), nameof(Furniture.draw))]
        public class Furniture_draw_Patch
        {
            public static void Postfix(Furniture __instance, SpriteBatch spriteBatch, int x, int y, NetVector2 ___drawPosition, NetInt ___sourceIndexOffset)
            {
                if (!Config.ModEnabled  || !furnitureDict.TryGetValue(__instance.Name, out PrismaticFurnitureData data))
                    return;

                Rectangle drawn_source_rect = new Rectangle(0, 0, __instance.sourceRect.Width, __instance.sourceRect.Height);

                drawn_source_rect.X += drawn_source_rect.Width * ___sourceIndexOffset.Value;

                var depth = __instance.HasSittingFarmers() && __instance.sourceRect.Right <= Furniture.furnitureFrontTexture.Width && __instance.sourceRect.Bottom <= Furniture.furnitureFrontTexture.Height ? ((__instance.boundingBox.Value.Top + 16) / 10000f + 0.00001f) : ((__instance.furniture_type.Value == 12) ? (2E-09f + __instance.TileLocation.Y / 100000f) : ((float)(__instance.boundingBox.Value.Bottom - ((__instance.furniture_type.Value == 6 || __instance.furniture_type.Value == 17 || __instance.furniture_type.Value == 13) ? 48 : 8)) / 10000f) + 0.001f);

                spriteBatch.Draw(data.texture, Game1.GlobalToLocal(Game1.viewport, ___drawPosition + ((__instance.shakeTimer > 0) ? new Vector2((float)Game1.random.Next(-1, 2), (float)Game1.random.Next(-1, 2)) : Vector2.Zero)), drawn_source_rect,  Utility.GetPrismaticColor(data.offset, data.speed), 0f, Vector2.Zero, 4f, __instance.Flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, depth);
            }
        }
        [HarmonyPatch(typeof(BedFurniture), nameof(BedFurniture.draw))]
        public class BedFurniture_draw_Patch
        {
            public static void Postfix(BedFurniture __instance, SpriteBatch spriteBatch, int x, int y, NetVector2 ___drawPosition)
            {
                if (!Config.ModEnabled || !Furniture.isDrawingLocationFurniture || !furnitureDict.TryGetValue(__instance.Name, out PrismaticFurnitureData data))
                    return;

                spriteBatch.Draw(data.texture, Game1.GlobalToLocal(Game1.viewport, ___drawPosition + ((__instance.shakeTimer > 0) ? new Vector2((float)Game1.random.Next(-1, 2), (float)Game1.random.Next(-1, 2)) : Vector2.Zero)),new Rectangle(0, 0, data.texture.Width, data.texture.Height),  Utility.GetPrismaticColor(data.offset, data.speed), 0f, Vector2.Zero, 4f, __instance.Flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (__instance.furniture_type.Value == 12) ? (2E-09f + __instance.TileLocation.Y / 100000f) : ((float)(__instance.boundingBox.Value.Bottom - ((__instance.furniture_type.Value == 6 || __instance.furniture_type.Value == 17 || __instance.furniture_type.Value == 13) ? 48 : 8)) / 10000f + 0.001f));
            }
        }
    }
}