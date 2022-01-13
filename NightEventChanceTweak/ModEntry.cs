using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace NightEventChanceTweak
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry : Mod
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;

        public static ModEntry context;

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


            var harmony = new Harmony(ModManifest.UniqueID);

            harmony.Patch(
               original: AccessTools.Method(typeof(Utility), nameof(Utility.pickFarmEvent)),
               postfix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.Utility_pickFarmEvent_Postfix))
            );

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
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Cumulative Chance?",
                tooltip: () => "Check all events at once",
                getValue: () => Config.CumulativeChance,
                setValue: value => Config.CumulativeChance = value
            );
            configMenu.AddTextOption(
                mod: ModManifest,
                name: () => "Crop Fairy Chance",
                getValue: () => ""+Config.FairyChance,
                setValue: delegate(string value) { try { Config.FairyChance = float.Parse(value, CultureInfo.InvariantCulture); } catch { } }
            );
            configMenu.AddTextOption(
                mod: ModManifest,
                name: () => "Witch Chance",
                getValue: () => ""+Config.WitchChance,
                setValue: delegate(string value) { try { Config.WitchChance = float.Parse(value, CultureInfo.InvariantCulture); } catch { } }
            );
            configMenu.AddTextOption(
                mod: ModManifest,
                name: () => "Meteor Chance",
                getValue: () => ""+Config.MeteorChance,
                setValue: delegate(string value) { try { Config.MeteorChance = float.Parse(value, CultureInfo.InvariantCulture); } catch { } }
            );
            configMenu.AddTextOption(
                mod: ModManifest,
                name: () => "Owl Event Chance",
                getValue: () => ""+Config.OwlChance,
                setValue: delegate(string value) { try { Config.OwlChance = float.Parse(value, CultureInfo.InvariantCulture); } catch { } }
            );
            configMenu.AddTextOption(
                mod: ModManifest,
                name: () => "Capsule Event Chance",
                getValue: () => ""+Config.CapsuleChance,
                setValue: delegate(string value) { try { Config.CapsuleChance = float.Parse(value, CultureInfo.InvariantCulture); } catch { } }
            );
        }
    }

}