using HarmonyLib;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;

namespace PlayerAnimationFramework
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry : Mod, IAssetLoader
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;

        public static ModEntry context;
        public static readonly string dictPath = "player_animation_framework_dictionary";
        private static Dictionary<string, PlayerAnimation> animationDict;

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            Config = Helper.ReadConfig<ModConfig>();

            context = this;

            SMonitor = Monitor;
            SHelper = helper;
            helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            helper.Events.GameLoop.SaveLoaded += GameLoop_SaveLoaded;
            helper.Events.Input.ButtonsChanged += Input_ButtonsChanged;


            var harmony = new Harmony(ModManifest.UniqueID);

            harmony.Patch(
               original: AccessTools.Method(typeof(ChatBox), "runCommand"),
               prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.ChatBox_runCommand_Prefix))
            );

        }

        private void Input_ButtonsChanged(object sender, StardewModdingAPI.Events.ButtonsChangedEventArgs e)
        {
            if (!Config.EnableMod || animationDict == null)
                return;
            foreach(var kvp in animationDict)
            {
                if (kvp.Value.keyTrigger != null && KeybindList.TryParse(kvp.Value.keyTrigger, out KeybindList keybind, out string[] errors) && keybind.JustPressed())
                {
                    PlayAnimation(kvp.Key, kvp.Value);
                }
            }
        }

        private void GameLoop_SaveLoaded(object sender, StardewModdingAPI.Events.SaveLoadedEventArgs e)
        {
            LoadAnimations();
        }

        private static void LoadAnimations()
        {
            animationDict = SHelper.Content.Load<Dictionary<string, PlayerAnimation>>(dictPath, ContentSource.GameContent);
            SMonitor.Log($"Loaded {animationDict.Count} animations");
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
        }

        public static async void PlayAnimation(string id, PlayerAnimation data)
        {
            SMonitor.Log($"Playing animation {id}");
            foreach(PlayerAnimationFrame frame in data.animations)
            {
                Farmer who = Game1.player;
                who.completelyStopAnimatingOrDoingAction();
                int direction = who.FacingDirection;
                if (frame.facing > -1)
                    who.FacingDirection = frame.facing;

                PlayFrame(frame);

                await System.Threading.Tasks.Task.Delay(frame.length);

                if (frame.facing > -1)
                    who.FacingDirection = direction;
                who.stopJittering();
                who.completelyStopAnimatingOrDoingAction();
                who.forceCanMove();
            }
        }
        public static void PlayFrame(PlayerAnimationFrame frame)
        {
            Farmer who = Game1.player;

            if (frame.music != null && frame.music.Length > 0)
                Game1.changeMusicTrack(frame.music);

            if (frame.frame > -1)
            {
                who.jitterStrength = frame.jitter;
                List<FarmerSprite.AnimationFrame> animationFrames = new List<FarmerSprite.AnimationFrame>(){
                    new FarmerSprite.AnimationFrame(frame.frame, 100, frame.secondaryArm, frame.flip, null, false).AddFrameAction(delegate (Farmer f)
                    {
                        f.jitterStrength = frame.jitter;
                    })
                };
                who.FarmerSprite.setCurrentAnimation(animationFrames.ToArray());
                who.FarmerSprite.PauseForSingleAnimation = true;
                who.FarmerSprite.loop = true;
                who.FarmerSprite.loopThisAnimation = true;
                who.Sprite.currentFrame = frame.frame;
            }
            if(frame.sound != null && frame.sound.Length > 0)
            {
                who.currentLocation.playSound(frame.sound);
            }
            if (frame.jump > 0)
            {
                who.synchronizedJump(frame.jump);
            }
        }

        /// <summary>Get whether this instance can load the initial version of the given asset.</summary>
        /// <param name="asset">Basic metadata about the asset being loaded.</param>
        public bool CanLoad<T>(IAssetInfo asset)
        {
            if (!Config.EnableMod)
                return false;

            return asset.AssetNameEquals(dictPath);
        }

        /// <summary>Load a matched asset.</summary>
        /// <param name="asset">Basic metadata about the asset being loaded.</param>
        public T Load<T>(IAssetInfo asset)
        {
            Monitor.Log("Loading dictionary");

            return (T)(object)new Dictionary<string, PlayerAnimation>();
        }
    }

}