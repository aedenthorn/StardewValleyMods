using HarmonyLib;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using StardewValley.TerrainFeatures;

namespace FarmerHelper
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
            helper.Events.GameLoop.DayStarted += GameLoop_DayStarted;

            var harmony = new Harmony(ModManifest.UniqueID);

            harmony.Patch(
               original: AccessTools.Method(typeof(Utility), nameof(Utility.tryToPlaceItem)),
               prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.Utility_tryToPlaceItem_Prefix))
            );
            harmony.Patch(
               original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.createQuestionDialogue), new System.Type[] { typeof(string), typeof(Response[]), typeof(string), typeof(Object) }),
               prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.GameLocation_createQuestionDialogue_Prefix))
            );
            harmony.Patch(
               original: AccessTools.Method(typeof(IClickableMenu), nameof(IClickableMenu.drawToolTip)),
               prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.IClickableMenu_drawToolTip_Prefix))
            );
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
                name: () => "Label late planting?",
                getValue: () => Config.LabelLatePlanting,
                setValue: value => Config.LabelLatePlanting = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Prevent late planting?",
                getValue: () => Config.PreventLatePlant,
                setValue: value => Config.PreventLatePlant = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Warn plants unwatered?",
                getValue: () => Config.WarnAboutPlantsUnwateredBeforeSleep,
                setValue: value => Config.WarnAboutPlantsUnwateredBeforeSleep = value
            );;
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Ignore Flowers?",
                getValue: () => Config.IgnoreFlowers,
                setValue: value => Config.IgnoreFlowers = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Warn plants unharvested?",
                getValue: () => Config.WarnAboutPlantsUnharvestedBeforeSleep,
                setValue: value => Config.WarnAboutPlantsUnharvestedBeforeSleep = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Warn animals outside?",
                getValue: () => Config.WarnAboutAnimalsOutsideBeforeSleep,
                setValue: value => Config.WarnAboutAnimalsOutsideBeforeSleep = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Warn animals hungry?",
                getValue: () => Config.WarnAboutAnimalsHungryBeforeSleep,
                setValue: value => Config.WarnAboutAnimalsHungryBeforeSleep = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Warn animals unharvested?",
                getValue: () => Config.WarnAboutAnimalsUnharvestedBeforeSleep,
                setValue: value => Config.WarnAboutAnimalsUnharvestedBeforeSleep = value
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Days per month",
                getValue: () => Config.DaysPerMonth,
                setValue: value => Config.DaysPerMonth = value
            );
        }
        private void GameLoop_DayStarted(object sender, StardewModdingAPI.Events.DayStartedEventArgs e)
        {
            Helper.GameContent.InvalidateCache("Data/ObjectInformation");
        }

        public static string[] seasons = new string[] { "spring", "summer", "fall", "winter" };
        private static bool EnoughDaysLeft(Crop c, HoeDirt hoeDirt)
        {
            if (c.seasonsToGrowIn.Contains(seasons[(Utility.getSeasonNumber(Game1.currentSeason) + 1) % 4]))
                return true;
            if(hoeDirt is not null)
            {
                HoeDirt d = new HoeDirt(hoeDirt.state.Value, c);
                d.currentLocation = hoeDirt.currentLocation;
                d.currentTileLocation = hoeDirt.currentTileLocation;
                d.fertilizer.Value = hoeDirt.fertilizer.Value;
                AccessTools.Method(typeof(HoeDirt), "applySpeedIncreases").Invoke(d, new object[] { Game1.player });
                c = d.crop;
            }
            int days = 0;
            for (int i = 0; i < c.phaseDays.Count - 1; i++)
            {
                days += c.phaseDays[i];
            }
            return Config.DaysPerMonth - Game1.dayOfMonth >= days;
        }
    }
}