using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
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
        public static Dictionary<string, EnvelopeData> envelopeData = new Dictionary<string, EnvelopeData>();
        public static Dictionary<string, EnvelopeData> npcEnvelopeData = new Dictionary<string, EnvelopeData>();
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
            Helper.Events.GameLoop.DayStarted += GameLoop_DayStarted;
            Helper.Events.Content.AssetRequested += Content_AssetRequested;

            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();
        }

        private void GameLoop_DayStarted(object sender, StardewModdingAPI.Events.DayStartedEventArgs e)
        {
            npcEnvelopeData = Helper.GameContent.Load<Dictionary<string, EnvelopeData>>(npcPath);
            foreach (var key in npcEnvelopeData.Keys.ToArray())
            {
                if (!string.IsNullOrEmpty(npcEnvelopeData[key].texturePath))
                    npcEnvelopeData[key].texture = Helper.GameContent.Load<Texture2D>(npcEnvelopeData[key].texturePath);
            }
            envelopeData = Helper.GameContent.Load<Dictionary<string, EnvelopeData>>(mailPath);
            foreach (var key in envelopeData.Keys.ToArray())
            {
                if(!string.IsNullOrEmpty(envelopeData[key].texturePath))
                    envelopeData[key].texture = Helper.GameContent.Load<Texture2D>(envelopeData[key].texturePath);
            }
            if (!envelopeData.ContainsKey("default"))
            {
                envelopeData["default"] = new EnvelopeData() { 
                    texture = Helper.ModContent.Load<Texture2D>(Path.Combine("assets", "envelope.png")), 
                    scale = 1, 
                };
            }
            Monitor.Log($"npc envelopes: {npcEnvelopeData.Count}, mail data: {envelopeData.Count}");
        }

        private void Content_AssetRequested(object sender, StardewModdingAPI.Events.AssetRequestedEventArgs e)
        {

            if (e.NameWithoutLocale.IsEquivalentTo(npcPath))
            {
                e.LoadFrom(() => new Dictionary<string, EnvelopeData>(), StardewModdingAPI.Events.AssetLoadPriority.Exclusive);
            }
            else if (e.NameWithoutLocale.IsEquivalentTo(mailPath))
            {
                e.LoadFrom(() => new Dictionary<string, EnvelopeData>(), StardewModdingAPI.Events.AssetLoadPriority.Exclusive);
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

            configMenu.AddTextOption(
                mod: ModManifest,
                name: () => "Inbox Text",
                getValue: () => Config.InboxText,
                setValue: value => Config.InboxText = value
            );
            configMenu.AddTextOption(
                mod: ModManifest,
                name: () => "Archive Text",
                getValue: () => Config.ArchiveText,
                setValue: value => Config.ArchiveText = value
            );
        }
    }
}