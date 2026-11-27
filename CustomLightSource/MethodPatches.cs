using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;

namespace CustomLightSource
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry
    {


        private static bool LightSource_loadTextureFromConstantValue_Prefix(LightSource __instance, int value)
        {
            if (!Config.EnableMod || value < 9 || value - 8 > lightTextureList.Count)
                return true;

            __instance.lightTexture = SHelper.GameContent.Load<Texture2D>(lightTextureList[value - 9]);
            return false;
        }
        private static bool Object_initializeLightSource_Prefix(Object __instance, Vector2 tileLocation)
        {
            if (!Config.EnableMod)
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
    }
}