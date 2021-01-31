using Harmony;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;

namespace LooseAudioFiles
{
    /// <summary>The mod entry point.</summary>
    public class ModEntry : Mod
    {
        private static Dictionary<string, WaveBankFileData> waveBankFileDatas = new Dictionary<string, WaveBankFileData>();
        private static Dictionary<string, List<SoundEffect>> EffectsList = new Dictionary<string, List<SoundEffect>>();
        private static Dictionary<string, SoundEffectInstance> EffectInstancesList = new Dictionary<string, SoundEffectInstance>();
        private static Dictionary<string, float> CuePitches = new Dictionary<string, float>();

        private static ModConfig Config;
        private static IMonitor PMonitor;

        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            Config = this.Helper.ReadConfig<ModConfig>();
            if (!Config.EnableMod)
                return;

            PMonitor = Monitor;

            try
            {
                waveBankFileDatas.Add(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), Helper.Content.Load<WaveBankFileData>("wavebank.json", ContentSource.ModFolder));
            }
            catch
            {
                Monitor.Log("wavebank.json file not found!", LogLevel.Debug);
            }

            foreach (IContentPack contentPack in this.Helper.ContentPacks.GetOwned())
            {
                try
                {
                    this.Monitor.Log($"Reading content pack: {contentPack.Manifest.Name} {contentPack.Manifest.Version} from {contentPack.DirectoryPath}");
                    WaveBankFileData data = contentPack.ReadJsonFile<WaveBankFileData>("wavebank.json");
                    waveBankFileDatas.Add(contentPack.DirectoryPath, data);
                }
                catch
                {
                    PMonitor.Log($"wavebank.json file error in content pack {contentPack.Manifest.Name}", LogLevel.Warn);
                }
            }
            Monitor.Log($"Got {waveBankFileDatas.Count} wavebanks total", LogLevel.Debug);

            var harmony = HarmonyInstance.Create(this.ModManifest.UniqueID);

            harmony.Patch(
               original: AccessTools.Method(typeof(Cue), "SetVariable"),
               prefix: new HarmonyMethod(typeof(ModEntry), nameof(Cue_SetVariable_Prefix))
            );
            harmony.Patch(
               original: AccessTools.Method(typeof(SoundBank), "PlayCue", new Type[]{ typeof(string) }),
               prefix: new HarmonyMethod(typeof(ModEntry), nameof(SoundBank_PlayCue_Prefix))
            );
            harmony.Patch(
               original: AccessTools.Method(typeof(Cue), "Play"),
               prefix: new HarmonyMethod(typeof(ModEntry), nameof(Cue_Play_Prefix))
            );
        }

        private static void Cue_SetVariable_Prefix(string ____name, string name, float value)
        {
            if(name == "Pitch")
            {
                if (CuePitches.ContainsKey(____name))
                {
                    CuePitches[____name] = value;
                }
                else
                {
                    CuePitches.Add(____name, value);
                }
            }
        }

        private static bool SoundBank_PlayCue_Prefix(SoundBank __instance, string name)
        {
            string cueName = name;

            try
            {
                SoundEffect effect = GetSoundEffect(cueName);
                if (effect == null)
                {
                    if (Config.VerboseLog)
                    {
                        PMonitor.Log($"No audio entry for {cueName}", LogLevel.Debug);
                    }
                    return true;
                }

                float oPitch = -1;
                float pitch = 0f;
                float volume = 1f;
                if (CuePitches.ContainsKey(cueName))
                {
                    oPitch = CuePitches[cueName];
                    pitch = (float)(oPitch - 1200) / 1200f;
                    ;
                }
                try
                {
                    volume = (__instance.GetCue(cueName).GetVariable("Volume"))/ 100f;
                }
                catch
                {

                }

                effect.Play(volume == 0 ? 1f : volume, pitch,0);
                if (Config.VerboseLog)
                {
                    PMonitor.Log($"played soundbank cue audio file {cueName} at volume {volume}, pitch {oPitch}", LogLevel.Debug);
                }
                return false;
            }
            catch (Exception ex)
            {
                PMonitor.Log($"couldn't play soundbank cue audio file {cueName} {ex}", LogLevel.Warn);
            }
            return true;
        }
        
        private static bool Cue_Play_Prefix(Cue __instance, string ____name, ref bool ___played)
        {
            string cueName = ____name;

            try
            {
                SoundEffect effect = GetSoundEffect(cueName);
                if (effect == null)
                {
                    if (Config.VerboseLog)
                    {
                        PMonitor.Log($"No audio entry for {cueName}", LogLevel.Debug);
                    }
                    return true;
                }

                float oPitch = -1;
                float pitch = 0f;
                float volume = 1f;
                if (CuePitches.ContainsKey(cueName))
                {
                    oPitch = CuePitches[cueName];
                    pitch = (float)(oPitch - 1200) / 1200f;
                    ;
                }
                try
                {
                    volume = (__instance.GetVariable("Volume")) / 100f;
                }
                catch
                {

                }

                effect.Play(volume == 0 ? 1f : volume, pitch,0);
                if (Config.VerboseLog)
                {
                    PMonitor.Log($"played cue audio file {cueName} at volume {volume}, pitch {oPitch}", LogLevel.Debug);
                }
                ___played = true;
                return false;
            }
            catch (Exception ex)
            {
                PMonitor.Log($"couldn't play cue audio file {cueName} {ex}", LogLevel.Warn);
            }
            return true;
        }

        private static bool FarmAnimal_makeSound_Prefix(FarmAnimal __instance)
        {
            string cueName = __instance.sound.Value;
            try
            {
                SoundEffect effect = GetSoundEffect(cueName);
                if(effect == null)
                {
                    if (Config.VerboseLog)
                    {
                        PMonitor.Log($"No audio entry for {cueName}", LogLevel.Debug);
                    }
                    return true;
                }
                float pitch = (Game1.random.Next(-200, 201)) / 1200f;
                effect.Play(1.0f, pitch, 0);
                if (Config.VerboseLog)
                {
                    PMonitor.Log($"played farm animal audio file {cueName} at pitch {pitch}", LogLevel.Debug);
                }
                return false;
            }
            catch (Exception ex)
            {
                PMonitor.Log($"couldn't play audio file {cueName} {ex}", LogLevel.Warn);
            }
            return true;
        }

        private static bool playSound_Prefix(string cueName)
        {
            try
            {
                SoundEffect effect = GetSoundEffect(cueName);
                if (effect == null)
                {
                    if (Config.VerboseLog)
                    {
                        PMonitor.Log($"No audio entry for {cueName}", LogLevel.Debug);
                    }
                    return true;
                }

                effect.Play();
                if (Config.VerboseLog)
                {
                    PMonitor.Log($"played audio file {cueName}", LogLevel.Debug);
                }
                return false;
            }
            catch(Exception ex)
            {
                PMonitor.Log($"couldn't play audio file {cueName} {ex}", LogLevel.Warn);
            }
            return true;
        }
        private static bool playSoundPitched_Prefix(string cueName, int pitch)
        {
            try
            {
                SoundEffect effect = GetSoundEffect(cueName);
                if (effect == null)
                {
                    if (Config.VerboseLog)
                    {
                        PMonitor.Log($"No audio entry for {cueName}", LogLevel.Debug);
                    }
                    return true;
                }
                effect.Play(1.0f, pitch, 0);
                if (Config.VerboseLog)
                {
                    PMonitor.Log($"played audio file {cueName} at pitch {pitch}", LogLevel.Debug);
                }
                return false;
            }
            catch(Exception ex)
            {
                PMonitor.Log($"couldn't play pitched audio file {cueName} {ex}", LogLevel.Warn);
            }
            return true;
        }

        private static SoundEffect GetSoundEffect(string cueName)
        {

            foreach (KeyValuePair<string, WaveBankFileData> kvp in waveBankFileDatas)
            {
                if (kvp.Value.wavebank.ContainsKey(cueName))
                {
                    if (!EffectsList.ContainsKey(cueName))
                    {
                        EffectsList.Add(cueName, new List<SoundEffect>());
                        foreach (string code in kvp.Value.wavebank[cueName])
                        {
                            string filePath = $"{kvp.Key}\\assets\\{code}.wav";
                            SoundEffect effect = SoundEffect.FromStream(new FileStream(filePath, FileMode.Open));
                            EffectsList[cueName].Add(effect);
                        }
                    }
                    int idx = Game1.random.Next(0, EffectsList[cueName].Count);
                    return EffectsList[cueName][idx];

                }
            }
            return null;
        }
    }
}