using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using System;

namespace WeekStartsSunday
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry : Mod
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;

        public static ModEntry context;

        public static bool accelerating;
        private static Texture2D boardTexture;
        private Harmony harmony;

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
            SHelper.Events.Content.AssetRequested += Content_AssetRequested;
            SHelper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;

            harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();
        }
        private void Content_AssetRequested(object sender, StardewModdingAPI.Events.AssetRequestedEventArgs e)
        {
            if (e.NameWithoutLocale.IsEquivalentTo("LooseSprites/Billboard"))
                e.Edit(EditBillboard, StardewModdingAPI.Events.AssetEditPriority.Late);
        }

        private void EditBillboard(IAssetData obj)
        {
            var image = obj.AsImage();
            var data = new Color[image.Data.Width * image.Data.Height];
            image.Data.GetData(data);
            var copy = new Texture2D(Game1.graphics.GraphicsDevice, image.Data.Width, image.Data.Height);
            copy.SetData(data);
            image.PatchImage(copy, new Rectangle(230, 232, 31, 15), new Rectangle(38, 232, 31, 15));
            image.PatchImage(copy, new Rectangle(38, 232, 191, 15), new Rectangle(70, 232, 191, 15));
        }

        private void GameLoop_GameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
        {

            var cpapi = Helper.ModRegistry.GetApi("Pathoschild.ContentPatcher");
            if (cpapi != null)
            {
                Monitor.Log($"patching content patcher");
                harmony.Patch(
                   original: AccessTools.Method(cpapi.GetType().Assembly.GetType("ContentPatcher.Framework.TokenSaveReader"), "GetDayOfWeek"),
                   postfix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.ShiftCPDayOfWeek))
                );
            }

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
                getValue: () => Config.ModEnabled,
                setValue: value => Config.ModEnabled = value
            );
        }
    }
}