using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley;
using System;
using System.Globalization;

namespace FriendshipTweaks
{
    //by Xen0nex
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

            context = this;

            SMonitor = Monitor;
            SHelper = helper;

            helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;

            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();

            harmony.Patch(
            original: AccessTools.Method(typeof(NPC), nameof(NPC.receiveGift)),
            transpiler: new HarmonyMethod(typeof(New_Patches), nameof(New_Patches.GiftPatch))
        );

            harmony.Patch(
            original: AccessTools.Method(typeof(Event), nameof(Event.chooseSecretSantaGift)),
            transpiler: new HarmonyMethod(typeof(New_Patches), nameof(New_Patches.WinterStarPatch))
        );

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
                name: () => "Mod Enabled",
                getValue: () => Config.ModEnabled,
                setValue: value => Config.ModEnabled = value
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Max Hearts",
                tooltip: () => "Applies to all NPCs. Vanilla max is 10 Hearts (14 for spouses).",
                getValue: () => Config.MaxHearts,
                setValue: value => Config.MaxHearts = value,
                min: 10,
                max: 30
            );
            configMenu.AddTextOption(
                mod: ModManifest,
                name: () => "Increase Mult",
                tooltip: () => "Multiply friendship increase by this amount.",
                getValue: () => "" + Config.IncreaseModifier,
                setValue: delegate (string value) { try { Config.IncreaseModifier = float.Parse(value, CultureInfo.InvariantCulture); } catch { } }
            );
            configMenu.AddTextOption(
                mod: ModManifest,
                name: () => "Decrease Mult",
                tooltip: () => "Multiply friendship decrease by this amount.",
                getValue: () => "" + Config.DecreaseModifier,
                setValue: delegate (string value) { try { Config.DecreaseModifier = float.Parse(value, CultureInfo.InvariantCulture); } catch { } }
            );
            configMenu.AddTextOption(
                mod: ModManifest,
                name: () => "Birthday Mult",
                tooltip: () => "Multiply friendship increase from gifts on an NPC's birthday by this amount (Replaces the vanilla 8x Birthday Mult).",
                getValue: () => "" + Config.BirthdayMultiplier,
                setValue: delegate (string value) { try { Config.BirthdayMultiplier = float.Parse(value, CultureInfo.InvariantCulture); } catch { } }
            );
            configMenu.AddTextOption(
                mod: ModManifest,
                name: () => "Winter Star Mult",
                tooltip: () => "Multiply friendship increase from giving your Secret Winter Star gift by this amount (Replaces the vanilla 5x Winter Star Mult).",
                getValue: () => "" + Config.WinterStarMultiplier,
                setValue: delegate (string value) { try { Config.WinterStarMultiplier = float.Parse(value, CultureInfo.InvariantCulture); } catch { } }
            );
            configMenu.AddTextOption(
                mod: ModManifest,
                name: () => "Stardrop Tea Special Mult",
                tooltip: () => "Multiply friendship increase from giving Stardrop Tea as a gift on an NPC's Birthday or as a Secret Winter Star gift by this amount (Replaces the vanilla 3x Stardrop Tea Mult in those cases). If this multiplier is set higher than the Birthday or Winter Star Mult, it defaults to using those Mults instead when given as a Birthday / Winter Star gift.",
                getValue: () => "" + Config.StardropTeaMultiplier,
                setValue: delegate (string value) { try { Config.StardropTeaMultiplier = float.Parse(value, CultureInfo.InvariantCulture); } catch { } }
            );
        }
    }
}
