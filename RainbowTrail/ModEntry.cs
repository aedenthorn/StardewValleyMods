using HarmonyLib;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using System.Collections.Generic;

namespace RainbowTrail
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry : Mod
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;

        public static ModEntry context;
        
        public static string rainbowTrailKey = "aedenthorn.RainbowTrail";

        public static Texture2D rainbowTexture;

        public static Dictionary<long, Queue<PositionInfo>> trailDict = new();

        //public static string dictPath = "aedenthorn.RainbowTrail/dictionary";
        //public static Dictionary<string, RainbowTrailData> dataDict = new();

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
            helper.Events.GameLoop.OneSecondUpdateTicked += GameLoop_OneSecondUpdateTicked;
            helper.Events.GameLoop.DayStarted += GameLoop_DayStarted;
            helper.Events.GameLoop.DayEnding += GameLoop_DayEnding;
            helper.Events.GameLoop.ReturnedToTitle += GameLoop_ReturnedToTitle;

            helper.Events.Player.Warped += Player_Warped;

            helper.Events.Content.AssetRequested += Content_AssetRequested;

            helper.Events.Input.ButtonsChanged += Input_ButtonsChanged;
            
            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();

        }

        private void Content_AssetRequested(object sender, StardewModdingAPI.Events.AssetRequestedEventArgs e)
        {
            if (e.NameWithoutLocale.IsEquivalentTo(rainbowTrailKey))
            {
                e.LoadFromModFile<Texture2D>("assets/rainbow.png", StardewModdingAPI.Events.AssetLoadPriority.Low);
            }
        }

        private void GameLoop_DayEnding(object sender, StardewModdingAPI.Events.DayEndingEventArgs e)
        {
            ResetTrail();
        }

        private void Player_Warped(object sender, StardewModdingAPI.Events.WarpedEventArgs e)
        {
            if (!Config.ModEnabled || e.Player != Game1.player || !RainbowTrailStatus(e.Player))
                return;
            if(Game1.CurrentEvent != null)
            {
                ResetTrail();
            }
        }

        private void GameLoop_ReturnedToTitle(object sender, StardewModdingAPI.Events.ReturnedToTitleEventArgs e)
        {
            ResetTrail();
        }

        private void GameLoop_DayStarted(object sender, StardewModdingAPI.Events.DayStartedEventArgs e)
        {
            ResetTrail();
        }

        private void GameLoop_OneSecondUpdateTicked(object sender, StardewModdingAPI.Events.OneSecondUpdateTickedEventArgs e)
        {
            if(!Config.ModEnabled || !RainbowTrailStatus(Game1.player) || Config.StaminaUse <= 0) 
                return;
            if(Game1.player.Stamina <= Config.StaminaUse)
            {
                Game1.player.modData.Remove(rainbowTrailKey);
                return;
            }
            Game1.player.Stamina -= Config.StaminaUse;
        }


        private void Input_ButtonsChanged(object sender, StardewModdingAPI.Events.ButtonsChangedEventArgs e)
        {
            if(!Config.ModEnabled || !Context.CanPlayerMove || !Config.ToggleKey.JustPressed()) 
                return;
            if (RainbowTrailStatus(Game1.player))
            {
                Game1.player.modData.Remove(rainbowTrailKey);
            }
            else
            {
                if(!string.IsNullOrEmpty(Config.EnableSound))
                    Game1.currentLocation.playSound(Config.EnableSound);
                Game1.player.modData[rainbowTrailKey] = "true";
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
            

            configMenu.AddKeybindList(
                mod: ModManifest,
                name: () => "Toggle Key",
                getValue: () => Config.ToggleKey,
                setValue: value => Config.ToggleKey = value
            );
            
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Max Length",
                getValue: () => Config.MaxLength,
                setValue: value => Config.MaxLength = value
            );
            
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Move Speed",
                getValue: () => Config.MoveSpeed,
                setValue: value => Config.MoveSpeed = value
            );
            
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Stamina Use",
                getValue: () => Config.StaminaUse,
                setValue: value => Config.StaminaUse = value
            );

            configMenu.AddTextOption(
                mod: ModManifest,
                name: () => "Enable Sound",
                getValue: () => Config.EnableSound,
                setValue: value => Config.EnableSound = value
            );
        }
    }
}