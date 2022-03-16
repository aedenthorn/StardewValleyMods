using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Objects;
using StardewValley.Tools;
using StardewValley.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using xTile.Dimensions;
using Object = StardewValley.Object;

namespace SelfServe
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry
    {

        private static bool IslandSouth_checkAction_Prefix(IslandSouth __instance, Location tileLocation)
        {
            if (!Config.EnableMod)
                return true;
            if (tileLocation.X == 14 && tileLocation.Y == 22)
            {
                Dictionary<ISalable, int[]> stock = new Dictionary<ISalable, int[]>();
                Utility.AddStock(stock, new Object(Vector2.Zero, 873, int.MaxValue), 300, -1);
                Utility.AddStock(stock, new Object(Vector2.Zero, 346, int.MaxValue), 250, -1);
                Utility.AddStock(stock, new Object(Vector2.Zero, 303, int.MaxValue), 500, -1);
                Utility.AddStock(stock, new Object(Vector2.Zero, 459, int.MaxValue), 400, -1);
                Utility.AddStock(stock, new Object(Vector2.Zero, 612, int.MaxValue), 200, -1);
                Object wine = new Object(Vector2.Zero, 348, int.MaxValue);
                Object mango = new Object(834, 1, false, -1, 0);
                wine.Price = mango.Price * 3;
                wine.Name = mango.Name + " Wine";
                wine.preserve.Value = new Object.PreserveType?(Object.PreserveType.Wine);
                wine.preservedParentSheetIndex.Value = mango.ParentSheetIndex;
                wine.Quality = 2;
                Utility.AddStock(stock, wine, 2500, -1);
                if (!Game1.player.cookingRecipes.ContainsKey("Tropical Curry"))
                {
                    Utility.AddStock(stock, new Object(907, 1, true, -1, 0), 1000, -1);
                }
                string name = null;
                foreach (var c in __instance.characters)
                {
                    if (c.Name == "Gus")
                    {
                        name = "Gus";
                        break;
                    }
                }
                Game1.activeClickableMenu = new ShopMenu(stock, 0, name, null, null, "ResortBar");
            }
            return true;
        }
        private static bool GameLocation_performAction_Prefix(GameLocation __instance, string action, ref bool __result)
        {
            if (!Config.EnableMod)
                return true;
            string[] actionParams = action.Split(' ');
            string text = actionParams[0];
            List<Response> options = new List<Response>();
            switch (text)
            {
                case "Buy":
                    __instance.openShopMenu(actionParams[1]);
                    __result = true;
                    return false;
                    
                case "Saloon":
                    if (!Config.SaloonShop)
                        return true;
                    Game1.activeClickableMenu = new ShopMenu(Utility.getSaloonStock(), 0, "Gus", delegate (ISalable item, Farmer farmer, int amount)
                    {
                        Game1.player.team.synchronizedShopStock.OnItemPurchased(SynchronizedShopStock.SynchedShop.Saloon, item, amount);
                        return false;
                    }, null, null);
                    __result = true;
                    return false;
                    
                case "Carpenter":
                    if (!Config.CarpenterShop)
                        return true;
                    if (Game1.player.daysUntilHouseUpgrade.Value < 0 && !Game1.getFarm().isThereABuildingUnderConstruction())
                    {
                        options.Add(new Response("Shop", Game1.content.LoadString("Strings\\Locations:ScienceHouse_CarpenterMenu_Shop")));
                        if (Game1.IsMasterGame)
                        {
                            if (Game1.player.HouseUpgradeLevel < 3)
                            {
                                options.Add(new Response("Upgrade", Game1.content.LoadString("Strings\\Locations:ScienceHouse_CarpenterMenu_UpgradeHouse")));
                            }
                            else if ((Game1.MasterPlayer.mailReceived.Contains("ccIsComplete") || Game1.MasterPlayer.mailReceived.Contains("JojaMember") || Game1.MasterPlayer.hasCompletedCommunityCenter()) && (Game1.getLocationFromName("Town") as Town).daysUntilCommunityUpgrade.Value <= 0)
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
                        Game1.activeClickableMenu = new ShopMenu(Utility.getCarpenterStock(), 0, "Robin", null, null, null);
                    }
                    __result = true;
                    return false;
                    
                case "HospitalShop":
                    if (!Config.HospitalShop)
                        return true;
                    Game1.activeClickableMenu = new ShopMenu(Utility.getHospitalStock(), 0, null, null, null, null);
                    __result = true;
                    return false;
                      
                case "Blacksmith":
                    if (!Config.SmithShop)
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
                                AccessTools.FieldRefAccess<Game1, Multiplayer>(null, "multiplayer").globalChatInfoMessage("IridiumToolUpgrade", new string[]
                                {
                                    Game1.player.Name,
                                    tool.DisplayName
                                });
                            }
                        }
                        else
                        {
                            Game1.drawDialogue(Game1.getCharacterFromName("Clint"), Game1.content.LoadString("Data\\ExtraDialogue:Clint_NoInventorySpace"));
                        }
                    }
                    else
                    {
                        Response[] responses;
                        if (Game1.player.hasItemInInventory(535, 1, 0) || Game1.player.hasItemInInventory(536, 1, 0) || Game1.player.hasItemInInventory(537, 1, 0) || Game1.player.hasItemInInventory(749, 1, 0) || Game1.player.hasItemInInventory(275, 1, 0) || Game1.player.hasItemInInventory(791, 1, 0))
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
                    
                case "AnimalShop":
                    if (!Config.AnimalShop)
                        return true;
                    options = new List<Response>
                    {
                        new Response("Supplies", Game1.content.LoadString("Strings\\Locations:AnimalShop_Marnie_Supplies")),
                        new Response("Purchase", Game1.content.LoadString("Strings\\Locations:AnimalShop_Marnie_Animals")),
                        new Response("Leave", Game1.content.LoadString("Strings\\Locations:AnimalShop_Marnie_Leave"))
                    };
                    __instance.createQuestionDialogue("", options.ToArray(), "Marnie");
                    __result = true;
                    return false;
                    
                case "IceCreamStand":
                    if (!Config.IceCreamShop)
                        return true;
                    Game1.activeClickableMenu = new ShopMenu(new Dictionary<ISalable, int[]>
                    {
                        {
                            new Object(233, 1, false, -1, 0),
                            new int[]
                            {
                                250,
                                int.MaxValue
                            }
                        }
                    }, 0, null, null, null, null);
                    __result = true;
                    return false;

            }
            return true;
        }
        private static bool GameLocation_openShopMenu_Prefix(GameLocation __instance, string which, ref bool __result)
        {
            if (!Config.EnableMod)
                return true;
            string name = null;
            switch (which)
            {
                case "Fish":
                    if (!Config.FishShop)
                        return true;
                    foreach(var c in __instance.characters)
                    {
                        if (c.Name == "Willy")
                        {
                            name = "Willy";
                            break;
                        }
                    }
                    Game1.activeClickableMenu = new ShopMenu(Utility.getFishShopStock(Game1.player), 0, name, null, null, null);
                    __result = true;
                    return false;
                case "General":
                    if (!Config.SeedShop)
                        return true;
                    foreach (var c in __instance.characters)
                    {
                        if (c.Name == "Pierre")
                        {
                            name = "Pierre";
                            break;
                        }
                    }
                    Game1.activeClickableMenu = new ShopMenu((__instance as SeedShop).shopStock(), 0, name, null, null, null);
                    __result = true;
                    return false;
                case "SandyShop":
                    if (!Config.SandyShop)
                        return true;
                    foreach (var c in __instance.characters)
                    {
                        if (c.Name == "Sandy")
                        {
                            name = "Sandy";
                            break;
                        }
                    }
                    Game1.activeClickableMenu = new ShopMenu((Dictionary<ISalable, int[]>)AccessTools.Method(typeof(GameLocation), "sandyShopStock").Invoke(__instance, new object[] { }), 0, name, (Func<ISalable, Farmer, int, bool>)Delegate.CreateDelegate(typeof(Func<ISalable, Farmer, int, bool>), __instance, AccessTools.Method(typeof(GameLocation), "onSandyShopPurchase")), null, null);
                    __result = true;
                    return false;

            }
            return true;
        }
    }
}