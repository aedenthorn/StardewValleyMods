using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Object = StardewValley.Object;

namespace CustomStarterPackage
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry : Mod
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;

        public static ModEntry context;

        public static string dictPath = "aedenthorn.CustomStarterPackage/dictionary";
        public static string chestKey = "aedenthorn.CustomStarterPackage/chest";
        public static Dictionary<string, StarterItemData> dataDict = new();
        
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            Config = Helper.ReadConfig<ModConfig>();

            if (!Config.ModEnabled)
                return;

            context = this;

            SMonitor = Monitor;
            SHelper = helper;

            helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            helper.Events.GameLoop.DayStarted += GameLoop_DayStarted;
            helper.Events.Content.AssetRequested += Content_AssetRequested;

            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();

        }

        private void GameLoop_DayStarted(object sender, StardewModdingAPI.Events.DayStartedEventArgs e)
        {
            dataDict = Game1.content.Load <Dictionary<string, StarterItemData>>(dictPath);
            SMonitor.Log($"Loaded {dataDict.Count} items from content patcher");
            foreach (var o in Game1.player.currentLocation.objects.Pairs)
            {
                if (o.Value is Chest && (o.Value as Chest).giftbox.Value && (o.Value as Chest).items.Count == 1 && (o.Value as Chest).items[0].ParentSheetIndex == 472 && (o.Value as Chest).items[0].Stack == 15)
                {
                    SMonitor.Log($"Found starter chest at {o.Key}; replacing");
                    var items = (o.Value as Chest).items;
                    items.Clear();
                    foreach (var d in dataDict)
                    {
                        if (d.Value.ChancePercent < Game1.random.Next(100))
                            continue;
                        SMonitor.Log($"Adding {d.Key}");

                        Item obj = null;
                        int index = -1;
                        switch (d.Value.Type)
                        {
                            case "Object":
                                int amount = (d.Value.MinAmount < d.Value.MaxAmount) ? Game1.random.Next(d.Value.MinAmount, d.Value.MaxAmount + 1) : d.Value.MinAmount;
                                int quality = (d.Value.MinQuality < d.Value.MaxQuality) ? Game1.random.Next(d.Value.MinQuality, d.Value.MaxQuality + 1) : d.Value.MinQuality;
                                if (!int.TryParse(d.Value.NameOrIndex, out index))
                                {
                                    var dict = SHelper.GameContent.Load<Dictionary<int, string>>("Data/ObjectInformation");
                                    try
                                    {
                                        index = dict.First(p => p.Value.StartsWith(d.Value.NameOrIndex + "/")).Key;
                                    }
                                    catch
                                    {
                                        SMonitor.Log($"Object {d.Value.NameOrIndex} not found", StardewModdingAPI.LogLevel.Warn);
                                        continue;
                                    }
                                }
                                obj = new Object(index, amount, quality: quality);
                                break;
                            case "Chest":
                                if (!int.TryParse(d.Value.NameOrIndex, out index))
                                {
                                    var dict = SHelper.GameContent.Load<Dictionary<int, string>>("Data/ObjectInformation");
                                    try
                                    {
                                        index = dict.First(p => p.Value.StartsWith(d.Value.NameOrIndex + "/")).Key;
                                    }
                                    catch
                                    {
                                        SMonitor.Log($"Object {d.Value.NameOrIndex} not found", StardewModdingAPI.LogLevel.Warn);
                                        continue;
                                    }
                                }
                                obj = new Chest(true, index);
                                break;
                            case "BigCraftable":
                                if (!int.TryParse(d.Value.NameOrIndex, out index))
                                {
                                    var dict = SHelper.GameContent.Load<Dictionary<int, string>>("Data/BigCraftablesInformation");
                                    try
                                    {
                                        index = dict.First(p => p.Value.StartsWith(d.Value.NameOrIndex + "/")).Key;
                                    }
                                    catch
                                    {
                                        SMonitor.Log($"BigCraftable {d.Value.NameOrIndex} not found", StardewModdingAPI.LogLevel.Warn);
                                        continue;
                                    }
                                    obj = new Object(Vector2.Zero, index, false);
                                }
                                break;
                            case "Hat":
                                if (!int.TryParse(d.Value.NameOrIndex, out index))
                                {
                                    var dict = SHelper.GameContent.Load<Dictionary<int, string>>("Data/hats");
                                    try
                                    {
                                        index = dict.First(p => p.Value.StartsWith(d.Value.NameOrIndex + "/")).Key;
                                    }
                                    catch
                                    {
                                        SMonitor.Log($"Hat {d.Value.NameOrIndex} not found", StardewModdingAPI.LogLevel.Warn);
                                        continue;
                                    }
                                }
                                obj = new Hat(index);
                                break;
                            case "Boots":
                                if (!int.TryParse(d.Value.NameOrIndex, out index))
                                {
                                    var dict = SHelper.GameContent.Load<Dictionary<int, string>>("Data/Boots");
                                    try
                                    {
                                        index = dict.First(p => p.Value.StartsWith(d.Value.NameOrIndex + "/")).Key;
                                    }
                                    catch
                                    {
                                        SMonitor.Log($"Boots {d.Value.NameOrIndex} not found", StardewModdingAPI.LogLevel.Warn);
                                        continue;
                                    }
                                }
                                obj = new Boots(index);
                                break;
                            case "Ring":
                                if (!int.TryParse(d.Value.NameOrIndex, out index))
                                {
                                    var dict = SHelper.GameContent.Load<Dictionary<int, string>>("Data/ObjectInformation");
                                    try
                                    {
                                        index = dict.First(p => p.Value.StartsWith(d.Value.NameOrIndex + "/") && p.Value.Contains("/Ring/")).Key;
                                    }
                                    catch
                                    {
                                        SMonitor.Log($"Ring {d.Value.NameOrIndex} not found", StardewModdingAPI.LogLevel.Warn);
                                        continue;
                                    }
                                }
                                obj = new Ring(index);
                                break;
                            case "Clothing":
                                if (!int.TryParse(d.Value.NameOrIndex, out index))
                                {
                                    var dict = SHelper.GameContent.Load<Dictionary<int, string>>("Data/ClothingInformation");
                                    try
                                    {
                                        index = dict.First(p => p.Value.StartsWith(d.Value.NameOrIndex + "/")).Key;
                                    }
                                    catch
                                    {
                                        SMonitor.Log($"Clothing {d.Value.NameOrIndex} not found", StardewModdingAPI.LogLevel.Warn);
                                        continue;
                                    }
                                }
                                obj = new Clothing(index);
                                break;
                            case "Furniture":
                                if (!int.TryParse(d.Value.NameOrIndex, out index))
                                {
                                    var dict = SHelper.GameContent.Load<Dictionary<int, string>>("Data/ClothingInformation");
                                    try
                                    {
                                        index = dict.First(p => p.Value.StartsWith(d.Value.NameOrIndex + "/")).Key;
                                    }
                                    catch
                                    {
                                        SMonitor.Log($"Furniture {d.Value.NameOrIndex} not found", StardewModdingAPI.LogLevel.Warn);
                                        continue;
                                    }
                                }
                                obj = Furniture.GetFurnitureInstance(index, Vector2.Zero);
                                break;
                            case "Pickaxe":
                                if (int.TryParse(d.Value.NameOrIndex, out index))
                                {
                                    obj = new Pickaxe() { UpgradeLevel = index };
                                }
                                else
                                {
                                    SMonitor.Log($"Invalid tool level {d.Value.NameOrIndex}", StardewModdingAPI.LogLevel.Warn);
                                    continue;
                                }
                                break;
                            case "Axe":
                                if (int.TryParse(d.Value.NameOrIndex, out index))
                                {
                                    obj = new Axe() { UpgradeLevel = index };
                                }
                                else
                                {
                                    SMonitor.Log($"Invalid tool level {d.Value.NameOrIndex}", StardewModdingAPI.LogLevel.Warn);
                                    continue;
                                }
                                break;
                            case "Pan":
                                if (int.TryParse(d.Value.NameOrIndex, out index))
                                {
                                    obj = new Pan() { UpgradeLevel = index };
                                }
                                else
                                {
                                    SMonitor.Log($"Invalid tool level {d.Value.NameOrIndex}", StardewModdingAPI.LogLevel.Warn);
                                    continue;
                                }
                                break;
                            case "Shears":
                                if (int.TryParse(d.Value.NameOrIndex, out index))
                                {
                                    obj = new Shears() { UpgradeLevel = index };
                                }
                                else
                                {
                                    SMonitor.Log($"Invalid tool level {d.Value.NameOrIndex}", StardewModdingAPI.LogLevel.Warn);
                                    continue;
                                }
                                break;
                            case "FishingRod":
                                if (int.TryParse(d.Value.NameOrIndex, out index))
                                {
                                    obj = new FishingRod() { UpgradeLevel = index };
                                }
                                else
                                {
                                    SMonitor.Log($"Invalid tool level {d.Value.NameOrIndex}", StardewModdingAPI.LogLevel.Warn);
                                    continue;
                                }
                                break;
                            case "Hoe":
                                if (int.TryParse(d.Value.NameOrIndex, out index))
                                {
                                    obj = new Hoe() { UpgradeLevel = index };
                                }
                                else
                                {
                                    SMonitor.Log($"Invalid tool level {d.Value.NameOrIndex}", StardewModdingAPI.LogLevel.Warn);
                                    continue;
                                }
                                break;
                            case "WateringCan":
                                if (int.TryParse(d.Value.NameOrIndex, out index))
                                {
                                    obj = new WateringCan() { UpgradeLevel = index };
                                }
                                else
                                {
                                    SMonitor.Log($"Invalid tool level {d.Value.NameOrIndex}", StardewModdingAPI.LogLevel.Warn);
                                    continue;
                                }
                                break;
                            case "MeleeWeapon":
                                if (int.TryParse(d.Value.NameOrIndex, out index))
                                {
                                    obj = new MeleeWeapon(index);
                                }
                                else
                                {
                                    var dict = SHelper.GameContent.Load<Dictionary<int, string>>("Data/weapons");
                                    try
                                    {
                                        index = dict.First(p => p.Value.StartsWith(d.Value.NameOrIndex + "/")).Key;
                                        obj = new MeleeWeapon(index);
                                    }
                                    catch
                                    {
                                        SMonitor.Log($"MeleeWeapon {d.Value.NameOrIndex} not found", StardewModdingAPI.LogLevel.Warn);
                                        continue;
                                    }
                                }
                                break;
                            default:
                                SMonitor.Log($"Object type {d.Value.Type} not recognized", StardewModdingAPI.LogLevel.Warn);
                                continue;
                        }
                        if (obj != null)
                        {
                            items.Add(obj);
                        }
                        else
                        {
                            SMonitor.Log($"Object {d.Key} not recognized", StardewModdingAPI.LogLevel.Warn);
                        }
                    }
                    if(items.Count > 1)
                    {
                        (o.Value as Chest).dropContents.Value = true;
                    }
                    o.Value.modData[chestKey] = "true";
                    SMonitor.Log($"Added {items.Count} items to starter chest", LogLevel.Warn);
                    return;
                }
            }
        
        }

        private void Content_AssetRequested(object sender, StardewModdingAPI.Events.AssetRequestedEventArgs e)
        {
            if (e.NameWithoutLocale.IsEquivalentTo(dictPath))
            {
                e.LoadFrom(() => new Dictionary<string, StarterItemData>(), StardewModdingAPI.Events.AssetLoadPriority.Exclusive);
            }
        }

        private void GameLoop_SaveLoaded(object sender, StardewModdingAPI.Events.SaveLoadedEventArgs e)
        {
            dataDict = Game1.content.Load<Dictionary<string, StarterItemData>>(dictPath);
        }

        private void GameLoop_GameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
        {
            //MakeHatData();

            // get Generic Mod Config Menu's API (if it's installed)
            var configMenu = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (configMenu is null)
                return;

            // register mod
            configMenu.Register(
                mod: ModManifest,
                reset: () => Config = new ModConfig(),
                save: () => Helper.WriteConfig(Config)
            );

            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Mod Enabled",
                getValue: () => Config.ModEnabled,
                setValue: value => Config.ModEnabled = value
            );
        }
    }
}