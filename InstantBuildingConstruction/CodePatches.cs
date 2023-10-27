using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Locations;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Object = StardewValley.Object;

namespace InstantBuildingConstruction
{
    public partial class ModEntry
    {
        [HarmonyPatch(typeof(BluePrint), new Type[] { typeof(string) })]
        [HarmonyPatch(MethodType.Constructor)]
        public class Blueprint_Patch
        {
            public static void Postfix(BluePrint __instance)
            {
                if (!Config.ModEnabled)
                    return;
                __instance.daysToConstruct = 0;
            }
        }
        [HarmonyPatch(typeof(Building), new Type[] { typeof(BluePrint), typeof(Vector2) })]
        [HarmonyPatch(MethodType.Constructor)]
        public class Building_Patch
        {
            public static void Postfix(Building __instance)
            {
                if (!Config.ModEnabled)
                    return;
                __instance.daysOfConstructionLeft.Value = 0;
            }
        }
        [HarmonyPatch(typeof(Building), nameof(Building.performActionOnConstruction))]
        public class Building_performActionOnConstruction_Patch
        {
            public static void Postfix(Building __instance)
            {
                if (!Config.ModEnabled)
                    return;

                Game1.player.checkForQuestComplete(null, -1, -1, null, __instance.buildingType.Value, 8, -1);
                if (__instance.buildingType.Equals("Slime Hutch") && __instance.indoors.Value != null)
                {
                    __instance.indoors.Value.objects[new Vector2(1f, 4f)] = new Object(new Vector2(1f, 4f), 156, false)
                    {
                        Fragility = 2
                    };
                    if (!Game1.player.mailReceived.Contains("slimeHutchBuilt"))
                    {
                        Game1.player.mailReceived.Add("slimeHutchBuilt");
                    }
                }
            }
        }
        [HarmonyPatch(typeof(CarpenterMenu), nameof(CarpenterMenu.tryToBuild))]
        public class CarpenterMenu_tryToBuild_Patch
        {
            public static void Postfix()
            {
                if (!Config.ModEnabled)
                    return;
                foreach(var b in Game1.getFarm().buildings)
                {
                    if(b.daysOfConstructionLeft.Value > 0)
                    {
                        b.daysOfConstructionLeft.Value = 0;
                    }
                }
            }
        }

        [HarmonyPatch(typeof(CarpenterMenu), nameof(CarpenterMenu.receiveLeftClick))]
        public class CarpenterMenu_receiveLeftClick_Patch
        {
            public static void Postfix()
            {
                if (!Config.ModEnabled)
                    return;
                var buildings = Game1.getFarm().buildings;
                for (int i = 0; i < buildings.Count; i++)
                {
                    if (buildings[i].daysUntilUpgrade.Value > 0)
                    {
                        string upgrade = buildings[i].getNameOfNextUpgrade();
                        SMonitor.Log($"Upgrading {buildings[i].buildingType.Value} to {upgrade}");
                        buildings[i].daysUntilUpgrade.Value = 0;
                        Game1.player.checkForQuestComplete(null, -1, -1, null, upgrade, 8, -1);
                        BluePrint CurrentBlueprint = new BluePrint(upgrade);
                        buildings[i].buildingType.Value = CurrentBlueprint.name;
                        buildings[i].tilesHigh.Value = CurrentBlueprint.tilesHeight;
                        buildings[i].tilesWide.Value = CurrentBlueprint.tilesWidth;
                        if (buildings[i].indoors.Value is not null)
                        {
                            buildings[i].indoors.Value.mapPath.Value = "Maps\\" + CurrentBlueprint.mapToWarpTo;
                            buildings[i].indoors.Value.name.Value = CurrentBlueprint.mapToWarpTo;
                            if (buildings[i].indoors.Value is AnimalHouse)
                            {
                                ((AnimalHouse)buildings[i].indoors.Value).resetPositionsOfAllAnimals();
                                ((AnimalHouse)buildings[i].indoors.Value).animalLimit.Value += 4;
                                ((AnimalHouse)buildings[i].indoors.Value).loadLights();
                            }

                        }
                        buildings[i].upgrade();
                        buildings[i].resetTexture();
                    }
                }
            }
        }

    }
}