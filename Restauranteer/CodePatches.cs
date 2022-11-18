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
using Object = StardewValley.Object;

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
                if(__instance.CurrentEmoteIndex >= emoteBaseIndex)
                {
                    __instance.emoteInterval = 0;
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
            public static bool Prefix(GameLocation __instance, string action, Farmer who)
            {
                if (!Config.ModEnabled || !Game1.player.eventsSeen.Contains(980558) || __instance.Name != "Saloon" || (action != "kitchen"  && action != "fridge"))
                    return true;
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
                if (action == "kitchen")
                {
                    __instance.ActivateKitchen(fridge);
                }
                else
                {
                    fridge.Value.fridge.Value = true;
                    fridge.Value.checkForAction(who, false);
                }
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
                    
                    __instance.receiveGift(who.ActiveObject, who, true, 1f, true);
                    who.friendshipData[__instance.Name].GiftsThisWeek--;
                    who.friendshipData[__instance.Name].GiftsToday--;
                    Game1.stats.GiftsGiven--;
                    who.reduceActiveItemByOne();
                    who.completelyStopAnimatingOrDoingAction();
                    __instance.faceTowardFarmerForPeriod(4000, 3, false, who);
                    __instance.modData.Remove(orderKey);
                    return false;
                }
                return true;   
            }
        }
    }
}