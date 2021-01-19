using Harmony;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using Object = StardewValley.Object;

namespace OverworldChests
{
    public class ModEntry : Mod
	{
		public static ModEntry context;

		public static ModConfig Config;
        private List<string> niceNPCs = new List<string>();
		public static ITreasureChestsExpandedApi treasureChestsExpandedApi = null;
        private Random myRand;
		private Color[] tintColors = new Color[]
		{
			Color.DarkGray,
			Color.Brown,
			Color.Silver,
			Color.Gold,
			Color.Purple,
		};
        private static string namePrefix = "Overworld Chest";

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
		{
			context = this;
			Config = Helper.ReadConfig<ModConfig>();
			if (!Config.EnableMod)
				return;

			myRand = new Random();

			helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            helper.Events.GameLoop.DayStarted += GameLoop_DayStarted;

			var harmony = HarmonyInstance.Create(this.ModManifest.UniqueID);

			harmony.Patch(
				original: AccessTools.Method(typeof(Chest), nameof(Chest.draw), new Type[] { typeof(SpriteBatch), typeof(int), typeof(int), typeof(float) }),
				prefix: new HarmonyMethod(typeof(ModEntry), nameof(Chest_draw_Prefix))
			);
		}
		private static bool Chest_draw_Prefix(Chest __instance)
		{
			if (!__instance.name.StartsWith(namePrefix) || !Game1.player.currentLocation.overlayObjects.ContainsKey(__instance.tileLocation) || __instance.items.Any() || __instance.coins > 0)
				return true;
			context.Monitor.Log($"removing chest at {__instance.tileLocation}");
			Game1.player.currentLocation.overlayObjects.Remove(__instance.tileLocation);
			return false;
		}

		private void GameLoop_DayStarted(object sender, StardewModdingAPI.Events.DayStartedEventArgs e)
        {
			Monitor.Log($"Total days: {Game1.Date.TotalDays}, days since respawn {Game1.Date.TotalDays % Config.RespawnInterval}");
			if (Config.RespawnInterval > 0 && Game1.Date.TotalDays % Config.RespawnInterval == 0)
			{
				Monitor.Log($"Respawning chests", LogLevel.Debug);

				RespawnChests();
            }
        }

        private void RespawnChests()
        {
			foreach(GameLocation l in Game1.locations)
            {
				if (l is FarmHouse || l is Cabin || (!Config.AllowIndoorSpawns && !l.IsOutdoors))
					continue;

				Monitor.Log($"Respawning chests in {l.name}");
				int rem = 0;
				for (int i = l.overlayObjects.Keys.Count - 1; i >= 0; i--)
                {
					Object obj = l.overlayObjects[l.overlayObjects.Keys.ToArray()[i]];
					if (obj is Chest && obj.Name.StartsWith(namePrefix))
                    {
						rem++;
						l.overlayObjects.Remove(l.overlayObjects.Keys.ToArray()[i]);
                    }
				}
				Monitor.Log($"Removed {rem} chests");
				List<Vector2> freeTiles = new List<Vector2>();
                for (int x = 0; x < l.map.Layers[0].LayerWidth; x++)
                {
                    for (int y = 0; y < l.map.Layers[0].LayerHeight; y++)
                    {
						bool water = false;
						try { water = l.waterTiles[x, y]; } catch { }
						if (!l.isTileOccupiedForPlacement(new Vector2(x, y)) && !water)
							freeTiles.Add(new Vector2(x, y));

                    }
                }
				Monitor.Log($"Got {freeTiles.Count} free tiles");

				// shuffle list
				int n = freeTiles.Count;
				while (n > 1)
				{
					n--;
					int k = myRand.Next(n + 1);
					var value = freeTiles[k];
					freeTiles[k] = freeTiles[n];
					freeTiles[n] = value;
				}
				int maxChests = Math.Min(freeTiles.Count, (int)Math.Ceiling(freeTiles.Count * Config.ChestDensity));
				Monitor.Log($"Max chests: {maxChests}");
				for (int i = 0; i < maxChests; i++)
                {
					Chest chest;
					if (treasureChestsExpandedApi == null)
					{
						Monitor.Log($"Adding ordinary chest");
						chest = new Chest(0, new List<Item>() { MineShaft.getTreasureRoomItem() }, freeTiles[i], false, 0);
					}
					else
					{
						double fraction = Math.Pow(myRand.NextDouble(), 1 / Config.ChestsExtendedRarity);
						int level = (int)Math.Ceiling(fraction * Config.ChestsExtentedMaxLevel);
						Monitor.Log($"Adding expanded chest of value {level} to {l.name}");
						chest = treasureChestsExpandedApi.MakeChest(level, freeTiles[i]);
						chest.playerChoiceColor.Value = MakeTint(fraction);
					}
					chest.name = namePrefix;
					chest.modData["Pathoschild.ChestsAnywhere/IsIgnored"] = "true";
					l.overlayObjects[freeTiles[i]] = chest;
				}
			}
        }

        private Color MakeTint(double fraction)
        {
			Color color = tintColors[(int)Math.Floor(fraction * tintColors.Length)];
			return color;
        }

        private void GameLoop_GameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
		{
			treasureChestsExpandedApi = context.Helper.ModRegistry.GetApi<ITreasureChestsExpandedApi>("aedenthorn.TreasureChestsExpanded");
			if (treasureChestsExpandedApi != null)
			{
				Monitor.Log($"loaded TreasureChestsExpanded API", LogLevel.Debug);
			}
		}
	}
}
