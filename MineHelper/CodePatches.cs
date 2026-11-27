using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley;
using StardewValley.Extensions;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Network;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using xTile;
using xTile.Dimensions;
using xTile.Tiles;

namespace MineHelper
{
    public partial class ModEntry
    {
        [HarmonyPatch(typeof(GameLocation), nameof(GameLocation.checkAction))]
        public static class GameLocation_checkAction_Patch
        {
            public static bool Prefix(GameLocation __instance, Location tileLocation, xTile.Dimensions.Rectangle viewport, Farmer who, ref bool __result)
            {
                if (!Config.ModEnabled || !SHelper.Input.IsDown(Config.ModKey))
                    return true;
                if(__instance.Name == "Mine" || __instance.Name == "SkullCave")
                {
                    Tile tile = __instance.map.RequireLayer("Buildings").PickTile(new Location(tileLocation.X * 64, tileLocation.Y * 64), viewport.Size);
                    if(tile.Properties.TryGetValue("Action", out var str) && str == "MineElevator")
                    {
                        if (OpenChest(__instance))
                        {
                            __result = true;
                            return false;
                        }
                    }
                }
                return true;
            }
        }
        [HarmonyPatch(typeof(MineShaft), nameof(MineShaft.checkAction))]
        public static class MineShaft_checkAction_Patch
        {
            public static bool Prefix(MineShaft __instance, Location tileLocation, xTile.Dimensions.Rectangle viewport, Farmer who, ref bool __result)
            {
                if (!Config.ModEnabled || !SHelper.Input.IsDown(Config.ModKey))
                    return true;
                int tileIndexAt = __instance.getTileIndexAt(tileLocation, "Buildings", "mine");
                if (tileIndexAt == 112)
                {
                    if (OpenChest(Game1.getLocationFromName(__instance.mineLevel > 120 ? "SkullCave" : "Mine")))
                    {
                        __result = true;
                        return false;
                    }
                }
                return true;
            }
        }
        [HarmonyPatch(typeof(Chest), nameof(Chest.draw), new Type[] { typeof(SpriteBatch), typeof(int ), typeof(int), typeof(float) })]
        public static class Chest_draw_Patch
        {
            public static bool Prefix(Chest __instance)
            {
                if (!Config.ModEnabled || !__instance.modData.ContainsKey(chestKey))
                    return true;
                return false;
            }
        }
        //[HarmonyPatch(typeof(FarmAnimal), nameof(FarmAnimal.behaviors))]
        public static class FarmAnimal_behaviors_Patch
        {
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                SMonitor.Log($"Transpiling FarmAnimal.behaviors");

                var codes = new List<CodeInstruction>(instructions);
                for (int i = 0; i < codes.Count; i++)
                {
                    if (codes[i].opcode == OpCodes.Callvirt && codes[i].operand is MethodInfo mi && mi == AccessTools.Method(typeof(NetDictionary<Vector2, TerrainFeature, NetRef<TerrainFeature>, SerializableDictionary<Vector2, TerrainFeature>, NetVector2Dictionary<TerrainFeature, NetRef<TerrainFeature>>>), "ContainsKey"))
                    {
                        SMonitor.Log("Intercepting check for terrain feature");
                        codes[i].opcode = OpCodes.Call;
                        //codes[i].operand = AccessTools.Method(typeof(ModEntry), nameof(CheckForGrass));
                    }
                }

                return codes.AsEnumerable();
            }
        }
    }
}