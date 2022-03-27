using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Objects;
using Object = StardewValley.Object;

namespace WallTelevision
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry : Mod
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;
        public static ModEntry context;

        private static Texture2D plasmaTexture;
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
            helper.Events.GameLoop.SaveLoaded += GameLoop_SaveLoaded;
            helper.Events.GameLoop.UpdateTicking += GameLoop_UpdateTicking;
            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();
        }

        private void GameLoop_SaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            try
            {
                plasmaTexture = Game1.content.Load<Texture2D>("aedenthorn.WallTelevision/wall_plasma_tv");
            }
            catch
            {
                plasmaTexture = Helper.Content.Load<Texture2D>("assets/wall_plasma_tv.png");
            }
        }

        private void GameLoop_UpdateTicking(object sender, UpdateTickingEventArgs e)
        {
            if (!Config.EnableMod || !Context.IsWorldReady)
                return;
            int delta = Helper.Input.IsDown(Config.UpButton) ? 1 : (Helper.Input.IsDown(Config.DownButton) ? -1 : 0);
            if (delta != 0 && typeof(DecoratableLocation).IsAssignableFrom(Game1.currentLocation.GetType()))
            {
                foreach (var f in Game1.currentLocation.furniture)
                {
                    if (f.boundingBox.Value.Contains(Game1.viewport.X + Game1.getOldMouseX(), Game1.viewport.Y + Game1.getOldMouseY()) && f is TV && (Game1.currentLocation as DecoratableLocation).isTileOnWall((int)f.TileLocation.X, (int)f.TileLocation.Y))
                    {

                        f.boundingBox.Value = new Rectangle(f.boundingBox.Value.Location - new Point(0, delta), f.boundingBox.Value.Size);
                        f.updateDrawPosition();

                        return;
                    }
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
            configMenu.AddKeybind(
                mod: ModManifest,
                name: () => "Up Button",
                tooltip: () => "Moves the TV up when held down while hovering over it",
                getValue: () => Config.UpButton,
                setValue: value => Config.UpButton = value
            );
            configMenu.AddKeybind(
                mod: ModManifest,
                name: () => "Down Button",
                tooltip: () => "Moves the TV down when held down while hovering over it",
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