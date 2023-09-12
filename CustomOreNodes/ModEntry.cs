using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Object = StardewValley.Object;

namespace CustomOreNodes
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry : Mod
    {

        public static ModEntry context;

        public static ModConfig Config;
        public static List<CustomOreNode> customOreNodesList = new List<CustomOreNode>();
        public static readonly string dictPath = "aedenthorn.CustomOreNodes/dict";
        public static IMonitor SMonitor;
        

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            context = this;
            Config = Helper.ReadConfig<ModConfig>();
            SMonitor = Monitor;
            var harmony = new Harmony(ModManifest.UniqueID);

            harmony.Patch(
               original: AccessTools.Method(typeof(MineShaft), "chooseStoneType"),
               postfix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.chooseStoneType_Postfix))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(GameLocation), "breakStone"),
               postfix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.breakStone_Postfix))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(Object), nameof(Object.draw), new Type[] { typeof(SpriteBatch), typeof(int), typeof(int), typeof(float) }),
               prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.Object_draw_Prefix))
            );

            if (Config.AllowCustomOreNodesAboveGround)
            {
                ConstructorInfo ci = typeof(Object).GetConstructor(new Type[] { typeof(Vector2), typeof(int), typeof(string), typeof(bool), typeof(bool), typeof(bool), typeof(bool) });
                harmony.Patch(
                   original: ci,
                   prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.Object_Prefix)),
                   postfix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.Object_Postfix))
                );
            }


            helper.Events.GameLoop.SaveLoaded += GameLoop_SaveLoaded;
            helper.Events.Player.Warped += Player_Warped;
            helper.Events.Content.AssetRequested += Content_AssetRequested;
        }

        private void Content_AssetRequested(object sender, StardewModdingAPI.Events.AssetRequestedEventArgs e)
        {
            if (e.NameWithoutLocale.IsEquivalentTo(dictPath))
            {
                e.LoadFrom(() => new Dictionary<string, object>(), StardewModdingAPI.Events.AssetLoadPriority.Exclusive);
            }
        }

        private void GameLoop_SaveLoaded(object sender, StardewModdingAPI.Events.SaveLoadedEventArgs e)
        {
            ReloadOreData(true);
        }
        private void GameLoop_DayStarted(object sender, StardewModdingAPI.Events.DayStartedEventArgs e)
        {
            ReloadOreData();
        }
        private void Player_Warped(object sender, StardewModdingAPI.Events.WarpedEventArgs e)
        {
            ReloadOreData();
        }

        public override object GetApi()
        {
            return new CustomOreNodesAPI();
        }

    }
}
 