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
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                SMonitor.Log("Patching Crop.harvest");
                var codes = new List<CodeInstruction>(instructions);
                for (int i = 0; i < codes.Count; i++)
                {
                    if (codes[i].opcode == OpCodes.Ldloc_0 && codes[i + 1].opcode == OpCodes.Brfalse)
                    {
                        SMonitor.Log("Adding to successful harvest");
                        codes.Insert(i + 2, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ModEntry.HarvestSeeds))));
                        codes.Insert(i + 2, new CodeInstruction(OpCodes.Ldarg_2));
                        codes.Insert(i + 2, new CodeInstruction(OpCodes.Ldarg_1));
                        codes.Insert(i + 2, new CodeInstruction(OpCodes.Ldarg_0));
                        break;
                    }
                }
                return codes.AsEnumerable();
            }
        }
        public static void HarvestSeeds(Crop crop, int xTile, int yTile)
        {
            if (!Config.EnableMod || crop.dead.Value || crop.netSeedIndex.Value == crop.indexOfHarvest.Value || (!Config.RegrowableSeeds && crop.regrowAfterHarvest.Value > -1) || Game1.random.NextDouble() > Config.SeedChance / 100f)
                return;
            int index;
            if (crop.forageCrop.Value)
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
                index = crop.netSeedIndex.Value;
                if (index == -1)
                {
                    Dictionary<int, string> cropData = Game1.content.Load<Dictionary<int, string>>("Data\\Crops");
                    foreach (int key in cropData.Keys)
                    {
                        if (Convert.ToInt32(cropData[key].Split('/', StringSplitOptions.None)[3]) == crop.indexOfHarvest.Value)
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