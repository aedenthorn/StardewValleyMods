using Force.DeepCloner;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using System;
using System.Collections.Generic;

namespace CustomAchievements
{
    /// <summary>The mod entry point.</summary>
    public class ModEntry : Mod
    {

        public static IMonitor PMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;
        
        public static readonly string dictPath = "custom_achievements_dictionary";

        public static Dictionary<int, CustomAcheivementData> currentAchievements = new Dictionary<int, CustomAcheivementData>();

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            Config = Helper.ReadConfig<ModConfig>();

            if (!Config.EnableMod)
                return;

            PMonitor = Monitor;
            SHelper = helper;

            MyPatches.Initialize(Monitor, Helper, Config);

            MyPatches.MakePatches(ModManifest.UniqueID);

            Helper.Events.GameLoop.ReturnedToTitle += GameLoop_ReturnedToTitle;
            Helper.Events.GameLoop.DayStarted += GameLoop_DayStarted;
            Helper.Events.GameLoop.TimeChanged += GameLoop_TimeChanged;
            Helper.Events.Player.Warped += Player_Warped;
            helper.Events.Content.AssetRequested += Content_AssetRequested;
        }

        private void GameLoop_ReturnedToTitle(object sender, ReturnedToTitleEventArgs e)
        {
            currentAchievements.Clear();
        }
        private void Content_AssetRequested(object sender, StardewModdingAPI.Events.AssetRequestedEventArgs e)
        {

            if (e.NameWithoutLocale.IsEquivalentTo(dictPath))
            {
                e.LoadFrom(() => new Dictionary<string, CustomAcheivementData>(), StardewModdingAPI.Events.AssetLoadPriority.Exclusive);
            }
        }
        private void Player_Warped(object sender, StardewModdingAPI.Events.WarpedEventArgs e)
        {
            CheckForAchievements();
        }

        private void GameLoop_TimeChanged(object sender, StardewModdingAPI.Events.TimeChangedEventArgs e)
        {
            CheckForAchievements();

        }

        private void GameLoop_DayStarted(object sender, StardewModdingAPI.Events.DayStartedEventArgs e)
        {
            SHelper.Events.GameLoop.UpdateTicked += GameLoop_UpdateTicked;
        }

        private void GameLoop_UpdateTicked(object sender, StardewModdingAPI.Events.UpdateTickedEventArgs e)
        {
            CheckForAchievements();
            SHelper.Events.GameLoop.UpdateTicked -= GameLoop_UpdateTicked;
        }

        public static void CheckForAchievements()
        {
            var sound = false;
            var dict = Game1.content.Load<Dictionary<string, CustomAcheivementData>>(dictPath);
            if(dict.Count != currentAchievements.Count)
            {
                currentAchievements.Clear();
                foreach(var a in dict.Values)
                {
                    currentAchievements[a.ID.GetHashCode()] = a;
                }
            }
            else
            {
                foreach (var a in dict.Values)
                {
                    if (currentAchievements.TryGetValue(a.ID.GetHashCode(), out var a1) && !a1.achieved && a.achieved)
                    {
                        PMonitor.Log($"Achievement {a.name} achieved!", LogLevel.Debug);
                        currentAchievements[a.ID.GetHashCode()].achieved = true;
                        if (!sound)
                        {
                            Game1.playSound("achievement");
                            sound = true;
                        }
                        Game1.addHUDMessage(HUDMessage.ForAchievement(a.name));
                    }
                }
            }
        }

    }
}