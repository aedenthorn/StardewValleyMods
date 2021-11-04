using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Network;
using System;
using System.Collections.Generic;

namespace HugsAndKisses
{
    public static class NPCPatches
    {
        private static IMonitor Monitor;
        private static ModConfig Config;

        // call this method from your Entry class
        public static void Initialize(IMonitor monitor, ModConfig config, IModHelper helper)
        {
            Monitor = monitor;
            Config = config;
        }

        public static bool NPC_checkAction_Prefix(ref NPC __instance, ref Farmer who, ref bool __result)
        {
            try
            {
                Misc.ResetSpouses(who);

                if ((__instance.Name.Equals(who.spouse) || Misc.GetSpouses(who,1).ContainsKey(__instance.Name)) && who.IsLocalPlayer)
                {
                    int timeOfDay = Game1.timeOfDay;
                    if (__instance.Sprite.CurrentAnimation == null)
                    {
                        __instance.faceDirection(-3);
                    }
                    if (__instance.Sprite.CurrentAnimation == null && who.friendshipData.ContainsKey(__instance.Name) && who.friendshipData[__instance.Name].Points >= 3125 && !who.mailReceived.Contains("CF_Spouse"))
                    {
                        __instance.CurrentDialogue.Push(new Dialogue(Game1.content.LoadString(Game1.player.isRoommate(who.spouse) ? "Strings\\StringsFromCSFiles:Krobus_Stardrop" : "Strings\\StringsFromCSFiles:NPC.cs.4001"), __instance));
                        Game1.player.addItemByMenuIfNecessary(new StardewValley.Object(Vector2.Zero, 434, "Cosmic Fruit", false, false, false, false), null);
                        __instance.shouldSayMarriageDialogue.Value = false;
                        __instance.currentMarriageDialogue.Clear();
                        who.mailReceived.Add("CF_Spouse");
                        __result = true;
                        return false;
                    }
                    if (__instance.Sprite.CurrentAnimation == null && !__instance.hasTemporaryMessageAvailable() && __instance.currentMarriageDialogue.Count == 0 && __instance.CurrentDialogue.Count == 0 && Game1.timeOfDay < 2200 && !__instance.isMoving() && who.ActiveObject == null && (!__instance.hasBeenKissedToday.Value || Config.UnlimitedDailyKisses))
                    {
                        __instance.faceGeneralDirection(who.getStandingPosition(), 0, false);
                        who.faceGeneralDirection(__instance.getStandingPosition(), 0, false);
                        if (__instance.FacingDirection == 3 || __instance.FacingDirection == 1)
                        {
                            int spouseFrame = 28;
                            bool facingRight = true;
                            string name = __instance.Name;
                            if (name == "Sam")
                            {
                                spouseFrame = 36;
                                facingRight = true;
                            }
                            else if (name == "Penny")
                            {
                                spouseFrame = 35;
                                facingRight = true;
                            }
                            else if (name == "Sebastian")
                            {
                                spouseFrame = 40;
                                facingRight = false;
                            }
                            else if (name == "Alex")
                            {
                                spouseFrame = 42;
                                facingRight = true;
                            }
                            else if (name == "Krobus")
                            {
                                spouseFrame = 16;
                                facingRight = true;
                            }
                            else if (name == "Maru")
                            {
                                spouseFrame = 28;
                                facingRight = false;
                            }
                            else if (name == "Emily")
                            {
                                spouseFrame = 33;
                                facingRight = false;
                            }
                            else if (name == "Harvey")
                            {
                                spouseFrame = 31;
                                facingRight = false;
                            }
                            else if (name == "Shane")
                            {
                                spouseFrame = 34;
                                facingRight = false;
                            }
                            else if (name == "Elliott")
                            {
                                spouseFrame = 35;
                                facingRight = false;
                            }
                            else if (name == "Leah")
                            {
                                spouseFrame = 25;
                                facingRight = true;
                            }
                            else if (name == "Abigail")
                            {
                                spouseFrame = 33;
                                facingRight = false;
                            }
                            bool flip = (facingRight && __instance.FacingDirection == 3) || (!facingRight && __instance.FacingDirection == 1);
                            if (who.getFriendshipHeartLevelForNPC(__instance.Name) >= Config.MinHeartsForKiss)
                            {
                                int delay = Game1.IsMultiplayer ? 1000 : 10;
                                __instance.movementPause = delay;
                                __instance.Sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
                                {
                                    new FarmerSprite.AnimationFrame(spouseFrame, delay, false, flip, new AnimatedSprite.endOfAnimationBehavior(__instance.haltMe), true)
                                });
                                if (!__instance.hasBeenKissedToday.Value)
                                {
                                    who.changeFriendship(10, __instance);
                                }

                                if (!Config.RoommateKisses && who.friendshipData[__instance.Name].RoommateMarriage)
                                {
                                    ModEntry.mp.broadcastSprites(who.currentLocation, new TemporaryAnimatedSprite[]
                                    {
                                        new TemporaryAnimatedSprite("LooseSprites\\emojis", new Microsoft.Xna.Framework.Rectangle(0, 0, 9, 9), 2000f, 1, 0, new Vector2((float)__instance.getTileX(), (float)__instance.getTileY()) * 64f + new Vector2(16f, -64f), false, false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f, false)
                                        {
                                            motion = new Vector2(0f, -0.5f),
                                            alphaFade = 0.01f
                                        }
                                    });
                                }
                                else
                                {
                                    ModEntry.mp.broadcastSprites(who.currentLocation, new TemporaryAnimatedSprite[]
                                    {
                                        new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(211, 428, 7, 6), 2000f, 1, 0, new Vector2((float)__instance.getTileX(), (float)__instance.getTileY()) * 64f + new Vector2(16f, -64f), false, false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f, false)
                                        {
                                            motion = new Vector2(0f, -0.5f),
                                            alphaFade = 0.01f
                                        }
                                    });
                                }
                                if (Config.CustomKissSound.Length > 0 && Kissing.kissEffect != null && (Config.RoommateKisses || !who.friendshipData[__instance.Name].RoommateMarriage))
                                {
                                    Kissing.kissEffect.Play();
                                }
                                else
                                {
                                    __instance.currentLocation.playSound("dwop", NetAudio.SoundContext.NPC);
                                }
                                who.exhausted.Value = false;
                                __instance.hasBeenKissedToday.Value = true;
                                __instance.Sprite.UpdateSourceRect();
                            }
                            else
                            {
                                __instance.faceDirection((ModEntry.myRand.NextDouble() < 0.5) ? 2 : 0);
                                __instance.doEmote(12, true);
                            }
                            int playerFaceDirection = 1;
                            if ((facingRight && !flip) || (!facingRight && flip))
                            {
                                playerFaceDirection = 3;
                            }
                            who.PerformKiss(playerFaceDirection);
                            who.CanMove = false;
                            who.FarmerSprite.PauseForSingleAnimation = false;
                            who.FarmerSprite.animateOnce(new List<FarmerSprite.AnimationFrame>
                            {
                                new FarmerSprite.AnimationFrame(101, 1000, 0, false, who.FacingDirection == 3, null, false, 0),
                                new FarmerSprite.AnimationFrame(6, 1, false, who.FacingDirection == 3, new AnimatedSprite.endOfAnimationBehavior(Farmer.completelyStopAnimating), false)
                            }.ToArray(), null);
                            __result = true;
                            return false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(NPC_checkAction_Prefix)}:\n{ex}", LogLevel.Error);
            }
            return true;
        }

    }
}
