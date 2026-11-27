using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;
using System.Linq;
using System.Text;

namespace LauncherDrawer
{
    public partial class ModEntry
    {

        [HarmonyPatch(typeof(DayTimeMoneyBox), nameof(DayTimeMoneyBox.drawMoneyBox))]
        public static class DayTimeMoneyBox_drawMoneyBox_Patch
        {
            public static void Prefix(DayTimeMoneyBox __instance, SpriteBatch b, int overrideX, ref int overrideY)
            {
                if (!Config.ModEnabled || currentDrawerState.Value == DrawerState.Closed)
                    return;
                var keys = SortedKeys;
                if (!keys.Any())
                    return;
                var dict = LauncherDict;
                var height = GetHeight(keys);
                var per = height / keys.Count;
                if(!Config.CustomPosition)
                    overrideY = height + 184;
                int count = 0;
                Vector2 position = GetPosition(__instance.position);
                foreach (var key in keys)
                {
                    try
                    {
                        Rectangle bounds = GetBounds(position, per, count++);
                        IClickableMenu.drawTextureBox(b, bounds.X, bounds.Y, bounds.Width, per + 4, Color.White);
                        if(currentDrawerState.Value == DrawerState.Open)
                        {
                            string name = dict[key]["Name"].ToString();
                            var w = Game1.smallFont.MeasureString(name).X;
                            Utility.drawTextWithShadow(b, name, Game1.smallFont, new Vector2(bounds.X + (bounds.Width - w) / 2, bounds.Y + 16), Game1.textColor);
                        }
                    }
                    catch { }
                }
                if(currentDrawerState.Value == DrawerState.Closing)
                {
                    ticks.Value--;
                    if(ticks.Value <= 0)
                    {
                        currentDrawerState.Value = DrawerState.Closed;
                    }
                }
                else if(currentDrawerState.Value == DrawerState.Opening)
                {
                    ticks.Value++;
                    if(ticks.Value >= Config.DrawerSpeed)
                    {
                        currentDrawerState.Value = DrawerState.Open;
                    }
                }
            }

        }
        [HarmonyPatch(typeof(DayTimeMoneyBox), nameof(DayTimeMoneyBox.receiveLeftClick))]
        public static class DayTimeMoneyBox_receiveLeftClick_Patch
        {
            public static bool Prefix(DayTimeMoneyBox __instance, int x, int y)
            {
                if (!Config.ModEnabled || currentDrawerState.Value > DrawerState.Open)
                    return true;
                if (!LauncherDict.Any())
                    return true;

                if (new Rectangle(__instance.xPositionOnScreen + 116 + 48 + 4, __instance.yPositionOnScreen + 68, 40, 36).Contains(x, y))
                {
                    ToggleMenu();
                    return false;
                }
                else
                {
                    var dict = LauncherDict;
                    var keys = SortedKeys;
                    var height = GetHeight(keys);
                    var per = height / keys.Count;
                    int count = 0;
                    Vector2 position = GetPosition(__instance.position);

                    foreach (var key in SortedKeys)
                    {
                        Rectangle bounds = GetBounds(position, per, count++);
                        if(bounds.Contains(x, y))
                        {
                            LaunchEntry(dict[key]);
                            return false;
                        }
                    }
                }
                return true;
            }
        }
        [HarmonyPatch(typeof(DayTimeMoneyBox), nameof(DayTimeMoneyBox.isWithinBounds))]
        public static class DayTimeMoneyBox_isWithinBounds_Patch
        {
            public static bool Prefix(DayTimeMoneyBox __instance, int x, int y,ref bool __result)
            {
                if (!Config.ModEnabled || currentDrawerState.Value > DrawerState.Open)
                    return true;
                if (!LauncherDict.Any())
                    return true;
                if (new Rectangle(__instance.xPositionOnScreen + 116 + 48 + 4, __instance.yPositionOnScreen + 68, 40, 36).Contains(x, y))
                {
                    __result = true;
                    return false;
                }
                Vector2 position = GetPosition(__instance.position);
                var bounds = GetBounds(position, GetHeight(SortedKeys), 0);
                if(bounds.Contains(x, y))
                {
                    __result = true;
                    return false;
                }
                return true;
            }
        }
        [HarmonyPatch(typeof(DayTimeMoneyBox), nameof(DayTimeMoneyBox.performHoverAction))]
        public static class DayTimeMoneyBox_performHoverAction_Patch
        {
            public static bool Prefix(DayTimeMoneyBox __instance, int x, int y, StringBuilder ____hoverText)
            {
                if (!Config.ModEnabled || currentDrawerState.Value > DrawerState.Open)
                    return true;
                if (!LauncherDict.Any())
                    return true;
                if (new Rectangle(__instance.xPositionOnScreen + 116 + 48 + 4, __instance.yPositionOnScreen + 68, 40, 36).Contains(x, y))
                {
                    ____hoverText.Clear();
                    //____hoverText.Append(SHelper.Translation.Get(drawerState.Value == DrawerState.Open ? "close-drawer" : "open-drawer"));
                    return false;

                }
                if (currentDrawerState.Value == DrawerState.Closed)
                    return true;
                var dict = LauncherDict;
                var keys = SortedKeys;
                var height = GetHeight(keys);
                var per = height / keys.Count;
                int count = 0;
                foreach (var key in SortedKeys)
                {
                    try
                    {
                        Vector2 position = GetPosition(__instance.position);
                        Rectangle bounds = GetBounds(position, per, count++);
                        if(bounds.Contains(x, y))
                        {
                            ____hoverText.Clear(); 
                            if (dict[key].TryGetValue("Description", out var str) && !string.IsNullOrEmpty(str.ToString()))
                            {
                                tooltip.Value = str.ToString();
                            }
                            return false;
                        }
                    }
                    catch { }
                }
                return true;
            }
        }
        
    }
}