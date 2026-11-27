using HarmonyLib;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;

namespace UniqueValley
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry : Mod
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;

        public static ModEntry context;

        public static string nameKey = "aedenthorn.UniqueValley/name";

        public static Dictionary<string, SubData> subDict = new Dictionary<string, SubData>();

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

            helper.Events.GameLoop.SaveLoaded += GameLoop_SaveLoaded;
            helper.Events.GameLoop.ReturnedToTitle += GameLoop_ReturnedToTitle;

            helper.Events.Content.AssetRequested += Content_AssetRequested;


            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();

        }

        private void GameLoop_ReturnedToTitle(object sender, StardewModdingAPI.Events.ReturnedToTitleEventArgs e)
        {
            subDict.Clear();
        }

        private void GameLoop_SaveLoaded(object sender, StardewModdingAPI.Events.SaveLoadedEventArgs e)
        {
            subDict.Clear();
            for (int i = 0; i < Game1.locations.Count; i++)
            {
                if (!(Game1.locations[i] is MovieTheater))
                {
                    foreach (NPC c in Game1.locations[i].getCharacters())
                    {
                        if (!c.isVillager())
                            continue;
                        Helper.GameContent.InvalidateCache($"Characters/Dialogue/{c.Name}");
                        if (c.modData.TryGetValue(nameKey, out string sub))
                        {
                            subDict.Add(c.Name, new SubData() { name = sub });
                        }
                    }
                }
            }
        }

        private void Content_AssetRequested(object sender, StardewModdingAPI.Events.AssetRequestedEventArgs e)
        {
            if (e.Name.StartsWith("Characters/Dialogue/") || e.Name.StartsWith("Strings/"))
            {
                e.Edit(ChangeNames, StardewModdingAPI.Events.AssetEditPriority.Late);
            }
            else if (Config.RandomizeGiftTastes && e.NameWithoutLocale.IsEquivalentTo("Data/NPCGiftTastes"))
            {
                e.Edit(ChangeNames, StardewModdingAPI.Events.AssetEditPriority.Late);
            }
        }
        private void ChangeNames(IAssetData obj)
        {
            var dict = obj.AsDictionary<string, string>().Data;
            foreach (var key in dict.Keys.ToList())
            {
                for (int i = 0; i < dict[key].Length; i++)
                {
                    foreach (var kvp in subDict)
                    {
                        if (dict[key].Substring(i).StartsWith(kvp.Key))
                        {
                            dict[key] = dict[key].Substring(0, i) + kvp.Value.name + dict[key].Substring(i + kvp.Key.Length);
                            i += kvp.Value.name.Length;
                            break;
                        }

                    }
                }
            }
        }

        private void GameLoop_GameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
        {

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
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Maintain Age",
                getValue: () => Config.MaintainAge,
                setValue: value => Config.MaintainAge = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Maintain Gender",
                getValue: () => Config.MaintainGender,
                setValue: value => Config.MaintainGender = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Maintain Datable",
                getValue: () => Config.MaintainDatable,
                setValue: value => Config.MaintainDatable = value
            );
            configMenu.AddTextOption(
                mod: ModManifest,
                name: () => "Forbidden List",
                tooltip: () => "Comma-separated",
                getValue: () => Config.ForbiddenList,
                setValue: value => Config.ForbiddenList = value
            );
        }

    }
}