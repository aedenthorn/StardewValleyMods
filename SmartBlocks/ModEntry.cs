using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.GameData.Objects;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Object = StardewValley.Object;

namespace SmartBlocks
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry : Mod
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;
        public static ModEntry context;
        public const string blockKey = "aedenthorn.SmartBlocks/block";
        public const string selectedKey = "aedenthorn.SmartBlocks/selected";
        public const string blockPrefix = "aedenthorn.SmartBlocks_";
        public const string modPrefix = "aedenthorn.SmartBlocks/";
        public const string dictPath = "aedenthorn.SmartBlocks/dict";
        public static Dictionary<string, Dictionary<Vector2, List<SmartBlockInstanceData>>> BlockCache { get; set; } = new();
        public static Dictionary<string, SmartBlockData> BlockTypes
        {
            get
            {
                return SHelper.GameContent.Load<Dictionary<string, SmartBlockData>>(dictPath);
            }
        }
        public override void Entry(IModHelper helper)
        {
            Config = Helper.ReadConfig<ModConfig>();
            SMonitor = Monitor;
            SHelper = helper;

            context = this;

            helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            helper.Events.Content.AssetRequested += Content_AssetRequested;
            helper.Events.Input.ButtonsChanged += Input_ButtonsChanged;

            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();
        }

        private void Multiplayer_ModMessageReceived(object sender, ModMessageReceivedEventArgs e)
        {
            if (!Config.ModEnabled)
                return;
            if (e.FromModID == ModManifest.UniqueID && e.Type == "UpdateBlockInstance")
            {
                MyMessage m = e.ReadAs<MyMessage>();
                var l = Game1.getLocationFromName(m.Location);
                if (l is null || !l.Objects.TryGetValue(m.Tile, out var obj) || !obj.ItemId.StartsWith(blockPrefix))
                    return;
                
                if (!BlockCache.TryGetValue(m.Location, out var dict))
                {
                    dict = new();
                    BlockCache[m.Location] = dict;
                }
                if(!dict.TryGetValue(m.Tile, out var list))
                {
                    list = new();
                    dict[m.Tile] = list;
                }
                list.Clear();
                list.Add(GetBlocksAt(l, m.Tile.ToPoint()));
            }
        }
        private void Content_AssetRequested(object sender, AssetRequestedEventArgs e)
        {
            if (!Config.ModEnabled)
                return;
            if (e.NameWithoutLocale.IsEquivalentTo("Data/Objects"))
            {
                e.Edit((IAssetData data) =>
                {
                    var dict = data.AsDictionary<string, ObjectData>().Data;
                    foreach(var kvp in Helper.GameContent.Load<Dictionary<string, SmartBlockData>>(dictPath))
                    {
                        var id = $"{blockPrefix}{kvp.Key}";
                        dict[id] = new ObjectData()
                        {
                            Name = id,
                            DisplayName = SHelper.Translation.Get(id),
                            Description = SHelper.Translation.Get(id + ".desc"),
                            Type = "Crafting",
                            Category = 0,
                            Price = 1000,
                            Texture = blockKey,
                            SpriteIndex = kvp.Value.SpriteIndex
                        };
                    }
                });
            }
            else if (e.NameWithoutLocale.IsEquivalentTo("Data/CraftingRecipes"))
            {
                e.Edit((IAssetData data) =>
                {
                    var dict = data.AsDictionary<string, string>().Data;
                    foreach (var kvp in Helper.GameContent.Load<Dictionary<string, SmartBlockData>>(dictPath))
                    {
                        var id = $"{blockPrefix}{kvp.Key}";
                        dict[id] = $"{kvp.Value.CraftingCost}/Home/{id}/false/null/";
                    }
                });
            }
            else if (e.NameWithoutLocale.IsEquivalentTo(blockKey))
            {
                e.LoadFromModFile<Texture2D>("assets/blocks.png", AssetLoadPriority.Exclusive);
            }
            else if (e.NameWithoutLocale.IsEquivalentTo(dictPath))
            {
                e.LoadFrom(() => new Dictionary<string, SmartBlockData>()
                {
                    {
                        "ToolBlock",
                        new()
                        {
                            ItemSlot = true,
                            CloneItem = true,
                            ItemTypes = new Type[] { typeof(Tool) },
                            MinRadius = 1,
                            MaxRadius = 5,
                            SpriteIndex = 1
                        }
                    },
                    {
                        "DispenseBlock",
                        new()
                        {
                            ItemSlot = true,
                            ItemTypes = new Type[] { typeof(Object) },
                            MinRadius = 1,
                            MaxRadius = 5,
                            SpriteIndex = 3
                        }
                    },
                    {
                        "GatherBlock",
                        new()
                        {
                            MinRadius = 1,
                            MaxRadius = 5,
                            OutNodes = new Rectangle[] { 
                                new(3, 3, 2, 2), 
                                new(6, 3, 2, 2), 
                                new(9, 3, 2, 2), 
                                new(3, 6, 2, 2), 
                                new(9, 6, 2, 2), 
                                new(3, 9, 2, 2), 
                                new(6, 9, 2, 2), 
                                new(9, 9, 2, 2) 
                            },
                            SpriteIndex = 2
                        }
                    },
                    {
                        "SortBlock",
                        new()
                        {
                            MinRadius = 1,
                            MaxRadius = 5,
                            InNodes = new Rectangle[] { 
                                new(3, 3, 2, 2), 
                                new(6, 3, 2, 2), 
                                new(9, 3, 2, 2), 
                                new(3, 6, 2, 2), 
                                new(9, 6, 2, 2), 
                                new(3, 9, 2, 2), 
                                new(6, 9, 2, 2), 
                                new(9, 9, 2, 2) 
                            },
                            SpriteIndex = 4
                        }
                    },
                    {
                        "FilterBlock",
                        new()
                        {
                            ItemSlot = true,
                            CloneItem = true,
                            ItemTypes = new Type[] { typeof(Object) },
                            InNodes = new Rectangle[] {
                                new(3, 3, 2, 2),
                                new(9, 3, 2, 2),
                                new(3, 6, 2, 2),
                                new(9, 6, 2, 2),
                                new(3, 9, 2, 2),
                                new(6, 9, 2, 2),
                                new(9, 9, 2, 2)
                            },
                            OutNodes = new Rectangle[] { 
                                new(6, 3, 2, 2) 
                            },
                            SpriteIndex = 0
                        }
                    }
                }, AssetLoadPriority.Exclusive);
            }
        }

        private void Input_ButtonsChanged(object sender, ButtonsChangedEventArgs e)
        {
            if (!Config.ModEnabled || !Context.IsPlayerFree)
                return;
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