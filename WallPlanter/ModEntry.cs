using Force.DeepCloner;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using Rectangle = Microsoft.Xna.Framework.Rectangle;
using Object = StardewValley.Object;

namespace WallPlanter
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry : Mod
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;
        public static ModEntry context;

        private static List<string> loadedContentPacks = new List<string>();
        private static Texture2D wallPotTexture;
        private static Texture2D wallPotTextureWet;

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            Config = Helper.ReadConfig<ModConfig>();

            context = this;

            SMonitor = Monitor;
            SHelper = helper;

            helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            helper.Events.GameLoop.UpdateTicking += GameLoop_UpdateTicking;
            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();
        }

        private void GameLoop_UpdateTicking(object sender, UpdateTickingEventArgs e)
        {
            if (!Config.EnableMod)
                return;
            int delta = Helper.Input.IsDown(Config.UpButton) ? 1 : (Helper.Input.IsDown(Config.DownButton) ? -1 : 0);
            if (delta != 0 && typeof(DecoratableLocation).IsAssignableFrom(Game1.currentLocation.GetType()) && Game1.currentLocation.objects.TryGetValue(Game1.currentCursorTile, out Object obj) && obj is IndoorPot && (Game1.currentLocation as DecoratableLocation).isTileOnWall((int)obj.TileLocation.X, (int)obj.TileLocation.Y))
            {
                int offset = Config.OffsetY;
                string key = Helper.Input.IsDown(Config.ModKey) ? "aedenthorn.WallPlanter/innerOffset" : "aedenthorn.WallPlanter/offset";
                if (obj.modData.TryGetValue(key, out string offsetString))
                {
                    int.TryParse(offsetString, out offset);
                }
                obj.modData[key] = (offset + delta) + "";
            }
        }

        private void Input_ButtonPressed(object sender, ButtonPressedEventArgs e)
        {

        }

        private void GameLoop_GameLaunched(object sender, GameLaunchedEventArgs e)
        {
            wallPotTexture = Helper.Content.Load<Texture2D>("assets/wall_pot.png");
            wallPotTextureWet = Helper.Content.Load<Texture2D>("assets/wall_pot_wet.png");
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
            configMenu.AddKeybind(
                mod: ModManifest,
                name: () => "Mod Button",
                tooltip: () => "When held down, the up and down buttons move the contents instead",
                getValue: () => Config.UpButton,
                setValue: value => Config.UpButton = value
            );
            configMenu.AddKeybind(
                mod: ModManifest,
                name: () => "Up Button",
                tooltip: () => "Moves the pot up when held down while hovering over it",
                getValue: () => Config.UpButton,
                setValue: value => Config.UpButton = value
            );
            configMenu.AddKeybind(
                mod: ModManifest,
                name: () => "Down Button",
                tooltip: () => "Moves the pot down when held down while hovering over it",
                getValue: () => Config.ModKey,
                setValue: value => Config.ModKey = value
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Y Offset",
                tooltip: () => "Default wall position offset (positive is up)",
                getValue: () => Config.OffsetY,
                setValue: value => Config.OffsetY = value
            );
        }
    }
}