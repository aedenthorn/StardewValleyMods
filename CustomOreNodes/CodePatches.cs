using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Object = StardewValley.Object;

namespace CustomOreNodes
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry : Mod
    {

        private static bool Object_draw_Prefix(Object __instance, SpriteBatch spriteBatch, int x, int y, float alpha = 1f)
        {
            CustomOreNode node = customOreNodesList.Find(n => n.parentSheetIndex == __instance.ParentSheetIndex);
            if (node == null)
                return true;

            if (__instance.Fragility != 2)
            {
                spriteBatch.Draw(Game1.shadowTexture, Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64 + 32, y * 64 + 51 + 4)), new Rectangle?(Game1.shadowTexture.Bounds), Color.White * alpha, 0f, new Vector2(Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y), 4f, SpriteEffects.None, __instance.getBoundingBox(new Vector2(x, y)).Bottom / 15000f);
            }
            Vector2 position3 = Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64 + 32 + ((__instance.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), y * 64 + 32 + ((__instance.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0)));
            Rectangle? sourceRectangle2 = new Rectangle(node.spriteX, node.spriteY, node.spriteW, node.spriteH);
            Color color2 = Color.White * alpha;
            float rotation2 = 0f;
            Vector2 origin2 = new Vector2(8f, 8f);
            Vector2 vector2 = __instance.scale;
            spriteBatch.Draw(node.texture, position3, sourceRectangle2, color2, rotation2, origin2, (__instance.scale.Y > 1f) ? __instance.getScale().Y : 4f, __instance.Flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (__instance.isPassable() ? __instance.getBoundingBox(new Vector2(x, y)).Top : __instance.getBoundingBox(new Vector2(x, y)).Bottom) / 10000f);
            return false;
        }

        private static void chooseStoneType_Postfix(MineShaft __instance, ref Object __result, Vector2 tile)
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
                    CustomOreNode node = customOreNodesList[i];
                    foreach(OreLevelRange range in node.oreLevelRanges)
                    {
                        if ((range.minLevel < 1 || __instance.mineLevel >= range.minLevel) && (range.maxLevel < 1 || __instance.mineLevel <= range.maxLevel) && (range.minDifficulty <= difficulty) && (range.maxDifficulty < 0 || range.maxDifficulty >= difficulty))
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
                        CustomOreNode node = customOreNodesList[i];
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
                            SMonitor.Log($"Switching to custom ore \"{node.nodeDesc}\": {cumulativeChance}% / {ourChance}% (rolled)");

                            int index = node.parentSheetIndex;
                            //SMonitor.Log($"Displaying stone at index {index}", LogLevel.Debug);
                            __result = new Object(tile, index, "Stone", true, false, false, false)
                            {
                                MinutesUntilReady = node.durability
                            };

                            return;
                        }
                    }
                }
            }
        }

        private static void Object_Prefix(ref int parentSheetIndex, string Givenname)
        {
            if (Environment.StackTrace.Contains("chooseStoneType"))
            {
                return;
            }
            if (Givenname == "Stone" || parentSheetIndex == 294 || parentSheetIndex == 295)
            {
                float currentChance = 0;
                for (int i = 0; i < customOreNodesList.Count; i++)
                {
                    CustomOreNode node = customOreNodesList[i];
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
                        int index = node.parentSheetIndex;
                        parentSheetIndex = index;
                        break;
                    }
                }
            }
        }
        
        private static void Object_Postfix(Object __instance, ref int parentSheetIndex, ref string Givenname)
        {
            if (Givenname == "Stone")
            {
                for (int i = 0; i < customOreNodesList.Count; i++)
                {
                    if(parentSheetIndex == customOreNodesList[i].parentSheetIndex)
                    {
                        __instance.MinutesUntilReady = customOreNodesList[i].durability;
                        break;
                    }
                }
            }
        }


        private static void breakStone_Postfix(GameLocation __instance, ref bool __result, int indexOfStone, int x, int y, Farmer who, Random r)
        {
            SMonitor.Log($"Checking for custom ore in stone {indexOfStone}");

            CustomOreNode node = customOreNodesList.Find(n => n.parentSheetIndex == indexOfStone);

            if (node == null)
                return;

            SMonitor.Log($"Got custom ore in stone {indexOfStone}");


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
                SMonitor.Log($"No range for {indexOfStone}!", LogLevel.Warn);

                return;
            }

            int addedOres = who.professions.Contains(18) ? 1 : 0;
            SMonitor.Log($"custom node has {node.dropItems.Count} potential items.");
            foreach (DropItem item in node.dropItems)
            {
                if (Game1.random.NextDouble() < item.dropChance * gotRange.dropChanceMult/100) 
                {
                    SMonitor.Log($"dropping item {item.itemIdOrName}");

                    if(!int.TryParse(item.itemIdOrName, out int itemId))
                    {
                        foreach(KeyValuePair<int,string> kvp in Game1.objectInformation)
                        {
                            if (kvp.Value.StartsWith(item.itemIdOrName + "/"))
                            {
                                itemId = kvp.Key;
                                break;
                            }
                        }
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
 