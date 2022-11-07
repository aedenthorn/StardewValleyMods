using Newtonsoft.Json.Linq;
using StardewValley;
using StardewValley.Objects;
using System;
using Object = StardewValley.Object;

namespace LightMod
{
    public partial class ModEntry
    {
        private void ChangeLightAlpha(Object value, int delta)
        {
            int ident = (int)(Game1.currentCursorTile.X * 2000f + Game1.currentCursorTile.Y);
            if (Game1.currentLocation.sharedLights.TryGetValue(ident, out LightSource l))
            {
                suppressingScroll = true;
                int shiftAmount = Config.AlphaAmount;
                if (Helper.Input.IsDown(Config.ModButton1))
                {
                    shiftAmount = Config.Alpha1Amount;
                }
                else if (Helper.Input.IsDown(Config.ModButton2))
                {
                    shiftAmount = Config.Alpha2Amount;
                }
                shiftAmount *= (delta > 0 ? 1 : -1);
                if (value.modData.TryGetValue(alphaKey, out string alphaString) && int.TryParse(alphaString, out int oldAlpha))
                {
                    shiftAmount += oldAlpha;
                }
                else
                {
                    shiftAmount += l.color.A;
                }
                shiftAmount = Math.Max(0, Math.Min(255, shiftAmount));
                value.modData[alphaKey] = shiftAmount + "";
                SMonitor.Log($"Set alpha to {shiftAmount}");
                Game1.currentLocation.removeLightSource(ident);
                value.initializeLightSource(value.TileLocation);
            }
        }
        private void ChangeLightRadius(Object value, int delta)
        {
            int ident = (int)(Game1.currentCursorTile.X * 2000f + Game1.currentCursorTile.Y);
            if (Game1.currentLocation.sharedLights.TryGetValue(ident, out LightSource l))
            {
                suppressingScroll = true;
                
                float shiftAmount = Config.RadiusAmount;
                if (Helper.Input.IsDown(Config.ModButton1))
                {
                    shiftAmount = Config.Radius1Amount;
                }
                else if (Helper.Input.IsDown(Config.ModButton2))
                {
                    shiftAmount = Config.Radius2Amount;
                }
                shiftAmount *= (delta > 0 ? 1 : -1);

                if (value.modData.TryGetValue(radiusKey, out string radiusString) && int.TryParse(radiusString, out int oldRad))
                {
                    shiftAmount += oldRad;
                }
                else
                {
                    shiftAmount += l.radius.Value;
                }
                shiftAmount = (float)Math.Max(0, shiftAmount);

                value.modData[radiusKey] = shiftAmount + "";
                SMonitor.Log($"Set radius to {shiftAmount}");
                Game1.currentLocation.removeLightSource(ident);
                value.initializeLightSource(value.TileLocation);
            }
        }
    }
}