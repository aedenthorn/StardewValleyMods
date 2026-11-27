using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Network;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CustomResourceClumps
{
    /// <summary>The mod entry point.</summary>
    public class ModEntry : Mod
    {

        public static ModEntry context;

        internal static ModConfig Config;
        public static List<CustomResourceClump> customClumps = new List<CustomResourceClump>();
        public static IMonitor SMonitor;
        private static IModHelper SHelper;
        public bool finishedLoadingClumps = false;
        public static Dictionary<string, Type> ToolTypes { get; set; } = new Dictionary<string, Type>()
        {
            { "axe", typeof(Axe) },
            { "pick", typeof(Pickaxe) },
            { "hoe", typeof(Hoe) },
            { "weapon", typeof(MeleeWeapon) },
            { "wateringcan", typeof(WateringCan) },
            { "wand", typeof(Wand) },
        };
        public static Dictionary<string, int> expTypes = new Dictionary<string, int>()
        {
            { "farming", 0 },
            { "foraging", 2 },
            { "mining", 3 },
            { "combat", 4 },
        };

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            context = this;
            Config = Helper.ReadConfig<ModConfig>();
            SMonitor = Monitor;
            SHelper = Helper;
            var harmony = new Harmony(ModManifest.UniqueID);

            harmony.Patch(
               original: AccessTools.Method(typeof(MineShaft), "populateLevel"),
               postfix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.MineShaft_populateLevel_Postfix))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(ResourceClump), nameof(ResourceClump.performToolAction)),
               prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.ResourceClump_performToolAction_prefix))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(ResourceClump), nameof(ResourceClump.draw)),
               prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.ResourceClump_draw_prefix))
            );

            if (Config.AllowCustomResourceClumpsAboveGround)
            {

            }

            helper.ConsoleCommands.Add("crc", "Custom Resource Clump command.", CRCConsoleCommand);

            helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
        }
        public override object GetApi()
        {
            return new CustomResourceClumpsAPI();
        }
        private void CRCConsoleCommand(string arg1, string[] arg2)
        {
            if(arg2.Length == 0)
            {
                Monitor.Log("crc list", LogLevel.Info);
                Monitor.Log("crc spawn <id> <x> <y>", LogLevel.Info);
                return;
            }
            if (arg2[0] == "list")
            {
                foreach(var item in customClumps)
                {
                    Monitor.Log($"{item.id} ({item.index})", LogLevel.Info);
                }
            }
            if (arg2[0] == "spawn" && arg2.Length == 4 && int.TryParse(arg2[2], out int x) && int.TryParse(arg2[3], out int y))
            {
                CustomResourceClump data;
                if(int.TryParse(arg2[1], out int idx))
                {
                    data = customClumps.Find(c => c.index == idx);
                }
                else
                {
                    data = customClumps.Find(c => c.id == arg2[1]);
                }
                if (data is not null)
                {
                    var clump = new ResourceClump(data.index, 2, 2, new Vector2(x, y));
                    clump.health.Value = data.durability;
                    Game1.currentLocation.resourceClumps.Add(clump);
                }
            }
        }

        private void GameLoop_GameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
        {
            customClumps.Clear();
            CustomResourceClumpData data;
            Dictionary<string, Texture2D> gameTextures = new Dictionary<string, Texture2D>();
            try
            {
                if(File.Exists(Path.Combine(Helper.DirectoryPath, "custom_resource_clumps.json")))
                {
                    Monitor.Log($"Checking for clumps in mod", LogLevel.Debug);
                    Dictionary<string, Texture2D> modTextures = new Dictionary<string, Texture2D>();
                    data = Helper.ModContent.Load<CustomResourceClumpData>("custom_resource_clumps.json");
                    foreach (CustomResourceClump clump in data.clumps)
                    {
                        if (clump.spriteType == "mod")
                        {
                            if (!modTextures.ContainsKey(clump.spritePath))
                            {
                                modTextures.Add(clump.spritePath, Helper.ModContent.Load<Texture2D>(clump.spritePath));
                            }
                            clump.texture = modTextures[clump.spritePath];
                        }
                        else
                        {
                            if (!gameTextures.ContainsKey(clump.spritePath))
                            {
                                gameTextures.Add(clump.spritePath, Helper.ModContent.Load<Texture2D>(clump.spritePath));
                            }
                            clump.texture = gameTextures[clump.spritePath];
                        }
                        clump.index = -(customClumps.Count + 1);
                        customClumps.Add(clump);
                    }
                    Monitor.Log($"Got {customClumps.Count} clumps from mod", LogLevel.Debug);

                }
                else
                {
                    SMonitor.Log("No custom_resource_clumps.json in mod directory.");
                }
            }
            catch(Exception ex)
            {
                SMonitor.Log("Error processing custom_resource_clumps.json: "+ex, LogLevel.Error);
            }

            foreach (IContentPack contentPack in Helper.ContentPacks.GetOwned())
            {
                try
                {
                    Dictionary<string, Texture2D> modTextures = new Dictionary<string, Texture2D>();
                    Monitor.Log($"Reading content pack: {contentPack.Manifest.Name} {contentPack.Manifest.Version} from {contentPack.DirectoryPath}");
                    data = contentPack.ReadJsonFile<CustomResourceClumpData>("custom_resource_clumps.json");
                    foreach (CustomResourceClump clump in data.clumps)
                    {

                        if (clump.spriteType == "mod")
                        {
                            if (!modTextures.ContainsKey(clump.spritePath))
                            {
                                modTextures.Add(clump.spritePath, contentPack.ModContent.Load<Texture2D>(clump.spritePath));
                            }
                            clump.texture = modTextures[clump.spritePath];
                        }
                        else
                        {
                            if (!gameTextures.ContainsKey(clump.spritePath))
                            {
                                gameTextures.Add(clump.spritePath, Helper.GameContent.Load<Texture2D>(clump.spritePath));
                            }
                            clump.texture = gameTextures[clump.spritePath];
                        }
                        clump.index = -(customClumps.Count + 1);
                        customClumps.Add(clump);

                    }
                    Monitor.Log($"Got {data.clumps.Count} clumps from content pack {contentPack.Manifest.Name}", LogLevel.Debug);
                }
                catch(Exception ex)
                {
                    SMonitor.Log($"Error processing custom_resource_clumps.json in content pack {contentPack.Manifest.Name} {ex}", LogLevel.Error);
                }
            }
            finishedLoadingClumps = true;
            Monitor.Log($"Got {customClumps.Count} clumps total", LogLevel.Debug);
        }

        private static void MineShaft_populateLevel_Postfix(MineShaft __instance)
        {
            SMonitor.Log($"checking for custom clumps after populateLevel. total resource clumps: {__instance.resourceClumps.Count}");
            float totalChance = 0;

            for (int j = 0; j < customClumps.Count; j++)
            {
                CustomResourceClump clump = customClumps[j];
                if (clump.minLevel > -1 && __instance.mineLevel < clump.minLevel || clump.maxLevel > -1 && __instance.mineLevel > clump.maxLevel)
                {
                    continue;
                }
                totalChance += clump.baseSpawnChance + clump.additionalChancePerLevel * __instance.mineLevel;
            }
            if (totalChance > 100)
            {
                SMonitor.Log($"Total chance of a custom clump is greater than 100%", LogLevel.Warn);
            }
            for (int i = 0; i < __instance.resourceClumps.Count; i++)
            {
                double ourChance = Game1.random.NextDouble();
                if (ourChance < totalChance / 100f)
                {
                    float cumulativeChance = 0;
                    for (int j = 0; j < customClumps.Count; j++)
                    {
                        CustomResourceClump clump = customClumps[j];
                        if (clump.minLevel > -1 && __instance.mineLevel < clump.minLevel || clump.maxLevel > -1 && __instance.mineLevel > clump.maxLevel)
                        {
                            continue;
                        }
                        cumulativeChance += clump.baseSpawnChance + clump.additionalChancePerLevel * __instance.mineLevel;
                        if (ourChance < cumulativeChance / 100f)
                        {
                            SMonitor.Log($"Converting clump at {__instance.resourceClumps[i].currentTileLocation} to {clump.index} {clump.clumpDesc} ");
                            __instance.resourceClumps[i] = new ResourceClump(clump.index, clump.tileWidth, clump.tileHeight, __instance.resourceClumps[i].tile.Value);
                            __instance.resourceClumps[i].health.Value = clump.durability;
                            break;
                        }
                    }
                }
            }
        }
        private static bool ResourceClump_draw_prefix(ResourceClump __instance, SpriteBatch spriteBatch, float ___shakeTimer)
        {
            int indexOfClump = __instance.parentSheetIndex.Value;
            if (indexOfClump >= 0)
            {
                return true;
            }
            CustomResourceClump clump = customClumps.FirstOrDefault(c => c.index == indexOfClump);

            Rectangle sourceRect = new Rectangle(clump.spriteX, clump.spriteY, 32, 32);

            Vector2 position = __instance.tile.Value * 64f;
            if (___shakeTimer > 0f)
            {
                position.X += (float)Math.Sin(6.2831853071795862 / (double)___shakeTimer) * 4f;
            }
            spriteBatch.Draw(clump.texture, Game1.GlobalToLocal(Game1.viewport, position), new Rectangle?(sourceRect), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (__instance.tile.Y + 1f) * 64f / 10000f + __instance.tile.X / 100000f);
            return false;
        }

        private static bool ResourceClump_performToolAction_prefix(ref ResourceClump __instance, Tool t, Vector2 tileLocation, GameLocation location, ref bool __result)
        {
            int indexOfClump = __instance.parentSheetIndex.Value;
            if (indexOfClump >= 0)
            {
                return true;
            }
            CustomResourceClump clump = customClumps.FirstOrDefault(c => c.index == indexOfClump);
            if (clump == null)
                return true;

            if (t == null)
            {
                return false;
            }
            SMonitor.Log($"hitting custom clump {indexOfClump} with {t.GetType()} (should be {ToolTypes[clump.toolType]})");

            if (!CheckToolType(clump, t))
            {
                return false;
            } 
            SMonitor.Log($"tooltype is correct");
            if (t.UpgradeLevel < clump.toolMinLevel)
            {
                foreach (string sound in clump.failSounds)
                    location.playSound(sound, NetAudio.SoundContext.Default);
                
                Game1.drawObjectDialogue(string.Format(SHelper.Translation.Get("failed"), t.DisplayName));

                Game1.player.jitterStrength = 1f;
                return false;
            }

            float power = Math.Max(1f, (float)(t.UpgradeLevel + 1) * 0.75f);
            __instance.health.Value -= power;
            Game1.createRadialDebris(Game1.currentLocation, clump.debrisType, (int)tileLocation.X + Game1.random.Next(__instance.width.Value / 2 + 1), (int)tileLocation.Y + Game1.random.Next(__instance.height.Value / 2 + 1), Game1.random.Next(4, 9), false, -1, false, -1);

            if (__instance.health.Value > 0f)
            {
                foreach (string sound in clump.hitSounds)
                    location.playSound(sound, NetAudio.SoundContext.Default);
                if (clump.shake != 0)
                {
                    SHelper.Reflection.GetField<float>(__instance, "shakeTimer").SetValue(clump.shake);
                    __instance.NeedsUpdate = true;
                }
                return false;
            }

            __result = true;

            foreach (string sound in clump.breakSounds)
                location.playSound(sound, NetAudio.SoundContext.Default);

            Farmer who = t.getLastFarmerToUse();

            int addedItems = who.professions.Contains(18) ? 1 : 0;
            int experience = 0;
            SMonitor.Log($"custom clump has {clump.dropItems.Count} potential items.");
            foreach (DropItem item in clump.dropItems)
            {
                if (Game1.random.NextDouble() < item.dropChance/100) 
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
                    int amount = addedItems + Game1.random.Next(item.minAmount, Math.Max(item.minAmount + 1, item.maxAmount + 1)) + ((Game1.random.NextDouble() < (double)((float)who.LuckLevel / 100f)) ? item.luckyAmount : 0);
                    Game1.createMultipleObjectDebris(itemId, (int)tileLocation.X, (int)tileLocation.Y, amount, who.UniqueMultiplayerID, location);
                }
            }
            if (expTypes.ContainsKey(clump.expType))
            {
                experience = clump.exp;
                who.gainExperience(expTypes[clump.expType], experience);
            }
            else
            {
                SMonitor.Log($"Invalid experience type {clump.expType}", LogLevel.Warn);
            }
            return false;
        }

        private static bool CheckToolType(CustomResourceClump clump, Tool t)
        {
            if (clump.toolType.Contains(","))
            {
                string[] tools = clump.toolType.Split(',');
                foreach(string tool in tools)
                {
                    if (ToolTypes.ContainsKey(tool) && t.GetType() == ToolTypes[tool])
                        return true;
                }
                return false;
            }

            return ToolTypes.ContainsKey(clump.toolType) && t.GetType() == ToolTypes[clump.toolType];
        }
    }
}
 