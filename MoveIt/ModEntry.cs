using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using StardewModdingAPI;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Buildings;
using StardewValley.Locations;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using Object = StardewValley.Object;

namespace MoveIt
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry : Mod
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;

        public static ModEntry context;

        public static object movingObject;
        public static Vector2 movingTile;
        public static Vector2 movingOffset;

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
            helper.Events.Display.MenuChanged += Display_MenuChanged;
            helper.Events.Input.ButtonPressed += Input_ButtonPressed;
            helper.Events.Display.RenderedWorld += Display_RenderedWorld;

            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();

        }
        private void Display_RenderedWorld(object sender, StardewModdingAPI.Events.RenderedWorldEventArgs e)
        {
            if (!Config.ModEnabled)
            {
                movingObject = null;
                return;
            }
            if (movingObject is null)
                return;
            try
            {
                if (movingObject is ResourceClump)
                {
                    Rectangle sourceRect = Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, (movingObject as ResourceClump).parentSheetIndex.Value, 16, 16);
                    sourceRect.Width = (movingObject as ResourceClump).width.Value * 16;
                    sourceRect.Height = (movingObject as ResourceClump).height.Value * 16;
                    e.SpriteBatch.Draw(Game1.objectSpriteSheet, Game1.getMousePosition().ToVector2(), sourceRect, Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1);
                }
                else if (movingObject is TerrainFeature)
                {
                    (movingObject as TerrainFeature).draw(e.SpriteBatch, Game1.currentCursorTile);
                }
                else if (movingObject is Object)
                {
                    (movingObject as Object).draw(e.SpriteBatch, Game1.getMouseX() + Game1.viewport.X - 32, Game1.getMouseY() + Game1.viewport.Y - ((movingObject as Object).bigCraftable.Value ? 64 : 32), 1, 1);
                }
                else if (movingObject is Character)
                {
                    Rectangle box = (movingObject as Character).GetBoundingBox();
                    (movingObject as Character).Sprite.draw(e.SpriteBatch, new Vector2(Game1.getMouseX() - 32, Game1.getMouseY() - 32) + new Vector2((float)((movingObject as Character).GetSpriteWidthForPositioning() * 4 / 2), (float)(box.Height / 2)), (float)box.Center.Y / 10000f, 0, (movingObject as Character).ySourceRectOffset, Color.White, false, 4f, 0f, true);
                }
                else if (movingObject is Building)
                {
                    var building = (movingObject as Building);
                    var x = (int)Math.Round(Game1.currentCursorTile.X - movingOffset.X / 64);
                    var y = (int)Math.Round(Game1.currentCursorTile.Y - movingOffset.Y / 64);
                    
                    for (int x_offset = 0; x_offset < building.tilesWide.Value; x_offset++)
                    {
                        for (int y_offset = 0; y_offset < building.tilesHigh.Value; y_offset++)
                        {
                            e.SpriteBatch.Draw(Game1.mouseCursors, new Vector2((float)((x + x_offset) * 64 - Game1.viewport.X), (float)((y + y_offset) * 64 - Game1.viewport.Y)), new Microsoft.Xna.Framework.Rectangle?(new Rectangle(194, 388, 16, 16)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.01f);
                        }
                    }
                    //e.SpriteBatch.Draw(building.texture.Value, new Vector2(Game1.getMouseX() - movingOffset.X, Game1.getMouseY() + building.tilesHigh.Value * 64 - movingOffset.Y), new Rectangle?(building.getSourceRect()), building.color.Value, 0f, new Vector2(0f, (float)building.getSourceRect().Height), 4f, SpriteEffects.None, 1);
                }
            }
            catch { }
        }

        private void Input_ButtonPressed(object sender, StardewModdingAPI.Events.ButtonPressedEventArgs e)
        {
            if (!Config.ModEnabled || !Context.IsPlayerFree)
                return;
            if(e.Button == Config.CancelKey && movingObject is not null)
            {
                PlaySound();
                movingObject = null;
                Helper.Input.Suppress(e.Button);
                return;
            }
            if (e.Button == Config.MoveKey)
            {
                PickupObject();
            }
        }

        private void Display_MenuChanged(object sender, StardewModdingAPI.Events.MenuChangedEventArgs e)
        {
            movingObject = null;
        }

        private void Player_Warped(object sender, StardewModdingAPI.Events.WarpedEventArgs e)
        {
            if (e.Player.IsMainPlayer)
                movingObject = null;
        }

        private void GameLoop_SaveLoaded(object sender, StardewModdingAPI.Events.SaveLoadedEventArgs e)
        {
            movingObject = null;
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
            configMenu.AddKeybind(
                mod: ModManifest,
                name: () => "Mod Key",
                getValue: () => Config.ModKey,
                setValue: value => Config.ModKey = value
            );
            configMenu.AddKeybind(
                mod: ModManifest,
                name: () => "Move Key",
                getValue: () => Config.MoveKey,
                setValue: value => Config.MoveKey = value
            );
            configMenu.AddKeybind(
                mod: ModManifest,
                name: () => "Cancel Key",
                getValue: () => Config.CancelKey,
                setValue: value => Config.CancelKey = value
            );
            configMenu.AddTextOption(
                mod: ModManifest,
                name: () => "Sound",
                getValue: () => Config.Sound,
                setValue: value => Config.Sound = value
            );
        }

    }
}