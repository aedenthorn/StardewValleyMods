using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Extensions;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using Object = StardewValley.Object;

namespace AdvancedAutoGrabber
{
	public partial class ModEntry
	{
        [HarmonyPatch(typeof(FarmAnimal), nameof(FarmAnimal.DigUpProduce))]
        public static class FarmAnimal_DigUpProduce_Patch
        {
            public static void Postfix(FarmAnimal __instance, GameLocation location, Object produce)
            {
                if (!Config.ModEnabled || !Config.GrabOnDugUp)
                    return;
                List<Vector2> grabbers = new();
                List<Vector2> products = new();
                foreach (var key in location.Objects.Keys.ToArray())
                {
                    var obj = location.Objects[key];
                    if (obj?.QualifiedItemId == produce.QualifiedItemId)
                        products.Add(key);
                    else if (obj?.QualifiedItemId == "(BC)165")
                        grabbers.Add(key);
                }
                foreach(var pk in products)
                {
                    var product = location.Objects[pk];
                    foreach (var gk in grabbers)
                    {
                        var grabber = location.Objects[gk];
                        if ((Config.GrabRange < 0 || Vector2.Distance(product.TileLocation, grabber.TileLocation) <= Config.GrabRange))
                        {
                            Chest chest = grabber.heldObject.Value as Chest;
                            if (chest != null && TryGrab( grabber, chest, product))
                            {
                                grabber.showNextIndex.Value = true;
                                location.Objects.Remove(pk);
                                SMonitor.Log($"Auto-grabbed {product.ItemId} at {product.TileLocation} from {__instance.displayName} at {__instance.Tile}");
                                break;
                            }
                        }
                    }
                }
            }
        }

        [HarmonyPatch(typeof(Object), nameof(Object.draw), new Type[] { typeof(SpriteBatch), typeof(int), typeof(int), typeof(float) })]
        public class Object_draw_Patch
        {
            public static void Postfix(Object __instance, SpriteBatch spriteBatch, int x, int y, float alpha)
            {
                if (!Config.ModEnabled || __instance.QualifiedItemId != "(BC)165" || !__instance.modData.TryGetValue(limitKey, out var itemId))
                    return;
                var obj = ItemRegistry.Create<Object>(itemId, 1, 0, false);
                obj.Flipped = false;
                obj.draw(spriteBatch, x * 64, y * 64 - 32, (__instance.TileLocation.Y + 0.66f) * 64f / 10000f + (float)x * 1E-05f, alpha * (Config.OpacityPercent / 100f));
            }
        }

        [HarmonyPatch(typeof(GameLocation), nameof(GameLocation.loadObjects))]
        public class GameLocation_loadObjects_Patch
        {
            public static void Prefix(GameLocation __instance)
            {
                if (!Config.ModEnabled)
                    return;
                RegisterLocationTriggers(__instance);
            }
        }
        [HarmonyPatch(typeof(Game1), nameof(Game1.pressUseToolButton))]
        public static class Game1_pressUseToolButton_Patch
        {
            public static bool Prefix(ref bool __result)
            {
                if (!Config.ModEnabled || !Context.IsPlayerFree)
                    return true;

                Vector2 c = Game1.player.GetToolLocation(false) / 64f;
                c.X = (int)c.X;
                c.Y = (int)c.Y;
                if(!Game1.currentLocation.Objects.TryGetValue(c, out var grabber) || grabber.QualifiedItemId != "(BC)165")
                {
                    return true;
                }

                if (grabber.heldObject.Value is Chest chest && chest.isEmpty())
                {
                    bool produce =  Game1.player.ActiveObject is Object obj && Game1.farmAnimalData.Values.Any(d => d.ProduceItemIds.Exists(p => "(O)" + p.ItemId == obj.QualifiedItemId) || d.DeluxeProduceItemIds.Exists(p => "(O)" + p.ItemId == obj.QualifiedItemId));
                    bool exists = grabber.modData.TryGetValue(limitKey, out var val);
                    if (!produce && exists)
                    {
                        grabber.modData.Remove(limitKey);
                    }
                    else if (produce && (!exists || val != Game1.player.ActiveObject.ItemId))
                    {
                        grabber.modData[limitKey] = Game1.player.ActiveObject.ItemId;
                    }
                    else
                    {
                        if (Config.Debug)
                        {
                            var animal = new FarmAnimal("Pig", Game1.Multiplayer.getNewID(), Game1.player.UniqueMultiplayerID)
                            {
                                currentLocation = Game1.player.currentLocation,
                                Position = Game1.player.Position + new Vector2(64, 0)
                            };
                            animal.growFully(Game1.random);
                            animal.friendshipTowardFarmer.Value = 1500;
                            Game1.player.currentLocation.animals.Add(animal.myID.Value, animal);
                            animal.currentProduce.Value = animal.GetProduceID(Game1.random, Game1.random.NextBool());
                            Object p = ItemRegistry.Create<Object>(animal.currentProduce.Value, 1, 0, false);
                            animal.DigUpProduce(animal.currentLocation, p);
                            
                        }
                        return true;
                    }
                    __result = true;
                    Game1.playSound("Ship");
                    return false;
                }
                return true;
            }
        }
    }
}
