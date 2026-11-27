using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Monsters;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Xml.Serialization;
using static StardewValley.Minigames.CraneGame;

namespace Swim
{
    public partial class ModEntry
    {

        public static List<string> bigFishTextures = new List<string>()
        {
            "BigFishBlack",
            "BigFishBlue",
            "BigFishGold",
            "BigFishGreen",
            "BigFishGreenWhite",
            "BigFishGrey",
            "BigFishRed",
            "BigFishWhite"
        };

        [HarmonyPatch(typeof(Monster), nameof(Monster.drawAboveAllLayers))]
        public static class Monster_drawAboveAllLayers_Patch
        {
            public static bool Prefix(Monster __instance)
            {
                if (!IsMonster(__instance))
                    return true;
                __instance.invincibleCountdown = 1000;
                return false;
            }
        }

        [HarmonyPatch(typeof(DinoMonster), nameof(DinoMonster.behaviorAtGameTick))]
        public static class DinoMonster_behaviorAtGameTick_Patch
        {
            public static bool Prefix(DinoMonster __instance, GameTime time, ref bool ___moveLeft, ref bool ___moveRight, ref bool ___moveUp, ref bool ___moveDown)
            {
                if (!IsMonster(__instance))
                    return true;

                __instance.IsWalkingTowardPlayer = false;
                __instance.nextChangeDirectionTime -= time.ElapsedGameTime.Milliseconds;
                __instance.nextWanderTime -= time.ElapsedGameTime.Milliseconds;
                if (__instance.nextChangeDirectionTime < 0)
                {
                    __instance.nextChangeDirectionTime = Game1.random.Next(500, 1000);
                    int facingDirection = __instance.FacingDirection;
                    __instance.facingDirection.Value = (__instance.facingDirection.Value + (Game1.random.Next(0, 3) - 1) + 4) % 4;
                }
                if (__instance.nextWanderTime < 0)
                {
                    if (__instance.wanderState)
                    {
                        __instance.nextWanderTime = Game1.random.Next(1000, 2000);
                    }
                    else
                    {
                        __instance.nextWanderTime = Game1.random.Next(1000, 3000);
                    }
                    __instance.wanderState = !__instance.wanderState;
                }
                if (__instance.wanderState)
                {
                    ___moveLeft = (___moveUp = (___moveRight = (___moveDown = false)));
                    __instance.tryToMoveInDirection(__instance.facingDirection.Value, false, __instance.DamageToFarmer, __instance.isGlider.Value);
                }
                return false;
            }
        }

    }
}
