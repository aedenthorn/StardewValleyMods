using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Locations;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using System;
using System.Globalization;
using System.Linq;
using Object = StardewValley.Object;

namespace CropGrowthIndicator
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

            context = this;

            SMonitor = Monitor;
            SHelper = helper;

            helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            helper.Events.Display.RenderedWorld += Display_RenderedWorld;
        }

        int ticks;

        private void Display_RenderedWorld(object sender, RenderedWorldEventArgs e)
        {
            if (Config.EnableMod && Context.CanPlayerMove && Game1.currentLocation.terrainFeatures.TryGetValue(Game1.currentCursorTile, out TerrainFeature feature) && feature is HoeDirt dirt && dirt.crop is not null)
            {
                string text = "";
                string text2 = null;
                float scale = 1;
                if(dirt.crop.currentPhase.Value >= dirt.crop.phaseDays.Count - 1)
                {
                    text = Helper.Translation.Get("ready");
                }
                else
                {
                    int phase = dirt.crop.currentPhase.Value;
                    int days = dirt.crop.dayOfCurrentPhase.Value;
                    int maxdays = dirt.crop.phaseDays[dirt.crop.currentPhase.Value];
                    int totaldays = 0;
                    int totalmaxdays = 0;
                    for (int i = 0; i < dirt.crop.phaseDays.Count - 1; i++)
                    {
                        int p = dirt.crop.phaseDays[i];
                        totalmaxdays += p;
                        if (p < phase)
                        {
                            totaldays += p;
                        }
                        else if (p == phase)
                        {
                            totaldays += days;
                        }
                    }
                    if (Config.ShowDaysInCurrentPhase)
                    {
                        text2 = string.Format(Helper.Translation.Get("x/y"), days, maxdays);
                    }
                    text = string.Format(Helper.Translation.Get("x/y"), totaldays, totalmaxdays);
                }
                float xOffset = Game1.smallFont.MeasureString(text).X / 2;
                e.SpriteBatch.DrawString(Game1.smallFont, text, Game1.GlobalToLocal(Game1.currentCursorTile * 64 + new Vector2(32 - xOffset - 1, 31)) , Color.Black);
                e.SpriteBatch.DrawString(Game1.smallFont, text, Game1.GlobalToLocal(Game1.currentCursorTile * 64 + new Vector2(32 - xOffset, 32)) , Config.TotalGrowthColor);
                if(text2 is not null)
                {
                    xOffset = Game1.smallFont.MeasureString(text2).X / 2;
                    e.SpriteBatch.DrawString(Game1.smallFont, text2, Game1.GlobalToLocal(Game1.currentCursorTile * 64 + new Vector2(32 - xOffset - 1, -1)), Color.Black);
                    e.SpriteBatch.DrawString(Game1.smallFont, text2, Game1.GlobalToLocal(Game1.currentCursorTile * 64 + new Vector2(32 - xOffset, 0)), Config.CurrentGrowthColor);
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
                getValue: () => Config.EnableMod,
                setValue: value => Config.EnableMod = value
            );
        }

    }
}