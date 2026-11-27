using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.GameData.Objects;
using StardewValley.GameData.Shops;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Object = StardewValley.Object;

namespace DoorFurniture
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry : Mod
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;
        public static ModEntry context;
        public const string dictPath = "aedenthorn.DoorFurniture/dict";

        public const string keyId = "aedenthorn.DoorFurniture_key";
        public const string keyringId = "aedenthorn.DoorFurniture_keyring";
        public const string doorId = "aedenthorn.DoorFurniture_door";
        public const string door2Id = "aedenthorn.DoorFurniture_door2";
        //public const string door3Id = "aedenthorn.DoorFurniture_door3";
        //public const string door4Id = "aedenthorn.DoorFurniture_door4";

        public const string guidKey = "aedenthorn.DoorFurniture/guid";
        public const string nameKey = "aedenthorn.DoorFurniture/name";
        public const string openKey = "aedenthorn.DoorFurniture/open";
        public const string closeKey = "aedenthorn.DoorFurniture/close";
        public const string flipKey = "aedenthorn.DoorFurniture/flip";
        public const string lockKey = "aedenthorn.DoorFurniture/lock";
        public const string colorKey = "aedenthorn.DoorFurniture/color";

        public static Color[] defaultColors = new Color[]
        {
            new(85, 85, 255),
            new(119, 191, 255),
            new(0, 170, 170),
            new(0, 234, 175),
            new(0, 170, 0),
            new(159, 236, 0),
            new(255, 234, 18),
            new(255, 167, 18),
            new(255, 105, 18),
            new(255, 0, 0),
            new(135, 0, 35),
            new(255, 173, 199),
            new(255, 117, 195),
            new(172, 0, 198),
            new(143, 0, 255),
            new(89, 11, 142),
            new(64, 64, 64),
            new(100, 100, 100),
            new(200, 200, 200),
            new(254, 254, 254)
        };
        public override void Entry(IModHelper helper)
        {
            Config = Helper.ReadConfig<ModConfig>();
            SMonitor = Monitor;
            SHelper = helper;

            context = this;

            helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            helper.Events.Content.AssetRequested += Content_AssetRequested;
            helper.Events.Input.ButtonsChanged += Input_ButtonsChanged;
            helper.Events.Input.MouseWheelScrolled += Input_MouseWheelScrolled;
            

            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();
        }

        private void Input_MouseWheelScrolled(object sender, MouseWheelScrolledEventArgs e)
        {
            if (!Config.ModEnabled)
                return;
            if (Config.Debug)
            {
               // Game1.player.friendshipData["Emily"].WeddingDate.DayOfMonth = Game1.Date.DayOfMonth + 1;
                //SHelper.GameContent.InvalidateCache(doorId);
            }
            if (Context.IsPlayerFree)
            {
                var tile = Game1.currentCursorTile + new Vector2(0, 1);
                Furniture f = Game1.currentLocation.GetFurnitureAt(tile);
                if (!TryGetDoorData(f, out var data) || !f.modData.TryGetValue(openKey, out var open) || open != "closed")
                {
                    f = Game1.currentLocation.GetFurnitureAt(Game1.currentCursorTile);
                    if (!TryGetDoorData(f, out data))
                        return;
                }
                if (!data.Colorable)
                    return;
                Color color;
                if (!f.modData.TryGetValue(colorKey, out var str))
                {
                    color = data.DefaultColor;
                }
                else
                {
                    color = Utility.StringToColor(str) ?? data.DefaultColor;
                }
                var colors = (data.Colors ?? defaultColors).ToList();
                if (!colors.Contains(data.DefaultColor))
                {
                    colors.Insert(0, data.DefaultColor);
                }
                var newColor = GetNewColor(colors, color, e.Delta);
                if (data.DefaultUncolored && newColor == data.DefaultColor)
                {
                    f.modData.Remove(colorKey);
                }
                else
                {
                    f.modData[colorKey] = ColorToHexString(newColor);
                }
                f.Location.playSound("shiny4", f.TileLocation);
                SHelper.Input.SuppressScrollWheel();
            }
        }

        private void Input_ButtonsChanged(object sender, ButtonsChangedEventArgs e)
        {
            if(!Config.ModEnabled) 
                return;
            if(Context.IsPlayerFree && Config.FlipButton.JustPressed())
            {
                Furniture f = Game1.player.CurrentItem as Furniture;
                if (IsDoor(f))
                {
                    bool b = false;
                    if(f.modData.TryGetValue(flipKey, out var str))
                    {
                        _ = bool.TryParse(str, out b);
                    }
                    b = !b;
                    f.modData[flipKey] = b.ToString();
                    if (!f.modData.ContainsKey(openKey))
                    {
                        f.modData[openKey] = "closed";
                    }
                    Game1.playSound("grassyStep");
                    foreach(var bind in Config.FlipButton.Keybinds)
                    {
                        foreach(var key in bind.Buttons)
                        {
                            SHelper.Input.Suppress(key);
                        }
                    }
                }
            }
            else if(Context.IsPlayerFree && Config.ColorButton.JustPressed())
            {
                var tile = Game1.currentCursorTile + new Vector2(0, 1);
                Furniture f = Game1.currentLocation.GetFurnitureAt(tile);
                if (!TryGetDoorData(f, out var data) || !f.modData.TryGetValue(openKey, out var open) || open != "closed")
                {
                    f = Game1.currentLocation.GetFurnitureAt(Game1.currentCursorTile);
                    if (!TryGetDoorData(f, out data))
                        return;
                }
                if (!data.Colorable)
                    return;
                Game1.activeClickableMenu = new ColorPickMenu(f);
                foreach (var bind in Config.ColorButton.Keybinds)
                {
                    foreach (var key in bind.Buttons)
                    {
                        SHelper.Input.Suppress(key);
                    }
                }
            }
            else if(Context.IsPlayerFree && Config.LockButton.JustPressed())
            {
                var tile = Game1.player.GetGrabTile();
                Furniture f = Game1.currentLocation.GetFurnitureAt(tile);
                if (f?.modData.TryGetValue(openKey, out var open) == true && open == "closed" && TryGetDoorData(f, out var data) && data.Lockable)
                {
                    bool b = false;
                    if (!f.modData.TryGetValue(lockKey, out var str) || !bool.TryParse(str, out b))
                    {
                        Guid key = Guid.NewGuid();
                        f.modData[lockKey] = "True";
                        Game1.currentLocation.playSound(b ? data.UnlockSound : data.LockSound, f.TileLocation);
                        f.modData[guidKey] = key.ToString()+"=";
                        var obj = new Object(data.KeyItem, 1);
                        obj.modData[guidKey] = key.ToString();
                        TryReturnObject(obj, Game1.player);
                        Game1.activeClickableMenu = new KeyNamingMenu(obj, key.ToString(), obj.DisplayName);
                        return;
                    }
                    string guid = f.modData[guidKey];
                    var items = Game1.player.Items.GetById(data.KeyItem).ToList();
                    items.AddRange(Game1.player.Items.GetById(data.KeyRingItem));
                    bool keyfound = false;
                    foreach(var item in items)
                    {
                        if (item?.modData[guidKey].Contains(guid) == true)
                        {
                            Game1.currentLocation.playSound(b ? data.UnlockSound : data.LockSound, f.TileLocation);
                            Game1.hudMessages.Clear();
                            Game1.addHUDMessage(new HUDMessage(string.Format(SHelper.Translation.Get(b ? "unlocked-with-x" : "locked-with-x"), item.DisplayName))
                            {
                                number = 0,
                                type = string.Format(SHelper.Translation.Get(b ? "unlocked-with-x" : "locked-with-x"), item.DisplayName),
                                messageSubject = item
                            });
                            f.modData[lockKey] = (!b).ToString();
                            keyfound = true;
                            break;
                        }
                    }
                    if (!keyfound)
                    {
                        Game1.hudMessages.Clear();
                        Game1.addHUDMessage(new HUDMessage(SHelper.Translation.Get("no-key"))
                        {
                            messageSubject = f
                        });
                    }
                    foreach (var bind in Config.LockButton.Keybinds)
                    {
                        foreach (var key in bind.Buttons)
                        {
                            SHelper.Input.Suppress(key);
                        }
                    }
                }
            }
        }

        private void Content_AssetRequested(object sender, AssetRequestedEventArgs e)
        {
            if(!Config.ModEnabled) 
                return;
            if(e.NameWithoutLocale.IsEquivalentTo(dictPath))
            {
                var type = SHelper.Translation.Get("door-type");
                var desc = SHelper.Translation.Get("door-description");
                var dict = new Dictionary<string, DoorData>()
                {
                    { 
                        doorId, new DoorData() 
                        { 
                            Type = type,
                            Description = desc,
                            Colorable = true,
                            DefaultColor = Color.Black,
                            DefaultUncolored = true,
                            ColorSpriteOffset = new Point(0, 96)
                        }
                    },
                    { 
                        door2Id, new DoorData()
                        { 
                            Type = type,
                            Description = desc,
                            Colorable = true,
                            DefaultColor = Color.Black,
                            DefaultUncolored = true,
                            ColorSpriteOffset = new Point(0, 96)
                        }
                    }
                };
                e.LoadFrom(() => dict, AssetLoadPriority.Medium);
                if (Config.Debug)
                {
                    File.WriteAllText(Path.Combine(SHelper.DirectoryPath, "test.json"), JsonConvert.SerializeObject(dict, Formatting.Indented));
                }
            }
            else if(e.NameWithoutLocale.IsEquivalentTo(doorId))
            {
                e.LoadFromModFile<Texture2D>("assets/door.png", AssetLoadPriority.Exclusive);
            }
            else if(e.NameWithoutLocale.IsEquivalentTo(keyId))
            {
                e.LoadFromModFile<Texture2D>("assets/key.png", AssetLoadPriority.Exclusive);
            }
            else if(e.NameWithoutLocale.IsEquivalentTo("Data/Furniture"))
            {
                e.Edit((IAssetData asset) =>
                {
                    var dict = asset.AsDictionary<string, string>().Data;
                    dict[doorId] = $"{doorId}/door/1 3/1 1/4/{Config.WoodenDoorPrice}/2/{SHelper.Translation.Get(doorId)}/0/{doorId}/true";
                    dict[door2Id] = $"{door2Id}/door/1 3/1 1/4/{Config.MetalDoorPrice}/2/{SHelper.Translation.Get(door2Id)}/24/{doorId}/true";
                });
            }
            else if(e.NameWithoutLocale.IsEquivalentTo("Data/Objects"))
            {
                e.Edit((IAssetData asset) =>
                {
                    var dict = asset.AsDictionary<string, ObjectData>().Data;
                    dict[keyId] = new()
                    {
                        Name = keyId,
                        DisplayName = SHelper.Translation.Get(keyId),
                        Description = SHelper.Translation.Get(keyId+".Desc"),
                        Type = "DoorKey",
                        Category = -101,
                        Price = 0,
                        Texture = keyId,
                        Edibility = -300,
                        ExcludeFromShippingCollection = true
                    };
                    dict[keyringId] = new()
                    {
                        Name = keyringId,
                        DisplayName = SHelper.Translation.Get(keyringId),
                        Description = SHelper.Translation.Get(keyringId + ".Desc"),
                        Type = "KeyRing",
                        Category = -101,
                        Price = 0,
                        Texture = keyId,
                        SpriteIndex = 1,
                        Edibility = -300,
                        ExcludeFromShippingCollection = true
                    };
                });
            }
            else if (e.NameWithoutLocale.IsEquivalentTo("Data/CraftingRecipes"))
            {
                e.Edit(asset =>
                {
                    var dict = asset.AsDictionary<string, string>();
                    if(!string.IsNullOrEmpty(Config.WoodenDoorIngredients))
                        dict.Data[doorId] = $"{Config.WoodenDoorIngredients}/Home/{doorId}/false/default/";
                    if(!string.IsNullOrEmpty(Config.MetalDoorIngredients))
                        dict.Data[door2Id] = $"{Config.MetalDoorIngredients}/Home/{door2Id}/false/default/";
                });
            }
            else if (e.NameWithoutLocale.IsEquivalentTo("Data/Shops"))
            {
                e.Edit(asset =>
                {
                    var dict = asset.AsDictionary<string, ShopData>();
                    if(Config.WoodenDoorPrice >= 0)
                        dict.Data["Carpenter"].Items.Add(new ShopItemData()
                        {
                            Id = doorId,
                            ItemId = doorId,
                            Price = Config.WoodenDoorPrice
                        });
                    if(Config.MetalDoorPrice >= 0)
                        dict.Data["Carpenter"].Items.Add(new ShopItemData()
                        {
                            Id = door2Id,
                            ItemId = door2Id,
                            Price = Config.MetalDoorPrice
                        });
                });
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