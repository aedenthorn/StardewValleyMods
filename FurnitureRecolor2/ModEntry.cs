using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace FurnitureRecolor
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry : Mod
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;
        public static ModEntry context;
        public const string dictPath = "aedenthorn.FurnitureRecolor/dict";
        public const string colorsKey = "aedenthorn.FurnitureRecolor/colors";

        public static Dictionary<string, Dictionary<Color, Texture2D>> tileSheetDict = new();
        public static Dictionary<string, Texture2D> transparentDict = new();

        public override void Entry(IModHelper helper)
        {
            Config = Helper.ReadConfig<ModConfig>();
            SMonitor = Monitor;
            SHelper = helper;

            context = this;

            helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            helper.Events.Content.AssetRequested += Content_AssetRequested;
            helper.Events.Content.AssetReady += Content_AssetReady;
            helper.Events.Input.ButtonsChanged += Input_ButtonsChanged;

            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();
        }

        private void Content_AssetReady(object sender, AssetReadyEventArgs e)
        {
            if (e.NameWithoutLocale.IsEquivalentTo("Data/Furniture"))
            {
                Dictionary<string, Color[]> textureData = new();
                var dict = SHelper.GameContent.Load<Dictionary<string, string>>("Data/Furniture");
                foreach(var itemId in dict.Keys)
                {
                    Dictionary<Color, Texture2D> textureDict = new();

                    var dir = Path.Combine(SHelper.DirectoryPath, "SpriteSheets", SanitizeFileName(itemId));
                    if (Directory.Exists(dir))
                    {
                        foreach(var f in Directory.GetFiles(dir, "*.png"))
                        {
                            var name = Path.GetFileNameWithoutExtension(f);
                            using var stream = File.OpenRead(f);
                            var tex = Texture2D.FromStream(Game1.graphics.GraphicsDevice, stream);
                            if(name == "transparent")
                            {
                                transparentDict[itemId] = tex;
                                continue;
                            }
                            var color = Utility.StringToColor(name);
                            if (color is null)
                                continue;
                            textureDict[color.Value] = tex;
                        }
                        tileSheetDict[itemId] = textureDict;
                        continue;
                    }
                    if (!DataLoader.Furniture(Game1.content).TryGetValue(itemId, out var rawData))
                    {
                        continue;
                    }

                    SMonitor.Log($"Creating sprite sheets for furniture {itemId}", LogLevel.Info);

                    var data = rawData.Split('/'); 
                    var furniture_type = Furniture.getTypeNumberFromName(data[1]);
                    var rotations = Convert.ToInt32(data[4]);
                    Point specialSpecialSourceRectOffset = ((furniture_type == 12) ? new Point(1, -1) : Point.Zero);
                    var sourceRect = Furniture.GetDefaultSourceRect(itemId, null);
                    Point specialRotationOffsets = Point.Zero;
                    switch (furniture_type)
                    {
                        case 2:
                            specialRotationOffsets = new Point(-1, 1);
                            break;
                        case 3:
                            specialRotationOffsets = new Point(-1, 1);
                            break;
                        case 5:
                            specialRotationOffsets = new Point(-1, 0);
                            break;
                    }
                    var rect = sourceRect;
                    if (rotations == 4)
                    {
                        rect.Width += sourceRect.Height - 16 + specialRotationOffsets.Y * 16 + specialSpecialSourceRectOffset.X * 16 + sourceRect.Width;
                        rect.Height = Math.Max(sourceRect.Height, sourceRect.Width + 16 + specialRotationOffsets.X * 16 + specialSpecialSourceRectOffset.Y * 16);
                    }
                    else if (rotations == 2)
                    {
                        rect.Width += sourceRect.Height - 16 + specialRotationOffsets.Y * 16 + specialSpecialSourceRectOffset.X * 16;
                        rect.Height = Math.Max(sourceRect.Height, sourceRect.Width + 16 + specialRotationOffsets.X * 16 + specialSpecialSourceRectOffset.Y * 16);
                    }

                    ParsedItemData itemData = ItemRegistry.GetDataOrErrorItem("(F)" + itemId);
                    var texture = itemData.GetTexture();
                    if (!textureData.TryGetValue(itemData.TextureName, out var pixels))
                    {
                        pixels = new Color[texture.Width * texture.Height];
                        texture.GetData(pixels);
                        textureData[itemData.TextureName] = pixels;
                    }

                    if (rect.X < 0 || rect.Y < 0 || rect.X + rect.Width > texture.Width || rect.Y + rect.Height > texture.Height)
                    {
                        SMonitor.Log($"Invalid source rect for furniture {itemId}: {rect}, skipping recolor", LogLevel.Warn);
                        continue;
                    }

                    Color[] colors = new Color[rect.Width * rect.Height];
                    Color[] transparent = new Color[rect.Width * rect.Height];

                    List<ColorGroup> groups = new List<ColorGroup>();

                    bool hasTransparency = false;
                    for (int y = rect.Y; y < rect.Y + rect.Height; y++)
                    {
                        for (int x = rect.X; x < rect.X + rect.Width; x++)
                        {
                            int i = x + y * texture.Width;
                            int j = x - rect.X + (y - rect.Y) * rect.Width;
                            if (pixels[i].A < 255)
                            {
                                if (pixels[i] != Color.Transparent)
                                {
                                    hasTransparency = true;
                                    transparent[j] = pixels[i];
                                }
                                continue;
                            }
                            bool fit = false;
                            foreach (var g in groups)
                            {
                                if (g.FitsGroup(pixels[i]))
                                {
                                    fit = true;
                                    g.AddColor(pixels[i]);
                                }
                            }
                            if (!fit)
                            {
                                var g = new ColorGroup();
                                g.AddColor(pixels[i]);
                                groups.Add(g);
                            }
                        }
                    }
                    SMonitor.Log($"\tGot {groups.Count} color groups", LogLevel.Info);

                    while (groups.Count > Config.MaxGroups)
                    {
                        groups.Sort((ColorGroup a, ColorGroup b) =>
                        {
                            return a.colors.Sum(d => d.count).CompareTo(b.colors.Sum(d => d.count));
                        });
                        foreach (var c in groups[0].colors)
                        {
                            ColorGroup best = null;
                            foreach (var g in groups.Skip(1))
                            {
                                if (g.FitsBetterThanGroup(c, best))
                                {
                                    best = g;
                                    break;
                                }
                            }
                            best.colors.Add(c);
                        }
                        groups.RemoveAt(0);
                    }



                    Directory.CreateDirectory(dir);

                    if (hasTransparency)
                    {
                        SMonitor.Log($"\tCreating transparent sprite sheet", LogLevel.Info);
                        var trans = new Texture2D(Game1.graphics.GraphicsDevice, rect.Width, rect.Height);
                        trans.SetData(transparent);
                        transparentDict[itemId] = trans;
                        Stream fstream = File.Create(Path.Combine(dir, "transparent.png"));
                        trans.SaveAsPng(fstream, trans.Width, trans.Height);
                        fstream.Close();
                    }

                    SMonitor.Log($"\tCreating {groups.Count} greyscale sprite sheets", LogLevel.Info);

                    for(int idx = 0; idx < groups.Count; idx++)
                    {
                        var group = groups[idx];
                        for (int y = rect.Y; y < rect.Y + rect.Height; y++)
                        {
                            for (int x = rect.X; x < rect.X + rect.Width; x++)
                            {
                                int i = x + y * texture.Width;
                                int j = x - rect.X + (y - rect.Y) * rect.Width;
                                if (group.colors.Exists(d => d.color == pixels[i]))
                                {
                                    byte val = new List<byte>(){ pixels[i].R, pixels[i].G, pixels[i].B }.Max();
                                    colors[j] = new Color(val, val, val);
                                }
                                else
                                {
                                    colors[j] = Color.Transparent;
                                }
                                j++;
                            }
                        }
                        var tex = new Texture2D(Game1.graphics.GraphicsDevice, rect.Width, rect.Height);
                        tex.SetData(colors);

                        Color bright = Color.Black;
                        foreach (var c in group.colors)
                        {
                            if (c.color.R > bright.R && c.color.G > bright.G && c.color.B > bright.B)
                            {
                                bright = c.color;
                            }
                        }
                        textureDict[bright] = tex;
                        var fstream = File.Create(Path.Combine(dir, ColorToHexString(bright) + ".png"));
                        tex.SaveAsPng(fstream, tex.Width, tex.Height);
                        fstream.Close();
                    }
                    tileSheetDict[itemId] = textureDict;
                }
            }
        }



        private void Input_ButtonsChanged(object sender, ButtonsChangedEventArgs e)
        {
            if(!Config.ModEnabled) 
                return;
            if(Context.IsPlayerFree && Config.ColorButton.JustPressed())
            {
                var tile = Game1.currentCursorTile;
                Furniture f = Game1.currentLocation.GetFurnitureAt(tile);
                if (f == null || !tileSheetDict.TryGetValue(f.ItemId, out var dict))
                    return;
                Game1.activeClickableMenu = new ColorPickMenu(f, dict.Keys.Where(k => k != Color.Transparent).ToList());
                foreach (var bind in Config.ColorButton.Keybinds)
                {
                    foreach (var key in bind.Buttons)
                    {
                        SHelper.Input.Suppress(key);
                    }
                }
            }
        }

        private void Content_AssetRequested(object sender, AssetRequestedEventArgs e)
        {
            if(!Config.ModEnabled) 
                return;
            
            if (e.NameWithoutLocale.IsEquivalentTo(dictPath))
            {
                e.LoadFrom(() => new Dictionary<string, string>(), AssetLoadPriority.Exclusive);
            }
        }


        private void GameLoop_GameLaunched(object sender, GameLaunchedEventArgs e)
        {
        
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