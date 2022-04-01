
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Netcode;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using xTile.Dimensions;
using Object = StardewValley.Object;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace NPCConversations
{
    public partial class ModEntry
    {
        [HarmonyPatch(typeof(TerrainFeature), nameof(TerrainFeature.tickUpdate))]
        public class TerrainFeature_tickUpdate_Patch
        {
            public static void Postfix(TerrainFeature __instance, GameTime time, Vector2 tileLocation, GameLocation location)
            {
                if (!Config.EnableMod || currentConversations.Count == 0)
                    return;

                for (int i = currentConversations.Count - 1; i >= 0; i--)
                {
                    if (currentConversations[i].Participant == __instance)
                    {
                        var d = currentConversations[i].Dialogue;
                        if (d.textAboveHeadPreTimer > 0)
                        {
                            d.textAboveHeadPreTimer -= time.ElapsedGameTime.Milliseconds;
                        }
                        else
                        {
                            d.textAboveHeadTimer -= time.ElapsedGameTime.Milliseconds;
                            if (d.textAboveHeadTimer > 500)
                            {
                                d.textAboveHeadAlpha = Math.Min(1f, d.textAboveHeadAlpha + 0.1f);
                            }
                            else
                            {
                                d.textAboveHeadAlpha = Math.Max(0f, d.textAboveHeadAlpha - 0.04f);
                            }
                        }
                        if (d.textAboveHeadTimer < 0)
                        {
                            currentConversations[i].index++;
                            if (currentConversations[i].index >= currentConversations.Count)
                                currentConversations.RemoveAt(i);
                        }
                        currentConversations[i].Dialogue = d;
                        return;
                    }
                }
            }
        }

        //[HarmonyPatch(typeof(Tree), nameof(Tree.draw), new Type[] { typeof(SpriteBatch), typeof(Vector2) })]
        public class Tree_draw_Patch
        {
            public static void Postfix(Tree __instance, SpriteBatch spriteBatch, Vector2 tileLocation, NetBool ___falling)
            {
                Vector2 local = Game1.GlobalToLocal(tileLocation * 64 + new Vector2(32, ___falling.Value || __instance.stump.Value ? -128 : -192));
                //SpriteText.drawStringWithScrollCenteredAt(spriteBatch, speech, (int)local.X, (int)local.Y, "", alpha, -1, 1, 1, false);
            }
        }
        [HarmonyPatch(typeof(Character), nameof(Character.update), new Type[] { typeof(GameTime), typeof(GameLocation), typeof(long), typeof(bool) })]
        public static class Character_update_Patch
        {
            public static void Postfix(Character __instance, GameTime time, GameLocation location)
            {
                if (!Config.EnableMod || currentConversations.Count == 0 || __instance is NPC || location != Game1.currentLocation)
                    return;
                for(int i = currentConversations.Count - 1; i >= 0; i--)
                {
                    if(currentConversations[i].Participant == __instance)
                    {
                        var d = currentConversations[i].Dialogue;
                        if (d.textAboveHeadPreTimer > 0)
                        {
                            d.textAboveHeadPreTimer -= time.ElapsedGameTime.Milliseconds;
                        }
                        else
                        {
                            d.textAboveHeadTimer -= time.ElapsedGameTime.Milliseconds;
                            if (d.textAboveHeadTimer > 500)
                            {
                                d.textAboveHeadAlpha = Math.Min(1f, d.textAboveHeadAlpha + 0.1f);
                            }
                            else
                            {
                                d.textAboveHeadAlpha = Math.Max(0f, d.textAboveHeadAlpha - 0.04f);
                            }
                        }
                        if (d.textAboveHeadTimer < 0)
                        {
                            currentConversations[i].index++;
                            if (currentConversations[i].index >= currentConversations.Count)
                            {
                                currentConversations.RemoveAt(i);
                                return;
                            }
                        }
                        currentConversations[i].Dialogue = d;
                        return;
                    }
                }
            }
        }
        [HarmonyPatch(typeof(GameLocation), nameof(GameLocation.drawAboveAlwaysFrontLayer), new Type[] { typeof(SpriteBatch) })]
        public static class GameLocation_drawAboveAlwaysFrontLayer_Patch
        {
            public static void Postfix(GameLocation __instance, SpriteBatch b)
            {
                if (!Config.EnableMod || currentConversations.Count == 0 || (Game1.currentLocation is not Farm && Game1.currentLocation is not AnimalHouse && Game1.currentLocation is not Forest))
                    return;

                foreach(var c in currentConversations)
                {
                    if(c.Participant is FarmAnimal)
                    {
                        FarmAnimal[] animals;
                        if (Game1.currentLocation is AnimalHouse)
                            animals = (Game1.currentLocation as AnimalHouse).animals.Values.ToArray();
                        else if (Game1.currentLocation is Farm)
                            animals = (Game1.currentLocation as Farm).animals.Values.ToArray();
                        else if (Game1.currentLocation is Forest)
                            animals = (Game1.currentLocation as Forest).marniesLivestock.ToArray();
                        else return;
                        foreach (var a in animals)
                        {
                            if (c.Participant == a)
                            {
                                if (c.Dialogue.textAboveHeadTimer > 0 && c.Dialogue.data.text != null)
                                {
                                    Vector2 local = Game1.GlobalToLocal(new Vector2(a.getStandingX(), a.getStandingY() - a.Sprite.SpriteHeight * 4 - 64 + a.yJumpOffset));

                                    if (a.shouldShadowBeOffset)
                                    {
                                        local += a.drawOffset.Value;
                                    }
                                    SpriteText.drawStringWithScrollCenteredAt(b, c.Dialogue.data.text, (int)local.X, (int)local.Y, "", c.Dialogue.textAboveHeadAlpha, c.Dialogue.data.color, 1, a.getTileY() * 64 / 10000f + 0.001f + a.getTileX() / 10000f, false);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}