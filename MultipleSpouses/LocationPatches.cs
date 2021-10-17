using Microsoft.Xna.Framework;
using Netcode;
using StardewModdingAPI;
using StardewValley;
using StardewValley.BellsAndWhistles;
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

        public static void FarmHouse_checkAction_Postfix(FarmHouse __instance, Location tileLocation)
        {
            try
            {
                if (__instance.map.GetLayer("Buildings").Tiles[tileLocation] != null)
                {
                    int tileIndex = __instance.map.GetLayer("Buildings").Tiles[tileLocation].TileIndex;
                    if (tileIndex == 2173 && Game1.player.eventsSeen.Contains(463391) && Game1.player.friendshipData.ContainsKey("Emily") && Game1.player.friendshipData["Emily"].IsMarried())
                    {
                        TemporaryAnimatedSprite t = __instance.getTemporarySpriteByID(5858585);
                        if (t != null && t is EmilysParrot)
                        {
                            (t as EmilysParrot).doAction();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(FarmHouse_getWalls_Postfix)}:\n{ex}", LogLevel.Error);
            }
        }
        
        public static void FarmHouse_getWalls_Postfix(FarmHouse __instance, ref List<Microsoft.Xna.Framework.Rectangle> __result)
        {
            try
            {
                //Monitor.Log($"Getting walls for {__instance}");
                if (__instance.owner == null)
                    return;
                //int ecribs = Math.Max(ModEntry.config.ExtraCribs, 0);
                int espace = Math.Max(ModEntry.config.ExtraKidsRoomWidth, 0);
                //int ebeds = Math.Max(ModEntry.config.ExtraKidsBeds, 0);

                if (__instance.upgradeLevel > 1 && espace > 0)
                {
                    int x = espace;
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
                //Monitor.Log($"Getting floors for {__instance}");
                if (__instance.owner == null)
                    return;
                //int ecribs = Math.Max(ModEntry.config.ExtraCribs, 0);
                int espace = Math.Max(ModEntry.config.ExtraKidsRoomWidth, 0);
                //int ebeds = Math.Max(ModEntry.config.ExtraKidsBeds, 0);
                if (__instance.upgradeLevel > 1 && espace > 0)
                {
                    int x = espace;
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
                                __result.Add(new Microsoft.Xna.Framework.Rectangle(35 + i * 7, 13, 6, 6));
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
        


        public static void FarmHouse_updateFarmLayout_Postfix(ref FarmHouse __instance)
        {
            try
            {
                if (Misc.ChangingKidsRoom())
                {
                    Monitor.Log($"Changing kids room for {__instance}");
                    if (__instance.upgradeLevel > 1 && __instance.upgradeLevel < 4)
                    {
                        //NPCPatches.SetCribs(__instance);
                        Maps.ExpandKidsRoom(__instance);
                    }
                }
            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(FarmHouse_resetLocalState_Postfix)}:\n{ex}", LogLevel.Error);
            }
        }
        public static void FarmHouse_resetLocalState_Postfix(ref FarmHouse __instance)
        {
            try
            {
                Farmer f = __instance.owner;

                if (f == null)
                {
                    return;
                }
                ModEntry.PMonitor.Log($"reset farmhouse state - upgrade level {__instance.upgradeLevel}");

                Misc.ResetSpouses(f);

                if (f.currentLocation == __instance && Misc.IsInBed(__instance, f.GetBoundingBox()))
                {
                    f.position.Value = Misc.GetFarmerBedPosition(__instance);
                }
                if(__instance.upgradeLevel > 0 && __instance.upgradeLevel < 4)
                {
                    Maps.BuildSpouseRooms(__instance);
                    __instance.setFloors();
                }
                if (Misc.ChangingKidsRoom())
                {
                    if(__instance.upgradeLevel > 1 && __instance.upgradeLevel < 4)
                    {
                        //NPCPatches.SetCribs(__instance);
                        Maps.ExpandKidsRoom(__instance);
                    }
                }
                if(Misc.GetSpouses(f,0).ContainsKey("Sebastian") && Game1.netWorldState.Value.hasWorldStateID("sebastianFrog"))
                {
                    if (Game1.random.NextDouble() < 0.1 && Game1.timeOfDay > 610)
                    {
                        DelayedAction.playSoundAfterDelay("croak", 1000, null, -1);
                    }
                }
            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(FarmHouse_resetLocalState_Postfix)}:\n{ex}", LogLevel.Error);
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

        public static bool Beach_checkAction_Prefix(Beach __instance, Location tileLocation, xTile.Dimensions.Rectangle viewport, Farmer who, ref bool __result, NPC ___oldMariner)
        {
            try
            {
                if (___oldMariner != null && ___oldMariner.getTileX() == tileLocation.X && ___oldMariner.getTileY() == tileLocation.Y)
                {
                    string playerTerm = Game1.content.LoadString("Strings\\Locations:Beach_Mariner_Player_" + (who.IsMale ? "Male" : "Female"));
                    if (who.hasAFriendWithHeartLevel(10, true) && who.houseUpgradeLevel == 0)
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
                if (action.Split(' ')[0] == "Crib" && who.IsLocalPlayer)
                {
                    Monitor.Log($"Acting on crib tile {tileLocation}");

                    FarmHouse farmHouse = __instance as FarmHouse;
                    Microsoft.Xna.Framework.Rectangle? crib_location = farmHouse.GetCribBounds();

                    if (crib_location == null)
                        return true;

                    for (int i = 0; i <= Misc.GetExtraCribs(); i++)
                    {
                        Microsoft.Xna.Framework.Rectangle rect = new Microsoft.Xna.Framework.Rectangle(crib_location.Value.X + i * 3, crib_location.Value.Y, crib_location.Value.Width, crib_location.Value.Height);
                        if(rect.Contains(tileLocation.X, tileLocation.Y))
                        {
                            Monitor.Log($"Acting on crib idx {i}");
                            using (List<NPC>.Enumerator enumerator = __instance.characters.GetEnumerator())
                            {
                                while (enumerator.MoveNext())
                                {
                                    NPC j = enumerator.Current;
                                    if (j is Child)
                                    {
                                        if (rect.Contains(j.getTileLocationPoint()))
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
                        __instance.createQuestionDialogue(s2, responses.ToArray(), Divorce.afterDialogueBehavior);
                        //__instance.createQuestionDialogue(s2, responses.ToArray(), "divorce");
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

        internal static void GameLocation_answerDialogue_prefix(GameLocation __instance, Response answer)
        {
            try
            {
                if (answer.responseKey.StartsWith("divorce_"))
                    __instance.afterQuestion = Divorce.afterDialogueBehavior;

            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(GameLocation_answerDialogue_prefix)}:\n{ex}", LogLevel.Error);
            }
        }

   
        public static void GameLocation_checkEventPrecondition_Prefix(ref string precondition)
        {
            try
            {
                if (precondition == null || precondition == "")
                    return;
                string[] split = precondition.Split('/');
                if (split.Length == 0)
                    return;
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
                    if (split[i].Length == 0)
                        continue;

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

                                    Point bedStart = __instance.GetSpouseBed().GetBedSpot();
                                    int x = 1 + (int)(bedSpouses.IndexOf(spouseName) / (float)(bedSpouses.Count) * (bedWidth - 1));
                                    bedSpot = new Point(bedStart.X + x, bedStart.Y);

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