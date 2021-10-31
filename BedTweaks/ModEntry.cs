using HarmonyLib;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley.Objects;
using System;
using Object = StardewValley.Object;

namespace BedTweaks
{
    /// <summary>The mod entry point.</summary>
    public class ModEntry : Mod
    {

        public static IMonitor PMonitor;
        public static IModHelper PHelper;
        public static ModConfig config;

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            config = Helper.ReadConfig<ModConfig>();

            if (!config.EnableMod)
                return;

            PMonitor = Monitor;
            PHelper = helper;

            FurniturePatches.Initialize(Monitor, Helper, config);
            ObjectPatches.Initialize(Monitor, Helper, config);

            var harmony = new Harmony(ModManifest.UniqueID);

            // Object patches

            harmony.Patch(
               original: AccessTools.Method(typeof(Object), nameof(Object.draw), new Type[] { typeof(SpriteBatch), typeof(int), typeof(int), typeof(float) }),
               prefix: new HarmonyMethod(typeof(ObjectPatches), nameof(ObjectPatches.Object_draw_Prefix))
            );

            // Furniture patches

            harmony.Patch(
               original: AccessTools.Method(typeof(BedFurniture), nameof(BedFurniture.draw), new Type[] { typeof(SpriteBatch), typeof(int), typeof(int), typeof(float) }),
               prefix: new HarmonyMethod(typeof(FurniturePatches), nameof(FurniturePatches.BedFurniture_draw_Prefix))
            );
        }
        public override object GetApi()
        {
            return new BedTweaksAPI();
        }
    }
}