using HarmonyLib;
using Microsoft.Xna.Framework;
using Netcode;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Characters;
using StardewValley.Locations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;

namespace CustomMarriageDialogue
{
    public partial class ModEntry : Mod
    {

        public static ModConfig Config;
        private static IMonitor PMonitor;
        private static IModHelper PHelper;

        public override void Entry(IModHelper helper)
        {
            Config = Helper.ReadConfig<ModConfig>();

            if (!Config.ModEnabled)
                return;

            PMonitor = Monitor;
            PHelper = Helper;

            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();
        }
    }
}