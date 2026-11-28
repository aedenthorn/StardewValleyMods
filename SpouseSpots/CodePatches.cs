using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Pathfinding;
using System;

namespace SpouseSpots
{
    public partial class ModEntry
    {
        [HarmonyPatch(typeof(FarmHouse), nameof(FarmHouse.updateEvenIfFarmerIsntHere))]
        public static class FarmHouse_updateEvenIfFarmerIsntHere_Patch
        {
            public static void Prefix(FarmHouse __instance, GameTime time, bool ignoreWasUpdatedFlush)
            {
                if (!Config.ModEnabled)
                    return;
                if (__instance.HasOwner && Game1.IsMasterGame)
                {
                    foreach (NPC spouse in __instance.characters)
                    {
                        if(spouse.modData.TryGetValue(stayKey, out var str))
                        {
                            if(Game1.timeOfDay >= int.Parse(str))
                            {
                                AccessTools.Method(typeof(NPC), "routeEndAnimationFinished").Invoke(spouse, new object[] { null });
                                spouse.modData.Remove(stayKey);
                            }
                        }
                        else if (Game1.random.NextDouble() < Config.ChancePerTick && spouse.controller == null && spouse.Schedule == null)
                        {
                             var kvp = GetRandomSpot(spouse.Name, __instance);
                            if (kvp.Value is SpotData d && d.Tile != spouse.TilePoint)
                            {
                                spouse.modData[spotKey] = kvp.Key;
                                spouse.controller = new PathFindController(spouse, __instance, d.Tile, d.Facing, new PathFindController.endBehavior(DoBehaviour));
                            }
                        }
                    }
                }
            }

        }
        [HarmonyPatch(typeof(NPC), nameof(NPC.marriageDuties))]
        public static class NPC_marriageDuties_Patch
        {
            public static void Postfix(NPC __instance)
            {
                if (!Config.ModEnabled || SHelper.ModRegistry.IsLoaded("aedenthorn.FreeLove") || __instance.currentLocation is not FarmHouse fh)
                    return;


                if (Game1.random.NextDouble() < Config.ChanceAtNewDay)
                {
                    SetRandomSpot(__instance, fh, true, true);
                }
            }
        }
    }
}