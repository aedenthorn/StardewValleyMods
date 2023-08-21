using Force.DeepCloner;
using HarmonyLib;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley;
using System.Collections.Generic;

namespace BuffFramework
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry : Mod
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;

        public static ModEntry context;
        public static string dictKey = "aedenthorn.BuffFramework/dictionary";
        public static Dictionary<string, Dictionary<string, object>> buffDict = new();
        public static PerScreen<Dictionary<string, Buff>> farmerBuffs = new();
        public static Dictionary<string, ICue> cues = new();

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            Config = Helper.ReadConfig<ModConfig>();

            context = this;

            SMonitor = Monitor;
            SHelper = helper;

            Helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            Helper.Events.GameLoop.DayStarted += GameLoop_DayStarted;
            Helper.Events.Player.Warped += Player_Warped;
            Helper.Events.GameLoop.TimeChanged += GameLoop_TimeChanged;
            Helper.Events.GameLoop.SaveLoaded += GameLoop_SaveLoaded;
            Helper.Events.Content.AssetRequested += Content_AssetRequested;
            Helper.Events.GameLoop.ReturnedToTitle += GameLoop_ReturnedToTitle;

            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();
        }

        private void GameLoop_ReturnedToTitle(object sender, StardewModdingAPI.Events.ReturnedToTitleEventArgs e)
        {
            foreach(var cue in cues)
            {
                if(cue.Value.IsPlaying)
                {
                    cue.Value.Stop(Microsoft.Xna.Framework.Audio.AudioStopOptions.Immediate);
                }
            }
            cues.Clear();
        }

        private void GameLoop_TimeChanged(object sender, StardewModdingAPI.Events.TimeChangedEventArgs e)
        {
            Helper.Events.GameLoop.UpdateTicking += GameLoop_UpdateTicking;
        }

        private void Player_Warped(object sender, StardewModdingAPI.Events.WarpedEventArgs e)
        {
            Helper.Events.GameLoop.UpdateTicking += GameLoop_UpdateTicking;
        }
        private void GameLoop_DayStarted(object sender, StardewModdingAPI.Events.DayStartedEventArgs e)
        {
            Helper.Events.GameLoop.UpdateTicking += GameLoop_UpdateTicking;
        }

        private void GameLoop_UpdateTicking(object sender, StardewModdingAPI.Events.UpdateTickingEventArgs e)
        {
            UpdateBuffs();
            Helper.Events.GameLoop.UpdateTicking -= GameLoop_UpdateTicking;
        }

        private void Content_AssetRequested(object sender, StardewModdingAPI.Events.AssetRequestedEventArgs e)
        {
            if (e.NameWithoutLocale.IsEquivalentTo(dictKey))
            {
                e.LoadFrom(() => new Dictionary<string, Dictionary<string, object>>(), StardewModdingAPI.Events.AssetLoadPriority.Exclusive);
            }
        }

        private void GameLoop_SaveLoaded(object sender, StardewModdingAPI.Events.SaveLoadedEventArgs e)
        {
            farmerBuffs.Value = new();
        }

        private void GameLoop_GameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
        {

            var cpAPI = Helper.ModRegistry.GetApi<IContentPatcherAPI>("Pathoschild.ContentPatcher");
            if (cpAPI is not null)
            {
                cpAPI.RegisterToken(ModManifest, "PlayerShirt", () =>
                {
                    // save is loaded
                    if (Context.IsWorldReady)
                        return new[] { Game1.player.shirtItem.Value.Name };

                    // or save is currently loading
                    if (SaveGame.loaded?.player != null)
                        return new[] { SaveGame.loaded.player.shirtItem.Value.Name };

                    // no save loaded (e.g. on the title screen)
                    return null;
                });
                cpAPI.RegisterToken(ModManifest, "PlayerPants", () =>
                {
                    // save is loaded
                    if (Context.IsWorldReady)
                        return new[] { Game1.player.pantsItem.Value.Name};

                    // or save is currently loading
                    if (SaveGame.loaded?.player != null)
                        return new[] { SaveGame.loaded.player.pantsItem.Value.Name };

                    // no save loaded (e.g. on the title screen)
                    return null;
                });
                cpAPI.RegisterToken(ModManifest, "PlayerHat", () =>
                {
                    // save is loaded
                    if (Context.IsWorldReady)
                        return new[] { Game1.player.hat.Value.Name };

                    // or save is currently loading
                    if (SaveGame.loaded?.player != null)
                        return new[] { SaveGame.loaded.player.hat.Value.Name };

                    // no save loaded (e.g. on the title screen)
                    return null;
                });
                cpAPI.RegisterToken(ModManifest, "PlayerBoots", () =>
                {
                    // save is loaded
                    if (Context.IsWorldReady)
                        return new[] { Game1.player.boots.Value.Name };

                    // or save is currently loading
                    if (SaveGame.loaded?.player != null)
                        return new[] { SaveGame.loaded.player.boots.Value.Name };

                    // no save loaded (e.g. on the title screen)
                    return null;
                });
                cpAPI.RegisterToken(ModManifest, "PlayerRings", () =>
                {
                    // save is loaded
                    if (Context.IsWorldReady)
                        return new[] { Game1.player.leftRing.Value.Name, Game1.player.rightRing.Value.Name };

                    // or save is currently loading
                    if (SaveGame.loaded?.player != null)
                        return new[] { SaveGame.loaded.player.leftRing.Value.Name, SaveGame.loaded.player.rightRing.Value.Name };

                    // no save loaded (e.g. on the title screen)
                    return null;
                });
            }

            // get Generic Mod Config Menu's API (if it's installed)
            var configMenu = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (configMenu is not null)
            {
                // register mod
                configMenu.Register(
                    mod: ModManifest,
                    reset: () => Config = new ModConfig(),
                    save: () => Helper.WriteConfig(Config)
                );

                configMenu.AddBoolOption(
                    mod: ModManifest,
                    name: () => SHelper.Translation.Get("GMCM_Option_ModEnabled_Name"),
                    getValue: () => Config.ModEnabled,
                    setValue: value => Config.ModEnabled = value
                );
            }
        }
    }
}