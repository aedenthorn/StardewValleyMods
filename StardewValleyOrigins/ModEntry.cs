using HarmonyLib;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StardewValleyOrigins
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry : Mod
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;

        public static ModEntry context;
        
        public static string StardewValleyOriginsKey = "aedenthorn.StardewValleyOrigins";

        public static List<string> allowedNPCs = new();
        public static List<string> allowedEvents = new();
        public static List<string> allowedMail = new();
        public static List<int> allowedMapPoints = new();
        public static bool shippingBin;
        public static bool bus;
        public static bool minecarts;
        public static bool blacksmith;
        public static bool farmHouse;
        public static bool marniesLivestock;
        public static bool townBoard;
        public static bool specialOrdersBoard;
        public static bool linusCampfire;

        public static Dictionary<string, WorldStateData> worldStateDict= new();

        //public static string dictPath = "aedenthorn.StardewValleyOrigins/dictionary";
        //public static Dictionary<string, StardewValleyOriginsData> dataDict = new();

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            Config = Helper.ReadConfig<ModConfig>();

            context = this;

            SMonitor = Monitor;
            SHelper = helper;

            helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            helper.Events.GameLoop.SaveLoaded += GameLoop_SaveLoaded;
            helper.Events.GameLoop.DayStarted += GameLoop_DayStarted;

            helper.Events.Content.AssetRequested += Content_AssetRequested;

            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();

        }

        private void GameLoop_DayStarted(object sender, StardewModdingAPI.Events.DayStartedEventArgs e)
        {
            if(!Config.ModEnabled) 
                return;
            for(int i = Game1.mailbox.Count - 1; i >= 0; i--)
            {
                if (!allowedMail.Contains(Game1.mailbox[i]))
                {
                    Game1.mailbox.RemoveAt(i);
                }
            }
        }

        private void GameLoop_SaveLoaded(object sender, StardewModdingAPI.Events.SaveLoadedEventArgs e)
        {
            worldStateDict = Helper.GameContent.Load<Dictionary<string, WorldStateData>>(StardewValleyOriginsKey);
            GetAllowedWorldState();
            foreach(var l in Game1.locations)
            {
                for(int i = l.characters.Count - 1; i >= 0; i--)
                {
                    if (l.characters[i].isVillager() && !allowedNPCs.Contains(l.characters[i].Name))
                    {
                        l.characters.RemoveAt(i);
                    }
                }
            }
        }

        private void Content_AssetRequested(object sender, StardewModdingAPI.Events.AssetRequestedEventArgs e)
        {
            if (e.NameWithoutLocale.IsEquivalentTo(StardewValleyOriginsKey))
            {
                e.LoadFrom(() => new Dictionary<string, WorldStateData>(), StardewModdingAPI.Events.AssetLoadPriority.Exclusive);
            }
            else if (e.NameWithoutLocale.IsEquivalentTo("Data/Quests"))
            {
                e.Edit(delegate(IAssetData data)
                {
                    data.AsDictionary<int, string>().Data.Remove(9);
                });
            }
            else if (e.NameWithoutLocale.IsEquivalentTo("Strings/StringsFromCSFiles"))
            {
                e.Edit(delegate(IAssetData data)
                {
                    string orig = data.AsDictionary<string, string>().Data["Farmer.cs.1918"];
                    data.AsDictionary<string, string>().Data["Farmer.cs.1918"] = orig.Substring(0, orig.IndexOf('^'));
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
                name: () => "Mod Enabled",
                getValue: () => Config.ModEnabled,
                setValue: value => Config.ModEnabled = value
            );
            
        }
    }
}