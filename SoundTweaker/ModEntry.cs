using HarmonyLib;
using Microsoft.Xna.Framework.Audio;
using StardewModdingAPI;
using StardewValley;
using System.Collections.Generic;

namespace SoundTweaker
{
    public partial class ModEntry : Mod
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;

        public static ModEntry context;
        
        public static string dictPath = "aedenthorn.SoundTweaker/dictionary";
        public static Dictionary<string, TweakData> tweakDict = new Dictionary<string, TweakData>();
        public static Dictionary<string, XactSoundBankSound[]> originalSounds = new Dictionary<string, XactSoundBankSound[]>();

        public override void Entry(IModHelper helper)
        {
            Config = Helper.ReadConfig<ModConfig>();

            context = this;

            SMonitor = Monitor;
            SHelper = helper;

            Helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            Helper.Events.GameLoop.DayStarted += GameLoop_DayStarted;
            Helper.Events.Player.Warped += Player_Warped;
            Helper.Events.Content.AssetRequested += Content_AssetRequested;
            //Helper.Events.Input.ButtonPressed += Input_ButtonPressed;

            var harmony = new Harmony(ModManifest.UniqueID);
            //harmony.PatchAll();
            
            var data = new TweakData()
            {
                sounds = new List<SoundInfo> { new SoundInfo() { filePaths = { "aedenthorn.TestSound/guitar", "aedenthorn.TestSound/drum" }, cuePaths = { "toyPiano", "flute" }, soundIndexes = { 257, 258, 259 }, minVolume = 0.75f, maxVolume = 1f, minPitch = 1f, maxPitch = 2f, reverb = false, minFrequency = 0, maxFrequency = 1000, minQ = 0, maxQ = 500, filterMode = FilterMode.HighPass } }
            };
            //File.WriteAllText("contents.json", JsonConvert.SerializeObject(data, Formatting.Indented));
            
        }

        private void Player_Warped(object sender, StardewModdingAPI.Events.WarpedEventArgs e)
        {
            ReloadSounds();
        }

        private void GameLoop_DayStarted(object sender, StardewModdingAPI.Events.DayStartedEventArgs e)
        {
            ReloadSounds();
        }

        private void Input_ButtonPressed(object sender, StardewModdingAPI.Events.ButtonPressedEventArgs e)
        {
            if (e.Button == SButton.Delete)
            {
                Game1.stopMusicTrack(Game1.MusicContext.Default);
                ReloadSounds();

                Game1.playSound("snowyStep");
            }
        }


        private void Content_AssetRequested(object sender, StardewModdingAPI.Events.AssetRequestedEventArgs e)
        {
            if (!Config.ModEnabled)
                return;
            if (e.NameWithoutLocale.IsEquivalentTo(dictPath))
            {
                e.LoadFrom(() => new Dictionary<string, TweakData>(), StardewModdingAPI.Events.AssetLoadPriority.Exclusive);
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
                getValue: () => Config.ModEnabled,
                setValue: value => Config.ModEnabled = value
            );
        }
    }
}