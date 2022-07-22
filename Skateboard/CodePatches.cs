using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley;
using System;
using System.Collections.Generic;

namespace Skateboard
{
    public partial class ModEntry
    {
        public static Rectangle source = Rectangle.Empty;

        [HarmonyPatch(typeof(Farmer), nameof(Farmer.draw), new Type[] {typeof(SpriteBatch) })]
        public class Farmer_draw_Patch
        {
            public static void Prefix(Farmer __instance, SpriteBatch b)
            {
                if (!Config.ModEnabled || !onSkateboard)
                    return;
                Vector2 offset = new Vector2(0, 0);
                if(source != Rectangle.Empty && speed == Vector2.Zero)
                {

                }
                else if (((speed.X > 0 && speed.Y > 0) || (speed.X < 0 && speed.Y < 0)) & Math.Abs(Math.Abs(speed.X) - Math.Abs(speed.Y)) < Math.Max(Math.Abs(speed.X), Math.Abs(speed.Y)) / 2)
                {
                    source = new Rectangle(11, 0, 11, 12);
                }
                else if(((speed.X > 0 && speed.Y < 0) || (speed.X > 0 && speed.Y < 0)) & Math.Abs(Math.Abs(speed.X) - Math.Abs(speed.Y)) < Math.Max(Math.Abs(speed.X), Math.Abs(speed.Y)) / 2)
                {
                    source = new Rectangle(0, 0, 11, 12);
                }
                else if(Math.Abs(speed.Y) > Math.Abs(speed.X))
                {
                    source = new Rectangle(38, 0, 7, 12);
                }
                else
                {
                    source = new Rectangle(22, 0, 16, 12);
                }

                switch(source.X)
                {
                    case 38:
                        offset += new Vector2(14, 0);
                        break;
                    case 22:
                        offset += new Vector2(-8, 0);
                        break;
                }


                b.Draw(boardTexture, Game1.GlobalToLocal(__instance.Position) + offset, source, Color.White, 0, Vector2.Zero, 5, SpriteEffects.None, 0.0001f);
            }
        }

        [HarmonyPatch(typeof(Farmer), nameof(Farmer.getMovementSpeed))]
        public class Farmer_getMovementSpeed_Patch
        {
            public static void Postfix(Farmer __instance, ref float __result)
            {
                if (!Config.ModEnabled || !onSkateboard)
                    return;

                __result = speed.Length();
            }
        }
        [HarmonyPatch(typeof(Farmer), nameof(Farmer.MovePosition))]
        public class Farmer_MovePosition_Patch
        {
            public static void Prefix(Farmer __instance, ref FarmerState __state)
            {
                if (!Config.ModEnabled || !onSkateboard || !__instance.IsLocalPlayer)
                    return;
                __state = new FarmerState();
                __state.pos = __instance.position.Value;

                __instance.movementDirections.Clear();
                if (Game1.isOneOfTheseKeysDown(Game1.input.GetKeyboardState(), Game1.options.moveUpButton) ||
                    (Game1.options.gamepadControls && ((double)Game1.input.GetGamePadState().ThumbSticks.Left.Y > 0.25 || Game1.input.GetGamePadState().IsButtonDown(Buttons.DPadUp))))
                {
                    __instance.movementDirections.Add(0);
                }
                if (Game1.isOneOfTheseKeysDown(Game1.input.GetKeyboardState(), Game1.options.moveDownButton) ||
                    (Game1.options.gamepadControls && ((double)Game1.input.GetGamePadState().ThumbSticks.Left.Y < -0.25 || Game1.input.GetGamePadState().IsButtonDown(Buttons.DPadDown))))
                {
                    __instance.movementDirections.Add(2);
                }
                if (Game1.isOneOfTheseKeysDown(Game1.input.GetKeyboardState(), Game1.options.moveLeftButton) ||
                    (Game1.options.gamepadControls && ((double)Game1.input.GetGamePadState().ThumbSticks.Left.X < -0.25 || Game1.input.GetGamePadState().IsButtonDown(Buttons.DPadLeft))))
                {
                    __instance.movementDirections.Add(3);
                }
                if (Game1.isOneOfTheseKeysDown(Game1.input.GetKeyboardState(), Game1.options.moveRightButton) ||
                    (Game1.options.gamepadControls && ((double)Game1.input.GetGamePadState().ThumbSticks.Left.X > 0.25 || Game1.input.GetGamePadState().IsButtonDown(Buttons.DPadRight))))
                {
                    __instance.movementDirections.Add(1);
                }
                __state.dirs = new List<int>(__instance.movementDirections);
                accellerating = __instance.movementDirections.Count > 0;

                if (Game1.player.getMovementSpeed() > 0.1f * Config.Deccelleration)
                {
                    Vector2 s = speed;
                    s.Normalize();
                    speed -= (s * 0.05f * Config.Deccelleration);
                }
                else
                {
                    speed = Vector2.Zero;
                }
                if (accellerating)
                {
                    if (Game1.player.movementDirections.Count == 0)
                    {
                        accellerating = false;
                    }
                    else
                    {
                        float mult = 0.1f * Config.Accelleration;
                        foreach (var d in Game1.player.movementDirections)
                        {
                            switch (d)
                            {
                                case 0:
                                    speed.Y -= mult;
                                    break;
                                case 1:
                                    speed.X += mult;
                                    break;
                                case 2:
                                    speed.Y += mult;
                                    break;
                                case 3:
                                    speed.X -= mult;
                                    break;
                            }
                        }
                        speed.X = Math.Min(Config.MaxSpeed, speed.X);
                        speed.Y = Math.Min(Config.MaxSpeed, speed.Y);
                    }
                }


                if (speed.X < 0)
                {
                    __instance.movementDirections.Remove(1);
                    if (!__instance.movementDirections.Contains(3))
                        __instance.movementDirections.Add(3);
                }
                else if (speed.X > 0)
                {
                    __instance.movementDirections.Remove(3);
                    if (!__instance.movementDirections.Contains(1))
                        __instance.movementDirections.Add(1);
                }
                if (speed.Y < 0)
                {
                    __instance.movementDirections.Remove(2);
                    if (!__instance.movementDirections.Contains(0))
                        __instance.movementDirections.Add(0);
                }
                else if (speed.Y > 0)
                {
                    __instance.movementDirections.Remove(0);
                    if (!__instance.movementDirections.Contains(2))
                        __instance.movementDirections.Add(2);
                }


            }
            public static void Postfix(Farmer __instance, FarmerState __state)
            {
                if (!Config.ModEnabled || !onSkateboard || !__instance.IsLocalPlayer || speed == Vector2.Zero)
                    return;
                __instance.movementDirections = __state.dirs;
                var diff = __instance.position.Value - __state.pos;
                var newDiff = speed;
                newDiff.Normalize();
                __instance.position.Value = __state.pos + newDiff * diff.Length();
            }
        }

    }
}