using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.GameData.Buildings;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using xTile;

namespace GiantCropInteriors
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry : Mod
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;
        public static ModEntry context;
        public const string dictPath = "aedenthorn.GiantCropInteriors/dict";
        public const string giantCropKey = "aedenthorn.GiantCropInteriors/GiantCrop";
        public const string pumpkinKey = "aedenthorn.GiantCropInteriors/Pumpkin";
        public const string melonKey = "aedenthorn.GiantCropInteriors/Melon";
        public const string builtAtKey = "aedenthorn.GiantCropInteriors/BuiltAt";
        public const string cropKey = "aedenthorn.GiantCropInteriors/Crop";
        public const string modPrefix = "aedenthorn.GiantCropInteriors/";

        private static List<Building> ToRemove = new();


        public static Dictionary<string, GiantCropBuildingData> BuildingDict
        {
            get
            {
                return SHelper.GameContent.Load<Dictionary<string, GiantCropBuildingData>>(dictPath);
            }
        }

        public override void Entry(IModHelper helper)
        {
            Config = Helper.ReadConfig<ModConfig>();
            SMonitor = Monitor;
            SHelper = helper;

            context = this;

            helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            helper.Events.GameLoop.Saving += GameLoop_Saving;
            helper.Events.GameLoop.DayStarted += GameLoop_DayStarted;
            helper.Events.Content.AssetRequested += Content_AssetRequested;
            helper.Events.Input.ButtonPressed += Input_ButtonPressed;


            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();

        }

        private void GameLoop_Saving(object sender, SavingEventArgs e)
        {
            if (!Config.ModEnabled)
            {
                RemoveAllBuildings();
            }
        }

        private void GameLoop_DayStarted(object sender, DayStartedEventArgs e)
        {
            if (!Config.ModEnabled)
            {
                RemoveAllBuildings();
            }
        }

        private static void GameLoop_UpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            foreach (var b in ToRemove)
            {
                if (b.GetParentLocation() is GameLocation l)
                {
                    l.buildings.Remove(b);
                }
            }
            ToRemove.Clear();
            SHelper.Events.GameLoop.UpdateTicked -= GameLoop_UpdateTicked;
        }

        private void Input_ButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (!Config.ModEnabled)
                return;
            if (false && Config.Debug)
            {
                if (Context.IsWorldReady)
                {
                    Game1.currentLocation.MakeMapModifications();
                    //SHelper.GameContent.InvalidateCache("Maps/GiantCrop");
                    if (e.Button == SButton.NumPad7)
                    {


                        foreach (var t in Game1.getFarm().terrainFeatures.Values.Where(tf => tf is HoeDirt d && d.crop is not null))
                        {
                            (t as HoeDirt).crop.growCompletely();
                        }
                        foreach (var t in Game1.getFarm().terrainFeatures.Values.Where(tf => tf is HoeDirt d && d.crop is not null))
                        {
                            (t as HoeDirt).crop.TryGrowGiantCrop(false, Game1.random);
                        }
                    }
                }
            }
        }

        private void Content_AssetRequested(object sender, AssetRequestedEventArgs e)
        {
            if (!Config.ModEnabled)
                return;
            if (e.NameWithoutLocale.IsEquivalentTo(dictPath))
            {
                e.LoadFrom(() => new Dictionary<string, GiantCropBuildingData>()
                {
                    {
                        "Pumpkin",
                        new()
                        {
                            Texture = pumpkinKey
                        }
                    },
                    {
                        "Melon",
                        new()
                        {
                            Texture = melonKey
                        }
                    }
                }, AssetLoadPriority.Exclusive);
            }
            else if (e.NameWithoutLocale.IsEquivalentTo("Data/Buildings"))
            {
                e.Edit((IAssetData data) => 
                {
                    var dict = data.AsDictionary<string, BuildingData>().Data;
                    foreach(var kvp in BuildingDict)
                    {

                        dict[modPrefix + kvp.Key] = new()
                        {
                            Name = kvp.Value.Name ?? kvp.Key,
                            Description = kvp.Value.Description ?? kvp.Key,
                            Texture = kvp.Value.Texture,
                            SourceRect = new(0, 0, 1, 1),
                            Builder = null,
                            IndoorMap = kvp.Value.IndoorMap,
                            IndoorMapType = kvp.Value.IndoorMapType,
                            HumanDoor = new(1, 3),
                            Size = new(3, 3),
                            DrawShadow = false,
                            DrawLayers = new()
                            {
                                new()
                                {
                                    Id = "1",
                                    SortTileOffset = -0.0001f,
                                    DrawPosition = new(0,-64),
                                    Texture = kvp.Value.Texture,
                                    SourceRect = new(0,0,48,64)
                                }
                            }
                        };
                    }
                });
            }
            else if (e.NameWithoutLocale.IsEquivalentTo("Maps/GiantCrop"))
            {
                e.LoadFromModFile<Map>("assets/GiantCrop.tmx", AssetLoadPriority.Exclusive);
            }
            else if (e.NameWithoutLocale.IsEquivalentTo(pumpkinKey))
            {
                e.LoadFromModFile<Texture2D>("assets/Pumpkin.png", AssetLoadPriority.Exclusive);
            }
            else if (e.NameWithoutLocale.IsEquivalentTo(melonKey))
            {
                e.LoadFromModFile<Texture2D>("assets/Melon.png", AssetLoadPriority.Exclusive);
            }
        }

        private void GameLoop_GameLaunched(object sender, GameLaunchedEventArgs e)
        {
            //SMonitor.Log(string.Join(",", Game1.objectData.Where(kvp => kvp.Value.Type == "Arch").Select(kvp => kvp.Key)));
            var configMenu = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (configMenu is not null)
            {
                configMenu.Register(
                    mod: ModManifest,
                    reset: () => Config = new ModConfig(),
                    save: () => Helper.WriteConfig(Config)
                );

                var exclude = new List<string>()
                {
                    "Debug",
                    "ModEnabled"
                };
                var props = typeof(ModConfig).GetProperties().ToArray();
                var configMenuExt = Helper.ModRegistry.GetApi<IGMCMOptionsAPI>("jltaylor-us.GMCMOptions");

                configMenu.AddBoolOption(
                    mod: ModManifest,
                    name: () => { var t = Helper.Translation.Get("ModEnabled"); return t.HasValue() ? t : AddSpaces("ModEnabled"); },
                    tooltip: () => { var t = Helper.Translation.Get("ModEnabled" + ".Desc"); return t.HasValue() ? t : null; },
                    getValue: () => Config.ModEnabled,
                    setValue: value => { Config.ModEnabled = value; if (!value) { RemoveAllBuildings(); } }
                );

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
}