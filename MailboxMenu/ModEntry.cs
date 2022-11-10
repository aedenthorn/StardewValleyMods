using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Permissions;
using Object = StardewValley.Object;

namespace MailboxMenu
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry : Mod
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;

        public static ModEntry context;

        public static string npcPath = "aedenthorn.MailboxMenu/npcs";
        public static string mailPath = "aedenthorn.MailboxMenu/mail";
        public static Dictionary<string, MailData> mailDataDict = new Dictionary<string, MailData>();
        public static Dictionary<string, Texture2D> npcEnvelopeTextures = new Dictionary<string, Texture2D>();
        public static int whichTab;

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            Config = Helper.ReadConfig<ModConfig>();

            if (!Config.ModEnabled)
                return;

            context = this;

            SMonitor = Monitor;
            SHelper = helper;

            Helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            Helper.Events.GameLoop.SaveLoaded += GameLoop_SaveLoaded;
            Helper.Events.Content.AssetRequested += Content_AssetRequested;

            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();
        }

        private void GameLoop_SaveLoaded(object sender, StardewModdingAPI.Events.SaveLoadedEventArgs e)
        {
            npcEnvelopeTextures.Clear();
            var data = Helper.GameContent.Load<Dictionary<string, string>>(npcPath);
            foreach(var kvp in data)
            {
                npcEnvelopeTextures[kvp.Key] = Helper.GameContent.Load<Texture2D>(kvp.Value);
            }
            mailDataDict = Helper.GameContent.Load<Dictionary<string, MailData>>(mailPath);
            foreach (var key in mailDataDict.Keys.ToArray())
            {
                if(!string.IsNullOrEmpty(mailDataDict[key].texturePath))
                    mailDataDict[key].texture = Helper.GameContent.Load<Texture2D>(mailDataDict[key].texturePath);
            }
            if (!mailDataDict.ContainsKey("default"))
            {
                mailDataDict["default"] = new MailData() { 
                    texture = Helper.ModContent.Load<Texture2D>(Path.Combine("assets", "envelope.png")), 
                    scale = 16, 
                };
            }
            Monitor.Log($"npc envelopes: {npcEnvelopeTextures.Count}, mail data: {mailDataDict.Count}");
        }

        private void Content_AssetRequested(object sender, StardewModdingAPI.Events.AssetRequestedEventArgs e)
        {

            if (e.NameWithoutLocale.IsEquivalentTo(npcPath))
            {
                e.LoadFrom(() => new Dictionary<string, string>(), StardewModdingAPI.Events.AssetLoadPriority.Exclusive);
            }
            else if (e.NameWithoutLocale.IsEquivalentTo(mailPath))
            {
                e.LoadFrom(() => new Dictionary<string, MailData>(), StardewModdingAPI.Events.AssetLoadPriority.Exclusive);
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