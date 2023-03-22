using HarmonyLib;
using StardewModdingAPI;
using System.Globalization;

namespace ToolSmartSwitch
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry : Mod
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;

        public static ModEntry context;

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

        private void Input_ButtonPressed(object sender, StardewModdingAPI.Events.ButtonPressedEventArgs e)
        {
            if(Context.CanPlayerMove && Config.ToggleButton != SButton.None && e.Button == Config.ToggleButton)
            {
                Config.EnableMod = !Config.EnableMod;
                Helper.WriteConfig(Config);
                SMonitor.Log("Mod enabled: " + Config.EnableMod);
            }
        }

        public override object GetApi()
        {
            return new ToolSmartSwitchAPI();
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
            
            configMenu.AddKeybind(
                mod: ModManifest,
                name: () => "Toggle Button",
                getValue: () => Config.ToggleButton,
                setValue: value => Config.ToggleButton = value
            );

            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Only When Holding Tool",
                getValue: () => Config.HoldingTool,
                setValue: value => Config.HoldingTool = value
            );
            

            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Switch From Weapon",
                getValue: () => Config.FromWeapon,
                setValue: value => Config.FromWeapon = value
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
