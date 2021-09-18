using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;
using xTile;
using xTile.Tiles;

namespace Renovations
{
    /// <summary>The mod entry point.</summary>
    public class ModEntry : Mod, IAssetLoader
    {

        public static IMonitor PMonitor;
        public static IModHelper PHelper;
        public static ModEntry context;
        public static ModConfig config;

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            context = this;
            config = Helper.ReadConfig<ModConfig>();

            if (!config.EnableMod)
                return;

            PMonitor = Monitor;
            PHelper = helper;

            var harmony = new Harmony(ModManifest.UniqueID);

            harmony.Patch(
               original: AccessTools.Method(typeof(HouseRenovation), nameof(HouseRenovation.GetAvailableRenovations)),
               postfix: new HarmonyMethod(typeof(CodePatches), nameof(CodePatches.HouseRenovation_GetAvailableRenovations_Postfix))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.MakeMapModifications)),
               prefix: new HarmonyMethod(typeof(CodePatches), nameof(CodePatches.GameLocation_MakeMapModifications_Prefix))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(Farmer), nameof(Farmer.performRenovation)),
               prefix: new HarmonyMethod(typeof(CodePatches), nameof(CodePatches.Farmer_performRenovation_Prefix))
            );

            harmony.Patch(
               original: AccessTools.Method(typeof(FarmHouse), nameof(FarmHouse.UpdateForRenovation)),
               prefix: new HarmonyMethod(typeof(CodePatches), nameof(CodePatches.UpdateForRenovation_Prefix))
            );
        }


        public bool CanLoad<T>(IAssetInfo asset)
        {
            return asset.AssetNameEquals("CustomRenovations");
        }

        public T Load<T>(IAssetInfo asset)
        {
            return (T)(object)new Dictionary<string, CustomRenovationData>();
        }

    }
}