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
        public Dictionary<string, Point> GetDefaultSpouseOffsets()
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
        public void PlaceSpouses()
        {
            ModEntry.PlaceSpouses();
        }
        public void ReloadPatios()
        {
            Game1.getFarm().loadMap(Game1.getFarm().mapPath.Value, true);
            ReloadSpouseAreaData();
            AddTileSheets();
            ShowSpouseAreas();
            PlaceSpouses();
        }
        public void AddSpousePatioHere(string spouse_tilesOf, Point cursorLoc)
        {
            string spouse = spouse_tilesOf.Split('_')[0];
            string tilesOf = spouse_tilesOf.Split('_')[1];
            OutdoorArea outdoorArea = new OutdoorArea()
            {
                useDefaultTiles = true,
                location = cursorLoc,
                npcOffset = ModEntry.spousePatioOffsets.ContainsKey(spouse) ? ModEntry.spousePatioOffsets[spouse] : new Point( 2, 4),
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
        public bool MoveSpousePatio(string spouse_dir, Point cursorLoc)
        {
            Game1.getFarm().loadMap(Game1.getFarm().mapPath.Value, true);
            string spouse = spouse_dir.Split('_')[0];
            string dir = spouse_dir.Split('_')[1];
            bool success = false;
            OutdoorArea outdoorArea = (OutdoorArea)ModEntry.outdoorAreas[spouse];
            switch (dir)
            {
                case "cursorLoc":
                    outdoorArea.location = cursorLoc;
                    success = true;
                    break;
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
            PlaceSpouses();
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


        public bool IsSpousePatioDay(NPC npc)
        {
            return ModEntry.IsSpousePatioDay(npc);
        }
    }
}