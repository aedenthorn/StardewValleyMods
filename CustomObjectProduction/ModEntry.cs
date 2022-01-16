using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using System;
using System.Collections.Generic;
using Object = StardewValley.Object;

namespace CustomObjectProduction
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry : Mod, IAssetLoader
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;

        public static ModEntry context;

        public static readonly string dictPath = "custom_object_production_dictionary";
        public static Dictionary<string, ProductData> objectProductionDataDict = new Dictionary<string, ProductData>();
        private static IDynamicGameAssetsApi apiDGA;
        private static IJsonAssetsApi apiJA;

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            Config = Helper.ReadConfig<ModConfig>();

            if (!Config.EnableMod)
                return;

            context = this;

            SMonitor = Monitor;
            SHelper = helper;

            helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            helper.Events.GameLoop.SaveLoaded += GameLoop_SaveLoaded;

            var harmony = new Harmony(ModManifest.UniqueID);

            harmony.Patch(
               original: AccessTools.Method(typeof(Object), nameof(Object.DayUpdate)),
               postfix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.Object_DayUpdate_Postfix))
            );
            harmony.Patch(
               original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.checkAction)),
               prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.GameLocation_checkAction_Prefix))
            );
            harmony.Patch(
               original: AccessTools.Method(typeof(Game1), "_newDayAfterFade"),
               prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.Game1__newDayAfterFade_Prefix))
            );
            
        }

        private void GameLoop_SaveLoaded(object sender, StardewModdingAPI.Events.SaveLoadedEventArgs e)
        {
            objectProductionDataDict = SHelper.Content.Load<Dictionary<string, ProductData>>(dictPath, ContentSource.GameContent) ?? new Dictionary<string, ProductData>();
            SMonitor.Log($"Loaded {objectProductionDataDict.Count} products for today");
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
        }

        private static Object GetObjectFromID(string id, int amount, int quality)
        {
            SMonitor.Log($"Trying to get object {id}, DGA {apiDGA != null}, JA {apiJA != null}");

            Object obj = null;
            try
            {

                if (int.TryParse(id, out int index))
                {
                    SMonitor.Log($"Spawning object with index {id}");
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
                        SMonitor.Log($"Spawning DGA object {id}");
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
                        SMonitor.Log($"Spawning JA object {id}");
                        return new Object(idx, amount, false, -1, quality);

                    }
                }
            }
            catch(Exception ex)
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
            Monitor.Log("Loading dictionary");

            return (T)(object)new Dictionary<string, ProductData>();
        }
    }
}