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
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace Guns
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry : Mod
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;

        public static ModEntry context;
        public static Texture2D gunTexture;
        
        public static string dictPath = "aedenthorn.Guns/dictionary";
        public static string firingKey = "aedenthorn.Guns/firing";
        public static Dictionary<string, GunData> gunDict = new Dictionary<string, GunData>();
        public static Dictionary<long, string> farmerDict = new Dictionary<long, string>();

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            Config = Helper.ReadConfig<ModConfig>();

            context = this;

            SMonitor = Monitor;
            SHelper = helper;

            Helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            Helper.Events.GameLoop.DayStarted += GameLoop_DayStarted;
            Helper.Events.GameLoop.UpdateTicked += GameLoop_UpdateTicked;
            Helper.Events.Content.AssetRequested += Content_AssetRequested;

            //var data = new GunData();
            //File.WriteAllText("content.json", JsonConvert.SerializeObject(data));

            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();
            
        }


        private void Content_AssetRequested(object sender, StardewModdingAPI.Events.AssetRequestedEventArgs e)
        {
            if(!Config.ModEnabled)
            {
                return;
            }
            if(e.NameWithoutLocale.IsEquivalentTo(dictPath))
            {
                e.LoadFrom(() => new Dictionary<string, GunData>(), StardewModdingAPI.Events.AssetLoadPriority.Exclusive);
            }
        }

        private void GameLoop_UpdateTicked(object sender, StardewModdingAPI.Events.UpdateTickedEventArgs e)
        {
        }
        private void GameLoop_DayStarted(object sender, StardewModdingAPI.Events.DayStartedEventArgs e)
        {
            gunDict = Helper.GameContent.Load<Dictionary<string, GunData>>(dictPath);
            foreach(var key in gunDict.Keys.ToArray()) 
            {
                gunDict[key].gunTexture = Helper.GameContent.Load<Texture2D>(gunDict[key].gunTexturePath);
            }
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