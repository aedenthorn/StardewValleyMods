using HarmonyLib;
using Microsoft.Xna.Framework;
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

namespace ThrowableBombs
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry : Mod
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;

        public static ModEntry context;

        public static string explodingKey = "aedenthorn.ThrowableBombs/exploding";

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
            helper.Events.GameLoop.UpdateTicking += GameLoop_UpdateTicking;

            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();

        }

        private void GameLoop_UpdateTicking(object sender, StardewModdingAPI.Events.UpdateTickingEventArgs e)
        {
            if (!Config.ModEnabled)
                return;
            float throwSpeed = 8;
            float heightOffset = 32;
            foreach (var key in bombDict.Keys.ToArray())
            {
                var sprite = bombDict[key].location.getTemporarySpriteByID(key);
                if (sprite is null)
                    continue;
                float distanceTravelled = Vector2.Distance(bombDict[key].startPos, bombDict[key].currentPos);
                float totalDistance = Vector2.Distance(bombDict[key].startPos, bombDict[key].endPos);
                float distanceRemain = totalDistance - distanceTravelled;
                float height = (float)Math.Sin(distanceTravelled / totalDistance) * heightOffset; 
                bombDict[key].currentPos = Vector2.Lerp(bombDict[key].currentPos, bombDict[key].endPos, throwSpeed / distanceRemain);
                
                bombDict[key].location.TemporarySprites[key].Position = bombDict[key].startPos + bombDict[key].currentPos - bombDict[key].startPos
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
        }

    }
}