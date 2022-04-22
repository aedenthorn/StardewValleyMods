using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Object = StardewValley.Object;

namespace HarvestSeeds
{
    public partial class ModEntry
    {
        [HarmonyPatch(typeof(Crop), nameof(Crop.harvest))]
        public class Crop_harvest_Patch
        {
            public static void Postfix(Crop __instance, int xTile, int yTile, bool __result)
            {
                if (!Config.EnableMod || __instance.dead.Value || !__result || __instance.netSeedIndex.Value == __instance.indexOfHarvest.Value || (!Config.RegrowableSeeds && __instance.regrowAfterHarvest.Value > -1) || Game1.random.NextDouble() > Config.SeedChance / 100f)
                    return;
                int index;
                if (__instance.forageCrop.Value)
                {
                    switch (Game1.currentSeason)
                    {
                        case "summer":
                            index = 496;
                            break;
                        case "fall":
                            index = 497;
                            break;
                        case "winter":
                            index = 498;
                            break;
                        default:
                            index = 495;
                            break;
                    }
                }
                else
                {
                    index = __instance.netSeedIndex.Value;
                    if (index == -1)
                    {
                        Dictionary<int, string> cropData = Game1.content.Load<Dictionary<int, string>>("Data\\Crops");
                        foreach (int key in cropData.Keys)
                        {
                            if (Convert.ToInt32(cropData[key].Split('/', StringSplitOptions.None)[3]) == __instance.indexOfHarvest.Value)
                            {
                                index = key;
                            }
                        }
                    }
                }
                if (index == -1)
                    return;
                int amount = Game1.random.Next(Config.MinSeeds, Config.MaxSeeds + 1);
                if (amount <= 0)
                    return;
                SMonitor.Log($"Dropping {amount} seeds of {index}");
                Game1.createItemDebris(new Object(index, amount), new Vector2((float)(xTile * 64 + 32), (float)(yTile * 64 + 32)), -1, null, -1);
            }
        }
   }
}