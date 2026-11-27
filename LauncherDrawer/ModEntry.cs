using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace LauncherDrawer
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry : Mod
    {
        public enum DrawerState
        {
            Closed,
            Open,
            Closing,
            Opening
        }
        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;
        public static ModEntry context;
        public static PerScreen<DrawerState> currentDrawerState = new(() => DrawerState.Closed);
        public static PerScreen<int> ticks = new();
        public static PerScreen<int> scrolled = new();
        public static PerScreen<string> tooltip = new();
        public const string dictPath = "aedenthorn.LauncherDrawer/dict";
        public const string keybindPrefix = "aedenthorn.LauncherDrawer/keybinds/";
        public const string linkPrefix = "aedenthorn.LauncherDrawer/links/";
        public static Dictionary<string, Dictionary<string, object>> LauncherDict 
        { 
            get
            {
                var dict = SHelper.GameContent.Load<Dictionary<string, Dictionary<string, object>>>(dictPath);
                return dict;
            }
        }
        public static List<string> sortedKeys = new();
        
        public static PerScreen<string> keybindName = new(() => "");
        public static PerScreen<string> keybindDesc = new(() => "");
        public static PerScreen<KeybindList> keybind = new(() => new KeybindList());


        public static List<string> SortedKeys {
            get
            {
                var keys = sortedKeys.Where(k => !Config.HideList.Contains(k)).Skip(scrolled.Value).Take(Config.MaxEntries > 0 ? Config.MaxEntries : sortedKeys.Count).ToList();
                return keys;
            }
            set
            {
                sortedKeys = value;
            }
        }
        public static int MenuHeight
        { 
            get
            {
                return (int)(SortedKeys.Count * 56 * ticks.Value / (float)Config.DrawerSpeed);
            }
        }

        public override void Entry(IModHelper helper)
        {
            Config = Helper.ReadConfig<ModConfig>();
            SMonitor = Monitor;
            SHelper = helper;

            context = this;

            helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            helper.Events.Input.ButtonsChanged += Input_ButtonsChanged;
            helper.Events.Input.MouseWheelScrolled += Input_MouseWheelScrolled;
            helper.Events.Content.AssetRequested += Content_AssetRequested;
            helper.Events.Content.AssetReady += Content_AssetReady;
            helper.Events.Display.RenderedStep += Display_RenderedStep;
            
            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();
            
        }

        private void Input_MouseWheelScrolled(object sender, MouseWheelScrolledEventArgs e)
        {
            if (!Config.ModEnabled || currentDrawerState.Value != DrawerState.Open || Config.MaxEntries < 0)
                return;
            var count = LauncherDict.Keys.Where(k => !Config.HideList.Contains(k)).Count();
            if(count <= Config.MaxEntries)
                return;
            Vector2 position = GetPosition(Game1.dayTimeMoneyBox.position);
            var bounds = GetBounds(position, MenuHeight, 0);
            if (bounds.Contains(Game1.getMousePosition(true)))
            {
                if(e.Delta > 0 && scrolled.Value > 0)
                {
                    scrolled.Value--;
                    Game1.playSound("shiny4");
                }
                else if(e.Delta < 0 && scrolled.Value + Config.MaxEntries < count)
                {
                    scrolled.Value++;
                    Game1.playSound("shiny4");
                }
                SHelper.Input.SuppressScrollWheel();
            }
        }
        private void Display_RenderedStep(object sender, RenderedStepEventArgs e)
        {
            if (!Config.ModEnabled)
                return;
            if (e.Step == StardewValley.Mods.RenderSteps.Overlays && tooltip.Value is not null)
            {
                IClickableMenu.drawHoverText(e.SpriteBatch, tooltip.Value, Game1.smallFont);
                tooltip.Value = null;
            }
        }

        private void Content_AssetReady(object sender, AssetReadyEventArgs e)
        {
            if (true || e.NameWithoutLocale.IsEquivalentTo(dictPath))
            {
                var dict = LauncherDict;
                sortedKeys = dict.Keys.ToList();
                sortedKeys.Sort((string a, string b) =>
                {
                    return (dict[a].TryGetValue("Name", out var name) ? name.ToString() : a).CompareTo(dict[b].TryGetValue("Name", out var name2) ? name2.ToString() : b);
                });
            }
        }

        private void Content_AssetRequested(object sender, AssetRequestedEventArgs e)
        {
            if (!Config.ModEnabled)
                return;
            if (e.NameWithoutLocale.IsEquivalentTo(dictPath))
            {
                var dict = new Dictionary<string, Dictionary<string, object>>();
                if(Config.Keybinds is not null)
                {
                    for (int i = 0; i < Config.Keybinds.Count; i++)
                    {
                        var split = Config.Keybinds[i].Split('|');
                        if (split.Length < 3)
                            continue;
                        dict[keybindPrefix + i] = new()
                            {
                                { "Name", split[0] },
                                { "Description", split[1]  },
                                { "Keybind", split[2].Split(',') },
                            };
                    }
                }
                if(Config.Links is not null)
                {
                    for (int i = 0; i < Config.Links.Count; i++)
                    {
                        var split = Config.Links[i].Split('|');
                        if (split.Length < 3)
                            continue;
                        dict[linkPrefix + i] = new()
                            {
                                { "Name", split[0] },
                                { "Description", split[1]  },
                                { "Link", split[2] },
                            };
                    }
                }
                e.LoadFrom(() => dict, AssetLoadPriority.Exclusive);
            }
        }

        private void Input_ButtonsChanged(object sender, ButtonsChangedEventArgs e)
        {
            if (Config.Debug)
            {
                //var dict = LauncherDict;
                //SHelper.GameContent.InvalidateCache(dictPath);
            }
            if (!Config.ModEnabled || !Context.IsPlayerFree)
                return;
            if (!LauncherDict.Any())
                return;
            if (Config.DrawerKey.JustPressed())
            {
                if (ToggleMenu())
                {
                    Suppress(Config.DrawerKey);
                }
                return;
            }
            if (Config.HideKey.JustPressed())
            {
                var keys = SortedKeys;
                var dict = LauncherDict;
                var height = GetHeight(keys);
                var per = height / keys.Count;
                int count = 0;
                Vector2 position = GetPosition(Game1.dayTimeMoneyBox.position);
                var x = Game1.getMouseX(true);
                var y = Game1.getMouseY(true);
                foreach (var key in SortedKeys)
                {
                    Rectangle bounds = GetBounds(position, per, count++);
                    if (bounds.Contains(x, y))
                    {
                        var name = LauncherDict[key]["Name"];
                        foreach (var str in Config.Keybinds)
                        {
                            if (str.StartsWith(name + "|"))
                            {
                                Config.Keybinds.Remove(str);
                                SHelper.GameContent.InvalidateCache(dictPath);
                                goto cont;
                            }
                        }
                        foreach(var str in Config.Links)
                        {
                            if (str.StartsWith(name + "|"))
                            {
                                Config.Links.Remove(str);
                                SHelper.GameContent.InvalidateCache(dictPath);
                                goto cont;
                            }
                        }
                        Config.HideList.Add(key);
                    cont:
                        SHelper.WriteConfig(Config);
                        Suppress(Config.HideKey);
                        Game1.playSound("grassyStep");
                        return;
                    }
                }
                return;
            }
        }

        private void Suppress(KeybindList keylist)
        {
            List<SButton> buttons = new()
            {
                SButton.LeftShift,
                SButton.LeftControl,
                SButton.LeftAlt,
                SButton.RightShift,
                SButton.RightControl,
                SButton.RightAlt
            };
            foreach (var bind in keylist.Keybinds)
            {
                foreach (var key in bind.Buttons)
                {
                    if(!buttons.Contains(key))
                        SHelper.Input.Suppress(key);
                }
            }
        }

        private void GameLoop_GameLaunched(object sender, GameLaunchedEventArgs e)
        {
            SHelper.GameContent.InvalidateCache(dictPath);
            var configMenu = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (configMenu is not null)
            {
                configMenu.Register(
                    mod: ModManifest,
                    reset: () => Config = new ModConfig(),
                    save: () => Helper.WriteConfig(Config)
                );

                List<string> exclude = new()
                {
                    "Debug"
                };

                var props = typeof(ModConfig).GetProperties().ToArray();
                var configMenuExt = Helper.ModRegistry.GetApi<IGMCMOptionsAPI>("jltaylor-us.GMCMOptions");

                foreach (var p in props)
                {
                    if (exclude.Contains(p.Name))
                        continue;
                    if (p.PropertyType == typeof(bool))
                    {
                        configMenu.AddBoolOption(
                            mod: ModManifest,
                            name: () => { var t = Helper.Translation.Get(p.Name); return t.HasValue() ? t : AddSpaces(p.Name); },
                            tooltip: () => { var t = Helper.Translation.Get(p.Name + ".Desc"); return t.HasValue() ? t : null; },
                            getValue: () => (bool)p.GetValue(Config),
                            setValue: value => p.SetValue(Config, value)
                        );
                    }
                    else if (p.PropertyType == typeof(int))
                    {
                        configMenu.AddNumberOption(
                            mod: ModManifest,
                            name: () => { var t = Helper.Translation.Get(p.Name); return t.HasValue() ? t : AddSpaces(p.Name); },
                            tooltip: () => { var t = Helper.Translation.Get(p.Name + ".Desc"); return t.HasValue() ? t : null; },
                            getValue: () => (int)p.GetValue(Config),
                            setValue: value => p.SetValue(Config, value)
                        );
                    }
                    else if (p.PropertyType == typeof(float))
                    {
                        configMenu.AddNumberOption(
                            mod: ModManifest,
                            name: () => { var t = Helper.Translation.Get(p.Name); return t.HasValue() ? t : AddSpaces(p.Name); },
                            tooltip: () => { var t = Helper.Translation.Get(p.Name + ".Desc"); return t.HasValue() ? t : null; },
                            getValue: () => (float)p.GetValue(Config),
                            setValue: value => p.SetValue(Config, value)
                        );
                    }
                    else if (p.PropertyType == typeof(double))
                    {
                        configMenu.AddTextOption(
                            mod: ModManifest,
                            name: () => { var t = Helper.Translation.Get(p.Name); return t.HasValue() ? t : AddSpaces(p.Name); },
                            tooltip: () => { var t = Helper.Translation.Get(p.Name + ".Desc"); return t.HasValue() ? t : null; },
                            getValue: () => p.GetValue(Config).ToString(),
                            setValue: value => { if (double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var d)) { p.SetValue(Config, d); } }
                        );
                    }
                    else if (p.PropertyType == typeof(string))
                    {
                        configMenu.AddTextOption(
                            mod: ModManifest,
                            name: () => { var t = Helper.Translation.Get(p.Name); return t.HasValue() ? t : AddSpaces(p.Name); },
                            tooltip: () => { var t = Helper.Translation.Get(p.Name + ".Desc"); return t.HasValue() ? t : null; },
                            getValue: () => (string)p.GetValue(Config),
                            setValue: value => p.SetValue(Config, value)
                        );
                    }
                    else if (p.PropertyType == typeof(KeybindList))
                    {
                        configMenu.AddKeybindList(
                            mod: ModManifest,
                            name: () => { var t = Helper.Translation.Get(p.Name); return t.HasValue() ? t : AddSpaces(p.Name); },
                            tooltip: () => { var t = Helper.Translation.Get(p.Name + ".Desc"); return t.HasValue() ? t : null; },
                            getValue: () => (KeybindList)p.GetValue(Config),
                            setValue: value => p.SetValue(Config, value)
                        );
                    }
                    else if (p.PropertyType == typeof(SButton))
                    {
                        configMenu.AddKeybind(
                            mod: ModManifest,
                            name: () => { var t = Helper.Translation.Get(p.Name); return t.HasValue() ? t : AddSpaces(p.Name); },
                            tooltip: () => { var t = Helper.Translation.Get(p.Name + ".Desc"); return t.HasValue() ? t : null; },
                            getValue: () => (SButton)p.GetValue(Config),
                            setValue: value => p.SetValue(Config, value)
                        );
                    }
                    else if (p.PropertyType == typeof(Color) && configMenuExt is not null)
                    {
                        configMenuExt.AddColorOption(
                            mod: ModManifest,
                            name: () => { var t = Helper.Translation.Get(p.Name); return t.HasValue() ? t : AddSpaces(p.Name); },
                            tooltip: () => { var t = Helper.Translation.Get(p.Name + ".Desc"); return t.HasValue() ? t : null; },
                            getValue: () => (Color)p.GetValue(Config),
                            setValue: value => p.SetValue(Config, value)
                        );
                    }
                }
                configMenu.AddSectionTitle(
                    mod: ModManifest,
                    text: () => Helper.Translation.Get("add-keybind")
                );
                configMenu.AddTextOption(
                    mod: ModManifest,
                    name: () => Helper.Translation.Get("name"),
                    getValue: () => keybindName.Value,
                    setValue: value => { keybindName.Value = value; }
                );
                configMenu.AddTextOption(
                    mod: ModManifest,
                    name: () => Helper.Translation.Get("desc"),
                    getValue: () => keybindDesc.Value,
                    setValue: value => { keybindDesc.Value = value; }
                );
                configMenu.AddKeybindList(
                    mod: ModManifest,
                    name: () => Helper.Translation.Get("keybind"),
                    getValue: () => keybind.Value,
                    setValue: value => { keybind.Value = value; TryCreateKeyBind(); }
                );
                
            }
        }

        private void TryCreateKeyBind()
        {
            if(keybind.Value?.Keybinds.Length > 0 && !string.IsNullOrEmpty(keybindDesc.Value) && !string.IsNullOrEmpty(keybindName.Value))
            {
                var str = $"{keybindName.Value}|{keybindDesc.Value}|{string.Join(',', keybind.Value.Keybinds[0].Buttons)}";
                for(int i = 0; i < Config.Keybinds.Count; i++)
                {
                    if (Config.Keybinds[i].StartsWith($"{keybindName.Value}|"))
                    {
                        Config.Keybinds[i] = str;
                        goto cont;
                    }
                }
                Config.Keybinds.Add(str);
            cont:
                Helper.WriteConfig(Config);
                Helper.GameContent.InvalidateCache(dictPath);
                keybind.Value = new KeybindList();
                keybindName.Value = "";
                keybindDesc.Value = "";
            }
        }


        public static string AddSpaces(string str)
        {
            string newStr = "";
            foreach (var c in str)
            {
                if (c >= 'A' && c <= 'Z' && newStr.Length > 0)
                {
                    newStr += " ";
                }
                newStr += c;
            }
            return newStr;
        }
    }
    public static class Extensions
    {
        public static void Shuffle<T>(this List<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = Game1.random.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
    }
}