using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using Netcode;
using Newtonsoft.Json;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using xTile;
using xTile.Dimensions;
using xTile.Tiles;

namespace SoundTweaker
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry : Mod
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;

        public static ModEntry context;
        
        public static string dictPath = "aedenthorn.SoundTweaker/dictionary";
        public static Dictionary<string, TweakData> tweakDict = new Dictionary<string, TweakData>();

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            Config = Helper.ReadConfig<ModConfig>();

            context = this;

            SMonitor = Monitor;
            SHelper = helper;

            Helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            Helper.Events.GameLoop.DayStarted += GameLoop_DayStarted;
            Helper.Events.Content.AssetRequested += Content_AssetRequested;
            Helper.Events.Input.ButtonPressed += Input_ButtonPressed;

            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();

        }

        private void GameLoop_DayStarted(object sender, StardewModdingAPI.Events.DayStartedEventArgs e)
        {
            ReloadSounds();
        }

        private static void ReloadSounds()
        {

            tweakDict = SHelper.GameContent.Load<Dictionary<string, TweakData>>(dictPath);
            /*
            tweakDict["snowyStep"] = new TweakData()
            {
                sounds = new List<SoundInfo> { new SoundInfo() { cuePath = "toyPiano", minVolume = 0.5f, maxVolume = 0.5f, minPitch = 1f, maxPitch = 1f, reverb = false, minFrequency = 0, maxFrequency = 5000, minQ = 0, maxQ = 2500, filter = FilterMode.HighPass } }
            };
            */
            foreach (var key in tweakDict.Keys.ToArray())
            {
                if (tweakDict[key].sounds != null)
                {
                    var existingCueDef = Game1.soundBank.GetCueDefinition(key);
                    var oldSounds = existingCueDef.sounds.ToArray();
                    existingCueDef.sounds.Clear();
                    existingCueDef.instanceLimit = tweakDict[key].maxInstances;
                    existingCueDef.limitBehavior = tweakDict[key].limitBehavior;
                    foreach (var s in tweakDict[key].sounds)
                    {
                        var outsounds = new List<XactSoundBankSound>();
                        if (s.filePath is not null)
                        {
                            SoundEffect audio;
                            string filePathCombined = Path.Combine(SHelper.DirectoryPath, s.filePath);
                            using (var stream = new FileStream(filePathCombined, FileMode.Open))
                            {
                                audio = SoundEffect.FromStream(stream);
                            }
                            outsounds.Add(new XactSoundBankSound(new SoundEffect[] { audio }, Game1.audioEngine.GetCategoryIndex("Sound"), s.loop, s.reverb));

                        }
                        else if (s.cuePath is not null)
                        {
                            var existingCueDef2 = Game1.soundBank.GetCueDefinition(s.cuePath);
                            if (existingCueDef2 != null)
                            {
                                var sounds = (s.cuePath == key) ? oldSounds :  existingCueDef2.sounds.ToArray();

                                for (int i = 0; i < sounds.Length; i++)
                                {
                                    outsounds.Add(sounds[i]);
                                }
                            }
                        }
                        for(int i = 0; i < outsounds.Count; i++)
                        {
                            var sound = outsounds[i];
                            if (sound.soundClips is null)
                            {
                                var clip = new XactClip(new List<PlayWaveVariant> { new PlayWaveVariant() { soundBank = sound.soundBank, waveBank = sound.waveBankIndex, track = sound.trackIndex } }, false, s.reverb);
                                sound.soundClips = new XactClip[] { clip };
                            }
                            if(s.variation is not null)
                            {
                                for (int j = 0; j < sound.soundClips.Length; j++)
                                {
                                    if (sound.soundClips[j].clipEvents?[0] is not null)
                                    {
                                        (sound.soundClips[j].clipEvents[0] as PlayWaveEvent).variationType = s.variation.Value;
                                    }
                                }
                            }
                            if (s.volume is not null)
                            {
                                sound.volume = s.volume.Value;
                            }
                            else if (s.minVolume is not null)
                            {
                                sound.complexSound = true;
                                for (int j = 0; j < sound.soundClips.Length; j++)
                                {
                                    if (sound.soundClips[j].clipEvents?[0] is not null)
                                    {
                                        AccessTools.Field(typeof(PlayWaveEvent), "randomVolumeRange").SetValue(sound.soundClips[j].clipEvents[0], new Vector2(s.minVolume.Value, s.maxVolume.Value - s.minVolume.Value));
                                    }
                                }
                            }
                            if (s.pitch is not null)
                            {
                                sound.pitch = s.pitch.Value;
                            }
                            else if (s.minPitch is not null)
                            {
                                sound.complexSound = true;
                                for (int j = 0; j < sound.soundClips.Length; j++)
                                {
                                    if (sound.soundClips[j].clipEvents?[0] is not null)
                                    {
                                        AccessTools.Field(typeof(PlayWaveEvent), "randomPitchRange").SetValue(sound.soundClips[j].clipEvents[0], new Vector2(s.minPitch.Value, s.maxPitch.Value - s.minPitch.Value));
                                    }
                                }
                            }
                            if (s.minFrequency is not null)
                            {
                                sound.complexSound = true;
                                for (int j = 0; j < sound.soundClips.Length; j++)
                                {
                                    if (sound.soundClips[j].clipEvents?[0] is not null)
                                    {
                                        sound.soundClips[j].FilterEnabled = true;
                                        sound.soundClips[j].FilterMode = s.filter;
                                        AccessTools.Field(typeof(PlayWaveEvent), "randomFilterRange").SetValue(sound.soundClips[j].clipEvents[0], new Vector4(s.minFrequency.Value, s.maxFrequency.Value - s.minFrequency.Value, s.minQ.Value, s.maxQ.Value - s.minQ.Value));
                                    }
                                }
                            }
                            if (s.rpcCurves is not null)
                            {
                                sound.rpcCurves = s.rpcCurves;
                            }
                            existingCueDef.sounds.Add(sound);
                        }
                    }
                }
            }
        }

        private void Input_ButtonPressed(object sender, StardewModdingAPI.Events.ButtonPressedEventArgs e)
        {
            if(e.Button == SButton.Delete)
            {
                Game1.stopMusicTrack(Game1.MusicContext.Default);
                ReloadSounds();

                Game1.playSound("snowyStep");
            }
        }

        private void Content_AssetRequested(object sender, StardewModdingAPI.Events.AssetRequestedEventArgs e)
        {
            if (!Config.ModEnabled)
                return;
            if (e.NameWithoutLocale.IsEquivalentTo(dictPath))
            {
                e.LoadFrom(() => new Dictionary<string, TweakData>(), StardewModdingAPI.Events.AssetLoadPriority.Exclusive);
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
                name: () => "Mod Enabled",
                getValue: () => Config.ModEnabled,
                setValue: value => Config.ModEnabled = value
            );
        }
    }
}