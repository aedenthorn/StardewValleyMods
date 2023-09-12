using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Objects;
using System;
using Object = StardewValley.Object;

namespace LightMod
{
    public partial class ModEntry
    {
        private void ChangeLightAlpha(NPC value, int delta)
        {
            if (!lightDataDict.TryGetValue(value.Name, out var data))
                return;
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
                shiftAmount += data.color.A;
            }
            shiftAmount = Math.Max(0, Math.Min(255, shiftAmount));
            value.modData[alphaKey] = shiftAmount + "";
            SMonitor.Log($"Set alpha to {shiftAmount}");
        }
        private void ChangeLightRadius(NPC value, int delta)
        {
            if (!lightDataDict.TryGetValue(value.Name, out var data))
                return;
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
                shiftAmount += data.radius;
            }
            shiftAmount = (float)Math.Max(0, shiftAmount);

            value.modData[radiusKey] = shiftAmount + "";
            SMonitor.Log($"Set radius to {shiftAmount}");
        }
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
        
        private void ChangeLightGlowAlpha(Vector2 light, int delta)
        {
            suppressingScroll = true;
            string key = $"{alphaKey}_{light.X}_{light.Y}";
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
            if (Game1.currentLocation.modData.TryGetValue(key, out string alphaString) && int.TryParse(alphaString, out int oldAlpha))
            {
                shiftAmount += oldAlpha;
            }
            else
            {
                shiftAmount += 255;
            }
            shiftAmount = Math.Max(0, Math.Min(255, shiftAmount));
            Game1.currentLocation.modData[key] = shiftAmount + "";
            SMonitor.Log($"Set light glow {light} alpha to {shiftAmount}");
        }

        private static Color GetLightGlowAlpha(GameLocation l, Vector2 light)
        {
            if (!Config.ModEnabled || !l.modData.TryGetValue($"{alphaKey}_{light.X}_{light.Y}", out string alphaString) || !int.TryParse(alphaString, out int alpha))
                return Color.White;
            return Color.White * (alpha / 255f);
        }


        private void ToggleLight(Object value)
        {
            if (value.modData.TryGetValue(switchKey, out string status))
            {
                if (status == "off")
                {
                    TurnOnLight(value);
                }
                else
                {
                    TurnOffLight(value);
                }
            }
            else
            {
                if (value.lightSource is null)
                {
                    value.initializeLightSource(Game1.currentCursorTile);
                    if (value.lightSource is not null)
                    {
                        Monitor.Log($"turning on {value.Name}");
                        value.modData[switchKey] = "on";
                        int ident = (int)(value.TileLocation.X * 2000f + value.TileLocation.Y);
                        if (value.lightSource is not null && !Game1.currentLocation.hasLightSource(ident))
                            Game1.currentLocation.sharedLights[ident] = value.lightSource.Clone();
                        if (value is Furniture)
                        {
                            (value as Furniture).addLights(Game1.currentLocation);
                        }
                    }
                }
                else
                {
                    TurnOffLight(value);
                }
            }
        }

        private void TurnOffLight(Object value)
        {
            Monitor.Log($"turning off {value.Name}");
            value.modData[switchKey] = "off";
            if (value.lightSource != null)
            {
                value.lightSource = null;
            }
            Game1.currentLocation.removeLightSource((int)(value.TileLocation.X * 2000f + value.TileLocation.Y));
            if (value is Furniture)
            {
                (value as Furniture).removeLights(Game1.currentLocation);
            }
        }

        private void TurnOnLight(Object value)
        {
            Monitor.Log($"turning on {value.Name}");
            value.modData[switchKey] = "on";
            value.initializeLightSource(Game1.currentCursorTile);
            int ident = (int)(value.TileLocation.X * 2000f + value.TileLocation.Y);
            if (value.lightSource is not null && !Game1.currentLocation.hasLightSource(ident))
                Game1.currentLocation.sharedLights[ident] = value.lightSource.Clone();
            if (value is Furniture)
            {
                (value as Furniture).addLights(Game1.currentLocation);
                value.IsOn = true;
            }
        }


        private static int GetMorningLightTime()
        {
        /*
            switch (Game1.currentSeason)
            {
                case "spring":
                    return Config.SpringMorningLightTime;
                case "summer":
                    return Config.SummerMorningLightTime;
                case "fall":
                    return Config.SummerMorningLightTime;
                case "winter":
                    return Config.SummerMorningLightTime;
            }
        */
            return 0;
        }
        private static int GetNightDarkTime()
        {
            /*
            switch (Game1.currentSeason)
            {
                case "spring":
                    return Config.SpringDarkTime;
                case "summer":
                    return Config.SummerDarkTime;
                case "fall":
                    return Config.FallDarkTime;
                case "winter":
                    return Config.WinterDarkTime;
            }
        */
            return 1600;
        }
    }
}