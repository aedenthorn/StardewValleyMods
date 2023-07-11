using HarmonyLib;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewModdingAPI.Events;
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
        public static int elapsedTicks;

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
            helper.Events.GameLoop.UpdateTicked += GameLoop_UpdateTicked;
            helper.Events.Display.Rendered += Display_Rendered;
            helper.Events.Input.CursorMoved += PlayerInput;
            helper.Events.Input.MouseWheelScrolled += PlayerInput;
            helper.Events.Input.ButtonPressed += PlayerInput;

            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();

        }

        private void Display_Rendered(object sender, RenderedEventArgs e)
        {
            if (!Config.ModEnabled || !Config.ShowAFKText || Config.FreezeGame || elapsedTicks < Config.ticksTilAFK || !Context.IsPlayerFree)
                return;
            SpriteText.drawStringWithScrollCenteredAt(e.SpriteBatch, Config.AFKText, Game1.viewport.Width / 2, Game1.viewport.Height / 2);
        }

        private void GameLoop_UpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            if (Game1.activeClickableMenu is AFKMenu)
                return;
            if (!Config.ModEnabled || !Context.CanPlayerMove || Game1.player.movementDirections.Count > 0 || (Game1.player.CurrentTool is FishingRod && (Game1.player.CurrentTool as FishingRod).inUse()) || Game1.input.GetKeyboardState().GetPressedKeys().Length > 0 || (byte)AccessTools.Field(typeof(MouseState), "_buttons").GetValue(Game1.input.GetMouseState()) > 0)
            {
                elapsedTicks = 0;
                return;
            }
            if (elapsedTicks >= Config.ticksTilAFK && Config.FreezeGame)
            {
                SMonitor.Log("Going AFK");
                Game1.activeClickableMenu = new AFKMenu();
            }
            else if (elapsedTicks < Config.ticksTilAFK)
                elapsedTicks++;
        }

        private void PlayerInput(object sender, object e)
        {
            if (e is CursorMovedEventArgs && !Config.WakeOnMouseMove)
                return;
            elapsedTicks = 0;
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
                name: () => SHelper.Translation.Get("GMCM_Option_ModEnabled_Name"),
                getValue: () => Config.ModEnabled,
                setValue: value => Config.ModEnabled = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => SHelper.Translation.Get("GMCM_Option_FreezeGame_Name"),
                tooltip: () => SHelper.Translation.Get("GMCM_Option_FreezeGame_Tooltip"),
                getValue: () => Config.FreezeGame,
                setValue: value => Config.FreezeGame = value
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => SHelper.Translation.Get("GMCM_Option_TicksTilAFK_Name"),
                getValue: () => Config.ticksTilAFK,
                setValue: value => Config.ticksTilAFK = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => SHelper.Translation.Get("GMCM_Option_ShowAFKText_Name"),
                getValue: () => Config.ShowAFKText,
                setValue: value => Config.ShowAFKText = value
            );
            configMenu.AddTextOption(
                mod: ModManifest,
                name: () => SHelper.Translation.Get("GMCM_Option_AFKText_Name"),
                getValue: () => Config.AFKText,
                setValue: value => Config.AFKText = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => SHelper.Translation.Get("GMCM_Option_WakeOnMouseMove_Name"),
                getValue: () => Config.WakeOnMouseMove,
                setValue: value => Config.WakeOnMouseMove = value
            );

        }

    }
}