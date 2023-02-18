using Microsoft.Xna.Framework.Audio;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using System;
using System.IO;

namespace HugsAndKisses
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry
    {


        public void GameLoop_GameLaunched(object sender, GameLaunchedEventArgs e)
        {
            if (!Config.EnableMod)
                return;
            //Integrations.LoadModApis();

            if(Config.CustomKissSound.Length > 0)
            {
                string filePath = Path.Combine(Helper.DirectoryPath, "assets", $"{Config.CustomKissSound}");
                Monitor.Log("Kissing audio path: " + filePath);
                if (File.Exists(filePath))
                {
                    try
                    {
                        Kissing.kissEffect = SoundEffect.FromStream(new FileStream(filePath, FileMode.Open));
                    }
                    catch (Exception ex)
                    {
                        Monitor.Log("Error loading kissing audio: " + ex, LogLevel.Error);
                    }
                }
                else
                {
                    Monitor.Log("Kissing audio not found at path: " + filePath);
                }
            }
            if(Config.CustomHugSound.Length > 0)
            {
                string filePath = Path.Combine(Helper.DirectoryPath, "assets", $"{Config.CustomKissSound}.wav");
                Monitor.Log("Hug audio path: " + filePath);
                if (File.Exists(filePath))
                {
                    try
                    {
                        Kissing.hugEffect = SoundEffect.FromStream(new FileStream(filePath, FileMode.Open));
                    }
                    catch (Exception ex)
                    {
                        Monitor.Log("Error loading hug audio: " + ex, LogLevel.Error);
                    }
                }
                else
                {
                    Monitor.Log("Hug audio not found at path: " + filePath);
                }
            }

            // get Generic Mod Config Menu's API (if it's installed)
            var configMenu = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (configMenu != null)
            {

                // register mod
                configMenu.Register(
                    mod: ModManifest,
                    reset: () => Config = new ModConfig(),
                    save: () => Helper.WriteConfig(Config)
                );
                configMenu.AddBoolOption(
                    mod: ModManifest,
                    name: () => "Mod Enabled",
                    getValue: () => Config.EnableMod,
                    setValue: value => Config.EnableMod = value
                );
                configMenu.AddBoolOption(
                    mod: ModManifest,
                    name: () => "Roommate kisses",
                    getValue: () => Config.RoommateKisses,
                    setValue: value => Config.RoommateKisses = value
                );
                configMenu.AddNumberOption(
                    mod: ModManifest,
                    name: () => "Min Hearts For Marriage Kiss",
                    getValue: () => Config.MinHeartsForMarriageKiss,
                    setValue: value => Config.MinHeartsForMarriageKiss = value
                );
                configMenu.AddNumberOption(
                    mod: ModManifest,
                    name: () => "Hearts For Friendship",
                    getValue: () => Config.HeartsForFriendship,
                    setValue: value => Config.HeartsForFriendship = value
                );
                configMenu.AddBoolOption(
                    mod: ModManifest,
                    name: () => "Unlimited Kisses",
                    getValue: () => Config.UnlimitedDailyKisses,
                    setValue: value => Config.UnlimitedDailyKisses = value
                );
                configMenu.AddBoolOption(
                    mod: ModManifest,
                    name: () => "Dating Kisses",
                    getValue: () => Config.DatingKisses,
                    setValue: value => Config.DatingKisses = value
                );
                configMenu.AddBoolOption(
                    mod: ModManifest,
                    name: () => "Friend Hugs",
                    getValue: () => Config.FriendHugs,
                    setValue: value => Config.FriendHugs = value
                );
                configMenu.AddNumberOption(
                    mod: ModManifest,
                    name: () => "NPC Kiss Chance",
                    getValue: () => (int)Config.SpouseKissChance0to1*100,
                    setValue: value => Config.SpouseKissChance0to1 = value / 100f,
                    min: 0,
                    max: 100
                );
                configMenu.AddTextOption(
                    mod: ModManifest,
                    name: () => "Custom Kiss Sound",
                    getValue: () => Config.CustomKissSound,
                    setValue: value => Config.CustomKissSound = value
                );
                configMenu.AddTextOption(
                    mod: ModManifest,
                    name: () => "Custom Hug Sound",
                    getValue: () => Config.CustomHugSound,
                    setValue: value => Config.CustomHugSound = value
                );
                configMenu.AddTextOption(
                    mod: ModManifest,
                    name: () => "Custom Kiss Frames",
                    getValue: () => Config.CustomKissFrames,
                    setValue: value => Config.CustomKissFrames = value
                );
                configMenu.AddNumberOption(
                    mod: ModManifest,
                    name: () => "Max NPC Kiss Distance",
                    getValue: () => (int)Config.MaxDistanceToKiss,
                    setValue: value => Config.MaxDistanceToKiss = value
                );
                configMenu.AddNumberOption(
                    mod: ModManifest,
                    name: () => "Min Spouse Kiss Interval (s)",
                    getValue: () => (int)Config.MinSpouseKissIntervalSeconds,
                    setValue: value => Config.MinSpouseKissIntervalSeconds = value
                );
                configMenu.AddBoolOption(
                    mod: ModManifest,
                    name: () => "Multiple Spouses Kiss",
                    getValue: () => Config.AllowPlayerSpousesToKiss,
                    setValue: value => Config.AllowPlayerSpousesToKiss = value
                );
                configMenu.AddBoolOption(
                    mod: ModManifest,
                    name: () => "Allow Relatives To Hug",
                    getValue: () => Config.AllowNPCRelativesToHug,
                    setValue: value => Config.AllowNPCRelativesToHug = value
                );
                configMenu.AddBoolOption(
                    mod: ModManifest,
                    name: () => "Allow NPC Spouses To Kiss",
                    getValue: () => Config.AllowNPCSpousesToKiss,
                    setValue: value => Config.AllowNPCSpousesToKiss = value
                );
                configMenu.AddBoolOption(
                    mod: ModManifest,
                    name: () => "Allow Relatives To Kiss",
                    getValue: () => Config.AllowRelativesToKiss,
                    setValue: value => Config.AllowRelativesToKiss = value
                );
                configMenu.AddBoolOption(
                    mod: ModManifest,
                    name: () => "Allow Non-datable",
                    getValue: () => Config.AllowNonDateableNPCsToHugAndKiss,
                    setValue: value => Config.AllowNonDateableNPCsToHugAndKiss = value
                );
            }
        }

        public static void GameLoop_OneSecondUpdateTicked(object sender, OneSecondUpdateTickedEventArgs e)
        {
            if (!Config.EnableMod)
                return;

            Kissing.TrySpousesKiss(Game1.player.currentLocation);
        }
        public static void GameLoop_SaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            if (!Config.EnableMod)
                return;

            Misc.SetNPCRelations();
        }
    }
}