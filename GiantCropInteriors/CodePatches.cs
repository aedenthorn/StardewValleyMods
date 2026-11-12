using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Audio;
using StardewValley.Buildings;
using StardewValley.Locations;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using xTile.Dimensions;
using xTile.Layers;
using xTile.Tiles;

namespace GiantCropInteriors
{
    public partial class ModEntry
    {

        //[HarmonyPatch(typeof(GiantCrop), nameof(GiantCrop.draw), new Type[] { typeof(SpriteBatch) })]
        public class GiantCrop_draw_Patch
        {
            public static void Postfix(GiantCrop __instance)
            {
                
            }
        }

        //[HarmonyPatch(typeof(Building), nameof(Building.draw), new Type[] { typeof(SpriteBatch) })]
        public class Building_draw_Patch
        {
            public static void Postfix(Building __instance)
            {
                
            }
        }

        [HarmonyPatch(typeof(GiantCrop), nameof(GiantCrop.performToolAction))]
        public class GiantCrop_performToolAction_Patch
        {
            public static bool Prefix(GiantCrop __instance)
            {
                if (!BuildingDict.ContainsKey(__instance.Id))
                    return true;
                var x = __instance.Location.buildings.FirstOrDefault(i => (int)__instance.Tile.X == i.tileX.Value && (int)__instance.Tile.Y == i.tileY.Value);
                if (__instance.Location.buildings.FirstOrDefault(i => (int)__instance.Tile.X == i.tileX.Value && (int)__instance.Tile.Y == i.tileY.Value) is Building exists && exists.buildingType.Value == modPrefix + __instance.Id && exists.GetIndoors() is GameLocation l && (l.farmers.Any() || l.characters.Any() || l.animals.Any() || ((l.furniture.Any() || l.objects.Values.Where(o => o.Fragility != 2).Any()) && Config.ProtectObjects)))
                {
                    Game1.showRedMessage(SHelper.Translation.Get("not-empty"));
                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(DecoratableLocation), "IsFloorableTile")]
        public class DecoratableLocation_IsFloorableTile_Patch
        {
            public static void Postfix(DecoratableLocation __instance, int x, int y, ref bool __result)
            {
                if (!__result)
                    return;

                string floor_id = __instance.GetFloorID(x, y);
                __result = floor_id != null;
            }
        }
    }
}