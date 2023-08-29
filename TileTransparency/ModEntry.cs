using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using xTile.Tiles;

namespace TileTransparency
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

            context = this;

            SMonitor = Monitor;
            SHelper = helper;

            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.Patch(
                original: AccessTools.Method(helper.GetType().Assembly.GetType("StardewModdingAPI.Framework.Rendering.SDisplayDevice"), "DrawTile"),
                prefix: new HarmonyMethod(typeof(ModEntry), nameof(DrawTile_Prefix)),
                postfix: new HarmonyMethod(typeof(ModEntry), nameof(DrawTile_Postfix))
            );

            helper.Events.Player.Warped += Player_Warped;

        }


        private void Player_Warped(object sender, StardewModdingAPI.Events.WarpedEventArgs e)
        {
            if (Game1.mapDisplayDevice is null)
                return;
            var tileSheetsDict = (Dictionary<TileSheet, Texture2D>)AccessTools.Field(Game1.mapDisplayDevice.GetType(), "m_tileSheetTextures")?.GetValue(Game1.mapDisplayDevice);
            if (tileSheetsDict is null)
                return;
            foreach (TileSheet tileSheet in e.NewLocation.Map.TileSheets)
            {
                if (tileSheetsDict is not null && !tileSheetsDict.ContainsKey(tileSheet))
                {
                    try
                    {
                        tileSheetsDict.Add(tileSheet, SHelper.GameContent.Load<Texture2D>(tileSheet.ImageSource));
                    }
                    catch { }
                }
            }
        }
    }
}