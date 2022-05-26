using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using System.Globalization;

namespace CropGrowthInformation
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
            
            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();
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
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Require Toggle",
                getValue: () => Config.RequireToggle,
                setValue: value => Config.RequireToggle = value
            );
            configMenu.AddKeybind(
                mod: ModManifest,
                name: () => "Toggle Key",
                getValue: () => Config.ToggleButton,
                setValue: value => Config.ToggleButton = value
            );
            configMenu.AddTextOption(
                mod: ModManifest,
                name: () => "Text Scale",
                getValue: () => Config.TextScale+"",
                setValue: delegate (string value) { try { Config.TextScale = float.Parse(value, CultureInfo.InvariantCulture); } catch { } }
            );
            configMenu.AddTextOption(
                mod: ModManifest,
                name: () => "Crop Transparency",
                getValue: () => Config.CropTransparency+"",
                setValue: delegate (string value) { try { Config.CropTransparency = float.Parse(value, CultureInfo.InvariantCulture); } catch { } }
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Show Crop Name",
                getValue: () => Config.ShowCropName,
                setValue: value => Config.ShowCropName = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Show Ready Text",
                getValue: () => Config.ShowReadyText,
                setValue: value => Config.ShowReadyText = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Show Current Phase",
                getValue: () => Config.ShowCurrentPhase,
                setValue: value => Config.ShowCurrentPhase = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Show Phase Day",
                getValue: () => Config.ShowDaysInCurrentPhase,
                setValue: value => Config.ShowDaysInCurrentPhase = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Show Total Days",
                getValue: () => Config.ShowTotalGrowth,
                setValue: value => Config.ShowTotalGrowth = value
            );
            configMenu.AddTextOption(
                mod: ModManifest,
                name: () => "Ready Text",
                getValue: () => Config.ReadyText,
                setValue: value => Config.ReadyText = value
            );
            configMenu.AddTextOption(
                mod: ModManifest,
                name: () => "Phase Text",
                getValue: () => Config.PhaseText,
                setValue: value => Config.PhaseText = value
            );
            configMenu.AddTextOption(
                mod: ModManifest,
                name: () => "Days Text",
                getValue: () => Config.CurrentText,
                setValue: value => Config.CurrentText = value
            );
            configMenu.AddTextOption(
                mod: ModManifest,
                name: () => "Growth Text",
                getValue: () => Config.TotalText,
                setValue: value => Config.TotalText = value
            );
            configMenu.AddComplexOption(
                mod: ModManifest,
                name: () => "Name Color",
                beforeSave: DoNothing,
                draw: DrawNameText
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "   Red",
                getValue: () => Config.NameColor.R,
                setValue: value => Config.NameColor = new Color(value, Config.NameColor.G, Config.NameColor.B),
                min:0,
                max:255
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "   Green",
                getValue: () => Config.NameColor.G,
                setValue: value => Config.NameColor = new Color(Config.NameColor.R, value, Config.NameColor.B),
                min: 0,
                max: 255
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "   Blue",
                getValue: () => Config.NameColor.B,
                setValue: value => Config.NameColor = new Color(Config.NameColor.R, Config.NameColor.G, value),
                min: 0,
                max: 255
            );
            configMenu.AddComplexOption(
                mod: ModManifest,
                name: () => "Phase Color",
                beforeSave: DoNothing,
                draw: DrawPhaseText
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "   Red",
                getValue: () => Config.CurrentPhaseColor.R,
                setValue: value => Config.CurrentPhaseColor = new Color(value, Config.CurrentPhaseColor.G, Config.CurrentPhaseColor.B),
                min:0,
                max:255
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "   Green",
                getValue: () => Config.CurrentPhaseColor.G,
                setValue: value => Config.CurrentPhaseColor = new Color(Config.CurrentPhaseColor.R, value, Config.CurrentPhaseColor.B),
                min: 0,
                max: 255
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "   Blue",
                getValue: () => Config.CurrentPhaseColor.B,
                setValue: value => Config.CurrentPhaseColor = new Color(Config.CurrentPhaseColor.R, Config.CurrentPhaseColor.G, value),
                min: 0,
                max: 255
            );
            configMenu.AddComplexOption(
                mod: ModManifest,
                name: () => "Phase Days Color",
                beforeSave: DoNothing,
                draw: DrawPhaseDaysText
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "   Red",
                getValue: () => Config.CurrentGrowthColor.R,
                setValue: value => Config.CurrentGrowthColor = new Color(value, Config.CurrentGrowthColor.G, Config.CurrentGrowthColor.B),
                min:0,
                max:255
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "   Green",
                getValue: () => Config.CurrentGrowthColor.G,
                setValue: value => Config.CurrentGrowthColor = new Color(Config.CurrentGrowthColor.R, value, Config.CurrentGrowthColor.B),
                min: 0,
                max: 255
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "   Blue",
                getValue: () => Config.CurrentGrowthColor.B,
                setValue: value => Config.CurrentGrowthColor = new Color(Config.CurrentGrowthColor.R, Config.CurrentGrowthColor.G, value),
                min: 0,
                max: 255
            );
            configMenu.AddComplexOption(
                mod: ModManifest,
                name: () => "Total Days Color",
                beforeSave: DoNothing,
                draw: DrawTotalDaysText
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "   Red",
                getValue: () => Config.TotalGrowthColor.R,
                setValue: value => Config.TotalGrowthColor = new Color(value, Config.TotalGrowthColor.G, Config.TotalGrowthColor.B),
                min:0,
                max:255
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "   Green",
                getValue: () => Config.TotalGrowthColor.G,
                setValue: value => Config.TotalGrowthColor = new Color(Config.TotalGrowthColor.R, value, Config.TotalGrowthColor.B),
                min: 0,
                max: 255
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "   Blue",
                getValue: () => Config.TotalGrowthColor.B,
                setValue: value => Config.TotalGrowthColor = new Color(Config.TotalGrowthColor.R, Config.TotalGrowthColor.G, value),
                min: 0,
                max: 255
            );
            configMenu.AddComplexOption(
                mod: ModManifest,
                name: () => "Ready Color",
                beforeSave: DoNothing,
                draw: DrawReadyText
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "   Red",
                getValue: () => Config.ReadyColor.R,
                setValue: value => Config.ReadyColor = new Color(value, Config.ReadyColor.G, Config.ReadyColor.B),
                min:0,
                max:255
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "   Green",
                getValue: () => Config.ReadyColor.G,
                setValue: value => Config.ReadyColor = new Color(Config.ReadyColor.R, value, Config.ReadyColor.B),
                min: 0,
                max: 255
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "   Blue",
                getValue: () => Config.ReadyColor.B,
                setValue: value => Config.ReadyColor = new Color(Config.ReadyColor.R, Config.ReadyColor.G, value),
                min: 0,
                max: 255
            );
        }

        private void DoNothing()
        {
        }

        private void DrawNameText(SpriteBatch b, Vector2 pos)
        {
            b.DrawString(Game1.dialogueFont, "Crop Name", pos, Config.NameColor);

        }
        private void DrawPhaseText(SpriteBatch b, Vector2 pos)
        {
            b.DrawString(Game1.dialogueFont, string.Format(SHelper.Translation.Get("phase-x-y"), "x", "y"), pos, Config.CurrentPhaseColor);

        }
        private void DrawPhaseDaysText(SpriteBatch b, Vector2 pos)
        {
            b.DrawString(Game1.dialogueFont, string.Format(SHelper.Translation.Get("current-x-y"), "x", "y"), pos, Config.CurrentGrowthColor);

        }
        private void DrawTotalDaysText(SpriteBatch b, Vector2 pos)
        {
            b.DrawString(Game1.dialogueFont, string.Format(SHelper.Translation.Get("total-x-y"), "x", "y"), pos, Config.TotalGrowthColor);

        }
        private void DrawReadyText(SpriteBatch b, Vector2 pos)
        {
            b.DrawString(Game1.dialogueFont, SHelper.Translation.Get("ready"), pos, Config.ReadyColor);

        }
    }
}