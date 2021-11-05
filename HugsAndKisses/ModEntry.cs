using HarmonyLib;
using StardewModdingAPI;
using StardewValley;
using System;
using System.Collections.Generic;

namespace HugsAndKisses
{
    /// <summary>The mod entry point.</summary>
    public class ModEntry : Mod
    {

        public static IMonitor SMonitor;
        public static IModHelper PHelper;
        public static ModConfig SConfig;
        
        public static Multiplayer mp;
        public static Random myRand;
        public static int bedSleepOffset = 76;

        public static Dictionary<string, Dictionary<string, string>> relationships = new Dictionary<string, Dictionary<string, string>>();

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            SConfig = Helper.ReadConfig<ModConfig>();

            if (!SConfig.EnableMod)
                return;

            SMonitor = Monitor;
            PHelper = helper;

            mp = helper.Reflection.GetField<Multiplayer>(typeof(Game1), "multiplayer").GetValue();
            myRand = new Random();

            helper.Events.GameLoop.GameLaunched += HelperEvents.GameLoop_GameLaunched;
            helper.Events.GameLoop.OneSecondUpdateTicked += HelperEvents.GameLoop_OneSecondUpdateTicked;
            helper.Events.GameLoop.SaveLoaded += HelperEvents.GameLoop_SaveLoaded;

            HelperEvents.Initialize(Monitor, SConfig, helper);
            Misc.Initialize(Monitor, SConfig, helper);
            Kissing.Initialize(Monitor, SConfig, helper);
            NPCPatches.Initialize(Monitor, SConfig, helper);

            var harmony = new Harmony(ModManifest.UniqueID);


            // Event patches

            harmony.Patch(
               original: AccessTools.Method(typeof(Event), nameof(Event.command_playSound)),
               prefix: new HarmonyMethod(typeof(EventPatches), nameof(EventPatches.Event_command_playSound_Prefix))
            );

            // NPC patches

            harmony.Patch(
               original: AccessTools.Method(typeof(NPC), nameof(NPC.checkAction)),
               prefix: new HarmonyMethod(typeof(NPCPatches), nameof(NPCPatches.NPC_checkAction_Prefix))
            );

        }

        public override object GetApi()
        {
            return new KissingAPI();
        }
    }
}