using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Network;
using StardewValley.Objects;
using StardewValley.Projectiles;
using System;
using System.Collections.Generic;
using Object = StardewValley.Object;

namespace CropStacking
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry : Mod
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;

        public static ModEntry context;

        public static string modKey = "aedenthorn.CropStacking";


        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            Config = Helper.ReadConfig<ModConfig>();

            context = this;

            SMonitor = Monitor;
            SHelper = helper;

            helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            //helper.Events.Input.ButtonPressed += Input_ButtonPressed;

            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();

        }

        private void Input_ButtonPressed(object sender, StardewModdingAPI.Events.ButtonPressedEventArgs e)
        {
            //return;
            if(e.Button == SButton.I)
            {
                Crop.TryGetData("425", out var data);
                var f = new ColoredObject("595", Game1.random.Next(1, 10), Utility.StringToColor(data.TintColors[Game1.random.Next(data.TintColors.Count)]).Value);
                Game1.player.addItemToInventory(f);
                f = new ColoredObject("595", Game1.random.Next(1, 10), Utility.StringToColor(data.TintColors[Game1.random.Next(data.TintColors.Count)]).Value);
                f.Quality = 1;
                Game1.player.addItemToInventory(f);
                f = new ColoredObject("595", Game1.random.Next(1, 10), Utility.StringToColor(data.TintColors[Game1.random.Next(data.TintColors.Count)]).Value);
                f.Quality = 2;
                Game1.player.addItemToInventory(f);
                f = new ColoredObject("595", Game1.random.Next(1, 10), Utility.StringToColor(data.TintColors[Game1.random.Next(data.TintColors.Count)]).Value);
                f.Quality = 4;
                Game1.player.addItemToInventory(f);
                return;
                ObjectDataDefinition objectData = ItemRegistry.GetObjectTypeDefinition();
                var i = objectData.CreateFlavoredItem(Object.PreserveType.Wine, new Object("398", 999, quality: 0));
                i.Stack = 999;
                Game1.player.addItemToInventory(i);
                var j = objectData.CreateFlavoredItem(Object.PreserveType.Wine, new Object("398", 999, quality: 0));
                j.Quality = 1;
                j.Stack = 777;
                Game1.player.addItemToInventory(j);
                var k = objectData.CreateFlavoredItem(Object.PreserveType.Wine, new Object("398", 999, quality: 0));
                k.Quality = 2;
                k.Stack = 555;
                Game1.player.addItemToInventory(k);
                var l = objectData.CreateFlavoredItem(Object.PreserveType.Wine, new Object("398", 999, quality: 0));
                l.Quality = 4;
                l.Stack = 333;
                Game1.player.addItemToInventory(l);
                return;
                Game1.player.addItemToInventory(new ColoredObject("595", 4, Color.Purple));
                Game1.player.addItemToInventory(new ColoredObject("595", 5, Color.Green));
                Game1.player.addItemToInventory(new ColoredObject("595", 2, Color.Yellow));
                Game1.player.addItemToInventory(new ColoredObject("595", 7, Color.Blue));
                //Game1.player.addItemToInventory(new Object("190", 69));
                //Game1.player.addItemToInventory(new Object("190", 42, quality: 1));
                //Game1.player.addItemToInventory(new Object("190", 16, quality: 2));
                //Game1.player.addItemToInventory(new Object("190", 5, quality: 4));
            }
        }

        private void GameLoop_GameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
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
                    name: () => Helper.Translation.Get("GMCM_Option_ModEnabled_Name"),
                    getValue: () => Config.ModEnabled,
                    setValue: value => Config.ModEnabled = value
                );

                configMenu.AddKeybindList(
                    mod: ModManifest,
                    name: () => Helper.Translation.Get("GMCM_Option_CombineKey_Name"),
                    getValue: () => Config.CombineKey,
                    setValue: value => Config.CombineKey = value
                );

            }

        }
    }
}