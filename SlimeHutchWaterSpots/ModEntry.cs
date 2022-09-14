using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using System;

namespace SlimeHutchWaterSpots
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


            context = this;

            SMonitor = Monitor;
            SHelper = helper;

            harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();
        }
    }
}