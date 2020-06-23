using Microsoft.Xna.Framework;
using Netcode;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Characters;
using StardewValley.Locations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using xTile.Dimensions;

namespace MultipleSpouses
{
    public static class LocationPatches
    {
        private static IMonitor Monitor;

        // call this method from your Entry class
        public static void Initialize(IMonitor monitor)
        {
            Monitor = monitor;
        }

        public static void FarmHouse_getWalls_Postfix(FarmHouse __instance, ref List<Microsoft.Xna.Framework.Rectangle> __result)
        {
            try
            {
                if (__instance.owner == null)
                    return;
                int ecribs = Math.Max(ModEntry.config.ExtraCribs, 0);
                int espace = Math.Max(ModEntry.config.ExtraKidsRoomWidth, 0);
                int ebeds = Math.Max(ModEntry.config.ExtraKidsBeds, 0);

                if (__instance.upgradeLevel > 1 && ecribs + espace + ebeds > 0)
                {
                    int x = (ecribs * 3) + espace + (ebeds * 4);
                    __result.Remove(new Microsoft.Xna.Framework.Rectangle(15, 1, 13, 3));
                    __result.Add(new Microsoft.Xna.Framework.Rectangle(15, 1, 13 + x, 3));
                }
                if (ModEntry.config.BuildAllSpousesRooms)
                {
                    int count = Misc.GetSpouses(__instance.owner, 0).Keys.ToList().FindAll((spouse) => Maps.roomIndexes.ContainsKey(spouse) || Maps.tmxSpouseRooms.ContainsKey(spouse)).Count;

                    if (count > 0)
                    {
                        for(int i = 0; i < count; i++)
                        {
                            if (__instance.upgradeLevel > 1)
                            {
                                __result.Add(new Microsoft.Xna.Framework.Rectangle(41 + i * 7, 10, 7, 3));
                            }
                            else 
                            {
                                __result.Add(new Microsoft.Xna.Framework.Rectangle(35 + i * 7, 1, 7, 3));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(FarmHouse_getWalls_Postfix)}:\n{ex}", LogLevel.Error);
            }
        }
        
        public static void FarmHouse_getFloors_Postfix(FarmHouse __instance, ref List<Microsoft.Xna.Framework.Rectangle> __result)
        {
            try
            {
                if (__instance.owner == null)
                    return;
                int ecribs = Math.Max(ModEntry.config.ExtraCribs, 0);
                int espace = Math.Max(ModEntry.config.ExtraKidsRoomWidth, 0);
                int ebeds = Math.Max(ModEntry.config.ExtraKidsBeds, 0);
                if (__instance.upgradeLevel > 1 && ecribs + espace + ebeds > 0)
                {
                    int x = (ecribs * 3) + espace + (ebeds * 4);
                    __result.Remove(new Microsoft.Xna.Framework.Rectangle(15, 3, 13, 6));
                    __result.Add(new Microsoft.Xna.Framework.Rectangle(15, 3, 13 + x, 6));
                }
                if (ModEntry.config.BuildAllSpousesRooms)
                {
                    int count = Misc.GetSpouses(__instance.owner, 0).Keys.ToList().FindAll((spouse) => Maps.roomIndexes.ContainsKey(spouse) || Maps.tmxSpouseRooms.ContainsKey(spouse)).Count;

                    if (__instance.owner.spouse != null && !__instance.owner.friendshipData[__instance.owner.spouse].IsEngaged() && (Maps.roomIndexes.ContainsKey(__instance.owner.spouse) || Maps.tmxSpouseRooms.ContainsKey(__instance.owner.spouse)))
                        count++;
                    if (count > 0)
                    {
                        if (__instance.upgradeLevel > 1)
                        {
                            __result.Remove(new Microsoft.Xna.Framework.Rectangle(23, 12, 12, 11));
                            __result.Add(new Microsoft.Xna.Framework.Rectangle(23, 12, 11, 11));
                            __result.Add(new Microsoft.Xna.Framework.Rectangle(34, 19, count * 7, 1));
                        }
                        else
                        {
                            __result.Remove(new Microsoft.Xna.Framework.Rectangle(20, 3, 9, 8));
                            __result.Add(new Microsoft.Xna.Framework.Rectangle(20, 3, 8, 8));
                            __result.Add(new Microsoft.Xna.Framework.Rectangle(28, 10, count * 7, 1));
                        }
                        for (int i = 0; i < count; i++)
                        {
                            if (__instance.upgradeLevel > 1)
                            {
                                __result.Add(new Microsoft.Xna.Framework.Rectangle(41 + i * 7, 13, 6, 6));
                                __result.Add(new Microsoft.Xna.Framework.Rectangle(34 + (i * 7), 13, 1, 6));
                            }
                            else
                            {
                                __result.Add(new Microsoft.Xna.Framework.Rectangle(29 + i * 7, 4, 6, 6));
                                __result.Add(new Microsoft.Xna.Framework.Rectangle(28 + (i * 7), 4, 1, 6));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(FarmHouse_getFloors_Postfix)}:\n{ex}", LogLevel.Error);
            }
        }
        

        public static void Beach_resetLocalState_Postfix(Beach __instance)
        {
            try
            {

                if (ModEntry.config.BuyPendantsAnytime)
                {
                    ModEntry.PHelper.Reflection.GetField<NPC>(__instance, "oldMariner").SetValue(new NPC(new AnimatedSprite("Characters\\Mariner", 0, 16, 32), new Vector2(80f, 5f) * 64f, 2, "Old Mariner", null));
                }
            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(Beach_resetLocalState_Postfix)}:\n{ex}", LogLevel.Error);
            }
        }


        public static void FarmHouse_resetLocalState_Postfix(FarmHouse __instance)
        {
            try
            {
                Farmer f = __instance.owner;

                if (f == null || Misc.GetSpouses(f,1).Count == 0)
                {
                    return;
                }
                ModEntry.PMonitor.Log("reset farmhouse state");


                Misc.ResetSpouses(f);

                if (f.currentLocation == __instance && Misc.IsInBed(__instance, f.GetBoundingBox()))
                {
                    f.position.Value = Misc.GetFarmerBedPosition(__instance);
                }
                NPCPatches.SetCribs(__instance);

                if (Misc.ChangingHouse())
                {
                    if (__instance.upgradeLevel > 1)
                    {
                        Maps.ExpandKidsRoom(__instance);
                    }
                    __instance.showSpouseRoom();
                    Maps.BuildSpouseRooms(__instance);
                    if (ModEntry.config.CustomBed)
                    {
                        Maps.ReplaceBed(__instance);
                    }
                }
            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(FarmHouse_resetLocalState_Postfix)}:\n{ex}", LogLevel.Error);
            }
        }

        public static void Farm_addSpouseOutdoorArea_Prefix(ref string spouseName)
        {
            try
            {
                ModEntry.PMonitor.Log($"Checking for outdoor spouse to change area");
                if (ModEntry.outdoorSpouse != null && spouseName != "")
                {
                    spouseName = ModEntry.outdoorSpouse;
                    ModEntry.PMonitor.Log($"Setting outdoor spouse area for {spouseName}");
                }
            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(Farm_addSpouseOutdoorArea_Prefix)}:\n{ex}", LogLevel.Error);
            }

        }


        public static bool Beach_checkAction_Prefix(Beach __instance, Location tileLocation, xTile.Dimensions.Rectangle viewport, Farmer who, ref bool __result, NPC ___oldMariner)
        {
            try
            {
                if (___oldMariner != null && ___oldMariner.getTileX() == tileLocation.X && ___oldMariner.getTileY() == tileLocation.Y)
                {
                    string playerTerm = Game1.content.LoadString("Strings\\Locations:Beach_Mariner_Player_" + (who.IsMale ? "Male" : "Female"));
                    if (who.specialItems.Contains(460) && !Utility.doesItemWithThisIndexExistAnywhere(460, false))
                    {
                        for (int i = who.specialItems.Count - 1; i >= 0; i--)
                        {
                            if (who.specialItems[i] == 460)
                            {
                                who.specialItems.RemoveAt(i);
                            }
                        }
                    }
                    if (who.specialItems.Contains(460))
                    {
                        Game1.drawObjectDialogue(Game1.parseText(Game1.content.LoadString("Strings\\Locations:Beach_Mariner_PlayerHasItem", playerTerm)));
                    }
                    else if (who.hasAFriendWithHeartLevel(10, true) && who.houseUpgradeLevel == 0)
                    {
                        Game1.drawObjectDialogue(Game1.parseText(Game1.content.LoadString("Strings\\Locations:Beach_Mariner_PlayerNotUpgradedHouse", playerTerm)));
                    }
                    else if (who.hasAFriendWithHeartLevel(10, true))
                    {
                        Response[] answers = new Response[]
                        {
                    new Response("Buy", Game1.content.LoadString("Strings\\Locations:Beach_Mariner_PlayerBuyItem_AnswerYes")),
                    new Response("Not", Game1.content.LoadString("Strings\\Locations:Beach_Mariner_PlayerBuyItem_AnswerNo"))
                        };
                        __instance.createQuestionDialogue(Game1.parseText(Game1.content.LoadString("Strings\\Locations:Beach_Mariner_PlayerBuyItem_Question", playerTerm)), answers, "mariner");
                    }
                    else
                    {
                        Game1.drawObjectDialogue(Game1.parseText(Game1.content.LoadString("Strings\\Locations:Beach_Mariner_PlayerNoRelationship", playerTerm)));
                    }
                    __result = true;
                    return false;
                }
            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(Beach_checkAction_Prefix)}:\n{ex}", LogLevel.Error);
            }
            return true;
        }

        public static bool GameLocation_performAction_Prefix(GameLocation __instance, string action, Farmer who, ref bool __result, Location tileLocation)
        {
            try
            {
                if (action != null && who.IsLocalPlayer)
                {
                    string[] actionParams = action.Split(new char[]
                    {
                    ' '
                    });
                    string text = actionParams[0];
                    Regex pattern = new Regex(@"Crib[0-9][0-9]*");
                    if (pattern.IsMatch(text))
                    {
                        int crib = int.Parse(text.Substring(4));
                        Monitor.Log($"Acting on crib {crib+1}");

                        Microsoft.Xna.Framework.Rectangle rect = new Microsoft.Xna.Framework.Rectangle((ModEntry.config.ExistingKidsRoomOffsetX+15)*64 + (3 * crib * 64),(ModEntry.config.ExistingKidsRoomOffsetY+2)*64,3*64,4*64);
                        using (NetCollection<NPC>.Enumerator enumerator = __instance.characters.GetEnumerator())
                        {
                            while (enumerator.MoveNext())
                            {
                                NPC j = enumerator.Current;
                                if (j is Child)
                                {
                                    if (rect.Intersects(j.GetBoundingBox()))
                                    {
                                        if ((j as Child).Age == 1)
                                        {
                                            Monitor.Log($"Tossing {j.Name}");
                                            (j as Child).toss(who);
                                        }
                                        else if ((j as Child).Age == 0)
                                        {
                                            Monitor.Log($"{j.Name} is sleeping");
                                            Game1.drawObjectDialogue(Game1.parseText(Game1.content.LoadString("Strings\\Locations:FarmHouse_Crib_NewbornSleeping", j.displayName)));
                                        }
                                        else if ((j as Child).isInCrib() && (j as Child).Age == 2)
                                        {
                                            Monitor.Log($"acting on {j.Name}");
                                            return j.checkAction(who, __instance);
                                        }
                                        __result = true;
                                        return false;
                                    }
                                }
                            }
                        }
                        __result = true;
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(ManorHouse_performAction_Prefix)}:\n{ex}", LogLevel.Error);
            }
            return true;
        }

        public static bool ManorHouse_performAction_Prefix(ManorHouse __instance, string action, Farmer who, ref bool __result)
        {
            try
            {
                Misc.ResetSpouses(who);
                Dictionary<string, NPC> spouses = Misc.GetSpouses(who, -1);
                if (action != null && who.IsLocalPlayer && !Game1.player.divorceTonight && (Game1.player.isMarried() || spouses.Count > 0))
                {
                    string a = action.Split(new char[]
                    {
                    ' '
                    })[0];
                    if (a == "DivorceBook")
                    {
                        string s2 = Game1.content.LoadStringReturnNullIfNotFound("Strings\\Locations:ManorHouse_DivorceBook_Question_" + Game1.player.spouse);
                        if (s2 == null)
                        {
                            s2 = Game1.content.LoadStringReturnNullIfNotFound("Strings\\Locations:ManorHouse_DivorceBook_Question");
                        }
                        List<Response> responses = new List<Response>();
                        foreach (NPC spouse in spouses.Values)
                        {
                            responses.Add(new Response(spouse.name, spouse.displayName));
                        }
                        responses.Add(new Response("No", Game1.content.LoadString("Strings\\Lexicon:QuestionDialogue_No")));
                        __instance.createQuestionDialogue(s2, responses.ToArray(), "divorce");
                        __result = true;
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(ManorHouse_performAction_Prefix)}:\n{ex}", LogLevel.Error);
            }
            return true;
        }

        public static bool Event_endBehaviors_Postfix(string[] split)
        {
            try
            {
                if (split != null && split.Length > 1)
                {
                    string text = split[1];
                    if (text == "wedding")
                    {
                        Misc.PlaceSpousesInFarmhouse(Utility.getHomeOfFarmer(Game1.player));
                    }
                }
            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(Event_endBehaviors_Postfix)}:\n{ex}", LogLevel.Error);
            }
            return true;
        }
    
        public static void GameLocation_checkEventPrecondition_Prefix(ref string precondition)
        {
            try
            {
                string[] split = precondition.Split('/');
                int eventId;
                if (!int.TryParse(split[0], out eventId))
                {
                    return;
                }
                if (Game1.player.eventsSeen.Contains(eventId))
                {
                    return;
                }
                Dictionary<string, NPC> spouses = Misc.GetSpouses(Game1.player, -1);
                for (int i = 1; i < split.Length; i++)
                {
                    if (split[i][0] == 'O')
                    {
                        string name = split[i].Substring(2);
                        if (Game1.player.spouse != name && spouses.ContainsKey(name))
                        {
                            Monitor.Log($"Got unofficial spouse requirement for event: {name}, switching event condition to isSpouse O");
                            split[i] = $"o {name}";
                        }
                    }
                    else if (split[i][0] == 'o')
                    {
                        string name = split[i].Substring(2);
                        if (Game1.player.spouse != name && spouses.ContainsKey(name))
                        {
                            Monitor.Log($"Got unofficial spouse barrier to event: {name}, switching event condition to notSpouse o");
                            split[i] = $"O {name}";
                        }
                    }
                }
                precondition = string.Join("/", split);
            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(GameLocation_checkEventPrecondition_Prefix)}:\n{ex}", LogLevel.Error);
            }
        }

        public static void FarmHouse_performTenMinuteUpdate_Postfix(FarmHouse __instance, int timeOfDay)
        {
            try
            {
                if (__instance.owner == null)
                    return;

                List<string> mySpouses = Misc.GetSpouses(__instance.owner, 1).Keys.ToList();
                if (Game1.IsMasterGame && Game1.timeOfDay >= 2200 && Game1.IsMasterGame)
                {
                    int upgradeLevel = __instance.upgradeLevel;
                    List<string> roomSpouses = mySpouses.FindAll((s) => Maps.roomIndexes.ContainsKey(s) || Maps.tmxSpouseRooms.ContainsKey(s));
                    List<string> bedSpouses = mySpouses.FindAll((s) => ModEntry.config.RoommateRomance || !__instance.owner.friendshipData[s].RoommateMarriage);
                    foreach (NPC c in __instance.characters)
                    {
                        if (c.isMarried())
                        {
                            string spouseName = c.Name;

                            if (Misc.GetSpouses(Game1.player,1).ContainsKey(spouseName))
                            {
                                c.checkForMarriageDialogue(timeOfDay, __instance);
                            }

                            Point bedSpot;
                            if (timeOfDay >= 2200)
                            {
                                if (!bedSpouses.Contains(c.Name))
                                {
                                    if (!roomSpouses.Exists((n) => n == spouseName))
                                    {
                                        bedSpot = __instance.getRandomOpenPointInHouse(ModEntry.myRand);
                                    }
                                    else
                                    {
                                        int offset = roomSpouses.IndexOf(spouseName) * 7;
                                        Vector2 spot = (upgradeLevel == 1) ? new Vector2(32f, 5f) : new Vector2(38f, 14f);
                                        bedSpot = new Point((int)spot.X + offset, (int)spot.Y);
                                    }

                                }
                                else
                                {
                                    int bedWidth = Misc.GetBedWidth(__instance);
                                    bool up = upgradeLevel > 1;

                                    Point bedStart = new Point(21 - (up ? (bedWidth / 2) - 1 : 0) + (up ? 6 : 0), 2 + (up ? 9 : 0));
                                    int x = 1 + (int)((bedSpouses.IndexOf(spouseName) + 1) / (float)(bedSpouses.Count + 1) * (bedWidth - 2));
                                    bedSpot = new Point(bedStart.X + x, bedStart.Y + 2);

                                }

                                c.controller = null;
                                if (c.Position != Misc.GetSpouseBedPosition(__instance, bedSpouses, c.Name) && (!Misc.IsInBed(__instance,c.GetBoundingBox()) || !Kissing.kissingSpouses.Contains(c.Name)))
                                {
                                    c.controller = new PathFindController(c, __instance, bedSpot, 0, new PathFindController.endBehavior(FarmHouse.spouseSleepEndFunction));
                                    if (c.controller.pathToEndPoint == null || !__instance.isTileOnMap(c.controller.pathToEndPoint.Last<Point>().X, c.controller.pathToEndPoint.Last<Point>().Y))
                                    {
                                        c.controller = null;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(FarmHouse_performTenMinuteUpdate_Postfix)}:\n{ex}", LogLevel.Error);
            }
        }

        public static void Desert_getDesertMerchantTradeStock_Postfix(Farmer who, ref Dictionary<ISalable, int[]> __result)
        {
            try
            {
                if (who != null && who.getFriendshipHeartLevelForNPC("Krobus") >= 10 && !who.friendshipData["Krobus"].RoommateMarriage && who.houseUpgradeLevel >= 1 && (who.isMarried() || who.isEngaged()) && !who.hasItemInInventory(808, 1, 0))
                {
                    ISalable i = new StardewValley.Object(808, 1, false, -1, 0);
                    __result.Add(i, new int[]
                    {
                        0,
                        1,
                        769,
                        200
                    });
                }
            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(Desert_getDesertMerchantTradeStock_Postfix)}:\n{ex}", LogLevel.Error);
            }
        }

    }
}