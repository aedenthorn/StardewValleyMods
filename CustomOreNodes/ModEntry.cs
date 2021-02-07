using Harmony;
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
    public class ModEntry : Mod
    {

        public static ModEntry context;

        internal static ModConfig Config;
        private static List<CustomOreNode> CustomOreNodes = new List<CustomOreNode>();
        private static IMonitor SMonitor;
        

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            context = this;
            Config = Helper.ReadConfig<ModConfig>();
            SMonitor = Monitor;
            var harmony = HarmonyInstance.Create(ModManifest.UniqueID);

            harmony.Patch(
               original: AccessTools.Method(typeof(MineShaft), "chooseStoneType"),
               postfix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.chooseStoneType_Postfix))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(MineShaft), "breakStone"),
               postfix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.breakStone_Postfix))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(Object), nameof(Object.draw), new Type[] { typeof(SpriteBatch), typeof(int), typeof(int), typeof(float) }),
               prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.Object_draw_Prefix))
            );

            if (Config.AllowCustomOreNodesAboveGround)
            {
                ConstructorInfo ci = typeof(Object).GetConstructor(new Type[] { typeof(Vector2), typeof(int), typeof(string), typeof(bool), typeof(bool), typeof(bool), typeof(bool) });
                harmony.Patch(
                   original: ci,
                   prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.Object_Prefix)),
                   postfix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.Object_Postfix))
                );
            }


            helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
        }

        private static bool Object_draw_Prefix(Object __instance, SpriteBatch spriteBatch, int x, int y, float alpha = 1f)
        {
            CustomOreNode node = CustomOreNodes.Find(n => n.parentSheetIndex == __instance.parentSheetIndex);
            if (node == null)
                return true;

            if (__instance.fragility != 2)
            {
                spriteBatch.Draw(Game1.shadowTexture, Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64 + 32, y * 64 + 51 + 4)), new Rectangle?(Game1.shadowTexture.Bounds), Color.White * alpha, 0f, new Vector2(Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y), 4f, SpriteEffects.None, __instance.getBoundingBox(new Vector2(x, y)).Bottom / 15000f);
            }
            Vector2 position3 = Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64 + 32 + ((__instance.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), y * 64 + 32 + ((__instance.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0)));
            Rectangle? sourceRectangle2 = new Rectangle(node.spriteX, node.spriteY, node.spriteW, node.spriteH);
            Color color2 = Color.White * alpha;
            float rotation2 = 0f;
            Vector2 origin2 = new Vector2(8f, 8f);
            Vector2 vector2 = __instance.scale;
            spriteBatch.Draw(node.texture, position3, sourceRectangle2, color2, rotation2, origin2, (__instance.scale.Y > 1f) ? __instance.getScale().Y : 4f, __instance.flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (__instance.isPassable() ? __instance.getBoundingBox(new Vector2(x, y)).Top : __instance.getBoundingBox(new Vector2(x, y)).Bottom) / 10000f);
            return false;
        }

        private void GameLoop_GameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
        {
            CustomOreNodes.Clear();
            CustomOreData data;
            CustomOreConfig conf = new CustomOreConfig();
            int id = 42424000;
            Dictionary<int, int> existingPSIs = new Dictionary<int, int>();
            conf = Helper.Data.ReadJsonFile<CustomOreConfig>("ore_config.json") ?? new CustomOreConfig();
            foreach (KeyValuePair<int, int> psi in conf.parentSheetIndexes)
            {
                existingPSIs.Add(psi.Value, psi.Key);
            }
            foreach (IContentPack contentPack in Helper.ContentPacks.GetOwned())
            {
                conf = contentPack.ReadJsonFile<CustomOreConfig>("ore_config.json") ?? new CustomOreConfig();
                foreach (KeyValuePair<int, int> psi in conf.parentSheetIndexes)
                {
                    existingPSIs.Add(psi.Value, psi.Key);
                }

            }
            try
            {
                if(File.Exists(Path.Combine(Helper.DirectoryPath, "custom_ore_nodes.json")))
                {
                    int add = 0;
                    try
                    {
                        data = Helper.Content.Load<CustomOreData>("custom_ore_nodes.json", ContentSource.ModFolder);

                    }
                    catch
                    {
                        var tempData = Helper.Content.Load<CustomOreDataOld>("custom_ore_nodes.json", ContentSource.ModFolder);
                        data = new CustomOreData();
                        for (int i = 0; i < tempData.nodes.Count; i++)
                        {
                            data.nodes.Add(new CustomOreNode(tempData.nodes[i]));
                        }
                        Monitor.Log($"Rewriting custom_ore_nodes.json", LogLevel.Debug);
                        Helper.Data.WriteJsonFile("custom_ore_nodes.json", data);
                    }
                    conf = Helper.Data.ReadJsonFile<CustomOreConfig>("ore_config.json") ?? new CustomOreConfig();
                    foreach (object nodeObj in data.nodes)
                    {
                        CustomOreNode node = (CustomOreNode) nodeObj;

                        if (node.spriteType == "mod")
                        {
                            node.texture = Helper.Content.Load<Texture2D>(node.spritePath, ContentSource.ModFolder);
                        }
                        else
                        {
                            node.texture = Helper.Content.Load<Texture2D>(node.spritePath, ContentSource.GameContent);
                        }
                        if (conf.parentSheetIndexes.ContainsKey(add))
                        {
                            node.parentSheetIndex = conf.parentSheetIndexes[add];
                        }
                        else
                        {
                            while (existingPSIs.ContainsKey(id))
                                id++;
                            node.parentSheetIndex = id++;
                        }
                        conf.parentSheetIndexes[add] = node.parentSheetIndex;

                        CustomOreNodes.Add(node);
                        add++;
                    }
                    Monitor.Log($"Got {CustomOreNodes.Count} ores from mod", LogLevel.Debug);
                    Helper.Data.WriteJsonFile("ore_config.json", conf);

                }
                else
                {
                    SMonitor.Log("No custom_ore_nodes.json in mod directory.");
                }
            }
            catch(Exception ex)
            {
                SMonitor.Log("Error processing custom_ore_nodes.json: "+ex, LogLevel.Error);
            }

            foreach (IContentPack contentPack in Helper.ContentPacks.GetOwned()) 
            {
                try
                {
                    int add = 0;
                    conf = contentPack.ReadJsonFile<CustomOreConfig>("ore_config.json") ?? new CustomOreConfig();
                    Monitor.Log($"Reading content pack: {contentPack.Manifest.Name} {contentPack.Manifest.Version} from {contentPack.DirectoryPath}");

                    try
                    {
                        data = contentPack.ReadJsonFile<CustomOreData>("custom_ore_nodes.json");
                    }
                    catch(Exception ex)
                    {
                        Monitor.Log($"exception {ex}", LogLevel.Error);
                        var tempData = contentPack.ReadJsonFile<CustomOreDataOld>("custom_ore_nodes.json");
                        data = new CustomOreData();
                        for (int i = 0; i < tempData.nodes.Count; i++)
                        {
                            data.nodes.Add(new CustomOreNode(tempData.nodes[i]));
                        }
                        Monitor.Log($"Rewriting custom_ore_nodes.json", LogLevel.Debug);
                        contentPack.WriteJsonFile("custom_ore_nodes.json", data);
                    }

                    foreach (CustomOreNode node in data.nodes)
                    {
                        if (node.spriteType == "mod")
                        {
                            node.texture = contentPack.LoadAsset<Texture2D>(node.spritePath);

                        }
                        else
                        {
                            node.texture = Helper.Content.Load<Texture2D>(node.spritePath, ContentSource.GameContent);

                        }
                        if (conf.parentSheetIndexes.ContainsKey(add))
                        {
                            node.parentSheetIndex = conf.parentSheetIndexes[add];
                        }
                        else
                        {
                            while (existingPSIs.ContainsKey(id))
                                id++;
                            node.parentSheetIndex = id++;
                        }
                        conf.parentSheetIndexes[add] = node.parentSheetIndex;
                        CustomOreNodes.Add(node);
                        add++;
                    }
                    contentPack.WriteJsonFile("ore_config.json", conf);
                    Monitor.Log($"Got {data.nodes.Count} ores from content pack {contentPack.Manifest.Name}", LogLevel.Debug);
                }
                catch(Exception ex)
                {
                    SMonitor.Log($"Error processing custom_ore_nodes.json in content pack {contentPack.Manifest.Name} {ex}", LogLevel.Error);
                }
            }
            Monitor.Log($"Got {CustomOreNodes.Count} ores total", LogLevel.Debug);
        }

        private static void chooseStoneType_Postfix(MineShaft __instance, ref Object __result, Vector2 tile)
        {
            if (__result == null || __result.parentSheetIndex == null)
                return;

            int difficulty = __instance.mineLevel > 120 ? Game1.netWorldState.Value.SkullCavesDifficulty : Game1.netWorldState.Value.MinesDifficulty;

            List<int> ores = new List<int>() { 765, 764, 290, 751 };
            if (!ores.Contains(__result.ParentSheetIndex))
            {
                float totalChance = 0;
                for (int i = 0; i < CustomOreNodes.Count; i++)
                {
                    CustomOreNode node = CustomOreNodes[i];
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
                    for (int i = 0; i < CustomOreNodes.Count; i++)
                    {
                        CustomOreNode node = CustomOreNodes[i];
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
                for (int i = 0; i < CustomOreNodes.Count; i++)
                {
                    CustomOreNode node = CustomOreNodes[i];
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
                for (int i = 0; i < CustomOreNodes.Count; i++)
                {
                    if(parentSheetIndex == CustomOreNodes[i].parentSheetIndex)
                    {
                        __instance.MinutesUntilReady = CustomOreNodes[i].durability;
                        break;
                    }
                }
            }
        }


        private static void breakStone_Postfix(GameLocation __instance, ref bool __result, int indexOfStone, int x, int y, Farmer who, Random r)
        {
            SMonitor.Log($"Checking for custom ore in stone {indexOfStone}");

            CustomOreNode node = CustomOreNodes.Find(n => n.parentSheetIndex == indexOfStone);

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

                    Game1.createMultipleObjectDebris(itemId, x, y, addedOres + (int)Math.Round(r.Next(item.minAmount, (Math.Max(item.minAmount + 1, item.maxAmount + 1)) + ((r.NextDouble() < who.LuckLevel / 100f) ? item.luckyAmount : 0) + ((r.NextDouble() < who.MiningLevel / 100f) ? item.minerAmount : 0)) * gotRange.dropMult), who.uniqueMultiplayerID, __instance);
                }
            }
            int experience = (int)Math.Round(node.exp * gotRange.expMult);
            who.gainExperience(3, experience);
            __result = experience > 0;
        }

        private static bool IsInRange(OreLevelRange range, GameLocation location, bool mineOnly)
        {

            int difficulty = (location is MineShaft) ? ((location as MineShaft).mineLevel > 120 ? Game1.netWorldState.Value.SkullCavesDifficulty : Game1.netWorldState.Value.MinesDifficulty) : 0;

            return (range.minLevel < 1 && !(location is MineShaft) && !mineOnly) || (location is MineShaft && (range.minLevel <= (location as MineShaft).mineLevel && (range.maxLevel < 0 || (location as MineShaft).mineLevel <= range.maxLevel))) && (range.minDifficulty <= difficulty) && (range.maxDifficulty < 0 || range.maxDifficulty >= difficulty);
        }
    }
}
 