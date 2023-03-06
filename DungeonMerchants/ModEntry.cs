using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Objects;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using xTile;
using xTile.Dimensions;
using xTile.Tiles;

namespace DungeonMerchants
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry : Mod
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;

        public static string dwarfKey = "aedenthorn.DungeonMerchants/dwarfTile";
        public static string merchantKey = "aedenthorn.DungeonMerchants/merchantTile";
        public static ModEntry context;
        
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            Config = Helper.ReadConfig<ModConfig>();

            context = this;

            SMonitor = Monitor;
            SHelper = helper;

            Helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            Helper.Events.Player.Warped += Player_Warped;

            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();
        }

        private void Player_Warped(object sender, StardewModdingAPI.Events.WarpedEventArgs e)
        {
            if (!Config.ModEnabled || !e.NewLocation.modData.TryGetValue(merchantKey, out string tileString) || e.NewLocation.getTemporarySpriteByID(merchantSpriteID) is not null)
                return;
            var split = tileString.Split(',');
            Point tile = new Point(int.Parse(split[0]), int.Parse(split[1]));
            var pos = new Vector2(tile.X * 64 + 2, tile.Y * 64 - 32 - 9);
            e.NewLocation.temporarySprites.Add(new TemporaryAnimatedSprite
            {
                texture = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\temporary_sprites_1"),
                sourceRect = new Microsoft.Xna.Framework.Rectangle(0, 614, 20, 26),
                sourceRectStartingPos = new Vector2(0f, 614f),
                animationLength = 1,
                totalNumberOfLoops = 99999,
                interval = 99999f,
                scale = 3f,
                position = pos,
                layerDepth = (pos.Y) / 10000f,
                id = merchantSpriteID,
                destroyable = false
            });
            e.NewLocation.temporarySprites.Add(new TemporaryAnimatedSprite
            {
                texture = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\temporary_sprites_1"),
                sourceRect = new Microsoft.Xna.Framework.Rectangle(132, 561, 26, 43),
                sourceRectStartingPos = new Vector2(132f, 561f),
                animationLength = 1,
                totalNumberOfLoops = 99999,
                interval = 99999f,
                scale = 3f,
                position = pos - new Vector2(9, 24),
                layerDepth = (pos.Y - 1) / 10000f,
                id = merchantSpriteID - 1,
                color = Color.DarkGray,
                destroyable = false
            });
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
                name: () => "Dwarf Floor Min",
                getValue: () => Config.DwarfFloorMin,
                setValue: value => Config.DwarfFloorMin = value
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Dwarf Floor Max",
                getValue: () => Config.DwarfFloorMax,
                setValue: value => Config.DwarfFloorMax = value
            );
            configMenu.AddTextOption(
                mod: ModManifest,
                name: () => "Dwarf Floor Chance %",
                tooltip: () => "Percent chance to spawn dwarf on any floor",
                getValue: () => Config.DwarfFloorChancePercent + "",
                setValue: delegate(string value) { if (float.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out float f)) { Config.DwarfFloorChancePercent = f; } } 
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Dwarf Floor Mult",
                tooltip: () => "Spawn dwarf on floors that are multiples of this number",
                getValue: () => Config.DwarfFloorMult,
                setValue: value => Config.DwarfFloorMult = value
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Merchant Floor Min",
                getValue: () => Config.MerchantFloorMin,
                setValue: value => Config.MerchantFloorMin = value
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Merchant Floor Max",
                getValue: () => Config.MerchantFloorMax,
                setValue: value => Config.MerchantFloorMax = value
            );
            configMenu.AddTextOption(
                mod: ModManifest,
                name: () => "Merchant Floor Chance %",
                tooltip: () => "Percent chance to spawn merchant on any floor",
                getValue: () => Config.MerchantFloorChancePercent + "",
                setValue: delegate(string value) { if (float.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out float f)) { Config.MerchantFloorChancePercent = f; } } 
            );
            configMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Merchant Floor Mult",
                tooltip: () => "Spawn merchant on floors that are multiples of this number",
                getValue: () => Config.MerchantFloorMult,
                setValue: value => Config.MerchantFloorMult = value
            );
        }
    }
}