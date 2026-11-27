using HarmonyLib;
using Microsoft.Xna.Framework;
using Netcode;
using StardewValley;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;
using Object = StardewValley.Object;

namespace LogSpamFilter
{
    public partial class ModEntry
    {
        public static void StumpDropWoodFake(GameLocation location, int debrisType, int x, int y, int numberOfChunks, bool resource, int groundLevel, bool item, Color color, Tree tree)
        {
            if (!Config.EnableMod || !DropDict.ContainsKey(tree.treeType.Value.ToString()))
            {
                Game1.createRadialDebris(location, debrisType, x, y, numberOfChunks, resource, groundLevel, item, color);
            }
            else
            {
                Game1.createRadialDebris(location, debrisType, x, y, numberOfChunks, resource, groundLevel, item, color);
            }
        }
        public static void StumpDrop2ExtraNF(string index, int x, int y, int number, GameLocation location, Tree tree)
        {
            if (!Config.EnableMod || !DropDict.ContainsKey(tree.treeType.Value.ToString()))
            {
                Game1.createMultipleObjectDebris(index, x, y, number, location);
            }
            else
            {
                DropItems(tree, DropDict[tree.treeType.Value.ToString()].stumpSap, x, y, location);
            }
        }
        public static void StumpDropExtraNF1(Object item, Vector2 origin, int direction, GameLocation location, int groundLevel, bool flopFish, Tree tree)
        {
            if (!Config.EnableMod || !DropDict.ContainsKey(tree.treeType.Value.ToString()))
            {
                Game1.createItemDebris(item, origin, direction, location, groundLevel, flopFish);
            }
            else
            {
                DropItems(tree, DropDict[tree.treeType.Value.ToString()].stumpSap, (int)(origin.X / 64f), (int)(origin.Y / 64f), location);
                DropItems(tree, DropDict[tree.treeType.Value.ToString()].stumpSap, (int)(origin.X / 64f), (int)(origin.Y / 64f), location);
            }

        }
        public static void StumpDropExtraNF2(Tree tree, Object item, Vector2 origin, int direction, GameLocation location = null, int groundLevel = -1)
        {
            if (!Config.EnableMod || !DropDict.ContainsKey(tree.treeType.Value.ToString()))
            {
                Game1.createItemDebris(item, origin, direction, location, groundLevel);
            }
            else
            {
                DropItems(tree, DropDict[tree.treeType.Value.ToString()].stumpSap, (int)(origin.X / 64f), (int)(origin.Y / 64f), location);
            }

        }
        public static void StumpDropExtraMP(Tree tree, string index, int x, int y, int number, long who, GameLocation location)
        {
            if (!Config.EnableMod || !DropDict.ContainsKey(tree.treeType.Value.ToString()))
            {
                Game1.createMultipleObjectDebris(index, x, y, number, who, location);
            }
            else
            {
                DropItems(tree, DropDict[tree.treeType.Value.ToString()].stumpSap, x, y, location);
                DropItems(tree, DropDict[tree.treeType.Value.ToString()].stumpHardwood, x, y, location);
            }

        }
        public static void StumpDropWood(Tree tree, GameLocation location, int debrisType, int x, int y, int numberOfChunks, bool resource, int groundLevel, bool item, Color color)
        {
            if (!Config.EnableMod || !DropDict.ContainsKey(tree.treeType.Value.ToString()))
            {
                Game1.createRadialDebris(location, debrisType, x, y, numberOfChunks, resource, groundLevel, item, color);
            }
            else
            {
                DropItems(tree, DropDict[tree.treeType.Value.ToString()].stumpWood, x, y, location);
            }
        }

        public static void StumpDropExtra(Tree tree, string index, int x, int y, int number, GameLocation location)
        {
            if (!Config.EnableMod || !DropDict.ContainsKey(tree.treeType.Value.ToString()))
            {
                Game1.createMultipleObjectDebris(index, x, y, number, location);
            }
            else
            {
                DropItems(tree, DropDict[tree.treeType.Value.ToString()].stumpSap, x, y, location);
                DropItems(tree, DropDict[tree.treeType.Value.ToString()].stumpHardwood, x, y, location);
            }
        }

        public static void DropWood(Tree tree, GameLocation location, int debrisType, int x, int y, int numberOfChunks, bool resource, int groundLevel, bool item, Color color)
        {
            if (!Config.EnableMod || !DropDict.ContainsKey(tree.treeType.Value.ToString()))
            {
                Game1.createRadialDebris(location, debrisType, x, y, numberOfChunks, resource, groundLevel, item, color);
            }
            else
            {
                float woodMult = Game1.GetPlayer(AccessTools.FieldRefAccess<Tree, NetLong>(tree, "lastPlayerToHit").Value).professions.Contains(12) ? 1.25f : 1.0f + (int)AccessTools.Method(typeof(Tree), "extraWoodCalculator").Invoke(tree, new object[] { new Vector2(x, y) }) / 12f;
                DropItems(tree, DropDict[tree.treeType.Value.ToString()].wood, x, y, location, woodMult);
                DropItems(tree, DropDict[tree.treeType.Value.ToString()].sap, x, y, location);
                DropItems(tree, DropDict[tree.treeType.Value.ToString()].items, x, y, location);
            }
        }
        public static void DropWoodFake(Tree tree, GameLocation location, int debrisType, int x, int y, int numberOfChunks, bool resource, int groundLevel, bool item, Color color)
        {
            if (!Config.EnableMod || !DropDict.ContainsKey(tree.treeType.Value.ToString()))
            {
                Game1.createRadialDebris(location, debrisType, x, y, numberOfChunks, resource, groundLevel, item, color);
            }
            else
            {
                Game1.createRadialDebris(location, debrisType, x, y, numberOfChunks, resource, groundLevel, item, color);
            }
        }

        public static void DropSapMP(Tree tree, string index, int x, int y, int number, long who, GameLocation location)
        {
            if (!Config.EnableMod || !DropDict.ContainsKey(tree.treeType.Value.ToString()))
            {
                Game1.createMultipleObjectDebris(index, x, y, number, who, location);
            }
            else
            {
            }

        }
        public static void DropHardwoodMP(Tree tree, string index, int x, int y, int number, long who, GameLocation location)
        {
            if (!Config.EnableMod || !DropDict.ContainsKey(tree.treeType.Value.ToString()))
            {
                Game1.createMultipleObjectDebris(index, x, y, number, who, location);
            }
            else
            {
                DropHardwood(tree, index, x, y, number, location);
            }

        }
        public static void DropSeedMP1(Tree tree, string index, int x, int y, int number, long who, GameLocation location)
        {
            if (!Config.EnableMod || !DropDict.ContainsKey(tree.treeType.Value.ToString()))
            {
                Game1.createMultipleObjectDebris(index, x, y, number, who, location);
            }
            else
            {

            }

        }
        public static void DropSeedMP2(Tree tree, string index, int x, int y, int number, long who, GameLocation location)
        {
            if (!Config.EnableMod || !DropDict.ContainsKey(tree.treeType.Value.ToString()))
            {
                Game1.createMultipleObjectDebris(index, x, y, number, who, location);
            }
            else
            {

            }

        }

        public static void DropSeed1(Tree tree, string index, int x, int y, int number, GameLocation location)
        {
            if (!DropDict.ContainsKey(tree.treeType.Value.ToString()))
            {
                Game1.createMultipleObjectDebris(index, x, y, number, location);
            }
            else
            {

            }

        }
        public static void DropSeed2(Tree tree, string index, int x, int y, int number, GameLocation location)
        {
            if (!Config.EnableMod || !DropDict.ContainsKey(tree.treeType.Value.ToString()))
            {
                Game1.createMultipleObjectDebris(index, x, y, number, location);
            }
            else
            {

            }

        }
        public static void DropMushroomWood(Tree tree, string index, int x, int y, int number, GameLocation location)
        {
            if (!Config.EnableMod || !DropDict.ContainsKey(tree.treeType.Value.ToString()))
            {
                Game1.createMultipleObjectDebris(index, x, y, number, location);
            }
            else
            {

            }

        }
        public static void DropHardwood(Tree tree, string index, int x, int y, int number, GameLocation location)
        {
            if (!Config.EnableMod || !DropDict.ContainsKey(tree.treeType.Value.ToString()))
            {
                Game1.createMultipleObjectDebris(index, x, y, number, location);
            }
            else
            {
                Random r;
                if (Game1.IsMultiplayer)
                {
                    Game1.recentMultiplayerRandom = new Random(x * 1000 + y);
                    r = Game1.recentMultiplayerRandom;
                }
                else
                {
                    r = new Random((int)Game1.uniqueIDForThisGame + (int)Game1.stats.DaysPlayed + x * 7 + y * 11);
                }
                foreach (var item in DropDict[tree.treeType.Value.ToString()].hardwood)
                {
                    int amount = GetDropAmount(item);
                    if (Game1.GetPlayer(AccessTools.FieldRefAccess<Tree, NetLong>(tree, "lastPlayerToHit").Value).professions.Contains(14))
                    {
                        if(item.min == 0 && item.max == 0)
                        {
                            while (r.NextDouble() < 0.5)
                            {
                                amount++;
                            }
                        }
                        else
                            amount += (int)(amount * 0.25f + 0.9f);
                    }

                    amount = (int)Math.Round(amount * item.mult);
                    if (tree.growthStage.Value > 5)
                    {
                        amount += (int)Math.Round(amount * Math.Min(Config.MaxDaysSizeIncrease, tree.growthStage.Value - 5) * Config.LootIncreasePerDay);
                    }

                    //SMonitor.Log($"Dropping {amount}x {item.id}");
                    if (amount <= 0)
                        return;
                    for (int i = 0; i < amount; i++)
                    {
                        Object obj = ItemRegistry.Create(item.id, 1, GetQuality(item)) as Object;
                        if (obj == null)
                        {
                            SMonitor.Log($"error getting object from id {item.id}", StardewModdingAPI.LogLevel.Warn);
                            break;
                        }
                        Game1.createItemDebris(obj, new Vector2(x, y) * 64, Game1.random.Next(4), location, -1);
                    }
                }
            }

        }
        public static void DropSap(Tree tree, string index, int x, int y, int number, GameLocation location)
        {
            if (!Config.EnableMod || !DropDict.ContainsKey(tree.treeType.Value.ToString()))
            {
                Game1.createMultipleObjectDebris(index, x, y, number, location);
            }
            else
            {

            }

        }

        private static int GetDropAmount(ItemData item)
        {
            return Game1.random.Next((int)item.min, (int)item.max + 1);
        }
        private static int GetQuality(ItemData item)
        {
            return Game1.random.Next(item.minQuality, item.maxQuality + 1);
        }
        private static void DropItems(Tree tree, List<ItemData> itemlist, int x, int y, GameLocation location, float mult = 1)
        {
            foreach (var item in itemlist)
            {

                int amount = (int)Math.Round(GetDropAmount(item) * mult * item.mult);
                if(tree.growthStage.Value > 5)
                {
                    amount += (int)Math.Round(amount * Math.Min(Config.MaxDaysSizeIncrease, tree.growthStage.Value - 5) * Config.LootIncreasePerDay);
                }
                if (amount <= 0)
                    return;
                SMonitor.Log($"Dropping {amount}x {item.id}");

                for (int i = 0; i < amount; i++)
                {
                    Object obj = ItemRegistry.Create(item.id, 1, GetQuality(item)) as Object;
                    if (obj == null)
                    {
                        SMonitor.Log($"error getting object from id {item.id}", StardewModdingAPI.LogLevel.Warn);
                        break;
                    }
                    Game1.createItemDebris(obj, new Vector2(x, y) * 64, Game1.random.Next(4), location, -1);
                }
            }
        }

    }
}
