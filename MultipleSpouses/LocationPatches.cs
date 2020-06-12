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


        public static bool GameLocation_updateMap_Prefix(ref GameLocation __instance, string ___loadedMapPath)
        {
            try
            {
                if (__instance is FarmHouse && __instance.Name.StartsWith("FarmHouse"))
                {
                    FarmHouse farmHouse = __instance as FarmHouse;
                    if (farmHouse.owner == null)
                        return true;
                    bool showSpouse = ModEntry.spouses.Count > 0 || farmHouse.owner.spouse != null;
                    __instance.mapPath.Value = "Maps\\" + __instance.Name + ((farmHouse.upgradeLevel == 0) ? "" : ((farmHouse.upgradeLevel == 3) ? "2" : string.Concat(farmHouse.upgradeLevel))) + (showSpouse ? "_marriage" : "");

                    if (!object.Equals(__instance.mapPath.Value, ___loadedMapPath))
                    {
                        __instance.reloadMap();
                    }
                    return false;
                }
            }
            catch (Exception ex)
            {
                Monitor.Log($"Failed in {nameof(GameLocation_updateMap_Prefix)}:\n{ex}", LogLevel.Error);
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

                FarmHouse farmHouse = __instance as FarmHouse;

                NPCPatches.SetCribs(farmHouse);

                Farmer f = farmHouse.owner;
                ModEntry.ResetSpouses(f);

                if (f.currentLocation == farmHouse && ModEntry.IsInBed(f.GetBoundingBox()))
                {
                    f.position.Value = ModEntry.GetSpouseBedLocation("Game1.player");
                }
                if (ModEntry.config.CustomBed && !ModEntry.bedMadeToday)
                {
                    Maps.ReplaceBed();
                    ModEntry.bedMadeToday = true;
                }

                if (ModEntry.config.BuildAllSpousesRooms)
                {
                    Maps.BuildSpouseRooms(farmHouse);
                }
                if (!ModEntry.kidsRoomExpandedToday && farmHouse.upgradeLevel > 1 && (ModEntry.config.ExtraCribs > 0 || ModEntry.config.ExtraKidsBeds > 0))
                {
                    ModEntry.kidsRoomExpandedToday = true;
                    Maps.ExpandKidsRoom(farmHouse);
                }
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
    }
}