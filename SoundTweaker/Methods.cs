using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using StardewModdingAPI;
using StardewValley;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SoundTweaker
{
    public partial class ModEntry
    {
        private static void ReloadSounds()
        {
            /*
            tweakDict.Add("snowyStep", new TweakData()
            {
                sounds = new List<SoundInfo> { new SoundInfo() { filePaths = { "aedenthorn.TestSound/guitar", "aedenthorn.TestSound/drum" }, cuePaths = { "toyPiano", "flute" }, soundIndexes = { 257, 258, 259 }, minVolume = 0.75f, maxVolume = 1f, minPitch = 1f, maxPitch = 2f, reverb = false, minFrequency = 0, maxFrequency = 1000, minQ = 0, maxQ = 500, filter = FilterMode.HighPass } }
            });
            */
            tweakDict = SHelper.GameContent.Load<Dictionary<string, TweakData>>(dictPath);

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
            foreach(var os in originalSounds)
            {
                if (!tweakDict.ContainsKey(os.Key))
                {
                    var existingCueDef = Game1.soundBank.GetCueDefinition(os.Key);
                    existingCueDef.sounds = os.Value.ToList();
                }
            }
            foreach (var key in tweakDict.Keys.ToArray())
            {
                var existingCueDef = Game1.soundBank.GetCueDefinition(key);
                if(!originalSounds.TryGetValue(key, out XactSoundBankSound[] oldSounds))
                {
                    oldSounds = existingCueDef.sounds.ToArray();
                    originalSounds[key] = oldSounds;
                }
                existingCueDef.sounds.Clear();
                if(tweakDict[key].maxInstances is not null)
                {
                    existingCueDef.instanceLimit = tweakDict[key].maxInstances.Value;
                }
                if(tweakDict[key].limitBehavior is not null)
                {
                    existingCueDef.limitBehavior = tweakDict[key].limitBehavior.Value;
                }
                if (tweakDict[key].sounds is null || tweakDict[key].sounds.Count == 0)
                {
                    existingCueDef.sounds.Add(new XactSoundBankSound(new SoundEffect[0], Game1.audioEngine.GetCategoryIndex("Default")) { soundClips = new XactClip[0] });
                }
                else
                {
                    foreach (var s in tweakDict[key].sounds)
                    {
                        var outsounds = new List<XactSoundBankSound>();
                        if (s.filePaths is not null)
                        {
                            foreach (var str in s.filePaths)
                            {
                                var cat = Game1.audioEngine.GetCategoryIndex(s.category);
                                SoundEffect audio;
                                string dirPath;
                                string filePath;
                                if (str.StartsWith("SMAPI/"))
                                {
                                    var parts = str.Split('/', 3);
                                    IModInfo info = SHelper.ModRegistry.Get(parts[1]);
                                    if(info is not null)
                                    {
                                        dirPath = (string)AccessTools.Property(info.GetType(), "DirectoryPath").GetValue(info);
                                        filePath = parts[2];
                                    }
                                    else
                                    {
                                        continue;
                                    }
                                }
                                else
                                {
                                    dirPath = SHelper.DirectoryPath;
                                    filePath = str;
                                }
                                string filePathCombined = Path.Combine(dirPath, filePath);

                                if (File.Exists(filePathCombined))
                                {
                                    using (var stream = new FileStream(filePathCombined, FileMode.Open))
                                    {
                                        audio = SoundEffect.FromStream(stream);
                                    }
                                    outsounds.Add(new XactSoundBankSound(new SoundEffect[] { audio }, s.category is null ? (int)oldSounds[0].categoryID : Game1.audioEngine.GetCategoryIndex(s.category), s.loop, s.reverb));

                                }
                            }
                        }
                        if (s.cuePaths is not null)
                        {
                            foreach (var str in s.cuePaths)
                            {
                                var existingCueDef2 = Game1.soundBank.GetCueDefinition(str);
                                if (existingCueDef2 != null)
                                {
                                    var sounds = (str == key) ? oldSounds : existingCueDef2.sounds.ToArray();

                                    for (int i = 0; i < sounds.Length; i++)
                                    {
                                        outsounds.Add(sounds[i]);
                                    }
                                }
                            }
                        }
                        if (s.soundIndexes is not null)
                        {
                            foreach (var i in s.soundIndexes)
                            {
                                XactSoundBankSound sound = new XactSoundBankSound(soundBank, 0, i)
                                {
                                    useReverb = s.reverb
                                };
                                outsounds.Add(sound);
                            }
                        }
                        for (int i = 0; i < outsounds.Count; i++)
                        {
                            var sound = outsounds[i];
                            sound.useReverb = s.reverb;
                            if (sound.soundClips is null)
                            {
                                var clip = new XactClip(new List<PlayWaveVariant> { new PlayWaveVariant() { soundBank = sound.soundBank, waveBank = sound.waveBankIndex, track = sound.trackIndex } }, false, s.reverb);
                                sound.soundClips = new XactClip[] { clip };
                            }
                            if (s.variationType is not null)
                            {
                                for (int j = 0; j < sound.soundClips.Length; j++)
                                {
                                    if (sound.soundClips[j].clipEvents?[0] is not null)
                                    {
                                        (sound.soundClips[j].clipEvents[0] as PlayWaveEvent).variationType = s.variationType.Value;
                                    }
                                }
                            }
                            if (s.volume is not null)
                            {
                                if (sound.complexSound)
                                {
                                    for (int j = 0; j < sound.soundClips.Length; j++)
                                    {
                                        if (sound.soundClips[j].clipEvents?[0] is not null)
                                        {
                                            AccessTools.Field(typeof(PlayWaveEvent), "randomVolumeRange").SetValue(sound.soundClips[j].clipEvents[0], new Vector2(s.volume.Value, 0));
                                        }
                                    }
                                }
                                else
                                {
                                    sound.volume = s.volume.Value;
                                }
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
                                if (sound.complexSound)
                                {
                                    for (int j = 0; j < sound.soundClips.Length; j++)
                                    {
                                        if (sound.soundClips[j].clipEvents?[0] is not null)
                                        {
                                            AccessTools.Field(typeof(PlayWaveEvent), "randomVolumeRange").SetValue(sound.soundClips[j].clipEvents[0], new Vector2(s.pitch.Value, 0));
                                        }
                                    }
                                }
                                else
                                {
                                    sound.pitch = s.pitch.Value;
                                }
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
                                        sound.soundClips[j].FilterMode = s.filterMode;
                                        AccessTools.Field(typeof(PlayWaveEvent), "randomFilterRange").SetValue(sound.soundClips[j].clipEvents[0], new Vector4(s.minFrequency.Value, s.maxFrequency.Value - s.minFrequency.Value, s.minQ.Value, s.maxQ.Value - s.minQ.Value));
                                    }
                                }
                            }
                            if (s.rpcCurves is not null)
                            {
                                sound.rpcCurves = s.rpcCurves;
                            }
                            if (s.category is not null)
                            {
                                sound.categoryID = (uint)Game1.audioEngine.GetCategoryIndex(s.category);
                            }
                            existingCueDef.sounds.Add(sound);
                        }
                    }
                }
            }
        }
    }
}