using HarmonyLib;
using Newtonsoft.Json;
using StardewModdingAPI;
using StardewValley;
using System.Collections.Generic;

namespace Alarms
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry : Mod
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;

        public static ModEntry context;
        public static string modKey = "aedenthorn.Alarms/alarms";

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            Config = Helper.ReadConfig<ModConfig>();

            context = this;

            SMonitor = Monitor;
            SHelper = helper;

            Helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            Helper.Events.GameLoop.SaveLoaded += GameLoop_SaveLoaded;
            Helper.Events.GameLoop.Saving += GameLoop_Saving;
            Helper.Events.Input.ButtonsChanged += Input_ButtonsChanged;
            Helper.Events.GameLoop.TimeChanged += GameLoop_TimeChanged;

            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();
        }

        private void GameLoop_SaveLoaded(object sender, StardewModdingAPI.Events.SaveLoadedEventArgs e)
        {
            if(!Config.ModEnabled)
            {
                return;
            }
            if(Game1.player.modData.TryGetValue(modKey, out var dataString))
            {
                ClockSoundMenu.soundList = JsonConvert.DeserializeObject<List<ClockSound>>(dataString);
            }
        }
        private void GameLoop_Saving(object sender, StardewModdingAPI.Events.SavingEventArgs e)
        {
            Game1.player.modData[modKey] = JsonConvert.SerializeObject(ClockSoundMenu.soundList);
        }

        private void Input_ButtonsChanged(object sender, StardewModdingAPI.Events.ButtonsChangedEventArgs e)
        {
            if (Context.CanPlayerMove && Game1.activeClickableMenu is null && Config.ModEnabled && Config.MenuButton.JustPressed())
            {
                Game1.playSound("bigSelect");
                Game1.activeClickableMenu = new ClockSoundMenu();
            }
        }

        private void GameLoop_TimeChanged(object sender, StardewModdingAPI.Events.TimeChangedEventArgs e)
        {
            CheckForSound(e.NewTime);
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
                name: () => SHelper.Translation.Get("GMCM_Option_ModEnabled_Name"),
                getValue: () => Config.ModEnabled,
                setValue: value => Config.ModEnabled = value
            );
            
            configMenu.AddTextOption(
                mod: ModManifest,
                name: () => SHelper.Translation.Get("GMCM_Option_DefaultSound_Name"),
                getValue: () => Config.DefaultSound,
                setValue: value => Config.DefaultSound = value
            );
            configMenu.AddKeybindList(
                mod: ModManifest,
                name: () => SHelper.Translation.Get("GMCM_Option_MenuButton_Name"),
                getValue: () => Config.MenuButton,
                setValue: value => Config.MenuButton = value
            );

        }
    }
}