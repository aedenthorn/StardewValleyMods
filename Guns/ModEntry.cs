using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using Newtonsoft.Json;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Objects;
using StardewValley.Projectiles;
using StardewValley.Tools;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.RegularExpressions;
using xTile;
using xTile.Dimensions;
using xTile.Tiles;
using static StardewValley.Projectiles.BasicProjectile;
using static System.Net.Mime.MediaTypeNames;

namespace Guns
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry : Mod
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;

        public static ModEntry context;
        public static bool isFiring;
        public static int fireTicks;
        public static Texture2D gunTexture;
        
        public static string dictPath = "aedenthorn.Guns/dictionary";
        public static Dictionary<string, TweakData> tweakDict = new Dictionary<string, TweakData>();
        public static int altFrame;

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
            Helper.Events.GameLoop.UpdateTicked += GameLoop_UpdateTicked;

            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();

        }

        private void GameLoop_UpdateTicked(object sender, StardewModdingAPI.Events.UpdateTickedEventArgs e)
        {
            if(isFiring)
            {
                fireTicks++;
                altFrame = (fireTicks / 5 % 2);
                if(fireTicks % 5 == 0)
                {
                    Game1.playSound("shiny4");
                    Game1.currentLocation.projectiles.Add(new BasicProjectile(10, 380, 0, 0, 0, Game1.player.FacingDirection == 3 ? -25 : 25, 0, Game1.player.Position + new Vector2(Game1.player.FacingDirection == 3 ? -64 : 48, -48), "", "", false, true, Game1.player.currentLocation, Game1.player, true, null));
                }
            }
            else
            {
                fireTicks = 0;
            }
        }
        private void GameLoop_SaveLoaded(object sender, StardewModdingAPI.Events.SaveLoadedEventArgs e)
        {
            gunTexture = Helper.ModContent.Load<Texture2D>("assets/uzi.png");
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
        }
    }
}