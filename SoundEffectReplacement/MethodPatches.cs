using HarmonyLib;
using Microsoft.Xna.Framework.Audio;
using System;
using System.Collections.Generic;

namespace SoundEffectReplacement
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry
    {
        private static bool playSound_Prefix(ref string cueName)
        {
            if (!Config.EnableMod || !replacementDict.ContainsKey(cueName))
                return true;
            cueName = replacementDict[cueName];
            if (cueName.Length == 0)
                return false;
            if (soundEffectDict.ContainsKey(cueName))
            {
                soundEffectDict[cueName].Play();
                return false;
            }
            return true;
        }
        private static bool SoundBank_PlayCuePrefix(ref string name)
        {
            if (!Config.EnableMod || !replacementDict.ContainsKey(name))
                return true;
            name = replacementDict[name];
            if (name.Length == 0)
                return false;
            if (soundEffectDict.ContainsKey(name))
            {
                soundEffectDict[name].Play();
                return false;
            }
            return true;
        }
        private static bool SoundBank_GetCuePrefix(SoundBank __instance, string name, Dictionary<string, CueDefinition> ____cues, AudioEngine ____audioengine, ref Cue __result)
        {
            if (!Config.EnableMod || !replacementDict.ContainsKey(name) || string.IsNullOrEmpty(name))
                return true;
            CueDefinition cue_definition = __instance.GetCueDefinition(name);
            if (cue_definition.sounds.Count == 0)
                return true;
            int cat = (int)cue_definition.sounds[0].categoryID;
            bool reverb = cue_definition.sounds[0].useReverb;
            CueDefinition cue_definition2 = cue_definition;

            string newName = replacementDict[name];
            if (newName.Length == 0)
            {
                cue_definition2.sounds = null;
            }
            else if (soundEffectDict.ContainsKey(newName))
            {
                cue_definition2.sounds = new List<XactSoundBankSound>()
                {
                    new XactSoundBankSound(new SoundEffect[]{ soundEffectDict[newName] }, cat, false, reverb )
                };
            }
            else if (!____cues.TryGetValue(newName, out cue_definition2))
            {
                return true;
            }
            __result = (Cue)AccessTools.Constructor(typeof(Cue), new Type[] { typeof(AudioEngine), typeof(CueDefinition) }).Invoke(new object[] { ____audioengine, cue_definition2 });
            return false;
        }
    }
}