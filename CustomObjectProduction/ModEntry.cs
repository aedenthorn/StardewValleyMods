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
    public partial class ModEntry : Mod
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;

        public static ModEntry context;

        public static readonly string dictPath = "custom_object_production_dictionary";
        private static IDynamicGameAssetsApi apiDGA;
        private static IJsonAssetsApi apiJA;

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            Config = Helper.ReadConfig<ModConfig>();

            context = this;

            SMonitor = Monitor;
            SHelper = helper;

            helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            helper.Events.Content.AssetRequested += Content_AssetRequested;

            var harmony = new Harmony(ModManifest.UniqueID);

            harmony.Patch(
               original: AccessTools.Method(typeof(Object), nameof(Object.DayUpdate)),
               postfix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.Object_DayUpdate_Postfix))
            );
            harmony.Patch(
               original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.checkAction)),
               prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.GameLocation_checkAction_Prefix))
            );
        }

        private void Content_AssetRequested(object sender, StardewModdingAPI.Events.AssetRequestedEventArgs e)
        {
            if (!Config.EnableMod)
                return;
            if (e.NameWithoutLocale.IsEquivalentTo(dictPath))
            {
                e.LoadFrom(() => new Dictionary<string, ProductData>(), StardewModdingAPI.Events.AssetLoadPriority.Exclusive);
            }
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

    }
}