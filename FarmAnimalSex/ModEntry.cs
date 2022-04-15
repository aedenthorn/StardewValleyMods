using HarmonyLib;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;

namespace FarmAnimalSex
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry : Mod, IAssetLoader
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;

        public static ModEntry context;
        private static ClickableTextureComponent maleButt;
        private static ClickableTextureComponent femaleButt;
        private static ClickableTextureComponent intersexButt;
        private static ClickableTextureComponent hoveredComponent;
        
        private static Texture2D buttonTexture;
        private static string sexKey = "aedenthorn.FarmAnimalSex/sex";
        private static bool skipMale;
        private static bool skipLoad;
        private enum Sexes
        {
            Female,
            Male,
            Intersex
        }
        private static Sexes currentSex;


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

            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();
        }

        private void GameLoop_SaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            buttonTexture = Helper.Content.Load<Texture2D>("assets/sexes.png");
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
                name: () => "Mod Enabled?",
                getValue: () => Config.EnableMod,
                setValue: value => Config.EnableMod = value
            );
        }



        /// <summary>Get whether this instance can load the initial version of the given asset.</summary>
        /// <param name="asset">Basic metadata about the asset being loaded.</param>
        public bool CanLoad<T>(IAssetInfo asset)
        {
            if (!Config.EnableMod || skipLoad)
                return false;

            return asset.AssetName.StartsWith("Animals/"+Sexes.Male) || asset.AssetName.StartsWith("Animals/" + Sexes.Intersex);
        }

        /// <summary>Load a matched asset.</summary>
        /// <param name="asset">Basic metadata about the asset being loaded.</param>
        public T Load<T>(IAssetInfo asset)
        {
            Monitor.Log($"Loading asset {asset.AssetName}");

            try
            {
                return (T)(object)Helper.Content.Load<Texture2D>("assets/"+asset.AssetName.Substring("Animals/".Length));
            }
            catch
            {
                try
                {
                    skipLoad = true;
                    var t = Helper.Content.Load<Texture2D>(asset.AssetName, ContentSource.GameContent);
                    skipLoad = false;
                    return (T)(object)t;
                }
                catch
                {
                    if(asset.AssetName.StartsWith("Animals/" + Sexes.Male))
                        return (T)(object) Game1.content.Load<Texture2D>("Animals/" + asset.AssetName.Substring(("Animals/" + Sexes.Male).Length));
                    else
                        return (T)(object) Game1.content.Load<Texture2D>("Animals/" + asset.AssetName.Substring(("Animals/" + Sexes.Intersex).Length));
                }
            }
        }
    }
}