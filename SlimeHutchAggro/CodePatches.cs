using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Netcode;
using Newtonsoft.Json.Linq;
using StardewValley;
using StardewValley.Buffs;
using StardewValley.GameData.Objects;
using StardewValley.Menus;
using StardewValley.Monsters;
using StardewValley.Triggers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using Object = StardewValley.Object;

namespace SlimeHutchAggro
{
    public partial class ModEntry
    {
        [HarmonyPatch(typeof(NPC), nameof(NPC.withinPlayerThreshold), new Type[] { typeof(int) })]
        public static class NPC_withinPlayerThreshold_Patch
        {
            public static bool Prefix(NPC __instance, int threshold, ref bool __result)
            {
                if(!Config.ModEnabled || __instance is not GreenSlime slime || slime.currentLocation is not SlimeHutch hutch)
                    return true;
                if (!hutch.farmers.Any() || Config.NoAggro)
                {
                    return false;
                }
                var farmer = ((Farmer)AccessTools.Method(typeof(Monster), "findPlayer").Invoke(slime, Array.Empty<object>()));
                Vector2 playerTile = farmer.Tile;
                Vector2 slimeTile = slime.Tile;
                if (Config.NoBlockedAggro)
                {
                    var size = slime.GetBoundingBox().Size;
                    Vector2 slimePos = slime.StandingPixel.ToVector2();
                    Vector2 playerPos = farmer.StandingPixel.ToVector2();
                    Vector2 pos = slimePos;
                    for( ; ; )
                    {
                        var d = Vector2.Distance(pos, playerPos);
                        pos = Vector2.Lerp(pos, playerPos, 32f / d);
                        if (d < 64)
                            break;
                        if (hutch.isCollidingPosition(new Rectangle(pos.ToPoint(), size), Game1.viewport, slime))
                        {

                            return false;
                        }
                    }
                }
                __result = Math.Abs(slimeTile.X - playerTile.X) <= threshold && Math.Abs(slimeTile.Y - playerTile.Y) <= threshold;
                return false;
            }
        }
    }
}