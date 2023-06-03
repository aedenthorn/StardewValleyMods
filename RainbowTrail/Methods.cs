using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Objects;
using StardewValley.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Object = StardewValley.Object;

namespace RainbowTrail
{
    public partial class ModEntry
    {
        private void ResetTrail()
        {
            trailDict.Remove(Game1.player.UniqueMultiplayerID);
        }

        private static bool RainbowTrailStatus(Farmer player)
        {
            if (!Config.ModEnabled || !player.modData.TryGetValue(rainbowTrailKey, out string str))
                return false;
            return true;

        }
    }
}