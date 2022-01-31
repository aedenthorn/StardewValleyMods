using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewModdingAPI;
using StardewValley;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;
using System.Globalization;
using Object = StardewValley.Object;

namespace LogSpamFilter
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry : Mod, IAssetLoader
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;

        public static ModEntry context;
        private static IDynamicGameAssetsApi apiDGA;
        private static IJsonAssetsApi apiJA;
        private Harmony harmony;

        public static readonly string dictPath = "tree_tweaks_dictionary";
        private static Dictionary<string, DropData> dropDict = new Dictionary<string, DropData>();

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            Config = Helper.ReadConfig<ModConfig>();

            context = this;

            SMonitor = Monitor;
            SHelper = helper;
            helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;


            harmony = new Harmony(ModManifest.UniqueID);

            harmony.Patch(
               original: AccessTools.Method(typeof(Tree), "performTreeFall"),
               prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.Tree_performTreeFall_Prefix)),
               transpiler: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.Tree_performTreeFall_Transpiler))
            );
            harmony.Patch(
               original: AccessTools.Method(typeof(Tree), "tickUpdate"),
               transpiler: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.Tree_tickUpdate_Transpiler))
            );
            harmony.Patch(
               original: AccessTools.Method(typeof(Tree), "draw", new Type[] { typeof(SpriteBatch), typeof(Vector2) }),
               transpiler: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.Tree_draw_Transpiler))
            );
            
            harmony.Patch(
               original: AccessTools.Method(typeof(Tree), "getBoundingBox"),
               postfix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.Tree_getBoundingBox_Postfix))
            );

        }

        private void GameLoop_GameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
        {
            apiDGA = Helper.ModRegistry.GetApi<IDynamicGameAssetsApi>("spacechase0.DynamicGameAssets");
            apiJA = Helper.ModRegistry.GetApi<IJsonAssetsApi>("spacechase0.JsonAssets");
            

            // get Generic Mod Config Menu's API (if it's installed)
            var configMenu = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (configMenu is null)
                return;

            // register mod
            configMenu.Register(
                mod: ModManifest,
                reset: () => Config = new ModConfig(),
                save: () => Helper.WriteConfig(Config)
            );

            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Mod Enabled?",
                getValue: () => Config.EnableMod,
                setValue: value => Config.EnableMod = value
            );
            configMenu.AddTextOption(
                mod: ModManifest,
                name: () => "Oversize % / Day",
                getValue: () => "" + Config.SizeIncreasePerDay,
                setValue: delegate (string value) { try { Config.SizeIncreasePerDay = float.Parse(value, CultureInfo.InvariantCulture); } catch { } }
            );
            configMenu.AddTextOption(
                mod: ModManifest,
                name: () => "Loot % / Oversize",
                getValue: () => "" + Config.LootIncreasePerDay,
                setValue: delegate (string value) { try { Config.LootIncreasePerDay = float.Parse(value, CultureInfo.InvariantCulture); } catch { } }
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Max Oversize Days",
                getValue: () => Config.MaxDaysSizeIncrease,
                setValue: value => Config.MaxDaysSizeIncrease = value
            );
        }

        private static float GetTreeGrowth(Tree tree)
        {
            if (!Config.EnableMod || tree.growthStage.Value <= 5)
                return 0;
            return Math.Min(Config.MaxDaysSizeIncrease, tree.growthStage.Value - 5) * Config.SizeIncreasePerDay / 100f;
        }

        private static Vector2 GetTreePartSize(Tree tree, int count)
        {
            switch (count)
            {
                case 1: // shadow
                    return new Vector2(41, 30);
                case 2: // tree
                    return Vector2.Zero;
                case 3: // stump
                    return new Vector2(16, 64);
                case 4: // broken stump
                    return new Vector2(16, 16);
                default:
                    return Vector2.Zero;
            }
        }

        private static void DrawTree(SpriteBatch spriteBatch, Texture2D texture, Vector2 position, Rectangle? sourceRectangle, Color color, float rotation, Vector2 origin, float scale, SpriteEffects effects, float layerDepth, int count, Tree tree, Vector2 tileLocation)
        {
            if (!Config.EnableMod || tree.growthStage.Value <= 5 || Config.MaxDaysSizeIncrease <= 0 || count == 0)
            {
                spriteBatch.Draw(texture, position, sourceRectangle, color, rotation, origin, scale, effects, layerDepth);
                return;
            }
            var newScale = scale * (1 + GetTreeGrowth(tree));
            if(count != 5)
            {
                var size = GetTreePartSize(tree, count);
                position -= size * (newScale - scale) / 2;
            }
            layerDepth = GetLayerDepth(tree, layerDepth, count, tileLocation);
            spriteBatch.Draw(texture, position, sourceRectangle, color, rotation, origin, newScale, effects, layerDepth);
        }

        private static void DrawTree2(SpriteBatch spriteBatch, Texture2D texture, Vector2 position, Rectangle? sourceRectangle, Color color, float rotation, Vector2 origin, float scale, SpriteEffects effects, float layerDepth, int count, Tree tree, Vector2 tileLocation)
        {
            Rectangle? newSourceRectangle = sourceRectangle;
            if (texture.Width % 48 == 0)
            {
                float sourceScale = texture.Width / 48f;
                Vector2 sourceStart = new Vector2(sourceRectangle.Value.X, sourceRectangle.Value.Y) * sourceScale;
                Vector2 sourceSize = new Vector2(sourceRectangle.Value.Width, sourceRectangle.Value.Height) * sourceScale;
                newSourceRectangle = new Rectangle?(new Rectangle(Utility.Vector2ToPoint(sourceStart), Utility.Vector2ToPoint(sourceSize)));
            }

            if (!Config.EnableMod || tree.growthStage.Value <= 5 || count == 0)
            {
                spriteBatch.Draw(texture, new Rectangle(Utility.Vector2ToPoint(position), Utility.Vector2ToPoint(new Vector2(sourceRectangle.Value.Width, sourceRectangle.Value.Height) * scale)), newSourceRectangle, color, rotation, origin, SpriteEffects.None, layerDepth);
                return;
            }

            var newScale = scale * (1 + GetTreeGrowth(tree));
            if (count != 5)
            {
                var size = GetTreePartSize(tree, count);
                position -= size * (newScale - scale) / 2;
            }
            layerDepth = GetLayerDepth(tree, layerDepth, count, tileLocation);
            spriteBatch.Draw(texture, new Rectangle(Utility.Vector2ToPoint(position), Utility.Vector2ToPoint(new Vector2(sourceRectangle.Value.Width, sourceRectangle.Value.Height) * newScale)), newSourceRectangle, color, rotation, origin, SpriteEffects.None, layerDepth);
        }

        private static float GetLayerDepth(Tree tree, float layerDepth, int count, Vector2 tileLocation)
        {
            float growth = GetTreeGrowth(tree);
            switch (count)
            {
                case 2: // tree
                    return (tree.getBoundingBox(tileLocation).Bottom + 2 * (1 + growth)) / 10000f - tileLocation.X / 1000000f;
                case 4: // broken stump
                    return (tree.getBoundingBox(tileLocation).Bottom + (1 + growth)) / 10000f;
                case 5: // leaves
                    return tree.getBoundingBox(tileLocation).Bottom / 10000f + 0.01f * (1 + growth);
                default:
                    return layerDepth;
            }
        }

        private static Object GetObjectFromID(string id, int amount, int quality)
        {
            //SMonitor.Log($"Trying to get object {id}, DGA {apiDGA != null}, JA {apiJA != null}");

            Object obj = null;
            try
            {

                if (int.TryParse(id, out int index))
                {
                    //SMonitor.Log($"Spawning object with index {id}");
                    return new Object(index, amount, false, -1, quality);
                }
                else
                {
                    var dict = SHelper.Content.Load<Dictionary<int, string>>("Data/ObjectInformation", ContentSource.GameContent);
                    foreach (var kvp in dict)
                    {
                        if (kvp.Value.StartsWith(id + "/"))
                            return new Object(kvp.Key, amount, false, -1, quality);
                    }
                }
                if (apiDGA != null && id.Contains("/"))
                {
                    object o = apiDGA.SpawnDGAItem(id);
                    if (o is Object)
                    {
                        //SMonitor.Log($"Spawning DGA object {id}");
                        (o as Object).Stack = amount;
                        (o as Object).Quality = quality;
                        return (o as Object);
                    }
                }
                if (apiJA != null)
                {
                    int idx = apiJA.GetObjectId(id);
                    if (idx != -1)
                    {
                        //SMonitor.Log($"Spawning JA object {id}");
                        return new Object(idx, amount, false, -1, quality);

                    }
                }
            }
            catch (Exception ex)
            {
                SMonitor.Log($"Exception: {ex}", LogLevel.Error);
            }
            SMonitor.Log($"Couldn't find item with id {id}");
            return obj;
        }

        /// <summary>Get whether this instance can load the initial version of the given asset.</summary>
        /// <param name="asset">Basic metadata about the asset being loaded.</param>
        public bool CanLoad<T>(IAssetInfo asset)
        {
            if (!Config.EnableMod)
                return false;

            return asset.AssetNameEquals(dictPath);
        }

        /// <summary>Load a matched asset.</summary>
        /// <param name="asset">Basic metadata about the asset being loaded.</param>
        public T Load<T>(IAssetInfo asset)
        {
            var dict = Helper.Content.Load<Dictionary<string, DropData>>("assets/default.json") ?? new Dictionary<string, DropData>();
            Monitor.Log($"Loaded dictionary with {dict.Count} entries");

            return (T)(object) dict;
        }
    }
}