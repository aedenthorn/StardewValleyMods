using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Newtonsoft.Json.Linq;
using StardewValley;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Object = StardewValley.Object;

namespace BuffFramework
{
    public partial class ModEntry
    {
        public static float CheckGlowRate(Buff buff, float rate)
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


        public static void UpdateBuffs()
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
                int duration = 50;
                var b = kvp.Value;
                if (b.ContainsKey("consume"))
                {
                    if (farmerBuffs.Value.TryGetValue(kvp.Key, out var eatBuff))
                    {
                        newBuffDict.Add(kvp.Key, eatBuff);
                    }
                    continue;
                }
                if (b.TryGetValue("hat", out var hat) && Game1.player.hat.Value?.Name != (string)hat)
                    continue;
                if (b.TryGetValue("shirt", out var shirt) && Game1.player.shirtItem.Value?.Name != (string)shirt)
                    continue;
                if (b.TryGetValue("pants", out var pants) && Game1.player.pantsItem.Value?.Name != (string)pants)
                    continue;
                if (b.TryGetValue("boots", out var boots) && Game1.player.boots.Value?.Name != (string)boots)
                    continue;
                if (b.TryGetValue("ring", out var ring))
                {
                    bool found = false;
                    if(Game1.player.rightRing.Value is CombinedRing)
                    {
                        foreach(var r in (Game1.player.rightRing.Value as CombinedRing).combinedRings)
                        {
                            if(r.Name == (string)ring)
                            {
                                found = true;
                                break;
                            }
                        }
                    }
                    else if (Game1.player.rightRing.Value?.Name == (string)ring)
                    {
                        found = true;
                    }
                    if (!found)
                    {
                        if (Game1.player.leftRing.Value is CombinedRing)
                        {
                            foreach (var r in (Game1.player.leftRing.Value as CombinedRing).combinedRings)
                            {
                                if (r.Name == (string)ring)
                                {
                                    found = true;
                                    break;
                                }
                            }
                        }
                        else if (Game1.player.leftRing.Value?.Name == (string)ring)
                        {
                            found = true;
                        }
                        if (!found)
                            continue;
                    }
                }

                Buff buff = CreateBuff(kvp.Key, b, null, duration);
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

        public static Buff CreateBuff(string key, Dictionary<string, object> b, object food, int duration)
        {

            Buff buff;
            if (b.TryGetValue("which", out var which) && GetInt(which) > -1)
            {
                buff = new Buff(GetInt(which));
                if (food is null || duration > 50)
                {
                    buff.millisecondsDuration = duration;
                    buff.totalMillisecondsDuration = duration;
                }
            }
            else
            {
                buff = new Buff(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, "", "") { which = GetInt(b["buffId"]) };
                buff.millisecondsDuration = duration;
                buff.totalMillisecondsDuration = duration;
            }

            foreach (var p in b)
            {
                switch (p.Key)
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
                        if (p.Value is Color)
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
                        if (!farmerBuffs.Value.ContainsKey(key))
                        {
                            try
                            {
                                var cue = Game1.soundBank.GetCue((string)p.Value);
                                cue.Play();
                                cues.Add(key, cue);
                            }
                            catch { }
                        }
                        break;
                }
            }
            return buff;
        }

        public static int GetInt(object value)
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
        public static float GetFloat(object r)
        {
            if (r is float)
                return (float)r;
            else if (r is double)
                return (float)(double)r;
            else if (r is string)
                return float.TryParse((string)r, NumberStyles.Any, CultureInfo.InvariantCulture, out float f) ? f : 0;
            return 0;
        }


        public static void ClearCues()
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