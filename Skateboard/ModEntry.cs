using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Object = StardewValley.Object;

namespace Skateboard
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry : Mod
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;

        public static ModEntry context;

        public static readonly string boardKey = "aedenthorn.Skateboard/Board";
        public static readonly string sourceKey = "aedenthorn.Skateboard/SourceRect";
        public static readonly int boardIndex = -42424201;
        public static readonly string skateboardingKey = "aedenthorn.Skateboard/Skateboarding";

        public static bool accelerating;
        private static Texture2D boardTexture;

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

            helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            helper.Events.GameLoop.SaveLoaded += GameLoop_SaveLoaded;
            helper.Events.Player.Warped += Player_Warped;
            helper.Events.Input.ButtonPressed += Input_ButtonPressed;
            helper.Events.Content.AssetRequested += Content_AssetRequested;

            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();
            helper.ConsoleCommands.Add("skateboard", "Spawn a skateboard.", SpawnSkateboard);
        }

        private void Player_Warped(object sender, StardewModdingAPI.Events.WarpedEventArgs e)
        {
            foreach(var key in e.NewLocation.Objects.Keys.ToArray())
            {
                if (e.NewLocation.Objects[key]?.modData.ContainsKey(boardKey) == true)
                {
                    e.NewLocation.Objects.Remove(key);
                }
            }
        }

        private static void SpawnSkateboard(string arg1 = null, string[] arg2 = null)
        {
            var s = new Object(Vector2.Zero, boardIndex, false);
            s.modData[boardKey] = "true";
            s.Type = "Skateboard";
            s.Category = -20;
            if (!Game1.player.addItemToInventoryBool(s, true))
            {
                Game1.createItemDebris(s, Game1.player.Position, 1, Game1.player.currentLocation);
            }
        }

        private void GameLoop_SaveLoaded(object sender, StardewModdingAPI.Events.SaveLoadedEventArgs e)
        {
            boardTexture = Game1.content.Load<Texture2D>(boardKey);
            Monitor.Log("loaded skateboard texture");
        }

        private void Content_AssetRequested(object sender, StardewModdingAPI.Events.AssetRequestedEventArgs e)
        {
            if(e.NameWithoutLocale.IsEquivalentTo(boardKey))
            {
                e.LoadFromModFile<Texture2D>("assets/board.png", StardewModdingAPI.Events.AssetLoadPriority.Low);
            }
            else if (e.NameWithoutLocale.IsEquivalentTo("Data/CraftingRecipes"))
            {
                e.Edit(AddSkateBoardRecipe);
            }
            else if (e.NameWithoutLocale.IsEquivalentTo("Data/BigCraftablesInformation"))
            {
                e.Edit(AddSkateBoardInfo);
            }
        }

        private void AddSkateBoardRecipe(IAssetData obj)
        {
            IDictionary<string, string> data = obj.AsDictionary<string, string>().Data;
            data.Add("Skateboard", $"{Config.CraftingRequirements}/Home/{boardIndex}/true/{Helper.Translation.Get("name")}");
        }
        private void AddSkateBoardInfo(IAssetData obj)
        {
            IDictionary<int, string> data = obj.AsDictionary<int, string>().Data;
            data.Add(boardIndex, $"{Helper.Translation.Get("name")}/5000/-300/Crafting -9/{Helper.Translation.Get("description")}/true/true/0/{Helper.Translation.Get("name")}");
        }

        private void Input_ButtonPressed(object sender, StardewModdingAPI.Events.ButtonPressedEventArgs e)
        {
            if(Config.ModEnabled && Context.CanPlayerMove && e.Button == Config.RideButton && !Game1.currentLocation.isActionableTile((int)Game1.currentCursorTile.X, (int)Game1.currentCursorTile.Y, Game1.player))
            {
                if (Game1.player.modData.ContainsKey(skateboardingKey))
                {
                    SpawnSkateboard();
                    speed = Vector2.Zero;
                    Game1.player.modData.Remove(skateboardingKey);
                    Game1.player.drawOffset.Value = Vector2.Zero;
                }
                else if(Game1.player.CurrentItem is not null && Game1.player.CurrentItem.modData.ContainsKey(boardKey))
                {
                    Game1.player.reduceActiveItemByOne();
                    speed = Vector2.Zero;
                    Game1.player.modData[skateboardingKey] = "true";
                }
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
                name: () => "Mod Enabled?",
                getValue: () => Config.ModEnabled,
                setValue: value => Config.ModEnabled = value
            );
            configMenu.AddKeybind(
                mod: ModManifest,
                name: () => "Ride Button",
                getValue: () => Config.RideButton,
                setValue: value => Config.RideButton = value
            );
            configMenu.AddTextOption(
                mod: ModManifest,
                name: () => "Max Speed",
                getValue: () => Config.MaxSpeed +"",
                setValue: delegate(string value) { try { Config.MaxSpeed = float.Parse(value, NumberStyles.Any, CultureInfo.InvariantCulture); } catch { } }
            );
            configMenu.AddTextOption(
                mod: ModManifest,
                name: () => "Accel Rate",
                getValue: () => Config.Acceleration + "",
                setValue: delegate(string value) { try { Config.Acceleration = float.Parse(value, NumberStyles.Any, CultureInfo.InvariantCulture); } catch { } }
            );
            configMenu.AddTextOption(
                mod: ModManifest,
                name: () => "Decel Rate",
                getValue: () => Config.Deceleration+ "",
                setValue: delegate(string value) { try { Config.Deceleration = float.Parse(value, NumberStyles.Any, CultureInfo.InvariantCulture); } catch { } }
            );
            configMenu.AddTextOption(
                mod: ModManifest,
                name: () => "Crafting Reqs",
                getValue: () => Config.CraftingRequirements + "",
                setValue: value => Config.CraftingRequirements = value
            );
        }
    }
}