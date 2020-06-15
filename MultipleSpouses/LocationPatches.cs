using Microsoft.Xna.Framework;
using Netcode;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Characters;
using StardewValley.Locations;
using System;
using System.Collections.Generic;
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
                if(Game1.player != null && __instance.Equals(Utility.getHomeOfFarmer(Game1.player)) && __instance.upgradeLevel > 1 && ModEntry.config.ExtraCribs + ModEntry.config.ExtraKidsBeds > 0)
                {
                    //Monitor.Log("adding walls to farmhouse");
                    int x = (ModEntry.config.ExtraCribs * 3) + (ModEntry.config.ExtraKidsBeds * 4);
                    __result.Add(new Microsoft.Xna.Framework.Rectangle(28, 1, x, 3));
                }
            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(FarmHouse_getWalls_Postfix)}:\n{ex}", LogLevel.Error);
            }
        }
        

        public static bool FarmHouse_updateMap_Prefix(ref FarmHouse __instance, string ___loadedMapPath)
        {
            try
            {
                //ModEntry.ResetSpouses(Game1.player);
            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(FarmHouse_updateMap_Prefix)}:\n{ex}", LogLevel.Error);
            }
            return true;
        }

        public static void GameLocation_resetLocalState_Postfix(GameLocation __instance)
        {
            try
            {

                if (__instance is Beach && ModEntry.config.BuyPendantsAnytime)
                {
                    ModEntry.PHelper.Reflection.GetField<NPC>(__instance, "oldMariner").SetValue(new NPC(new AnimatedSprite("Characters\\Mariner", 0, 16, 32), new Vector2(80f, 5f) * 64f, 2, "Old Mariner", null));
                    return;
                }

                if (!(__instance is FarmHouse) || !__instance.Name.StartsWith("FarmHouse") || __instance != Utility.getHomeOfFarmer(Game1.player) || ModEntry.GetAllSpouses().Count == 0)
                {
                    return;
                }
                ModEntry.PMonitor.Log("reset farm state");
                ModEntry.PHelper.Content.InvalidateCache("Maps/FarmHouse1_marriage");
                ModEntry.PHelper.Content.InvalidateCache("Maps/FarmHouse2");
                ModEntry.PHelper.Content.InvalidateCache("Maps/FarmHouse2_marriage");

                FarmHouse farmHouse = __instance as FarmHouse;

                NPCPatches.SetCribs(farmHouse);

                Farmer f = farmHouse.owner;
                ModEntry.ResetSpouses(f);

                if (f.currentLocation == farmHouse && ModEntry.IsInBed(f.GetBoundingBox()))
                {
                    f.position.Value = ModEntry.GetSpouseBedLocation("Game1.player");
                }
                if (ModEntry.config.CustomBed)
                {
                    Maps.ReplaceBed();
                }

                if (farmHouse.upgradeLevel > 1 && (ModEntry.config.ExtraCribs > 0 || ModEntry.config.ExtraKidsBeds > 0))
                {
                    Maps.ExpandKidsRoom(farmHouse);
                }
                farmHouse.showSpouseRoom();
                Maps.BuildSpouseRooms(farmHouse);

            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(GameLocation_resetLocalState_Postfix)}:\n{ex}", LogLevel.Error);
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
                ModEntry.ResetSpouses(who);
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
                        ModEntry.PlaceSpousesInFarmhouse();
                    }
                }
            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(ManorHouse_performAction_Prefix)}:\n{ex}", LogLevel.Error);
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
                Monitor.Log($"Failed in {nameof(ManorHouse_performAction_Prefix)}:\n{ex}", LogLevel.Error);
            }
        }
    }
}