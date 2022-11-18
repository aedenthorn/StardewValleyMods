using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using Newtonsoft.Json;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using xTile.Dimensions;
using Object = StardewValley.Object;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace Restauranteer
{
    public partial class ModEntry
    {

        [HarmonyPatch(typeof(NPC), nameof(NPC.draw))]
        public class NPC_draw_Patch
        {
            private static int emoteBaseIndex = 424242;

            public static void Prefix(NPC __instance, ref bool __state)
            {
                if (!Config.ModEnabled || !__instance.IsEmoting || __instance.CurrentEmote != emoteBaseIndex)
                    return;
                __state = true;
                __instance.IsEmoting = false;
            }
            public static void Postfix(NPC __instance, SpriteBatch b, float alpha, ref bool __state)
            {
                if (!Config.ModEnabled || !__state)
                    return;
                __instance.IsEmoting = true;
                if (!__instance.modData.TryGetValue(orderKey, out string data))
                    return;
                if(__instance.currentLocation.Name != "Saloon")
                {
                    __instance.modData.Remove(orderKey);
                    return;
                }
                OrderData orderData = JsonConvert.DeserializeObject<OrderData>(data);
                int emoteIndex = __instance.CurrentEmoteIndex >= emoteBaseIndex ? __instance.CurrentEmoteIndex - emoteBaseIndex : __instance.CurrentEmoteIndex;
                if(__instance.CurrentEmoteIndex >= emoteBaseIndex + 3)
                {
                    AccessTools.Field(typeof(Character), "currentEmoteFrame").SetValue(__instance, emoteBaseIndex);
                }
                Vector2 emotePosition = __instance.getLocalPosition(Game1.viewport);
                emotePosition.Y -= 32 + __instance.Sprite.SpriteHeight * 4;
                b.Draw(emoteSprite, emotePosition, new Rectangle?(new Rectangle(emoteIndex * 16 % Game1.emoteSpriteSheet.Width, emoteIndex * 16 / emoteSprite.Width * 16, 16, 16)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, __instance.getStandingY() / 10000f);
                b.Draw(Game1.objectSpriteSheet, emotePosition + new Vector2(16, 8), GameLocation.getSourceRectForObject(orderData.dish), Color.White, 0f, Vector2.Zero, 2f, SpriteEffects.None, (__instance.getStandingY() + 1) / 10000f);
            }
        }
        [HarmonyPatch(typeof(GameLocation), nameof(GameLocation.performAction))]
        public class GameLocation_performAction_Patch
        {
            public static bool Prefix(GameLocation __instance, string action, Farmer who, Location tileLocation, ref bool __result)
            {
                if (!Config.ModEnabled || __instance.Name != "Saloon" || (action != "kitchen"  && action != "fridge" && (SHelper.Input.IsDown(Config.FridgeModKey) || action != "DropBox GusFridge")))
                    return true;
                if (!Game1.player.eventsSeen.Contains(980558))
                {
                    Game1.drawObjectDialogue(SHelper.Translation.Get("low-friendship"));
                    __result = true;
                    return false;
                }
                if (Config.AuotFillFridge)
                {
                    fridge.Value.items.Clear();
                    foreach (var c in __instance.characters)
                    {
                        if (c.modData.TryGetValue(orderKey, out string dataString))
                        {
                            OrderData data = JsonConvert.DeserializeObject<OrderData>(dataString);
                            CraftingRecipe r = new CraftingRecipe(data.dishName, true);
                            if (r is not null)
                            {
                                foreach (var key in r.recipeList.Keys)
                                {
                                    if (Game1.objectInformation.ContainsKey(key))
                                    {
                                        var obj = new Object(key, r.recipeList[key]);
                                        SMonitor.Log($"Adding {obj.Name} ({obj.ParentSheetIndex}) x{obj.Stack} to fridge");
                                        fridge.Value.addItem(obj);
                                    }
                                    else
                                    {
                                        List<int> list = new List<int>();
                                        foreach (var kvp in Game1.objectInformation)
                                        {
                                            string[] objectInfoArray = kvp.Value.Split('/', StringSplitOptions.None);
                                            string[] typeAndCategory = objectInfoArray[3].Split(' ', StringSplitOptions.None);
                                            if (typeAndCategory.Length > 1 && typeAndCategory[1] == key.ToString())
                                            {
                                                list.Add(kvp.Key);
                                            }
                                        }
                                        if (list.Any())
                                        {
                                            var obj = new Object(list[Game1.random.Next(list.Count)], r.recipeList[key]);
                                            SMonitor.Log($"Adding {obj.Name} ({obj.ParentSheetIndex}) x{obj.Stack} to fridge");
                                            fridge.Value.addItem(obj);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                if (action == "kitchen")
                {
                    __instance.ActivateKitchen(fridge);
                }
                else if(action == "fridge" || (!SHelper.Input.IsDown(Config.FridgeModKey) && action == "DropBox GusFridge"))
                {
                    fridgePosition.Value = tileLocation;
                    fridge.Value.fridge.Value = true;
                    fridge.Value.checkForAction(who, false);
                }
                __result = true;
                return false;
            }
        }
        [HarmonyPatch(typeof(GameLocation), "initNetFields")]
        public class GameLocation_initNetFields_Patch
        {
            public static void Postfix(GameLocation __instance)
            {
                if (!Config.ModEnabled || __instance.Name != "Saloon")
                    return;
                __instance.NetFields.AddFields(new INetSerializable[]
                {
                    fridge
                });
            }
        }

        [HarmonyPatch(typeof(GameLocation), "drawAboveFrontLayer")]
        public class GameLocation_drawAboveFrontLayer_Patch
        {
            public static void Postfix(GameLocation __instance, SpriteBatch b)
            {
                if (!Config.ModEnabled || __instance.Name != "Saloon")
                    return;
            }
        }
        [HarmonyPatch(typeof(GameLocation), nameof(GameLocation.UpdateWhenCurrentLocation))]
        public class GameLocation_UpdateWhenCurrentLocation_Patch
        {
            public static void Postfix(GameLocation __instance, GameTime time)
            {
                if (!Config.ModEnabled || __instance.Name != "Saloon")
                    return;
                fridge.Value.updateWhenCurrentLocation(time, __instance);
            }
        }

        [HarmonyPatch(typeof(NPC), nameof(NPC.tryToReceiveActiveObject))]
        public class NPC_tryToReceiveActiveObject_Patch
        {
            public static bool Prefix(NPC __instance, Farmer who)
            {
                if (!Config.ModEnabled || __instance.currentLocation.Name != "Saloon" || !__instance.modData.TryGetValue(orderKey, out string data))
                    return true;
                OrderData orderData = JsonConvert.DeserializeObject<OrderData>(data);
                if(who.ActiveObject?.ParentSheetIndex == orderData.dish)
                {
                    SMonitor.Log($"Fulfilling {__instance.Name}'s order of {orderData.dishName}");
                    if(!npcOrderNumbers.Value.ContainsKey(__instance.Name))
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
                            while(dict.TryGetValue($"{prefix}Loved-{++count}", out r))
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
                            while(dict.TryGetValue($"{prefix}Liked-{++count}", out r))
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
                        who.revealGiftTaste(__instance, orderData.dish);
                    }
                    if(Config.PriceMarkup > 0)
                    {
                        int price = (int)Math.Round(who.ActiveObject.Price * Config.PriceMarkup);
                        who.Money += price;
                        SMonitor.Log($"Received {price} coins for order");
                    }
                    who.reduceActiveItemByOne();
                    who.completelyStopAnimatingOrDoingAction();
                    Game1.drawDialogue(__instance, reaction + "$h");
                    __instance.faceTowardFarmerForPeriod(2000, 3, false, who);
                    __instance.modData.Remove(orderKey);
                    return false;
                }
                return true;   
            }
        }
    }
}