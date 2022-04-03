using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Object = StardewValley.Object;

namespace WallTelevision
{
    public partial class ModEntry
    {


        [HarmonyPatch(typeof(Furniture), nameof(Furniture.canBePlacedHere))]
        public class Furniture_canBePlacedHere_Patch
        {
            public static bool Prefix(Furniture __instance, GameLocation l, Vector2 tile, ref bool __result)
            {
                if (!Config.EnableMod || __instance is not TV || __instance.Name.Contains("Budget") || __instance.Name.Contains("Floor") || !typeof(DecoratableLocation).IsAssignableFrom(l.GetType()) || !(l as DecoratableLocation).isTileOnWall((int)tile.X, (int)tile.Y) || (l as DecoratableLocation).isTileOnWall((int)tile.X, (int)tile.Y + 1))
                    return true;
                __result = true;
                return false;
            }
        }
        
        [HarmonyPatch(typeof(Furniture), nameof(Furniture.GetAdditionalFurniturePlacementStatus))]
        public class Furniture_GetAdditionalFurniturePlacementStatus_Patch
        {
            public static bool Prefix(Furniture __instance, GameLocation location, int x, int y, ref int __result)
            {
                if (!Config.EnableMod || __instance is not TV || __instance.Name.Contains("Budget") || __instance.Name.Contains("Floor") || !typeof(DecoratableLocation).IsAssignableFrom(location.GetType()) || !(location as DecoratableLocation).isTileOnWall((int)x / 64, (int)y / 64) || (location as DecoratableLocation).isTileOnWall(x / 64, y / 64 + 1))
                    return true;
                __instance.TileLocation = new Vector2(x / 64, y / 64);
                __result = 0;
                return false;
            }
        }

        [HarmonyPatch(typeof(Utility), nameof(Utility.playerCanPlaceItemHere))]
        public class playerCanPlaceItemHere_Patch
        {
            public static bool Prefix(GameLocation location, Item item, int x, int y, Farmer f, ref bool __result)
            {
                if (!Config.EnableMod || item is not TV || item.Name.Contains("Budget") || item.Name.Contains("Floor") || !typeof(DecoratableLocation).IsAssignableFrom(location.GetType()) || !(location as DecoratableLocation).isTileOnWall(x / 64, y / 64) || (location as DecoratableLocation).isTileOnWall(x / 64, y / 64 + 1) || !Utility.isWithinTileWithLeeway(x, y, item, f))
                    return true;
                __result = true;
                return false;
            }
        }
        [HarmonyPatch(typeof(Furniture), nameof(Furniture.draw), new Type[] { typeof(SpriteBatch), typeof(int), typeof(int), typeof(float) })]
        public class Furniture_draw_Patch
        {
            public static bool Prefix(Furniture __instance, SpriteBatch spriteBatch, int x, int y, float alpha, NetVector2 ___drawPosition)
            {
                if (!Config.EnableMod || __instance is not TV || (!__instance.Name.Contains("Plasma")  && !__instance.Name.Contains("Tropical")) || !typeof(DecoratableLocation).IsAssignableFrom(Game1.currentLocation.GetType()))
                    return true;
                if (!(Game1.currentLocation as DecoratableLocation).isTileOnWall((int)__instance.TileLocation.X, (int)__instance.TileLocation.Y))
                    return true;

                Rectangle source = new Rectangle(0, 0, 48, 48); 

                if (Furniture.isDrawingLocationFurniture)
                {
                    spriteBatch.Draw(__instance.Name.Contains("Plasma") ? plasmaTexture : tropicalTexture, Game1.GlobalToLocal(Game1.viewport, ___drawPosition + ((__instance.shakeTimer > 0) ? new Vector2((float)Game1.random.Next(-1, 2), (float)Game1.random.Next(-1, 2)) : Vector2.Zero)), source, Color.White * alpha, 0f, Vector2.Zero, 4f, __instance.Flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (__instance.boundingBox.Value.Bottom - 8) / 10000f);
                }
                else
                {
                    spriteBatch.Draw(__instance.Name.Contains("Plasma") ? plasmaTexture : tropicalTexture, Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(x * 64 + ((__instance.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0)), (float)(y * 64 - (__instance.sourceRect.Height * 4 - __instance.boundingBox.Height) + ((__instance.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0)))), source, Color.White * alpha, 0f, Vector2.Zero, 4f, __instance.Flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (__instance.boundingBox.Value.Bottom - 8 ) / 10000f);
                }
                return false;
            }
        }
    }
}