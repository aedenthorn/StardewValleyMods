using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley;
using System;
using System.Globalization;

namespace CooperativeEggHunt
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry : Mod
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;

        public static ModEntry context;
        
        public static string talkedKey = "aedenthorn.CooperativeEggHunt/talked";
        
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            Config = Helper.ReadConfig<ModConfig>();

            context = this;

            SMonitor = Monitor;
            SHelper = helper;

            helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            helper.Events.Content.AssetRequested += Content_AssetRequested;

            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();

        }

        private void Content_AssetRequested(object sender, StardewModdingAPI.Events.AssetRequestedEventArgs e)
        {
            if (!Config.ModEnabled)
                return;
            if (e.NameWithoutLocale.IsEquivalentTo("Characters/Dialogue/Abigail"))
            {
                e.Edit(delegate (IAssetData assetData)
                {
                    var dict = assetData.AsDictionary<string, string>().Data;
                    dict["spring_12"] = Helper.Translation.Get("Abigail_spring_12");
                });
            }

            else if (e.NameWithoutLocale.IsEquivalentTo("Characters/Dialogue/MarriageDialogueAbigail"))
            {
                e.Edit(delegate (IAssetData assetData)
                {
                    var dict = assetData.AsDictionary<string, string>().Data;
                    dict["spring_12"] = Helper.Translation.Get("MarriageDialogueAbigail_spring_12");
                });
            }

            else if (e.NameWithoutLocale.IsEquivalentTo("Data/Festivals/spring13"))
            {
                e.Edit(delegate (IAssetData assetData)
                {
                    var dict = assetData.AsDictionary<string, string>().Data;
                    dict["mainEvent"] = $"pause 500/playMusic none/pause 500/globalFade/viewport -1000 -1000/loadActors MainEvent/warp farmer1 27 69 /warp farmer2 27 67/warp farmer3 32 67/warp farmer4 28 71/faceDirection farmer1 0/faceDirection farmer2 1/faceDirection farmer3 3/faceDirection farmer4 0/pause 1000/viewport 27 67 true unfreeze/pause 2000/speak Lewis \"{Helper.Translation.Get("mainEvent-1")}\"/pause 100/jump Jas/jump Vincent/pause 1000/speak Lewis \"{Helper.Translation.Get("mainEvent-2")}\"/pause 100/faceDirection Vincent 3 true/faceDirection Jas 1 true/pause 1000/faceDirection Vincent 0 true/faceDirection Jas 0 /pause 800/faceDirection Lewis 3/faceDirection Lewis 2/faceDirection Lewis 1/faceDirection Lewis 2/pause 800/speak Lewis \"{Helper.Translation.Get("mainEvent-3")}\"/pause 1000/waitForOtherPlayers startContest/jump Lewis/pause 1000/speak Lewis \"{Helper.Translation.Get("mainEvent-4")}\"/advancedMove Maru false -2 0 0 6 -9 0 0 -2 -9 0 0 -1 2 0 0 -12 6 0 0 -7 3 0/advancedMove Abigail false 0 21 -20 0 0 3 7 0 0 -15 -9 0 9 0 0 18/advancedMove Jas false 0 12 8 0 0 10 3 0 0 4 14 0 0 -20 -6 0 0 3/advancedMove Sam false 4 0 0 -4 2 0 0 -4 12 0 0 -6 19 0 0 20 0 -20 0 20/advancedMove Vincent false 0 3 24 0 0 21 9 0 0 4 -20 0 20 0 -20 0/advancedMove Leo false 0 -10 -1 0 0 -3 -6 0 0 -2 -3 0 0 -3 -1 0 0 20 -5 0 0 -10 -2 0/playSound whistle/playMusic tickTock/playerControl eggHunt";
                    dict["afterEggHunt"] = $"pause 100/playSound whistle/waitForOtherPlayers endContest/pause 1000/globalFade/viewport -1000 -1000/playMusic event1/loadActors MainEvent/warp farmer1 27 69 /warp farmer2 26 67/warp farmer3 32 67/warp farmer4 28 71/faceDirection farmer1 0/faceDirection farmer2 1/faceDirection farmer3 3/faceDirection farmer4 0/pause 1000/viewport 27 67 true/pause 2000/speak Lewis \"{Helper.Translation.Get("afterEggHunt-1")}\"/pause 800/playMusic none/speak Lewis \"{Helper.Translation.Get("afterEggHunt-2")}\"/playMusic none/pause 3000/cutscene eggHuntWinner/null/playMusic event1/pause 500/fork AbbyWin/jump Abigail/faceDirection Vincent 3/move Lewis 0 1 2/speak Lewis \"{Helper.Translation.Get("afterEggHunt-3")}\"/awardFestivalPrize/null/move Lewis 0 -1 0/speak Lewis \"{Helper.Translation.Get("afterEggHunt-4")}\"/pause 600/viewport move 1 0 5000/pause 2000/globalFade/viewport -1000 -1000/waitForOtherPlayers festivalEnd/end";
                    dict["AbbyWin"] = $"pause 400/speak Lewis \"{Helper.Translation.Get("afterEggHunt-4")}\"/pause 1000/viewport move 1 -1 5000/pause 2000/globalFade/viewport -1000 -1000/waitForOtherPlayers festivalEnd/end";
                    dict["Maru_y2"] = Helper.Translation.Get("Maru_y2");
                    dict["Sebastian_spouse"] = Helper.Translation.Get("Sebastian_spouse");
                    dict["Jas_y2"] = Helper.Translation.Get("Jas_y2");
                });
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
                name: () => ModEntry.SHelper.Translation.Get("GMCM_Option_ModEnabled_Name"),
                getValue: () => Config.ModEnabled,
                setValue: value => Config.ModEnabled = value
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => ModEntry.SHelper.Translation.Get("GMCM_Option_EggsToWin_Name"),
                getValue: () => Config.EggsToWin,
                setValue: value => Config.EggsToWin = value
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => ModEntry.SHelper.Translation.Get("GMCM_Option_NPCMinEggs_Name"),
                getValue: () => Config.NPCMinEggs,
                setValue: value => Config.NPCMinEggs = value
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => ModEntry.SHelper.Translation.Get("GMCM_Option_NPCMaxEggs_Name"),
                getValue: () => Config.NPCMaxEggs,
                setValue: value => Config.NPCMaxEggs = value
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => ModEntry.SHelper.Translation.Get("GMCM_Option_PointsPerEgg_Name"),
                getValue: () => Config.PointsPerEgg,
                setValue: value => Config.PointsPerEgg = value
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => ModEntry.SHelper.Translation.Get("GMCM_Option_EggsPerTalk_Name"),
                getValue: () => Config.EggsPerTalk,
                setValue: value => Config.EggsPerTalk = value
            );

        }
    }
}