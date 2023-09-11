using Force.DeepCloner;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BuffFramework
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry : Mod
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;

        public static ModEntry context;
        public static string dictKey = "aedenthorn.BuffFramework/dictionary";
        public static Dictionary<string, Dictionary<string, object>> buffDict = new();
        public static PerScreen<Dictionary<string, Buff>> farmerBuffs = new();
        public static Dictionary<string, ICue> cues = new();

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            Config = Helper.ReadConfig<ModConfig>();

            context = this;

            SMonitor = Monitor;
            SHelper = helper;

            Helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            Helper.Events.GameLoop.DayStarted += GameLoop_DayStarted;
            Helper.Events.Player.Warped += Player_Warped;
            
            Helper.Events.GameLoop.TimeChanged += GameLoop_TimeChanged;
            Helper.Events.GameLoop.OneSecondUpdateTicked += GameLoop_OneSecondUpdateTicked;
            Helper.Events.GameLoop.SaveLoaded += GameLoop_SaveLoaded;
            Helper.Events.Content.AssetRequested += Content_AssetRequested;
            Helper.Events.GameLoop.ReturnedToTitle += GameLoop_ReturnedToTitle;

            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();
            return;
            var dict = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, string>>>(File.ReadAllText(Path.Combine(SHelper.DirectoryPath, "content.json")));
            var outDict = new Dictionary<string, string>();
            foreach(var k in dict.Keys.ToArray())
            {
                var key = "clothing-buff-"+k.Split('/')[1].Replace("Buff", "").ToLower();
                outDict[key] = dict[k]["displaySource"];
                dict[k]["displaySource"] = $"{{{{i18n: {key}}}}}";
                if (dict[k].TryGetValue("description", out var desc))
                {
                    outDict[key + "-desc"] = desc;
                    dict[k]["description"] = $"{{{{i18n: {key + "-desc"}}}}}";
                }
            }
            File.WriteAllText(Path.Combine(SHelper.DirectoryPath, "out.json"), JsonConvert.SerializeObject(outDict, Formatting.Indented));
            File.WriteAllText(Path.Combine(SHelper.DirectoryPath, "out2.json"), JsonConvert.SerializeObject(dict, Formatting.Indented));
        }

        public override object GetApi()
        {
            return new BuffFrameworkAPI();
        }

        public void GameLoop_OneSecondUpdateTicked(object sender, StardewModdingAPI.Events.OneSecondUpdateTickedEventArgs e)
        {
            if(!Config.ModEnabled || !Context.IsPlayerFree) 
                return;
            foreach(var key in farmerBuffs.Value.Keys)
            {
                if (buffDict[key].TryGetValue("healthRegen", out var healthRegen))
                {
                    Game1.player.health = MathHelper.Clamp(Game1.player.health + GetInt(healthRegen), 0, Game1.player.maxHealth);
                }
                if(buffDict[key].TryGetValue("staminaRegen", out var staminaRegen))
                {
                    Game1.player.Stamina = MathHelper.Clamp(Game1.player.Stamina + GetInt(staminaRegen), 0, Game1.player.MaxStamina);
                }
            }
        }

        public void GameLoop_ReturnedToTitle(object sender, StardewModdingAPI.Events.ReturnedToTitleEventArgs e)
        {
            ClearCues();
        }

        public void GameLoop_TimeChanged(object sender, StardewModdingAPI.Events.TimeChangedEventArgs e)
        {
            Helper.Events.GameLoop.UpdateTicking += GameLoop_UpdateTicking;
        }

        public void Player_Warped(object sender, StardewModdingAPI.Events.WarpedEventArgs e)
        {
            Helper.Events.GameLoop.UpdateTicking += GameLoop_UpdateTicking;
        }

        public void GameLoop_DayStarted(object sender, StardewModdingAPI.Events.DayStartedEventArgs e)
        {
            Helper.Events.GameLoop.UpdateTicking += GameLoop_UpdateTicking;
        }

        public void GameLoop_UpdateTicking(object sender, StardewModdingAPI.Events.UpdateTickingEventArgs e)
        {
            UpdateBuffs();
            Helper.Events.GameLoop.UpdateTicking -= GameLoop_UpdateTicking;
        }

        public void Content_AssetRequested(object sender, StardewModdingAPI.Events.AssetRequestedEventArgs e)
        {
            if (e.NameWithoutLocale.IsEquivalentTo(dictKey))
            {
                e.LoadFrom(() => new Dictionary<string, Dictionary<string, object>>(), StardewModdingAPI.Events.AssetLoadPriority.Exclusive);
            }
        }

        public void GameLoop_SaveLoaded(object sender, StardewModdingAPI.Events.SaveLoadedEventArgs e)
        {
            farmerBuffs.Value = new();
        }

        public void GameLoop_GameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
        {


            // get Generic Mod Config Menu's API (if it's installed)
            var configMenu = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (configMenu is not null)
            {
                // register mod
                configMenu.Register(
                    mod: ModManifest,
                    reset: () => Config = new ModConfig(),
                    save: () => Helper.WriteConfig(Config)
                );

                configMenu.AddBoolOption(
                    mod: ModManifest,
                    name: () => SHelper.Translation.Get("GMCM_Option_ModEnabled_Name"),
                    getValue: () => Config.ModEnabled,
                    setValue: value => Config.ModEnabled = value
                );
            }
        }
    }
}