using HarmonyLib;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using System;
using System.Collections.Generic;

namespace BirthdayBuff
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry : Mod
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;

        public static ModEntry context;
        public static string dictKey = "aedenthorn.BuffFramework/dictionary";
        public static object hbAPI;

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            Config = Helper.ReadConfig<ModConfig>();

            context = this;

            SMonitor = Monitor;
            SHelper = helper;

            Helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            Helper.Events.Content.AssetRequested += Content_AssetRequested;

            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();
        }

        private void Content_AssetRequested(object sender, StardewModdingAPI.Events.AssetRequestedEventArgs e)
        {
            if (!Config.ModEnabled)
                return;
            if (e.NameWithoutLocale.IsEquivalentTo("aedenthorn.BirthdayBuff/icon"))
            {
                e.LoadFromModFile<Texture2D>("assets/icon.png", StardewModdingAPI.Events.AssetLoadPriority.Low);
            }
            if (e.NameWithoutLocale.IsEquivalentTo(dictKey))
            {
                e.Edit(delegate(IAssetData data)
                {
                    if(Game1.player is not null)
                    {
                        var core = hbAPI.GetType().Assembly.GetType("Omegasis.HappyBirthday.HappyBirthdayModCore");
                        var instance = AccessTools.Field(core, "Instance").GetValue(null);
                        var bmgr = AccessTools.Field(instance.GetType(), "birthdayManager").GetValue(instance);
                        var bdc = (bool)AccessTools.Method(bmgr.GetType(), "hasChosenBirthday").Invoke(bmgr, new object[] { });
                        if (!bdc)
                            return;
                        var bdd = AccessTools.Field(bmgr.GetType(), "playerBirthdayData").GetValue(bmgr);
                        var bdday = (int)AccessTools.Field(bdd.GetType(), "BirthdayDay").GetValue(bdd);
                        var bds = (string)AccessTools.Field(bdd.GetType(), "BirthdaySeason").GetValue(bdd);
                        if(Game1.dayOfMonth == bdday && Game1.currentSeason == bds.ToLower())
                        {
                            data.AsDictionary<string, Dictionary<string, object>>().Data.Add("aedenthorn.BirthdayBuff", new Dictionary<string, object>()
                            {
                                { "separate", Config.ShowSeparate },
                                { "glow", Config.GlowColor },
                                { "glowRate", Config.GlowRate },
                                { "farming", Config.Farming },
                                { "fishing", Config.Fishing },
                                { "mining", Config.Mining },
                                { "luck", Config.Luck },
                                { "foraging", Config.Foraging },
                                { "maxStamina", Config.MaxStamina },
                                { "magneticRadius", Config.MagneticRadius },
                                { "speed", Config.Speed },
                                { "defense", Config.Defense },
                                { "attack", Config.Attack },
                                { "source", "aedenthorn.BirthdayBuff" },
                                { "displaySource", SHelper.Translation.Get("birthday-buff").ToString() },
                                { "texturePath", "aedenthorn.BirthdayBuff/icon" },
                                { "description", GetBuffDescription() },
                                { "sound", Config.Sound },
                                { "buffId", 1890175411 }
                            });
                        }
                    }
                });
            }
        }


        private void GameLoop_DayStarted(object sender, StardewModdingAPI.Events.DayStartedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void GameLoop_GameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
        {

            hbAPI = Helper.ModRegistry.GetApi("Omegasis.HappyBirthday");
            
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
                configMenu.AddBoolOption(
                    mod: ModManifest,
                    name: () => SHelper.Translation.Get("GMCM_Option_ShowSeparate_Name"),
                    getValue: () => Config.ShowSeparate,
                    setValue: value => Config.ShowSeparate = value
                );

                configMenu.AddTextOption(
                    mod: ModManifest,
                    name: () => SHelper.Translation.Get("GMCM_Option_Sound_Name"),
                    getValue: () => Config.Sound,
                    setValue: value => Config.Sound = value
                );

                configMenu.AddNumberOption(
                    mod: ModManifest,
                    name: () => SHelper.Translation.Get("GMCM_Option_Farming_Name"),
                    getValue: () => Config.Farming,
                    setValue: value => Config.Farming = value
                );
                configMenu.AddNumberOption(
                    mod: ModManifest,
                    name: () => SHelper.Translation.Get("GMCM_Option_Fishing_Name"),
                    getValue: () => Config.Fishing,
                    setValue: value => Config.Fishing = value
                );
                configMenu.AddNumberOption(
                    mod: ModManifest,
                    name: () => SHelper.Translation.Get("GMCM_Option_Mining_Name"),
                    getValue: () => Config.Mining,
                    setValue: value => Config.Mining = value
                );
                configMenu.AddNumberOption(
                    mod: ModManifest,
                    name: () => SHelper.Translation.Get("GMCM_Option_Luck_Name"),
                    getValue: () => Config.Luck,
                    setValue: value => Config.Luck = value
                );
                configMenu.AddNumberOption(
                    mod: ModManifest,
                    name: () => SHelper.Translation.Get("GMCM_Option_Foraging_Name"),
                    getValue: () => Config.Foraging,
                    setValue: value => Config.Foraging = value
                );
                configMenu.AddNumberOption(
                    mod: ModManifest,
                    name: () => SHelper.Translation.Get("GMCM_Option_MaxStamina_Name"),
                    getValue: () => Config.MaxStamina,
                    setValue: value => Config.MaxStamina = value
                );
                configMenu.AddNumberOption(
                    mod: ModManifest,
                    name: () => SHelper.Translation.Get("GMCM_Option_MagneticRadius_Name"),
                    getValue: () => Config.MagneticRadius,
                    setValue: value => Config.MagneticRadius = value
                );
                configMenu.AddNumberOption(
                    mod: ModManifest,
                    name: () => SHelper.Translation.Get("GMCM_Option_Speed_Name"),
                    getValue: () => Config.Speed,
                    setValue: value => Config.Speed = value
                );
                configMenu.AddNumberOption(
                    mod: ModManifest,
                    name: () => SHelper.Translation.Get("GMCM_Option_Defense_Name"),
                    getValue: () => Config.Defense,
                    setValue: value => Config.Defense = value
                );
                configMenu.AddNumberOption(
                    mod: ModManifest,
                    name: () => SHelper.Translation.Get("GMCM_Option_Attack_Name"),
                    getValue: () => Config.Attack,
                    setValue: value => Config.Attack = value
                );
            }
        }
    }
}