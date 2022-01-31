using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewValley;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using xTile.Layers;
using xTile.Tiles;

namespace CustomSpousePatioRedux
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry
    {


        public static bool Farm_CacheOffBasePatioArea_Prefix(Farm __instance)
        {
            if (!Config.EnableMod || outdoorAreas == null || outdoorAreas.areas.Count == 0)
                return true;

            baseSpouseAreaTiles = new Dictionary<string, Dictionary<string, Dictionary<Point, xTile.Tiles.Tile>>>();
            foreach(var data in outdoorAreas.areas)
            {
                baseSpouseAreaTiles[data.Key] = new Dictionary<string, Dictionary<Point, Tile>>();

                List<string> layers_to_cache = new List<string>();
                foreach (Layer layer in __instance.map.Layers)
                {
                    layers_to_cache.Add(layer.Id);
                }
                foreach (string layer_name in layers_to_cache)
                {
                    Layer original_layer = __instance.map.GetLayer(layer_name);
                    Dictionary<Point, Tile> tiles = new Dictionary<Point, Tile>();
                    baseSpouseAreaTiles[data.Key][layer_name] = tiles;
                    Vector2 spouse_area_corner = data.Value;
                    for (int x = (int)spouse_area_corner.X; x < (int)spouse_area_corner.X + 4; x++)
                    {
                        for (int y = (int)spouse_area_corner.Y; y < (int)spouse_area_corner.Y + 4; y++)
                        {
                            if (original_layer == null)
                            {
                                tiles[new Point(x, y)] = null;
                            }
                            else
                            {
                                tiles[new Point(x, y)] = original_layer.Tiles[x, y];
                            }
                        }
                    }
                }
            }

            return false;
        }
        public static bool Farm_ReapplyBasePatioArea_Prefix(Farm __instance)
        {
            if (!Config.EnableMod || outdoorAreas == null || outdoorAreas.areas.Count == 0)
                return true;
            if (addingExtraAreas)
                return false;

            foreach (var kvp in baseSpouseAreaTiles)
            {
                foreach (string layer in baseSpouseAreaTiles[kvp.Key].Keys)
                {
                    Layer map_layer = __instance.map.GetLayer(layer);
                    foreach (Point location in baseSpouseAreaTiles[kvp.Key][layer].Keys)
                    {
                        Tile base_tile = baseSpouseAreaTiles[kvp.Key][layer][location];
                        if (map_layer != null)
                        {
                            map_layer.Tiles[location.X, location.Y] = base_tile;
                        }
                    }
                }
            }
            return false;
        }
        public static IEnumerable<CodeInstruction> Farm_addSpouseOutdoorArea_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            SMonitor.Log($"Transpiling Farm.addSpouseOutdoorArea");

            var codes = new List<CodeInstruction>(instructions);
            for (int i = 0; i < codes.Count; i++)
            {
                if (i > 0 && codes[i - 1].opcode == OpCodes.Ldarg_0 && codes[i].opcode == OpCodes.Callvirt && (MethodInfo)codes[i].operand == AccessTools.Method(typeof(Farm), nameof(Farm.GetSpouseOutdoorAreaCorner)))
                {
                    SMonitor.Log("Overriding Farm.GetSpouseOutdoorAreaCorner");
                    codes[i - 1] = new CodeInstruction(OpCodes.Ldarg_1);
                    codes[i] = new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ModEntry.GetSpouseOutdoorAreaCorner)));
                }
            }

            return codes.AsEnumerable();
        }

        public static bool addingExtraAreas = false;

        public static void Farm_addSpouseOutdoorArea_Postfix(Farm __instance, string spouseName)
        {
            if (!Config.EnableMod || outdoorAreas == null || outdoorAreas.areas.Count == 0 || spouseName == "" || spouseName == null)
                return;
            spousePositions[spouseName] = __instance.spousePatioSpot;
            if (addingExtraAreas)
                return;
            addingExtraAreas = true;
            foreach(var name in outdoorAreas.areas.Keys)
            {
                if(name != spouseName)
                    __instance.addSpouseOutdoorArea(name);
            }
            addingExtraAreas = false;
        }

        
        public static bool NPC_GetSpousePatioPosition_Prefix(NPC __instance, ref Vector2 __result)
        {
            if (!Config.EnableMod || outdoorAreas == null || outdoorAreas.areas.Count == 0 || !spousePositions.ContainsKey(__instance.Name))
                return true;
            __result = spousePositions[__instance.Name].ToVector2();
            return false;
        }
    }
}