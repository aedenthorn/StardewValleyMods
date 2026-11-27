using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Monsters;
using StardewValley.Tools;
using System;
using System.Collections.Generic;

namespace Swim
{
    public partial class ModEntry
    {
        public static List<string> crabTextures = new List<string>()
        {
            "HermitCrab",
            "ChestCrab",
        };

        [HarmonyPatch(typeof(RockCrab), "initNetFields")]
        public static class RockCrab_initNetFields_Patch
        {
            public static void Postfix(Monster __instance)
            {
                if (!IsMonster(__instance))
                    return;
                __instance.position.Field.AxisAlignedMovement = true;
            }
        }

        [HarmonyPatch(typeof(RockCrab), nameof(RockCrab.takeDamage))]
        public static class RockCrab_takeDamage_Patch
        {
            public static bool Prefix(RockCrab __instance, ref int __result)
            {
                if (!IsMonster(__instance))
                    return true;
                __result = 0;
                return false;
            }
        }

        [HarmonyPatch(typeof(RockCrab), nameof(RockCrab.hitWithTool))]
        public static class RockCrab_hitWithTool_Patch
        {
            public static bool Prefix(RockCrab __instance, Tool t, ref bool __result)
            {
                if (!IsMonster(__instance) || t is not Pickaxe || t.getLastFarmerToUse() is null || __instance.shellHealth.Value <= 0)
                    return true;

                __instance.currentLocation.playSound("hammer");

                __instance.shellHealth.Value--;
                __instance.shake(500);
                __instance.setTrajectory(Utility.getAwayFromPlayerTrajectory(__instance.GetBoundingBox(), t.getLastFarmerToUse()));
                if (__instance.shellHealth.Value <= 0)
                {
                    __instance.shellGone.Value = true;
                    __instance.moveTowardPlayer(-1);
                    __instance.currentLocation.playSound("stoneCrack");
                    Game1.createRadialDebris(__instance.currentLocation, 14, (int)__instance.Tile.X, (int)__instance.Tile.Y, Game1.random.Next(2, 7), false, -1, false);
                    Game1.createRadialDebris(__instance.currentLocation, 14, (int)__instance.Tile.X, (int)__instance.Tile.Y, Game1.random.Next(2, 7), false, -1, false);
                }
                __result = true;
                return false;
            }
        }

        [HarmonyPatch(typeof(RockCrab), nameof(RockCrab.behaviorAtGameTick))]
        public static class RockCrab_behaviorAtGameTick_Patch
        {
            public static bool Prefix(RockCrab __instance, GameTime time)
            {
                if (!IsMonster(__instance))
                    return true;

                if (__instance.shellGone.Value)
                {
                    __instance.Sprite.CurrentFrame = 16 + __instance.Sprite.currentFrame % 4;
                }
                if (__instance.withinPlayerThreshold())
                {
                    if (Math.Abs(__instance.Player.GetBoundingBox().Center.Y - __instance.GetBoundingBox().Center.Y) < Math.Abs(__instance.Player.GetBoundingBox().Center.X - __instance.GetBoundingBox().Center.X))
                    {
                        if (__instance.Player.GetBoundingBox().Center.X - __instance.GetBoundingBox().Center.X > 0 && __instance.TilePoint.X > 0)
                        {
                            __instance.SetMovingLeft(true);
                        }
                        else if (__instance.Player.GetBoundingBox().Center.X - __instance.GetBoundingBox().Center.X < 0 && __instance.TilePoint.X < __instance.currentLocation.map.Layers[0].TileWidth)
                        {
                            __instance.SetMovingRight(true);
                        }
                    }
                    else if (__instance.Player.GetBoundingBox().Center.Y - __instance.GetBoundingBox().Center.Y > 0 && __instance.TilePoint.Y > 0)
                    {
                        __instance.SetMovingUp(true);
                    }
                    else if (__instance.Player.GetBoundingBox().Center.Y - __instance.GetBoundingBox().Center.Y < 0 && __instance.TilePoint.Y < __instance.currentLocation.map.Layers[0].TileHeight)
                    {
                        __instance.SetMovingDown(true);
                    }
                    __instance.MovePosition(time, Game1.viewport, __instance.currentLocation);
                }
                else
                {
                    __instance.Halt();
                }
                return false;
            }
        }
        [HarmonyPatch(typeof(RockCrab), "updateMonsterSlaveAnimation")]
        public static class RockCrab_updateMonsterSlaveAnimation_Patch
        {
            public static bool Prefix(RockCrab __instance, GameTime time)
            {
                if (!IsMonster(__instance))
                    return true;

                if (__instance.isMoving())
                {
                    if (__instance.FacingDirection == 0)
                    {
                        __instance.Sprite.AnimateUp(time, 0, "");
                    }
                    else if (__instance.FacingDirection == 3)
                    {
                        __instance.Sprite.AnimateLeft(time, 0, "");
                    }
                    else if (__instance.FacingDirection == 1)
                    {
                        __instance.Sprite.AnimateRight(time, 0, "");
                    }
                    else if (__instance.FacingDirection == 2)
                    {
                        __instance.Sprite.AnimateDown(time, 0, "");
                    }
                }
                else
                {
                    __instance.Sprite.StopAnimation();
                }
                if (__instance.isMoving() && __instance.Sprite.currentFrame % 4 == 0)
                {
                    __instance.Sprite.currentFrame++;
                    __instance.Sprite.UpdateSourceRect();
                }
                if (__instance.shellGone.Value)
                {
                    __instance.updateGlow();
                    if (__instance.invincibleCountdown > 0)
                    {
                        __instance.glowingColor = Color.Cyan;
                        __instance.invincibleCountdown -= time.ElapsedGameTime.Milliseconds;
                        if (__instance.invincibleCountdown <= 0)
                        {
                            __instance.stopGlowing();
                        }
                    }
                    __instance.Sprite.currentFrame = 16 + __instance.Sprite.currentFrame % 4;
                }
                return false;
            }
        }
    }
}
