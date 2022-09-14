using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using System.Collections.Generic;

namespace PlantAll
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


            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();
        }
        private void Display_RenderedWorld(object sender, RenderedWorldEventArgs e)
        {
            if (Config.EnableMod && Context.IsPlayerFree && Game1.player.CurrentItem?.Category == -74 && (Helper.Input.IsDown(Config.ModButton) || Helper.Input.IsDown(Config.StraightModButton) || Helper.Input.IsDown(Config.SprinklerModButton)))
            {
                Vector2 grabTile = Game1.GetPlacementGrabTile();
                if (((!Game1.player.currentLocation.terrainFeatures.TryGetValue(new Vector2((int)grabTile.X, (int)grabTile.Y), out TerrainFeature f) || f is not HoeDirt) && (!Game1.player.currentLocation.objects.TryGetValue(new Vector2((int)grabTile.X, (int)grabTile.Y), out Object o) || o is not IndoorPot)) || !Utility.playerCanPlaceItemHere(Game1.player.currentLocation, Game1.player.CurrentItem, (int)grabTile.X * 64, (int)grabTile.Y * 64, Game1.player))
                    return;
                List<Point> placeables = new List<Point>();
                GetPlaceable(Game1.player.CurrentItem, Game1.player.currentLocation, (int)grabTile.X, (int)grabTile.Y, (int)grabTile.X, (int)grabTile.Y, placeables);
                foreach (Point p in placeables)
                {
                    e.SpriteBatch.Draw(Game1.mouseCursors, Game1.GlobalToLocal(new Vector2((float)((int)p.X * 64), (float)((int)p.Y * 64))), new Rectangle?(new Rectangle(194, 388, 16, 16)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.01f);
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
                name: () => "Mod Enabled?",
                getValue: () => Config.EnableMod,
                setValue: value => Config.EnableMod = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Allow Diagonal Join?",
                getValue: () => Config.AllowDiagonal,
                setValue: value => Config.AllowDiagonal = value
            );
            configMenu.AddKeybind(
                mod: ModManifest,
                name: () => "Modifier Key",
                getValue: () => Config.ModButton,
                setValue: value => Config.ModButton = value
            );
            configMenu.AddKeybind(
                mod: ModManifest,
                name: () => "Straight Mod Key",
                getValue: () => Config.StraightModButton,
                setValue: value => Config.StraightModButton = value
            );
        }
    }

}