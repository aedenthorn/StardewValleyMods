using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Objects;
using System;
using System.Globalization;
using System.Linq;
using Object = StardewValley.Object;

namespace LightMod
{
    public partial class ModEntry
    {
        [HarmonyPatch(typeof(LightSource), "loadTextureFromConstantValue")]
        private static class LightSource_loadTextureFromConstantValue_Patch
        {
            public static bool Prefix(LightSource __instance, int value)
            {
                if (!Config.ModEnabled || value < 9 || value - 8 > lightTextureList.Count)
                    return true;

                __instance.lightTexture = SHelper.GameContent.Load<Texture2D>(lightTextureList[value - 9]);
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

                LightData light = null;
                if (lightDataDict.ContainsKey(__instance.ParentSheetIndex + ""))
                    light = lightDataDict[__instance.ParentSheetIndex + ""];
                else if (lightDataDict.ContainsKey(__instance.Name))
                    light = lightDataDict[__instance.Name];
                else
                    return true;

                SMonitor.Log($"Adding light to {__instance.Name}");

                int identifier = (int)(tileLocation.X * 2000f + tileLocation.Y);
                __instance.lightSource = new LightSource(light.textureIndex, new Vector2(tileLocation.X * 64f + light.offset.X, tileLocation.Y * 64f + light.offset.Y), light.radius, new Color(255 - light.color.R, 255 - light.color.G, 255 - light.color.B, 255 - light.color.A), identifier, LightSource.LightContext.None, 0L);
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

        [HarmonyPatch(typeof(Game1), nameof(Game1.pressSwitchToolButton))]
        private static class Farmer_pressSwitchToolButton_Patch
        {
            public static bool Prefix()
            {
                return (!Config.ModEnabled || !suppressingScroll);

            }
        }
    }
}