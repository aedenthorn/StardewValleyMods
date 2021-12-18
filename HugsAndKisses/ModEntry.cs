using HarmonyLib;
using StardewModdingAPI;
using StardewValley;
using System;
using System.Collections.Generic;

namespace HugsAndKisses
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry : Mod
    {

        public static IMonitor SMonitor;
        public static IModHelper PHelper;
        public static ModConfig Config;
        
        public static Multiplayer mp;
        public static Random myRand;
        public static int bedSleepOffset = 76;

        public static Dictionary<string, Dictionary<string, string>> relationships = new Dictionary<string, Dictionary<string, string>>();

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            Config = Helper.ReadConfig<ModConfig>();

            if (!Config.EnableMod)
                return;

            SMonitor = Monitor;
            PHelper = helper;

            mp = helper.Reflection.GetField<Multiplayer>(typeof(Game1), "multiplayer").GetValue();
            myRand = new Random();

            helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            helper.Events.GameLoop.OneSecondUpdateTicked += GameLoop_OneSecondUpdateTicked;
            helper.Events.GameLoop.SaveLoaded += GameLoop_SaveLoaded;

            Misc.Initialize(Monitor, Config, helper);
            Kissing.Initialize(Monitor, Config, helper);
            NPCPatches.Initialize(Monitor, Config, helper);

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