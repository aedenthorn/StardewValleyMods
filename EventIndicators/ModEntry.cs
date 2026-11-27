using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Characters;
using StardewValley.GameData.Pets;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Quests;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace EventIndicators
{
	public partial class ModEntry : Mod
	{
		public static IMonitor SMonitor;
		public static IModHelper SHelper;
		public static IManifest SModManifest;
		public static ModConfig Config;
		public static ModEntry context;
		public static Dictionary<Point, string> eventsDict = new Dictionary<Point, string>();
		public static Dictionary<Point, string> eventsDictFull = new Dictionary<Point, string>();

        public override void Entry(IModHelper helper)
		{
			Config = Helper.ReadConfig<ModConfig>();

			context = this;
			SMonitor = Monitor;
			SHelper = helper;
			SModManifest = ModManifest;

			helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            helper.Events.Display.RenderedStep += Display_RenderedStep;
            helper.Events.Player.Warped += Player_Warped;
            Harmony harmony = new Harmony(ModManifest.UniqueID);
			harmony.PatchAll();
        }

        private void Player_Warped(object sender, StardewModdingAPI.Events.WarpedEventArgs e)
        {
            if (Game1.currentLocation?.warps is null)
                return;
            eventsDict.Clear();
            eventsDictFull.Clear();
            foreach (var w in Game1.currentLocation.warps)
            {
                TryAddWarp(w);
            }
            var l = Game1.currentLocation.Map.GetLayer("Buildings");
            if (l != null)
            {
                for (int x = 0; x < l.Tiles.Array.GetLength(0); x++) 
                {
                    for (int y = 0; y < l.Tiles.Array.GetLength(1); y++)
                    {
                        if (l.Tiles[x, y]?.Properties.TryGetValue("Action", out var actionStr) == true)
                        {
                            var action = actionStr.ToString().Split(' ');
                            if (ArgUtility.TryGet(action, 0, out var actionType, out var error, true, "string actionType") && actionType == "LockedDoorWarp")
                            {
                                TryAddWarp(new Warp(x, y, action[3], int.Parse(action[1]), int.Parse(action[2]), false));
                            }
                        }
                    }
                }
            }
        }


        private void Display_RenderedStep(object sender, StardewModdingAPI.Events.RenderedStepEventArgs e)
        {
            if (Config.ModEnabled && Context.IsPlayerFree && e.Step == StardewValley.Mods.RenderSteps.World && Game1.currentLocation != null)
            {
                foreach (var kvp in eventsDict)
                {
                    var rect = new Rectangle(Game1.GlobalToLocal(new Vector2(kvp.Key.X * 64, kvp.Key.Y * 64)).ToPoint(), new Point(64, 64));
                    float yOffset2 = 4f * (float)Math.Round(Math.Sin(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / 250.0), 2);
                    e.SpriteBatch.Draw(Game1.mouseCursors, rect.Location.ToVector2() + new Vector2(28, 40 + yOffset2), new Microsoft.Xna.Framework.Rectangle?(new Rectangle(395, 497, 3, 8)), Color.White, 0f, new Vector2(1f, 4f), 4f + Math.Max(0f, 0.25f - yOffset2 / 16f), SpriteEffects.None, 1f);
                    if (rect.Contains(Game1.getMousePosition()))
                    {
                        if (SHelper.Input.IsDown(Config.ModKey) && eventsDictFull.TryGetValue(kvp.Key, out var data))
                        {
                            IClickableMenu.drawHoverText(e.SpriteBatch, data, Game1.smallFont, boldTitleText: kvp.Value);
                        }
                        else
                        {
                            IClickableMenu.drawHoverText(e.SpriteBatch, kvp.Value, Game1.smallFont, boldTitleText: SHelper.Translation.Get("event"));
                        }
                    }
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
                // Main section
                gmcm.AddKeybind(
					mod: ModManifest,
					name: () => SHelper.Translation.Get("GMCM.ModEnabled.Name"),
					getValue: () => Config.ModKey,
					setValue: value => Config.ModKey = value
				);
			}
		}
	}
}
