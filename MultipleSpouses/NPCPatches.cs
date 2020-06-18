using Harmony;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Characters;
using StardewValley.Locations;
using StardewValley.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace MultipleSpouses
{
    public static class NPCPatches
    {
        private static IMonitor Monitor;

        // call this method from your Entry class
        public static void Initialize(IMonitor monitor)
        {
            Monitor = monitor;
        }

        public static string[] csMarriageDialoguesReplace = new string[]
        {
            "NPC.cs.4406",
            "NPC.cs.4431",
            "NPC.cs.4427",
            "NPC.cs.4429",
            "NPC.cs.4439",
            "NPC.cs.4442",
            "NPC.cs.4443",
            "NPC.cs.4446",
            "NPC.cs.4449",
            "NPC.cs.4452",
            "NPC.cs.4455",
            "NPC.cs.4462",
            "NPC.cs.4470",
            "NPC.cs.4474",
            "NPC.cs.4481",
            "NPC.cs.4488",
            "NPC.cs.4489",
            "NPC.cs.4490",
            "NPC.cs.4496",
            "NPC.cs.4497",
            "NPC.cs.4498",
            "NPC.cs.4499",
            "NPC.cs.4440",
            "NPC.cs.4441",
            "NPC.cs.4444",
            "NPC.cs.4445",
            "NPC.cs.4447",
            "NPC.cs.4448",
            "NPC.cs.4463",
            "NPC.cs.4465",
            "NPC.cs.4466",
            "NPC.cs.4486",
            "NPC.cs.4488",
            "NPC.cs.4489",
            "NPC.cs.4490",
            "NPC.cs.4496",
            "NPC.cs.4497",
            "NPC.cs.4498",
            "NPC.cs.4499",
            "NPC.cs.4500",
        };

        public static string[][] csMarriageDialoguesChoose = new string[][]
        {
            new string[]
            {
                "NPC.cs.4420",
                "NPC.cs.4421",
                "NPC.cs.4422",
                "NPC.cs.4423",
                "NPC.cs.4424",
                "NPC.cs.4425",
                "NPC.cs.4426",
                "NPC.cs.4432",
                "NPC.cs.4433",
            },
            new string[]
            {
                "NPC.cs.4434",
                "NPC.cs.4435",
                "NPC.cs.4436",
                "NPC.cs.4437",
                "NPC.cs.4438",
            },
        };

        public static string[][] marriageDialogues = new string[][]
        {
            new string[]{
                "Indoor_Day_0",
                "Indoor_Day_1",
                "Indoor_Day_2",
                "Indoor_Day_3",
                "Indoor_Day_4",
            },
            new string[]{
                "OneKid_0",
                "OneKid_1",
                "OneKid_2",
                "OneKid_3",
            },
            new string[]{
                "Outdoor_0",
                "Outdoor_1",
                "Outdoor_2",
                "Outdoor_3",
                "Outdoor_4",
            },
            new string[]{
                "Rainy_Day_0",
                "Rainy_Day_1",
                "Rainy_Day_2",
                "Rainy_Day_3",
                "Rainy_Day_4",
            },
            new string[]{
                "TwoKids_0",
                "TwoKids_1",
                "TwoKids_2",
                "TwoKids_3",
            },
            new string[]{
                "Good_0",
                "Good_1",
                "Good_2",
                "Good_3",
                "Good_4",
                "Good_5",
                "Good_6",
                "Good_7",
                "Good_8",
                "Good_9",
            },
            new string[]{
                "Neutral_0",
                "Neutral_1",
                "Neutral_2",
                "Neutral_3",
                "Neutral_4",
                "Neutral_5",
                "Neutral_6",
                "Neutral_7",
                "Neutral_8",
                "Neutral_9",
            },
            new string[]{
                "Bad_0",
                "Bad_1",
                "Bad_2",
                "Bad_3",
                "Bad_4",
                "Bad_5",
                "Bad_6",
                "Bad_7",
                "Bad_8",
                "Bad_9",
            },
        };

        public static bool NPC_setUpForOutdoorPatioActivity_Prefix(NPC __instance)
        {
            if (ModEntry.outdoorSpouse != __instance.Name)
            {
                return false;
            }
            ModEntry.PMonitor.Log("is outdoor spouse: " + __instance.Name);
            return true;
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
                    if (__instance.Sprite.CurrentAnimation == null && who.friendshipData.ContainsKey(__instance.name) && who.friendshipData[__instance.name].Points >= 3125 && !who.mailReceived.Contains("CF_Spouse"))
                    {
                        __instance.CurrentDialogue.Push(new Dialogue(Game1.content.LoadString(Game1.player.isRoommate(who.spouse) ? "Strings\\StringsFromCSFiles:Krobus_Stardrop" : "Strings\\StringsFromCSFiles:NPC.cs.4001"), __instance));
                        Game1.player.addItemByMenuIfNecessary(new StardewValley.Object(Vector2.Zero, 434, "Cosmic Fruit", false, false, false, false), null);
                        __instance.shouldSayMarriageDialogue.Value = false;
                        __instance.currentMarriageDialogue.Clear();
                        who.mailReceived.Add("CF_Spouse");
                        __result = true;
                        return false;
                    }
                    if (__instance.Sprite.CurrentAnimation == null && !__instance.hasTemporaryMessageAvailable() && __instance.currentMarriageDialogue.Count == 0 && __instance.CurrentDialogue.Count == 0 && Game1.timeOfDay < 2200 && !__instance.isMoving() && who.ActiveObject == null)
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
                            if (who.getFriendshipHeartLevelForNPC(__instance.Name) >= ModEntry.config.MinHeartsForKiss)
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

                                if (!ModEntry.config.RoommateRomance && who.friendshipData[__instance.Name].RoommateMarriage)
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
                                if (ModEntry.config.RealKissSound && Kissing.kissEffect != null)
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

        public static void NPC_marriageDuties_Prefix(NPC __instance)
        {
            try
            {
                if (!ModEntry.config.SpousesKeepOrdinaryDialogue)
                {
                    __instance.CurrentDialogue.Clear();
                }
            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(NPC_marriageDuties_Prefix)}:\n{ex}", LogLevel.Error);
            }
        }
        
        public static void NPC_marriageDuties_Postfix(NPC __instance)
        {
            try
            {

                // custom dialogues


                // dialogues

                if (__instance.currentMarriageDialogue == null || __instance.currentMarriageDialogue.Count == 0)
                    return;

                bool gotDialogue = false;

                for (int i = 0; i < __instance.currentMarriageDialogue.Count; i++)
                {
                    MarriageDialogueReference mdr = __instance.currentMarriageDialogue[i];

                    if (mdr.DialogueFile == "Strings\\StringsFromCSFiles")
                    {
                        foreach (string[] array in csMarriageDialoguesChoose)
                        {
                            string key = array[ModEntry.myRand.Next(0, array.Length)];
                            if (array.Contains(key))
                            {
                                Dictionary<string, string> marriageDialogues = null;
                                try
                                {
                                    marriageDialogues = ModEntry.PHelper.Content.Load<Dictionary<string, string>>("Characters\\Dialogue\\MarriageDialogue" + __instance.Name, ContentSource.GameContent);
                                }
                                catch (Exception)
                                {
                                }
                                MarriageDialogueReference mdrn;
                                if (marriageDialogues != null && marriageDialogues.ContainsKey(key))
                                {
                                    mdrn = new MarriageDialogueReference("Characters\\Dialogue\\MarriageDialogue" + __instance.Name, key, mdr.IsGendered, mdr.Substitutions.ToArray());
                                }
                                else
                                {
                                    mdrn = new MarriageDialogueReference("Characters\\Dialogue\\MarriageDialogue" + __instance.Name, key, mdr.IsGendered, mdr.Substitutions.ToArray());
                                }
                                if(mdrn != null)
                                {
                                    __instance.currentMarriageDialogue[i] = mdrn;
                                }
                                gotDialogue = true;
                                break;
                            }
                        }
                        if (!gotDialogue)
                        {
                            if (csMarriageDialoguesReplace.Contains(mdr.DialogueKey))
                            {
                                Dictionary<string, string> marriageDialogues = null;
                                try
                                {
                                    marriageDialogues = ModEntry.PHelper.Content.Load<Dictionary<string, string>>("Characters\\Dialogue\\MarriageDialogue" + __instance.Name, ContentSource.GameContent);
                                }
                                catch (Exception)
                                {
                                }
                                if (marriageDialogues != null && marriageDialogues.ContainsKey(mdr.DialogueKey))
                                {
                                    MarriageDialogueReference mdrn = new MarriageDialogueReference("Characters\\Dialogue\\MarriageDialogue" + __instance.Name, mdr.DialogueKey, mdr.IsGendered, mdr.Substitutions.ToArray());

                                    if (mdrn != null)
                                    {
                                        __instance.currentMarriageDialogue[i] = mdrn;
                                    }
                                    break;
                                }

                            }
                        }

                    }
                    else if (mdr.DialogueFile == "MarriageDialogue")
                    {
                        foreach (string[] array in csMarriageDialoguesChoose)
                        {
                            string key = array[ModEntry.myRand.Next(0, array.Length)];
                            if (array.Contains(key))
                            {
                                Dictionary<string, string> marriageDialogues = null;
                                try
                                {
                                    marriageDialogues = ModEntry.PHelper.Content.Load<Dictionary<string, string>>("Characters\\Dialogue\\MarriageDialogue" + __instance.Name, ContentSource.GameContent);
                                }
                                catch (Exception)
                                {
                                }
                                if (marriageDialogues != null && marriageDialogues.ContainsKey(key))
                                {
                                    MarriageDialogueReference mdrn = new MarriageDialogueReference("Characters\\Dialogue\\MarriageDialogue" + __instance.Name, key, mdr.IsGendered, mdr.Substitutions.ToArray());
                                    if (mdrn != null)
                                    {
                                        __instance.currentMarriageDialogue[i] = mdrn;
                                    }
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(NPC_marriageDuties_Postfix)}:\n{ex}", LogLevel.Error);
            }
        }

        public static void NPC_spouseObstacleCheck_Postfix(NPC __instance, bool __result)
        {
            try
            {
                if (__result && __instance.getSpouse() != null && __instance.currentLocation == Utility.getHomeOfFarmer(__instance.getSpouse()))
                {
                    Farmer spouse = __instance.getSpouse();
                    Misc.ResetSpouses(spouse);

                    int offset = 0;
                    if (spouse.spouse != __instance.Name)
                    {
                        int idx = Misc.GetSpouses(spouse,0).Keys.ToList().IndexOf(__instance.Name);
                        offset = 7 * (idx + 1);
                    }
                    Vector2 spot = ((__instance.currentLocation as FarmHouse).upgradeLevel == 1) ? new Vector2(32f, 5f) : new Vector2(38f, 14f);
                    __instance.setTilePosition((int)spot.X + offset, (int)spot.Y);
                    __instance.faceDirection(ModEntry.myRand.Next(0, 4));
                }
            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(NPC_spouseObstacleCheck_Postfix)}:\n{ex}", LogLevel.Error);
            }
        }
        public static void NPC_engagementResponse_Postfix(NPC __instance, Farmer who, bool asRoommate = false)
        {
            if (asRoommate)
                return;
            Misc.ResetSpouses(who);
            Friendship friendship = who.friendshipData[__instance.Name];
            WorldDate weddingDate = new WorldDate(Game1.Date);
            weddingDate.TotalDays += Math.Max(1,ModEntry.config.DaysUntilMarriage);
            while (!Game1.canHaveWeddingOnDay(weddingDate.DayOfMonth, weddingDate.Season))
            {
                weddingDate.TotalDays++;
            }
            friendship.WeddingDate = weddingDate;

            Maps.BuildSpouseRooms(Utility.getHomeOfFarmer(who));
        }

        public static bool NPC_isRoommate_Prefix(NPC __instance, ref bool __result)
        {
            try
            {

                if (!__instance.isVillager())
                {
                    __result = false;
                    return false;
                }
                foreach (Farmer f in Game1.getAllFarmers())
                {
                    if (f.isRoommate(__instance.Name))
                    {
                        __result = true;
                        return false;
                    }
                }
                __result = false;
                return false;
            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(NPC_isRoommate_Prefix)}:\n{ex}", LogLevel.Error);
                return true; // run original logic
            }
        }
        
        public static bool NPC_getSpouse_Prefix(NPC __instance, ref Farmer __result)
        {
            foreach (Farmer f in Game1.getAllFarmers())
            {
                if (f.friendshipData.ContainsKey(__instance.Name) && f.friendshipData[__instance.Name].IsMarried())
                {
                    __result = f;
                    return false;
                }
            }
            return true;
        }

        public static bool NPC_isMarried_Prefix(NPC __instance, ref bool __result)
        {
            __result = false;
            if (!__instance.isVillager())
            {
                return false;
            }
            foreach (Farmer f in Game1.getAllFarmers())
            {
                if (f.friendshipData.ContainsKey(__instance.Name) && f.friendshipData[__instance.Name].IsMarried())
                {
                    __result = true;
                    return false;
                }
            }
            return true;
        }

        public static bool NPC_isMarriedOrEngaged_Prefix(NPC __instance, ref bool __result)
        {
            __result = false;
            if (!__instance.isVillager())
            {
                return false;
            }
            foreach (Farmer f in Game1.getAllFarmers())
            {
                if (f.friendshipData.ContainsKey(__instance.Name) && (f.friendshipData[__instance.Name].IsMarried() || f.friendshipData[__instance.Name].IsEngaged()))
                {
                    __result = true;
                    return false;
                }
            }
            return true;
        }


        public static bool NPC_tryToReceiveActiveObject_Prefix(NPC __instance, ref Farmer who, Dictionary<string, string> ___dialogue, ref List<int> __state)
        {
            try
            {
                if (Misc.GetSpouses(who,1).ContainsKey(__instance.Name))
                {
                    __state = new List<int> { 
                        who.friendshipData[__instance.Name].GiftsThisWeek,
                        who.friendshipData[__instance.Name].GiftsToday,
                        0
                    };
                    who.friendshipData[__instance.Name].GiftsThisWeek = 0;
                    if (ModEntry.config.MaxGiftsPerDay < 0 || who.friendshipData[__instance.Name].GiftsToday < ModEntry.config.MaxGiftsPerDay)
                    {
                        who.friendshipData[__instance.Name].GiftsToday = 0;
                    }
                    else
                    {
                        who.friendshipData[__instance.Name].GiftsToday = 1;
                        __state[2] = 1; // flag to say we set it to 1
                    }
                }
                if (who.ActiveObject.ParentSheetIndex == 808 && __instance.Name.Equals("Krobus"))
                {
                    if (who.getFriendshipHeartLevelForNPC(__instance.Name) >= 10 && who.houseUpgradeLevel >= 1 && !who.isEngaged())
                    {
                        typeof(NPC).GetMethod("engagementResponse", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(__instance, new object[] { who, true });
                        return false;
                    }
                }
                if (who.ActiveObject.ParentSheetIndex == 458)
                {
                    if (Misc.GetSpouses(who, 1).ContainsKey(__instance.Name))
                    {
                        who.spouse = __instance.Name;
                        Misc.ResetSpouses(who);
                        GameLocation l = Game1.currentLocation;
                        l.playSound("dwop", NetAudio.SoundContext.NPC);
                        Utility.getHomeOfFarmer(who).showSpouseRoom();
                        if (ModEntry.config.BuildAllSpousesRooms)
                        {
                            Maps.BuildSpouseRooms(Utility.getHomeOfFarmer(who));
                        }
                        return false;
                    }

                    if (!__instance.datable)
                    {
                        if (ModEntry.myRand.NextDouble() < 0.5)
                        {
                            Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.3955", __instance.displayName));
                            return false;
                        }
                        __instance.CurrentDialogue.Push(new Dialogue((ModEntry.myRand.NextDouble() < 0.5) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.3956") : Game1.LoadStringByGender(__instance.gender, "Strings\\StringsFromCSFiles:NPC.cs.3957"), __instance));
                        Game1.drawDialogue(__instance);
                        return false;
                    }
                    else
                    {
                        if (__instance.datable && who.friendshipData.ContainsKey(__instance.Name) && who.friendshipData[__instance.Name].IsDating())
                        {
                            Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\UI:AlreadyDatingBouquet", __instance.displayName));
                            return false;
                        }
                        if (__instance.datable && who.friendshipData.ContainsKey(__instance.Name) && who.friendshipData[__instance.Name].IsDivorced())
                        {
                            if (ModEntry.config.FriendlyDivorce)
                            {
                                who.friendshipData[__instance.Name].Points = ModEntry.config.MinPointsToDate;
                                who.friendshipData[__instance.Name].Status = FriendshipStatus.Friendly;
                            }
                            else
                            {
                                __instance.CurrentDialogue.Push(new Dialogue(Game1.content.LoadString("Strings\\Characters:Divorced_bouquet"), __instance));
                                Game1.drawDialogue(__instance);
                                return false;
                            }
                        }
                        if (__instance.datable && who.friendshipData.ContainsKey(__instance.Name) && who.friendshipData[__instance.Name].Points < ModEntry.config.MinPointsToDate / 2f)
                        {
                            __instance.CurrentDialogue.Push(new Dialogue((ModEntry.myRand.NextDouble() < 0.5) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.3958") : Game1.LoadStringByGender(__instance.gender, "Strings\\StringsFromCSFiles:NPC.cs.3959"), __instance));
                            Game1.drawDialogue(__instance);
                            return false;
                        }
                        if (__instance.datable && who.friendshipData.ContainsKey(__instance.Name) && who.friendshipData[__instance.Name].Points < ModEntry.config.MinPointsToDate)
                        {
                            __instance.CurrentDialogue.Push(new Dialogue((ModEntry.myRand.NextDouble() < 0.5) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.3960") : Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.3961"), __instance));
                            Game1.drawDialogue(__instance);
                            return false;
                        }
                        Friendship friendship = who.friendshipData[__instance.Name];
                        if (!friendship.IsDating())
                        {
                            friendship.Status = FriendshipStatus.Dating;
                            Multiplayer mp = ModEntry.PHelper.Reflection.GetField<Multiplayer>(typeof(Game1), "multiplayer").GetValue();
                            mp.globalChatInfoMessage("Dating", new string[]
                            {
                                    who.Name,
                                    __instance.displayName
                            });
                        }
                        __instance.CurrentDialogue.Push(new Dialogue((ModEntry.myRand.NextDouble() < 0.5) ? Game1.LoadStringByGender(__instance.gender, "Strings\\StringsFromCSFiles:NPC.cs.3962") : Game1.LoadStringByGender(__instance.gender, "Strings\\StringsFromCSFiles:NPC.cs.3963"), __instance));
                        who.changeFriendship(25, __instance);
                        who.reduceActiveItemByOne();
                        who.completelyStopAnimatingOrDoingAction();
                        __instance.doEmote(20, true);
                        Game1.drawDialogue(__instance);
                        return false;
                    }
                }
                else if (who.ActiveObject.ParentSheetIndex == 460)
                {
                    if (who.isEngaged())
                    {
                        Monitor.Log($"Tried to give pendant while engaged");

                        __instance.CurrentDialogue.Push(new Dialogue((ModEntry.myRand.NextDouble() < 0.5) ? Game1.LoadStringByGender(__instance.gender, "Strings\\StringsFromCSFiles:NPC.cs.3965") : Game1.LoadStringByGender(__instance.gender, "Strings\\StringsFromCSFiles:NPC.cs.3966"), __instance));
                        Game1.drawDialogue(__instance);
                        return false;
                    }
                    if (!__instance.datable || __instance.isMarriedOrEngaged() || (who.friendshipData.ContainsKey(__instance.Name) && who.friendshipData[__instance.Name].Points < ModEntry.config.MinPointsToMarry * 0.6f))
                    {
                        Monitor.Log($"Tried to give pendant to someone not datable");

                        if (ModEntry.myRand.NextDouble() < 0.5)
                        {
                            Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.3969", __instance.displayName));
                            return false;
                        }
                        __instance.CurrentDialogue.Push(new Dialogue((__instance.Gender == 1) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.3970") : Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.3971"), __instance));
                        Game1.drawDialogue(__instance);
                        return false;
                    }
                    else if (__instance.datable && who.friendshipData.ContainsKey(__instance.Name) && who.friendshipData[__instance.Name].Points < ModEntry.config.MinPointsToMarry)
                    {
                        Monitor.Log($"Tried to give pendant to someone not marriable");

                        if (!who.friendshipData[__instance.Name].ProposalRejected)
                        {
                            __instance.CurrentDialogue.Push(new Dialogue((ModEntry.myRand.NextDouble() < 0.5) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.3972") : Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.3973"), __instance));
                            Game1.drawDialogue(__instance);
                            who.changeFriendship(-20, __instance);
                            who.friendshipData[__instance.Name].ProposalRejected = true;
                            return false;
                        }
                        __instance.CurrentDialogue.Push(new Dialogue((ModEntry.myRand.NextDouble() < 0.5) ? Game1.LoadStringByGender(__instance.gender, "Strings\\StringsFromCSFiles:NPC.cs.3974") : Game1.LoadStringByGender(__instance.gender, "Strings\\StringsFromCSFiles:NPC.cs.3975"), __instance));
                        Game1.drawDialogue(__instance);
                        who.changeFriendship(-50, __instance);
                        return false;
                    }
                    else
                    {
                        Monitor.Log($"Tried to give pendant to someone marriable");
                        if (!__instance.datable || who.houseUpgradeLevel >= 1)
                        {
                            typeof(NPC).GetMethod("engagementResponse", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(__instance, new object[] { who, false });
                            return false;
                        }
                        Monitor.Log($"Can't marry");
                        if (ModEntry.myRand.NextDouble() < 0.5)
                        {
                            Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.3969", __instance.displayName));
                            return false;
                        }
                        __instance.CurrentDialogue.Push(new Dialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.3972"), __instance));
                        Game1.drawDialogue(__instance);
                        return false;
                    }
                }
                else if (who.ActiveObject.parentSheetIndex == 809 && !who.ActiveObject.bigCraftable)
                {
                    Monitor.Log($"Tried to give movie ticket");
                    if (Misc.GetSpouses(who, 1).ContainsKey(__instance.Name) && Utility.doesMasterPlayerHaveMailReceivedButNotMailForTomorrow("ccMovieTheater") && !__instance.Name.Equals("Krobus") && who.lastSeenMovieWeek.Value < Game1.Date.TotalWeeks && !Utility.isFestivalDay(Game1.dayOfMonth, Game1.currentSeason) && Game1.timeOfDay <= 2100 && __instance.lastSeenMovieWeek.Value < Game1.Date.TotalWeeks && MovieTheater.GetResponseForMovie(__instance) != "reject")
                    {
                        Monitor.Log($"Tried to give movie ticket to spouse");
                        foreach (MovieInvitation invitation in who.team.movieInvitations)
                        {
                            if (invitation.farmer == who)
                            {
                                return true;
                            }
                        }
                        foreach (MovieInvitation invitation2 in who.team.movieInvitations)
                        {
                            if (invitation2.invitedNPC == __instance)
                            {
                                return true;
                            }
                        }

                        Monitor.Log($"Giving movie ticket to spouse");

                        if (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.en)
                        {
                            __instance.CurrentDialogue.Push(new Dialogue(__instance.GetDispositionModifiedString("Strings\\Characters:MovieInvite_Spouse_" + __instance.name, new object[0]), __instance));
                        }
                        else if (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.en && ___dialogue != null && ___dialogue.ContainsKey("MovieInvitation"))
                        {
                            __instance.CurrentDialogue.Push(new Dialogue(___dialogue["MovieInvitation"], __instance));
                        }
                        else
                        {
                            __instance.CurrentDialogue.Push(new Dialogue(__instance.GetDispositionModifiedString("Strings\\Characters:MovieInvite_Invited", new object[0]), __instance));
                        }
                        Game1.drawDialogue(__instance);
                        who.reduceActiveItemByOne();
                        who.completelyStopAnimatingOrDoingAction();
                        who.currentLocation.localSound("give_gift");
                        MovieTheater.Invite(who, __instance);
                        if (who == Game1.player)
                        {
                            ModEntry.mp.globalChatInfoMessage("MovieInviteAccept", new string[]
                            {
                            Game1.player.displayName,
                            __instance.displayName
                            });
                            return false;
                        }
                    }
                    return true;
                }

            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(NPC_tryToReceiveActiveObject_Prefix)}:\n{ex}", LogLevel.Error);
            }
            return true;
        }

        public static void NPC_tryToReceiveActiveObject_Postfix(NPC __instance, ref Farmer who, List<int> __state)
        {
            try
            {
                if(__state != null)
                {
                    if (__state.Count > 0)
                    {
                        who.friendshipData[__instance.Name].GiftsThisWeek += __state[0];
                        who.friendshipData[__instance.Name].GiftsToday += __state[1] - (__state[2] == 1 ? 1 : 0);
                        Monitor.Log($"gifts this week {who.friendshipData[__instance.Name].GiftsThisWeek}");
                        Monitor.Log($"gifts today {who.friendshipData[__instance.Name].GiftsToday}");
                    }
                }
            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(NPC_tryToReceiveActiveObject_Postfix)}:\n{ex}", LogLevel.Error);
            }
        }

        public static IEnumerable<CodeInstruction> NPC_tryToReceiveActiveObject_Transpiler(IEnumerable<CodeInstruction> instructions)
        {

            var codes = new List<CodeInstruction>(instructions);
            bool startLooking = false;
            for (int i = 0; i < codes.Count; i++)
            {
                if (startLooking)
                {
                    if(codes[i].opcode == OpCodes.Ldc_I4_S && int.Parse(codes[i].operand.ToString()) < -10)
                    {
                        Monitor.Log($"got int!");
                        codes[i] = new CodeInstruction(OpCodes.Ldc_I4_S, 30);
                        break;
                    }
                }
                else if ((codes[i].operand as string) == "Strings\\StringsFromCSFiles:NPC.cs.3981")
                {
                    Monitor.Log($"got string!");
                    startLooking = true;
                }
            }

            return codes.AsEnumerable();
        }

        public static void Child_reloadSprite_Postfix(ref Child __instance)
        {
            try
            {
                if (__instance.Name == null)
                    return;
                string[] names = __instance.Name.Split(' ');
                if (names.Length < 2 || names[names.Length - 1].Length < 3)
                {
                    return;
                }
                if (!ModEntry.config.ShowParentNames && __instance.Name.EndsWith(")"))
                {
                    __instance.displayName = string.Join(" ", names.Take(names.Length - 1));
                }
                string parent = names[names.Length - 1].Substring(1, names[names.Length - 1].Length - 2);
                __instance.Sprite.textureName.Value += $"_{parent}";
                Monitor.Log($"set child texture to: {__instance.Sprite.textureName.Value}");
            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(Child_reloadSprite_Postfix)}:\n{ex}", LogLevel.Error);
            }
        }



        public static void Child_resetForPlayerEntry_Postfix(ref Child __instance, GameLocation l)
        {
            try
            {
                if (l is FarmHouse && (__instance.age == 0 || __instance.age == 1))
                {
                    SetCribs(l);
                }
            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(Child_resetForPlayerEntry_Postfix)}:\n{ex}", LogLevel.Error);
            }
        }

        public static void SetCribs(GameLocation location)
        {

            if (ModEntry.config.ExtraCribs < 0)
                return;

            int babies = 0;
            foreach (NPC npc in location.characters)
            {
                if (npc is Child && (npc.age == 0 || npc.age == 1))
                {
                    if (ModEntry.config.ExtraCribs >= babies)
                    {
                        npc.Position = new Vector2(16f + (3 * babies), 4f) * 64f + new Vector2(0f, -24f);
                    }
                    else
                    {
                        int crib = babies % (ModEntry.config.ExtraCribs+1);
                        npc.Position = new Vector2(15f + (3 * crib), 4f) * 64f + new Vector2(24f, -48f);
                    }
                    babies++;
                }
            }
        }
        public static void Child_dayUpdate_Prefix(Child __instance)
        {
            try
            {
                __instance.daysOld.Value += Math.Max(0, (ModEntry.config.ChildGrowthMultiplier - 1));

            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(Child_dayUpdate_Prefix)}:\n{ex}", LogLevel.Error);
            }
        }
        public static void Child_tenMinuteUpdate_Postfix(Child __instance)
        {
            try
            {

                if (Game1.IsMasterGame && __instance.Age == 3 && Game1.timeOfDay == 1900)
                {
                    __instance.IsWalkingInSquare = false;
                    __instance.Halt();
                    FarmHouse farmHouse = __instance.currentLocation as FarmHouse;
                    if (farmHouse.characters.Contains(__instance))
                    {
                        __instance.controller = new PathFindController(__instance, farmHouse, Misc.getChildBed(farmHouse, __instance.Name), -1, new PathFindController.endBehavior(__instance.toddlerReachedDestination));
                        if (__instance.controller.pathToEndPoint == null || !farmHouse.isTileOnMap(__instance.controller.pathToEndPoint.Last<Point>().X, __instance.controller.pathToEndPoint.Last<Point>().Y))
                        {
                            __instance.controller = null;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(Child_tenMinuteUpdate_Postfix)}:\n{ex}", LogLevel.Error);
            }
        }
    }
}
