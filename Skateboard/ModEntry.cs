using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using System;

namespace Skateboard
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry : Mod
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;

        public static ModEntry context;

        public static readonly string boardKey = "aedenthorn.Skateboard/Board";

        public static bool onSkateboard;
        public static Vector2 speed;
        public static bool accellerating;
        private static Texture2D boardTexture;

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
            helper.Events.GameLoop.SaveLoaded += GameLoop_SaveLoaded;
            helper.Events.Input.ButtonPressed += Input_ButtonPressed;
            helper.Events.Content.AssetRequested += Content_AssetRequested;

            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();

        }

        private void GameLoop_SaveLoaded(object sender, StardewModdingAPI.Events.SaveLoadedEventArgs e)
        {
            boardTexture = Game1.content.Load<Texture2D>(boardKey);
            Monitor.Log("loaded skateboard texture");
        }

        private void Content_AssetRequested(object sender, StardewModdingAPI.Events.AssetRequestedEventArgs e)
        {
            if(e.Name.BaseName == boardKey)
            {
                e.LoadFromModFile<Texture2D>("assets/board.png", StardewModdingAPI.Events.AssetLoadPriority.Low);
            }
        }

        private void GameLoop_UpdateTicked(object sender, StardewModdingAPI.Events.UpdateTickedEventArgs e)
        {
        }

        private void Input_ButtonPressed(object sender, StardewModdingAPI.Events.ButtonPressedEventArgs e)
        {
            if(Config.ModEnabled && Context.CanPlayerMove && e.Button == SButton.V)
            {
                speed = Vector2.Zero;
                onSkateboard = !onSkateboard;
                Monitor.Log($"Skateboarding: {onSkateboard}");
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
                name: () => "Mod Enabled?",
                getValue: () => Config.ModEnabled,
                setValue: value => Config.ModEnabled = value
            );
        }
    }
}