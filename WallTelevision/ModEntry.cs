using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Objects;
using System.Collections.Generic;
using Object = StardewValley.Object;

namespace WallTelevision
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
            helper.Events.Content.AssetRequested += Content_AssetRequested;
            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();
            // Tropical tv item id is 2326
            // Plasma tv item it is 1468
        }

        private void Content_AssetRequested(object sender, AssetRequestedEventArgs args)
        {
            if(args.NameWithoutLocale.IsEquivalentTo("aedenthorn.WallTelevision/plasma"))
            {
                args.LoadFromModFile<Texture2D>("assets/plasma.png", AssetLoadPriority.Medium);
            }else if(args.NameWithoutLocale.IsEquivalentTo("aedenthorn.WallTelevision/tropical"))
            {
                args.LoadFromModFile<Texture2D>("assets/tropical.png", AssetLoadPriority.Medium);
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

            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Mod Enabled",
                getValue: () => Config.EnableMod,
                setValue: value => Config.EnableMod = value
            );
        }

        private static Texture2D getPlasmaTexture()
        {
            return Game1.content.Load<Texture2D>("aedenthorn.WallTelevision/plasma");
        }

        private static Texture2D getTropicalTexture()
        {
            return Game1.content.Load<Texture2D>("aedenthorn.WallTelevision/tropical");
        }
    }
}