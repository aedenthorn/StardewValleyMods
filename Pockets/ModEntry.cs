using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Newtonsoft.Json;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Inventories;
using StardewValley.Menus;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json.Nodes;

namespace Pockets
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry : Mod
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;
        public static ModEntry context;
        private static Dictionary<Item, Dictionary<string, Inventory>> InventoryDict = new();
        private static Dictionary<long, Dictionary<string, Inventory>> DefaultInventoryDict = new ();
        public const string dictPath = "aedenthorn.Pockets/dict";
        public const string modKey = "aedenthorn.Pockets/pocket";
        public static PocketData openPocket;

        public static Dictionary<string, Dictionary<string, PocketData>> PocketDict
        {
            get
            {
                return SHelper.GameContent.Load<Dictionary<string, Dictionary<string, PocketData>>>(dictPath);
            }
        }
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            Config = Helper.ReadConfig<ModConfig>();
            SMonitor = Monitor;
            SHelper = helper;

            context = this;

            helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            helper.Events.GameLoop.ReturnedToTitle += GameLoop_ReturnedToTitle;
            helper.Events.GameLoop.SaveLoaded += GameLoop_SaveLoaded;
            helper.Events.GameLoop.Saving += GameLoop_Saving;
            helper.Events.Display.MenuChanged += Display_MenuChanged;
            helper.Events.Content.AssetRequested += Content_AssetRequested;
            helper.Events.Input.ButtonPressed += Input_ButtonPressed;
            

            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();
        }

        private void Input_ButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (!Config.ModEnabled || !Context.IsWorldReady ||
                (Game1.activeClickableMenu is not null && Game1.activeClickableMenu is not GameMenu) ||
                (Game1.activeClickableMenu is null && Config.HoverForHotkey && !new Rectangle(Game1.GlobalToLocal(Game1.player.Position).ToPoint() + new Point(0, -64), new Point(64, 128)).Contains(Game1.getMousePosition())))
                return;
            GameMenu menu = Game1.activeClickableMenu as GameMenu;
            if (menu is not null && menu.currentTab == GameMenu.inventoryTab && Game1.options.doesInputListContain(Game1.options.menuButton, (Keys)e.Button) && openPocket is not null)
            {
                ClosePocket(menu.GetCurrentPage() as InventoryPage, true);
                SHelper.Input.Suppress(e.Button);
                return;
            }
            if (!TryGetPocket(Game1.player, out var data, out var inv))
            {
                return;
            }
            foreach (var key in data.HotKey.Buttons)
            {
                SHelper.Input.Suppress(key);
            }
            if (Game1.activeClickableMenu is null)
            {
                menu = new GameMenu(GameMenu.inventoryTab, -1, true);
                Game1.activeClickableMenu = menu;
            }
            else if(menu.currentTab != GameMenu.inventoryTab) 
            {
                menu.changeTab(GameMenu.inventoryTab);
            }
            OpenPocket(menu.GetCurrentPage() as InventoryPage, data, inv, true);
        }

        private void GameLoop_SaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            InventoryDict.Clear();
            DefaultInventoryDict.Clear();
            Game1.player.shirtItem.fieldChangeEvent -= ShirtItem_fieldChangeEvent;
            Game1.player.pantsItem.fieldChangeEvent -= PantsItem_fieldChangeEvent;
            Game1.player.shirtItem.fieldChangeEvent += ShirtItem_fieldChangeEvent;
            Game1.player.pantsItem.fieldChangeEvent += PantsItem_fieldChangeEvent;
        }

        private void GameLoop_ReturnedToTitle(object sender, ReturnedToTitleEventArgs e)
        {
            InventoryDict.Clear();
            DefaultInventoryDict.Clear();
        }

        private void Content_AssetRequested(object sender, AssetRequestedEventArgs e)
        {
            if(e.NameWithoutLocale.IsEquivalentTo(dictPath))
            {
                e.LoadFrom(() => new Dictionary<string, Dictionary<string, PocketData>>(), AssetLoadPriority.Medium);
                if (Config.Debug && false)
                {
                    e.Edit((IAssetData data) =>
                    {
                        var dict = data.AsDictionary<string, Dictionary<string, PocketData>>().Data;
                        dict["1007"] = new Dictionary<string, PocketData>()
                        {
                            ["RightPocket"] = new PocketData()
                            {
                                HotKey = new Keybind(SButton.RightControl, SButton.L),
                                PocketSlots = 12,
                                PocketRows = 3,
                                StartX = 16,
                                StartY = 36,
                                Width = 16,
                                Height = 24
                            },
                            ["LeftPocket"] = new PocketData()
                            {
                                HotKey = new Keybind(SButton.RightControl, SButton.R),
                                PocketSlots = 12,
                                PocketRows = 3,
                                StartX = 32,
                                StartY = 36,
                                Width = 16,
                                Height = 24
                            }
                        };
                        File.WriteAllText(Path.Combine(SHelper.DirectoryPath, "test.json"), JsonConvert.SerializeObject(dict, Formatting.Indented));
                    });
                }
            }
        }

        private void Display_MenuChanged(object sender, MenuChangedEventArgs e)
        {
            if(e.NewMenu is not GameMenu menu || menu.currentTab != GameMenu.inventoryTab)
                openPocket = null;
        }

        private void GameLoop_Saving(object sender, SavingEventArgs e)
        {
            foreach(var kvp in InventoryDict)
            {
                kvp.Key.modData[modKey] = MakeXMLFromInventories(kvp.Value);
            }
            foreach(var kvp in DefaultInventoryDict)
            {
                Game1.GetPlayer(kvp.Key).modData[modKey] = MakeXMLFromInventories(kvp.Value);
            }
        }

        private void GameLoop_GameLaunched(object sender, GameLaunchedEventArgs e)
        {   
            // get Generic Mod Config Menu's API (if it's installed)
            var configMenu = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (configMenu is not null)
            {
                // register mod
                configMenu.Register(
                    mod: ModManifest,
                    reset: () => Config = new ModConfig(),
                    save: () => Helper.WriteConfig(Config)
                );

                configMenu.AddBoolOption(
                    mod: ModManifest,
                    name: () => SHelper.Translation.Get("ModEnabled"),
                    getValue: () => Config.ModEnabled,
                    setValue: value => Config.ModEnabled = value
                );
                var props = typeof(ModConfig).GetProperties().ToArray();
                var configMenuExt = Helper.ModRegistry.GetApi<IGMCMOptionsAPI>("jltaylor-us.GMCMOptions");

                foreach (var p in props)
                {
                    if (p.Name == nameof(Config.ModEnabled) || p.Name == nameof(Config.Debug))
                        continue;
                    if (p.PropertyType == typeof(bool))
                    {
                        configMenu.AddBoolOption(
                            mod: ModManifest,
                            name: () => SHelper.Translation.Get(p.Name),
                            tooltip: () => { var t = SHelper.Translation.Get(p.Name + ".Desc"); return t.HasValue() ? t : null; },
                            getValue: () => (bool)p.GetValue(Config),
                            setValue: value => p.SetValue(Config, value)
                        );
                    }
                    else if (p.PropertyType == typeof(int))
                    {
                        configMenu.AddNumberOption(
                            mod: ModManifest,
                            name: () => SHelper.Translation.Get(p.Name),
                            tooltip: () => { var t = SHelper.Translation.Get(p.Name + ".Desc"); return t.HasValue() ? t : null; },
                            getValue: () => (int)p.GetValue(Config),
                            setValue: value => p.SetValue(Config, value)
                        );
                    }
                    else if (p.PropertyType == typeof(float))
                    {
                        configMenu.AddTextOption(
                            mod: ModManifest,
                            name: () => SHelper.Translation.Get(p.Name),
                            tooltip: () => { var t = SHelper.Translation.Get(p.Name + ".Desc"); return t.HasValue() ? t : null; },
                            getValue: () => p.GetValue(Config).ToString(),
                            setValue: value => { if (float.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var f)) { p.SetValue(Config, f); } }
                        );
                    }
                    else if (p.PropertyType == typeof(double))
                    {
                        configMenu.AddTextOption(
                            mod: ModManifest,
                            name: () => SHelper.Translation.Get(p.Name),
                            tooltip: () => { var t = SHelper.Translation.Get(p.Name + ".Desc"); return t.HasValue() ? t : null; },
                            getValue: () => p.GetValue(Config).ToString(),
                            setValue: value => { if (double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var d)) { p.SetValue(Config, d); } }
                        );
                    }
                    else if (p.PropertyType == typeof(string))
                    {
                        configMenu.AddTextOption(
                            mod: ModManifest,
                            name: () => SHelper.Translation.Get(p.Name),
                            tooltip: () => { var t = SHelper.Translation.Get(p.Name + ".Desc"); return t.HasValue() ? t : null; },
                            getValue: () => (string)p.GetValue(Config),
                            setValue: value => p.SetValue(Config, value)
                        );
                    }
                    else if (p.PropertyType == typeof(SButton))
                    {
                        configMenu.AddKeybind(
                            mod: ModManifest,
                            name: () => SHelper.Translation.Get(p.Name),
                            tooltip: () => { var t = SHelper.Translation.Get(p.Name + ".Desc"); return t.HasValue() ? t : null; },
                            getValue: () => (SButton)p.GetValue(Config),
                            setValue: value => p.SetValue(Config, value)
                        );
                    }
                    else if (p.PropertyType == typeof(Color) && configMenuExt is not null)
                    {
                        configMenuExt.AddColorOption(
                            mod: ModManifest,
                            name: () => SHelper.Translation.Get(p.Name),
                            tooltip: () => { var t = SHelper.Translation.Get(p.Name + ".Desc"); return t.HasValue() ? t : null; },
                            getValue: () => (Color)p.GetValue(Config),
                            setValue: value => p.SetValue(Config, value)
                        );
                    }
                }
            }
        }
    }
}