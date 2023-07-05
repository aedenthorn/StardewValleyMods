using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace TileTransparency
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

            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.Patch(
                original: AccessTools.Method(helper.GetType().Assembly.GetType("StardewModdingAPI.Framework.Rendering.SDisplayDevice"), "DrawTile"),
                prefix: new HarmonyMethod(typeof(ModEntry), nameof(DrawTile_Prefix)),
                postfix: new HarmonyMethod(typeof(ModEntry), nameof(DrawTile_Postfix))
            );

        }
    }
}