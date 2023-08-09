using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using Newtonsoft.Json.Linq;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Locations;
using StardewValley.Quests;
using StardewValley.SDKs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using xTile.Dimensions;
using static StardewValley.LocationRequest;
using Object = StardewValley.Object;

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
                if(GetInt(b["buffId"]) == buff.which) 
                    return b.TryGetValue("glowRate", out var r) ? (float)(double)r : 0.05f;
            }
            return rate;
        }
        private static void UpdateBuffs()
        {
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
                if (b.TryGetValue("which", out var which) && (long)which > -1)
                {
                    buff = new Buff((int)(long)which);
                }
                else
                {
                    buff = new Buff(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, minutesDuration: 1, source: (string)b["source"], (string)b["displaySource"]) { which = GetInt(b["buffId"]) };
                }

                foreach (var p in b)
                {
                    switch(p.Key)
                    {
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
                            var j = (JObject)p.Value;
                            buff.glow = new Color((byte)(long)j["R"],(byte)(long)j["G"],(byte)(long)j["B"]);
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
                return int.Parse((string)value);
            }
            else
            {
                return 0;
            }
        }
    }
}