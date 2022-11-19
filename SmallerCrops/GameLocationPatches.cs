using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.TerrainFeatures;
using System;
using xTile.Dimensions;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace SmallerCrops
{
    public partial class ModEntry
    {

        [HarmonyPatch(typeof(GameLocation), nameof(GameLocation.checkAction))]
        public static class GameLocation_checkAction_Patch
        {
            public static bool Prefix(GameLocation __instance, Location tileLocation, xTile.Dimensions.Rectangle viewport, Farmer who)
            {
                if (!Config.ModEnabled || !Context.IsWorldReady)
                    return true;
                var mousePos = Game1.getMousePosition();
                var box = new Rectangle(Utility.Vector2ToPoint(Game1.GlobalToLocal(new Vector2(tileLocation.X, tileLocation.Y) * 64)), new Point(64, 64));
                if (!box.Contains(mousePos))
                    return true;
                var offset = mousePos - box.Location;
                if (offset.X < 32 && offset.Y < 32)
                    return true;
                int idx = 1;
                if (offset.Y > 32)
                {
                    if (offset.X > 32)
                    {
                        idx = 3;
                    }
                    else
                    {
                        idx = 2;
                    }
                }
                Location tile = tileLocation + new Location(tileOffset * idx, tileOffset * idx);
                return true;
            }
        }

        [HarmonyPatch(typeof(GameLocation), nameof(GameLocation.isTileOnMap), new Type[] { typeof(Vector2) })]
        public static class GameLocation_isTileOnMap_Patch
        {
            public static void Postfix(GameLocation __instance, Vector2 position, ref bool __result)
            {
                if (!Config.ModEnabled || __result || position.X < tileOffset)
                    return;
                __result = __instance.terrainFeatures.ContainsKey(position);
            }
        }
        [HarmonyPatch(typeof(GameLocation), nameof(GameLocation.makeHoeDirt))]
        public static class GameLocation_makeHoeDirt_Patch
        {
            public static void Prefix(GameLocation __instance, Vector2 tileLocation, ref bool __state)
            {
                if (!Config.ModEnabled)
                    return;
                __state = __instance.terrainFeatures.ContainsKey(tileLocation);
            }
            public static void Postfix(GameLocation __instance, Vector2 tileLocation, bool __state)
            {
                if (!Config.ModEnabled || !__instance.terrainFeatures.ContainsKey(tileLocation) || __state)
                    return;
                for(int i = 1; i < 4; i++)
                {
                    var tile = tileLocation + new Vector2(tileOffset * i, tileOffset * i);
                    if (!__instance.terrainFeatures.ContainsKey(tile))
                    {
                        __instance.terrainFeatures.Add(tile, new HoeDirt((Game1.IsRainingHere(__instance) && __instance.IsOutdoors && !__instance.Name.Equals("Desert")) ? 1 : 0, __instance));
                    }
                }
            }
        }

    }
}