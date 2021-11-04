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

        public static IMonitor PMonitor;
        public static IModHelper PHelper;
        public static ModConfig config;
        
        public static Multiplayer mp;
        public static Random myRand;
        public static int bedSleepOffset = 76;

        public static Dictionary<string, Dictionary<string, string>> relationships = new Dictionary<string, Dictionary<string, string>>();

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            config = Helper.ReadConfig<ModConfig>();

            if (!config.EnableMod)
                return;

            PMonitor = Monitor;
            PHelper = helper;

            mp = helper.Reflection.GetField<Multiplayer>(typeof(Game1), "multiplayer").GetValue();
            myRand = new Random();

            helper.Events.GameLoop.GameLaunched += HelperEvents.GameLoop_GameLaunched;
            helper.Events.GameLoop.OneSecondUpdateTicked += HelperEvents.GameLoop_OneSecondUpdateTicked;
            helper.Events.GameLoop.SaveLoaded += HelperEvents.GameLoop_SaveLoaded;

            HelperEvents.Initialize(Monitor, config, helper);
            NPCPatches.Initialize(Monitor, config, helper);
            Misc.Initialize(Monitor, config, helper);
            Kissing.Initialize(Monitor, config, helper);

            var harmony = new Harmony(ModManifest.UniqueID);


            // npc patches

            harmony.Patch(
               original: AccessTools.Method(typeof(NPC), nameof(NPC.checkAction)),
               prefix: new HarmonyMethod(typeof(NPCPatches), nameof(NPCPatches.NPC_checkAction_Prefix))
            );


            // Event patches

            harmony.Patch(
               original: AccessTools.Method(typeof(Event), nameof(Event.command_playSound)),
               prefix: new HarmonyMethod(typeof(EventPatches), nameof(EventPatches.Event_command_playSound_Prefix))
            );

        }
    }
}