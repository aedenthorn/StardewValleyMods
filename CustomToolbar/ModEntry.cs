using Force.DeepCloner;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace CustomToolbar
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry : Mod
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;
        public static ModEntry context;
        private static Point totalDelta = Point.Zero;
        private static Harmony harmony;
        private static List<Type> menuTypes = new List<Type>();
        private static List<ClickableComponent> adjustedComponents = new List<ClickableComponent>();
        private static List<IClickableMenu> adjustedMenus = new List<IClickableMenu>();
        private static List<IClickableMenu> detachedMenus = new List<IClickableMenu>();

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            Config = Helper.ReadConfig<ModConfig>();

            context = this;

            SMonitor = Monitor;
            SHelper = helper;

            helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            helper.Events.Input.ButtonPressed += Input_ButtonPressed;

            harmony = new Harmony(ModManifest.UniqueID);
            harmony.Patch(
                original: AccessTools.Method(typeof(Toolbar), nameof(Toolbar.draw), new Type[] { typeof(SpriteBatch) }),
                prefix: new HarmonyMethod(AccessTools.Method(typeof(ModEntry), nameof(ModEntry.Toolbar_draw_prefix)))
            );
            harmony.Patch(
                original: AccessTools.Method(typeof(Toolbar), nameof(Toolbar.isWithinBounds)),
                prefix: new HarmonyMethod(AccessTools.Method(typeof(ModEntry), nameof(ModEntry.Toolbar_isWithinBounds_prefix)))
            );
            harmony.Patch(
                original: AccessTools.Constructor(typeof(IClickableMenu), new Type[] { typeof(int), typeof( int), typeof(int), typeof(int), typeof(bool) }),
                postfix: new HarmonyMethod(AccessTools.Method(typeof(ModEntry), nameof(ModEntry.Toolbar_postfix)))
            );
        }

        private void Input_ButtonPressed(object sender, StardewModdingAPI.Events.ButtonPressedEventArgs e)
        {
            if (e.Button == Config.RotateKey)
            {
                foreach (var menu in Game1.onScreenMenus)
                {
                    if (menu is not Toolbar)
                        continue;
                    if (menu.isWithinBounds(Game1.getMouseX(), Game1.getMouseY()))
                    {
                        Monitor.Log($"Switching orientation to {(Config.Vertical ? "horizontal" : "vertical")}");
                        Config.Vertical = !Config.Vertical;
                        Helper.WriteConfig(Config);
                        Game1.playSound("dwop");
                    }
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
                getValue: () => Config.EnableMod,
                setValue: value => Config.EnableMod = value
            );

            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Show With Menu",
                tooltip: () => "Show toolbar while menu is open",
                getValue: () => Config.ShowWithActiveMenu,
                setValue: value => Config.ShowWithActiveMenu = value
            );

            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Vertical Toolbar",
                getValue: () => Config.Vertical,
                setValue: value => Config.Vertical = value
            );
            configMenu.AddKeybind(
                mod: ModManifest,
                name: () => "Rotate Key",
                tooltip: () => "Hover over the toolbar and press this key to rotate",
                getValue: () => Config.RotateKey,
                setValue: value => Config.RotateKey = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Pin Toolbar",
                tooltip: () => "Don't swap sides when player moves",
                getValue: () => Game1.options.pinToolbarToggle,
                setValue: value => Game1.options.pinToolbarToggle = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Set Position",
                tooltip: () => "Don't allow toolbar to move using Advanced Menu Positioning",
                getValue: () => Config.SetPosition,
                setValue: value => Config.SetPosition = value
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Set Margin X",
                getValue: () => Config.MarginX,
                setValue: value => Config.MarginX = value
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Set Margin Y",
                getValue: () => Config.MarginY,
                setValue: value => Config.MarginY = value
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Set Offset X",
                getValue: () => Config.OffsetX,
                setValue: value => Config.OffsetX = value
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Set Offset Y",
                getValue: () => Config.OffsetY,
                setValue: value => Config.OffsetY = value
            );
            configMenu.AddTextOption(
                mod: ModManifest,
                name: () => "Pinned Position",
                tooltip: () => "When pinned is set, which of the four positions to keep the toolbar in",
                allowedValues: new string[] { "bottom", "top", "left", "right" },
                getValue: () => Config.PinnedPosition,
                setValue: value => Config.PinnedPosition = value
            );
        }
   }
}