using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Extensions;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;

namespace FlowerColors
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry : Mod
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;
        public static ModEntry context;
        public static IPrismaticFlowersAPI prismaticFlowersAPI;
        public const string colorsKey = "aedenthorn.PrismaticFlowers/colors";
        public const string prismaticKey = "aedenthorn.PrismaticFlowers/prismatic";
        public const string prismaticKeyDisabled = "aedenthorn.PrismaticFlowers/prismatic_";
        public static PerScreen<Color> pasteColor = new(() => Color.Transparent);
        public static PerScreen<string> pastePrismatic = new();
        public static PerScreen<Vector2> lastCursorTile = new(() => new Vector2(-1,-1));
        public static PerScreen<int> lastScrollDelta = new();
        public override void Entry(IModHelper helper)
        {
            Config = Helper.ReadConfig<ModConfig>();
            SMonitor = Monitor;
            SHelper = helper;

            context = this;

            helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            helper.Events.GameLoop.UpdateTicked += GameLoop_UpdateTicked;
            helper.Events.Input.MouseWheelScrolled += Input_MouseWheelScrolled;
            helper.Events.Input.ButtonPressed += Input_ButtonPressed;
            helper.Events.Input.ButtonsChanged += Input_ButtonsChanged;
            

            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();
        }

        private void GameLoop_UpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            if (!Config.ModEnabled || !Context.IsPlayerFree)
            {
                return;
            }
            if (lastCursorTile.Value.X < 0 || lastCursorTile.Value == Game1.currentCursorTile || pasteColor.Value == Color.Transparent)
                return;
            foreach (var k in Config.PasteButton.Keybinds)
            {
                foreach (var b in k.Buttons)
                {
                    if (!SHelper.Input.IsSuppressed(b) && !SHelper.Input.IsDown(b))
                        return;
                }
            }
            if (Game1.currentLocation.terrainFeatures.TryGetValue(Game1.currentCursorTile, out TerrainFeature tf) && tf is HoeDirt dirt && dirt.crop != null && dirt.crop.programColored.Value && TryGetColoredCrop(out Crop crop))
            {
                if(pastePrismatic.Value != null)
                {
                    crop.modData[prismaticKey] = pastePrismatic.Value;
                }
                else
                {
                    crop.modData.Remove(prismaticKey);
                    crop.tintColor.Value = pasteColor.Value;
                }
                Game1.playSound("grassyStep");
            }
            lastCursorTile.Value = Game1.currentCursorTile;
        }

        private void Input_ButtonsChanged(object sender, ButtonsChangedEventArgs e)
        {
            if (!Config.ModEnabled || !Context.IsPlayerFree)
                return;
            if (Config.CopyButton.JustPressed())
            {
                if (!TryGetColoredCrop(out Crop crop))
                    return;
                var co = new ColoredObject(crop.indexOfHarvest.Value, 1, crop.tintColor.Value);
                if (crop.modData.TryGetValue(prismaticKey, out var str))
                {
                    pastePrismatic.Value = str;
                    co.modData[prismaticKey] = str;
                }
                else
                {
                    pastePrismatic.Value = null;
                }
                pasteColor.Value = crop.tintColor.Value;
                Game1.hudMessages.Clear();
                Game1.player.ShowItemReceivedHudMessage(co, 1);
                Game1.playSound("bigSelect");
                foreach (var k in Config.CopyButton.Keybinds)
                {
                    foreach (var b in k.Buttons)
                    {
                        if (!new SButton[] { SButton.LeftControl, SButton.LeftShift, SButton.LeftAlt, SButton.RightShift, SButton.RightControl, SButton.RightAlt, }.Contains(b))
                            SHelper.Input.Suppress(b);
                    }
                }
            }
            else if(Config.PasteButton.JustPressed())
            {
                if (pasteColor.Value == Color.Transparent || !TryGetColoredCrop(out Crop crop))
                    return;
                if (pastePrismatic.Value != null)
                {
                    crop.modData[prismaticKey] = pastePrismatic.Value;
                }
                else
                {
                    crop.modData.Remove(prismaticKey);
                    crop.tintColor.Value = pasteColor.Value;
                }

                Game1.playSound("grassyStep");
                lastCursorTile.Value = Game1.currentCursorTile;
                foreach (var k in Config.PasteButton.Keybinds)
                {
                    foreach (var b in k.Buttons)
                    {
                        if (!new SButton[] { SButton.LeftControl, SButton.LeftShift, SButton.LeftAlt, SButton.RightShift, SButton.RightControl, SButton.RightAlt, }.Contains(b))
                            SHelper.Input.Suppress(b);
                    }
                }
            }
            else if(Config.PickButton.JustPressed())
            {
                if (!TryGetColoredCrop(out Crop crop))
                    return;
                if (!crop.modData.ContainsKey(prismaticKey))
                {
                    Game1.activeClickableMenu = new ColorPickMenu(crop);
                }
                foreach (var k in Config.PickButton.Keybinds)
                {
                    foreach (var b in k.Buttons)
                    {
                        SHelper.Input.Suppress(b);
                    }
                }
            }
        }

        private void Input_ButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (!Config.ModEnabled)
                return;
            if (false && Context.IsPlayerFree && Config.Debug && e.Button == SButton.OemPeriod)
            {
                var crops = new string[] { "455", "453", "429", "427", "425" };
                foreach (var kvp in Game1.getFarm().terrainFeatures.Pairs.Where(kvp => kvp.Value is HoeDirt))
                {
                    //var crop = new Crop(Game1.random.Choose(crops), (int)kvp.Key.X, (int)kvp.Key.Y, Game1.getFarm());
                    //var crop = (kvp.Value as HoeDirt).crop ?? new Crop(crops[(int)kvp.Key.Y % crops.Length], (int)kvp.Key.X, (int)kvp.Key.Y, Game1.getFarm());

                    var crop = new Crop("455", (int)kvp.Key.X, (int)kvp.Key.Y, Game1.getFarm());
                    //var crop = (kvp.Value as HoeDirt).crop ?? new Crop("455", (int)kvp.Key.X, (int)kvp.Key.Y, Game1.getFarm());
                    crop.growCompletely();
                    (kvp.Value as HoeDirt).crop = crop;
                }
            }
            if (false && Config.Debug && e.Button == SButton.OemComma)
            {
                foreach (var kvp in Game1.getFarm().terrainFeatures.Pairs.Where(kvp => kvp.Value is HoeDirt))
                {
                    //var crop = new Crop(Game1.random.Choose("455", "453", "429", "427", "425", "431"), (int)kvp.Key.X, (int)kvp.Key.Y, Game1.getFarm());

                    (kvp.Value as HoeDirt).crop?.harvest((int)kvp.Value.Tile.X, (int)kvp.Value.Tile.Y, (kvp.Value as HoeDirt));
                    (kvp.Value as HoeDirt).crop = null;
                }
            }
        }

        private void Input_MouseWheelScrolled(object sender, MouseWheelScrolledEventArgs e)
        {
            lastScrollDelta.Value = e.Delta;
            if (!Config.ModEnabled || !Context.IsPlayerFree || !TryGetColoredCrop(out Crop crop))
                return;
            if (SHelper.Input.IsDown(Config.PrismaticModKey))
            {
                if (crop.modData.TryGetValue(prismaticKey, out var prismatic))
                {
                    crop.modData[prismaticKeyDisabled] = prismatic;
                    crop.modData.Remove(prismaticKey);
                    Game1.playSound("yoba");
                    SHelper.Input.SuppressScrollWheel();
                }
                else if (crop.modData.TryGetValue(prismaticKeyDisabled, out var prismatic2))
                {
                    crop.modData[prismaticKey] = prismatic2;
                    crop.modData.Remove(prismaticKeyDisabled);
                    Game1.playSound("yoba");
                    SHelper.Input.SuppressScrollWheel();
                }
                else if (prismaticFlowersAPI?.MakePrismatic(crop) == true)
                {
                    Game1.playSound("yoba");
                    SHelper.Input.SuppressScrollWheel();
                }
            }
            else if(!crop.modData.ContainsKey(prismaticKey))
            {
                var tintColors = crop.GetData()?.TintColors;
                var newColor = GetNewColor(tintColors, crop.tintColor.Value, e.Delta);
                if (newColor is null)
                    return;
                crop.tintColor.Value = newColor.Value;
                Game1.playSound("shiny4");
                SHelper.Input.SuppressScrollWheel();
            }
        }

        private void GameLoop_GameLaunched(object sender, GameLaunchedEventArgs e)
        {
            prismaticFlowersAPI = Helper.ModRegistry.GetApi<IPrismaticFlowersAPI>("aedenthorn.PrismaticFlowers");

            var configMenu = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (configMenu is not null)
            {
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
                    else if (p.PropertyType == typeof(KeybindList))
                    {
                        configMenu.AddKeybindList(
                            mod: ModManifest,
                            name: () => SHelper.Translation.Get(p.Name),
                            tooltip: () => { var t = SHelper.Translation.Get(p.Name + ".Desc"); return t.HasValue() ? t : null; },
                            getValue: () => (KeybindList)p.GetValue(Config),
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