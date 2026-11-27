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
            SMonitor.Log($"Trying to get object {id}");

            Object obj = null;
            try
            {

                if (Game1.objectData.TryGetValue(id, out var data))
                {
                    SMonitor.Log($"Spawning object with index {id}");
                    return new Object(id, amount, false, -1, quality);
                }
                else
                {
                    var dict = SHelper.GameContent.Load<Dictionary<int, string>>("Data/ObjectInformation");
                    foreach (var kvp in dict)
                    {
                        if (kvp.Value.StartsWith(id + "/"))
                            return new Object(kvp.Key, amount, false, -1, quality);
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