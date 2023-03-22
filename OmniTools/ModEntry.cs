using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Netcode;
using Newtonsoft.Json;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Menus;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using xTile;
using xTile.Layers;
using xTile.ObjectModel;
using Object = StardewValley.Object;

namespace OmniTools
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry : Mod
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;

        public static ModEntry context;
        public static string toolsKey = "aedenthorn.OmniTools/tools";
        public static string toolCountKey = "aedenthorn.OmniTools/toolCount";
        public static bool skip;

        public static List<Type> toolList = new() 
        {
            { typeof(Axe) },
            { typeof(Hoe) },
            { typeof(FishingRod) },
            { typeof(Pickaxe) },
            { typeof(WateringCan) },
            { typeof(MeleeWeapon) },
            { typeof(Slingshot) },
            { typeof(MilkPail) },
            { typeof(Pan) },
            { typeof(Shears) },
            { typeof(Wand) }
        };
        public static List<string> toolSoundList = new()
        {
            "axe",
            "hoeHit",
            "cast",
            "hammer",
            "glug",
            "axe",
            "slingshot",
            "fishingRodBend",
            "slosh",
            "scissors",
            "wand"
        };
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            Config = Helper.ReadConfig<ModConfig>();


            context = this;

            SMonitor = Monitor;
            SHelper = helper;

            Helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            Helper.Events.Input.ButtonPressed += Input_ButtonPressed;
            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();

        }

        public override object GetApi()
        {
            return new OmniToolsAPI();
        }

        private void Input_ButtonPressed(object sender, StardewModdingAPI.Events.ButtonPressedEventArgs e)
        {
            if (!Config.EnableMod || !Context.CanPlayerMove)
                return;
            if(Config.ToggleButton != SButton.None && e.Button == Config.ToggleButton)
            {
                Config.SmartSwitch = !Config.SmartSwitch;
                Helper.WriteConfig(Config);
            }
            if(Game1.player.CurrentTool?.modData.TryGetValue(toolsKey, out string toolsString) != true)
                return;
            if(e.Button == Config.CycleButton)
            {
                var newTool = CycleTool(Game1.player.CurrentTool, toolsString);
                if(newTool != null) 
                {
                    UpdateEnchantments(Game1.player, Game1.player.CurrentTool, newTool);
                    Game1.player.CurrentTool = newTool;
                }
            }
            else if(e.Button == Config.RemoveButton)
            {
                var newTool = RemoveTool(Game1.player.CurrentTool, toolsString);
                if (newTool != null)
                {
                    UpdateEnchantments(Game1.player, Game1.player.CurrentTool, newTool);
                    Game1.player.CurrentTool = newTool;
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
                getValue: () => Config.EnableMod,
                setValue: value => Config.EnableMod = value
            );
            

            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Smart Switch Enabled",
                getValue: () => Config.SmartSwitch,
                setValue: value => Config.SmartSwitch = value
            );
            
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Switch From Weapon",
                getValue: () => Config.FromWeapon,
                setValue: value => Config.FromWeapon = value
            );
            
            configMenu.AddKeybind(
                mod: ModManifest,
                name: () => "Mod Key",
                getValue: () => Config.ModButton,
                setValue: value => Config.ModButton = value
            );

            configMenu.AddKeybind(
                mod: ModManifest,
                name: () => "Cycle Key",
                getValue: () => Config.CycleButton,
                setValue: value => Config.CycleButton = value
            );

            configMenu.AddKeybind(
                mod: ModManifest,
                name: () => "Remove Key",
                getValue: () => Config.RemoveButton,
                setValue: value => Config.RemoveButton = value
            );

            configMenu.AddKeybind(
                mod: ModManifest,
                name: () => "Toggle Key",
                getValue: () => Config.ToggleButton,
                setValue: value => Config.ToggleButton = value
            );

            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Show Number",
                getValue: () => Config.ShowNumber,
                setValue: value => Config.ShowNumber = value
            );
            
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Number Color R",
                getValue: () => Config.NumberColor.R,
                setValue: value => Config.NumberColor = new Color(value, Config.NumberColor.G, Config.NumberColor.B, Config.NumberColor.A),
                min: 0,
                max: 255
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Number Color G",
                getValue: () => Config.NumberColor.G,
                setValue: value => Config.NumberColor = new Color(Config.NumberColor.R, value, Config.NumberColor.B, Config.NumberColor.A),
                min: 0,
                max: 255
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Number Color B",
                getValue: () => Config.NumberColor.B,
                setValue: value => Config.NumberColor = new Color(Config.NumberColor.R, Config.NumberColor.G, value, Config.NumberColor.A),
                min: 0,
                max: 255
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Number Color A",
                getValue: () => Config.NumberColor.A,
                setValue: value => Config.NumberColor = new Color(Config.NumberColor.R, Config.NumberColor.G, Config.NumberColor.B, value),
                min: 0,
                max: 255
            );

            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Switch For Animals",
                getValue: () => Config.SwitchForAnimals,
                setValue: value => Config.SwitchForAnimals = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Switch For Monsters",
                getValue: () => Config.SwitchForMonsters,
                setValue: value => Config.SwitchForMonsters = value
            );
            configMenu.AddTextOption(
                mod: ModManifest,
                name: () => "Max Monster Distance",
                getValue: () => Config.MonsterMaxDistance + "",
                setValue: delegate (string value) { if (float.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out float f)) { Config.MonsterMaxDistance = f; } }
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Switch For Trees",
                getValue: () => Config.SwitchForTrees,
                setValue: value => Config.SwitchForTrees = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Switch For Grass",
                getValue: () => Config.SwitchForGrass,
                setValue: value => Config.SwitchForGrass = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Switch For Crops",
                getValue: () => Config.SwitchForCrops,
                setValue: value => Config.SwitchForCrops = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Harvest With Scythe (Mod)",
                getValue: () => Config.HarvestWithScythe,
                setValue: value => Config.HarvestWithScythe = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Switch For Resource Clumps",
                getValue: () => Config.SwitchForResourceClumps,
                setValue: value => Config.SwitchForResourceClumps = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Switch For Pan",
                getValue: () => Config.SwitchForPan,
                setValue: value => Config.SwitchForPan = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Switch For Watering Can",
                getValue: () => Config.SwitchForWateringCan,
                setValue: value => Config.SwitchForWateringCan = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Switch For Tilling",
                getValue: () => Config.SwitchForTilling,
                setValue: value => Config.SwitchForTilling = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Switch For Watering",
                getValue: () => Config.SwitchForWatering,
                setValue: value => Config.SwitchForWatering = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Switch For Fishing",
                getValue: () => Config.SwitchForFishing,
                setValue: value => Config.SwitchForFishing = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Switch For Objects",
                getValue: () => Config.SwitchForObjects,
                setValue: value => Config.SwitchForObjects = value
            );
        }

    }
}
