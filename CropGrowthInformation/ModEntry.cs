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
                name: () => ModEntry.SHelper.Translation.Get("GMCM_Option_ModEnabled_Name"),
                getValue: () => Config.EnableMod,
                setValue: value => Config.EnableMod = value
            );
            configMenu.AddSectionTitle(
                mod: ModManifest,
                text: () => ModEntry.SHelper.Translation.Get("GMCM_Title_Keys_Text")
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => ModEntry.SHelper.Translation.Get("GMCM_Option_RequireToggle_Name"),
                getValue: () => Config.RequireToggle,
                setValue: value => Config.RequireToggle = value
            );
            configMenu.AddKeybind(
                mod: ModManifest,
                name: () => ModEntry.SHelper.Translation.Get("GMCM_Option_ToggleButton_Name"),
                getValue: () => Config.ToggleButton,
                setValue: value => Config.ToggleButton = value
            );
            configMenu.AddSectionTitle(
                mod: ModManifest,
                text: () => ModEntry.SHelper.Translation.Get("GMCM_Title_Information_Text")
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => ModEntry.SHelper.Translation.Get("GMCM_Option_ShowCropName_Name"),
                getValue: () => Config.ShowCropName,
                setValue: value => Config.ShowCropName = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => ModEntry.SHelper.Translation.Get("GMCM_Option_ShowCurrentPhase_Name"),
                getValue: () => Config.ShowCurrentPhase,
                setValue: value => Config.ShowCurrentPhase = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => ModEntry.SHelper.Translation.Get("GMCM_Option_ShowDaysInCurrentPhase_Name"),
                getValue: () => Config.ShowDaysInCurrentPhase,
                setValue: value => Config.ShowDaysInCurrentPhase = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => ModEntry.SHelper.Translation.Get("GMCM_Option_ShowTotalGrowth_Name"),
                getValue: () => Config.ShowTotalGrowth,
                setValue: value => Config.ShowTotalGrowth = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => ModEntry.SHelper.Translation.Get("GMCM_Option_ShowReadyText_Name"),
                getValue: () => Config.ShowReadyText,
                setValue: value => Config.ShowReadyText = value
            );
            configMenu.AddSectionTitle(
                mod: ModManifest,
                text: () => ModEntry.SHelper.Translation.Get("GMCM_Title_Display_Text")
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => ModEntry.SHelper.Translation.Get("GMCM_Option_TextScale_Name"),
                getValue: () => Config.TextScale,
                setValue: value => Config.TextScale = value,
                min: 0.5f,
                max: 4.0f,
                interval: 0.1f
            );
            configMenu.AddComplexOption(
                mod: ModManifest,
                name: () => ModEntry.SHelper.Translation.Get("GMCM_Option_CropNameColor_Name"),
                beforeSave: DoNothing,
                draw: DrawNameText
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "   " + ModEntry.SHelper.Translation.Get("GMCM_Option_Red_Name"),
                getValue: () => Config.NameColor.R,
                setValue: value => Config.NameColor = new Color(value, Config.NameColor.G, Config.NameColor.B),
                min:0,
                max:255
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "   " + ModEntry.SHelper.Translation.Get("GMCM_Option_Green_Name"),
                getValue: () => Config.NameColor.G,
                setValue: value => Config.NameColor = new Color(Config.NameColor.R, value, Config.NameColor.B),
                min: 0,
                max: 255
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "   " + ModEntry.SHelper.Translation.Get("GMCM_Option_Blue_Name"),
                getValue: () => Config.NameColor.B,
                setValue: value => Config.NameColor = new Color(Config.NameColor.R, Config.NameColor.G, value),
                min: 0,
                max: 255
            );
            configMenu.AddComplexOption(
                mod: ModManifest,
                name: () => ModEntry.SHelper.Translation.Get("GMCM_Option_CurrentPhaseColor_Name"),
                beforeSave: DoNothing,
                draw: DrawPhaseText
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "   " + ModEntry.SHelper.Translation.Get("GMCM_Option_Red_Name"),
                getValue: () => Config.CurrentPhaseColor.R,
                setValue: value => Config.CurrentPhaseColor = new Color(value, Config.CurrentPhaseColor.G, Config.CurrentPhaseColor.B),
                min:0,
                max:255
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "   " + ModEntry.SHelper.Translation.Get("GMCM_Option_Green_Name"),
                getValue: () => Config.CurrentPhaseColor.G,
                setValue: value => Config.CurrentPhaseColor = new Color(Config.CurrentPhaseColor.R, value, Config.CurrentPhaseColor.B),
                min: 0,
                max: 255
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "   " + ModEntry.SHelper.Translation.Get("GMCM_Option_Blue_Name"),
                getValue: () => Config.CurrentPhaseColor.B,
                setValue: value => Config.CurrentPhaseColor = new Color(Config.CurrentPhaseColor.R, Config.CurrentPhaseColor.G, value),
                min: 0,
                max: 255
            );
            configMenu.AddComplexOption(
                mod: ModManifest,
                name: () => ModEntry.SHelper.Translation.Get("GMCM_Option_DaysInCurrentPhaseColor_Name"),
                beforeSave: DoNothing,
                draw: DrawPhaseDaysText
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "   " + ModEntry.SHelper.Translation.Get("GMCM_Option_Red_Name"),
                getValue: () => Config.CurrentGrowthColor.R,
                setValue: value => Config.CurrentGrowthColor = new Color(value, Config.CurrentGrowthColor.G, Config.CurrentGrowthColor.B),
                min:0,
                max:255
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "   " + ModEntry.SHelper.Translation.Get("GMCM_Option_Green_Name"),
                getValue: () => Config.CurrentGrowthColor.G,
                setValue: value => Config.CurrentGrowthColor = new Color(Config.CurrentGrowthColor.R, value, Config.CurrentGrowthColor.B),
                min: 0,
                max: 255
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "   " + ModEntry.SHelper.Translation.Get("GMCM_Option_Blue_Name"),
                getValue: () => Config.CurrentGrowthColor.B,
                setValue: value => Config.CurrentGrowthColor = new Color(Config.CurrentGrowthColor.R, Config.CurrentGrowthColor.G, value),
                min: 0,
                max: 255
            );
            configMenu.AddComplexOption(
                mod: ModManifest,
                name: () => ModEntry.SHelper.Translation.Get("GMCM_Option_TotalGrowthColor_Name"),
                beforeSave: DoNothing,
                draw: DrawTotalDaysText
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "   " + ModEntry.SHelper.Translation.Get("GMCM_Option_Red_Name"),
                getValue: () => Config.TotalGrowthColor.R,
                setValue: value => Config.TotalGrowthColor = new Color(value, Config.TotalGrowthColor.G, Config.TotalGrowthColor.B),
                min:0,
                max:255
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "   " + ModEntry.SHelper.Translation.Get("GMCM_Option_Green_Name"),
                getValue: () => Config.TotalGrowthColor.G,
                setValue: value => Config.TotalGrowthColor = new Color(Config.TotalGrowthColor.R, value, Config.TotalGrowthColor.B),
                min: 0,
                max: 255
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "   " + ModEntry.SHelper.Translation.Get("GMCM_Option_Blue_Name"),
                getValue: () => Config.TotalGrowthColor.B,
                setValue: value => Config.TotalGrowthColor = new Color(Config.TotalGrowthColor.R, Config.TotalGrowthColor.G, value),
                min: 0,
                max: 255
            );
            configMenu.AddComplexOption(
                mod: ModManifest,
                name: () => ModEntry.SHelper.Translation.Get("GMCM_Option_ReadyTextColor_Name"),
                beforeSave: DoNothing,
                draw: DrawReadyText
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "   " + ModEntry.SHelper.Translation.Get("GMCM_Option_Red_Name"),
                getValue: () => Config.ReadyColor.R,
                setValue: value => Config.ReadyColor = new Color(value, Config.ReadyColor.G, Config.ReadyColor.B),
                min:0,
                max:255
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "   " + ModEntry.SHelper.Translation.Get("GMCM_Option_Green_Name"),
                getValue: () => Config.ReadyColor.G,
                setValue: value => Config.ReadyColor = new Color(Config.ReadyColor.R, value, Config.ReadyColor.B),
                min: 0,
                max: 255
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "   " + ModEntry.SHelper.Translation.Get("GMCM_Option_Blue_Name"),
                getValue: () => Config.ReadyColor.B,
                setValue: value => Config.ReadyColor = new Color(Config.ReadyColor.R, Config.ReadyColor.G, value),
                min: 0,
                max: 255
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => ModEntry.SHelper.Translation.Get("GMCM_Option_CropOpacity_Name"),
                getValue: () => Config.CropOpacity,
                setValue: value => Config.CropOpacity = value,
                min: 0.0f,
                max: 1.0f,
                interval: 0.01f
            );
        }

        private void DoNothing()
        {
        }

        private void DrawNameText(SpriteBatch b, Vector2 pos)
        {
            b.DrawString(Game1.dialogueFont, SHelper.Translation.Get("name"), pos, Config.NameColor);

        }
        private void DrawPhaseText(SpriteBatch b, Vector2 pos)
        {
            b.DrawString(Game1.dialogueFont, string.Format(SHelper.Translation.Get("phase"), "x", "y"), pos, Config.CurrentPhaseColor);

        }
        private void DrawPhaseDaysText(SpriteBatch b, Vector2 pos)
        {
            b.DrawString(Game1.dialogueFont, string.Format(SHelper.Translation.Get("current"), "x", "y"), pos, Config.CurrentGrowthColor);

        }
        private void DrawTotalDaysText(SpriteBatch b, Vector2 pos)
        {
            b.DrawString(Game1.dialogueFont, string.Format(SHelper.Translation.Get("total"), "x", "y"), pos, Config.TotalGrowthColor);

        }
        private void DrawReadyText(SpriteBatch b, Vector2 pos)
        {
            b.DrawString(Game1.dialogueFont, SHelper.Translation.Get("ready"), pos, Config.ReadyColor);

        }
    }
}