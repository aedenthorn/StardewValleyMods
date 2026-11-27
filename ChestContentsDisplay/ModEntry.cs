using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Objects;
using System;

namespace ChestContentsDisplay
{
    public partial class ModEntry : Mod
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;

        public static ModEntry context;

        public static int hoverFrames;
        public static Vector2 lastTile;

        public override void Entry(IModHelper helper)
        {
            Config = Helper.ReadConfig<ModConfig>();

            context = this;

            SMonitor = Monitor;
            SHelper = helper;

            Helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            Helper.Events.Display.RenderedWorld += Display_RenderedWorld;

            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();
        }

        private void Display_RenderedWorld(object sender, StardewModdingAPI.Events.RenderedWorldEventArgs e)
        {
            if (!Config.ModEnabled || (Config.RequireKeyPress && !Config.PressKeys.IsDown()) || !Context.IsPlayerFree || !Game1.currentLocation.Objects.TryGetValue(new Vector2(Game1.currentCursorTile.X, Game1.currentCursorTile.Y), out var obj) || obj is not Chest)
            {
                hoverFrames = 0;
                return;
            }
            Config.PauseFrames = 30;
            if(lastTile != Game1.currentCursorTile)
            {
                hoverFrames = 0;
            }
            lastTile = Game1.currentCursorTile;
            if (hoverFrames++ < Config.PauseFrames)
                return;
            Chest chest = obj as Chest;
            if ((int)AccessTools.Field(typeof(Chest), "currentLidFrame").GetValue(chest) != chest.startingLidFrame.Value) 
                return;

            int cellSize = 72;
            int boxMargin = 72;
            int capacity = chest.GetActualCapacity();
            int rows = (int)Math.Ceiling(capacity / (float)Config.Width);
            Config.Width = 12;
            int width = cellSize * Config.Width + boxMargin;
            int height = cellSize * (rows + 1) + boxMargin;
            Color tint = Color.White;

            Vector2 tileScreenPos = Game1.GlobalToLocal(Game1.currentCursorTile * 64);
            Point pos = new Point(MathHelper.Clamp((int)tileScreenPos.X - width / 2 + cellSize / 2, 0, Game1.viewport.Width - width), MathHelper.Clamp((int)tileScreenPos.Y - height, 0, Game1.viewport.Height - height));
            Point gridPos = pos + new Point(boxMargin / 2, boxMargin / 2 + cellSize);


            Config.ShowTarget = true;
            if (Config.ShowTarget)
            {
                e.SpriteBatch.Draw(Game1.mouseCursors, new Rectangle((int)tileScreenPos.X, (int)tileScreenPos.Y - 16, 64, 64), new Rectangle(652, 204, 44, 44), Color.White * 0.75f);
            }

            Game1.drawDialogueBox(pos.X, pos.Y, width, height, false, true, null, false, true);
            for (int i = 0; i < capacity; i++)
            {
                Vector2 toDraw = new Vector2(gridPos.X + (i % Config.Width) * cellSize, gridPos.Y + (i / Config.Width) * cellSize);
                e.SpriteBatch.Draw(Game1.menuTexture, toDraw, new Rectangle?(Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 10, -1, -1)), tint, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.5f);
                if (chest.items.Count > i && chest.items[i] != null)
                {
                    chest.items[i].drawInMenu(e.SpriteBatch, toDraw, 1f, 1f, 0.865f, StackDrawType.Draw, Color.White, true);
                }
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
                    name: () => SHelper.Translation.Get("GMCM_Option_ModEnabled_Name"),
                    getValue: () => Config.ModEnabled,
                    setValue: value => Config.ModEnabled = value
                );

                configMenu.AddBoolOption(
                    mod: ModManifest,
                    name: () => SHelper.Translation.Get("GMCM_Option_RequireKeyPress_Name"),
                    getValue: () => Config.RequireKeyPress,
                    setValue: value => Config.RequireKeyPress = value
                );

                configMenu.AddKeybindList(
                    mod: ModManifest,
                    name: () => SHelper.Translation.Get("GMCM_Option_PressKeys_Name"),
                    getValue: () => Config.PressKeys,
                    setValue: value => Config.PressKeys = value
                );

            }
        }
    }
}