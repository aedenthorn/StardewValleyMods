using HarmonyLib;
using Microsoft.Xna.Framework;
using Netcode;
using Newtonsoft.Json;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;
using System.Linq;
using Object = StardewValley.Object;

namespace Guns
{
    public partial class ModEntry
    {
        private static void dontExplodeOnImpact(GameLocation location, int x, int y, Character who)
        {
        }
        private static void explodeOnImpact(GameLocation location, int x, int y, Character who)
        {
            var f = who as Farmer;
            GunData data = gunDict[f.CurrentTool.Name];
            location.explode(new Vector2(x / 64, y / 64), data.explosionRadius, (Farmer)who, true, data.explosionDamage);
        }
    }
}