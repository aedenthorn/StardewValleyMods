using HarmonyLib;
using Netcode;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace BuildingUpgrades
{
    public partial class ModEntry
    {
        [HarmonyPatch(typeof(CarpenterMenu), new Type[] { typeof(bool) })]
        [HarmonyPatch(MethodType.Constructor)]
        public class CarpenterMenu_Patch
        {
            public static void Postfix(CarpenterMenu __instance, List<BluePrint> ___blueprints)
            {
                if (!Config.ModEnabled)
                    return;

                Dictionary<string, string> dictionary = Game1.content.Load<Dictionary<string, string>>("Data\\Blueprints");
                foreach(var kvp in dictionary)
                {
                    var split = kvp.Value.Split('/');
                    if (split[0].Equals("animal") || !split[10].Equals("Upgrades"))
                        continue;
                    if(!___blueprints.Exists(b => b.name == kvp.Key) && Game1.getFarm().isBuildingConstructed(split[11]))
                    {
                        ___blueprints.Add(new BluePrint(kvp.Key));
                        SMonitor.Log($"Added blueprint for {kvp.Key}");
                    }
                }
            }
        }

        [HarmonyPatch(typeof(Building), nameof(Building.dayUpdate))]
        public class Building_dayUpdate_Patch
        {
            public static bool Prefix(Building __instance, int dayOfMonth)
            {
                if (!Config.ModEnabled || __instance.indoors.Value is not null || ((__instance.daysUntilUpgrade.Value <= 0 || Utility.isFestivalDay(dayOfMonth, Game1.currentSeason)) && !__instance.buildingType.Value.Contains("Deluxe")))
                    return true;
                if (__instance.daysUntilUpgrade.Value > 0 && !Utility.isFestivalDay(dayOfMonth, Game1.currentSeason))
                {
                    NetInt netInt2 = __instance.daysUntilUpgrade;
                    int value = netInt2.Value;
                    netInt2.Value = value - 1;
                    if (__instance.daysUntilUpgrade.Value <= 0)
                    {
                        Game1.player.checkForQuestComplete(null, -1, -1, null, __instance.getNameOfNextUpgrade(), 8, -1);
                        BluePrint CurrentBlueprint = new BluePrint(__instance.getNameOfNextUpgrade());
                        __instance.buildingType.Value = CurrentBlueprint.name;
                        __instance.tilesHigh.Value = CurrentBlueprint.tilesHeight;
                        __instance.tilesWide.Value = CurrentBlueprint.tilesWidth;
                        __instance.upgrade();
                        __instance.resetTexture();
                    }
                }
                return false;
            }
        }

        [HarmonyPatch(typeof(Utility), nameof(Utility.numSilos))]
        public class Utility_numSilos_Patch
        {
            public static void Postfix(ref int __result)
            {
                if (!Config.ModEnabled)
                    return;
                foreach (Building b in (Game1.getLocationFromName("Farm") as Farm).buildings)
                {
                    if (b.daysOfConstructionLeft.Value <= 0 && siloDict.TryGetValue(b.buildingType.Value, out string s) && int.TryParse(s, out int mult))
                    {
                        __result += mult;
                    }
                }
            }
        }
        [HarmonyPatch(typeof(Building), nameof(Building.getNameOfNextUpgrade))]
        public class Building_getNameOfNextUpgrade_Patch
        {
            public static bool Prefix(Building __instance, ref string __result)
            {
                if (!Config.ModEnabled || !__instance.modData.TryGetValue(upgradeKey, out string upgrade))
                    return true;
                __result = upgrade;
                __instance.modData.Remove(upgradeKey);
                SMonitor.Log($"Got upgrade {upgrade} for {__instance.buildingType.Value} ");
                return false;
            }
        }

        [HarmonyPatch(typeof(CarpenterMenu), nameof(CarpenterMenu.receiveLeftClick))]
        public class CarpenterMenu_receiveLeftClick_Patch
        {
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {

                var codes = new List<CodeInstruction>(instructions);
                for (int i = 0; i < codes.Count; i++)
                {
                    if (i > 3 && codes[i - 2].opcode == OpCodes.Ldc_I4_2 && codes[i - 3].opcode == OpCodes.Ldfld && (FieldInfo)codes[i - 3].operand == AccessTools.Field(typeof(Building), nameof(Building.daysUntilUpgrade)) && codes[i - 1].opcode == OpCodes.Callvirt)
                    {
                        SMonitor.Log($"Adding method to set next upgrade");
                        codes.Insert(i + 1, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ModEntry.SetNextUpgrade))));
                        codes.Insert(i + 1, new CodeInstruction(codes[i].Clone()));
                        codes.Insert(i + 1, new CodeInstruction(OpCodes.Ldarg_0));
                        break;
                    }
                }

                return codes.AsEnumerable();
            }
        }

        private static void SetNextUpgrade(CarpenterMenu menu, Building b)
        {
            if (!Config.ModEnabled)
                return;
            b.modData[upgradeKey] = menu.CurrentBlueprint.name;
            SMonitor.Log($"Set upgrade of {b.buildingType.Value} to {menu.CurrentBlueprint.name}");
        }
    }
}