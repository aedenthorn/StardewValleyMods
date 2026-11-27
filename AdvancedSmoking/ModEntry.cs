using HarmonyLib;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.GameData.Machines;
using StardewValley.GameData.Objects;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Object = StardewValley.Object;

namespace AdvancedSmoking
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry : Mod
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;
        public static ModEntry context;
        public static string dictPath = "aedenthorn.AdvancedSmoking/products";

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            Config = Helper.ReadConfig<ModConfig>();


            context = this;

            SMonitor = Monitor;
            SHelper = helper;

            Helper.Events.Content.AssetRequested += Content_AssetRequested;
            
            Helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            
            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();
        }


        private void Content_AssetRequested(object sender, StardewModdingAPI.Events.AssetRequestedEventArgs e)
        {
            if (!Config.EnableMod)
                return;
            if (e.NameWithoutLocale.IsEquivalentTo("Data/Objects"))
            {
                e.Edit(delegate (IAssetData data)
                {
                    var dict = data.AsDictionary<string, ObjectData>();

                    foreach (var kvp in dict.Data.Where(kvp => kvp.Value.Category == Object.FishCategory))
                    {
                        dict.Data[$"aedenthorn.AdvancedSmoking_{kvp.Key}"] = new ObjectData()
                        {    
                            Name = $"aedenthorn.AdvancedSmoking_{kvp.Key}",
                            DisplayName = string.Format(SHelper.Translation.Get("smoked_x"), kvp.Value.DisplayName),
                            Description = SHelper.Translation.Get("smoked_x_desc"),
                            Type = "Basic",
                            Category = -26,
                            Price = 25,
                            Texture = kvp.Value.Texture,
                            SpriteIndex = kvp.Value.SpriteIndex
                        };
                    }
                });
            }
            else if (e.NameWithoutLocale.IsEquivalentTo("Data/Machines"))
            {
                e.Edit(delegate (IAssetData data)
                {
                    var dict = data.AsDictionary<string, MachineData>();
                    var rules = dict.Data["(BC)FishSmoker"].OutputRules;
                    var temp = rules.First(r => r.Id == "SmokedFish");
                    rules.Clear();
                    foreach (var kvp in Game1.objectData.Where(kvp => kvp.Value.Category == Object.FishCategory))
                    {
                        var newRule = (MachineOutputRule)AccessTools.Method(typeof(object), "MemberwiseClone").Invoke(temp, null);
                        newRule.Id = kvp.Key;
                        var newOutput = (MachineItemOutput)AccessTools.Method(typeof(object), "MemberwiseClone").Invoke(temp.OutputItem[0], null);
                        newOutput.Id = $"FLAVORED_ITEM aedenthorn.AdvancedSmoking_{kvp.Key} DROP_IN_ID DROP_IN_QUALITY";
                        newOutput.ItemId = $"FLAVORED_ITEM aedenthorn.AdvancedSmoking_{kvp.Key} DROP_IN_ID DROP_IN_QUALITY";
                        newRule.OutputItem = new List<MachineItemOutput>()
                        {
                            newOutput
                        };
                    }
                });
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
                getValue: () => Config.EnableMod,
                setValue: value => Config.EnableMod = value
            );
            
            configMenu.AddKeybindList(
                mod: ModManifest,
                name: () => "Toggle Key",
                getValue: () => Config.ToggleButton,
                setValue: value => Config.ToggleButton = value
            );
            configMenu.AddTextOption(
                mod: ModManifest,
                name: () => "Copper Time Mult",
                getValue: () => Config.TimeMultCopper + "",
                setValue: delegate (string value) { if (float.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out float f)) { Config.TimeMultCopper = f; } }
            );
            configMenu.AddTextOption(
                mod: ModManifest,
                name: () => "Iron Time Mult",
                getValue: () => Config.TimeMultIron + "",
                setValue: delegate (string value) { if (float.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out float f)) { Config.TimeMultIron = f; } }
            );
            configMenu.AddTextOption(
                mod: ModManifest,
                name: () => "Gold Time Mult",
                getValue: () => Config.TimeMultGold + "",
                setValue: delegate (string value) { if (float.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out float f)) { Config.TimeMultGold = f; } }
            );
            configMenu.AddTextOption(
                mod: ModManifest,
                name: () => "Iridium Time Mult",
                getValue: () => Config.TimeMultIridium + "",
                setValue: delegate (string value) { if (float.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out float f)) { Config.TimeMultIridium = f; } }
            );
        }
    }
}
