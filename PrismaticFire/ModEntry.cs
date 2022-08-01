using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace PrismaticFire
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry : Mod
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;

        public static ModEntry context;
        public static string modKey = "aedenthorn.PrismaticFire";
        public static string dictKey = "aedenthorn.PrismaticFire/dictionary";
        public static Texture2D cursorsSheet;

        public static Dictionary<string, ColorData> colorDict = new Dictionary<string, ColorData>();
        public static Dictionary<string, ColorData> builtInColorDict = new Dictionary<string, ColorData>()
        {
            { "Prismatic Shard", new ColorData(Color.Red, Color.Blue) },
            { "Amethyst", new ColorData(Color.Purple) },
            { "Ruby", new ColorData(Color.Red) },
            { "Emerald", new ColorData(Color.Green) },
            { "Diamond", new ColorData(Color.PaleTurquoise) },
            { "Aquamarine", new ColorData(Color.LightBlue) },
            { "Topaz", new ColorData(Color.Yellow) }
        };

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
            helper.Events.GameLoop.SaveLoaded += GameLoop_SaveLoaded;

            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();

            helper.Events.Content.AssetRequested += Content_AssetRequested;

        }

        private void Content_AssetRequested(object sender, AssetRequestedEventArgs e)
        {
            if (e.NameWithoutLocale.IsEquivalentTo(dictKey))
            {
                e.LoadFrom(static () => new Dictionary<string, ColorData>(builtInColorDict), AssetLoadPriority.Exclusive);
            }
        }

        private void GameLoop_SaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            cursorsSheet = new Texture2D(Game1.graphics.GraphicsDevice, Game1.mouseCursors.Width, Game1.mouseCursors.Height);
            Color[] data = new Color[cursorsSheet.Width * cursorsSheet.Height];
            Game1.mouseCursors.GetData(data);
            for(int i = 0; i < data.Length; i++)
            {
                //SMonitor.Log($"{data[i]}");
                byte c = Convert.ToByte((data[i].R + data[i].G + data[i].B) / 3f);
                data[i] = new Color(c, c, c, data[i].A);
            }
            cursorsSheet.SetData(data);

            colorDict = Game1.content.Load<Dictionary<string, ColorData>>(dictKey);
            if(colorDict.Count > 0)
            {
                Monitor.Log($"Loaded {colorDict.Count} custom colors");
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
            
            configMenu.OnFieldChanged(
                mod: ModManifest,
                onChange: delegate(string id, object value)
                {
                    Helper.WriteConfig(Config);
                }
            );

            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Mod Enabled?",
                getValue: () => Config.ModEnabled,
                setValue: value => Config.ModEnabled = value
            );
            configMenu.AddTextOption(
                mod: ModManifest,
                name: () => "Prismatic Speed",
                getValue: () => Config.PrismaticSpeed.ToString(),
                setValue: delegate(string value) { try { Config.PrismaticSpeed = float.Parse(value, System.Globalization.NumberStyles.Any, CultureInfo.InvariantCulture); } catch { } } 
            );
            configMenu.AddTextOption(
                mod: ModManifest,
                name: () => "Drop In Sound",
                getValue: () => Config.TriggerSound,
                setValue: value => Config.TriggerSound = value
            );
            configMenu.AddSectionTitle(
                mod: ModManifest,
                text: () => "Colors"
            );
            configMenu.AddComplexOption(
                mod: ModManifest,
                name: () => "Amethyst Color",
                beforeSave: DoNothing,
                draw: DrawAmethystText
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "   Red",
                getValue: () => Config.AmethystColor.R,
                setValue: value => Config.AmethystColor = new Color(value, Config.AmethystColor.G, Config.AmethystColor.B),
                min: 0,
                max: 255
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "   Green",
                getValue: () => Config.AmethystColor.G,
                setValue: value => Config.AmethystColor = new Color(Config.AmethystColor.R, value, Config.AmethystColor.B),
                min: 0,
                max: 255
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "   Blue",
                getValue: () => Config.AmethystColor.B,
                setValue: value => Config.AmethystColor = new Color(Config.AmethystColor.R, Config.AmethystColor.G, value),
                min: 0,
                max: 255
            );
            configMenu.AddComplexOption(
                mod: ModManifest,
                name: () => "Aquamarine Color",
                beforeSave: DoNothing,
                draw: DrawAquamarineText
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "   Red",
                getValue: () => Config.AquamarineColor.R,
                setValue: value => Config.AquamarineColor = new Color(value, Config.AquamarineColor.G, Config.AquamarineColor.B),
                min: 0,
                max: 255
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "   Green",
                getValue: () => Config.AquamarineColor.G,
                setValue: value => Config.AquamarineColor = new Color(Config.AquamarineColor.R, value, Config.AquamarineColor.B),
                min: 0,
                max: 255
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "   Blue",
                getValue: () => Config.AquamarineColor.B,
                setValue: value => Config.AquamarineColor = new Color(Config.AquamarineColor.R, Config.AquamarineColor.G, value),
                min: 0,
                max: 255
            );
            configMenu.AddComplexOption(
                mod: ModManifest,
                name: () => "Emerald Color",
                beforeSave: DoNothing,
                draw: DrawEmeraldText
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "   Red",
                getValue: () => Config.EmeraldColor.R,
                setValue: value => Config.EmeraldColor = new Color(value, Config.EmeraldColor.G, Config.EmeraldColor.B),
                min: 0,
                max: 255
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "   Green",
                getValue: () => Config.EmeraldColor.G,
                setValue: value => Config.EmeraldColor = new Color(Config.EmeraldColor.R, value, Config.EmeraldColor.B),
                min: 0,
                max: 255
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "   Blue",
                getValue: () => Config.EmeraldColor.B,
                setValue: value => Config.EmeraldColor = new Color(Config.EmeraldColor.R, Config.EmeraldColor.G, value),
                min: 0,
                max: 255
            );
            configMenu.AddComplexOption(
                mod: ModManifest,
                name: () => "Ruby Color",
                beforeSave: DoNothing,
                draw: DrawRubyText
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "   Red",
                getValue: () => Config.RubyColor.R,
                setValue: value => Config.RubyColor = new Color(value, Config.RubyColor.G, Config.RubyColor.B),
                min: 0,
                max: 255
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "   Green",
                getValue: () => Config.RubyColor.G,
                setValue: value => Config.RubyColor = new Color(Config.RubyColor.R, value, Config.RubyColor.B),
                min: 0,
                max: 255
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "   Blue",
                getValue: () => Config.RubyColor.B,
                setValue: value => Config.RubyColor = new Color(Config.RubyColor.R, Config.RubyColor.G, value),
                min: 0,
                max: 255
            );
            configMenu.AddComplexOption(
                mod: ModManifest,
                name: () => "Topaz Color",
                beforeSave: DoNothing,
                draw: DrawTopazText
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "   Red",
                getValue: () => Config.TopazColor.R,
                setValue: value => Config.TopazColor = new Color(value, Config.TopazColor.G, Config.TopazColor.B),
                min: 0,
                max: 255
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "   Green",
                getValue: () => Config.TopazColor.G,
                setValue: value => Config.TopazColor = new Color(Config.TopazColor.R, value, Config.TopazColor.B),
                min: 0,
                max: 255
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "   Blue",
                getValue: () => Config.TopazColor.B,
                setValue: value => Config.TopazColor = new Color(Config.TopazColor.R, Config.TopazColor.G, value),
                min: 0,
                max: 255
            );
            configMenu.AddComplexOption(
                mod: ModManifest,
                name: () => "Diamond Color",
                beforeSave: DoNothing,
                draw: DrawDiamondText
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "   Red",
                getValue: () => Config.DiamondColor.R,
                setValue: value => Config.DiamondColor = new Color(value, Config.DiamondColor.G, Config.DiamondColor.B),
                min: 0,
                max: 255
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "   Green",
                getValue: () => Config.DiamondColor.G,
                setValue: value => Config.DiamondColor = new Color(Config.DiamondColor.R, value, Config.DiamondColor.B),
                min: 0,
                max: 255
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "   Blue",
                getValue: () => Config.DiamondColor.B,
                setValue: value => Config.DiamondColor = new Color(Config.DiamondColor.R, Config.DiamondColor.G, value),
                min: 0,
                max: 255
            );

        }
        private void DoNothing()
        {
        }

        private void DrawAmethystText(SpriteBatch b, Vector2 pos)
        {
            b.DrawString(Game1.dialogueFont, "Amethyst", pos, Config.AmethystColor);

        }
        private void DrawAquamarineText(SpriteBatch b, Vector2 pos)
        {
            b.DrawString(Game1.dialogueFont, "Aquamarine", pos, Config.AquamarineColor);

        }
        private void DrawEmeraldText(SpriteBatch b, Vector2 pos)
        {
            b.DrawString(Game1.dialogueFont, "Emerald", pos, Config.EmeraldColor);

        }
        private void DrawRubyText(SpriteBatch b, Vector2 pos)
        {
            b.DrawString(Game1.dialogueFont, "Ruby", pos, Config.RubyColor);

        }
        private void DrawTopazText(SpriteBatch b, Vector2 pos)
        {
            b.DrawString(Game1.dialogueFont, "Topaz", pos, Config.TopazColor);

        }
        private void DrawDiamondText(SpriteBatch b, Vector2 pos)
        {
            b.DrawString(Game1.dialogueFont, "Diamond", pos, Config.DiamondColor);

        }
        private static Color GetColor(string which)
        {
            Color color = Color.White;
            if (colorDict.TryGetValue(which, out ColorData data))
            {
                if (data.speed > 0)
                {
                    color = Utility.Get2PhaseColor(data.startColor, data.endColor, 0, data.speed);
                }
                else
                {
                    color = data.startColor;
                }
                return color;
            }
            switch (which)
            {
                case "Prismatic Shard":
                    color = Utility.GetPrismaticColor(0, Config.PrismaticSpeed);
                    break;
                case "Amethyst":
                    color = Config.AmethystColor;
                    break;
                case "Aquamarine":
                    color = Config.AquamarineColor;
                    break;
                case "Ruby":
                    color = Config.RubyColor;
                    break;
                case "Emerald":
                    color = Config.EmeraldColor;
                    break;
                case "Topaz":
                    color = Config.TopazColor;
                    break;
            }
            return color;
        }
    }
}