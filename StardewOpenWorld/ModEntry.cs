using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Locations;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using xTile;

namespace StardewOpenWorld
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry : Mod
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;
        public static ModEntry context;

        public static string dataPath = "aedenthorn.StardewOpenWorld/dictionary";
        public static string namePrefix = "StardewOpenWorld";
        public static string tilePrefix = "StardewOpenWorldTile";

        private static GameLocation openWorldLocation;
        private static GameLocation openWorldTile;

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            Config = Helper.ReadConfig<ModConfig>();
            SMonitor = Monitor;
            SHelper = helper;

            context = this;

            helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            helper.Events.GameLoop.UpdateTicked += GameLoop_UpdateTicked;

            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();
        }

        private void GameLoop_UpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            if (!Config.ModEnabled || !Context.IsWorldReady || !Game1.player.currentLocation.Name.StartsWith(tilePrefix))
                return;
            var p = GetTileFromName(Game1.player.currentLocation.Name);
                    
            if(Game1.player.Position.X < 0)
            {
                if (p.X > 1)
                {
                    WarpToOpenWorldTile(p.X - 1, p.Y, Game1.player.Position + new Vector2(500 * 64, 0));
                    return;
                }
                else
                {
                    Game1.player.Position = new Vector2(0, Game1.player.Position.Y);
                }
            }
            if(Game1.player.Position.X >= 500 * 64)
            {
                if (p.X < 200)
                {
                    WarpToOpenWorldTile(p.X + 1, p.Y, Game1.player.Position + new Vector2(-500 * 64, 0));
                    return;
                }
                else
                {
                    Game1.player.Position = new Vector2(500 * 64, Game1.player.Position.Y);
                }
            }
            if(Game1.player.Position.Y < 0)
            {
                if (p.Y > 1)
                {
                    WarpToOpenWorldTile(p.X, p.Y - 1, Game1.player.Position + new Vector2(0, 500 * 64));
                    return;
                }
                else
                {
                    Game1.player.Position = new Vector2(Game1.player.Position.X, 0);
                }
            }
            if(Game1.player.Position.Y >= 500 * 64)
            {
                if (p.Y < 200)
                {
                    WarpToOpenWorldTile(p.X, p.Y + 1, Game1.player.Position + new Vector2(0, -500 * 64));
                    return;
                }
                else
                {
                    Game1.player.Position = new Vector2(Game1.player.Position.X, 500 * 64);
                }
            }
            
        }

        private void GameLoop_GameLaunched(object sender, GameLaunchedEventArgs e)
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
                name: () => "Draw Cart Exterior",
                getValue: () => Config.DrawCartExterior,
                setValue: value => Config.DrawCartExterior = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Draw Cart Exterior Weather",
                getValue: () => Config.DrawCartExteriorWeather,
                setValue: value => Config.DrawCartExteriorWeather = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Debug",
                getValue: () => Config.Debug,
                setValue: value => Config.Debug = value
            );
            configMenu.AddKeybind(
                mod: ModManifest,
                name: () => "Hitch Button",
                getValue: () => Config.HitchButton,
                setValue: value => Config.HitchButton = value
            );
        }
    }
}