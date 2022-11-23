using HarmonyLib;
using StardewModdingAPI;
using StardewValley;
using System.Collections.Generic;
using System.Linq;

namespace CustomMineCartSteam
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry : Mod
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;

        public static ModEntry context;

        public static string dictPath = "aedenthorn.CustomMineCartSteam/dictionary";
        public static Dictionary<string, SteamData> steamDict = new();

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

            Helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            Helper.Events.GameLoop.DayStarted += GameLoop_DayStarted;
            Helper.Events.Player.Warped += Player_Warped;
            Helper.Events.Content.AssetRequested += Content_AssetRequested;

            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();


        }

        private void Player_Warped(object sender, StardewModdingAPI.Events.WarpedEventArgs e)
        {
            int id = 424200;
            foreach (var sd in steamDict.Values)
            {
                if (sd.location == e.NewLocation.Name)
                {
                    if (sd.replaceSteam)
                    {
                        if (!Game1.MasterPlayer.mailReceived.Contains("ccBoilerRoom"))
                        {
                            continue;
                        }
                        var mcs = AccessTools.Field(Game1.currentLocation.GetType(), "minecartSteam");
                        if (mcs is not null)
                        {
                            AccessTools.Field(Game1.currentLocation.GetType(), "minecartSteam").SetValue(Game1.currentLocation, null);
                        }
                    }
                        
                    TemporaryAnimatedSprite sprite;
                    if (string.IsNullOrEmpty(sd.texturePath))
                    {
                        sprite = new TemporaryAnimatedSprite(sd.animationRow, sd.position.ToVector2(), sd.color, sd.animationLength, sd.flipped, sd.animationInterval, sd.animationLoops, sd.sourceRectWidth, sd.layerDepth, sd.sourceRectHeight, sd.delay)
                        {
                            layerDepth = sd.layerDepth
                        };
                    }
                    else
                    {
                        sprite = new TemporaryAnimatedSprite(sd.texturePath, sd.sourceRect, sd.animationInterval, sd.animationLength, sd.animationLoops, sd.position.ToVector2(), sd.flicker, sd.flipped, sd.layerDepth, sd.alphaFade, sd.color, sd.scale, sd.scaleChange, sd.rotation, sd.rotationChange, sd.local)
                        {
                            layerDepth = sd.layerDepth,
                            motion = sd.motion.ToVector2(),
                            acceleration = sd.acceleration.ToVector2()
                        };
                    }
                    sprite.id = id++;
                    Game1.currentLocation.removeTemporarySpritesWithIDLocal(sprite.id);
                    Game1.currentLocation.TemporarySprites.Add(sprite);

                }
            }
        }

        private void GameLoop_DayStarted(object sender, StardewModdingAPI.Events.DayStartedEventArgs e)
        {
            if (!Config.ModEnabled)
                return;
            steamDict = Helper.GameContent.Load<Dictionary<string, SteamData>>(dictPath);

            Monitor.Log($"loaded {steamDict.Count} custom temporary sprites");
        }

        private void Content_AssetRequested(object sender, StardewModdingAPI.Events.AssetRequestedEventArgs e)
        {
            if (e.NameWithoutLocale.IsEquivalentTo(dictPath))
            {
                e.LoadFrom(() => new Dictionary<string, SteamData>(), StardewModdingAPI.Events.AssetLoadPriority.Exclusive);
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