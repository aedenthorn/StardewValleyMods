using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;

namespace FurnitureAdjustment
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry : Mod
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;

        public static ModEntry context;

        public static int ticks = 0;

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
            helper.Events.GameLoop.UpdateTicked += GameLoop_UpdateTicked;
            helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
        }

        private void GameLoop_UpdateTicked(object sender, StardewModdingAPI.Events.UpdateTickedEventArgs e)
        {
            if (!Config.EnableMod || !Context.IsPlayerFree)
                return;
            if (++ticks < Config.MoveSpeed)
                return;
            ticks = 0;
            if (Helper.Input.IsDown(Config.RaiseButton) || Helper.Input.IsSuppressed(Config.RaiseButton))
            {
                MoveFurniture(0, -1, Config.RaiseButton);
            }
            else if (Helper.Input.IsDown(Config.LowerButton) || Helper.Input.IsSuppressed(Config.LowerButton))
            {
                MoveFurniture(0, 1, Config.LowerButton);
            }
            else if (Helper.Input.IsDown(Config.LeftButton) || Helper.Input.IsSuppressed(Config.LeftButton))
            {
                MoveFurniture(-1, 0, Config.LeftButton);
            }
            else if (Helper.Input.IsDown(Config.RightButton) || Helper.Input.IsSuppressed(Config.RightButton))
            {
                MoveFurniture(1, 0, Config.RightButton);
            }
        }

        private void Input_ButtonPressed(object sender, StardewModdingAPI.Events.ButtonPressedEventArgs e)
        {
            if (!Config.EnableMod || !Context.IsWorldReady)
                return;
            if (e.Button == Config.RaiseButton)
            {
                MoveFurniture(0, -1, e.Button);
            }
            else if (e.Button == Config.LowerButton)
            {
                MoveFurniture(0, 1, e.Button);
            }
            else if (e.Button == Config.LeftButton)
            {
                MoveFurniture(-1, 0, e.Button);
            }
            else if (e.Button == Config.RightButton)
            {
                MoveFurniture(1, 0, e.Button);
            }

        }

        private void MoveFurniture(int x, int y, SButton button)
        {
            int mod = (Helper.Input.IsDown(Config.ModKey) ? Config.ModSpeed : 1);
            Point shift = new Point(x * mod, y * mod);
            foreach (var f in Game1.currentLocation.furniture)
            {
                if (f.boundingBox.Value.Contains(Game1.viewport.X + Game1.getOldMouseX(), Game1.viewport.Y + Game1.getOldMouseY()))
                {
                    f.RemoveLightGlow(Game1.currentLocation);
                    f.boundingBox.Value = new Rectangle(f.boundingBox.Value.Location + shift, f.boundingBox.Value.Size);
                    f.updateDrawPosition();
                    if(Config.MoveCursor)
                        Game1.setMousePosition(Game1.getOldMouseX() + shift.X, Game1.getOldMouseY() + shift.Y);
                    f.removeLights(Game1.currentLocation);

                    Helper.Input.Suppress(button);
                    /*
                    var ptr = typeof(Object).GetMethod("placementAction", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance).MethodHandle.GetFunctionPointer();
                    var basePlacementAction = (Func<GameLocation, int, int, Farmer, bool>)Activator.CreateInstance(typeof(Func<GameLocation, int, int, Farmer, bool>), f, ptr);
                    basePlacementAction(Game1.currentLocation, (int)f.TileLocation.X + shift.X, (int)f.TileLocation.Y + shift.Y, Game1.player);
                    */
                    return;
                }
            }
            //Monitor.Log($"no wall furniture at {Game1.viewport.X + Game1.getOldMouseX()},{Game1.viewport.Y + Game1.getOldMouseY()}");
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
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Move Cursor with Furniture?",
                getValue: () => Config.MoveCursor,
                setValue: value => Config.MoveCursor = value
            );
            configMenu.AddKeybind(
                mod: ModManifest,
                name: () => "Raise Button",
                getValue: () => Config.RaiseButton,
                setValue: value => Config.RaiseButton = value
            );
            configMenu.AddKeybind(
                mod: ModManifest,
                name: () => "Lower Button",
                getValue: () => Config.LowerButton,
                setValue: value => Config.LowerButton = value
            );
            configMenu.AddKeybind(
                mod: ModManifest,
                name: () => "Left Button",
                getValue: () => Config.LeftButton,
                setValue: value => Config.LeftButton = value
            );
            configMenu.AddKeybind(
                mod: ModManifest,
                name: () => "Right Button",
                getValue: () => Config.RightButton,
                setValue: value => Config.RightButton = value
            );
            configMenu.AddKeybind(
                mod: ModManifest,
                name: () => "Mod Button",
                getValue: () => Config.ModKey,
                setValue: value => Config.ModKey = value
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Mod Speed",
                getValue: () => Config.ModSpeed,
                setValue: value => Config.ModSpeed = value,
                min: 2,
                max: 64
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Ticks Per Move",
                getValue: () => Config.MoveSpeed,
                setValue: value => Config.MoveSpeed = value,
                min: 1,
                max: 30
            );
        }

    }

}