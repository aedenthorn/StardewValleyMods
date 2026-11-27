using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Buildings;
using System.Collections.Generic;

namespace FarmAnimalHarvestHelper
{
    public partial class ModEntry
    {
        public static Dictionary<string, Dictionary<long, Vector2>> slottedDict = new Dictionary<string, Dictionary<long, Vector2>>();

        [HarmonyPatch(typeof(AnimalHouse), nameof(AnimalHouse.DayUpdate))]
        public class AnimalHouse_DayUpdate_Patch
        {
            public static void Postfix(AnimalHouse __instance)
            {
                slottedDict.Remove(__instance.uniqueName.Value);
            }
        }
        
        [HarmonyPatch(typeof(AnimalHouse), nameof(AnimalHouse.updateWhenNotCurrentLocation))]
        public class AnimalHouse_updateWhenNotCurrentLocation_Patch
        {
            public static void Postfix(AnimalHouse __instance, Building parentBuilding)
            {
                if (!Config.ModEnabled || parentBuilding is not Barn || slottedDict.ContainsKey(__instance.uniqueName.Value) || __instance.animals.Count() == 0)
                    return;
                int slot = 0;
                slottedDict.Add(__instance.uniqueName.Value, new Dictionary<long, Vector2>());
                for (int i = __instance.animals.Count() - 1; i >= 0; i--)
                {
                    var animal = __instance.animals.Pairs.ElementAt(i).Value;
                    if (animal.currentProduce.Value == -1 || animal.age.Value < animal.ageWhenMature.Value || animal.type.Contains("Pig") || !animal.buildingTypeILiveIn.Contains("Barn"))
                        continue;
                    var pos = Config.FirstSlotTile;
                    pos += new Vector2(slot, 0);
                    slot++;
                    slottedDict[__instance.uniqueName.Value][animal.myID.Value] = pos;
                }
                SMonitor.Log($"Slotted {slot} animails in {__instance.uniqueName.Value}");
            }
        }

        [HarmonyPatch(typeof(FarmAnimal), nameof(FarmAnimal.updateWhenCurrentLocation))]
        public class FarmAnimal_updateWhenCurrentLocation_Patch
        {
            public static void Postfix(FarmAnimal __instance, GameTime time, GameLocation location)
            {
                if (!Config.ModEnabled || location is not AnimalHouse || __instance.currentProduce.Value == -1 || __instance.age.Value < __instance.ageWhenMature.Value || __instance.type.Contains("Pig") || (Config.MaxWaitHour >= 0 && Game1.timeOfDay > Config.MaxWaitHour) || Game1.timeOfDay < 600 || !slottedDict.TryGetValue(location.uniqueName.Value, out Dictionary<long, Vector2> dict) || !dict.TryGetValue(__instance.myID.Value, out Vector2 pos))
                    return;
                pos -= new Vector2(0.5f, 0);
                __instance.Position = pos * 64;
                __instance.faceDirection(0);
            }
        }
        
        [HarmonyPatch(typeof(FarmAnimal), nameof(FarmAnimal.updateWhenNotCurrentLocation))]
        public class FarmAnimal_updateWhenNotCurrentLocation_Patch
        {
            public static bool Prefix(FarmAnimal __instance, GameTime time, GameLocation environment)
            {
                if (!Config.ModEnabled || environment is not AnimalHouse || __instance.currentProduce.Value == -1 || __instance.age.Value < __instance.ageWhenMature.Value || __instance.type.Contains("Pig") || (Config.MaxWaitHour >= 0 && Game1.timeOfDay > Config.MaxWaitHour) || Game1.timeOfDay < 600 || !slottedDict.TryGetValue(environment.uniqueName.Value, out Dictionary<long, Vector2> dict) || !dict.TryGetValue(__instance.myID.Value, out Vector2 pos))
                    return true;
                pos -= new Vector2(0.5f, 0);
                __instance.Position = pos * 64;
                __instance.faceDirection(0);

                return false;
            }
        }

   }
}