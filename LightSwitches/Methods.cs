using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Objects;
using System;
using System.Linq;
using Color = Microsoft.Xna.Framework.Color;

namespace LightSwitches
{
    public partial class ModEntry
    {
        public static void ToggleSwitch(Furniture f, LightSwitchData data)
        {
            if (!Config.ModEnabled)
                return;
            if (f is null || f.Location is not GameLocation l)
            {
                SMonitor.Log("Error, switch is null or location is null", StardewModdingAPI.LogLevel.Warn);
                return;
            }
            if(!f.modData.TryGetValue(onOffKey, out var str))
            {
                str = "off";
            }
            bool on = str == "off";
            f.modData[onOffKey] = on ? "on" : "off";
            l.modData[onOffKey] = on ? "on" : "off";
            if(f.modData.TryGetValue(onTimeKey, out var onTime))
            {
                l.modData[onTimeKey] = onTime;
            }
            else
            {
                l.modData.Remove(onTimeKey);
            }
            if(f.modData.TryGetValue(offTimeKey, out var offTime))
            {
                l.modData[offTimeKey] = offTime;
            }
            else
            {
                l.modData.Remove(offTimeKey);
            }
            if (on)
            {
                string color;
                if (data.Prismatic)
                {
                    color = f.ItemId;
                }
                else if (!f.modData.TryGetValue(colorKey, out color))
                    color = "#FFFFFF";
                l.modData[colorKey] = color;
                if(data.StrobeSpeed > 0)
                {
                    l.modData[strobeKey] = data.StrobeSpeed.ToString();
                }
                else
                {
                    l.modData.Remove(strobeKey);
                }
                
                foreach (var f2 in l.furniture.Where(f => LightSwitches.ContainsKey(f.ItemId)))
                {
                    if(f2 != f)
                        f2.modData[onOffKey] = "off";
                }
            }
            l.playSound(on ? Config.OnSound : Config.OffSound, f.TileLocation);

        }
        public static bool TrySetAmbientLight(GameLocation l, float lightLevel)
        {
            if (!Config.ModEnabled || lightLevel > 0f || (l.IsOutdoors && Config.IndoorsOnly) || !l.modData.TryGetValue(onOffKey, out var str) || str != "on")
            {
                return true;
            }
            if(l.modData.TryGetValue(strobeKey, out var sstr) && int.TryParse(sstr, out var s) && Game1.currentGameTime.TotalGameTime.TotalMilliseconds % (s * 2) < s)
            {
                return true;
            }
            var color = GetLightColor(l);

            System.Drawing.Color scolor = System.Drawing.Color.FromArgb(color.A, color.R, color.G, color.B);
            scolor = System.Drawing.Color.FromArgb(scolor.ToArgb() ^ 0xffffff);
            
            Game1.ambientLight = new(scolor.R, scolor.G, scolor.B);
            return false;

        }

        private static bool IsOffTime(Furniture f)
        {
            if (!f.modData.TryGetValue(onTimeKey, out var onTime))
                return false;
            if (Game1.timeOfDay < int.Parse(onTime))
                return true;
            if (!f.modData.TryGetValue(offTimeKey, out var offTime))
                return false;
            return Game1.timeOfDay >= int.Parse(offTime);
        }
        private static bool IsOnTime(Furniture f, int time)
        {
            if (!f.modData.TryGetValue(onTimeKey, out var onTime))
                return false;
            return time == int.Parse(onTime);
        }

        public static Color GetLightColor(GameLocation l)
        {
            if (!l.modData.TryGetValue(colorKey, out var cstr))
                return Color.White;
            if(LightSwitches.TryGetValue(cstr, out var data) && data.Prismatic)
            {
                if(data.PrismaticStart is not null && data.PrismaticEnd is not null)
                {
                    return Utility.Get2PhaseColor(Utility.StringToColor(data.PrismaticStart).Value, Utility.StringToColor(data.PrismaticEnd).Value, data.PrismaticSpeed);
                }
                else
                {
                    return Utility.GetPrismaticColor(0, data.PrismaticSpeed);
                }
            }
            return Utility.StringToColor(cstr) ?? Color.White;
        }

        public static string ColorToHexString(Color value)
        {
            return $"#{value.R:X2}{value.G:X2}{value.B:X2}";
        }
        public static Color SetColorForTranspiler(Color color, Furniture f)
        {
            if (!Config.ModEnabled || !LightSwitches.ContainsKey(f.ItemId) || !f.modData.TryGetValue(colorKey, out var str))
                return color;
            return Utility.StringToColor(str) ?? color;
        }
        public static Texture2D SetTextureForTranspiler(Texture2D tex, Furniture f)
        {
            if (!Config.ModEnabled || !LightSwitches.TryGetValue(f.ItemId, out var data))
                return tex;
            if (!f.modData.TryGetValue(onOffKey, out var str))
            {
                str = "off";
                f.modData[onOffKey] = str;
            }
            if(str == "off" && data.OffTexture != null)
            {
                return SHelper.GameContent.Load<Texture2D>(data.OffTexture);
            }
            if(str == "on" && data.OnTexture != null)
            {
                return SHelper.GameContent.Load<Texture2D>(data.OnTexture);
            }
            return tex;
        }
        public static bool IsOn(Furniture f)
        {
            return f.modData.TryGetValue(ModEntry.onOffKey, out var on) && on == "on";
        }

    }
}