using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json.Linq;
using StardewValley;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection;
using xTile;
using xTile.Layers;
using xTile.ObjectModel;
using Object = StardewValley.Object;

namespace LightMod
{
    public partial class ModEntry
    {
        public static IEnumerable<CodeInstruction> SGame_DrawImpl_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            SMonitor.Log($"Transpiling Game1._draw");

            var codes = new List<CodeInstruction>(instructions);
            for (int i = 0; i < codes.Count; i++)
            {
                if (i < codes.Count - 3 && codes[i].opcode == OpCodes.Ldloc_S && codes[i + 1].opcode == OpCodes.Ldfld && codes[i + 2].opcode == OpCodes.Callvirt && (FieldInfo)codes[i + 1].operand == AccessTools.Field(typeof(LightSource), nameof(LightSource.lightTexture)) && (MethodInfo)codes[i + 2].operand == AccessTools.PropertyGetter(typeof(Texture2D), nameof(Texture2D.Bounds)))
                {
                    SMonitor.Log("adding method to check light source texture bounds");
                    codes.Insert(i + 3, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModEntry), nameof(ModEntry.GetTextureBounds))));
                    codes.Insert(i + 3, codes[i].Clone());
                    i += 4;
                }
            }

            return codes.AsEnumerable();
        }
        public static long lastFrame;
        private static Rectangle GetTextureBounds(Rectangle bounds, LightSource lightSource)
        {
            if (!Config.ModEnabled || lightSource.textureIndex.Value < 9 || lightSource.textureIndex.Value - 8 > lightTextureList.Count)
                return bounds;
            var d = lightTextureList[lightSource.textureIndex.Value - 9];
            if (d.frames < 2)
                return bounds;
            int currentFrame = (int)(Game1.currentGameTime.TotalGameTime.TotalSeconds / d.duration) % d.frames;

            return new Rectangle(d.width * currentFrame, 0, d.width, bounds.Height);
        }

        [HarmonyPatch(typeof(LightSource), "loadTextureFromConstantValue")]
        private static class LightSource_loadTextureFromConstantValue_Patch
        {
            public static bool Prefix(LightSource __instance, int value)
            {
                if (!Config.ModEnabled || value < 9 || value - 8 > lightTextureList.Count)
                    return true;

                __instance.lightTexture = SHelper.GameContent.Load<Texture2D>(lightTextureList[value - 9].texturePath);
                return false;
            }
        }
        [HarmonyPatch(typeof(Object), "initializeLightSource")]
        private static class Object_initializeLightSource_Patch
        {
            public static bool Prefix(Object __instance, Vector2 tileLocation)
            {
                if (!Config.ModEnabled)
                    return true;
                if (__instance.modData.TryGetValue(switchKey, out string status) && status == "off")
                {
                    __instance.lightSource = null;
                    return false;
                }

                if (!lightDataDict.TryGetValue(__instance.ParentSheetIndex + "", out LightData light))
                {
                    if(!lightDataDict.TryGetValue(__instance.Name, out light))
                    {
                        return true;
                    }
                }

                SMonitor.Log($"Adding light to {__instance.Name}");

                int identifier = (int)(tileLocation.X * 2000f + tileLocation.Y);
                __instance.lightSource = new LightSource(light.textureIndex, new Vector2(tileLocation.X * 64f + light.offset.X, tileLocation.Y * 64f + light.offset.Y), light.radius, light.color, identifier, LightSource.LightContext.None, 0L);
                __instance.isLamp.Value = light.isLamp;
                __instance.IsOn = true;
                return false;
            }
            public static void Postfix(Object __instance)
            {
                if (!Config.ModEnabled || __instance.lightSource is null)
                    return;

                if (__instance.modData.TryGetValue(alphaKey, out string astr) && int.TryParse(astr, out int alpha))
                {
                    __instance.lightSource.color.A = (byte)alpha;
                    //SMonitor.Log($"New light alpha: {__instance.lightSource.color.A}");
                }
                if (__instance.modData.TryGetValue(radiusKey, out string rstr) && float.TryParse(rstr, NumberStyles.Float, CultureInfo.InvariantCulture, out float radius))
                {
                    __instance.lightSource.radius.Value = radius;
                    //SMonitor.Log($"New light radius: {__instance.lightSource.radius.Value}");
                }
                if(__instance is Furniture && Game1.currentLocation.furniture.ToArray().Contains(__instance))
                {
                    Game1.currentLocation.removeLightSource(__instance.lightSource.Identifier);
                    Game1.currentLocation.sharedLights.Add(__instance.lightSource.Identifier, __instance.lightSource.Clone());
                }
            }
        }


        [HarmonyPatch(typeof(Furniture), nameof(Furniture.addLights))]
        private static class Furniture_addLights_Patch
        {
            public static bool xPrefix(Furniture __instance)
            {
                return (!Config.ModEnabled || !__instance.modData.TryGetValue(switchKey, out string status) || status == "on");
            }
            public static void Postfix(Furniture __instance, GameLocation environment)
            {
                if (!Config.ModEnabled || __instance.lightSource is null)
                    return;

                if (__instance.modData.TryGetValue(alphaKey, out string astr) && int.TryParse(astr, out int alpha))
                {
                    __instance.lightSource.color.A = (byte)alpha;
                    SMonitor.Log($"New light alpha: {__instance.lightSource.color.A}");
                }
                if (__instance.modData.TryGetValue(radiusKey, out string rstr) && float.TryParse(rstr, NumberStyles.Float, CultureInfo.InvariantCulture, out float radius))
                {
                    __instance.lightSource.radius.Value = radius;
                    SMonitor.Log($"New light radius: {__instance.lightSource.radius.Value}");
                }
                environment.removeLightSource(__instance.lightSource.Identifier);
                environment.sharedLights.Add(__instance.lightSource.Identifier, __instance.lightSource.Clone());
            }
        }
        
        [HarmonyPatch(typeof(Object), nameof(Object.minutesElapsed))]
        private static class Object_minutesElapsed_Patch
        {
            public static void Postfix(Object __instance)
            {
                if (!Config.ModEnabled)
                    return;
                __instance.initializeLightSource(__instance.TileLocation);
            }
        }

        [HarmonyPatch(typeof(GameLocation), nameof(GameLocation.drawLightGlows))]
        private static class GameLocation_drawLightGlows_Patch
        {
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                SMonitor.Log($"Transpiling GameLocation.drawLightGlows");

                var codes = new List<CodeInstruction>(instructions);
                for (int i = 0; i < codes.Count; i++)
                {
                    if (codes[i].opcode == OpCodes.Call && (MethodInfo)codes[i].operand == AccessTools.PropertyGetter(typeof(Color), nameof(Color.White)))
                    {
                        SMonitor.Log("adding method to change light glow alpha");
                        codes[i].operand = AccessTools.Method(typeof(ModEntry), nameof(ModEntry.GetLightGlowAlpha));
                        codes.Insert(i, new CodeInstruction(OpCodes.Ldloc_1));
                        codes.Insert(i, new CodeInstruction(OpCodes.Ldarg_0));
                        break;
                    }
                }

                return codes.AsEnumerable();
            }
        }
        [HarmonyPatch(typeof(GameLocation), nameof(GameLocation.drawAboveFrontLayer))]
        private static class GameLocation_drawAboveFrontLayer_Patch
        {
            public static void Postfix(GameLocation __instance, SpriteBatch b)
            {
                if (!Config.ModEnabled)
                    return;

                foreach(var npc in __instance.characters)
                {
                    if (!lightDataDict.TryGetValue(npc.Name, out var data))
                        continue;

                    float alpha = 1;
                    float radius = data.radius;
                    if (npc.modData.TryGetValue(alphaKey, out string astr) && int.TryParse(astr, out int a))
                    {
                        alpha = (byte)a / 255f;
                    }
                    if (npc.modData.TryGetValue(radiusKey, out string rstr) && float.TryParse(rstr, NumberStyles.Float, CultureInfo.InvariantCulture, out float r))
                    {
                        radius = r;
                    }
                    float scale = 1;
                    Point location = Utility.Vector2ToPoint(Game1.GlobalToLocal(Game1.viewport, npc.GetBoundingBox().Center.ToVector2() - new Vector2(20.5f, 33.5f) * scale * 4));
                    Point size = Utility.Vector2ToPoint(new Vector2(41f, 67f) * scale * 4);
                    b.Draw(Game1.mouseCursors, new Rectangle(location, size), new Microsoft.Xna.Framework.Rectangle?(new Rectangle(21, 1695, 41, 67)), Color.Yellow * 0.5f, 0f, Vector2.Zero, SpriteEffects.None, 1f);

                }
            }
        }

        [HarmonyPatch(typeof(Furniture), nameof(Furniture.removeLights))]
        private static class Furniture_removeLights_Patch
        {
            public static bool Prefix(Furniture __instance)
            {
                return (!Config.ModEnabled || !__instance.modData.TryGetValue(switchKey, out string status) || status == "off");
            }
        }
        
        [HarmonyPatch(typeof(Game1), nameof(Game1.pressSwitchToolButton))]
        private static class Game1_pressSwitchToolButton_Patch
        {
            public static bool Prefix()
            {
                return (!Config.ModEnabled || !suppressingScroll);

            }
        }

        //[HarmonyPatch(typeof(Game1), nameof(Game1.getStartingToGetDarkTime))]
        private static class Game1_getStartingToGetDarkTime_Patch
        {
            public static bool Prefix(ref int __result)
            {
                if (!Config.ModEnabled)
                    return true;

                int morningLightTime = GetMorningLightTime();

                if (Game1.timeOfDay < morningLightTime && Game1.timeOfDay >= morningLightTime - 100)
                {
                    __result = Game1.timeOfDay;
                }
                else
                {
                    __result = GetNightDarkTime() - 200;
                }

                return false;
            }
        }
        //[HarmonyPatch(typeof(Game1), nameof(Game1.getModeratelyDarkTime))]
        private static class Game1_getModeratelyDarkTime_Patch
        {
            public static bool Prefix(ref int __result)
            {
                if (!Config.ModEnabled)
                    return true;
                int morningLightTime = GetMorningLightTime();

                if (Game1.timeOfDay < morningLightTime - 100 && Game1.timeOfDay >= morningLightTime - 200)
                {
                    __result = Game1.timeOfDay;
                }
                else
                {
                    __result = GetNightDarkTime() - 100;
                }
                return false;
            }
        }
        //[HarmonyPatch(typeof(Game1), nameof(Game1.getTrulyDarkTime))]
        private static class Game1_getTrulyDarkTime_Patch
        {
            public static bool Prefix(ref int __result)
            {
                if (!Config.ModEnabled)
                    return true;

                if(Game1.timeOfDay < GetMorningLightTime() - 200)
                {
                    __result = Game1.timeOfDay;
                }
                else
                {
                    __result = GetNightDarkTime();
                }
                return false;
            }
        }
        //[HarmonyPatch(typeof(Game1), nameof(Game1.performTenMinuteClockUpdate))]
        private static class Game1_performTenMinuteClockUpdate_Patch
        {
            public static void Postfix()
            {
                if (!Config.ModEnabled)
                    return;

                if(Game1.timeOfDay == GetMorningLightTime())
                {
                    PropertyValue dayTiles;
                    
                    if (Game1.currentLocation.Map.Properties.TryGetValue("DayTiles", out dayTiles) && dayTiles != null)
                    {
                        string[] split = dayTiles.ToString().Trim().Split(' ', StringSplitOptions.None);
                        for (int i = 0; i < split.Length; i += 4)
                        {
                            Layer layer = Game1.currentLocation.Map.GetLayer(split[i]);
                            if ((!split[i + 3].Equals("720") || !Game1.MasterPlayer.mailReceived.Contains("pamHouseUpgrade")) && layer.Tiles[Convert.ToInt32(split[i + 1]), Convert.ToInt32(split[i + 2])] != null)
                            {
                                layer.Tiles[Convert.ToInt32(split[i + 1]), Convert.ToInt32(split[i + 2])].TileIndex = Convert.ToInt32(split[i + 3]);
                            }
                        }
                    }
                    if(!Game1.currentLocation.IsOutdoors)
                        Game1.ambientLight = Color.White;
                    Game1.outdoorLight = (Game1.IsRainingHere(null) ? Game1.ambientLight : Color.White);
                    AccessTools.Method(typeof(GameLocation), "resetLocalState").Invoke(Game1.currentLocation, new object[] { });
                }
            }
        }

        [HarmonyPatch(typeof(Furniture), nameof(Furniture.resetOnPlayerEntry))]
        private static class Furniture_resetOnPlayerEntry_Patch
        {
            public static void Postfix(Furniture __instance, GameLocation environment)
            {
                if (!Config.ModEnabled)
                    return;
                if(__instance.modData.TryGetValue(switchKey, out string status))
                {
                    if(status == "on")
                    {
                        __instance.initializeLightSource(__instance.TileLocation);
                    }
                    else
                    {
                        __instance.lightSource = null;
                        environment.removeLightSource((int)(__instance.TileLocation.X * 2000f + __instance.TileLocation.Y));
                        __instance.RemoveLightGlow(Game1.currentLocation);
                    }
                }
            }
        }
    }
}