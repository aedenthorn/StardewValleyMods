using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Permissions;
using xTile.Tiles;
using Object = StardewValley.Object;

namespace ChestPreview
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

            if (!Config.ModEnabled)
                return;

            context = this;

            SMonitor = Monitor;
            SHelper = helper;

            Helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            Helper.Events.Display.RenderedWorld += Display_RenderedWorld;

            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();
        }

        private void Display_RenderedWorld(object sender, StardewModdingAPI.Events.RenderedWorldEventArgs e)
        {
            if (!Config.ModEnabled || !Helper.Input.IsDown(Config.ModKey))
                return;
            if (Config.ShowWhenHover)
            {
                if (Game1.player.currentLocation.Objects.TryGetValue(Game1.currentCursorTile, out Object obj) && obj is Chest)
                {
                    ShowChestPreview(e.SpriteBatch, obj as Chest, Game1.currentCursorTile);
                    return;
                }
            }
            if (Config.ShowWhenFacing)
            {
                Vector2 tile = Game1.player.getTileLocation();
                switch (Game1.player.FacingDirection)
                {
                    case 0:
                        tile.Y -= 1;
                        break;
                    case 1:
                        tile.X += 1;
                        break;
                    case 2:
                        tile.Y += 1;
                        break;
                    case 3:
                        tile.X -= 1;
                        break;
                }
                if (Game1.player.currentLocation.Objects.TryGetValue(tile, out Object obj) && obj is Chest)
                {
                    ShowChestPreview(e.SpriteBatch, obj as Chest, tile);
                    return;
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
                getValue: () => Config.ModEnabled,
                setValue: value => Config.ModEnabled = value
            );
            configMenu.AddKeybind(
                mod: ModManifest,
                name: () => "Mod Key",
                getValue: () => Config.ModKey,
                setValue: value => Config.ModKey = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Show When Facing",
                getValue: () => Config.ShowWhenFacing,
                setValue: value => Config.ShowWhenFacing = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Show When Hover",
                getValue: () => Config.ShowWhenHover,
                setValue: value => Config.ShowWhenHover = value
            );
        }
    }
}