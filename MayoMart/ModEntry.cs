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

namespace MayoMart
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry : Mod
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;

        public static ModEntry context;

        //public static string dictPath = "aedenthorn.MayoMart/dictionary";
        //public static Dictionary<string, MayoMartData> dataDict = new();
        
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
            helper.Events.Content.AssetRequested += Content_AssetRequested;

            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();

        }
        
        private void Content_AssetRequested(object sender, StardewModdingAPI.Events.AssetRequestedEventArgs e)
        {
            if (!Config.ModEnabled)
                return;
            if (e.Name.IsEquivalentTo("Maps/spring_town") || e.Name.IsEquivalentTo("Maps/summer_town") || e.Name.IsEquivalentTo("Maps/fall_town") || e.Name.IsEquivalentTo("Maps/winter_town"))
            {
                e.Edit(delegate(IAssetData data)
                {
                    data.AsImage().PatchImage(Helper.ModContent.Load<Texture2D>("assets/town.png"), null, new Rectangle(68, 905, 387, 36), PatchMode.Overlay);
                });
            }
            else if (e.Name.IsEquivalentTo("Maps/townInterior"))
            {
                e.Edit(delegate(IAssetData data)
                {
                    data.AsImage().PatchImage(Helper.ModContent.Load<Texture2D>("assets/townInterior.png"), null, new Rectangle(96, 936, 207, 109), PatchMode.Replace);
                });
            }
            else if (e.Name.IsEquivalentTo("TileSheets/furniture"))
            {
                e.Edit(delegate(IAssetData data)
                {
                    data.AsImage().PatchImage(Helper.ModContent.Load<Texture2D>("assets/furniture.png"), null, new Rectangle(146, 807, 21, 15), PatchMode.Replace);
                });
            }
            else if (e.Name.IsEquivalentTo("TileSheets/Craftables"))
            {
                e.Edit(delegate(IAssetData data)
                {
                    data.AsImage().PatchImage(Helper.ModContent.Load<Texture2D>("assets/Craftables.png"), null, new Rectangle(81, 457, 7, 21), PatchMode.Replace);
                });
            }
            else if (e.Name.IsEquivalentTo("Strings/StringsFromCSFiles"))
            {
                e.Edit(delegate(IAssetData data)
                {
                    var dict = data.AsDictionary<string, string>().Data;
                    foreach(var key in dict.Keys.ToArray())
                    {
                        dict[key] = dict[key].Replace("Joja", "Mayo");
                    }
                });
            }
            else if (e.Name.IsEquivalentTo("Data/ObjectInformation") || e.Name.IsEquivalentTo("Data/BigCraftablesInformation"))
            {
                e.Edit(delegate(IAssetData data)
                {
                    var dict = data.AsDictionary<int, string>().Data;
                    foreach(var key in dict.Keys.ToArray())
                    {
                        dict[key] = dict[key].Replace("Joja", "Mayo");
                    }
                });
            }
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

            configMenu.AddTextOption(
                mod: ModManifest,
                name: () => "Ordinary Chance",
                getValue: () => (Config.OrdinaryHayChance * 100) + "",
                setValue: delegate(string value) { if (int.TryParse(value, out int i)) { Config.OrdinaryHayChance = i / 100f; } }
            );
            configMenu.AddTextOption(
                mod: ModManifest,
                name: () => "Gold Chance",
                getValue: () => (Config.GoldHayChance * 100) + "",
                setValue: delegate(string value) { if (int.TryParse(value, out int i)) { Config.GoldHayChance = i / 100f; } }
            );
        }
    }
}