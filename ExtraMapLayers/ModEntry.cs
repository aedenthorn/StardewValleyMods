using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using xTile;
using xTile.Dimensions;
using xTile.Display;
using xTile.Layers;
using xTile.Tiles;
using Rectangle = xTile.Dimensions.Rectangle;

namespace ExtraMapLayers
{
    /// <summary>The mod entry point.</summary>
    public class ModEntry : Mod
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
               original: AccessTools.Method(typeof(Layer), nameof(Layer.Draw)),
               postfix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.Layer_Draw_Postfix))
            );
        }
        public static void Layer_Draw_Postfix(Layer __instance)
        {
            if (!config.EnableMod || Regex.IsMatch(__instance.Id, "[0-9]$"))
                return;

            foreach (Layer layer in Game1.currentLocation.Map.Layers)
            {
                if(Regex.IsMatch(layer.Id, $"^{__instance.Id}[0-9]+$"))
                   layer.Draw(Game1.mapDisplayDevice, Game1.viewport, Location.Origin, false, 4);
            }
        }
    }
}