using Harmony;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Network;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Object = StardewValley.Object;

namespace CustomPaintings
{
    public class ModEntry : Mod
    {
        public static ModEntry context;
        private static ModConfig Config;
        private static IMonitor SMonitor;
        private static IModHelper SHelper;

        public override void Entry(IModHelper helper)
        {
            return;
            context = this;
            Config = Helper.ReadConfig<ModConfig>();
            if (!Config.EnableMod)
                return;

            SMonitor = Monitor;
            SHelper = Helper;


            var harmony = HarmonyInstance.Create(this.ModManifest.UniqueID);

            harmony.Patch(
                original: AccessTools.Method(typeof(Furniture), nameof(Furniture.placementAction)),
                prefix: new HarmonyMethod(typeof(ModEntry), nameof(Furniture_placementAction_Prefix))
            );
        }

        private static void Furniture_placementAction_Prefix(Furniture __instance)
        {
            if(__instance.furniture_type == 6 && SHelper.Input.IsDown(Config.ModKey))
            {
                var paintingFiles = Directory.GetFiles(Path.Combine(SHelper.DirectoryPath, "paintings"), "*.*");

            }
        }
    }
}
