using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using Newtonsoft.Json;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;
using System;
using System.Collections.Generic;
using System.Diagnostics.SymbolStore;
using System.Globalization;
using System.IO;
using System.Linq;
using xTile;
using xTile.Layers;
using xTile.ObjectModel;
using Object = StardewValley.Object;

namespace PetHats
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry : Mod
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;

        public static ModEntry context;
        public static Texture2D colorTexture;

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            Config = Helper.ReadConfig<ModConfig>();

            context = this;

            SMonitor = Monitor;
            SHelper = helper;

            Helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;

            Helper.Events.Display.Rendered += Display_Rendered;

            colorTexture = new Texture2D(Game1.graphics.GraphicsDevice, 1, 1);
            colorTexture.SetData(new Color[] { Color.White });
        }

        private void Display_Rendered(object sender, StardewModdingAPI.Events.RenderedEventArgs e)
        {
            e.SpriteBatch.Draw(colorTexture, new Rectangle(0, 0, Game1.viewport.Width, Game1.viewport.Height), Config.TweakedColor * (Config.Opacity / 100f));
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
                name: () => "Debug",
                getValue: () => Config.Debug,
                setValue: value => Config.Debug = value
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                fieldId: "Red",
                name: () => "Color R",
                getValue: () => Config.TweakedColor.R,
                setValue: value => Config.TweakedColor = new Color(value, Config.TweakedColor.G, Config.TweakedColor.B),
                min: 0,
                max: 255
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                fieldId: "Green",
                name: () => "Color G",
                getValue: () => Config.TweakedColor.G,
                setValue: value => Config.TweakedColor = new Color(Config.TweakedColor.R, value, Config.TweakedColor.B),
                min: 0,
                max: 255
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                fieldId: "Blue",
                name: () => "Color B",
                getValue: () => Config.TweakedColor.B,
                setValue: value => Config.TweakedColor = new Color(Config.TweakedColor.R, Config.TweakedColor.G, value),
                min: 0,
                max: 255
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                fieldId: "Opacity",
                name: () => "Opacity",
                getValue: () => Config.Opacity,
                setValue: value => Config.Opacity = value,
                min: 0,
                max: 90
            );
            configMenu.OnFieldChanged(ModManifest, FieldChanged);
        }

        private void FieldChanged(string id, object value)
        {
            switch (id)
            {
                case "Red":
                    Config.TweakedColor = new Color((int)value, Config.TweakedColor.G, Config.TweakedColor.B);
                    break;
                case "Green":
                    Config.TweakedColor = new Color(Config.TweakedColor.R, (int)value, Config.TweakedColor.B);
                    break;
                case "Blue":
                    Config.TweakedColor = new Color(Config.TweakedColor.R, Config.TweakedColor.G, (int)value);
                    break;
                case "Opacity":
                    Config.Opacity = (int)value;
                    break;
            }
        }
    }
}
