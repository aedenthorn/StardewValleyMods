using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using System;
using System.Collections.Generic;

namespace UtilityGrid
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry : Mod
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;

        public static ModEntry context;
        public static Texture2D pipeTexture;

        public static bool ShowingGrid { get; set; } = false;
        public static bool ElectricGrid { get; set; } = false;
        public static int CurrentTile { get; set; } = 0;
        public static int CurrentRotation { get; set; } = 0;

        public static int[][] intakeArray = { new int[]{0, 1, 1, 1}, new int[]{1, 0, 0, 1}, new int[]{1, 0, 0, 0}, new int[]{0, 1, 0, 1}, new int[]{1, 1, 1, 1} };

        public Dictionary<Vector2, Point> waterPipes = new Dictionary<Vector2, Point>();
        public Dictionary<Vector2, Point> electricPipes = new Dictionary<Vector2, Point>();
        public List<PipeGroup> waterGroups = new List<PipeGroup>();
        public List<PipeGroup> electricGroups = new List<PipeGroup>();

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            Config = Helper.ReadConfig<ModConfig>();

            if (!Config.EnableMod)
                return;

            context = this;

            SMonitor = Monitor;
            SHelper = helper;

            helper.Events.Input.ButtonPressed += Input_ButtonPressed;
            helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            helper.Events.Display.RenderedWorld += Display_RenderedWorld;

            var harmony = new Harmony(ModManifest.UniqueID);

            // Game1 Patches
            /*
                        harmony.Patch(
                           original: AccessTools.Method(typeof(Game1), "_newDayAfterFade"),
                           prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.Game1__newDayAfterFade_Prefix))
                        );
            */

            pipeTexture = Helper.Content.Load<Texture2D>("assets/pipes.png");

        }
        private void Input_ButtonPressed(object sender, StardewModdingAPI.Events.ButtonPressedEventArgs e)
        {
            if (!Config.EnableMod || !(Game1.currentLocation is Farm) || !Game1.currentLocation.IsOutdoors)
                return;

            if (e.Button == Config.ToggleGrid)
            {
                Helper.Input.Suppress(e.Button);
                ShowingGrid = !ShowingGrid;
                Monitor.Log($"Showing grid: {ShowingGrid}");
            }
            if (!ShowingGrid)
                return;
            if (e.Button == Config.SwitchGrid)
            {
                Helper.Input.Suppress(e.Button);
                ElectricGrid = !ElectricGrid;
                Monitor.Log($"Showing Electric grid: {ElectricGrid}");
            }
            else if (e.Button == Config.SwitchTile)
            {
                Helper.Input.Suppress(e.Button);
                CurrentTile++;
                CurrentTile %= 6;
                CurrentRotation = 0;
                Monitor.Log($"Showing tile: {CurrentTile},{CurrentRotation}");
            }
            else if (e.Button == Config.RotateTile)
            {
                Helper.Input.Suppress(e.Button);
                CurrentRotation++;
                if (CurrentTile < 3)
                    CurrentRotation %= 4;
                else if (CurrentTile == 3)
                    CurrentRotation %= 2;
                else
                    CurrentRotation = 0;
                Monitor.Log($"Showing tile: {CurrentTile},{CurrentRotation}");
            }
            else if (e.Button == Config.PlaceTile)
            {
                Helper.Input.Suppress(e.Button);
                Dictionary<Vector2, Point> pipeDict;
                if (ElectricGrid)
                {
                    pipeDict = electricPipes;
                }
                else
                {
                    pipeDict = waterPipes;
                }
                if(CurrentTile == 5)
                    pipeDict.Remove(Game1.lastCursorTile);
                else
                    pipeDict[Game1.lastCursorTile] = new Point(CurrentTile, CurrentRotation);
                RemakeGroups(ElectricGrid);
                Monitor.Log($"Placing tile: {CurrentTile},{CurrentRotation} at {Game1.currentCursorTile}; connected? {PipeIsPowered(Game1.currentCursorTile, ElectricGrid)}");
            }
        }
        private void Display_RenderedWorld(object sender, StardewModdingAPI.Events.RenderedWorldEventArgs e)
        {
            if (!Config.EnableMod || !ShowingGrid)
                return;
            Dictionary<Vector2, Point> pipeDict;
            Color color;
            if (ElectricGrid)
            {
                pipeDict = electricPipes;
                color = Config.ElectricityColor;
            }
            else
            {
                pipeDict = waterPipes;
                color = Config.WaterColor;
            }
            foreach (var kvp in pipeDict)
            {
                if (kvp.Key == Game1.currentCursorTile)
                {
                    continue;
                }
                if (Utility.isOnScreen(new Vector2(kvp.Key.X * 64 + 32, kvp.Key.Y * 64 + 32), 32))
                {
                    if (kvp.Value.X == 4)
                        DrawTile(e.SpriteBatch, kvp.Key, new Point(3, 2), ElectricGrid, PipeIsPowered(kvp.Key, ElectricGrid) ? color : Color.White);
                    else
                        DrawTile(e.SpriteBatch, kvp.Key, kvp.Value, ElectricGrid, PipeIsPowered(kvp.Key, ElectricGrid) ? color : Color.White);
                }
            }
            if (CurrentTile == 4)
                DrawTile(e.SpriteBatch, Game1.currentCursorTile, new Point(3, 2), ElectricGrid, color);
            else if(CurrentTile != 5)
                DrawTile(e.SpriteBatch, Game1.currentCursorTile, new Point(CurrentTile, CurrentRotation), ElectricGrid, color);
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
                getValue: () => Config.EnableMod,
                setValue: value => Config.EnableMod = value
            );
            configMenu.AddKeybind(
                mod: ModManifest,
                name: () => "Toggle Grid Key",
                getValue: () => Config.ToggleGrid,
                setValue: value => Config.ToggleGrid = value
            );
            configMenu.AddKeybind(
                mod: ModManifest,
                name: () => "Switch Grid Key",
                getValue: () => Config.ToggleGrid,
                setValue: value => Config.ToggleGrid = value
            );
            configMenu.AddKeybind(
                mod: ModManifest,
                name: () => "Change Tile Key",
                getValue: () => Config.SwitchTile,
                setValue: value => Config.SwitchTile = value
            );
            configMenu.AddKeybind(
                mod: ModManifest,
                name: () => "Rotate Tile Key",
                getValue: () => Config.RotateTile,
                setValue: value => Config.RotateTile = value
            );
            configMenu.AddKeybind(
                mod: ModManifest,
                name: () => "Place Tile Key",
                getValue: () => Config.PlaceTile,
                setValue: value => Config.PlaceTile = value
            );
        }
    }
}