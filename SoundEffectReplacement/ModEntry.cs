using HarmonyLib;
using Microsoft.Xna.Framework.Audio;
using StardewModdingAPI;
using StardewValley;
using System;
using System.Collections.Generic;
using System.IO;

namespace SoundEffectReplacement
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry : Mod, IAssetLoader
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;

        public static ModEntry context;

        public static readonly string dictPath = "sound_effect_replacement_dictionary";
        public static Dictionary<string, string> replacementDict = new Dictionary<string, string>();
        public static Dictionary<string, SoundEffect> soundEffectDict = new Dictionary<string, SoundEffect>();
        public static SoundEffect silentSound;
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            Config = Helper.ReadConfig<ModConfig>();

            context = this;

            SMonitor = Monitor;
            SHelper = helper;

            helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            helper.Events.GameLoop.SaveLoaded += GameLoop_SaveLoaded;
            helper.Events.GameLoop.DayStarted += GameLoop_DayStarted;
            helper.Events.GameLoop.TimeChanged += GameLoop_TimeChanged;

            var harmony = new Harmony(ModManifest.UniqueID);

            harmony.Patch(
               original: AccessTools.Method(typeof(Game1), nameof(Game1.playSoundPitched)),
               prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.playSound_Prefix))
            );
            harmony.Patch(
               original: AccessTools.Method(typeof(Game1), nameof(Game1.playSound)),
               prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.playSound_Prefix))
            );
            harmony.Patch(
               original: AccessTools.Method(typeof(SoundBank), nameof(SoundBank.GetCue), new Type[] { typeof(string) }),
               prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.SoundBank_GetCuePrefix))
            );
            harmony.Patch(
               original: AccessTools.Method(typeof(SoundBank), nameof(SoundBank.PlayCue), new Type[] { typeof(string) }),
               prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.SoundBank_PlayCuePrefix))
            );
            harmony.Patch(
               original: AccessTools.Method(typeof(SoundBank), nameof(SoundBank.PlayCue), new Type[] { typeof(string), typeof(AudioListener), typeof(AudioEmitter) }),
               prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.SoundBank_PlayCuePrefix))
            );
        }

        private void GameLoop_TimeChanged(object sender, StardewModdingAPI.Events.TimeChangedEventArgs e)
        {
            ReloadDict();
        }

        private void GameLoop_DayStarted(object sender, StardewModdingAPI.Events.DayStartedEventArgs e)
        {
            ReloadDict();
        }

        private void GameLoop_SaveLoaded(object sender, StardewModdingAPI.Events.SaveLoadedEventArgs e)
        {
            ReloadDict();
        }

        private void ReloadDict()
        {
            if (!Config.EnableMod)
                return;
 
            replacementDict = SHelper.GameContent.Load<Dictionary<string, string>>(dictPath);
            //SMonitor.Log($"Loaded {replacementDict.Count} replacements");
            foreach(string replacementList in replacementDict.Values)
            {
                var parts = replacementList.Split('|');
                foreach(string replacement in parts)
                {
                    if (replacement.Contains("."))
                    {
                        if (soundEffectDict.ContainsKey(replacement))
                            continue;
                        string path = Path.Combine(Helper.DirectoryPath, replacement);
                        if (File.Exists(path))
                        {
                            FileStream fs = new FileStream(path, FileMode.Open);
                            soundEffectDict[replacement] = SoundEffect.FromStream(fs);
                            fs.Dispose();
                        }
                    }
                }
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
            string path = Path.Combine(Helper.DirectoryPath, "assets", "silent.wav");
            FileStream fs = new FileStream(path, FileMode.Open);
            silentSound = SoundEffect.FromStream(fs);
            fs.Dispose();
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

            return (T)(object)new Dictionary<string, string>();
        }
    }
}