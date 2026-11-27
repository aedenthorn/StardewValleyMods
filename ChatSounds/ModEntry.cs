using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Menus;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using xTile;
using xTile.Layers;
using xTile.ObjectModel;
using Object = StardewValley.Object;

namespace ChatSounds
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry : Mod
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;

        public static ModEntry context;
        public static Dictionary<string, CueDefinition> cues;
        private static string end;

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

            Helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();


            SoundBank soundBank;
            var fi = AccessTools.Field(Game1.soundBank.GetType(), "soundBank");
            if (fi is null)
            {
                fi = AccessTools.Field(Game1.soundBank.GetType(), "sdvSoundBankWrapper");
                if (fi is null)
                    return;
                var w = fi.GetValue(Game1.soundBank);
                fi = AccessTools.Field(w.GetType(), "soundBank");
                soundBank = (SoundBank)fi.GetValue(w);
            }
            else
            {
                soundBank = (SoundBank)fi.GetValue(Game1.soundBank);
            }
            cues = AccessTools.FieldRefAccess<SoundBank, Dictionary<string, CueDefinition>>(soundBank, "_cues");
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
                name: () => "Proximity Based Vol?",
                getValue: () => Config.ProximityBased,
                setValue: value => Config.ProximityBased = value
            );
        }
    }
}
