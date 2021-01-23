using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using System.Collections.Generic;
using System.IO;

namespace CustomSpousePatio
{
    public class CustomSpousePatioApi
    {
        public Dictionary<string, object> GetCurrentSpouseAreas()
        {
            return ModEntry.outdoorAreas;
        }
        public Dictionary<string, int[]> GetDefaultSpouseOffsets()
        {
            return ModEntry.spousePatioOffsets;
        }
        public void RemoveAllSpouseAreas()
        {
            ModEntry.RemoveAllSpouseAreas();
        }
        public void ReloadSpouseAreaData()
        {
            ModEntry.LoadSpouseAreaData();
        }
        public void AddTileSheets()
        {
            ModEntry.AddTileSheets();
        }
        public void ShowSpouseAreas()
        {
            ModEntry.ShowSpouseAreas();
        }
		public void ReloadPatios()
		{
			RemoveAllSpouseAreas();
			Game1.getFarm().loadMap(Game1.getFarm().mapPath, true);
			ReloadSpouseAreaData();
			AddTileSheets();
			ShowSpouseAreas();

		}
		public bool MoveSpousePatio(string spouse_dir)
        {
			RemoveAllSpouseAreas();
			Game1.getFarm().loadMap(Game1.getFarm().mapPath, true);
			string spouse = spouse_dir.Split('_')[0];
			string dir = spouse_dir.Split('_')[1];
			bool success = false;
			OutdoorArea outdoorArea = (OutdoorArea)ModEntry.outdoorAreas[spouse];
            switch (dir)
            {
				case "up":
					if (outdoorArea.location.Y <= 0)
						break;
					outdoorArea.location.Y--;
					success = true;
					break;
				case "down":
					if (outdoorArea.location.Y >= Game1.getFarm().map.Layers[0].LayerHeight - 1)
						break;
					outdoorArea.location.Y++;
					success = true;
					break;
				case "left":
					if (outdoorArea.location.X == 0)
						break;
					outdoorArea.location.X--;
					success = true;
					break;
				case "right":
					if (outdoorArea.location.X >= Game1.getFarm().map.Layers[0].LayerWidth - 1)
						break;
					outdoorArea.location.X++;
					success = true;
					break;
            }
			string path = Path.Combine("assets", "outdoor-areas.json");

			if (!File.Exists(Path.Combine(ModEntry.SHelper.DirectoryPath, path)))
				path = "outdoor-areas.json";

			OutdoorAreaData json = ModEntry.SHelper.Data.ReadJsonFile<OutdoorAreaData>(path) ?? new OutdoorAreaData();
			json.areas[spouse] = outdoorArea;
			ModEntry.SHelper.Data.WriteJsonFile(path, json);

			ReloadSpouseAreaData();
			AddTileSheets();
			ShowSpouseAreas();

			ModEntry.SMonitor.Log($"Added spouse {spouse} to {path}");
			return success;
        }

		public bool RemoveSpousePatio(string spouse)
		{
			string path = Path.Combine("assets", "outdoor-areas.json");

			if (!File.Exists(Path.Combine(ModEntry.SHelper.DirectoryPath, path)))
				path = "outdoor-areas.json";
			OutdoorAreaData json = ModEntry.SHelper.Data.ReadJsonFile<OutdoorAreaData>(path) ?? new OutdoorAreaData();
			if (json.areas.ContainsKey(spouse))
			{
				json.areas.Remove(spouse);
				ModEntry.SHelper.Data.WriteJsonFile(path, json);
				ModEntry.SMonitor.Log($"removed spouse {spouse} from {path}");
			}
			else
			{
				foreach (IContentPack contentPack in ModEntry.SHelper.ContentPacks.GetOwned())
				{
					json = contentPack.ReadJsonFile<OutdoorAreaData>("content.json") ?? new OutdoorAreaData();
					if (json.areas.ContainsKey(spouse))
					{
						ModEntry.SMonitor.Log($"Spouse patio in content pack: {contentPack.Manifest.Name} {contentPack.Manifest.Version} from {contentPack.DirectoryPath}. Not removing!", LogLevel.Warn);
						return false;
						json.areas.Remove(spouse);
						contentPack.WriteJsonFile("content.json", json);
						ModEntry.SMonitor.Log($"removed spouse {spouse} from {contentPack.DirectoryPath}");
					}
				}
			}
			ReloadPatios();
			return true;
		}

		public void AddSpousePatioHere(string spouse_tilesOf)
		{
			string spouse = spouse_tilesOf.Split('_')[0];
			string tilesOf = spouse_tilesOf.Split('_')[1];
			Point playerLocation = Utility.Vector2ToPoint(Game1.player.getTileLocation());
			OutdoorArea outdoorArea = new OutdoorArea()
			{
				useDefaultTiles = true,
				location = playerLocation,
				npcOffset = new Point(playerLocation.X + 2, playerLocation.Y + 4),
				useTilesOf = tilesOf,
			};
			string path = Path.Combine("assets", "outdoor-areas.json");

			if (!File.Exists(Path.Combine(ModEntry.SHelper.DirectoryPath, path)))
				path = "outdoor-areas.json";

			OutdoorAreaData json = ModEntry.SHelper.Data.ReadJsonFile<OutdoorAreaData>(path) ?? new OutdoorAreaData();
			json.areas[spouse] = outdoorArea;
			ModEntry.SHelper.Data.WriteJsonFile(path, json);
			ModEntry.SMonitor.Log($"Added spouse {spouse} to {path}");
			ReloadPatios();

		}
	}
}