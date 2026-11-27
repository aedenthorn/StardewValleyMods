using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Network;
using StardewValley.Objects;
using StardewValley.Quests;
using StardewValley.Tools;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Reflection;
using xTile.Dimensions;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;
using System;
using Rectangle = Microsoft.Xna.Framework.Rectangle;
using Object = StardewValley.Object;
using StardewValley.Characters;
using static StardewValley.Minigames.CraneGame;
using StardewValley.Locations;
using System.Text.RegularExpressions;

namespace ItemAboveHead
{
    public partial class ModEntry
    {
        [HarmonyPatch(typeof(Event), nameof(Event.command_itemAboveHead))]
        public class Event_command_itemAboveHead_Patch
        {
            public static bool Prefix(Event __instance, string[] split)
            {
                if (!Config.ModEnabled || split.Length < 3)
                    return true;
                SMonitor.Log($"command_itemAboveHead {string.Join(" ", split)}, show {split[2]}");
                int i = -1;
                if (!int.TryParse(split[1], out i) && Regex.IsMatch(split[1].Substring(0, 1), @"[A-Z]"))
                {
                    try
                    {
                        i = Game1.objectInformation.First(p => p.Value.StartsWith(split[1])).Key;
                    }
                    catch { }
                }
                if (i == -1)
                    return true;
                SMonitor.Log($"Got index {i}");
                var show = split.Length > 2 && split[2] == "show";
                var obj = new Object(i, 1, false, -1, 0);
                SMonitor.Log($"Holding up item {obj.Name}, show message: {show}");
                __instance.farmer.holdUpItemThenMessage(obj, show ? true : false);
                __instance.CurrentCommand++;
                return false;
            }
        }
    }
}