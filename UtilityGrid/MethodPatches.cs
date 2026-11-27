using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Network;
using System.Collections.Generic;

namespace UtilityGrid
{
    public partial class ModEntry
    {
        // dga_add "aedenthorn.UtilityGridStorageDGA/Utility Grid Battery" 1
        // dga_add "aedenthorn.SimpleIrrigationSystemDGA/Iridium Water Pump" 1
        public static bool Utility_playerCanPlaceItemHere_Prefix(GameLocation location, Item item, int x, int y, ref bool __result)
        {
            //SMonitor.Log($"placing {item.Name}, {objectDict.ContainsKey(item.Name)}");
            if (!Config.EnableMod || !utilityObjectDict.ContainsKey(item.Name) || !(location is Farm) || !utilityObjectDict[item.Name].onlyInWater || location.Objects.ContainsKey(new Vector2(x, y)))
                return true;
            __result = location.isWaterTile(x / 64, y / 64);
            return false;
        }
        public static Object preItem = null;
        public static int preMinutesUntilReady = 0;
        public static bool Object_DayUpdate_Prefix(Object __instance, GameLocation location, ref int ___health, ref bool __state)
        {
            //SMonitor.Log($"placing {item.Name}, {objectDict.ContainsKey(item.Name)}");
            if (!Config.EnableMod || !utilityObjectDict.ContainsKey(__instance.Name))
                return true;

            if (!utilitySystemDict.ContainsKey(location.NameOrUniqueName))
            {
                RemakeAllGroups(location.NameOrUniqueName);
            }

            ___health = 10;
            if (__instance.IsSprinkler())
            {
                return !ObjectNeedsPower(utilityObjectDict[__instance.Name]) || IsObjectPowered(location.NameOrUniqueName, __instance.TileLocation, utilityObjectDict[__instance.Name]);
            }
            SMonitor.Log($"Day update for {__instance.Name}, needs power {ObjectNeedsPower(utilityObjectDict[__instance.Name])}, is powered {IsObjectPowered(location.NameOrUniqueName, __instance.TileLocation, utilityObjectDict[__instance.Name])}");
            if(!ObjectNeedsPower(utilityObjectDict[__instance.Name]) || IsObjectPowered(location.NameOrUniqueName, __instance.TileLocation, utilityObjectDict[__instance.Name]))
            {
                if (utilityObjectDict.ContainsKey(__instance.Name))
                {
                    if (__instance.heldObject.Value == null && __instance.MinutesUntilReady <= 0)
                        __instance.readyForHarvest.Value = true;
                }
                return true;
            }
            preItem = __instance.heldObject.Value;
            preMinutesUntilReady = __instance.MinutesUntilReady;
            __instance.MinutesUntilReady = 9999;
            __instance.heldObject.Value = null;
            __state = true;
            return true;
        }

        public static void Object_minutesElapsed_Prefix(Object __instance, GameLocation environment, ref bool __state)
        {
            if (!Config.EnableMod || !utilityObjectDict.ContainsKey(__instance.Name))
                return;
            if (!utilitySystemDict.ContainsKey(environment.Name))
            {
                RemakeAllGroups(environment.Name);
            }
            if (!ObjectNeedsPower(utilityObjectDict[__instance.Name]) || IsObjectPowered(environment.Name, __instance.TileLocation, utilityObjectDict[__instance.Name]))
            {
                if (utilityObjectDict.ContainsKey(__instance.Name))
                {
                    if (__instance.heldObject.Value == null && __instance.MinutesUntilReady <= 0)
                        __instance.readyForHarvest.Value = true;
                }
                return;
            }
            preItem = __instance.heldObject.Value;
            preMinutesUntilReady = __instance.MinutesUntilReady;
            __instance.MinutesUntilReady = 9999;
            __instance.heldObject.Value = null;
            __state = true;

        }
        public static void Object_Method_Postfix(Object __instance, bool __state)
        {
            //SMonitor.Log($"placing {item.Name}, {objectDict.ContainsKey(item.Name)}");
            if (!__state)
                return;
            __instance.MinutesUntilReady = preMinutesUntilReady;
            __instance.heldObject.Value = preItem;
        }
        public static bool Object_getScale_Prefix(Object __instance, ref Vector2 __result)
        {
            //SMonitor.Log($"placing {item.Name}, {objectDict.ContainsKey(item.Name)}");
            if (!Config.EnableMod || !utilityObjectDict.ContainsKey(__instance.Name))
                return true;
            if (!utilitySystemDict.ContainsKey(Game1.player.currentLocation.NameOrUniqueName))
            {
                RemakeAllGroups(Game1.player.currentLocation.NameOrUniqueName);
            }
            if (!ObjectNeedsPower(utilityObjectDict[__instance.Name]) || IsObjectPowered(Game1.player.currentLocation.NameOrUniqueName, __instance.TileLocation, utilityObjectDict[__instance.Name]) )
                return true;
            __result = Vector2.Zero;
            return false;
        }
        public static void Object_placementAction_Postfix(Object __instance, GameLocation location)
        {
            if (!Config.EnableMod || !utilityObjectDict.ContainsKey(__instance.Name))
                return;
            SMonitor.Log($"Placing object {__instance} in {location.NameOrUniqueName}");
            RemakeAllGroups(location.NameOrUniqueName);
        }
        public static void Object_performRemoveAction_Postfix(Object __instance, GameLocation environment)
        {
            if (!Config.EnableMod || !utilityObjectDict.ContainsKey(__instance.Name))
                return;
            SMonitor.Log($"Removing object {__instance} in {environment.NameOrUniqueName}");
            DelayedAction.functionAfterDelay(delegate
            {
                RemakeAllGroups(environment.Name);
            }, 50); 
        }


/*
				"Heater": {
					"electric": -1
				},
				"Auto-Grabber": {
					"electric": -1
				},
				"Hopper": {
					"electric": -2
				}

				
				"Prairie King Arcade System": {
					"electric": -2
				},
				"Mini-Jukebox": {
					"electric": -1
				},
				"Farm Computer": {
					"electric": -2
				},
				"Sewing Machine": {
					"electric": -1
				},

        public static bool GameLocation_numberOfObjectsWithName_Prefix(GameLocation __instance, string name, ref int __result)
        {
            if (!Config.EnableMod || name != "Heater" || !objectDict.ContainsKey(name))
                return true;
            int number = 0;
            using (OverlaidDictionary.PairsCollection.Enumerator enumerator = __instance.objects.Pairs.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    if (enumerator.Current.Value.Name.Equals(name) && IsObjectPowered(__instance.Name, enumerator.Current.Key, objectDict[name]))
                    {
                        number++;
                    }
                }
            }
            __result = number;
            return false;
        }
        public static List<Vector2> wasBigCraftableBeforeFAdayUpdate = new List<Vector2>();
        public static void FarmAnimal_dayUpdate_Prefix(FarmAnimal __instance, GameLocation environtment)
        {
            if (!Config.EnableMod)
                return;
            wasBigCraftableBeforeFAdayUpdate.Clear();
            using (OverlaidDictionary.PairsCollection.Enumerator enumerator = environtment.objects.Pairs.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    if (enumerator.Current.Value.bigCraftable.Value && enumerator.Current.Value.ParentSheetIndex.Equals(165) && objectDict.ContainsKey(enumerator.Current.Value.Name) && !IsObjectPowered(environtment.Name, enumerator.Current.Key, objectDict[enumerator.Current.Value.Name]))
                    {
                        enumerator.Current.Value.bigCraftable.Value = false;
                        wasBigCraftableBeforeFAdayUpdate.Add(enumerator.Current.Key);
                    }
                }
            }
        }
        public static void FarmAnimal_dayUpdate_Postfix(FarmAnimal __instance, GameLocation environtment)
        {
            if (!Config.EnableMod)
                return;
            foreach(Vector2 v in wasBigCraftableBeforeFAdayUpdate)
            {
                environtment.objects[v].bigCraftable.Value = true;
            }
            wasBigCraftableBeforeFAdayUpdate.Clear();
        }
*/
    }
}