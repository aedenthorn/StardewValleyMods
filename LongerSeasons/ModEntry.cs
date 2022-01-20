using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Menus;
using StardewValley.TerrainFeatures;
using System;

namespace LongerSeasons
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry : Mod, IAssetEditor
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

            Helper.Events.GameLoop.DayStarted += GameLoop_DayStarted;
            Helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;

            var harmony = new Harmony(ModManifest.UniqueID);

            // Game1 Patches

            harmony.Patch(
               original: AccessTools.Method(typeof(Game1), "_newDayAfterFade"),
               prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.Game1__newDayAfterFade_Prefix))
            );
            
            harmony.Patch(
               original: AccessTools.Method(typeof(Game1), "newSeason"),
               prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.Game1_newSeason_Prefix))
            );

            foreach(var type in typeof(Game1).Assembly.GetTypes())
            {
                if (type.FullName.StartsWith("StardewValley.Game1+<_newDayAfterFade>"))
                {
                    Monitor.Log($"Found {type}");
                    harmony.Patch(
                       original: AccessTools.Method(type, "MoveNext"),
                       transpiler: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.Game1__newDayAfterFade_Transpiler))
                    );
                    break;
                }
            }
            

            // SDate Patches

            harmony.Patch(
               original: AccessTools.Constructor(typeof(SDate), new Type[] { typeof(int), typeof(string), typeof(int), typeof(bool) }),
               transpiler: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.SDate_Transpiler)),
               postfix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.SDate_Postfix))
            );
            harmony.Patch(
               original: AccessTools.Constructor(typeof(SDate), new Type[] { typeof(int), typeof(string)}),
               postfix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.SDate_Postfix))
            );
            harmony.Patch(
               original: AccessTools.Constructor(typeof(SDate), new Type[] { typeof(int), typeof(string), typeof(int)}),
               postfix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.SDate_Postfix))
            );

            // Utility Patches


            harmony.Patch(
               original: AccessTools.Method(typeof(Utility), nameof(Utility.getDateStringFor)),
               transpiler: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.Utility_getDateStringFor_Transpiler))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(Utility), nameof(Utility.getSeasonNameFromNumber)),
               postfix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.Utility_getSeasonNameFromNumber_Postfix))
            );


            // Billboard Patches

            harmony.Patch(
               original: AccessTools.Constructor(typeof(Billboard), new Type[]{ typeof(bool) }),
               postfix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.Billboard_Postfix))
            );
            harmony.Patch(
               original: AccessTools.Method(typeof(Billboard), nameof(Billboard.draw), new Type[] { typeof(SpriteBatch) }),
               transpiler: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.Billboard_draw_Transpiler)),
               postfix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.Billboard_draw_Postfix))
            );

            // WorldDate Patches
            harmony.Patch(
               original: AccessTools.PropertyGetter(typeof(WorldDate), nameof(WorldDate.TotalDays)),
               prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.WorldDate_TotalDays_Getter_Prefix))
            );
            harmony.Patch(
               original: AccessTools.PropertySetter(typeof(WorldDate), nameof(WorldDate.TotalDays)),
               prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.WorldDate_TotalDays_Setter_Prefix))
            );

            // Bush Patches
            harmony.Patch(
               original: AccessTools.Method(typeof(Bush), nameof(Bush.inBloom)),
               prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.Bush_inBloom_Prefix))
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
                name: () => "Enable Mod",
                getValue: () => Config.EnableMod,
                setValue: value => Config.EnableMod = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Extend Berry Seasons",
                getValue: () => Config.ExtendBerry,
                setValue: value => Config.ExtendBerry = value
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Days per Month",
                getValue: () => Config.DaysPerMonth,
                setValue: value => Config.DaysPerMonth = value
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Months per Season",
                getValue: () => Config.MonthsPerSeason,
                setValue: value => Config.MonthsPerSeason = value
            );
        }

        private void GameLoop_DayStarted(object sender, StardewModdingAPI.Events.DayStartedEventArgs e)
        {
            Helper.Content.InvalidateCache("LooseSprites/Billboard");
        }

        /// <summary>Get whether this instance can load the initial version of the given asset.</summary>
        /// <param name="asset">Basic metadata about the asset being loaded.</param>
        public bool CanEdit<T>(IAssetInfo asset)
        {
            if (!Config.EnableMod)
                return false;

            return Game1.dayOfMonth > 28 && asset.AssetNameEquals("LooseSprites/Billboard");
        }

        /// <summary>Load a matched asset.</summary>
        /// <param name="asset">Basic metadata about the asset being loaded.</param>
        public void Edit<T>(IAssetData asset)
        {
            var editor = asset.AsImage();
            
            Texture2D sourceImage = Helper.Content.Load<Texture2D>("assets/numbers.png");

            int startDay = 28 * (Game1.dayOfMonth / 28) + 1;

            for(int i = startDay; i < startDay + 28; i++)
            {
                int cents = i / 100;
                int tens = (i - cents * 100) / 10;
                int ones = i - cents * 100 - tens * 10;
                int xOff = 7;
                if(cents > 0)
                {
                    xOff = 14;
                    editor.PatchImage(sourceImage, new Rectangle(6 * cents, 0, 7, 11), new Rectangle(39 + (i - 1) % 7 * 32, 248 + (i - startDay) / 7 * 32, 7, 11), PatchMode.Overlay);
                }
                editor.PatchImage(sourceImage, new Rectangle(6 * tens, 0, 7, 11), new Rectangle(32 + xOff + (i - 1) % 7 * 32, 248 + (i - startDay) / 7 * 32, 7, 11), PatchMode.Overlay);
                editor.PatchImage(sourceImage, new Rectangle(6 * ones, 0, 7, 11), new Rectangle(39 + xOff + (i - 1) % 7 * 32, 248 + (i - startDay) / 7 * 32, 7, 11), PatchMode.Overlay);
            }
        }
    }

    public class SeasonMonth
    {
        public int month = 1;
    }
}