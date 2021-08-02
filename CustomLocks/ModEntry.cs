using HarmonyLib;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Network;
using System;

namespace CustomLocks
{
    /// <summary>The mod entry point.</summary>
    public class ModEntry : Mod
    {

        public static ModConfig Config;

        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            Config = Helper.ReadConfig<ModConfig>();

            if (!Config.Enabled)
                return;


            CustomLocksPatches.Initialize(Monitor);
            var harmony = new Harmony(this.ModManifest.UniqueID);

            harmony.Patch(
               original: AccessTools.Method(typeof(Mountain), nameof(Mountain.checkAction)),
               prefix: new HarmonyMethod(typeof(CustomLocksPatches), nameof(CustomLocksPatches.Mountain_checkAction_Prefix))
            );
            harmony.Patch(
               original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.performTouchAction)),
               prefix: new HarmonyMethod(typeof(CustomLocksPatches), nameof(CustomLocksPatches.GameLocation_performTouchAction_Prefix))
            );
            harmony.Patch(
               original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.performAction)),
               prefix: new HarmonyMethod(typeof(CustomLocksPatches), nameof(CustomLocksPatches.GameLocation_performAction_Prefix))
            );
            harmony.Patch(
               original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.lockedDoorWarp)),
               prefix: new HarmonyMethod(typeof(CustomLocksPatches), nameof(CustomLocksPatches.GameLocation_lockedDoorWarp))
            );
        }

        internal static void DoWarp(string[] actionParams, GameLocation instance)
        {
            Rumble.rumble(0.15f, 200f);
            Game1.player.completelyStopAnimatingOrDoingAction();
            instance.playSoundAt("doorClose", Game1.player.getTileLocation(), NetAudio.SoundContext.Default);
            Game1.warpFarmer(actionParams[3], Convert.ToInt32(actionParams[1]), Convert.ToInt32(actionParams[2]), false);
        }
    }
}