using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Newtonsoft.Json.Linq;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace BuffFramework
{
    public partial class ModEntry
    {
        private static float CheckGlowRate(Buff buff, float rate)
        {
            if (!Config.ModEnabled)
                return rate;
            foreach(var b in buffDict.Values)
            {
                if((b.TryGetValue("buffId", out var id) && GetInt(id) == buff.which) || (b.TryGetValue("which", out var which) && GetInt(which) == buff.which)) 
                    return b.TryGetValue("glowRate", out var r) ? GetFloat(r) : 0.05f;
            }
            return rate;
        }


        private static void UpdateBuffs()
        {
            if (farmerBuffs.Value is null)
                farmerBuffs.Value = new();
            var oldBuffDict = new Dictionary<string, Dictionary<string, object>>();
            foreach (var b in buffDict)
            {
                oldBuffDict.Add(b.Key, b.Value);
            }
            SHelper.GameContent.InvalidateCache(dictKey);
            buffDict = SHelper.GameContent.Load<Dictionary<string, Dictionary<string, object>>>(dictKey);
            var newBuffDict = new Dictionary<string, Buff>();
            foreach(var kvp in buffDict)
            {
                var b = kvp.Value;
                Buff buff;
                if (b.TryGetValue("which", out var which) && GetInt(which) > -1)
                {
                    buff = new Buff(GetInt(which));
                }
                else
                {
                    buff = new Buff(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, "", "") { which = GetInt(b["buffId"]) };
                }
                buff.millisecondsDuration = 50;
                buff.totalMillisecondsDuration = 50;

                foreach (var p in b)
                {
                    switch(p.Key)
                    {
                        case "source":
                            buff.source = (string)p.Value;
                            break;
                        case "displaySource":
                            buff.displaySource = (string)p.Value;
                            break;
                        case "farming":
	                        buff.buffAttributes[Buff.farming] = GetInt(p.Value);
	                        break;
                        case "fishing":
	                        buff.buffAttributes[Buff.fishing] = GetInt(p.Value);
	                        break;
                        case "mining":
	                        buff.buffAttributes[Buff.mining] = GetInt(p.Value);
	                        break;
                        case "luck":
	                        buff.buffAttributes[Buff.luck] = GetInt(p.Value);
	                        break;
                        case "foraging":
	                        buff.buffAttributes[Buff.foraging] = GetInt(p.Value);
	                        break;
                        case "crafting":
	                        buff.buffAttributes[Buff.crafting] = GetInt(p.Value);
	                        break;
                        case "maxStamina":
	                        buff.buffAttributes[Buff.maxStamina] = GetInt(p.Value);
	                        break;
                        case "magneticRadius":
	                        buff.buffAttributes[Buff.magneticRadius] = GetInt(p.Value);
	                        break;
                        case "speed":
	                        buff.buffAttributes[Buff.speed] = GetInt(p.Value);
	                        break;
                        case "defense":
	                        buff.buffAttributes[Buff.defense] = GetInt(p.Value);
	                        break;
                        case "attack":
	                        buff.buffAttributes[Buff.attack] = GetInt(p.Value);
	                        break;
                        case "sheetIndex":
                            buff.sheetIndex = GetInt(p.Value);
                            break;
                        case "glow":
                            if(p.Value is Color)
                            {
                                buff.glow = (Color)p.Value;
                            }
                            else if (p.Value is JObject)
                            {
                                var j = (JObject)p.Value;
                                buff.glow = new Color((byte)(long)j["R"], (byte)(long)j["G"], (byte)(long)j["B"]);
                            }
                            break;
                        case "description":
                            buff.description = (string)p.Value;
                            break;
                        case "sound":
                            if (!farmerBuffs.Value.ContainsKey(kvp.Key))
                            {
                                try
                                {
                                    var cue = Game1.soundBank.GetCue((string)p.Value);
                                    cue.Play();
                                    cues.Add(kvp.Key, cue);
                                }
                                catch { }
                            }
                            break;
                    }
                }
                newBuffDict.Add(kvp.Key, buff);
            }
            foreach(var b in farmerBuffs.Value)
            {
                if (!newBuffDict.ContainsKey(b.Key))
                {
                    if(cues.TryGetValue(b.Key, out var cue))
                    {
                        if (cue.IsPlaying)
                        {
                            cue.Stop(AudioStopOptions.Immediate);
                        }
                        cues.Remove(b.Key);
                    }
                }
            }
            farmerBuffs.Value = newBuffDict;
        }

        private static int GetInt(object value)
        {
            if(value is long)
            {
                return (int)(long)value;
            }
            else if(value is int)
            {
                return (int)value;
            }
            else if(value is string)
            {
                return int.TryParse((string)value, out int i) ? i : 0;
            }
            else
            {
                return 0;
            }

        }
        private static float GetFloat(object r)
        {
            if (r is float)
                return (float)r;
            else if (r is double)
                return (float)(double)r;
            else if (r is string)
                return float.TryParse((string)r, NumberStyles.Any, CultureInfo.InvariantCulture, out float f) ? f : 0;
            return 0;
        }


        private static void ClearCues()
        {
            foreach (var cue in cues)
            {
                if (cue.Value.IsPlaying)
                {
                    cue.Value.Stop(Microsoft.Xna.Framework.Audio.AudioStopOptions.Immediate);
                }
            }
            cues.Clear();
        }
    }
}