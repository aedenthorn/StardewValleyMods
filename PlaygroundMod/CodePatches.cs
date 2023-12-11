using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace PlaygroundMod
{
    public partial class ModEntry
    {
        public static float climbSpeed = 4;
        public static bool skip;
        public static bool swingSkip;
        [HarmonyPatch(typeof(FarmerRenderer), nameof(FarmerRenderer.drawHairAndAccesories))]
        public class FarmerRenderer_drawHairAndAccesories_Patch
        {
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                SMonitor.Log($"Transpiling FarmerRenderer.drawHairAndAccesories");
                var codes = new List<CodeInstruction>(instructions);
                CodeInstruction texture = null;
                for (int i = 0; i < codes.Count; i++)
                {
                    if (i > 0 && i < codes.Count -1 && codes[i].opcode == OpCodes.Ldarg_S && (byte)codes[i].operand == 4 && codes[i + 1].opcode == OpCodes.Ldarg_S && (byte)codes[i + 1].operand == 5)
                    {
                        texture = codes[i - 1].Clone();
                        
                        SMonitor.Log("replacing added origin");
                        codes.Insert(i + 2, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ModEntry.ChangeAddedOrigin))));
                        codes.Insert(i + 1, texture);
                        codes.Insert(i + 1, new CodeInstruction(OpCodes.Ldarg_3));
                        i += 3;
                    }
                    else if (i < codes.Count -1 && codes[i].opcode == OpCodes.Ldarg_S && (byte)codes[i].operand == 8 && codes[i + 1].opcode == OpCodes.Ldarg_S && (byte)codes[i + 1].operand == 5)
                    {
                        SMonitor.Log("replacing actual origin");
                        codes.Insert(i + 2, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ModEntry.ChangeActualOrigin))));
                        codes.Insert(i + 1, texture);
                        codes.Insert(i + 1, new CodeInstruction(OpCodes.Ldarg_3));
                        i += 3;
                    }
                }

                return codes.AsEnumerable();
            }
        }

        private static Vector2 ChangeActualOrigin(Farmer farmer, Texture2D texture, Vector2 origin)
        {
            if (!Config.ModEnabled)
                return origin;
            if (springTicks.ContainsKey(farmer.UniqueMultiplayerID))
            {
                if(texture == FarmerRenderer.shirtsTexture)
                {
                    origin = new Vector2(2f, 19f);
                }
                else if (texture == FarmerRenderer.accessoriesTexture)
                {
                    origin = new Vector2(6f,32f);
                }
                else if (texture == FarmerRenderer.hatsTexture)
                {
                    origin = new Vector2(8, 35);
                }
                else if (texture == FarmerRenderer.hairStylesTexture)
                {
                    origin = new Vector2(6, 32.5f);

                }
            }
            else if (swingTicks.ContainsKey(farmer.UniqueMultiplayerID))
            {
                if (texture == FarmerRenderer.shirtsTexture)
                {
                    origin = new Vector2(origin.X * 0.25f, origin.Y );
                }
            }
            return origin;
        }

        private static Vector2 ChangeAddedOrigin(Farmer farmer, Texture2D texture, Vector2 origin)
        {
            if (!Config.ModEnabled)
                return origin;
            if (springTicks.ContainsKey(farmer.UniqueMultiplayerID))
            {
                if (texture == FarmerRenderer.shirtsTexture)
                {
                    origin = new Vector2(-12f, -16f);
                }
                else if (texture == FarmerRenderer.accessoriesTexture)
                {
                    origin = new Vector2(3f, 38);
                }
                else if (texture == FarmerRenderer.hatsTexture)
                { 
                    origin += new Vector2(6, 20);

                }
                else if (texture == FarmerRenderer.hairStylesTexture)
                {
                    origin = new Vector2(4, 40);

                }

            }
            else if (swingTicks.ContainsKey(farmer.UniqueMultiplayerID))
            {
                if (texture == FarmerRenderer.shirtsTexture)
                {
                    origin = new Vector2(0, origin.Y * 0.70f + 5);
                }
            }
            return origin;
        }

        [HarmonyPatch(typeof(FarmerRenderer), nameof(FarmerRenderer.draw), new Type[] { typeof(SpriteBatch), typeof(FarmerSprite.AnimationFrame), typeof(int), typeof(Rectangle), typeof(Vector2), typeof(Vector2), typeof(float), typeof(int), typeof(Color), typeof(float), typeof(float), typeof(Farmer) })]
        public class FarmerRenderer_draw_Patch
        {
            public static bool Prefix(FarmerRenderer __instance, SpriteBatch b, ref float scale, ref Farmer who, ref Vector2 origin, ref FarmerSprite.AnimationFrame animationFrame, ref int currentFrame, ref Rectangle sourceRect, ref float rotation)
            {
                if (!Config.ModEnabled || who.currentLocation is not Town || (!Config.Festivals && Game1.isFestival()))
                {
                    springTicks.Remove(who.UniqueMultiplayerID);
                    swingTicks.Remove(who.UniqueMultiplayerID);
                    climbTicks.Remove(who.UniqueMultiplayerID);
                    slideTicks.Remove(who.UniqueMultiplayerID);
                    return true;
                }
                if (skip || !Context.IsPlayerFree)
                    return true;

                var n = who.TilePoint;
                int ticks;
                if (who.IsSitting() && (n == new Point(15, 12) || n == new Point(17, 12)))
                {
                    if (!swingTicks.TryGetValue(who.UniqueMultiplayerID, out ticks))
                        ticks = 0;
                    float factor = (float)Math.Sin((Math.PI / 180f) * ticks * Config.swingSpeed);
                    scale = 1 + factor * 0.1f;
                    if (factor > 0)
                        factor *= 0.5f;
                    origin += new Vector2(1.2f * (factor), -16 * (factor));
                    if (!swingSkip)
                    {
                        ticks++;
                        ticks %= 360;
                        swingTicks[who.UniqueMultiplayerID] = ticks;
                    }
                    if (ticks % 180 == 0)
                    {
                        string sound = factor > 0 ? Config.swingBackSound : Config.swingForthSound;
                        if (!string.IsNullOrEmpty(sound))
                            who.currentLocation.playSound(sound);
                    }
                }
                else if (who.IsSitting() && n == new Point(24, 11))
                {
                    if (!springTicks.TryGetValue(who.UniqueMultiplayerID, out ticks))
                    {
                        springTicks[who.UniqueMultiplayerID] = 1;
                    }
                    if (ticks % 180 == 30)
                    {
                        if (!string.IsNullOrEmpty(Config.springSound))
                            who.currentLocation.playSound(Config.springSound);
                    }
                    float factor = (float)Math.Sin((Math.PI / 180f) * ticks * Config.springSpeed) / 4f;
                    b.Draw(springTexture, Game1.GlobalToLocal(new Vector2(23 * 64, 10 * 64)), new Rectangle(Game1.currentSeason == "winter" ? 144 : 0, 0, 48, 32), Color.White, 0, Vector2.Zero, 4, SpriteEffects.None, who.getDrawLayer() + 0.0000002f);
                    b.Draw(springTexture, Game1.GlobalToLocal(new Vector2(23 * 64, 10 * 64)) + new Vector2(16, 28) * 4, new Rectangle(48, 0, 48, 32), Color.White, factor, new Vector2(16, 28), 4, SpriteEffects.None, who.getDrawLayer() + 0.0000004f);
                    b.Draw(springTexture, Game1.GlobalToLocal(new Vector2(23 * 64, 10 * 64)) + new Vector2(16, 24) * 4, new Rectangle(96, 0, 48, 32), Color.White, factor * 2, new Vector2(16, 24), 4, SpriteEffects.None, who.getDrawLayer() + 0.0000005f);

                    ticks++;
                    ticks %= 360;
                    springTicks[who.UniqueMultiplayerID] = ticks;
                    origin = new Vector2(7, 30);
                    animationFrame.xOffset += 2;
                    animationFrame.positionOffset += 2;
                    rotation = factor * 2;
                    springEyes[who.UniqueMultiplayerID] = who.currentEyes;
                    who.currentEyes = 0;
                }
                else if (slideTicks.TryGetValue(who.UniqueMultiplayerID, out ticks))
                {
                    float slideSpeed = Config.slideSpeed / 2f;
                    who.isSitting.Value = true;
                    who.sittingFurniture = new MySeat();
                    who.canMove = false;
                    who.FacingDirection = 1;
                    who.FarmerSprite.setCurrentSingleFrame(117, 32000, false, false);
                    animationFrame = who.FarmerSprite.CurrentAnimationFrame;
                    sourceRect = who.FarmerSprite.sourceRect;
                    currentFrame = 117;

                    if (ticks > 40 * 4)
                    {
                        if(ticks < 170)
                        {
                            who.currentLocation.playSound(Config.slideSound);
                            slideTicks[who.UniqueMultiplayerID] = 170;
                        }

                        var dest = new Vector2(25 * 64, 13 * 64 + 8);
                        who.isSitting.Value = false;
                        who.sittingFurniture = null;
                        who.FarmerSprite.setCurrentSingleFrame(6, 32000, false, false);
                        animationFrame = who.FarmerSprite.CurrentAnimationFrame;
                        sourceRect = who.FarmerSprite.sourceRect;
                        if (who == Game1.player)
                        {

                            if (Vector2.Distance(who.Position, dest) > 2)
                            {
                                who.Position = Vector2.Lerp(who.Position, dest, 0.2f);
                            }
                            else
                            {
                                who.Position = dest;
                                slideTicks.Remove(who.UniqueMultiplayerID);
                                who.canMove = true;
                            }
                        }
                    }
                    else
                    {
                        if (who == Game1.player)
                        {
                            who.Position = new Vector2(20 * 64 + 32, 10 * 64 + 56) + new Vector2(ticks, ticks);
                            ticks += (int)Math.Round(slideSpeed + ticks / 25f);
                            slideTicks[who.UniqueMultiplayerID] = ticks;
                        }
                    }
                    return false;
                }
                else if (climbTicks.TryGetValue(who.UniqueMultiplayerID, out ticks))
                {
                    who.canMove = false;
                    who.FacingDirection = 0;
                    who.FarmerSprite.setCurrentSingleFrame(14 + (ticks % 32 < 16 ? 1 : 0), 32000, false, ticks % 64 < 32);
                    animationFrame = who.FarmerSprite.CurrentAnimationFrame;
                    sourceRect = who.FarmerSprite.sourceRect;
                    if (who == Game1.player)
                    {
                        climbSpeed = 2 / Config.climbSpeed;
                        who.Position = new Vector2(who.Position.X, 14 * 64 - ticks / climbSpeed * 4);
                        ticks++;
                        if (ticks % 24 == 0)
                        {
                            if (!string.IsNullOrEmpty(Config.climbSound))
                                who.currentLocation.playSound(Config.climbSound);
                        }
                        if (ticks > 64 * climbSpeed)
                        {
                            who.canMove = true;
                            climbTicks.Remove(who.UniqueMultiplayerID);
                            slideTicks[who.UniqueMultiplayerID] = 1;
                        }
                        else
                        {
                            climbTicks[who.UniqueMultiplayerID] = ticks;
                        }
                    }
                    return false;
                }
                else if ((n.Y == 14 && (n.X == 19 || n.X == 20)) && who.movementDirections.Contains(0) && who.Position.Y % 64 < 8)
                {
                    climbTicks[who.UniqueMultiplayerID] = 1;
                }
                else
                {
                    springTicks.Remove(who.UniqueMultiplayerID);
                    swingTicks.Remove(who.UniqueMultiplayerID);
                    climbTicks.Remove(who.UniqueMultiplayerID);
                    slideTicks.Remove(who.UniqueMultiplayerID);
                }
                return true;
            }
        }
        [HarmonyPatch(typeof(GameLocation), nameof(GameLocation.drawAboveAlwaysFrontLayer))]
        public class GameLocation_drawAboveAlwaysFrontLayer_Patch
        {
            public static void Postfix(GameLocation __instance, SpriteBatch b)
            {
                if (!Config.ModEnabled || __instance is not Town || (!Config.Festivals && Game1.isFestival()))
                    return;
                skip = true;
                foreach (long id in climbTicks.Keys)
                {
                    var f = Game1.getFarmer(id);
                    if(f is not null && f.currentLocation == __instance)
                    {
                        f.draw(b);
                    }
                }
                foreach (long id in swingTicks.Keys)
                {
                    var f = Game1.getFarmer(id);
                    if(f is not null && f.currentLocation == __instance)
                    {
                        var ticks = swingTicks[id];
                        float factor = (float)Math.Sin((Math.PI / 180f) * ticks * Config.swingSpeed);
                        var scale = 1 + factor * 0.1f;
                        if (factor > 0)
                            factor *= 0.5f;
                        int offset = (f.TilePoint == new Point(17, 12)) ? 128 : 0;
                        b.Draw(swingTexture, Game1.GlobalToLocal(new Vector2(15 * 64 - 4 + offset, 10 * 64 - 8)), new Rectangle(Game1.currentSeason == "winter" ? 34 : 0, 5, 17, 61), Color.White, 0, Vector2.Zero, 4, SpriteEffects.None, f.getDrawLayer() + 0.0000001f);

                        var y = 10 * 64 + factor * 4 * 4 - 8;
                        var yo = (int)(Math.Max(0, (632 - y)) / 4f);

                        b.Draw(swingTexture, Game1.GlobalToLocal(new Vector2(15 * 64 - 4 - 4f * (factor + 0.2f) + offset, y + yo)), new Rectangle(17, 5 + yo, 17, 61 - yo), Color.White, 0, Vector2.Zero, 4 * scale, SpriteEffects.None, f.getDrawLayer() + 0.0000002f);
                        skip = false;
                        swingSkip = true;
                        f.draw(b);
                        swingSkip = false;
                        skip = true;
                    }
                }
                foreach(long id in slideTicks.Keys)
                {
                    var f = Game1.getFarmer(id);
                    if(f is not null && f.currentLocation == __instance)
                    {
                        f.draw(b);
                        b.Draw(slideTexture, Game1.GlobalToLocal(new Vector2(21 * 64, 11 * 64 - 12)), new Rectangle(Game1.currentSeason == "winter" ? 48 : 0, 0, 48, 48), Color.White, 0, Vector2.Zero, 4, SpriteEffects.None, f.getDrawLayer() + 0.0000002f);
                    }
                }
                skip = false;
            }
        }
        [HarmonyPatch(typeof(Town), "resetLocalState")]
        public class Town_resetLocalState_Patch
        {
            public static void Postfix(Town __instance)
            {
                if (!Config.ModEnabled)
                    return;
                __instance.setTileProperty(24, 13, "Buildings", "Passable", "T");

            }
        }
    }
}