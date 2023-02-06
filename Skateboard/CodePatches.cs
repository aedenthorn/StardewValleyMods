using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using xTile.Dimensions;
using Object = StardewValley.Object;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace Skateboard
{
    public partial class ModEntry
    {
        public static bool checkingPos;
        public static Vector2 lastPos;
        public static Vector2 lastSpeed;
        public static Vector2 speed;

        [HarmonyPatch(typeof(Character), nameof(Character.GetShadowOffset))]
        public class Character_GetShadowOffset_Patch
        {
            public static void Postfix(Character __instance, ref Vector2 __result)
            {
                if (!Config.ModEnabled || Game1.eventUp || __instance is not Farmer || !__instance.modData.ContainsKey(skateboardingKey))
                    return;
                string source;
                if (!__instance.modData.TryGetValue(sourceKey, out source))
                {
                    source = "0";
                    __instance.modData[sourceKey] = source;
                }

                switch (source)
                {
                    case "0":
                    case "1":
                        __result.Y -= 24;
                        break;
                    case "2":
                        __result.Y -= 20;
                        break;
                    case "3":
                        __result.Y -= 28;
                        break;
                }
            }
        }
        
        [HarmonyPatch(typeof(Farmer), nameof(Farmer.draw), new Type[] {typeof(SpriteBatch) })]
        public class Farmer_draw_Patch
        {
            public static void Prefix(Farmer __instance, SpriteBatch b)
            {
                if (!Config.ModEnabled || Game1.eventUp || !__instance.modData.ContainsKey(skateboardingKey))
                    return;
                
                Vector2 offset = Vector2.Zero;
                string sourceString;
                if (!__instance.modData.TryGetValue(sourceKey, out sourceString))
                {
                    sourceString = "0";
                    __instance.modData[sourceKey] = sourceString;
                }

                var source = int.Parse(sourceString);
                if (__instance.IsLocalPlayer)
                {
                    if (speed == Vector2.Zero)
                    {

                    }
                    else if (((speed.X > 0 && speed.Y > 0) || (speed.X < 0 && speed.Y < 0)) & Math.Abs(Math.Abs(speed.X) - Math.Abs(speed.Y)) < Math.Max(Math.Abs(speed.X), Math.Abs(speed.Y)) / 2)
                    {
                        source = 1;
                    }
                    else if (((speed.X > 0 && speed.Y < 0) || (speed.X > 0 && speed.Y < 0)) & Math.Abs(Math.Abs(speed.X) - Math.Abs(speed.Y)) < Math.Max(Math.Abs(speed.X), Math.Abs(speed.Y)) / 2)
                    {
                        source = 0;
                    }
                    else if (Math.Abs(speed.Y) > Math.Abs(speed.X))
                    {
                        source = 3;
                    }
                    else
                    {
                        source = 2;
                    }
                    __instance.modData[sourceKey] = source + "";
                }

                __instance.drawOffset.Value = new Vector2(0, -24);

                switch (source)
                {
                    case 0:
                        offset = new Vector2(12, 36);
                        break;
                    case 1:
                        offset = new Vector2(8, 36);
                        break;
                    case 2:
                        offset = new Vector2(8, 36);
                        break;
                    case 3:
                        offset = new Vector2(8, 40);
                        break;

                }
                b.Draw(boardTexture, Game1.GlobalToLocal(__instance.Position) - offset, new Rectangle(source * 16, 0, 16, 16), Color.White, 0, Vector2.Zero, 5, SpriteEffects.None, __instance.getDrawLayer() - 0.00001f);
            }
        }

        [HarmonyPatch(typeof(Farmer), nameof(Farmer.nextPosition))]
        public class Farmer_nextPosition_Patch
        {
            public static void Prefix(Farmer __instance)
            {
                checkingPos = true;
            }
            public static void Postfix(Farmer __instance)
            {
                checkingPos = false;
            }
        }
        
        [HarmonyPatch(typeof(Farmer), nameof(Farmer.nextPositionHalf))]
        public class Farmer_nextPositionHalf_Patch
        {
            public static void Prefix(Farmer __instance)
            {
                checkingPos = true;
            }
            public static void Postfix(Farmer __instance)
            {
                checkingPos = false;
            }
        }

        [HarmonyPatch(typeof(GameLocation), nameof(GameLocation.isCollidingPosition), new Type[] { typeof(Rectangle), typeof(xTile.Dimensions.Rectangle), typeof(bool), typeof(int), typeof(bool), typeof(Character) })]
        public class GameLocation_isCollidingPosition_Patch
        {
            public static void Postfix(Farmer __instance, Rectangle position, ref bool __result, Character character)
            {
                if (!Config.ModEnabled || Game1.eventUp || !__result || !Game1.player.modData.ContainsKey(skateboardingKey) || character != Game1.player)
                    return;

                if(character.yJumpOffset > 0)
                {
                    __result = false;
                    return;
                }
                if (speed.Length() <= lastSpeed.Length() - 0.05f * Config.Deceleration)
                {
                    speed = Vector2.Zero;
                }
            }
        }

        [HarmonyPatch(typeof(Farmer), nameof(Farmer.getMovementSpeed))]
        public class Farmer_getMovementSpeed_Patch
        {
            public static void Postfix(Farmer __instance, ref float __result)
            {
                if (checkingPos || !Config.ModEnabled || Game1.eventUp || !__instance.IsLocalPlayer || !__instance.modData.ContainsKey(skateboardingKey))
                    return;

                __result = speed.Length();
            }
        }
        [HarmonyPatch(typeof(Farmer), nameof(Farmer.MovePosition))]
        public class Farmer_MovePosition_Patch
        {
            public static void Prefix(Farmer __instance, ref FarmerState __state)
            {
                if (!Config.ModEnabled || !Game1.player.modData.ContainsKey(skateboardingKey) || Game1.eventUp || !__instance.IsLocalPlayer)
                    return;

                lastSpeed = speed;
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
                accelerating = __instance.movementDirections.Count > 0;

                if (speed.Length() > 0.05f * Config.Deceleration)
                {
                    Vector2 s = speed;
                    s.Normalize();
                    speed -= (s * 0.05f * Config.Deceleration);
                }
                else
                {
                    speed = Vector2.Zero;
                }
                if (accelerating)
                {
                    float mult = 0.1f * Config.Acceleration;
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
                    if(speed.X > 0)
                        speed.X = Math.Min(Config.MaxSpeed, speed.X);
                    else
                        speed.X = Math.Max(-Config.MaxSpeed, speed.X);
                    if(speed.Y > 0)
                        speed.Y = Math.Min(Config.MaxSpeed, speed.Y);
                    else
                        speed.Y = Math.Max(-Config.MaxSpeed, speed.Y);
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
            public static void Postfix(Farmer __instance, FarmerState __state, xTile.Dimensions.Rectangle viewport, GameLocation currentLocation)
            {
                if (!Config.ModEnabled || Game1.eventUp|| !Game1.player.modData.ContainsKey(skateboardingKey) || !__instance.IsLocalPlayer)
                    return;
                lastPos = __state.pos;
                var diff = __instance.position.Value - __state.pos;
                if (speed != Vector2.Zero && __instance.movementDirections.Count == 0 && __state.dirs.Count == 0 && diff.Length() == 0)
                {
                    speed = Vector2.Zero;
                }
                __instance.movementDirections = __state.dirs;
                if (diff.Length() > 64 || diff == Vector2.Zero || speed == Vector2.Zero)
                    return;
                var newDiff = speed;
                newDiff.Normalize();
                __instance.Position = __state.pos + newDiff * diff.Length();
                if (currentLocation.isCollidingPosition(__instance.GetBoundingBox(), viewport, true, 0, false, __instance))
                {
                    speed = Vector2.Zero;
                    __instance.Position = __state.pos;
                }

            }
        }

        [HarmonyPatch(typeof(CraftingPage), new Type[] { typeof(int), typeof(int), typeof(int), typeof(int), typeof(bool), typeof(bool), typeof(List<Chest>) })]
        [HarmonyPatch(MethodType.Constructor)]
        public class CraftingPage_Patch
        {
            public static void Prefix()
            {
                if (!Config.ModEnabled || Game1.player.craftingRecipes.ContainsKey("Skateboard"))
                    return;
                Game1.player.craftingRecipes.Add("Skateboard", 0);
            }
        }
        [HarmonyPatch(typeof(CraftingPage), "layoutRecipes")]
        public class CraftingPage_layoutRecipes_Patch
        {
            public static void Postfix(CraftingPage __instance, bool ___cooking)
            {
                if (!Config.ModEnabled || ___cooking || __instance.pagesOfCraftingRecipes.Count == 0)
                    return;
                foreach(var key in __instance.pagesOfCraftingRecipes[__instance.pagesOfCraftingRecipes.Count - 1].Keys.ToList())
                {
                    if(__instance.pagesOfCraftingRecipes[__instance.pagesOfCraftingRecipes.Count - 1][key].name == "Skateboard")
                    {
                        var cc = key;
                        var recipe = __instance.pagesOfCraftingRecipes[__instance.pagesOfCraftingRecipes.Count - 1][key];
                        cc.texture = boardTexture;
                        cc.sourceRect = new Rectangle(0, 0, 16, 16);
                        __instance.pagesOfCraftingRecipes[__instance.pagesOfCraftingRecipes.Count - 1].Remove(key);
                        __instance.pagesOfCraftingRecipes[__instance.pagesOfCraftingRecipes.Count - 1][cc] = recipe;
                    }
                }
            }
        }
        [HarmonyPatch(typeof(CraftingRecipe), nameof(CraftingRecipe.createItem))]
        public class CraftingRecipe_createItem_Patch
        {
            public static void Postfix(CraftingRecipe __instance, ref Item __result)
            {
                if (!Config.ModEnabled || __instance.name != "Skateboard")
                    return;
                __result.modData[boardKey] = "true";
                (__result as Object).Type = "Skateboard";
                (__result as Object).Category = -20;
            }
        }
        [HarmonyPatch(typeof(Object), nameof(Object.isPlaceable))]
        public class Object_isPlaceable_Patch
        {
            public static bool Prefix(Object __instance)
            {
                var result = (!Config.ModEnabled || !__instance.modData.ContainsKey(boardKey));
                return result;
            }
        }
        [HarmonyPatch(typeof(Object), nameof(Object.drawPlacementBounds))]
        public class Object_drawPlacementBounds_Patch
        {
            public static bool Prefix(Object __instance)
            {
                var result = (!Config.ModEnabled || !__instance.modData.ContainsKey(boardKey));
                return result;
            }
        }
        [HarmonyPatch(typeof(Object), nameof(Object.placementAction))]
        public class Object_placementAction_Patch
        {
            public static bool Prefix(Object __instance)
            {
                return (!Config.ModEnabled || !__instance.modData.ContainsKey(boardKey));
            }
        }
        [HarmonyPatch(typeof(GameLocation), nameof(GameLocation.checkAction))]
        public class GameLocation_checkAction_Patch
        {
            public static bool Prefix(GameLocation __instance, Location tileLocation)
            {
                if (__instance.objects.TryGetValue(new Vector2(tileLocation.X, tileLocation.Y), out var obj) && obj.modData.ContainsKey(boardKey))
                {
                    __instance.objects.Remove(new Vector2(tileLocation.X, tileLocation.Y));
                    return false;
                }
                return true;
            }
        }
        [HarmonyPatch(typeof(Object), nameof(Object.drawWhenHeld))]
        public class Object_drawWhenHeld_Patch
        {
            public static bool Prefix(Object __instance, SpriteBatch spriteBatch, Vector2 objectPosition, Farmer f)
            {
                if (!Config.ModEnabled || !__instance.modData.ContainsKey(boardKey))
                    return true;
                spriteBatch.Draw(boardTexture, objectPosition + new Vector2(0, 92), new Rectangle(32, 0, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, Math.Max(0f, (f.getStandingY() + 3) / 10000f));
                return false;
            }
        }
        [HarmonyPatch(typeof(Object), nameof(Object.drawInMenu))]
        public class Object_drawInMenu_Patch
        {
            public static bool Prefix(Object __instance, SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow)
            {
                if (!Config.ModEnabled || boardTexture is null || !__instance.modData.ContainsKey(boardKey))
                    return true;
                spriteBatch.Draw(boardTexture, location + new Vector2(32f, 64f), new Rectangle(0,0,16,16), color * transparency, 0f, new Vector2(8f, 16f), 8f * (((double)scaleSize < 0.2) ? scaleSize : (scaleSize / 2f)), SpriteEffects.None, layerDepth);
                return false;
            }
        }
        [HarmonyPatch(typeof(Object), nameof(Object.draw), new Type[] { typeof(SpriteBatch), typeof(int), typeof(int), typeof(float) })]
        public class Object_draw_Patch_1
        {
            public static bool Prefix(Object __instance, SpriteBatch spriteBatch, int x, int y, float alpha)
            {
                if (!Config.ModEnabled || !__instance.modData.ContainsKey(boardKey))
                    return true;
                Vector2 scaleFactor = __instance.getScale();
                scaleFactor *= 16f;
                Vector2 position = Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(x * 64), (float)(y * 64 - 64)));
                Rectangle destination = new Rectangle((int)(position.X - scaleFactor.X / 2f) + ((__instance.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), (int)(position.Y - scaleFactor.Y / 2f) + ((__instance.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), (int)(64f + scaleFactor.X), (int)(128f + scaleFactor.Y / 2f));
                float draw_layer = Math.Max(0f, (float)((y + 1) * 64 - 24) / 10000f) + (float)x * 1E-05f;
                spriteBatch.Draw(boardTexture, destination, new Rectangle(0,0,16,16), Color.White * alpha, 0f, Vector2.Zero, SpriteEffects.None, draw_layer);
                return false;
            }
        }
        [HarmonyPatch(typeof(Object), nameof(Object.draw), new Type[] { typeof(SpriteBatch), typeof(int), typeof(int), typeof(float), typeof(float) })]
        public class Object_draw_Patch_2
        {
            public static bool Prefix(Object __instance, SpriteBatch spriteBatch, int xNonTile, int yNonTile, float layerDepth, float alpha)
            {
                if (!Config.ModEnabled || !__instance.modData.ContainsKey(boardKey))
                    return true;
                Vector2 scaleFactor = __instance.getScale();
                scaleFactor *= 4f;
                Vector2 position = Game1.GlobalToLocal(Game1.viewport, new Vector2((float)xNonTile, (float)yNonTile));
                Microsoft.Xna.Framework.Rectangle destination = new Microsoft.Xna.Framework.Rectangle((int)(position.X - scaleFactor.X / 2f) + ((__instance.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), (int)(position.Y - scaleFactor.Y / 2f) + ((__instance.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), (int)(64f + scaleFactor.X), (int)(128f + scaleFactor.Y / 2f));
                spriteBatch.Draw(Game1.bigCraftableSpriteSheet, destination, new Rectangle(0,0,16,16), Color.White * alpha, 0f, Vector2.Zero, SpriteEffects.None, layerDepth);
                return false;
            }
        }
    }
}