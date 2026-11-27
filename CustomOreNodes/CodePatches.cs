using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using System;
using System.Collections.Generic;
using Object = StardewValley.Object;

namespace CustomOreNodes
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry : Mod
    {

        private static void createLitterObject_Postfix(MineShaft __instance, ref Object __result, Vector2 tile)
        {
            if (__result == null)
                return;

            int difficulty = __instance.mineLevel > 120 ? Game1.netWorldState.Value.SkullCavesDifficulty : Game1.netWorldState.Value.MinesDifficulty;

            List<int> ores = new List<int>() { 765, 764, 290, 751 };
            if (!ores.Contains(__result.ParentSheetIndex))
            {
                float totalChance = 0;
                for (int i = 0; i < customOreNodesList.Count; i++)
                {
                    ICustomOreNode node = customOreNodesList[i];
                    foreach(OreLevelRange range in node.oreLevelRanges)
                    {
                        if (IsInRange(range, __instance, true)) 
                        { 
                            totalChance += node.spawnChance * range.spawnChanceMult;
                            break;
                        }
                    }
                }
                double ourChance = Game1.random.NextDouble() * 100;
                if (ourChance < totalChance)
                {
                    // SMonitor.Log($"Chance of custom ore: {ourChance}%");
                    float cumulativeChance = 0f;
                    for (int i = 0; i < customOreNodesList.Count; i++)
                    {
                        ICustomOreNode node = customOreNodesList[i];
                        OreLevelRange gotRange = null;
                        foreach (OreLevelRange range in node.oreLevelRanges)
                        {
                            if (IsInRange(range, __instance, true))
                            {
                                gotRange = range;
                                break;
                            }
                        }
                        if (gotRange == null)
                        {
                            continue;
                        }
                        cumulativeChance += node.spawnChance * gotRange.spawnChanceMult;
                        if (ourChance < cumulativeChance)
                        {
                            SMonitor.Log($"Switching to custom ore \"{node.itemId}\": {cumulativeChance} / {ourChance} (rolled)");

                            //SMonitor.Log($"Displaying stone at index {index}", LogLevel.Debug);
                            __result = new Object(node.itemId, 1, false, -1, 0)
                            {
                                MinutesUntilReady = node.durability
                            };

                            return;
                        }
                    }
                }
            }
        }

        private static void Object_Prefix(ref string itemId)
        {
            if (!Config.AllowCustomOreNodesAboveGround || Environment.StackTrace.Contains("createLitterObject") || !new List<string>() { "32", "38", "40", "42", "668", "670" }.Contains(itemId))
            {
                return;
            }

            float currentChance = 0;
            for (int i = 0; i < customOreNodesList.Count; i++)
            {
                ICustomOreNode node = customOreNodesList[i];
                OreLevelRange gotRange = null;
                foreach (OreLevelRange range in node.oreLevelRanges)
                {
                    if (range.minLevel < 1)
                    {
                        gotRange = range;
                        break;
                    }
                }
                if (gotRange == null)
                {
                    continue;
                }
                currentChance += node.spawnChance * gotRange.spawnChanceMult;
                if (Game1.random.NextDouble() < currentChance / 100f)
                {
                    itemId = node.itemId;
                    break;
                }
            }
        }
        
        private static void Object_Postfix(Object __instance, ref string itemId)
        {
            if (!Config.AllowCustomOreNodesAboveGround || Environment.StackTrace.Contains("createLitterObject") || !new List<string>() { "32", "38", "40", "42", "668", "670" }.Contains(itemId))
            {
                return;
            }
            for (int i = 0; i < customOreNodesList.Count; i++)
            {
                if (itemId == customOreNodesList[i].itemId)
                {
                    __instance.MinutesUntilReady = customOreNodesList[i].durability;
                    break;
                }
            }
        }


        private static void breakStone_Postfix(GameLocation __instance, ref bool __result, string stoneId, int x, int y, Farmer who, Random r)
        {
            SMonitor.Log($"Checking for custom ore in stone {stoneId}");

            ICustomOreNode node = customOreNodesList.Find(n => n.itemId == stoneId);

            if (node == null)
                return;

            SMonitor.Log($"Got custom ore in stone {stoneId}");


            OreLevelRange gotRange = null;
            foreach (OreLevelRange range in node.oreLevelRanges)
            {
                if (IsInRange(range, __instance, false))
                {
                    gotRange = range;
                    break;
                }
            }
            if (gotRange == null)
            {
                SMonitor.Log($"No range for {stoneId}!", LogLevel.Warn);

                return;
            }

            int addedOres = who.professions.Contains(18) ? 1 : 0;
            SMonitor.Log($"custom node has {node.dropItems.Count} potential items.");
            foreach (DropItem item in node.dropItems)
            {
                if (Game1.random.NextDouble() < item.dropChance * gotRange.dropChanceMult/100) 
                {
                    SMonitor.Log($"dropping item {item.itemIdOrName}");

                    string itemId = null;
                    foreach (var kvp in Game1.objectData)
                    {
                        if (kvp.Key == item.itemIdOrName || kvp.Value.Name == item.itemIdOrName)
                        {
                            itemId = kvp.Key;
                            break;
                        }
                    }
                    if (itemId == null)
                    {
                        SMonitor.Log($"couldn't find item: {item.itemIdOrName}");
                        continue;
                    }
                    Game1.createMultipleObjectDebris(itemId, x, y, addedOres + (int)Math.Round(r.Next(item.minAmount, (Math.Max(item.minAmount + 1, item.maxAmount + 1)) + ((r.NextDouble() < who.LuckLevel / 100f) ? item.luckyAmount : 0) + ((r.NextDouble() < who.MiningLevel / 100f) ? item.minerAmount : 0)) * gotRange.dropMult), who.UniqueMultiplayerID, __instance);
                }
            }
            int experience = (int)Math.Round(node.exp * gotRange.expMult);
            who.gainExperience(3, experience);
            __result = experience > 0;
        }

    }
}
 