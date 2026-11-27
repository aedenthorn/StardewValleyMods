using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Triggers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LauncherDrawer
{
    public partial class ModEntry
    {
        public static int GetHeight(List<string> keys)
        {
            return (int)(keys.Count * 56 * ticks.Value / (float)Config.DrawerSpeed);
        }
        public static bool ToggleMenu()
        {
            if (!Context.IsPlayerFree)
            {
                return false;
            }
            if (currentDrawerState.Value == DrawerState.Open)
            {
                ticks.Value = Config.DrawerSpeed;
                currentDrawerState.Value = DrawerState.Closing;
                Game1.playSound(Config.CloseSound);
            }
            else if (currentDrawerState.Value == DrawerState.Closed)
            {
                ticks.Value = 0;
                currentDrawerState.Value = DrawerState.Opening;
                Game1.playSound(Config.OpenSound);
            }
            else
            {
                return false;
            }
            return true;
        }
        public static Rectangle GetBounds(Vector2 position, int height, int count)
        {
            int xoff = (int)position.X + 48;
            int yoff = (int)position.Y + 172;
            int width = 220;
            return new(xoff, yoff + height * count, width, height);
        }
        public static void LaunchEntry(Dictionary<string, object> value)
        {
            try
            {
                if (value.TryGetValue("Keybind", out var obj))
                {
                    if(obj is IEnumerable list)
                    {
                        foreach (var s in list)
                        {
                            SHelper.Input.Press(Enum.Parse<SButton>(s.ToString()));
                        }
                        Game1.playSound(Config.KeybindSound);
                    }
                }
                if (value.TryGetValue("Action", out obj))
                {
                    if(obj is Action a)
                        a.Invoke();
                    else if (obj is string s)
                    {
                        TriggerActionManager.TryRunAction(s, out _, out _);
                    }
                }
                else if (value.TryGetValue("Trigger", out obj) && obj is string s)
                {
                    TriggerActionManager.TryRunAction(s, out _, out _);
                }
                else if (value.TryGetValue("Link", out obj) && obj is string s2)
                {
                    OpenUrl(s2);
                    Game1.playSound(Config.LinkSound);
                }
            }
            catch { }
        }

        public static Vector2 GetPosition(Vector2 position)
        {
            return Config.CustomPosition ? new(Config.X < 0 ? position.X - 272 : Config.X, Config.Y < 0 ? position.Y - 168 : Config.Y) : position;
        }
        private static readonly HttpClient httpClient = new();

        public static async void OpenPage(string url)
        {
            (bool isSuccess, string content) = await FetchUrlContentAsync(url);

            if (isSuccess)
            {
                //string localeCode = SHelper.Translation.Locale.Split('-').First();
                //string localizedUrl = ExtractLocalizedUrl(content, localeCode) ?? url;

                OpenUrl(url);
            }
        }
        private static async Task<(bool isSuccess, string content)> FetchUrlContentAsync(string url)
        {
            try
            {
                HttpResponseMessage response = await httpClient.GetAsync(url);
                string content = await response.Content.ReadAsStringAsync();

                return (response.IsSuccessStatusCode, content);
            }
            catch
            {
                return (false, null);
            }
        }

        private static string ExtractLocalizedUrl(string htmlContent, string localeCode)
        {
            const string regex = "<a href=\"([^\"]*)\"\\s+title=\"[^\"]*\"\\s+lang=\"([^\"]*)\"\\s+hreflang=\"([^\"]*)\"\\s+class=\"([^\"]*)\">";
            MatchCollection matches = Regex.Matches(htmlContent, regex);

            foreach (Match match in matches.Cast<Match>())
            {
                if (match.Groups.Count == 5)
                {
                    string hrefValue = match.Groups[1].Value;
                    string langValue = match.Groups[2].Value;
                    string hreflangValue = match.Groups[3].Value;
                    string classValue = match.Groups[4].Value;

                    if (langValue.Equals(localeCode) && hreflangValue.Equals(localeCode) && classValue.Equals("interlanguage-link-target"))
                    {
                        return hrefValue;
                    }
                }
            }
            return null;
        }

        private static void OpenUrl(string url)
        {
            SMonitor.Log($"Opening the link: {url}");
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error opening link: {ex.Message}");
            }
        }
    }
}