using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Inventories;
using StardewValley.Menus;
using StardewValley.Objects;
using System;
using System.IO;
using System.Linq;

namespace ChestContentDisplay
{
	public partial class ModEntry : Mod
	{
		public static IMonitor SMonitor;
		public static IModHelper SHelper;
		public static IManifest SModManifest;
		public static ModConfig Config;
		public static ModEntry context;
		public static PerScreen<Vector2> chestTile = new PerScreen<Vector2>();
		public static PerScreen<InventoryMenu> chestMenu = new PerScreen<InventoryMenu>();
		public static PerScreen<bool> offset = new PerScreen<bool>();
		public static PerScreen<bool> facing = new PerScreen<bool>();
		public static PerScreen<float> delay = new PerScreen<float>();
		

        public override void Entry(IModHelper helper)
		{
			Config = Helper.ReadConfig<ModConfig>();

			context = this;
			SMonitor = Monitor;
			SHelper = helper;
			SModManifest = ModManifest;

			helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            helper.Events.Display.RenderedWorld += Display_RenderedWorld;

            //Harmony harmony = new Harmony(ModManifest.UniqueID);
			//harmony.PatchAll();
        }

        private void Display_RenderedWorld(object sender, StardewModdingAPI.Events.RenderedWorldEventArgs e)
        {
			if (!Config.ModEnabled || !Context.CanPlayerMove)
				return;
			if (chestMenu.Value == null)
			{
                if (Config.EnableKey == SButton.None || Helper.Input.IsDown(Config.EnableKey))
				{
					var tile = Game1.currentCursorTile;
					if (Config.WhenHovering && Game1.currentLocation.Objects.TryGetValue(tile, out var obj) && obj is Chest)
					{
						offset.Value = false;
                        facing.Value = false;
                    }
                    else if (Config.WhenHovering && (Game1.getMousePosition() + new Point(Game1.viewport.Location.X, Game1.viewport.Location.Y)).Y % 64 > 32 && Game1.currentLocation.Objects.TryGetValue(tile + new Vector2(0, 1), out obj) && obj is Chest)
					{
						offset.Value = true;
						tile += new Vector2(0, 1);
                        facing.Value = false;
                    }
                    else if (Config.WhenFacing && Game1.currentLocation.Objects.TryGetValue(Game1.player.GetGrabTile(), out obj) && obj is Chest)
                    {
						tile = Game1.player.GetGrabTile();
                        offset.Value = false;
                        facing.Value = true;
                    }
                    else
					{
						return;
                    }

                    Chest chest = obj as Chest;
                    delay.Value = 0;
                    chestTile.Value = tile;
                    int capacity = chest.GetActualCapacity();
                    int rows = ((capacity >= 70) ? 5 : 3);
                    if (capacity < 9)
                    {
                        rows = 1;
                    }
                    chestMenu.Value = new InventoryMenu(0, 0, false, chest.Items, null, capacity, rows, 0, 0, true);
                    Point pos = GetChestPos();
                }
            }
			else
            {
                if (Config.EnableKey != SButton.None && !Helper.Input.IsDown(Config.EnableKey))
				{
                    chestMenu.Value = null;
                }
                else if (!facing.Value && Game1.currentCursorTile != chestTile.Value + (offset.Value ? new Vector2(0, -1) : Vector2.Zero))
                {
					if(offset.Value && Game1.currentCursorTile == chestTile.Value)
					{
						offset.Value = false;
						chestTile.Value = Game1.currentCursorTile;

                    }
					else if(!offset.Value && Game1.currentCursorTile == chestTile.Value + new Vector2(0,-1))
					{
						offset.Value = true;
						chestTile.Value = Game1.currentCursorTile + new Vector2(0, 1);

                    }
					else
					{
						chestMenu.Value = null;
					}
                }
				else if(facing.Value && (facing.Value && Game1.player.GetGrabTile() != chestTile.Value))
				{
                    chestMenu.Value = null;
                }
                if (chestMenu.Value != null)
				{
					if (Config.EnableKey == SButton.None && delay.Value < (facing.Value ? Config.DelayTicksFace : Config.DelayTicksHover))
					{
						delay.Value++;
						return;
					}
					var add = (chestMenu.Value.rows > 3 ? ((chestMenu.Value.rows - 3) * 4) : 0);
					Point pos = GetChestPos() + new Point(0, -add);
					chestMenu.Value.xPositionOnScreen = pos.X;
					chestMenu.Value.yPositionOnScreen = pos.Y;
					Game1.drawDialogueBox(pos.X - 32, pos.Y - 100, chestMenu.Value.width + 64, chestMenu.Value.height + 124 + add, false, true);
					chestMenu.Value.draw(e.SpriteBatch);
					e.SpriteBatch.Draw(Game1.mouseCursors, new Rectangle(pos.X + chestMenu.Value.width / 2 - 19, pos.Y + chestMenu.Value.height + 4 + add, 40, 36), new Rectangle(13, 85, 40, 36), Color.White);
					e.SpriteBatch.Draw(Game1.mouseCursors, new Rectangle(pos.X + chestMenu.Value.width / 2 - 8, pos.Y + chestMenu.Value.height + add, 16, 4), new Rectangle(24, 85, 16, 4), Color.White);
				}
            }
        }

        public void GameLoop_GameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
		{
			// Get Generic Mod Config Menu's API
			var gmcm = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");

            if (gmcm is not null)
			{
				// Register mod
				gmcm.Register(
					mod: ModManifest,
					reset: () => Config = new ModConfig(),
                    save: () => Helper.WriteConfig(Config)
                );
                // Main section
                gmcm.AddBoolOption(
					mod: ModManifest,
					name: () => SHelper.Translation.Get("GMCM.ModEnabled.Name"),
					getValue: () => Config.ModEnabled,
					setValue: value => Config.ModEnabled = value
				);
                gmcm.AddKeybind(
					mod: ModManifest,
					name: () => SHelper.Translation.Get("GMCM.EnableKey.Name"),
					getValue: () => Config.EnableKey,
					setValue: value => Config.EnableKey = value
				);
                gmcm.AddNumberOption(
					mod: ModManifest,
					name: () => SHelper.Translation.Get("GMCM.DelayTicksHover.Name"),
					getValue: () => Config.DelayTicksHover,
					setValue: value => Config.DelayTicksHover = value
				);
                gmcm.AddNumberOption(
					mod: ModManifest,
					name: () => SHelper.Translation.Get("GMCM.DelayTicksFace.Name"),
					getValue: () => Config.DelayTicksFace,
					setValue: value => Config.DelayTicksFace = value
				);
            }
		}
	}

}
