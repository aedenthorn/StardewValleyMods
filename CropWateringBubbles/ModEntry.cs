using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CropWateringBubbles
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry : Mod
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;

        public static ModEntry context;

        public static bool isEmoting;
        public static bool emoteFading;
        public static int currentEmoteFrame;
        public static float emoteInterval;
        public static int repeatInterval;

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            Config = Helper.ReadConfig<ModConfig>();

            context = this;

            SMonitor = Monitor;
            SHelper = helper;

            Helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            Helper.Events.GameLoop.UpdateTicked += GameLoop_UpdateTicked;
            Helper.Events.Input.ButtonsChanged += Input_ButtonsChanged;
            Helper.Events.Player.Warped += Player_Warped;

            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();
        }

        private void Player_Warped(object sender, StardewModdingAPI.Events.WarpedEventArgs e)
        {
            if (!e.Player.IsLocalPlayer)
                return;
            isEmoting = false;
        }

        private void GameLoop_UpdateTicked(object sender, StardewModdingAPI.Events.UpdateTickedEventArgs e)
        {
            if (!Config.ModEnabled)
                return;
            if (Config.OnlyWhenWatering && Game1.player.CurrentTool is not WateringCan)
            {
                isEmoting = false;
                emoteFading = false;
                currentEmoteFrame = 0;
                emoteInterval = 0;
                repeatInterval = 0;
                return;
            }
            if (isEmoting)
            {
                updateEmote();
            }
            else if (!Config.RequireKeyPress)
            {
                if (repeatInterval <= 0)
                {
                    repeatInterval = Config.RepeatInterval * 60;
                    isEmoting = true;
                }
                repeatInterval--;
            }
        }

        private void Input_ButtonsChanged(object sender, ButtonsChangedEventArgs e)
        {
            if (Config.ModEnabled && Context.CanPlayerMove && !isEmoting && Config.RequireKeyPress && Config.PressKeys.JustPressed() && (!Config.OnlyWhenWatering || Game1.player.CurrentTool is WateringCan))
            {
                isEmoting = true;
                Game1.playSound("dwoop");
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
                name: () => SHelper.Translation.Get("GMCM_Option_ModEnabled_Name"),
                getValue: () => Config.ModEnabled,
                setValue: value => Config.ModEnabled = value
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => SHelper.Translation.Get("GMCM_Option_RepeatInterval_Name"),
                getValue: () => Config.RepeatInterval,
                setValue: value => Config.RepeatInterval = value,
                min: 1,
                max: 100
            );

            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => SHelper.Translation.Get("GMCM_Option_OnlyWhenWatering_Name"),
                getValue: () => Config.OnlyWhenWatering,
                setValue: value => Config.OnlyWhenWatering = value
            );

            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => SHelper.Translation.Get("GMCM_Option_RequireKeyPress_Name"),
                getValue: () => Config.RequireKeyPress,
                setValue: value => Config.RequireKeyPress = value
            );

            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => SHelper.Translation.Get("GMCM_Option_IncludeGiantable_Name"),
                getValue: () => Config.IncludeGiantable,
                setValue: value => Config.IncludeGiantable = value
            );

            configMenu.AddKeybindList(
                mod: ModManifest,
                name: () => SHelper.Translation.Get("GMCM_Option_PressKeys_Name"),
                getValue: () => Config.PressKeys,
                setValue: value => Config.PressKeys = value
            );

            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => SHelper.Translation.Get("GMCM_Option_OpacityPercent_Name"),
                getValue: () => Config.OpacityPercent,
                setValue: value => Config.OpacityPercent = value,
                min: 1,
                max: 100
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => SHelper.Translation.Get("GMCM_Option_SizePercent_Name"),
                getValue: () => Config.SizePercent,
                setValue: value => Config.SizePercent = value,
                min: 1,
                max: 100
            );

        }
    }
}