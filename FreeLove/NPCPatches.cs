using HarmonyLib;
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
using Object = StardewValley.Object;

namespace FreeLove
{
    public static class NPCPatches
    {
        private static IMonitor Monitor;
        private static ModConfig Config;
        private static IModHelper Helper;

        // call this method from your Entry class
        public static void Initialize(IMonitor monitor, ModConfig config, IModHelper helper)
        {
            Monitor = monitor;
            Config = config;
            Helper = helper;
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

        // currently unused

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
         

        internal static bool NPC_tryToRetrieveDialogue_Prefix(NPC __instance, ref Dialogue __result, string appendToEnd)
        {
            try
            {
                if (appendToEnd.Contains("_inlaw_") && Game1.player.friendshipData.ContainsKey(__instance.Name) && Game1.player.friendshipData[__instance.Name].IsMarried())
                {
                    __result = null;
                    return false;
                }
            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(NPC_tryToRetrieveDialogue_Prefix)}:\n{ex}", LogLevel.Error);
            }
            return true;
        }
        
        internal static void NPC_GetDispositionModifiedString_Prefix(NPC __instance, ref bool __state)
        {
            try
            {
                if (Game1.player.isMarried() && Game1.player.friendshipData.ContainsKey(__instance.Name) &&  Game1.player.friendshipData[__instance.Name].IsMarried() && Game1.player.spouse != __instance.Name)
                {
                    ModEntry.tempOfficialSpouse = __instance;
                    __state = true;
                }
            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(NPC_GetDispositionModifiedString_Prefix)}:\n{ex}", LogLevel.Error);
            }
        }

        internal static void NPC_GetDispositionModifiedString_Postfix(bool __state)
        {
            try
            {
                if (__state)
                {
                    ModEntry.tempOfficialSpouse = null;
                }
            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(NPC_GetDispositionModifiedString_Postfix)}:\n{ex}", LogLevel.Error);
            }
        }

        public static void NPC_marriageDuties_Prefix(NPC __instance)
        {
            try
            {
                if (Misc.GetSpouses(Game1.player, 0).ContainsKey(__instance.Name))
                {
                    ModEntry.tempOfficialSpouse = __instance;
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
                if (ModEntry.tempOfficialSpouse == __instance)
                {
                    ModEntry.tempOfficialSpouse = null;
                }
                return;

                // custom dialogues


                // dialogues

                if (__instance.currentMarriageDialogue is null || __instance.currentMarriageDialogue.Count == 0)
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
/*
                return;
                if (Config.RemoveSpouseOrdinaryDialogue && __instance.Name != Game1.player.spouse && __instance.currentMarriageDialogue.Count > 0)
                {
                    __instance.CurrentDialogue.Clear();
                    foreach (MarriageDialogueReference mdr in __instance.currentMarriageDialogue)
                    {
                        __instance.CurrentDialogue.Push(mdr.GetDialogue(__instance));
                        __instance.currentMarriageDialogue.Clear();
                    }
                }
*/
            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(NPC_marriageDuties_Postfix)}:\n{ex}", LogLevel.Error);
            }
        }

        public static bool NPC_engagementResponse_Prefix(NPC __instance, Farmer who, bool asRoommate = false)
        {
            Monitor.Log($"engagement response");
            Monitor.Log($"engagement response for {__instance.Name}");
            if (asRoommate)
            {
                Monitor.Log($"{__instance.Name} is roomate");
                return true;
            }
            if (!who.friendshipData.ContainsKey(__instance.Name))
            {
                Monitor.Log($"{who.Name} has no friendship data for {__instance.Name}", LogLevel.Error);
                return false;
            }
            return true;
        }
        public static void NPC_engagementResponse_Postfix(NPC __instance, Farmer who, bool asRoommate = false)
        {
            Misc.ResetSpouses(who);
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


        internal static void NPC_loadCurrentDialogue_Prefix(NPC __instance, ref string __state)
        {
            try
            {
                if (Misc.GetSpouses(Game1.player, 0).ContainsKey(__instance.Name))
                {
                    __state = Game1.player.spouse;
                    Game1.player.spouse = __instance.Name;
                }
            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(NPC_loadCurrentDialogue_Prefix)}:\n{ex}", LogLevel.Error);
            }
        }


        public static void NPC_loadCurrentDialogue_Postfix(string __state)
        {
            try
            {
                if (__state != null)
                {
                    Game1.player.spouse = __state;
                }
            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(NPC_loadCurrentDialogue_Postfix)}:\n{ex}", LogLevel.Error);
            }
        }

        public static bool NPC_checkAction_Prefix(ref NPC __instance, ref Farmer who, GameLocation l, ref bool __result)
        {
            if (!Config.EnableMod || __instance.IsInvisible || __instance.isSleeping.Value || !who.canMove || who.checkForQuestComplete(__instance, -1, -1, who.ActiveObject, null, -1, 5) || (who.pantsItem.Value?.ParentSheetIndex == 15 && (__instance.Name.Equals("Lewis") || __instance.Name.Equals("Marnie"))) || (__instance.Name.Equals("Krobus") && who.hasQuest(28)))
                return true;

            try
            {
                Misc.ResetSpouses(who);

                if ((__instance.Name.Equals(who.spouse) || Misc.GetSpouses(who, 1).ContainsKey(__instance.Name)) && __instance.Sprite.CurrentAnimation == null && who.IsLocalPlayer)
                {
                    Monitor.Log($"{__instance.Name} is married to {who.Name}");

                    __instance.faceDirection(-3);

                    if (who.friendshipData.ContainsKey(__instance.Name) && who.friendshipData[__instance.Name].Points >= 3125 && !who.mailReceived.Contains("CF_Spouse"))
                    {
                        Monitor.Log($"getting starfruit");
                        __instance.CurrentDialogue.Push(new Dialogue(Game1.content.LoadString(Game1.player.isRoommate(who.spouse) ? "Strings\\StringsFromCSFiles:Krobus_Stardrop" : "Strings\\StringsFromCSFiles:NPC.cs.4001"), __instance));
                        Game1.player.addItemByMenuIfNecessary(new Object(Vector2.Zero, 434, "Cosmic Fruit", false, false, false, false), null);
                        __instance.shouldSayMarriageDialogue.Value = false;
                        __instance.currentMarriageDialogue.Clear();
                        who.mailReceived.Add("CF_Spouse");
                        __result = true;
                        return false;
                    }
                    if (__instance.Sprite.CurrentAnimation == null && !__instance.hasTemporaryMessageAvailable() && __instance.currentMarriageDialogue.Count == 0 && __instance.CurrentDialogue.Count == 0 && Game1.timeOfDay < 2200 && !__instance.isMoving() && who.ActiveObject == null && Integrations.kissingAPI == null)
                    {
                        Monitor.Log($"Trying to kiss/hug {__instance.Name}");

                        __instance.faceGeneralDirection(who.getStandingPosition(), 0, false);
                        who.faceGeneralDirection(__instance.getStandingPosition(), 0, false);
                        if (__instance.FacingDirection == 3 || __instance.FacingDirection == 1)
                        {

                            if (__instance.hasBeenKissedToday.Value)
                            {
                                Monitor.Log($"{__instance.Name} has been kissed today");
                                return true;
                            }

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
                            if (who.getFriendshipHeartLevelForNPC(__instance.Name) >= 9)
                            {
                                Monitor.Log($"Can kiss/hug {__instance.Name}");

                                int delay = Game1.IsMultiplayer ? 1000 : 10;
                                __instance.movementPause = delay;
                                __instance.Sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
                                {
                                    new FarmerSprite.AnimationFrame(spouseFrame, delay, false, flip, new AnimatedSprite.endOfAnimationBehavior(__instance.haltMe), true)
                                });
                                if (!__instance.hasBeenKissedToday.Value)
                                {
                                    who.changeFriendship(10, __instance);
                                    if (who.friendshipData[__instance.Name].RoommateMarriage)
                                    {
                                        Monitor.Log($"Hugging {__instance.Name}");
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
                                        Monitor.Log($"Kissing {__instance.Name}");
                                        ModEntry.mp.broadcastSprites(who.currentLocation, new TemporaryAnimatedSprite[]
                                        {
                                            new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(211, 428, 7, 6), 2000f, 1, 0, new Vector2((float)__instance.getTileX(), (float)__instance.getTileY()) * 64f + new Vector2(16f, -64f), false, false, 1f, 0f, Color.White, 4f, 0f, 0f, 0f, false)
                                            {
                                                motion = new Vector2(0f, -0.5f),
                                                alphaFade = 0.01f
                                            }
                                        });
                                    }
                                    __instance.currentLocation.playSound("dwop", NetAudio.SoundContext.NPC);
                                    who.exhausted.Value = false;

                                }
                                __instance.hasBeenKissedToday.Value = true;
                                __instance.Sprite.UpdateSourceRect();
                            }
                            else
                            {
                                Monitor.Log($"Kiss/hug rejected by {__instance.Name}");

                                __instance.faceDirection((ModEntry.myRand.NextDouble() < 0.5) ? 2 : 0);
                                __instance.doEmote(12, true);
                            }
                            int playerFaceDirection = 1;
                            if ((facingRight && !flip) || (!facingRight && flip))
                            {
                                playerFaceDirection = 3;
                            }
                            who.PerformKiss(playerFaceDirection);
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


        public static bool NPC_tryToReceiveActiveObject_Prefix(NPC __instance, ref Farmer who, Dictionary<string, string> ___dialogue, ref List<int> __state)
        {
            try
            {
                if (Misc.GetSpouses(who, 1).ContainsKey(__instance.Name) && Game1.NPCGiftTastes.ContainsKey(__instance.Name))
                {
                    Monitor.Log($"Gift to spouse {__instance.Name}");
                    __state = new List<int> {
                        who.friendshipData[__instance.Name].GiftsToday,
                        who.friendshipData[__instance.Name].GiftsThisWeek,
                        0,
                        0
                    };
                    if (Config.MaxGiftsPerSpousePerDay < 0 || who.friendshipData[__instance.Name].GiftsToday < Config.MaxGiftsPerSpousePerDay)
                    {
                        who.friendshipData[__instance.Name].GiftsToday = 0;
                    }
                    else
                    {
                        who.friendshipData[__instance.Name].GiftsToday = 1;
                        __state[2] = 1; // flag to say we set it to 1
                    }
                    if (Config.MaxGiftsPerSpousePerWeek < 0 || who.friendshipData[__instance.Name].GiftsThisWeek < Config.MaxGiftsPerSpousePerWeek)
                    {
                        who.friendshipData[__instance.Name].GiftsThisWeek = 0;
                    }
                    else
                    {
                        who.friendshipData[__instance.Name].GiftsThisWeek = 2;
                        __state[3] = 1; // flag to say we set it to 1
                    }
                }
                if (who.ActiveObject.ParentSheetIndex == 808 && __instance.Name.Equals("Krobus"))
                {
                    Monitor.Log($"Void pendant to {__instance.Name}");
                    if (who.getFriendshipHeartLevelForNPC(__instance.Name) >= 10 && who.HouseUpgradeLevel >= 1 && !who.isEngaged())
                    {
                        typeof(NPC).GetMethod("engagementResponse", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(__instance, new object[] { who, true });
                        return false;
                    }
                }
                else if (who.ActiveObject.ParentSheetIndex == 458)
                {
                    Monitor.Log($"Try give bouquet to {__instance.Name}");

                    if (Misc.GetSpouses(who, 1).ContainsKey(__instance.Name))
                    {
                        who.spouse = __instance.Name;
                        Misc.ResetSpouses(who);
                        Game1.currentLocation.playSound("dwop", NetAudio.SoundContext.NPC);
                        FarmHouse fh = Utility.getHomeOfFarmer(who);
                        fh.showSpouseRoom();
                        Helper.Reflection.GetMethod(fh, "resetLocalState").Invoke();
                        return false;
                    }

                    if (!__instance.datable.Value)
                    {
                        if (ModEntry.myRand.NextDouble() < 0.5)
                        {
                            Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.3955", __instance.displayName));
                            return false;
                        }
                        __instance.CurrentDialogue.Push(new Dialogue((ModEntry.myRand.NextDouble() < 0.5) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.3956") : Game1.LoadStringByGender(__instance.Gender, "Strings\\StringsFromCSFiles:NPC.cs.3957"), __instance));
                        Game1.drawDialogue(__instance);
                        return false;
                    }
                    else
                    {
                        if (__instance.datable.Value && who.friendshipData.ContainsKey(__instance.Name) && who.friendshipData[__instance.Name].IsDating())
                        {
                            Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\UI:AlreadyDatingBouquet", __instance.displayName));
                            return false;
                        }
                        if (__instance.datable.Value && who.friendshipData.ContainsKey(__instance.Name) && who.friendshipData[__instance.Name].IsDivorced())
                        {
                            __instance.CurrentDialogue.Push(new Dialogue(Game1.content.LoadString("Strings\\Characters:Divorced_bouquet"), __instance));
                            Game1.drawDialogue(__instance);
                            return false;
                        }
                        if (__instance.datable.Value && who.friendshipData.ContainsKey(__instance.Name) && who.friendshipData[__instance.Name].Points < Config.MinPointsToDate / 2f)
                        {
                            __instance.CurrentDialogue.Push(new Dialogue((ModEntry.myRand.NextDouble() < 0.5) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:NPC.cs.3958") : Game1.LoadStringByGender(__instance.Gender, "Strings\\StringsFromCSFiles:NPC.cs.3959"), __instance));
                            Game1.drawDialogue(__instance);
                            return false;
                        }
                        if (__instance.datable.Value && who.friendshipData.ContainsKey(__instance.Name) && who.friendshipData[__instance.Name].Points < Config.MinPointsToDate)
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
                        __instance.CurrentDialogue.Push(new Dialogue((ModEntry.myRand.NextDouble() < 0.5) ? Game1.LoadStringByGender(__instance.Gender, "Strings\\StringsFromCSFiles:NPC.cs.3962") : Game1.LoadStringByGender(__instance.Gender, "Strings\\StringsFromCSFiles:NPC.cs.3963"), __instance));
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
                    Monitor.Log($"Try give pendant to {__instance.Name}");
                    if (who.isEngaged())
                    {
                        Monitor.Log($"Tried to give pendant while engaged");

                        __instance.CurrentDialogue.Push(new Dialogue((ModEntry.myRand.NextDouble() < 0.5) ? Game1.LoadStringByGender(__instance.Gender, "Strings\\StringsFromCSFiles:NPC.cs.3965") : Game1.LoadStringByGender(__instance.Gender, "Strings\\StringsFromCSFiles:NPC.cs.3966"), __instance));
                        Game1.drawDialogue(__instance);
                        return false;
                    }
                    if (!__instance.datable.Value || __instance.isMarriedOrEngaged() || (who.friendshipData.ContainsKey(__instance.Name) && who.friendshipData[__instance.Name].Points < Config.MinPointsToMarry * 0.6f))
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
                    else if (__instance.datable.Value && who.friendshipData.ContainsKey(__instance.Name) && who.friendshipData[__instance.Name].Points < Config.MinPointsToMarry)
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
                        __instance.CurrentDialogue.Push(new Dialogue((ModEntry.myRand.NextDouble() < 0.5) ? Game1.LoadStringByGender(__instance.Gender, "Strings\\StringsFromCSFiles:NPC.cs.3974") : Game1.LoadStringByGender(__instance.Gender, "Strings\\StringsFromCSFiles:NPC.cs.3975"), __instance));
                        Game1.drawDialogue(__instance);
                        who.changeFriendship(-50, __instance);
                        return false;
                    }
                    else
                    {
                        Monitor.Log($"Tried to give pendant to someone marriable");
                        if (!__instance.datable.Value || who.HouseUpgradeLevel >= 1)
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
                else if (who.ActiveObject.ParentSheetIndex == 809 && !who.ActiveObject.bigCraftable.Value)
                {
                    Monitor.Log($"Tried to give movie ticket to {__instance.Name}");
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
                            __instance.CurrentDialogue.Push(new Dialogue(__instance.GetDispositionModifiedString("Strings\\Characters:MovieInvite_Spouse_" + __instance.Name, new object[0]), __instance));
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
                if (__state != null && __state.Count > 0)
                {
                    who.friendshipData[__instance.Name].GiftsToday += __state[0] - (__state[2] == 1 ? 1 : 0);
                    who.friendshipData[__instance.Name].GiftsThisWeek += __state[1] - (__state[3] == 1 ? 2 : 0);
                    Monitor.Log($"gifts this week {who.friendshipData[__instance.Name].GiftsThisWeek}");
                    Monitor.Log($"gifts today {who.friendshipData[__instance.Name].GiftsToday}");
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
                        codes[i] = new CodeInstruction(OpCodes.Ldc_I4_S, 10);
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

        public static void NPC_playSleepingAnimation_Postfix(NPC __instance)
        {
            try
            {
                Dictionary<string, string> animationDescriptions = Game1.content.Load<Dictionary<string, string>>("Data\\animationDescriptions");
                if (!animationDescriptions.ContainsKey(__instance.Name.ToLower() + "_sleep") && animationDescriptions.ContainsKey(__instance.Name + "_Sleep"))
                {
                    if(int.TryParse(animationDescriptions[__instance.Name + "_Sleep"].Split('/')[0], out int sleep_frame))
                    {
                        __instance.Sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
                        {
                            new FarmerSprite.AnimationFrame(sleep_frame, 100, false, false, null, false)
                        });
                        __instance.Sprite.loop = true;
                    }
                }
            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(NPC_playSleepingAnimation_Postfix)}:\n{ex}", LogLevel.Error);
            }
        }
        
        public static void Character_displayName_Getter_Postfix(ref Character __instance, ref string __result)
        {
            try
            {
                if (__instance.Name == null || !(__instance is Child) || !Config.ShowParentNames || !__instance.modData.ContainsKey("aedenthorn.FreeLove/OtherParent"))
                    return;
                __result = $"{__result} ({__instance.modData["aedenthorn.FreeLove/OtherParent"]})";
            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(Character_displayName_Getter_Postfix)}:\n{ex}", LogLevel.Error);
            }
        }
    }
}
