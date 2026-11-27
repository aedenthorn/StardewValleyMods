using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using StardewValley;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Object = StardewValley.Object;

namespace SimpleCooking
{
    public partial class ModEntry
    {
        [HarmonyPatch(typeof(Object), nameof(Object.draw), new Type[] { typeof(SpriteBatch), typeof(int), typeof(int), typeof(float) })]
        public class Object_draw_Patch
        {
            public static void Postfix(Object __instance, SpriteBatch spriteBatch, int x, int y, float alpha)
            {
                if (!Config.ModEnabled)
                {
                    return;
                }
                if (CookerDict.TryGetValue(__instance.QualifiedItemId, out var cdata) && TryGetCookingDataForCooker(__instance, out var data))
                {
                    var offset = new Vector2(cdata.X, cdata.Y);
                    ParsedItemData idata;
                    Color color = Color.White;
                    if (data.Burned)
                    {
                        if(data.BurntID == null) color = Config.BurnedColor;
                        idata = ItemRegistry.GetDataOrErrorItem(data.BurntID ?? data.InputID);
                        if (data.Smoke && (__instance is not Torch || __instance.IsOn) && Game1.currentGameTime.TotalGameTime.Ticks % 50 == 0)
                        {
                            TemporaryAnimatedSprite sprite = TemporaryAnimatedSprite.GetTemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(372, 1956, 10, 10), __instance.TileLocation * 64 + offset + new Vector2(32, 0), false, 0.002f, Color.Gray);
                            sprite.alpha = 1f;
                            sprite.motion = new Vector2(0f, -0.5f);
                            sprite.acceleration = new Vector2(0.002f, 0f);
                            sprite.interval = 99999f;
                            sprite.layerDepth = 1f;
                            sprite.scale = 1;
                            sprite.scaleChange = 0.02f;
                            sprite.rotationChange = (float)Game1.random.Next(-5, 6) * 3.1415927f / 256f;
                            __instance.Location.temporarySprites.Add(sprite);
                        }
                    }
                    else if(data.Progress >= 1)
                    {
                        color = data.ProductID is null ? Config.GrilledColor : Color.White;
                        idata = ItemRegistry.GetDataOrErrorItem(data.ProductID ?? data.InputID);

                        if (data.Smoke && (__instance is not Torch || __instance.IsOn) && Game1.currentGameTime.TotalGameTime.Ticks % 50 == 0)
                        {
                            TemporaryAnimatedSprite sprite = TemporaryAnimatedSprite.GetTemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(372, 1956, 10, 10), __instance.TileLocation * 64 + offset + new Vector2(32, 0), false, 0.002f, Color.Gray);
                            sprite.alpha = 0.5f + (data.Progress - 1) / Math.Max(1, data.BurntAt - 1) / 2f;
                            sprite.motion = new Vector2(0f, -0.5f);
                            sprite.acceleration = new Vector2(0.002f, 0f);
                            sprite.interval = 99999f;
                            sprite.layerDepth = 1f;
                            sprite.scale = 1;
                            sprite.scaleChange = 0.02f;
                            sprite.rotationChange = (float)Game1.random.Next(-5, 6) * 3.1415927f / 256f;
                            __instance.Location.temporarySprites.Add(sprite);
                        }

                    }
                    else
                    {
                        idata = ItemRegistry.GetDataOrErrorItem(data.InputID);
                    }
                    float draw_layer = Math.Max(0f, (float)((y + 1) * 64 - 24) / 10000f) + (float)x * 1E-05f;
                    spriteBatch.Draw(idata.GetTexture(), Game1.GlobalToLocal(__instance.TileLocation * 64) + offset, idata.GetSourceRect(), color, 0, Vector2.Zero, 3f, SpriteEffects.None, draw_layer + cdata.Z / 10000f);
                }
            }
        }
        [HarmonyPatch(typeof(ColoredObject), "drawSmokedFish")]
        public class ColoredObject_drawSmokedFish_Patch
        {
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                SMonitor.Log($"Transpiling ColoredObject.drawSmokedFish");

                var codes = new List<CodeInstruction>(instructions);
                for (int i = 0; i < codes.Count; i++)
                {
                    if (codes[i].opcode == OpCodes.Newobj && codes[i].operand is ConstructorInfo mi && mi == AccessTools.Constructor(typeof(Color), new Type[] { typeof(int), typeof(int), typeof(int) }))
                    {
                        SMonitor.Log($"Adding color switch");
                        codes.Insert(i + 1, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ModEntry.SwitchColor))));
                        codes.Insert(i + 1, new CodeInstruction(OpCodes.Ldarg_0));
                        i += 2;
                    }
                }

                return codes.AsEnumerable();
            }
        }
        [HarmonyPatch(typeof(Object), "loadDisplayName")]
        public class Object_loadDisplayName_Patch
        {
            public static bool Prefix(Object __instance, ref string __result)
            {
                if (!Config.ModEnabled || __instance.ItemId != "SmokedFish")
                    return true;
                if(__instance.Name.StartsWith("Grilled "))
                {
                    __result = string.Format(SHelper.Translation.Get("grilled-x"), ItemRegistry.GetDataOrErrorItem(__instance.preservedParentSheetIndex.Value).DisplayName);
                    return false;
                }
                if (__instance.Name.StartsWith("Burnt "))
                {
                    __result = string.Format(SHelper.Translation.Get("burnt-x"), ItemRegistry.GetDataOrErrorItem(__instance.preservedParentSheetIndex.Value).DisplayName);
                    return false;
                }
                return true;
            }
        }
        [HarmonyPatch(typeof(Object), "getDescription")]
        public class Object_getDescription_Patch
        {
            public static bool Prefix(Object __instance, ref string __result)
            {
                if (!Config.ModEnabled || __instance.ItemId != "SmokedFish")
                    return true;
                if (__instance.Name.StartsWith("Grilled "))
                {
                    __result = SHelper.Translation.Get("grilled-desc");

                    return false;
                }
                if (__instance.Name.StartsWith("Burnt "))
                {
                    __result = SHelper.Translation.Get("burnt-desc");
                    return false;
                }
                return true;
            }
        }
        [HarmonyPatch(typeof(GameLocation), nameof(GameLocation.performTenMinuteUpdate))]
        public class GameLocation_performTenMinuteUpdate_Patch
        {
            public static void Postfix(GameLocation __instance, int timeOfDay)
            {
                if (!Config.ModEnabled)
                    return;
                foreach(var kvp in __instance.Objects.Pairs)
                {
                    if(TryGetCookingDataForCooker(kvp.Value, out var data))
                    {
                        if(kvp.Value is Torch t && !t.IsOn)
                        {
                            continue;
                        }
                        data.Update(kvp.Value, timeOfDay);
                    }
                }
            }
        }
        [HarmonyPatch(typeof(Game1), nameof(Game1.pressUseToolButton))]
        public class Game1_pressUseToolButton_Patch
        {
            public static bool Prefix()
            {
                if (!Config.ModEnabled)
                    return true;
                Farmer f = Game1.player;
                Vector2 c = f.ActiveObject == null ? f.GetToolLocation(false) / 64f : new Vector2((float)(Game1.getOldMouseX() + Game1.viewport.X), (float)(Game1.getOldMouseY() + Game1.viewport.Y)) / 64;
                c.X = (int)c.X;
                c.Y = (int)c.Y;
                GameLocation l = Game1.currentLocation;
                if (!l.Objects.TryGetValue(c, out var obj) || !CookerDict.TryGetValue(obj.QualifiedItemId, out var cooker))
                {
                    return true;
                }
                var cookable = TryGetCookingDataForCookable(f.ActiveObject, out var data);
                if (TryGetCookingDataForCooker(obj, out var cdata))
                {
                    var progress = cdata.Progress;
                    if (progress < 1)
                    {
                        Game1.showRedMessage(SHelper.Translation.Get("cooker-busy"));
                        return false;
                    }
                    
                    TryReturnObject(cdata.GetProduct(), f);
                    obj.modData.Remove(cookingKey);
                    if (!cookable)
                    {
                        l.playSound("coin", obj.TileLocation);
                        return false;
                    }
                }
                if (cookable)
                {
                    l.playSound(data.PlacedSound, obj.TileLocation);
                    f.ignoreItemConsumptionThisFrame = false;
                    obj.modData[cookingKey] = JsonConvert.SerializeObject(data);
                    f.reduceActiveItemByOne();
                    return false;
                }
                return true;
            }


        }
    }
}