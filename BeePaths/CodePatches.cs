using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using xTile.Dimensions;
using xTile.Tiles;
using xTile;
using Rectangle = Microsoft.Xna.Framework.Rectangle;
using System.Text;
using StardewValley.TerrainFeatures;
using Object = StardewValley.Object;

namespace BeePaths
{
    public partial class ModEntry
    {

        [HarmonyPatch(typeof(Utility), nameof(Utility.findCloseFlower), new Type[] { typeof(GameLocation), typeof(Vector2), typeof(int), typeof(Func<Crop, bool>) })]
        public class findCloseFlower_Patch
        {
            public static bool Prefix(GameLocation location, Vector2 startTileLocation, ref int range, Func<Crop, bool> additional_check, ref Crop __result)
            {
                if (!Config.ModEnabled || !Config.FixFlowerFind)
                    return true;
                range = Config.BeeRange;
                Vector2 closest = Vector2.Zero;
                float closestDistance = float.MaxValue;
                foreach(var kvp in location.terrainFeatures.Pairs)
                {
                    if(kvp.Value is not HoeDirt || (kvp.Value as HoeDirt).crop is null || new Object((kvp.Value as HoeDirt).crop.indexOfHarvest.Value, 1, false, -1, 0).Category != -80 || (kvp.Value as HoeDirt).crop.currentPhase.Value < (kvp.Value as HoeDirt).crop.phaseDays.Count - 1 || (kvp.Value as HoeDirt).crop.dead.Value || (additional_check != null && !additional_check((kvp.Value as HoeDirt).crop)))
                        continue;

                    var distance = Vector2.Distance(startTileLocation, kvp.Key);
                    if (distance <= range && distance < closestDistance)
                    {
                        closest = kvp.Key;
                        closestDistance = distance;
                        __result = (kvp.Value as HoeDirt).crop;
                        AccessTools.FieldRefAccess<Crop, Vector2>(__result, "tilePosition") = kvp.Key;
                    }
                }
                return false;
            }   
        }
    }
}