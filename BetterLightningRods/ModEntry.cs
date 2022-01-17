using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Characters;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BetterLightningRods
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry : Mod
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;

        public static ModEntry context;

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            Config = Helper.ReadConfig<ModConfig>();

            if (!Config.EnableMod)
                return;

            context = this;

            SMonitor = Monitor;
            SHelper = helper;
            helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            helper.Events.Input.ButtonPressed += Input_ButtonPressed;


            var harmony = new Harmony(ModManifest.UniqueID);

            harmony.Patch(
               original: AccessTools.Method(typeof(Utility), nameof(Utility.performLightningUpdate)),
               transpiler: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.Utility_performLightningUpdate_Transpiler))
            );

        }

        private void Input_ButtonPressed(object sender, StardewModdingAPI.Events.ButtonPressedEventArgs e)
        {
            if(Context.IsWorldReady && e.Button == Config.LightningButton)
            {
                Monitor.Log("Lightning strike!");
                Utility.performLightningUpdate(Game1.timeOfDay);

            }
        }

        private void GameLoop_GameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
        {
            // get Generic Mod Config Menu's API (if it's installed)
            var configMenu = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (configMenu is null)
                return;

            // register mod
            configMenu.Register(
                mod: ModManifest,
                reset: () => Config = new ModConfig(),
                save: () => Helper.WriteConfig(Config)
            );

            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Mod Enabled?",
                getValue: () => Config.EnableMod,
                setValue: value => Config.EnableMod = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Unique Check?",
                tooltip: () => "Don't check the same rod twice per strike.",
                getValue: () => Config.UniqueCheck,
                setValue: value => Config.UniqueCheck = value
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Rods To Check",
                tooltip: () => "Each time lightning strikes, check this many rods to see if they can receive the charge.",
                getValue: () => Config.RodsToCheck,
                setValue: value => Config.RodsToCheck = value
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Base Lightning Chance",
                getValue: () => (int)Config.LightningChance,
                setValue: value => Config.LightningChance = value,
                min: 0,
                max: 100
            );
        }
        private static int GetLightningRod(List<Vector2> rods, int index)
        {
            int rod = Math.Min(index, rods.Count - 1);
            return rod;
        }
        private static List<Vector2> ShuffleRodList(List<Vector2> rods)
        {
            //SMonitor.Log($"Shuffling {rods.Count} rods");
            ShuffleList(rods);
            if (Config.OnlyCheckEmpty)
            {
                for(int i = rods.Count - 1; i >= 0; i--)
                {
                    if (Game1.getFarm().objects[rods[i]].heldObject.Value != null)
                        rods.RemoveAt(i);
                }
            }
            return rods;
        }
        public static List<T> ShuffleList<T>(List<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = Game1.random.Next(n + 1);
                var value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
            return list;
        }
    }

}