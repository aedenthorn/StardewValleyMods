using HarmonyLib;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Events;
using System.Globalization;

namespace MeteorDefence
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

            if (!Config.EnableMod)
                return;

            context = this;

            SMonitor = Monitor;
            SHelper = helper;
            helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;


            var harmony = new Harmony(ModManifest.UniqueID);

            harmony.Patch(
               original: AccessTools.Method(typeof(SoundInTheNightEvent), nameof(SoundInTheNightEvent.tickUpdate)),
               prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.SoundInTheNightEvent_tickUpdate_Prefix))
            );
            harmony.Patch(
               original: AccessTools.Method(typeof(SoundInTheNightEvent), nameof(SoundInTheNightEvent.makeChangesToLocation)),
               prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.SoundInTheNightEvent_makeChangesToLocation_Prefix)),
               postfix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.SoundInTheNightEvent_makeChangesToLocation_Postfix))
            );
            harmony.Patch(
               original: AccessTools.Method(typeof(SoundInTheNightEvent), nameof(SoundInTheNightEvent.setUp)),
               prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.SoundInTheNightEvent_setUp_Prefix))
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
                name: () => "Strike Anywhere?",
                getValue: () => Config.StrikeAnywhere,
                setValue: value => Config.StrikeAnywhere = value
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Minimum Meteorites",
                getValue: () => Config.MinMeteorites,
                setValue: value => Config.MinMeteorites = value
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Maximum Meteorites",
                getValue: () => Config.MaxMeteorites,
                setValue: value => Config.MaxMeteorites = value
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Meteors / Object",
                tooltip: () => "Set to -1 to let one object destroy all meteors",
                getValue: () => Config.MeteorsPerObject,
                setValue: value => Config.MeteorsPerObject = value
            );
            configMenu.AddTextOption(
                mod: ModManifest,
                name: () => "Defence Object",
                tooltip: () => "index or name",
                getValue: () => Config.DefenceObject,
                setValue: value => Config.DefenceObject = value
            );
            configMenu.AddTextOption(
                mod: ModManifest,
                name: () => "Defence Sound",
                getValue: () => Config.DefenceSound,
                setValue: value => Config.DefenceSound = value
            );
            configMenu.AddTextOption(
                mod: ModManifest,
                name: () => "Explosion Sound",
                getValue: () => Config.ExplodeSound,
                setValue: value => Config.ExplodeSound = value
            );
        }
    }

}