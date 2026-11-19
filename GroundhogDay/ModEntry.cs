using System;
using HarmonyLib;
using StardewModdingAPI;
using StardewValley;

namespace GroundhogDay
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

            helper.Events.Input.ButtonPressed += Input_ButtonPressed;
            helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;

            var harmony = new Harmony(ModManifest.UniqueID);

            // Game1 Patches

            // Patch the public newDayAfterFade(Action) wrapper rather than the private
            // _newDayAfterFade() iterator stub it calls: the stub is only 1-2 IL
            // instructions (it just constructs the compiler-generated state machine),
            // so the JIT inlines it at both call sites, which silently bypasses a
            // Harmony patch on it (the patch "applies" with no error, but never runs).
            // The wrapper has real logic and can't be inlined, and it always runs
            // synchronously before the state machine's day-rollover logic does.
            var newDayAfterFade = AccessTools.Method(typeof(Game1), "newDayAfterFade", new[] { typeof(Action) });
            if (newDayAfterFade is null)
            {
                Monitor.Log("Couldn't find Game1.newDayAfterFade(Action) to patch — this mod may need an update for the current game version.", LogLevel.Error);
            }
            else
            {
                harmony.Patch(
                   original: newDayAfterFade,
                   prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.Game1_newDayAfterFade_Prefix))
                );
            }

            // Mail queued via any "AddMail ... tomorrow" trigger action (vanilla or a content
            // mod like Stardew Valley Expanded) isn't tied to the calendar - it's just a queue
            // that this method drains into the mailbox on every sleep. Hold it back while the
            // mod is enabled so that content doesn't arrive on every repeated day.
            var receiveMailForTomorrow = AccessTools.Method(typeof(Game1), "ReceiveMailForTomorrow", new[] { typeof(string) });
            if (receiveMailForTomorrow is null)
            {
                Monitor.Log("Couldn't find Game1.ReceiveMailForTomorrow(string) to patch — this mod may need an update for the current game version.", LogLevel.Error);
            }
            else
            {
                harmony.Patch(
                   original: receiveMailForTomorrow,
                   prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.Game1_ReceiveMailForTomorrow_Prefix))
                );
            }
        }

        private void Input_ButtonPressed(object sender, StardewModdingAPI.Events.ButtonPressedEventArgs e)
        {
            if(e.Button == Config.ToggleModKey)
            {
                Config.EnableMod = !Config.EnableMod;
                Helper.WriteConfig(Config);
                Monitor.Log($"Mod Enabled: {Config.EnableMod}");
                if (Config.ShowMessage)
                {
                    Game1.addHUDMessage(new HUDMessage(string.Format(Helper.Translation.Get("mod-enabled-x"), Config.EnableMod), 1));
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
                name: () => "Mod Enabled?",
                getValue: () => Config.EnableMod,
                setValue: value => Config.EnableMod = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Show Message?",
                getValue: () => Config.ShowMessage,
                setValue: value => Config.ShowMessage = value
            );
            configMenu.AddKeybind(
                mod: ModManifest,
                name: () => "Toggle Mod Key",
                getValue: () => Config.ToggleModKey,
                setValue: value => Config.ToggleModKey = value
            );
        }

    }

}