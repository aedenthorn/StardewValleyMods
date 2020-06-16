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
                if(Game1.player != null && Utility.getHomeOfFarmer(Game1.player) != null && __instance.Equals(Utility.getHomeOfFarmer(Game1.player)))
                {
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
                        List<NPC> spouses = Maps.spousesWithRooms;
                        if(spouses != null && spouses.Count > 0)
                        {
                            for(int i = 0; i < spouses.Count; i++)
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
                if (Game1.player != null && __instance.Equals(Utility.getHomeOfFarmer(Game1.player)))
                {
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
                        List<NPC> spouses = Maps.spousesWithRooms;
                        if (spouses != null && spouses.Count > 0)
                        {
                            for (int i = 0; i < spouses.Count; i++)
                            {
                                if (__instance.upgradeLevel > 1)
                                {
                                    __result.Add(new Microsoft.Xna.Framework.Rectangle(41 + i * 7, 13, 7, 6));
                                }
                                else
                                {
                                    __result.Add(new Microsoft.Xna.Framework.Rectangle(35 + i * 7, 4, 7, 6));
                                }
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


                if (!(__instance is FarmHouse) || !__instance.Name.StartsWith("FarmHouse") || __instance != Utility.getHomeOfFarmer(Game1.player) || Misc.GetAllSpouses().Count == 0)
                {
                    return;
                }
                ModEntry.PMonitor.Log("reset farmhouse state");


                Farmer f = __instance.owner;
                Misc.ResetSpouses(f);

                if (f.currentLocation == __instance && Misc.IsInBed(f.GetBoundingBox()))
                {
                    f.position.Value = Misc.GetSpouseBedPosition(__instance, "Game1.player");
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
                if (action != null && who.IsLocalPlayer && !Game1.player.divorceTonight && (Game1.player.isMarried() || ModEntry.spouses.Count > 0))
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
                        if(who.spouse != null)
                            responses.Add(new Response(who.spouse, who.spouse));
                        foreach (string spouse in ModEntry.spouses.Keys)
                        {
                            responses.Add(new Response(spouse, ModEntry.spouses[spouse].displayName));
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
                        Misc.PlaceSpousesInFarmhouse();
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
                for (int i = 1; i < split.Length; i++)
                {
                    if (split[i][0] == 'O')
                    {
                        string name = split[i].Substring(2);
                        if (Game1.player.spouse != name && ModEntry.spouses.ContainsKey(name))
                        {
                            Monitor.Log($"Got unofficial spouse requirement for event: {name}, switching event condition to isSpouse O");
                            split[i] = $"o {name}";
                        }
                    }
                    else if (split[i][0] == 'o')
                    {
                        string name = split[i].Substring(2);
                        if (Game1.player.spouse != name && ModEntry.spouses.ContainsKey(name))
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
                foreach (NPC c in __instance.characters)
                {
                    if (c.isMarried())
                    {
                        string spouseName = c.Name;
                        int upgradeLevel = __instance.upgradeLevel;
                        Point bedSpot;
                        if (Game1.player.friendshipData[spouseName].RoommateMarriage && !ModEntry.config.RoommateRomance)
                        {
                            List<string> mySpouses = Misc.GetAllSpouseNamesOfficialFirst(Game1.player);
                            List<string> roomSpouses = mySpouses.FindAll((s) => Maps.roomIndexes.ContainsKey(s) || Maps.tmxSpouseRooms.ContainsKey(s));

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
                            Misc.SetBedmates();

                            int bedWidth;
                            bool up = upgradeLevel > 1;
                            if (ModEntry.config.CustomBed)
                            {
                                bedWidth = Math.Min(up ? 9 : 6, Math.Max(ModEntry.config.BedWidth, 3));
                            }
                            else
                            {
                                bedWidth = 3;
                            }

                            Point bedStart = new Point(21 - (up ? (bedWidth / 2) - 1 : 0) + (up ? 6 : 0), 2 + (up ? 9 : 0));
                            int x = (int)(ModEntry.allBedmates.IndexOf(spouseName) / (float)ModEntry.allBedmates.Count * (bedWidth - 1));
                            bedSpot = new Point(bedStart.X + x, bedStart.Y + 2);

                        }
                        if (Game1.IsMasterGame && Game1.timeOfDay >= 2200 && Game1.IsMasterGame && c.position != Misc.GetSpouseBedPosition(__instance,spouseName) && (timeOfDay == 2200 || (c.controller == null && timeOfDay % 100 % 30 == 0)))
                        {
                            c.controller = null;
                            c.controller = new PathFindController(c, __instance, bedSpot, 0, new PathFindController.endBehavior(FarmHouse.spouseSleepEndFunction));
                            if (c.controller.pathToEndPoint == null || !__instance.isTileOnMap(c.controller.pathToEndPoint.Last<Point>().X, c.controller.pathToEndPoint.Last<Point>().Y))
                            {
                                c.controller = null;
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
    }
}