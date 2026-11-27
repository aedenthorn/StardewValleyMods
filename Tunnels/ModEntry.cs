using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.GameData.Fences;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace Tunnels
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry : Mod
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;
        public static ModEntry context;
        public const string dictPath = "aedenthorn.Tunnels/dict";
        public static Dictionary<string, TunnelData> Monsters
        {
            get
            {
                return SHelper.GameContent.Load<Dictionary<string, TunnelData>>(dictPath);
            }
        }

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            Config = Helper.ReadConfig<ModConfig>();
            SMonitor = Monitor;
            SHelper = helper;

            context = this;

            helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            helper.Events.Content.AssetRequested += Content_AssetRequested;

            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();
        }

        private void Content_AssetRequested(object sender, AssetRequestedEventArgs e)
        {
            if(!Config.ModEnabled) return;
            if(Config.RemoveEndDrawOffsets && e.NameWithoutLocale.IsEquivalentTo("Data/Fences"))
            {
                e.Edit((IAssetData data) =>
                {
                    var dict = data.AsDictionary<string, FenceData>().Data;
                    foreach(var kvp in dict) 
                    {
                        kvp.Value.LeftEndHeldObjectDrawX = 0;
                        kvp.Value.RightEndHeldObjectDrawX = 0;
                    }
                });
            }
        }

        private void GameLoop_GameLaunched(object sender, GameLaunchedEventArgs e)
        {   
            // get Generic Mod Config Menu's API (if it's installed)
            var configMenu = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (configMenu is not null)
            {
                // register mod
                configMenu.Register(
                    mod: ModManifest,
                    reset: () => Config = new ModConfig(),
                    save: () => Helper.WriteConfig(Config)
                );

                configMenu.AddBoolOption(
                    mod: ModManifest,
                    name: () => SHelper.Translation.Get("ModEnabled"),
                    getValue: () => Config.ModEnabled,
                    setValue: value => Config.ModEnabled = value
                );

                configMenu.AddBoolOption(
                    mod: ModManifest,
                    name: () => SHelper.Translation.Get("ReturnOnDestroy"),
                    getValue: () => Config.ReturnOnDestroy,
                    setValue: value => Config.ReturnOnDestroy = value
                );

                configMenu.AddBoolOption(
                    mod: ModManifest,
                    name: () => SHelper.Translation.Get("RemoveEndDrawOffsets"),
                    getValue: () => Config.RemoveEndDrawOffsets,
                    setValue: value => Config.RemoveEndDrawOffsets = value
                );
            }
        }
    }
}