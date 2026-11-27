using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;
using StardewValley.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection;
using xTile.Dimensions;
using Object = StardewValley.Object;
using StardewValley.TokenizableStrings;

namespace SelfServe
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry
    {
        [HarmonyPatch(typeof(Utility), nameof(Utility.TryOpenShopMenu), new Type[] { typeof(string), typeof(GameLocation), typeof(Microsoft.Xna.Framework.Rectangle?), typeof(int?), typeof(bool), typeof(bool), typeof(Action<string>) })]
        private static class Utility_TryOpenShopMenu_Patch
        {
            public static void Prefix(ref bool forceOpen)
            {
                if(Config.EnableMod)
                    forceOpen = true;
            }
        }
        [HarmonyPatch(typeof(GameLocation), nameof(GameLocation.blacksmith))]
        private static class GameLocation_blacksmith_Patch
        {
            public static bool Prefix(GameLocation __instance, ref bool __result)
            {
                if(!Config.EnableMod) 
                    return true;

                if (Game1.player.toolBeingUpgraded.Value != null && Game1.player.daysLeftForToolUpgrade.Value <= 0)
                {
                    if (Game1.player.freeSpotsInInventory() > 0 || Game1.player.toolBeingUpgraded.Value is GenericTool)
                    {
                        Tool tool = Game1.player.toolBeingUpgraded.Value;
                        Game1.player.toolBeingUpgraded.Value = null;
                        Game1.player.hasReceivedToolUpgradeMessageYet = false;
                        Game1.player.holdUpItemThenMessage(tool, true);
                        if (tool is GenericTool)
                        {
                            tool.actionWhenClaimed();
                        }
                        else
                        {
                            Game1.player.addItemToInventoryBool(tool, false);
                        }
                        if (Game1.player.team.useSeparateWallets.Value && tool.UpgradeLevel == 4)
                        {
                            Game1.Multiplayer.globalChatInfoMessage("IridiumToolUpgrade", new string[]
                            {
                                    Game1.player.Name,
                                    TokenStringBuilder.ToolName(tool.QualifiedItemId, tool.UpgradeLevel)
                            });
                        }
                    }
                    else
                    {
                        Game1.DrawDialogue(Game1.getCharacterFromName("Clint"), "Data\\ExtraDialogue:Clint_NoInventorySpace");
                    }
                }
                else
                {
                    bool hasGeode = false;
                    using (IEnumerator<Item> enumerator2 = Game1.player.Items.GetEnumerator())
                    {
                        while (enumerator2.MoveNext())
                        {
                            if (Utility.IsGeode(enumerator2.Current, false))
                            {
                                hasGeode = true;
                                break;
                            }
                        }
                    }
                    Response[] responses;
                    if (hasGeode)
                    {
                        responses = new Response[]
                        {
                                new Response("Shop", Game1.content.LoadString("Strings\\Locations:Blacksmith_Clint_Shop")),
                                new Response("Upgrade", Game1.content.LoadString("Strings\\Locations:Blacksmith_Clint_Upgrade")),
                                new Response("Process", Game1.content.LoadString("Strings\\Locations:Blacksmith_Clint_Geodes")),
                                new Response("Leave", Game1.content.LoadString("Strings\\Locations:Blacksmith_Clint_Leave"))
                        };
                    }
                    else
                    {
                        responses = new Response[]
                        {
                                new Response("Shop", Game1.content.LoadString("Strings\\Locations:Blacksmith_Clint_Shop")),
                                new Response("Upgrade", Game1.content.LoadString("Strings\\Locations:Blacksmith_Clint_Upgrade")),
                                new Response("Leave", Game1.content.LoadString("Strings\\Locations:Blacksmith_Clint_Leave"))
                        };
                    }
                    __instance.createQuestionDialogue("", responses, "Blacksmith");
                }
                __result = true;
                return false;
            }
        }
        [HarmonyPatch(typeof(GameLocation), nameof(GameLocation.animalShop))]
        private static class GameLocation_animalShop_Patch
        {
            public static bool Prefix(GameLocation __instance, ref bool __result)
            {
                if(!Config.EnableMod) 
                    return true;
                List<Response> options = new List<Response>
                        {
                            new Response("Supplies", Game1.content.LoadString("Strings\\Locations:AnimalShop_Marnie_Supplies")),
                            new Response("Purchase", Game1.content.LoadString("Strings\\Locations:AnimalShop_Marnie_Animals")),
                            new Response("Leave", Game1.content.LoadString("Strings\\Locations:AnimalShop_Marnie_Leave"))
                        };
                if ((Utility.getAllPets().Count == 0 && Game1.year >= 2) || Game1.player.mailReceived.Contains("MarniePetAdoption") || Game1.player.mailReceived.Contains("MarniePetRejectedAdoption"))
                {
                    options.Insert(2, new Response("Adopt", Game1.content.LoadString("Strings\\1_6_Strings:AdoptPets")));
                }
                __instance.createQuestionDialogue("", options.ToArray(), "Marnie");
                __result = true;
                return false;
            }
        }
        [HarmonyPatch(typeof(GameLocation), nameof(GameLocation.carpenters))]
        private static class GameLocation_carpenters_Patch
        {
            public static bool Prefix(GameLocation __instance, ref bool __result)
            {
                if(!Config.EnableMod) 
                    return true;
                if (Game1.player.daysUntilHouseUpgrade.Value < 0 && !Game1.IsThereABuildingUnderConstruction("Robin"))
                {
                    List<Response> options = new List<Response>();
                    options.Add(new Response("Shop", Game1.content.LoadString("Strings\\Locations:ScienceHouse_CarpenterMenu_Shop")));
                    if (Game1.IsMasterGame)
                    {
                        if (Game1.player.HouseUpgradeLevel < 3)
                        {
                            options.Add(new Response("Upgrade", Game1.content.LoadString("Strings\\Locations:ScienceHouse_CarpenterMenu_UpgradeHouse")));
                        }
                        else if ((Game1.MasterPlayer.mailReceived.Contains("ccIsComplete") || Game1.MasterPlayer.mailReceived.Contains("JojaMember") || Game1.MasterPlayer.hasCompletedCommunityCenter()) && Game1.RequireLocation<Town>("Town", false).daysUntilCommunityUpgrade.Value <= 0)
                        {
                            if (!Game1.MasterPlayer.mailReceived.Contains("pamHouseUpgrade"))
                            {
                                options.Add(new Response("CommunityUpgrade", Game1.content.LoadString("Strings\\Locations:ScienceHouse_CarpenterMenu_CommunityUpgrade")));
                            }
                            else if (!Game1.MasterPlayer.mailReceived.Contains("communityUpgradeShortcuts"))
                            {
                                options.Add(new Response("CommunityUpgrade", Game1.content.LoadString("Strings\\Locations:ScienceHouse_CarpenterMenu_CommunityUpgrade")));
                            }
                        }
                    }
                    else if (Game1.player.HouseUpgradeLevel < 3)
                    {
                        options.Add(new Response("Upgrade", Game1.content.LoadString("Strings\\Locations:ScienceHouse_CarpenterMenu_UpgradeCabin")));
                    }
                    if (Game1.player.HouseUpgradeLevel >= 2)
                    {
                        if (Game1.IsMasterGame)
                        {
                            options.Add(new Response("Renovate", Game1.content.LoadString("Strings\\Locations:ScienceHouse_CarpenterMenu_RenovateHouse")));
                        }
                        else
                        {
                            options.Add(new Response("Renovate", Game1.content.LoadString("Strings\\Locations:ScienceHouse_CarpenterMenu_RenovateCabin")));
                        }
                    }
                    options.Add(new Response("Construct", Game1.content.LoadString("Strings\\Locations:ScienceHouse_CarpenterMenu_Construct")));
                    options.Add(new Response("Leave", Game1.content.LoadString("Strings\\Locations:ScienceHouse_CarpenterMenu_Leave")));
                    __instance.createQuestionDialogue(Game1.content.LoadString("Strings\\Locations:ScienceHouse_CarpenterMenu"), options.ToArray(), "carpenter");
                }
                else
                {
                    Utility.TryOpenShopMenu("Carpenter", "Robin", true);
                }
                __result = true;
                return false;
            }
        }
    }
}