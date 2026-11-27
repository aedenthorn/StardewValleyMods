using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewValley;
using System.Collections.Generic;

namespace StardewVN
{
	public partial class ModEntry
    {
        [HarmonyPatch(typeof(GameLocation), nameof(GameLocation.loadMap))]
        public class GameLocation_loadMap_Patch
        {
            public static void Postfix(GameLocation __instance)
            {
                if (!Config.ModEnabled)
                    return;
                bool changed = false;
                Dictionary<string, Point> dict = null;
                if (!changed && !mapPropertyDict.TryGetValue(__instance.NameOrUniqueName, out dict))
                {
                    changed = true;
                }
                Dictionary<string, Point> newDict = new();
                foreach (var key in __instance.Map?.Properties.Keys)
                {
                    var val = __instance.GetMapPropertySplitBySpaces(key);
                    if (ArgUtility.TryGetPoint(val, 0, out Point parsed, out var error, "parsed"))
                    {
                        newDict.Add(key, parsed);
                        if (!changed && (dict?.TryGetValue(key, out var oldPoint) != true || oldPoint != parsed))
                        {
                            changed = true;
                        }
                    }
                }
                if (!changed && dict?.Count != newDict.Count)
                    changed = true;


                if (changed)
                {
                    mapPropertyDict[__instance.NameOrUniqueName] = newDict;
                    NotifyMapChanged();
                }
            }
        }
    }
}
