using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using System;
using Rectangle = Microsoft.Xna.Framework.Rectangle;
using StardewModdingAPI;

namespace CropHarvestBubbles
{
    /// <summary>The mod entry point.</summary>
    public class ModEntry : Mod
    {
        internal static IMonitor SMonitor;
        internal static IModHelper SHelper;
        internal static ModConfig Config;
        internal static Harmony harmony;

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            Config = Helper.ReadConfig<ModConfig>();

            SMonitor = Monitor;
            SHelper = helper;
            harmony = new Harmony(ModManifest.UniqueID);
            Helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;

            harmony.Patch(
                original: AccessTools.Method(typeof(Crop), nameof(Crop.draw)),
                postfix: new HarmonyMethod(typeof(ModEntry), nameof(Postfix_draw))
            );
            harmony.Patch(
                original: AccessTools.Method(typeof(Crop), nameof(Crop.drawWithOffset)),
                postfix: new HarmonyMethod(typeof(ModEntry), nameof(Postfix_drawWithOffset))
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
                name: () => SHelper.Translation.Get("GMCM_Option_ModEnabled_Name"),
                getValue: () => Config.ModEnabled,
                setValue: value => Config.ModEnabled = value
            );

            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => SHelper.Translation.Get("GMCM_Option_IgnoreFlowers_Name"),
                getValue: () => Config.IgnoreFlowers,
                setValue: value => Config.IgnoreFlowers = value
            );

            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => SHelper.Translation.Get("GMCM_Option_RequireKeyPress_Name"),
                getValue: () => Config.RequireKeyPress,
                setValue: value => Config.RequireKeyPress = value
            );

            configMenu.AddKeybindList(
                mod: ModManifest,
                name: () => SHelper.Translation.Get("GMCM_Option_PressKeys_Name"),
                getValue: () => Config.PressKeys,
                setValue: value => Config.PressKeys = value
            );
            
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => SHelper.Translation.Get("GMCM_Option_OpacityPercent_Name"),
                getValue: () => Config.OpacityPercent,
                setValue: value => Config.OpacityPercent = value,
                min: 1,
                max: 100
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => SHelper.Translation.Get("GMCM_Option_SizePercent_Name"),
                getValue: () => Config.SizePercent,
                setValue: value => Config.SizePercent = value,
                min: 1,
                max: 100
            );
        }
        public static void Postfix_draw(Crop __instance, SpriteBatch b, Vector2 tileLocation)
        {
            drawLogic(__instance, b, tileLocation);
        }
        public static void Postfix_drawWithOffset(Crop __instance, SpriteBatch b, Vector2 tileLocation, Vector2 offset)
        {
            drawLogic(__instance, b, tileLocation, (int)offset.X);
        }
        public static void drawLogic(Crop __instance, SpriteBatch b, Vector2 tileLocation, int offset = 0)
        {
            var item = ItemRegistry.GetDataOrErrorItem(__instance.indexOfHarvest.Value);

            if (!Config.ModEnabled || (Config.RequireKeyPress && !Config.PressKeys.IsDown()) || __instance.forageCrop.Value || __instance.dead.Value || __instance.currentPhase.Value < __instance.phaseDays.Count - 1 || (__instance.fullyGrown.Value && __instance.dayOfCurrentPhase.Value > 0) || !Game1.objectData.ContainsKey(__instance.indexOfHarvest.Value) || (Config.IgnoreFlowers && item.Category == StardewValley.Object.flowersCategory))
                return;

            float base_sort = (float)((tileLocation.Y + 1) * 64) / 10000f + tileLocation.X / 50000f;
            float yOffset = 4f * (float)Math.Round(Math.Sin(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / 250.0), 2);
            float movePercent = (100 - Config.SizePercent) / 100f;

            b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(tileLocation.X * 64 - 8 + movePercent * 40, tileLocation.Y * 64 - 96 - 16 - offset + yOffset + movePercent * 96)), new Rectangle?(new Rectangle(141, 465, 20, 24)), Color.White * (Config.OpacityPercent / 100f), 0f, Vector2.Zero, 4f * (Config.SizePercent / 100f), SpriteEffects.None, base_sort + 1E-06f);

            b.Draw(
                item.GetTexture(),
                Game1.GlobalToLocal(Game1.viewport, new Vector2(tileLocation.X * 64 + 32, tileLocation.Y * 64 - 64 - offset - 8 + yOffset + movePercent * 56)),
                item.GetSourceRect(),
                Color.White * (Config.OpacityPercent / 100f), 0f, new Vector2(8f, 8f), 4f * (Config.SizePercent / 100f), SpriteEffects.None, base_sort + 1E-05f
            );
            if (__instance.programColored.Value)
            {
                b.Draw(
                    item.GetTexture(),
                    Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(tileLocation.X * 64 + 32), (float)(tileLocation.Y * 64 - 64 - 8 - offset) + yOffset)),
                    item.GetSourceRect(),
                    __instance.tintColor.Value * (Config.OpacityPercent / 100f), 0f, new Vector2(8f, 8f), 4f * (Config.SizePercent / 100f), SpriteEffects.None, base_sort + 1.1E-05f
                );
            }
        }
    }
}