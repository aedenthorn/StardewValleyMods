using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using StardewModdingAPI;
using StardewValley;
using StardewValley.BellsAndWhistles;
using System;
using System.Collections.Generic;
using xTile.Dimensions;
using xTile.ObjectModel;
using xTile.Tiles;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace Restauranteer
{
    public partial class ModEntry
    {
        private static int emoteBaseIndex = 424242;

        public static void NPCdraw_Postfix(NPC __instance, SpriteBatch b, float alpha, ref bool __state)
        {
            try
            {
                if (!Config.ModEnabled || !__state)
                    return;
                __instance.IsEmoting = true;
                if (!__instance.modData.TryGetValue(orderKey, out string data))
                    return;
                if (!Config.RestaurantLocations.Contains(__instance.currentLocation.Name))
                {
                    __instance.modData.Remove(orderKey);
                    return;
                }
                OrderData orderData = JsonConvert.DeserializeObject<OrderData>(data);
                int emoteIndex = __instance.CurrentEmoteIndex >= emoteBaseIndex ? __instance.CurrentEmoteIndex - emoteBaseIndex : __instance.CurrentEmoteIndex;
                if (__instance.CurrentEmoteIndex >= emoteBaseIndex + 3)
                {
                    AccessTools.Field(typeof(Character), "currentEmoteFrame").SetValue(__instance, emoteBaseIndex);
                }
                Vector2 emotePosition = __instance.getLocalPosition(Game1.viewport);
                emotePosition.Y -= 32 + __instance.Sprite.SpriteHeight * 4;
                if (SHelper.Input.IsDown(Config.ModKey))
                {
                    //SpriteText.drawStringWithScrollCenteredAt(b, orderData.dishName, (int)emotePosition.X + 32, (int)emotePosition.Y, "", 1, -1, 1);
                    SpriteText.drawStringWithScrollCenteredAt(b, orderData.dishName, (int)emotePosition.X + 32, (int)emotePosition.Y, "");
                }
                else
                {
                    b.Draw(emoteSprite, emotePosition, new Rectangle?(new Rectangle(emoteIndex * 16 % Game1.emoteSpriteSheet.Width, emoteIndex * 16 / emoteSprite.Width * 16, 16, 16)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, __instance.getStandingPosition().Y / 10000f);
                    b.Draw(Game1.objectSpriteSheet, emotePosition + new Vector2(16, 8), GameLocation.getSourceRectForObject(orderData.dish), Color.White, 0f, Vector2.Zero, 2f, SpriteEffects.None, (__instance.getStandingPosition().Y + 1) / 10000f);
                }
            }
            catch (Exception ex)
            {
                SMonitor.Log($"Failed in {nameof(NPCdraw_Postfix)}:\n{ex}", LogLevel.Error);
            }

        }

        public static bool GameLocationcheckAction_Prefix(GameLocation __instance, Location tileLocation, xTile.Dimensions.Rectangle viewport, Farmer who, ref bool __result)
        {
            try
            {
                if (!Config.ModEnabled || !Config.RestaurantLocations.Contains(__instance.Name))
                    return true;
                Tile tile = __instance.map.GetLayer("Buildings").PickTile(new Location(tileLocation.X * 64, tileLocation.Y * 64), viewport.Size);
                if (tile != null && tile.Properties.TryGetValue("Action", out PropertyValue property) && property == "DropBox GusFridge")
                {
                    if (__instance.performAction(property, who, tileLocation))
                    {

                        __result = true;
                        return false;
                    }
                    else if (Config.RequireEvent && !Game1.player.eventsSeen.Contains("980558"))
                    {
                        Game1.drawObjectDialogue(SHelper.Translation.Get("low-friendship"));
                        __result = true;
                        return false;
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                SMonitor.Log($"Failed in {nameof(GameLocationcheckAction_Prefix)}:\n{ex}", LogLevel.Error);
                return true;
            }
        }

        public static bool GameLocationPerformAction_Prefix(GameLocation __instance, Location tileLocation, xTile.Dimensions.Rectangle viewport, Farmer who, ref bool __result)
        {
            try
            {
                if (!Config.ModEnabled || !Config.RestaurantLocations.Contains(__instance.Name))
                    return true;
                Tile tile = __instance.map.GetLayer("Buildings").PickTile(new Location(tileLocation.X * 64, tileLocation.Y * 64), viewport.Size);
                if (tile != null && tile.Properties.TryGetValue("Action", out PropertyValue property) && property == "DropBox GusFridge")
                {
                    if (__instance.performAction(property, who, tileLocation))
                    {

                        __result = true;
                        return false;
                    }
                    else if (Config.RequireEvent && !Game1.player.eventsSeen.Contains("980558"))
                    {
                        Game1.drawObjectDialogue(SHelper.Translation.Get("low-friendship"));
                        __result = true;
                        return false;
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                SMonitor.Log($"Failed in {nameof(GameLocationPerformAction_Prefix)}:\n{ex}", LogLevel.Error);
                return true;
            }
        }

        public static void GameLocation_UpdateWhenCurrentLocation_Postfix(GameLocation __instance, GameTime time)
        {
            try
            {
                if (!Config.ModEnabled || !Config.RestaurantLocations.Contains(__instance.Name))
                    return;
                var fridge = GetFridge(__instance);
                fridge.Value.updateWhenCurrentLocation(time);
            }
            catch (Exception ex)
            {
                SMonitor.Log($"Failed in {nameof(GameLocation_UpdateWhenCurrentLocation_Postfix)}:\n{ex}", LogLevel.Error);
            }

        }

        public static bool Utility_checkForCharacterInteractionAtTile_Prefix(Vector2 tileLocation, Farmer who)
        {
            try
            {
                if (!Config.ModEnabled)
                    return true;
                NPC npc = Game1.currentLocation.isCharacterAtTile(tileLocation);
                if (npc is null || !npc.modData.TryGetValue(orderKey, out string data))
                    return true;
                if (!Config.RestaurantLocations.Contains(Game1.currentLocation.Name))
                {
                    npc.modData.Remove(orderKey);
                    return true;
                }
                OrderData orderData = JsonConvert.DeserializeObject<OrderData>(data);
                if (who.ActiveObject != null && who.ActiveObject.canBeGivenAsGift() && who.ActiveObject.Name == orderData.dishName)
                {
                    Game1.mouseCursor = 6;
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                SMonitor.Log($"Failed in {nameof(Utility_checkForCharacterInteractionAtTile_Prefix)}:\n{ex}", LogLevel.Error);
                return true;
            }
        }

        public static bool NPC_tryToReceiveActiveObject_Prefix(NPC __instance, Farmer who)
        {
            try
            {
                if (!Config.ModEnabled || !Config.RestaurantLocations.Contains(__instance.currentLocation.Name) || !__instance.modData.TryGetValue(orderKey, out string data))
                    return true;
                OrderData orderData = JsonConvert.DeserializeObject<OrderData>(data);
                if (who.ActiveObject?.ParentSheetIndex == orderData.dish)
                {
                    SMonitor.Log($"Fulfilling {__instance.Name}'s order of {orderData.dishName}");
                    if (!npcOrderNumbers.Value.ContainsKey(__instance.Name))
                    {
                        npcOrderNumbers.Value[__instance.Name] = 1;
                    }
                    else
                    {
                        npcOrderNumbers.Value[__instance.Name]++;
                    }
                    List<string> possibleReactions = new();
                    int count = 0;
                    string prefix = "RestauranteerMod-";
                    var dict = SHelper.GameContent.Load<Dictionary<string, string>>($"Characters/Dialogue/{__instance.Name}");
                    if (orderData.loved)
                    {
                        if (dict is not null && dict.TryGetValue($"{prefix}Loved-{++count}", out string r))
                        {
                            possibleReactions.Add(r);
                            while (dict.TryGetValue($"{prefix}Loved-{++count}", out r))
                            {
                                possibleReactions.Add(r);
                            }
                        }
                        else
                        {
                            possibleReactions.Add(SHelper.Translation.Get("loved-order-reaction-1"));
                            possibleReactions.Add(SHelper.Translation.Get("loved-order-reaction-2"));
                            possibleReactions.Add(SHelper.Translation.Get("loved-order-reaction-3"));
                        }
                    }
                    else
                    {
                        if (dict is not null && dict.TryGetValue($"{prefix}Liked-{++count}", out string r))
                        {
                            possibleReactions.Add(r);
                            while (dict.TryGetValue($"{prefix}Liked-{++count}", out r))
                            {
                                possibleReactions.Add(r);
                            }
                        }
                        else
                        {
                            possibleReactions.Add(SHelper.Translation.Get("liked-order-reaction-1"));
                            possibleReactions.Add(SHelper.Translation.Get("liked-order-reaction-2"));
                            possibleReactions.Add(SHelper.Translation.Get("liked-order-reaction-3"));
                        }
                    }
                    string reaction = possibleReactions[Game1.random.Next(possibleReactions.Count)];

                    switch (who.FacingDirection)
                    {
                        case 0:
                            ((FarmerSprite)who.Sprite).animateBackwardsOnce(80, 50f);
                            break;
                        case 1:
                            ((FarmerSprite)who.Sprite).animateBackwardsOnce(72, 50f);
                            break;
                        case 2:
                            ((FarmerSprite)who.Sprite).animateBackwardsOnce(64, 50f);
                            break;
                        case 3:
                            ((FarmerSprite)who.Sprite).animateBackwardsOnce(88, 50f);
                            break;
                    }
                    int friendshipAmount = orderData.loved ? Config.LovedFriendshipChange : Config.LikedFriendshipChange;
                    who.changeFriendship(friendshipAmount, __instance);
                    SMonitor.Log($"Changed friendship with {__instance.Name} by {friendshipAmount}");
                    if (Config.RevealGiftTaste)
                    {
                        who.revealGiftTaste(__instance.getName(), orderData.dish.ToString());
                    }
                    if (Config.PriceMarkup > 0)
                    {
                        int price = (int)Math.Round(who.ActiveObject.Price * Config.PriceMarkup);
                        who.Money += price;
                        SMonitor.Log($"Received {price} coins for order");
                    }
                    who.reduceActiveItemByOne();
                    who.completelyStopAnimatingOrDoingAction();
                    Game1.drawDialogue(__instance);
                    //Game1.drawDialogue(__instance, reaction + "$h");
                    __instance.faceTowardFarmerForPeriod(2000, 3, false, who);
                    __instance.modData.Remove(orderKey);
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                SMonitor.Log($"Failed in {nameof(NPC_tryToReceiveActiveObject_Prefix)}:\n{ex}", LogLevel.Error);
                return true;
            }
        }


    }
    /*
     * 

    /*
     * TODO: Find method 
    [HarmonyPatch(typeof(Utility), nameof(Utility.getSaloonStock))]
    public class Utility_getSaloonStock_Patch
    {
        public static void Postfix(Dictionary<ISalable, int[]> __result)
        {
            if (!Config.ModEnabled || !Config.SellCurrentRecipes)
                return;
            foreach (var npc in Game1.getLocationFromName("Saloon").characters)
            {
                if (npc.modData.TryGetValue(orderKey, out string dataString))
                {
                    var data = JsonConvert.DeserializeObject<OrderData>(dataString);
                    if (!Game1.player.cookingRecipes.ContainsKey(data.dishName))
                    {
                        Utility.AddStock(__result, new Object(data.dish, 1, true, -1, 0), data.dishPrice, -1);
                    }
                }
            }
        }
    }
    */

}