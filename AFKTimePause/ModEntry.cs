using HarmonyLib;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Locations;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;

namespace AFKTimePause
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry : Mod
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;

        public static ModEntry context;
        public static int elapsedSeconds;

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
            helper.Events.Display.Rendered += Display_Rendered;
            helper.Events.Input.CursorMoved += PlayerInput;
            helper.Events.Input.MouseWheelScrolled += PlayerInput;
            helper.Events.Input.ButtonPressed += PlayerInput;

            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();

        }

        private void Display_Rendered(object sender, StardewModdingAPI.Events.RenderedEventArgs e)
        {
            if (!Config.ModEnabled || !Config.ShowAFKText || elapsedSeconds < Config.SecondsTilAFK || !Context.IsPlayerFree)
                return;
            SpriteText.drawStringWithScrollCenteredAt(e.SpriteBatch, Config.AFKText, Game1.viewport.Width / 2, Game1.viewport.Height / 2);
        }

        private void GameLoop_OneSecondUpdateTicked(object sender, StardewModdingAPI.Events.OneSecondUpdateTickedEventArgs e)
        {
            if (!Config.ModEnabled || !Context.IsPlayerFree || (Game1.player.CurrentTool is FishingRod && (Game1.player.CurrentTool as FishingRod).inUse()))
            {
                elapsedSeconds = 0;
                return;
            }
            if (elapsedSeconds == Config.SecondsTilAFK && Config.FreezeGame)
            {
                SMonitor.Log("Going AFK");
                Game1.activeClickableMenu = new AFKMenu();
                elapsedSeconds++;
            }
            else if (elapsedSeconds < Config.SecondsTilAFK)
                elapsedSeconds++;
        }

        private void PlayerInput(object sender, object e)
        {
            elapsedSeconds = 0;
            if (Game1.activeClickableMenu is AFKMenu)
                Game1.activeClickableMenu = null;
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
                name: () => "Freeze Game",
                getValue: () => Config.FreezeGame,
                setValue: value => Config.FreezeGame = value
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Seconds Until AFK",
                getValue: () => Config.SecondsTilAFK,
                setValue: value => Config.SecondsTilAFK = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Show AFK Text",
                getValue: () => Config.ShowAFKText,
                setValue: value => Config.ShowAFKText = value
            );
            configMenu.AddTextOption(
                mod: ModManifest,
                name: () => "AFK Text",
                getValue: () => Config.AFKText,
                setValue: value => Config.AFKText = value
            );

        }

    }
}